using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

public class BuildInfo
{
    public string Tenant { get; set; }
    public string BuildVersion { get; set; }
    public string Server { get; set; }
    public string Database { get; set; }
//    public string UserId { get; set; }
//    public string Password { get; set; }
    public string IdentityServer { get; set; }
    public string CreatedTime { get; set; }
    public bool HasError { get; set; } = false;
    public bool IsLatestBuild { get; set; } = false;

    public void ReadDbSetting(string configFile)
    {
        try 
        {
            var doc = XDocument.Load(configFile);
            var xmlAttr = doc.Descendants("connectionStrings")
                             .Descendants("add")
                             .Attributes().First(x => x.Name=="connectionString");
            var sqlConnString = new SqlConnectionStringBuilder(xmlAttr.Value);

            Server = sqlConnString.DataSource;
            Database = sqlConnString.InitialCatalog;
//            UserId = sqlConnString.UserID;
//            Password = sqlConnString.Password;
        }
        catch
        {
            HasError = false;
        }
    }

    public void ReadIdServerSetting(string configFile)
    {
        try 
        {
            var doc = XDocument.Load(configFile);
            var xmlAttr = doc.Descendants("appSettings")
                            .Descendants("add")
                            .First(x => x.Attributes().Any(y => y.Name == "key" && y.Value == "authority"))
                            .Attributes()
                            .First(x => x.Name == "value");

            IdentityServer = xmlAttr.Value;
        }
        catch
        {
            HasError = false;
        }
    }

    public static List<string> GetTitles()
    {
        return GetProperties().Select(x => x.Name).ToList();
    }

    public static List<string> GetContents(BuildInfo buildInfo)
    {
        return GetProperties().Select(x => (string) x.GetValue(buildInfo) ?? "").ToList();
    }

    private static IEnumerable<PropertyInfo> GetProperties()
    {
        return typeof(BuildInfo).GetProperties().Where(x => x.PropertyType == typeof(string));
    }
}


