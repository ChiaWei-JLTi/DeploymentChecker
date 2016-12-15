using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Runtime.InteropServices.ComTypes;
using DeploymentChecker.Controllers;

namespace DeploymentChecker
{
    public class Program
    {
        static void Main(string[] args)
        {
            DeploymentReport.Run();

        }
    }   
}

