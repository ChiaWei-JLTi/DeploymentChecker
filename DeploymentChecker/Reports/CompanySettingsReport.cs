using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using DeploymentChecker.Models;

namespace DeploymentChecker.Reports
{
    public class CompanySettingsReport: IReport
    {
        public void Run()
        {
            var companySettings = new List<CompanySettingInfo>();

            foreach (ConnectionStringSettings connStringSetting in ConfigurationManager.ConnectionStrings)
            {
                if (connStringSetting.Name == "LocalSqlServer")
                    continue;
                companySettings.AddRange(GetCompanySettingsFromDatabase(connStringSetting.Name));
            }

            var masterKeys = CompanySettingInfo.Sync(companySettings);
            masterKeys.Sort();
            Generate(companySettings, masterKeys);
        }

        private void Generate(IEnumerable<CompanySettingInfo> companySettings, IEnumerable<string> masterKeys)
        {
            var reportName = CommonFunctions.GetFileName(ReportType.CompanySettingsReport);
            using (var writer = File.CreateText(reportName))
            {
                writer.WriteLine(@"<!DOCTYPE html>
<html>
<head>
<title>Company Settings Report</title>
</head>
<style>
table {
    border-collapse: collapse;
    margin: 20px;
    max-width: 100%;
    white-space:nowrap;
    font-size: 90%;
}
th, td {
    text-align: center;
    padding-top: 5px;
    padding-right: 15px;
    padding-bottom: 5px;
    padding-left: 15px;
}
td:first-child{
    text-align: left;
}
tr:nth-child(even){
    background-color: #f2f2f2
}
th {
    background-color: #4CAF50;
    color: white;
    font-weight: normal;
}
.null-value{
    color:red;
}
</style>
<body>
<h1>Overview of Company Settings</h1>");

                writer.WriteLine(GetTableMarkUp(companySettings, masterKeys));
                writer.WriteLine(@"</body>
</html>");

                Process.Start(reportName);
            }
        }

        private string GetTableMarkUp(IEnumerable<CompanySettingInfo> companySettings, IEnumerable<string> masterKeys)
        {
            var html = "<table><tr>\n";

            html += "<tr>\n";
            html += "<th><b>Key</b></th>\n";
            foreach (var setting in companySettings)
                html += $"<th><b>Server: </b>{setting.Server}<br><b>Database: </b>{setting.Database}<br><b>Table: </b>{setting.Table}</th>\n";
            html += "</tr>\n";

            foreach (var key in masterKeys)
            {
                html += "<tr>\n";
                html += $"<td>{key}</td>\n";
                foreach (var setting in companySettings)
                {
                    var value = setting.Settings[key];
                    if(value == null)
                        html += $"<td class='null-value'>(Not Found)</td>\n";
                    else
                        html += $"<td>{value}</td>\n";
                }
                html += "</tr>\n";
            }

            html += "</tr></table>";
            return html;
        }

        private IEnumerable<CompanySettingInfo> GetCompanySettingsFromDatabase(string connectionName)
        {
            var companySettingInfos = new List<CompanySettingInfo>();

            try
            {
                var connString = GetConnectionString(connectionName);
                var sqlConnString = new SqlConnectionStringBuilder(connString);
                using (var conn = new SqlConnection(connString))
                {
                    conn.Open();

                    foreach (var table in GetCompanySettingTables(connectionName))
                    {
                        var sql = $"SELECT [Key], [Value] FROM {table}";
                        var cmd = new SqlCommand(sql, conn);

                        var companySettingInfo = new CompanySettingInfo()
                        {
                            Database = sqlConnString.InitialCatalog,
                            Server = sqlConnString.DataSource,
                            Table = table
                        };

                        using (var reader = cmd.ExecuteReader())
                            while (reader.Read())
                                companySettingInfo.AddOrUpdate(reader["Key"].ToString(), reader["Value"].ToString());

                        companySettingInfos.Add(companySettingInfo);
                    }
                }
            }
            catch
            {
                return new List<CompanySettingInfo>();
                ;
            }

            return companySettingInfos;
        }

        private IEnumerable<string> GetCompanySettingTables(string connectionName)
        {
            var companySettingTables = new List<string>();

            try
            {
                var connString = GetConnectionString(connectionName);
                using (var conn = new SqlConnection(connString))
                {
                    const string sql =
                        "SELECT TABLE_SCHEMA + '.' + TABLE_NAME AS Name FROM information_schema.tables WHERE TABLE_TYPE='BASE TABLE' AND TABLE_NAME = 'CompanySettings'";

                    var cmd = new SqlCommand(sql, conn);
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                        while (reader.Read())
                            companySettingTables.Add(reader["Name"].ToString());
                }
            }
            catch
            {
                return new List<string>();
                ;
            }

            return companySettingTables;
        }

        private string GetConnectionString(string connectionName)
        {
            return ConfigurationManager.ConnectionStrings[connectionName].ConnectionString;
        }
    }
}
