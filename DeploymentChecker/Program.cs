using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Runtime.InteropServices.ComTypes;

public class Program
{
    static void Main(string[] args)
    {
        //        Report.Run();

        //        var conn = ConfigurationManager.ConnectionStrings;

        //        foreach (ConnectionStringSettings conn in ConfigurationManager.ConnectionStrings)
        //        {
        //            var sqlConnString = new SqlConnectionStringBuilder(conn.ConnectionString);
        //            Console.WriteLine(sqlConnString.ApplicationName);
        //            Console.WriteLine(sqlConnString.DataSource);
        //            Console.WriteLine(sqlConnString.InitialCatalog);
        //            Console.WriteLine(sqlConnString.UserID);
        //            Console.WriteLine(sqlConnString.Password);
        //        }



        var connString = ConfigurationManager.ConnectionStrings["Test"].ConnectionString;
        var companySettingTables = new List<string>();
        using (var conn = new SqlConnection(connString))
        {
            const string sqlGetTables = "SELECT TABLE_SCHEMA + '.' + TABLE_NAME AS Name FROM information_schema.tables WHERE TABLE_TYPE='BASE TABLE' AND TABLE_NAME = 'CompanySettings'";
            conn.Open();
            
            
            var cmd = new SqlCommand(sqlGetTables, conn);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                companySettingTables.Add(reader["Name"].ToString());
            }
        }

//        var c = new CompanySettingInfo();
//        c.AddOrUpdate("ShowBalance", "true");
//        c.AddOrUpdate("ShowBenefits", "false");
//        c.AddOrUpdate("ShowBenefits", "true");
//        var keys = new List<string>() {"NationalService", "ShowPrice", "ShowBalance", "HongKong"};
//        c.Sync(keys);



    }



}

