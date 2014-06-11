using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Data.SqlClient;
using System.Data;
using System.ComponentModel;
using System.Windows.Forms;
namespace Utility
{
    class DatabaseHandler
    {
        const string clientSelectQuery = "Select ClientID,ClientName,ReportFolder from {0} where UserID is not null and ParentClintID is not null ";
        const string selectReportType = "Select ReportTypeID,ReportTypeName from {0} ";
        //const string clientReportsSelectQuery = "SELECT Report_ID,Report_Name FROM T_ReportMaster where Client_ID in {0} AND Active=1 ";
        //const string clientReportsSelectQuery = "SELECT ClientID,ReportID , ReportName,ReportFormat FROM T_ReportMaster inner join t_ClientMail where ClientID in {0} AND Active=1 Order by ClientID ";
        const string clientReportsSelectQuery = "SELECT RMaster.ClientID,RMaster.ReportID,RMaster.ReportName,RMaster.ReportFormat FROM T_ReportMaster RMaster inner join t_ClientMail CMail ON (CMail.ClientID = RMaster.ClientID AND RMaster.ClientID in {0} AND Active=1) INNER JOIN T_Reports Report on (RMaster.ReportID = Report.ReportID) INNER JOIN T_ReportType RType ON (RType.ReportTypeID = Report.RepprtTypeID AND Report.RepprtTypeID IN {1}) Order by ClientID";
        public static string _connectionString = String.Empty;
        public static SqlConnection _connection;
        public static Dictionary<int, String> dic_ClientMapping=new Dictionary<int,string>();
        public static DataTable ClientReportsDataTable;
       

        public DatabaseHandler(String ConnectionString)
        {
            _connectionString = ConnectionString;
        }

        public static DataTable getClients()
        {
            DataTable clientDataTable=new DataTable();
            using (_connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand(String.Format(clientSelectQuery, "T_Client"), _connection))
                {
                    command.CommandType = CommandType.Text;
                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    adapter.Fill(clientDataTable);
                }
            }

            foreach (DataColumn col in clientDataTable.Columns)
            {
                col.ReadOnly = true;
            }
            return clientDataTable;
        }

        public static DataTable addCheckBoxToDataTable(DataTable clientDataTable)
        {
            foreach (DataRow row in clientDataTable.Rows)
            {
                dic_ClientMapping.Add(Convert.ToInt32(row[0]),row[1].ToString());
            }
            return clientDataTable;
        }

        public static DataTable getClientReports(List<int> clientIDs,String ReportTypeIDs)
        {
            ClientReportsDataTable = new DataTable();
            StringBuilder clientBuilder=new StringBuilder("(");
            foreach (int id in clientIDs)
            {
                clientBuilder.Append(Convert.ToString(id));
                clientBuilder.Append(",");
            }
            clientBuilder.Remove((clientBuilder.Length - 1), 1);
            clientBuilder.Append(")");
            

            try
            {
                using (_connection = new SqlConnection(_connectionString))
                {
                    using (SqlCommand command = new SqlCommand(String.Format(clientReportsSelectQuery, clientBuilder.ToString(), ReportTypeIDs), _connection))
                    {
                        command.CommandType = CommandType.Text;
                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        adapter.Fill(ClientReportsDataTable);
                    }
                }
            }
            catch (SqlException exp)
            { 
            }
            return ClientReportsDataTable;
        }

        public static DataTable getReportTypes()
        {
            DataTable reportTypeDataTable = new DataTable();
            try
            {
                using (_connection = new SqlConnection(_connectionString))
                {
                    using (SqlCommand command = new SqlCommand(String.Format(selectReportType, "T_ReportType"), _connection))
                    {
                        command.CommandType = CommandType.Text;
                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        adapter.Fill(reportTypeDataTable);
                    }
                }
                return reportTypeDataTable;
            }
            catch (Exception ex)
            {
                return reportTypeDataTable;
            }
        }


    }
}
