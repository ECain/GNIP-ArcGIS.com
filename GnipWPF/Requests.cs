using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;

namespace Gnip
{
  class Requests
  {
    List<GnipResponse> _gnipResponses;

    public Requests()
    {
      _gnipResponses = new List<GnipResponse>();
    }

    private HttpWebRequest makeRequest(string urlString, string username, string password)
    {      
      HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlString);
      //Search API should use this method of Basic Authentication.
      string authInfo = string.Format("{0}:{1}", username, password);
      authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
      request.Headers.Add("Authorization", "Basic " + authInfo);
      return request;
    }

    public GnipResponse SearchGetRequest(string urlString, string username, string password, string query, int maxRecords, string boundingBox, string next)
    {
      string queryString = string.Empty;

      //if (maxRecords > -1 && next == null)
      queryString = urlString + "?query=" + query + "%20bounding_box%3A%5B" + boundingBox + "%5D&publisher=twitter";

      if (next != "")
       queryString += "&next=" + next;

      HttpWebRequest request = makeRequest(queryString, username, password);
      request.Method = "GET";

      HttpWebResponse response;

      try
      {
        response = (HttpWebResponse)request.GetResponse();
      }
      catch (System.Net.WebException ex)
      {
        Console.WriteLine("\r\n GNIP call error: " + ex.Message);
        return null;
      }

      Console.WriteLine(((HttpWebResponse)response).StatusDescription);
      StreamReader reader = new StreamReader(response.GetResponseStream());

      string responseFromServer = reader.ReadToEnd();
      reader.Close();
      response.Close();
      JavaScriptSerializer javaSciptSerializer = new JavaScriptSerializer();
      GnipResponse gnipResponse = javaSciptSerializer.Deserialize<GnipResponse>(responseFromServer);

      return gnipResponse;
    }

    private HttpWebRequest PostRequest(string urlString, string username, string password)
    {
      HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlString);
      request.ServicePoint.Expect100Continue = false;

      //Search API should use this method of Basic Authentication.
      string authInfo = string.Format("{0}:{1}", username, password);
      authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
      request.Headers.Add("Authorization", "Basic " + authInfo);
      request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;

      return request;
    }

    public string SearchPostRequest(string urlString, string username, string password, string query, int maxRecords, string fromDate, string toDate, string publisher)
    {
      HttpWebRequest request = PostRequest(urlString, username, password);

      string postData = "";

      request.Method = "POST";

      postData = "{\"query\":\"" + query + "\",\"publisher\":\"" + publisher + "\"}";

      byte[] byteArray = Encoding.UTF8.GetBytes(postData);
      request.ContentType = "application/x-www-form-urlencoded";
      request.ContentLength = byteArray.Length;
      Stream dataStream = request.GetRequestStream();
      dataStream.Write(byteArray, 0, byteArray.Length);
      dataStream.Close();

      WebResponse response = request.GetResponse();
      Console.WriteLine(((HttpWebResponse)response).StatusDescription);
      dataStream = response.GetResponseStream();
      StreamReader reader = new StreamReader(dataStream);
      string responseFromServer = reader.ReadToEnd();
      Console.WriteLine(responseFromServer);
      Console.WriteLine();
      reader.Close();
      dataStream.Close();
      response.Close();

      return responseFromServer;
    }


    //NB: Currently not used in Sample
    public void CreateHistoricalJob(string urlString, string username, string password)
    {
			HttpWebRequest request = makeRequest(urlString, username, password);

      string postData = "";
      string publisher = "twitter";
      string streamType = "track";
      string dataFormat = "activity-streams";
      string fromDate = "";//"201301010000"; // This time is inclusive -- meaning the minute specified will be included in the data returned
      string toDate = "";// "201301010001"; // This time is exclusive -- meaning the data returned will not contain the minute specified, but will contain the minute immediately preceding it
      string jobTitle = "my historical job";
      string serviceUsername = "mylessutherland"; // This is the Twitter username your company white listed with Gnip for access.
      string rules = "[{\"value\":\"rule 1\",\"tag\":\"ruleTag\"},{\"value\":\"rule 2\",\"tag\":\"ruleTag\"}]";

      request.Method = "POST";
      postData = "{\"publisher\":\"" + publisher + "\",\"streamType\":\"" + streamType + "\",\"dataFormat\":\"" + dataFormat + "\",\"fromDate\":\"" + fromDate + "\",\"toDate\":\"" + toDate + "\",\"title\":\"" + jobTitle + "\",\"serviceUsername\":\"" + serviceUsername + "\",\"rules\":" + rules + "}";


			request.Method = "POST";
			byte[] byteArray = Encoding.UTF8.GetBytes (postData);
      request.ContentType = "application/x-www-form-urlencoded";
	    request.ContentLength = byteArray.Length;
      Stream dataStream = request.GetRequestStream ();			
      dataStream.Write (byteArray, 0, byteArray.Length);
      dataStream.Close ();

      WebResponse response = request.GetResponse ();
      Console.WriteLine (((HttpWebResponse)response).StatusDescription);
      dataStream = response.GetResponseStream ();
      StreamReader reader = new StreamReader (dataStream);
      string responseFromServer = reader.ReadToEnd ();
      Console.WriteLine (responseFromServer);
		  Console.WriteLine();
      reader.Close ();
      dataStream.Close ();
      response.Close ();
		}		
  }
}
