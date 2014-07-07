using System.Runtime.Serialization;
using System;

namespace Gnip
{
  [DataContract]
  public class GnipResponse
  {
    [DataMember(Name = "next")]
    public string Next { get; set; }

    [DataMember(Name = "results")]
    public Result[] Results { get; set; }
  }

  [DataContract]
  public class Result
  {
    [DataMember(Name = "id")]
    public string Id { get; set; }

    [DataMember(Name = "objectType")]
    public string ObjectType { get; set; }

    [DataMember(Name = "actor")]
    public Actor Actor { get; set; } //Actor

    [DataMember(Name = "verb")]
    public string Verb { get; set; }

    [DataMember(Name = "postedTime")]
    public string PostedTime { get; set; }

    [DataMember(Name = "generator")]
    public Generator Generator { get; set; }

    [DataMember(Name = "provider")]
    public Provider Provider { get; set; }

    [DataMember(Name = "link")]
    public string Link { get; set; }

    [DataMember(Name = "body")]
    public string Body { get; set; }

    [DataMember(Name = "object")]
    public GnipObject Object { get; set; }

    [DataMember(Name = "favoritesCount")]
    public int FavoritesCount { get; set; }

    [DataMember(Name = "location")]
    public Location Location { get; set; }

    [DataMember(Name = "geo")]
    public Geo Geo { get; set; }

    [DataMember(Name = "twitter_entities")]
    public twitter_entities Twitter_Entities { get; set; }

    [DataMember(Name = "twitter_filter_level")]
    public string Twitter_filter_level { get; set; }

    [DataMember(Name = "twitter_lang")]
    public string Twitter_Lang { get; set; }

    [DataMember(Name = "retweetCount")]
    public int RetweetCount { get; set; }

    [DataMember(Name = "gnip")]
    public Gnip Gnip { get; set; }
  }

  [DataContract]
  public class Gnip
  {
    [DataMember(Name = "urls")]
    public object Urls { get; set; }

    [DataMember(Name = "klout_score")]
    public int Klout_Score { get; set; }

    [DataMember(Name = "language")]
    public Language Language { get; set; }

    /*[DataMember(Name = "profileLocations")]
    public ProfileLocation ProfileLocation { get; set; }

    [DataMember(Name = "ObjectType")]
    public string ObjectType { get; set; }

    [DataMember(Name = "geoType")]
    public Geo Geo { get; set; }*/

  }

  [DataContract]
  public class ProfileLocation
  {
    [DataMember(Name = "objectType")]
    public string ObjectType { get; set; }

    [DataMember(Name = "geo")]
    public Geo Geo { get; set; }

    [DataMember(Name = "address")]
    public Address Address { get; set; }

    [DataMember(Name = "displayName")]
    public string DisplayName { get; set; }

    [DataMember(Name = "country")]
    public string Country { get; set; }

    [DataMember(Name = "countryCode")]
    public string CountryCode { get; set; }
  }

  [DataContract]
  public class Geo
  {
    [DataMember(Name = "type")]
    public string Type { get; set; }

    [DataMember(Name = "coordinates")]
    public object[] Coordinates { get; set; }
  }

  [DataContract]
  public class Address
  {
    [DataMember(Name = "country")]
    public string Country { get; set; }

    [DataMember(Name = "countryCode")]
    public string CountryCode { get; set; }
  }

  [DataContract]
  public class Language
  {
    [DataMember(Name = "value")]
    public string Value { get; set; }
  }

  [DataContract]
  public class Urls
  {
    [DataMember(Name = "url")]
    public string Url { get; set; }

    [DataMember(Name = "expanded_url")]
    public string Expanded_Url { get; set; }

    [DataMember(Name = "expanded_status")]
    public string Expanded_Status { get; set; }
  }

  [DataContract]
  public class twitter_entities
  {
    [DataMember(Name = "hashtags")]
    public object[] Hashtags { get; set; }

    [DataMember(Name = "symbols")]
    public object[] Symbols { get; set; }

    [DataMember(Name = "urls")]
    public object[] Urls { get; set; }

    [DataMember(Name = "user_mentions")]
    public object[] User_Mentions { get; set; }

    [DataMember(Name = "media")]
    public Media[] Media { get; set; }

    [DataMember(Name = "twitter_filter_level")]
    public string Twitter_Filter_Level { get; set; }

    [DataMember(Name = "twitter_lang")]
    public string Twitter_Language { get; set; }

    [DataMember(Name = "retweetCount")]
    public int RetweetCount { get; set; }
  }

  [DataContract]
  public class Media
  {
    [DataMember(Name = "id")]
    public long ID { get; set; }

    [DataMember(Name = "id_str")]
    public string ID_String { get; set; }

