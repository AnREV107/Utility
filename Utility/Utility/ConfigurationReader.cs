using System;
using System.Text;
using Microsoft.Web.Administration;
using System.Collections.Generic;
using System.Data;

namespace Utility
{
    class ConfigurationReader
    {
        public static DataTable dt_reportPaths;
        public static KeyValuePair<String, String> _reportPortalPath;

        static ConfigurationReader()
        {
            dt_reportPaths = new DataTable();
            _reportPortalPath =new KeyValuePair<string,string>();
        }

        public static DataTable getReportPortals()
        {
            using (ServerManager serverManager = new ServerManager())
            {
                DataColumn ReportPortal = new DataColumn("ReportPortal", Type.GetType("System.String"));
                ReportPortal.Caption = "Report Portal";
                DataColumn PhysicalLocation = new DataColumn("PhysicalLocation", Type.GetType("System.String"));
                ReportPortal.Caption = "Physical Location";
                dt_reportPaths.Columns.Add(ReportPortal);
                dt_reportPaths.Columns.Add(PhysicalLocation);
                SiteCollection sites = serverManager.Sites;
                Configuration config = serverManager.GetApplicationHostConfiguration();
                ConfigurationSection sitesSection = config.GetSection("system.applicationHost/sites");
                String siteName = String.Empty;
                String path = string.Empty;
                String pathDefault = String.Empty;
                VirtualDirectoryCollection vDirectory;
                String siteNameDefault = string.Empty;
                foreach (Site site in sites)
                {
                    foreach (Application app in site.Applications)
                    {
                        siteNameDefault = app.Path.ToString();
                        if (!siteNameDefault.Equals("/"))
                        {
                            path = app.VirtualDirectories[0].PhysicalPath.ToString();
                            siteName = app.Path.ToString();
                            dt_reportPaths.Rows.Add(siteName, path);
                        }
                    }
                }
                
             }
            return dt_reportPaths;
        }

    }
}
