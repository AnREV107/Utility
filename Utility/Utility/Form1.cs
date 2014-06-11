using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Configuration;
using System.Data.SqlClient;


namespace Utility
{
    public partial class Form1 : Form
    {
        const string dsn1 = "Data Source={0};Initial Catalog={1};Integrated Security=SSPI;Connection Timeout=30";
        const string dsn2 = "Data Source={0};Initial Catalog={1};Persist Security Info=True;User ID={2};Password={3};Connection Timeout=30";
        private static SqlConnection _gConnection = null;
        static Boolean _isConnected = false;
        private static int _authenticationIndex { get; set; }
        private static DataTable ApprovedAndUnapprovedDataTable = null;
        

        public Form1()
        {
            MaximizeBox = false;
            InitializeComponent();
            comboDatabase.Visible = false;
            lblDatabase.Visible = false;
            combobox_portals.DataSource = ConfigurationReader.getReportPortals();
            combobox_portals.ValueMember = "PhysicalLocation";
            combobox_portals.DisplayMember = "ReportPortal";
            comboBoxSaturday.DataSource = Functions.isAllowedTable;
            comboBoxSaturday.ValueMember = "Value";
            comboBoxSaturday.DisplayMember = "Key";
            
            //comboBoxSunday.DataSource = Functions.isAllowedTable;
            //comboBoxSunday.ValueMember = "Value";
            //comboBoxSunday.DisplayMember = "Key";
        }

 

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                
                if (_gConnection.State == ConnectionState.Open)
                    _gConnection.Close();

