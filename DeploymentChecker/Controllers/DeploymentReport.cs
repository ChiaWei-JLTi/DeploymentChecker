using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using DeploymentChecker.Models;

namespace DeploymentChecker.Controllers
{
    public static class DeploymentReport
    {
        public static void Run()
        {
            var projectsFolder = ConfigurationManager.AppSettings["ProjectsFolder"];
            var projectName = ConfigurationManager.AppSettings["ProjectName"];
            var idServerRelativePath = ConfigurationManager.AppSettings["IdServerSettingPath"];
            var dbSettingRelativePath = ConfigurationManager.AppSettings["DbSettingPath"];
            var builds = new List<BuildInfo>();
            BuildInfo lastestBuild = null;

            var projectFolders = Directory.GetDirectories(projectsFolder).OrderBy(Directory.GetCreationTime);
            foreach(var projectFolder in projectFolders)
            {
                if(!Path.GetFileName(projectFolder).ToLower().Contains(projectName.ToLower()))
                    continue;

                var buildFolders = Directory.GetDirectories(projectFolder).OrderBy(Directory.GetCreationTime);
                foreach (var buildFolder in buildFolders)
                {
                    if(!Path.GetFileName(buildFolder).ToLower().Contains(projectName.ToLower()))
                        continue;

                    var build = new BuildInfo
                    {
                        Tenant = Path.GetFileName(projectFolder),
                        BuildVersion = Path.GetFileName(buildFolder),
                        CreatedTime = Directory.GetCreationTime(projectFolder).ToString(CultureInfo.InvariantCulture)
                    };

                    var idServerPath = $"{buildFolder}\\{idServerRelativePath}";
                    var dbSettingPath = $"{buildFolder}\\{dbSettingRelativePath}";
                    build.ReadIdServerSetting(idServerPath);
                    build.ReadDbSetting(dbSettingPath);
                    builds.Add(build);

                    lastestBuild = build;
                }

                if (lastestBuild != null)
                    lastestBuild.IsLatestBuild = true;
            }

            Generate(builds);
        }

        public static void Generate(IEnumerable<BuildInfo> buildInfos)
        {
            var reportName = CommonFunctions.GetFileName(ReportType.DeploymentReport);
            using (var writer = File.CreateText(reportName)) 
            {
                writer.WriteLine(@"<!DOCTYPE html>
<html>
<head>
<title>Report</title>
<style>
table {
    border-collapse: collapse;
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
.error{
    color: red;
}
.top-divider{
    border-top: 3pt double #ff4d4d;
}
</style>
</head>
<body>
<h1>
Overview of Deployments
</h1>
<table>");

                //Titles
                writer.WriteLine("<tr>");
                writer.WriteLine("<th></th>");
                foreach (var title in BuildInfo.GetTitles()) 
                {
                    writer.WriteLine($"<th>{title}</th>");
                }
                writer.WriteLine("</tr>");

                //Datas
                var previousTenant = buildInfos.Any() ? buildInfos.ToList()[0].Tenant : "";
                foreach (var buildInfo in buildInfos)
                {
                    var classes = new List<string>();

                    if(buildInfo.Tenant != previousTenant)
                        classes.Add("top-divider");

                    previousTenant = buildInfo.Tenant;

                    if(buildInfo.HasError)
                        classes.Add("error");

                    if(classes.Any())
                        writer.WriteLine($"<tr class=\"{string.Join(" ", classes)}\">");
                    else
                        writer.WriteLine("<tr>");

                    writer.WriteLine($"<td>{(buildInfo.IsLatestBuild ? "*" : "")}</td>");
                    foreach (var data in BuildInfo.GetContents(buildInfo)) 
                    {
                        writer.WriteLine($"<td>{data}</td>");
                    }
                    writer.WriteLine("</tr>");

                }

                writer.WriteLine(@"</table>
</body>
</html>");

                Process.Start(reportName);
            }
        }
    }
}

