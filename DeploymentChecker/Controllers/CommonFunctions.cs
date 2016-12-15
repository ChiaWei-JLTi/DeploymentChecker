using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeploymentChecker.Controllers
{
    public static class CommonFunctions
    {
        public enum ReportType
        {
            DeploymentReport,
            CompanySettingsReport
        }

        public static string GetFileName(ReportType type)
        {
            var reportFolder = Path.Combine(Directory.GetCurrentDirectory(), "Reports");
            if (!Directory.Exists(reportFolder))
                Directory.CreateDirectory(reportFolder);

            return Path.Combine(reportFolder, $"{type.ToString()}_{DateTime.Now:yyyyMMdd_HHmmss}.html");
        } 
    }
}