    [DataMember(Name = "indices")]
    public object Indices { get; set; }

    [DataMember(Name = "media_url")]
    public string Media_Url { get; set; }

    [DataMember(Name = "media_url_https")]
    public string Media_Url_Https { get; set; }

    [DataMember(Name = "url")]
    public string Url { get; set; }

    [DataMember(Name = "display_url")]
    public string Display_Url { get; set; }

    [DataMember(Name = "expanded_url")]
    public string Expeded_Url { get; set; }

    [DataMember(Name = "type")]
    public string Type { get; set; }

    [DataMember(Name = "sizes")]
    public Sizes Sizes { get; set; }
  }

  [DataContract]
  public class Sizes
  {
    [DataMember(Name = "large")]
    public Size Large { get; set; }

    [DataMember(Name = "medium")]
    public Size Medium { get; set; }

    [DataMember(Name = "thumb")]
    public Size Thumb { get; set; }

    [DataMember(Name = "small")]
    public Size Small { get; set; }
  }

  [DataContract]
  public class Size
  {
    [DataMember(Name = "w")]
    public int Width { get; set; }

    [DataMember(Name = "h")]
    public int Height { get; set; }

    [DataMember(Name = "resize")]
    public string Resize { get; set; }
  }

  [DataContract]
  public class GnipObject
  {
    [DataMember(Name = "objectType")]
    public string ObjectType { get; set; }

    [DataMember(Name = "id")]
    public string Id { get; set; }

    [DataMember(Name = "summary")]
    public string Summary { get; set; }

    [DataMember(Name = "link")]
    public string Link { get; set; }

    [DataMember(Name = "postedTime")]
    public string PostedTime { get; set; }
  }

  [DataContract]
  public class Provider
  {
    [DataMember(Name = "objectType")]
    public string ObjectType { get; set; }

    [DataMember(Name = "displayName")]
    public string DisplayName { get; set; }

    [DataMember(Name = "link")]
    public string Link { get; set; }
  }

  [DataContract]
  public class Generator
  {
    [DataMember(Name = "displayName")]
    public string DisplayName { get; set; }

    [DataMember(Name = "link")]
    public string Link { get; set; }

    /*[DataMember(Name = "objectType")]
    public string ObjectType { get; set; }*/
  }

  [DataContract]
  public class Actor
  {
    [DataMember(Name = "objectType")]
    public string ObjectType { get; set; }

    [DataMember(Name = "id")]
    public string ID { get; set; }

    [DataMember(Name = "link")]
    public string Link { get; set; }

    [DataMember(Name = "displayName")]
    public string DisplayName { get; set; }

    [DataMember(Name = "postedTime")]
    public string PostedTime { get; set; }

    [DataMember(Name = "image")]
    public string Image { get; set; }

    [DataMember(Name = "summary")]
    public string Summary { get; set; }

    [DataMember(Name = "links")]
    public Link[] Links { get; set; }

    [DataMember(Name = "friendsCount")]
    public int FriendsCount { get; set; }

    [DataMember(Name = "followersCount")]
    public int FollowersCount { get; set; }

    [DataMember(Name = "listedCount")]
    public int ListedCount { get; set; }

    [DataMember(Name = "statusesCount")]
    public int StatusesCount { get; set; }

    [DataMember(Name = "twitterTimeZone")]
    public string TwitterTimeZone { get; set; }

    [DataMember(Name = "verified")]
    public object Verified { get; set; }

    [DataMember(Name = "utcOffset")]
    public string UtcOffset { get; set; }

    [DataMember(Name = "preferredUsername")]
    public string PreferredUsername { get; set; }

    [DataMember(Name = "languages")]
    public string[] Languages { get; set; }

    [DataMember(Name = "location")]
    public Location Location { get; set; }

    [DataMember(Name = "favoritesCount")]
    public int FavoritesCount { get; set; }
  }

  [DataContract]
  public class Location
  {
    [DataMember(Name = "objectType")]
    public string ObjectType { get; set; }

    [DataMember(Name = "displayName")]
    public string DisplayName { get; set; }

    [DataMember(Name = "name")]
    public string Name { get; set; }

    [DataMember(Name = "country_code")]
    public string Country_Code { get; set; }

    [DataMember(Name = "twitter_country_code")]
    public string Twitter_Country_Code { get; set; }

    [DataMember(Name = "link")]
    public string Link { get; set; }

    [DataMember(Name = "geo")]
    public Geo Geo { get; set; }
  }

  [DataContract]
  public class Link
  {
    [DataMember(Name = "href")]
    public object Href { get; set; }

    [DataMember(Name = "rel")]
    public string Rel { get; set; }

  }
}
