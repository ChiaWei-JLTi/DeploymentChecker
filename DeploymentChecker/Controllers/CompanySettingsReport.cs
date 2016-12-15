using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using DeploymentChecker.Controllers;
using DeploymentChecker.Models;

namespace DeploymentChecker.Controllers
{
    public static class CompanySettingsReport
    {
        public static void Run()
        {
            var companySettings = new List<CompanySettingInfo>();

            foreach (ConnectionStringSettings connStringSetting in ConfigurationManager.ConnectionStrings)
            {
                if (connStringSetting.Name == "LocalSqlServer")
                    continue;
                companySettings.AddRange(GetCompanySettingsFromDatabase(connStringSetting.Name));
            }

            var masterKeys = CompanySettingInfo.Sync(companySettings);
            Generate(companySettings, masterKeys);
        }

        private static void Generate(IEnumerable<CompanySettingInfo> companySettings, IEnumerable<string> masterKeys)
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
}
th, td {
    text-align: center;
    padding-top: 5px;
    padding-right: 15px;
    padding-bottom: 5px;
    padding-left: 15px;
}
tr:nth-child(even){background-color: #f2f2f2}
th {
    background-color: #4CAF50;
    color: white;
}
.config-card{
	float: left;
}

.config-header{
	text-align: center;
}

.config-values td:first-child{
	text-align:  left;
	font-weight: bold;
}
.config-values td:last-child{
	text-align:  center;
}
.null{
    color:red;
}
</style>
<body>");

                foreach (var companySetting in companySettings)
                    writer.WriteLine(GetTableMarkUp(companySetting, masterKeys));

                writer.WriteLine(@"</body>
</html>");

                Process.Start(reportName);
            }
        }

        private static string GetTableMarkUp(CompanySettingInfo companySetting, IEnumerable<string> masterKeys)
        {
            var html = $@"<table class='config-card'>
<tbody class='config-header'>
	<tr>
		<td colspan='2'>
			<b>Server: </b>{companySetting.Server}<br/>
			<b>Database: </b>{companySetting.Database}<br/> 
			<b>Table: </b>{companySetting.Table}
		</td>
	</tr>
</tbody>
<tr><td colspan='2'><hr></td></tr>
<tbody class='config-values'>";

            foreach (var key in masterKeys)
                html += $@"	<tr><td>{key}</td><td>{(companySetting.Settings[key] ?? "<span class='null'>(Not Found)</span>")}</td></tr>";

            html += @"</tbody>
</table>";

            return html;
        }

        private static IEnumerable<CompanySettingInfo> GetCompanySettingsFromDatabase(string connectionName)
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

        private static IEnumerable<string> GetCompanySettingTables(string connectionName)
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

        private static string GetConnectionString(string connectionName)
        {
            return ConfigurationManager.ConnectionStrings[connectionName].ConnectionString;
        }
    }
}
