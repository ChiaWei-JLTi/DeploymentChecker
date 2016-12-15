using System;
using System.IO;

namespace DeploymentChecker.Controllers
{
    public enum ReportType
    {
        DeploymentReport,
        CompanySettingsReport
    }

    public static class CommonFunctions
    {
        public static string GetFileName(ReportType type)
        {
            var reportFolder = Path.Combine(Directory.GetCurrentDirectory(), "Reports");
            if (!Directory.Exists(reportFolder))
                Directory.CreateDirectory(reportFolder);

            return Path.Combine(reportFolder, $"{type.ToString()}_{DateTime.Now:yyyyMMdd_HHmmss}.html");
        } 
    }
}
