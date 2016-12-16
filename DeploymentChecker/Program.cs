using System;
using System.Collections.Generic;
using DeploymentChecker.Reports;

namespace DeploymentChecker
{
    public class Program
    {
        static void Main(string[] args)
        {
            ShowMenu();
        }

        public static void ShowMenu()
        {
            var menuItems = new List<MenuItem>()
            {
                new MenuItem(new DeploymentReport(), "Run Deployment Report."),
                new MenuItem(new CompanySettingsReport(), "Run Company Settings Report.")
            };

            Console.WriteLine(@"__________________________________________________
  ____  _____  _____ _____   _____ ______  _____ 
 |  _ \|  __ \|_   _|  __ \ / ____|  ____|/ ____|
 | |_) | |__) | | | | |  | | |  __| |__  | (___  
 |  _ <|  _  /  | | | |  | | | |_ |  __|  \___ \ 
 | |_) | | \ \ _| |_| |__| | |__| | |____ ____) |
 |____/|_|  \_\_____|_____/ \_____|______|_____/ 
__________________________________________________
");
            Console.WriteLine("Please select below actions:");
            Console.WriteLine();
            for (int i = 0; i < menuItems.Count; i++)
            {
                var menuItem = menuItems[i];
                Console.WriteLine($"    {i + 1}. {menuItem.Description}");
            }
            Console.WriteLine();
            Console.WriteLine("Input a number and press enter.");

            while (true)
            {
                string input = Console.ReadLine();
                int selection;

                if (int.TryParse(input, out selection))
                {
                    if (selection >= 1 && selection <= menuItems.Count)
                    {
                        menuItems[selection - 1].Report.Run();
                        return;
                    }
                }

                Console.WriteLine("Invalid input. Please try again.");
            }
        }

    }

    public class MenuItem
    {
        public IReport Report { get; set; }
        public string Description { get; set; }
 
        public MenuItem(IReport report, string description)
        {
            Report = report;
            Description = description;
        }
    }
 

}

