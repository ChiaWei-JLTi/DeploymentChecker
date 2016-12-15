using System.Collections.Generic;

public class CompanySettingInfo
{
    public string Server { get; set; }
    public string Database { get; set; }
    public string Table { get; set; }
    public Dictionary<string, string> Settings { get; } = new Dictionary<string, string>();

    public void AddOrUpdate(string key, string value)
    {
        if (Settings.ContainsKey(key))
            Settings[key] = value;
        else
            Settings.Add(key, value);
    }

    public void Sync(IEnumerable<string> keys)
    {
        foreach (var key in keys)
        {
           if(!Settings.ContainsKey(key))
                Settings.Add(key, null);
        }
    }

    public void Sync(Dictionary<string, string> settings)
    {
        Sync(settings.Keys);
    }
}


