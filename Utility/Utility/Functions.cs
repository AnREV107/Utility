using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.IO;
using System.Globalization;

namespace Utility
{
    class Functions
    {
        public static Boolean isSaturedayReportsAllowed = false;
        public static Boolean isSundayReportsAllowed = false;
        public static string reportPortalPath = string.Empty;
        public static DataTable isAllowedTable;

        static Functions()
        {
            isAllowedTable = new DataTable();
            DataColumn Key = new DataColumn("Key", Type.GetType("System.Boolean"));
            DataColumn Value = new DataColumn("Value", Type.GetType("System.Boolean"));
            isAllowedTable.Columns.Add(Key);
            isAllowedTable.Columns.Add(Value);
            isAllowedTable.Rows.Add(false, false);
            isAllowedTable.Rows.Add(true, true);
        }

        public static void clearComboBoxFirst(ComboBox temp)
        {
            if (temp.Items.Count >= 1)
            {
                while (temp.Items.Count >= 1)
                {
                    temp.Items.RemoveAt(0);
                    temp.Refresh();
                }
            }
            temp.Refresh();
        }

        public static Dictionary<String,List<String>> getReportsPerClientDict(DataTable ClientReportTable)
        {
            Dictionary<String, List<String>> ReportsPerClientDict = new Dictionary<String, List<String>>();
            foreach (DataRow row in ClientReportTable.Rows)
            {
                if(!ReportsPerClientDict.ContainsKey(Convert.ToString(row[0])))
                    ReportsPerClientDict.Add(Convert.ToString(row[0]), new List<string>());
               // ReportsPerClientDict[Convert.ToString(row[0])].Add(row[1]+"_"+row[2]);
                ReportsPerClientDict[Convert.ToString(row[0])].Add(Convert.ToString((row[2]+"#$$#"+row[3])));
            }
            return ReportsPerClientDict;
        }


        public static DataTable getSelectedClientDataTable(DataTable ClientTable, Dictionary<String, List<String>> ReportsPerClientDict)
        {
            ClientTable.PrimaryKey = new DataColumn[] { ClientTable.Columns[0] };
            DataTable selectedClientTable = new DataTable();
            DataColumn ClientID= new DataColumn("ClientId",Type.GetType("System.String"));
            DataColumn ClientName= new DataColumn("ClientName",Type.GetType("System.String"));
            DataColumn ReportFolder = new DataColumn("ReportFolder", Type.GetType("System.String"));
            selectedClientTable.Columns.Add(ClientID);
            selectedClientTable.Columns.Add(ClientName);
            selectedClientTable.Columns.Add(ReportFolder);
            DataRow row;
            DataRow newRow;

            foreach (String key in ReportsPerClientDict.Keys)
            {
                row = ClientTable.NewRow();
                newRow = selectedClientTable.NewRow();
                row = ClientTable.Rows.Find(key);
                newRow[0] = row[0].ToString();
                newRow[1] = row[1].ToString();
                newRow[2] = row[2].ToString();
                selectedClientTable.Rows.Add(newRow);
            }
            return selectedClientTable;
        }

        public static DataTable createApprovedAndUnapprovedDataTable(DataTable ClientTable, Dictionary<String, List<String>> ReportsPerClientDict,DateTime From ,DateTime To)
        {
            List<DateTime> dateRange = new List<DateTime>();
            dateRange = getDateRange(From,To);
            List<String> reportList;
            DataTable SelectedClientTable;
            SelectedClientTable=getSelectedClientDataTable(ClientTable, ReportsPerClientDict);
            String datestr = From.ToShortDateString();
            DataTable ApprovedAndUnapprovedDataTable = new DataTable();
            DataColumn clientName = new DataColumn("Fund", Type.GetType("System.String"));
            DataColumn reportName = new DataColumn("Report", Type.GetType("System.String"));
            DataColumn Date = new DataColumn("Date", Type.GetType("System.String"));
            DataColumn ApprovalStatus = new DataColumn("Status", Type.GetType("System.Boolean"));
            ApprovedAndUnapprovedDataTable.Columns.Add(clientName);
            ApprovedAndUnapprovedDataTable.Columns.Add(reportName);
            ApprovedAndUnapprovedDataTable.Columns.Add(Date);
            ApprovedAndUnapprovedDataTable.Columns.Add(ApprovalStatus);
            
            String[] reportNameAndFormat;
            String[] seperator =new String[]{"#$$#"};
            foreach(DataRow row in SelectedClientTable.Rows)
            {
                
                if (ReportsPerClientDict.ContainsKey(Convert.ToString(row[0])))
                {
                    reportList = ReportsPerClientDict[Convert.ToString(row[0])];
                    foreach (String report in reportList)
                    {
                        reportNameAndFormat = report.Split(seperator,StringSplitOptions.RemoveEmptyEntries);
                        
                        foreach (DateTime date in dateRange)
                        {
                            if (File.Exists(getFileName(reportNameAndFormat[0].Trim(),date,reportPortalPath.Trim(),row[2].ToString().Trim(),reportNameAndFormat[1].Trim())))
                                ApprovedAndUnapprovedDataTable.Rows.Add(Convert.ToString(row[1]), reportNameAndFormat[0], date.ToString("dd-MMM-yyyy"), true);
                            else
                                ApprovedAndUnapprovedDataTable.Rows.Add(Convert.ToString(row[1]), reportNameAndFormat[0], date.ToString("dd-MMM-yyyy"), false);     
                        }
                    
                    }
                }
            
            }
            return ApprovedAndUnapprovedDataTable;
        }

