using System.Collections.Generic;

namespace DeploymentChecker.Models
{
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

        private void Sync(IEnumerable<string> keys)
        {
            foreach (var key in keys)
            {
               if(!Settings.ContainsKey(key))
                    Settings.Add(key, null);
            }
        }

        public static List<string> Sync(List<CompanySettingInfo> collection)
        {
            var masterKeys = new List<string>();

            foreach (var companySettingInfo in collection)
            {
                foreach (var key in companySettingInfo.Settings.Keys)
                {
                    if(!masterKeys.Contains(key))
                        masterKeys.Add(key);
                }
            }

            foreach (var companySettingInfo in collection)
            {
                companySettingInfo.Sync(masterKeys);
            }

            return masterKeys;
        }
    }
}



