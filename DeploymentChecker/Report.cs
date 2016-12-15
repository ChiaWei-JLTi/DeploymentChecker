using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

public static class Report
{
    public static void Run()
    {
        var projectsFolder = ConfigurationManager.AppSettings["ProjectsFolder"];
        var projectName = ConfigurationManager.AppSettings["ProjectName"];
        var idServerRelativePath = ConfigurationManager.AppSettings["IdServerSettingPath"];
        var dbSettingRelativePath = ConfigurationManager.AppSettings["DbSettingPath"];
        var builds = new List<BuildInfo>();

        foreach(var projectFolder in Directory.GetDirectories(projectsFolder))
        {
            if(!Path.GetFileName(projectFolder).ToLower().Contains(projectName.ToLower()))
                continue;

            foreach (var buildFolder in Directory.GetDirectories(projectFolder).Where(x => x.ToLower().Contains(projectName.ToLower())))
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
            }
        }

        Generate(builds);
    }

    public static void Generate(IEnumerable<BuildInfo> buildInfos)
    {
        var reportFolder = Path.Combine(Directory.GetCurrentDirectory(), "Report");
        if (!Directory.Exists(reportFolder))
            Directory.CreateDirectory(reportFolder);

        var reportName = Path.Combine(reportFolder, $"Report_{DateTime.Now:yyyyMMdd_HHmmss}.html");

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
</style>
</head>
<body>
<h1>
Overview of Deployments
</h1>
<table>");

            //Titles
            writer.WriteLine("<tr>");
            foreach (var title in BuildInfo.GetTitles()) 
            {
                writer.WriteLine($"<th>{title}</th>");
            }
            writer.WriteLine("</tr>");

            //Datas
            foreach (var buildInfo in buildInfos) 
            {
                writer.WriteLine(buildInfo.HasError ? "<tr class=\"error\">" : "<tr>");
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