        public static String getFileName(string name,DateTime date,String ReportPortalPath,String folderName,String extension)
        {
            name = name.Replace(' ', '_');
            name = name.Replace("&", "And");
            string datestr = Convert.ToString(date.Year);
            datestr = datestr + ((Convert.ToString(date.Month).Length == 1) ? "0" + Convert.ToString(date.Month) : Convert.ToString(date.Month));
            datestr = datestr + ((Convert.ToString(date.Day).Length == 1) ? "0" + Convert.ToString(date.Day) : Convert.ToString(date.Day));
            String fullpath= name+"_"+datestr+"."+extension;
            fullpath = System.IO.Path.Combine(ReportPortalPath, folderName, fullpath);
            return fullpath;
                
        }

        public static List<DateTime> getDateRange(DateTime From, DateTime To)
        {
            List<DateTime> DateRange = new List<DateTime>();
            int totalNoOfDays= (Int32)((To.DayOfYear - From.DayOfYear)+1);
            DateTime temp = From;
            for (int i = 0; i < totalNoOfDays; i++)
            {
                if (((isSaturedayReportsAllowed && (temp.DayOfWeek == DayOfWeek.Saturday)) || (isSundayReportsAllowed && (temp.DayOfWeek == DayOfWeek.Sunday)))) 
                {
                    DateRange.Add(temp);
                }
                else if(temp.DayOfWeek != DayOfWeek.Saturday && temp.DayOfWeek != DayOfWeek.Sunday)
                {
                    DateRange.Add(temp);
                }
                temp=temp.AddDays(1);
            }
                return DateRange;
        }

        public static DataTable getApprovedClientReports(DataTable ApprovedAndUnapprovedDataTable)
        {
            DataTable ApprovedDataTable = new DataTable();
            DataColumn clientName = new DataColumn("Fund", Type.GetType("System.String"));
            DataColumn reportName = new DataColumn("Report", Type.GetType("System.String"));
            DataColumn Date = new DataColumn("Date", Type.GetType("System.String"));
           
            ApprovedDataTable.Columns.Add(clientName);
            ApprovedDataTable.Columns.Add(reportName);
            ApprovedDataTable.Columns.Add(Date);
            
            DataRow rowNew;
            foreach (DataRow row in ApprovedAndUnapprovedDataTable.Rows)
            {
                rowNew = ApprovedDataTable.NewRow();
                if (Convert.ToBoolean(row[3]))
                {
                    rowNew[0] = row[0];
                    rowNew[1] = row[1];
                    rowNew[2] = row[2];
                    ApprovedDataTable.Rows.Add(rowNew);
                }
                
            }
            return ApprovedDataTable;
        }

        public static DataTable getUnApprovedClientReports(DataTable ApprovedAndUnapprovedDataTable)
        {
            DataTable UnApprovedDataTable = new DataTable();
            DataColumn clientName = new DataColumn("Fund", Type.GetType("System.String"));
            DataColumn reportName = new DataColumn("Report", Type.GetType("System.String"));
            DataColumn Date = new DataColumn("Date", Type.GetType("System.String"));
            
            UnApprovedDataTable.Columns.Add(clientName);
            UnApprovedDataTable.Columns.Add(reportName);
            UnApprovedDataTable.Columns.Add(Date);
            
            DataRow rowNew;
            foreach (DataRow row in ApprovedAndUnapprovedDataTable.Rows)
            {
                rowNew = UnApprovedDataTable.NewRow();
                if (!Convert.ToBoolean(row[3]))
                {
                    rowNew[0] = row[0];
                    rowNew[1] = row[1];
                    rowNew[2] = row[2];
                    UnApprovedDataTable.Rows.Add(rowNew);
                }
                    
            }
            return UnApprovedDataTable;
        }
       
    }
}
