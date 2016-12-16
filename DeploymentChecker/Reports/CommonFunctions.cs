using System;
using System.IO;

namespace DeploymentChecker.Reports
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

            var reportSubFolder = Path.Combine(reportFolder, type.ToString());
            if (!Directory.Exists(reportSubFolder))
                Directory.CreateDirectory(reportSubFolder);

            return Path.Combine(reportSubFolder, $"{type.ToString()}_{DateTime.Now:yyyyMMdd_HHmmss}.html");
        } 
    }
}