                lblServerName.Enabled = true;
                lblAuthentication.Enabled = true;
                comboServerName.Enabled = true;
                comboAuthentication.Enabled = true;
                _isConnected = false;
                //toolStripLabel1.BackColor = System.Drawing.SystemColors.ButtonFace;
                lblDatabase.Visible = false;
                comboDatabase.DataSource = null;
                comboDatabase.Visible = false;
                btn_Connect.Text = "Connect";
                btn_Cancel.Text = "Cancel";
                btn_Connect.Enabled = true;
                richTBUsernae.Text = String.Empty;
                tb_Password.Text = String.Empty;
                dataGridView1.DataSource = null;
                dataGridView2.DataSource = null;
                dataGridView3.DataSource = null;
                dataGridView1.Refresh();
                dataGridView2.Refresh();
                dataGridView3.Refresh();
                DatabaseHandler._connectionString = String.Empty; ;
                //toolStripLabel1.Text = "Database disconnected .";
                //toolStripLabel1.ForeColor = System.Drawing.Color.Blue;
                showResultStatus.Text = "Connection terminated.";
            }
            catch (Exception ex)
            {

            }
        }

        private void frmLoad(object sender, EventArgs e)
        {
            loadActions();
        }

        private void loadActions()
        {
            String fileName = ConfigurationManager.AppSettings.Get("ServerNames");
            fileName = "../" + fileName;
            String appPath = Application.StartupPath;
            String filePath = System.IO.Path.Combine(appPath, fileName);
            if (File.Exists(fileName))
            {
                comboServerName.DataSource = File.ReadAllLines(filePath);
                comboServerName.SelectedIndex = 0;
            }
            comboServerName.BackColor = System.Drawing.Color.FromArgb(245, 245, 245);

            // Loading Authentication types
            String[] authenticationTypes = new String[] { "Windows Authentication", "SQL Server Authentication" };
            comboAuthentication.DataSource = authenticationTypes;
            comboAuthentication.SelectedIndex = 0;
            comboAuthentication.BackColor = System.Drawing.Color.FromArgb(245, 245, 245);
        }

        private String createConnectionString(Boolean isConnected)
        {
            if (isConnected)
            {
                if (_authenticationIndex == 0)
                    return String.Format(dsn1, comboServerName.Text, comboDatabase.Text);
                else
                    return String.Format(dsn2, comboServerName.Text, comboDatabase.Text, richTBUsernae.Text, tb_Password.Text);
            }
            else
            {
                if (comboAuthentication.SelectedIndex == 0)
                    return String.Format(dsn1, comboServerName.Text, "master");
                else
                    return String.Format(dsn2, comboServerName.Text, "master", richTBUsernae.Text, tb_Password.Text);
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboAuthentication.SelectedIndex == 1)
            {
                tb_Password.Enabled = true;
                richTBUsernae.Enabled = true;
                checkBoxRememberMe.Enabled = true;
                lblUsername.Enabled = true;
                lblPassword.Enabled = true;
            }
            else
            {
                tb_Password.Enabled = false;
                richTBUsernae.Enabled = false;
                checkBoxRememberMe.Enabled = false;
                lblUsername.Enabled = false;
                lblPassword.Enabled = false;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            bool isDatabaseLoadComplete = false;
            btn_Connect.Enabled = false;
            try
            {
                if (!_isConnected)
                    using (SqlConnection connection = new SqlConnection(createConnectionString(_isConnected)))
                    {
                        showResultStatus.Text = "Connecting";
                        connection.Open();
                        _gConnection = connection;
                        Functions.clearComboBoxFirst(comboDatabase);
                        SqlCommand command = new SqlCommand("exec sp_databases", connection);
                        var reader = command.ExecuteReader();

                        while (reader.Read())
                        {
                            isDatabaseLoadComplete = true;
                            comboDatabase.Items.Add(reader.GetString(0));
                        }
                        reader.Close();
                        if (isDatabaseLoadComplete)
                        {
                            _authenticationIndex = comboAuthentication.SelectedIndex;
                            comboDatabase.Visible = true;
                            comboDatabase.Enabled = true;
                            lblDatabase.Visible = true;
                            lblDatabase.Enabled = true;
                            comboDatabase.SelectedIndex = 3;
                            comboServerName.Enabled = false;
                            comboAuthentication.Enabled = false;
                            lblServerName.Enabled = false;
                            lblAuthentication.Enabled = false;
                            richTBUsernae.Enabled = false;
                            tb_Password.Enabled = false;
                            checkBoxRememberMe.Enabled = false;
                            btn_Connect.Text = "Save";
                            btn_Cancel.Text = "Disconnect";
                            _isConnected = true;
                            
                        }
                        btn_Cancel.Enabled = true;
                        btn_Connect.Enabled = true;
                        showResultStatus.Text = "Connected";
                        //showResultStatus.Text = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                    }
                else
                {
                   
                    showResultStatus.Text = "Saving database catalog.";
                    btn_Connect.Enabled = false;
                    lblDatabase.Enabled = false;
                    comboDatabase.Enabled = false;
                    richTBUsernae.Enabled = false;
                    tb_Password.Enabled = false;
                    DatabaseHandler._connectionString = createConnectionString(_isConnected);
                    populateClientDataTable();
                    showResultStatus.Text = "Database catalog saved.";
                    Fund._databaseName = comboDatabase.Text;
                    comboBoxReportType.DataSource = DatabaseHandler.getReportTypes();
                    comboBoxReportType.DisplayMember = "ReportTypeName";
                    comboBoxReportType.ValueMember = "ReportTypeID";
                }
            }
            catch (SqlException sqlExcp) { showResultStatus.Text = "Login Failed . Try with correct credentials."; }
            catch (Exception exp) { }
            
        }

        
        private void dataGridView1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            List<int> ClientData;
            ClientData = FundStateManager.getFundState();
            if (ClientData == null || ClientData.Count == 0)
                for (int i = 0; i < dataGridView1.RowCount; i++)
                    dataGridView1.Rows[i].Cells["CheckFund"].Value = true;
            else
            {
                int i=0;
                foreach (DataRow row in DatabaseHandler.getClients().Rows)
                {
                    if (ClientData.Contains(Convert.ToInt32(row[0])))
                    {
                       dataGridView1.Rows[i].Cells["CheckFund"].Value = true;
                    }
                    i++;
                }
            }
        }

        private void btn_saveFunds_click(object sender, EventArgs e)
        {
            List<int> fundState = new List<int>();
            List<Int32> ClientToBeWatched = new List<Int32>();
            StringBuilder reportTypreIDs = new StringBuilder("(");
            reportTypreIDs = reportTypreIDs.Append(comboBoxReportType.SelectedValue.ToString());
            reportTypreIDs = reportTypreIDs.Append(")");

            for (int i = 0; i < dataGridView1.RowCount; i++)
            {
                if (Convert.ToBoolean(dataGridView1.Rows[i].Cells["CheckFund"].Value))
                {
                    ClientToBeWatched.Add(Convert.ToInt32(dataGridView1.Rows[i].Cells["ClientId"].Value));
                    fundState.Add(Convert.ToInt32(dataGridView1.Rows[i].Cells["ClientId"].Value));
                }
            }

            
            
            // get all reports of above selected clients;
            DatabaseHandler.getClientReports(ClientToBeWatched, reportTypreIDs.ToString());

            // Saving Fund class State to xml file
            if (FundStateManager.saveFundState(fundState))
            {
                showResultStatus.Text = "Funds Saved.";
            }
            else
            {
                showResultStatus.Text = "Funds saving failed";
            }
            btn_saveFunds.Enabled = false;
            dataGridView1.ReadOnly = true;
            btn_Selecte_All.Enabled = false;
            btn_Unselect_All.Enabled = false;

           
        }

        protected void populateClientDataTable()
        {
            
            DataTable ClientSDataToBeViewd = new DataTable();
            dataGridView1.DataSource = null;
            dataGridView1.Columns.Clear();
            dataGridView1.Refresh();
            DataGridViewCheckBoxColumn column = new DataGridViewCheckBoxColumn();
            column.HeaderText = "Check Fund";
            column.FalseValue = false;
            column.TrueValue = true;
            column.IndeterminateValue = false;
            column.Name = "CheckFund";
            column.ValueType = Type.GetType("System.Boolean");
            dataGridView1.Columns.Insert(0, column);
            dataGridView1.DataSource = DatabaseHandler.getClients();
 
        }

        private void btn_reset_Click(object sender, EventArgs e)
        {
            btn_Selecte_All.Enabled = true;
            btn_Unselect_All.Enabled = true;
            dataGridView1.ReadOnly = false;
            btn_saveFunds.Enabled = true;
            populateClientDataTable();
            dataGridView2.DataSource = null;
            dataGridView2.Refresh();
            dataGridView3.DataSource = null;
            dataGridView3.Refresh();
        }

        private void combobox_portals_SelectedIndexChanged(object sender, EventArgs e)
        {

            Functions.reportPortalPath = combobox_portals.SelectedValue.ToString();
            
            

        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            try
            {
                if (dateTimePicker2.Value.Year == dateTimePicker3.Value.Year)
                {
                    if (((dateTimePicker2.Value.DayOfYear - dateTimePicker3.Value.DayOfYear) + 1) >= 1)
                    {
                        showResultStatus.Text = "";
                        ApprovedAndUnapprovedDataTable = Functions.createApprovedAndUnapprovedDataTable((DataTable)dataGridView1.DataSource, Functions.getReportsPerClientDict(DatabaseHandler.ClientReportsDataTable), dateTimePicker3.Value, dateTimePicker2.Value);
                        dataGridView2.DataSource = Functions.getUnApprovedClientReports(ApprovedAndUnapprovedDataTable);
                        dataGridView3.DataSource = Functions.getApprovedClientReports(ApprovedAndUnapprovedDataTable);
                    }
                    else
                        showResultStatus.Text = "To date can not be less than From date.";
                }
                else
                {
                    if (dateTimePicker2.Value.Year > dateTimePicker3.Value.Year)
                    {
                        showResultStatus.Text = "";
                        ApprovedAndUnapprovedDataTable = Functions.createApprovedAndUnapprovedDataTable((DataTable)dataGridView1.DataSource, Functions.getReportsPerClientDict(DatabaseHandler.ClientReportsDataTable), dateTimePicker3.Value, dateTimePicker2.Value);
                        dataGridView2.DataSource = Functions.getUnApprovedClientReports(ApprovedAndUnapprovedDataTable);
                        dataGridView3.DataSource = Functions.getApprovedClientReports(ApprovedAndUnapprovedDataTable);
                    }
                    else
                        showResultStatus.Text = "To date can not be less than From date.";
                }
            }
            catch (Exception exec)
            { showResultStatus.Text = "Please try again."; }
            
        }

        private void comboBoxSaturday_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxSaturday.SelectedIndex == 0)
                Functions.isSaturedayReportsAllowed = false;
            else
                Functions.isSaturedayReportsAllowed = true;
        }

        private void btn_Selecte_All_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < dataGridView1.RowCount; i++)
            {
                dataGridView1.Rows[i].Cells["CheckFund"].Value = true;
            }
        }

        private void btn_Unselect_All_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < dataGridView1.RowCount; i++)
            {
                dataGridView1.Rows[i].Cells["CheckFund"].Value = false;
            }
        }

        private void comboDatabase_SelectedIndexChanged(object sender, EventArgs e)
        {
            FundStateManager.fileName = comboDatabase.Text.ToString();
        }

        

    }
}
