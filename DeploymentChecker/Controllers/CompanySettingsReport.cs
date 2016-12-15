using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

public class CompanySettingsReport
{
    public void Run()
    {
         
    }

    private List<CompanySettingInfo> GetCompanySettingInfos(string connectionName)
    {
        var companySettingInfos = new List<CompanySettingInfo>();

        try
        {
            var connString = ConfigurationManager.ConnectionStrings[connectionName].ConnectionString;
            using (var conn = new SqlConnection(connString))
            {
                conn.Open();

                foreach (var table in GetCompanySettingTables(connectionName))
                {
                    var sql = $"SELECT [Key], [Value] FROM {table}";
                    var cmd = new SqlCommand(sql, conn);
                    
                    var reader = cmd.ExecuteReader();
                    var companySettingInfo = new CompanySettingInfo()
                        {
                            
                        };
                    while (reader.Read())
                        companySettingInfo.AddOrUpdate(reader["Key"].ToString(), reader["Value"].ToString());

                }




                //const string sql = "SELECT TABLE_SCHEMA + '.' + TABLE_NAME AS Name FROM information_schema.tables WHERE TABLE_TYPE='BASE TABLE' AND TABLE_NAME = 'CompanySettings'";

                

                
            }
        }
        catch
        {
            return new List<CompanySettingInfo>();;
        }

        return companySettingInfos;
    }

    private List<string> GetCompanySettingTables(string connectionName)
    {
        var companySettingTables = new List<string>();

        try
        {
            var connString = ConfigurationManager.ConnectionStrings[connectionName].ConnectionString;
            using (var conn = new SqlConnection(connString))
            {
                const string sql = "SELECT TABLE_SCHEMA + '.' + TABLE_NAME AS Name FROM information_schema.tables WHERE TABLE_TYPE='BASE TABLE' AND TABLE_NAME = 'CompanySettings'";

                var cmd = new SqlCommand(sql, conn);
                conn.Open();

                var reader = cmd.ExecuteReader();
                while (reader.Read())
                    companySettingTables.Add(reader["Name"].ToString());
            }
        }
        catch
        {
            return new List<string>();;
        }

        return companySettingTables;
    }
    
}
