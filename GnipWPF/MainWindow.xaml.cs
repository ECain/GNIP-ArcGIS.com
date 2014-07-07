using AGOLRestHandler;
using ESRI.ArcGIS.Client.Geometry;
using Gnip;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISWPFSDK
{
  public partial class GnipSample : UserControl
  {
    string _baseApplyEditsURL;
    string _arcgisToken;
    string _arcgisOrganizationID;
    string _myOrgServicesEndPoint;
    string _folderContentEndPoint;
    string serviceURL;

    Dictionary<string, Item> _myOrganizationalContent;
    FeatureEditsResponse _featureEditResponse;
    DataTable _dataTable;
    private static ESRI.ArcGIS.Client.Projection.WebMercator _mercator =
                new ESRI.ArcGIS.Client.Projection.WebMercator();

    Dictionary<string, FeatureLayerAttributes> _featureServiceAttributesDataDictionary;
    Dictionary<string, string> _featureServiceRequestAndResponse; //key value pair will have a $ delimiter for the request string then the response

    FeatureServiceInfo _featureServiceInfo;
    FeatureLayerAttributes _featLayerAttributes;

    //GNIP
    Requests _requests;

    public List<Folder> folders { get; set; }
    public List<Item> items { get; set; }
    public Folder selectedFolder { get; set; }
    public string GnipUserName { get; set; }
    public string GnipPassword { get; set; }
    public string AGOLUserName { get; set; }
    public string AGOLPassword { get; set; }
    public string GnipID { get; set; }
    public string BoundingBox { get; set; }
    public string Query { get; set; }
    public int MaxRecords { get; set; }
    public string Next { get; set; }

    public GnipSample()
    {
      InitializeComponent();
    }

    #region GNIP

    private void Gnip_Click(object sender, RoutedEventArgs e)
    {
      ESRI.ArcGIS.Client.Geometry.Envelope extent = _mercator.ToGeographic(MyMap.Extent) as ESRI.ArcGIS.Client.Geometry.Envelope;
      BoundingBox = string.Format("{0}%20{1}%20{2}%20{3}", extent.XMin.ToString("###.######"), extent.YMin.ToString("###.######"), extent.XMax.ToString("###.######"), extent.YMax.ToString("###.######"));

      GnipUserName = txtGnipUserName.Text;
      GnipPassword = txtGnipPassword.Password;
      GnipID = txtAccount.Text;
      Query = txtQuery.Text.ToLower();
      Next = "";
      //TODO: MaxRecords. Give the user some UI to enter this

      QueryGnip(); 
    }
    private void QueryGnip()
    {
      GnipResponse response = BuildQuery();

      if (response == null)
        return;

      EditFeatureService(response);

      if (response.Next != "" && response.Next != null)
      {
        Next = response.Next;
        QueryGnip();
      }
    }

    private GnipResponse BuildQuery()
    {
      ////SEARCH
      string uri = string.Format("https://search.gnip.com/accounts/{0}/search/{1}.json", GnipID, "prod");

      if (_requests == null)
        _requests = new Requests();

      return _requests.SearchGetRequest(uri, GnipUserName, GnipPassword, Query, MaxRecords, BoundingBox, Next);
    }

    #endregion

    #region ESRI
    private void Authenticate_Click(object sender, RoutedEventArgs e)
    {
      //this.Cursor = Cursors.WaitCursor;
      string formattedRequest = string.Empty;
      string jsonResponse = string.Empty;

      Authentication authenticationDataContract = RequestAndResponseHandler.AuthorizeAgainstArcGISOnline(txtUserN.Text, txtPassw.Password, txtOrgAccountUrl.Text, out formattedRequest, out jsonResponse);

      //On success store the token for further use
      if (authenticationDataContract != null)
      {
        if (authenticationDataContract.token != null)
        {
          _arcgisToken = authenticationDataContract.token;

          Self();
          AdminRoot();
          OrganizationContentPublicPrivate();
        }
      }
    }

    private void AdminRoot()
    {
      string formattedRequest = string.Empty;
      string jsonResponse = string.Empty;

      int index = _baseApplyEditsURL.IndexOf("rest");
      string url = _baseApplyEditsURL.Substring(0, index);
      url += "admin?f=pjson";
      Administration administration = null;

      try
      {
        administration = RequestAndResponseHandler.GetDataContractInfo(url, DataContractsEnum.Administration, out jsonResponse) as Administration;
      }
      catch { }

      //On failure change from the trial server (Services1) to the release server endpoint.
      if (administration.currentVersion == null)
      {
        formattedRequest = url = url.Replace("services1", "services");

        administration = RequestAndResponseHandler.GetDataContractInfo(url, DataContractsEnum.Administration, out jsonResponse) as Administration;

        if (administration != null)
          _baseApplyEditsURL.Replace("services1", "services");
      }
    }

    private void Self()
    {
      //self
      string formattedRequest;
      string responseJSON;

      Self response = RequestAndResponseHandler.SelfWebRequest("http://www.arcgis.com/sharing/rest/community/self", _arcgisToken, out formattedRequest, out responseJSON);

      _arcgisOrganizationID = response.orgId;
      txtFullName.Text = "Full Name: " + response.fullName;
      txtEmail.Text = "Email: " + response.email;
      txtQuota.Text = "Storage Quota: " + response.storageQuota.ToString();
      txtStorage.Text = "Storage Usage: " + response.storageUsage.ToString();
      txtOrgID.Text = "Organization ID:" + response.orgId;
      txtRole.Text = "Role: " + response.role;

      //Organizational Publicly exposed layers
      _baseApplyEditsURL = string.Format("http://services1.arcgis.com/{0}/ArcGIS/rest/services/", response.orgId);

      //user only content.
      txtOrgPublicURL.Text = _folderContentEndPoint = _myOrgServicesEndPoint = string.Format("{0}sharing/content/users/{1}", txtOrgAccountUrl.Text, txtUserN.Text);

      cboFolders.ItemsSource = null;
      cboFeatureServices.Items.Clear();
    }

    private void OrganizationContentPublicPrivate()
    {
      string formattedRequest = string.Empty;
      string jsonResponse = string.Empty;

      //check to see if we have an instantiated dictionary to store the attributes
      if (_myOrganizationalContent == null)
        _myOrganizationalContent = new Dictionary<string, Item>();
      else
        _myOrganizationalContent.Clear();

      UserOrganizationContent myContent = RequestAndResponseHandler.UserOrgContent(_folderContentEndPoint, _arcgisToken, out formattedRequest, out jsonResponse);

      if (myContent.currentFolder == null && myContent.items == null && myContent.username == null)
      {
        //cboFolders.SelectionChanged -= cboFolders_SelectionChanged;
        cboFeatureServices.ItemsSource = null;
        return;
      }

      cboFeatureServices.ItemsSource = null;

      string key = "";
      List<Image> imagefiles = new List<Image>();

      if (items != null)
        items.Clear();

      int counter = -1;
      int gnipLocation = 0;
      //display the thumb nail for Webmaps and City Engine Web Scenes
      foreach (Item item in myContent.items)
      {
        if (item.type == "Feature Service")
        {
          counter++;

          key = string.Format("Title: {0} ID: {1}", item.title, item.id);
          if (items == null)
            items = new List<Item>();

          items.Add(item);
          _myOrganizationalContent.Add(key, item);

          if (item.title == "Gnip")
            gnipLocation = counter;
        }
      }

      cboFeatureServices.ItemsSource = items;

      //load the images into the DataGrid:
      if (cboFeatureServices.Items.Count > 0)
        cboFeatureServices.SelectedIndex = gnipLocation;

      if (myContent.folders == null)
        return;

      //ensure the folders collection is not a null object
      if (folders == null)
        folders = new List<Folder>();
      else
        folders.Clear();

      //ensure we have the user's default named folder inside of the combobox
      Folder folder = new Folder();
      folder.title = txtUserN.Text;
      folder.id = string.Empty;

      folders.Add(folder);

      //ensure the order is correct.
      foreach (Folder foldr in myContent.folders)
      {
        folders.Add(foldr);
      }

      cboFolders.ItemsSource = folders;

      cboFolders.SelectionChanged += cboFolders_SelectionChanged;

      cboFeatureServices.SelectionChanged += cboFeatureServices_SelectionChanged;

      if (_featureServiceAttributesDataDictionary == null)
      {
        _featureServiceAttributesDataDictionary = new Dictionary<string, FeatureLayerAttributes>();
        _featureServiceRequestAndResponse = new Dictionary<string, string>();
      }

      //TODO: GET THIS IS THE RIGHT ORDER TO GET THE CALLS GOING SO YOU CAN GET THE FIELD NAMES FOR POPULATING
      string serviceRequest;
      string serviceResponse;

      //Feature Layer Attributes
      serviceURL = string.Format("http://services.arcgis.com/q7zPNeKmTWeh7Aor/ArcGIS/rest/services/{0}/FeatureServer/0/", "GNIP");
      _featLayerAttributes = RequestAndResponseHandler.GetFeatureServiceAttributes(serviceURL, _arcgisToken, out serviceRequest, out serviceResponse);

      if (_featLayerAttributes == null)
        return;

      //populate the Field Names, results in _datatable
      if (_featLayerAttributes.fields != null)
        PopulateFieldsList(_featLayerAttributes.fields);

      string url = string.Format("http://services1.arcgis.com/{0}/arcgis/rest/services/{1}/FeatureServer?f=pjson", _arcgisOrganizationID, "GNIP");

      _featureServiceInfo = RequestAndResponseHandler.GetDataContractInfo(url, DataContractsEnum.FeatureServiceInfo, out jsonResponse) as FeatureServiceInfo;

      //lets store the name of the featureLayer into the listbox
      ListViewItem listviewItem = new ListViewItem();
    }

    void cboFeatureServices_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      _dataTable = new DataTable();
      Item item = cboFeatureServices.SelectedValue as Item;
      string id = item.id;
    }

    void cboFolders_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (cboFolders.Text == string.Empty)
        return;

      if (((Folder)cboFolders.SelectedValue).id == string.Empty)
        txtOrgPublicURL.Text = _folderContentEndPoint = _myOrgServicesEndPoint;
      else
        txtOrgPublicURL.Text = _folderContentEndPoint = string.Format("{0}/{1}", _myOrgServicesEndPoint, ((Folder)cboFolders.SelectedValue).id);

      OrganizationContentPublicPrivate();
    }

    /// <summary>
    /// Populate the Feature Service Field list with field names
    /// </summary>
    /// <param name="fields"></param>
    private void PopulateFieldsList(object[] fields)
    {
      Dictionary<string, object> dict;

      if (_dataTable == null)
        _dataTable = new DataTable();

      _dataTable.Columns.Clear();
      txtFields.Text = "";

      Type type;
      foreach (var item in fields)
      {
        dict = item as Dictionary<string, object>;
        string name = dict["name"].ToString();

        txtFields.Text += name + "\r\n";

        type = FieldType(dict["type"].ToString());
        if (type == null || name == "FID")
          continue;

        _dataTable.Columns.Add(name, type);
      }
    }

    private Type FieldType(string esriFieldType)
    {
      Type type = null;
      switch (esriFieldType)
      {
        case "esriFieldTypeDate":
          type = typeof(DateTime);
          break;
        case "esriFieldTypeInteger":
          type = typeof(int);
          break;
        case "esriFieldTypeSingle":
          type = typeof(Single);
          break;
        case "esriFieldTypeDouble":
          type = typeof(double);
          break;
        case "esriFieldTypeString":
          type = typeof(string);
          break;
        case "esriFieldTypeOID": //not required for project
          break;
        case "esriFieldTypeGeometry":
          break;
        case "esriFieldTypeBlob":
          break;
        case "esriFieldTypeRaster":
          break;
        case "esriFieldTypeGUID":
          break;
        case "esriFieldTypeGlobalID":
          break;
        default:
          break;
      }

      return type;
    }

    private void EditFeatureService(GnipResponse response)
    {
      if (response.Results.Length == 0)
        return;

      string formattedRequest = string.Empty;
      
      string jsonToSend = "adds=[";
      string attributes = string.Empty;

      double x = 0.0;
      double y = 0.0;

      foreach (Gnip.Result result in response.Results)
      {
        attributes = "\"attributes\":{\"";
        attributes += string.Format("{0}\":\"{1}\",\"", _dataTable.Columns[0].ColumnName, result.Actor.ObjectType);
        attributes += string.Format("{0}\":\"{1}\",\"", _dataTable.Columns[1].ColumnName, result.Actor.ObjectType);
        attributes += string.Format("{0}\":\"{1}\",\"", _dataTable.Columns[2].ColumnName, result.Actor.ID);
        attributes += string.Format("{0}\":\"{1}\",\"", _dataTable.Columns[3].ColumnName, result.Actor.Link);
        attributes += string.Format("{0}\":\"{1}\",\"", _dataTable.Columns[4].ColumnName, result.Actor.DisplayName);
        attributes += string.Format("{0}\":\"{1}\",\"", _dataTable.Columns[5].ColumnName, "");//TODO: Get it into a format that they will push to the feature seriver. result.Actor.PostedTime);
        attributes += string.Format("{0}\":\"{1}\",\"", _dataTable.Columns[6].ColumnName, result.Actor.Image);
        attributes += string.Format("{0}\":\"{1}\",\"", _dataTable.Columns[7].ColumnName, StripInvalidChars(result.Actor.Summary));
        attributes += string.Format("{0}\":\"{1}\",\"", _dataTable.Columns[8].ColumnName, "");//TODO: Get it into a format that they will push to the feature seriver.result.Actor.TwitterTimeZone);
        attributes += string.Format("{0}\":\"{1}\",\"", _dataTable.Columns[9].ColumnName, result.Actor.UtcOffset);
        attributes += string.Format("{0}\":\"{1}\",\"", _dataTable.Columns[10].ColumnName, result.Actor.PreferredUsername);
        attributes += string.Format("{0}\":\"{1}\",\"", _dataTable.Columns[11].ColumnName, result.Actor.Languages);
        attributes += string.Format("{0}\":\"{1}\",\"", _dataTable.Columns[12].ColumnName, result.Location.ObjectType);
        attributes += string.Format("{0}\":\"{1}\",\"", _dataTable.Columns[13].ColumnName, result.Location.DisplayName);
        attributes += string.Format("{0}\":\"{1}\",\"", _dataTable.Columns[14].ColumnName, result.Verb);
        attributes += string.Format("{0}\":\"{1}\",\"", _dataTable.Columns[15].ColumnName, "");//TODO: Get it into a format that they will push to the feature seriver.result.PostedTime);
        attributes += string.Format("{0}\":\"{1}\",\"", _dataTable.Columns[16].ColumnName, result.Generator.DisplayName);
        attributes += string.Format("{0}\":\"{1}\",\"", _dataTable.Columns[17].ColumnName, ""); //generator type TODO
        attributes += string.Format("{0}\":\"{1}\",\"", _dataTable.Columns[18].ColumnName, result.Provider.DisplayName);
        attributes += string.Format("{0}\":\"{1}\",\"", _dataTable.Columns[19].ColumnName, result.Provider.Link);
        attributes += string.Format("{0}\":\"{1}\",\"", _dataTable.Columns[20].ColumnName, result.Link);
        attributes += string.Format("{0}\":\"{1}\",\"", _dataTable.Columns[21].ColumnName, StripInvalidChars(result.Body));
        attributes += string.Format("{0}\":\"{1}\",\"", _dataTable.Columns[22].ColumnName, result.ObjectType);
        attributes += string.Format("{0}\":\"{1}\",\"", _dataTable.Columns[23].ColumnName, result.Object.Id);
        attributes += string.Format("{0}\":\"{1}\",\"", _dataTable.Columns[24].ColumnName, StripInvalidChars(result.Object.Summary));
        attributes += string.Format("{0}\":\"{1}\",\"", _dataTable.Columns[25].ColumnName, result.Object.Link);
        attributes += string.Format("{0}\":\"{1}\",\"", _dataTable.Columns[26].ColumnName, "");//TODO: Get it into a format that they will push to the feature seriver.result.Object.PostedTime);
        attributes += string.Format("{0}\":\"{1}\",\"", _dataTable.Columns[27].ColumnName, result.Twitter_Entities.Hashtags);
        attributes += string.Format("{0}\":\"{1}\",\"", _dataTable.Columns[28].ColumnName, result.Twitter_Entities.Symbols);
        attributes += string.Format("{0}\":\"{1}\",\"", _dataTable.Columns[29].ColumnName, result.Twitter_Entities.Urls);
        attributes += string.Format("{0}\":\"{1}\",\"", _dataTable.Columns[30].ColumnName, result.Twitter_Entities.User_Mentions);
        attributes += string.Format("{0}\":\"{1}\",\"", _dataTable.Columns[31].ColumnName, result.Twitter_Entities.Twitter_Filter_Level);
        attributes += string.Format("{0}\":\"{1}\",\"", _dataTable.Columns[32].ColumnName, result.Twitter_Entities.Twitter_Language);
        attributes += string.Format("{0}\":\"{1}\",\"", _dataTable.Columns[33].ColumnName, result.Gnip.Urls);
        attributes += string.Format("{0}\":\"{1}\",\"", _dataTable.Columns[34].ColumnName, ""); //todo expanded urls
        attributes += string.Format("{0}\":\"{1}\",\"", _dataTable.Columns[35].ColumnName, ""); //todo statuses
        attributes += string.Format("{0}\":\"{1}\",\"", _dataTable.Columns[36].ColumnName, result.Gnip.Language);
        attributes += string.Format("{0}\":\"{1}\",\"", _dataTable.Columns[37].ColumnName, ""); //profile type
        attributes += string.Format("{0}\":\"{1}\",\"", _dataTable.Columns[38].ColumnName, ""); //geotype
        attributes += string.Format("{0}\":\"{1}\",\"", _dataTable.Columns[39].ColumnName, "");//coordinates
        attributes += string.Format("{0}\":\"{1}\",\"", _dataTable.Columns[40].ColumnName, "");
        attributes += string.Format("{0}\":\"{1}\",\"", _dataTable.Columns[41].ColumnName, "");
        attributes += string.Format("{0}\":\"{1}\"", _dataTable.Columns[42].ColumnName, "");

        double.TryParse(result.Geo.Coordinates[0].ToString(), out y);
        double.TryParse(result.Geo.Coordinates[1].ToString(), out x);

        MapPoint p = new MapPoint(x, y);
        p = _mercator.FromGeographic(p) as ESRI.ArcGIS.Client.Geometry.MapPoint;

        jsonToSend += "{\"geometry\":{\"x\":" + p.X + ",\"y\":" + p.Y + ",\"spatialReference\":{\"wkid\":102100, \"latestWkid\":3857}}," + attributes + "}},";
      }

        SendRequest(jsonToSend);

      //TODO: implement a basic query click event for the features displayed to the user to enable visualization of the data behind each point.
    }

    private void SendRequest(string jsonToSend)
    {
      string jsonResponse = string.Empty;

      try
      {
        jsonToSend = jsonToSend.Remove(jsonToSend.Length - 1, 1);
        jsonToSend += "]";

        string url = ((Item)cboFeatureServices.SelectedValue).url + "/0/applyEdits?f=pjson&token=" + _arcgisToken;

        //Make the request and display response success
        _featureEditResponse = RequestAndResponseHandler.FeatureEditRequest(url, jsonToSend, out jsonResponse);
      }
      catch { }
    }

    private string StripInvalidChars(string test)
    {
      if (string.IsNullOrEmpty(test))
        return "";

      string test2 = test.Replace("\r\n", "");
      test2 = test2.Replace("\"", "");

      test2 = test2.Replace("\"", "");
      test2 = test2.Replace("nl", "");
      test2 = test2.Replace("&amp;", "");
      test2 = test2.Replace("...", "");
      test2 = test2.Replace("&", "");
      return test2;
    }
    #endregion
  }

  public class MyClass
  {
    public int Age { get; set; }
    public string Name { get; set; }
  }
}


