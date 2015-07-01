using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;

namespace LogExport
{
    public partial class MainForm : Form
    {
        string IIS_DIR = string.Empty;
        DBType DBType = DBType.None;
        string DbUser = string.Empty;
        string DbPass = string.Empty;
        string DbAddr = string.Empty;
        string DbName = string.Empty;
        string DbTNam = string.Empty;
        String Software = "#Software: Microsoft Internet Information Services 6.0";
        String Version = "#Version: 1.0";

        public MainForm()
        {
            InitializeComponent();
#if DEBUG
            IIS_DIR = @"C:\WINDOWS\system32\LogFiles\W3SVC1872720702";
#endif
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "请选择一个要导出的IIS目志目录\r\n\r\n如果不清楚可以到网站属性->网站->属性中可以找到";
            dialog.SelectedPath = @"C:\WINDOWS\system32\LogFiles";
            dialog.ShowNewFolderButton = false;
            DialogResult result = dialog.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                IIS_DIR = dialog.SelectedPath;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DbSettings dialog = new DbSettings();
            DialogResult result = dialog.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                DBType = dialog.DBType;
                DbUser = dialog.DBUser;
                DbPass = dialog.DBPass;
                DbName = dialog.DBName;
                DbAddr = dialog.DBAddr;
                DbTNam = dialog.DBTNam;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {

            if (string.IsNullOrEmpty(IIS_DIR))
            {
                MessageBox.Show("请选择IIS 日志目录");
                return;
            }

            if (DBType == DBType.None)
            {
                MessageBox.Show("请设置目标数据库");
                return;
            }
            ThreadPool.QueueUserWorkItem(new WaitCallback(ReadLog));
        }

        private void ReadLog(object state)
        {

            IDbConnection conn = new SqlConnection();
            conn.ConnectionString = string.Format("Data Source={0};Initial Catalog={1};User ID={2};Password={3}",
                this.DbAddr,
                this.DbName,
                this.DbUser,
                this.DbPass);
            conn.Open();
            CreateTempTable(conn);

            foreach (var f in Directory.GetFiles(IIS_DIR, "*.log"))
            {
                FileStream fs = File.OpenRead(f);
                StreamReader sr = new StreamReader(fs);
                String Line = null;
                String[] Fields = null;
                DateTime Date;

                Line = sr.ReadLine();
                if (Line != Software)
                {
                    OutputLog("不能识别 IIS 版本");
                    continue;
                }
                Line = sr.ReadLine();
                if (Line != Version)
                {
                    OutputLog("不能识别日志版本");
                    continue;
                }
                Line = sr.ReadLine();
                Date = Convert.ToDateTime(Line.Substring(7));

                Line = sr.ReadLine();
                Fields = Line.Substring(9).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                while ((Line = sr.ReadLine()) != null)
                {
                    String[] item = new String[22];
                    Line.Split(' ').CopyTo(item, 0);
                    ExportToMsSql(conn, item);
                    OutputLog(Line);
                }
                sr.Close();
                fs.Close();
            }
            IDbCommand cmd = conn.CreateCommand();
            cmd.CommandText = string.Format(@"
if exists(select * from sysobjects where name = '{0}' and type='u')
drop table {0}
SELECT * INTO {0} FROM #TEMP", DbTNam);
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        private void CreateTempTable(IDbConnection conn)
        {
            string sql = string.Empty;

            sql = @"create table #temp(
[date] varchar(50),
[time] varchar(50),
[s-sitename] varchar(50),
[s-computername] varchar(50),
[s-ip] varchar(50),
[cs-method] varchar(50),
[cs-uri-stem] varchar(50),
[cs-uri-query] varchar(50),
[s-port] varchar(50),
[cs-username] varchar(50),
[c-ip] varchar(50),
[cs-version] varchar(50),
[cs(User-Agent)] varchar(50),
[cs(Cookie)] varchar(50),
[cs(Referer)] varchar(50),
[cs-host] varchar(50),
[sc-status] varchar(50),
[sc-substatus] varchar(50),
[sc-win32-status] varchar(50),
[sc-bytes] varchar(50),
[cs-bytes] varchar(50),
[time-taken] varchar(50)
)"; 

sql = @"create table #temp(
[date] varchar(50),
[time] varchar(50),
[s-sitename] varchar(50),
[s-computername] varchar(50),
[s-ip] varchar(50),
[cs-method] varchar(50),
[cs-uri-stem] varchar(50),
[cs-uri-query] varchar(50),
[s-port] varchar(50),
[cs-username] varchar(50),
[c-ip] varchar(50),
[cs-version] varchar(50),
[cs(User-Agent)] varchar(50),
[cs(Cookie)] varchar(50),
[cs(Referer)] varchar(50),
[cs-host] varchar(50),
[sc-status] varchar(50),
[sc-substatus] varchar(50),
[sc-win32-status] varchar(50),
[sc-bytes] varchar(50),
[cs-bytes] varchar(50),
[time-taken] varchar(50)
)";
            IDbCommand cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
        }

        private void ExportToMySql(IDbConnection conn, object[] item)
        {
            string sql = @"INSERT INTO #temp
           ([date]
           ,[time]
           ,[s-sitename]
           ,[s-computername]
           ,[s-ip]
           ,[cs-method]
           ,[cs-uri-stem]
           ,[cs-uri-query]
           ,[s-port]
           ,[cs-username]
           ,[c-ip]
           ,[cs-version]
           ,[cs(User-Agent)]
           ,[cs(Cookie)]
           ,[cs(Referer)]
           ,[cs-host]
           ,[sc-status]
           ,[sc-substatus]
           ,[sc-win32-status]
           ,[sc-bytes]
           ,[cs-bytes]
           ,[time-taken])
     VALUES
           (@date 
           ,@time 
           ,@s_sitename 
           ,@s_computername 
           ,@s_ip 
           ,@cs_method 
           ,@cs_uri_stem 
           ,@cs_uri_query 
           ,@s_port 
           ,@cs_username 
           ,@c_ip 
           ,@cs_version 
           ,@cs_User_Agent_ 
           ,@cs_Cookie_ 
           ,@cs_Referer_ 
           ,@cs_host 
           ,@sc_status 
           ,@sc_substatus 
           ,@sc_win32_status 
           ,@sc_bytes 
           ,@cs_bytes 
           ,@time_taken)";

            SqlCommand cmd = conn.CreateCommand() as SqlCommand;
            cmd.CommandText = sql;

            cmd.Parameters.Add("@date", SqlDbType.VarChar, 50).Value = item[0];
            cmd.Parameters.Add("@time", SqlDbType.VarChar, 50).Value = item[1];
            cmd.Parameters.Add("@s_sitename", SqlDbType.VarChar, 50).Value = item[2];
            cmd.Parameters.Add("@s_computername", SqlDbType.VarChar, 50).Value = item[3];
            cmd.Parameters.Add("@s_ip", SqlDbType.VarChar, 50).Value = item[4];
            cmd.Parameters.Add("@cs_method", SqlDbType.VarChar, 50).Value = item[5];
            cmd.Parameters.Add("@cs_uri_stem", SqlDbType.VarChar, 50).Value = item[6];
            cmd.Parameters.Add("@cs_uri_query", SqlDbType.VarChar, 50).Value = item[7];
            cmd.Parameters.Add("@s_port", SqlDbType.VarChar, 50).Value = item[8];
            cmd.Parameters.Add("@cs_username", SqlDbType.VarChar, 50).Value = item[9];
            cmd.Parameters.Add("@c_ip", SqlDbType.VarChar, 50).Value = item[10];
            cmd.Parameters.Add("@cs_version", SqlDbType.VarChar, 50).Value = item[11];
            cmd.Parameters.Add("@cs_User_Agent_", SqlDbType.VarChar, 50).Value = item[12];
            cmd.Parameters.Add("@cs_Cookie_", SqlDbType.VarChar, 50).Value = item[13];
            cmd.Parameters.Add("@cs_Referer_", SqlDbType.VarChar, 50).Value = item[14];
            cmd.Parameters.Add("@cs_host", SqlDbType.VarChar, 50).Value = item[15];
            cmd.Parameters.Add("@sc_status", SqlDbType.VarChar, 50).Value = item[16];
            cmd.Parameters.Add("@sc_substatus", SqlDbType.VarChar, 50).Value = item[17];
            cmd.Parameters.Add("@sc_win32_status", SqlDbType.VarChar, 50).Value = item[18];
            cmd.Parameters.Add("@sc_bytes", SqlDbType.VarChar, 50).Value = item[19];
            cmd.Parameters.Add("@cs_bytes", SqlDbType.VarChar, 50).Value = item[20];
            cmd.Parameters.Add("@time_taken	", SqlDbType.VarChar, 50).Value = item[21];

            cmd.ExecuteNonQuery();
        }

        private void ExportToMsSql(IDbConnection conn,object[] item)
        {
            string sql = @"INSERT INTO #temp
           ([date]
           ,[time]
           ,[s-sitename]
           ,[s-computername]
           ,[s-ip]
           ,[cs-method]
           ,[cs-uri-stem]
           ,[cs-uri-query]
           ,[s-port]
           ,[cs-username]
           ,[c-ip]
           ,[cs-version]
           ,[cs(User-Agent)]
           ,[cs(Cookie)]
           ,[cs(Referer)]
           ,[cs-host]
           ,[sc-status]
           ,[sc-substatus]
           ,[sc-win32-status]
           ,[sc-bytes]
           ,[cs-bytes]
           ,[time-taken])
     VALUES
           (@date 
           ,@time 
           ,@s_sitename 
           ,@s_computername 
           ,@s_ip 
           ,@cs_method 
           ,@cs_uri_stem 
           ,@cs_uri_query 
           ,@s_port 
           ,@cs_username 
           ,@c_ip 
           ,@cs_version 
           ,@cs_User_Agent_ 
           ,@cs_Cookie_ 
           ,@cs_Referer_ 
           ,@cs_host 
           ,@sc_status 
           ,@sc_substatus 
           ,@sc_win32_status 
           ,@sc_bytes 
           ,@cs_bytes 
           ,@time_taken)";

            SqlCommand cmd = conn.CreateCommand() as SqlCommand;
            cmd.CommandText = sql;

            cmd.Parameters.Add("@date", SqlDbType.VarChar, 50).Value = item[0] != null ? item[0] : "";
            cmd.Parameters.Add("@time", SqlDbType.VarChar, 50).Value = item[1] != null ? item[1] : "";
            cmd.Parameters.Add("@s_sitename", SqlDbType.VarChar, 50).Value = item[2] != null ? item[2] : "";
            cmd.Parameters.Add("@s_computername", SqlDbType.VarChar, 50).Value = item[3] != null ? item[3] : "";
            cmd.Parameters.Add("@s_ip", SqlDbType.VarChar, 50).Value = item[4] != null ? item[4] : "";
            cmd.Parameters.Add("@cs_method", SqlDbType.VarChar, 50).Value = item[5] != null ? item[5] : "";
            cmd.Parameters.Add("@cs_uri_stem", SqlDbType.VarChar, 50).Value = item[6] != null ? item[6] : "";
            cmd.Parameters.Add("@cs_uri_query", SqlDbType.VarChar, 50).Value = item[7] != null ? item[7] : "";
            cmd.Parameters.Add("@s_port", SqlDbType.VarChar, 50).Value = item[8] != null ? item[8] : "";
            cmd.Parameters.Add("@cs_username", SqlDbType.VarChar, 50).Value = item[9] != null ? item[8] : "";
            cmd.Parameters.Add("@c_ip", SqlDbType.VarChar, 50).Value = item[10] != null ? item[10] : "";
            cmd.Parameters.Add("@cs_version", SqlDbType.VarChar, 50).Value = item[11] != null ? item[11] : "";
            cmd.Parameters.Add("@cs_User_Agent_", SqlDbType.VarChar, 50).Value = item[12] != null ? item[12] : "";
            cmd.Parameters.Add("@cs_Cookie_", SqlDbType.VarChar, 50).Value = item[13] != null ? item[13] : "";
            cmd.Parameters.Add("@cs_Referer_", SqlDbType.VarChar, 50).Value = item[14] != null ? item[14] : "";
            cmd.Parameters.Add("@cs_host", SqlDbType.VarChar, 50).Value = item[15] != null ? item[15] : "";
            cmd.Parameters.Add("@sc_status", SqlDbType.VarChar, 50).Value = item[16] != null ? item[16] : "";
            cmd.Parameters.Add("@sc_substatus", SqlDbType.VarChar, 50).Value = item[17] != null ? item[17] : "";
            cmd.Parameters.Add("@sc_win32_status", SqlDbType.VarChar, 50).Value = item[18] != null ? item[18] : "";
            cmd.Parameters.Add("@sc_bytes", SqlDbType.VarChar, 50).Value = item[19] != null ? item[19] : "";
            cmd.Parameters.Add("@cs_bytes", SqlDbType.VarChar, 50).Value = item[20] != null ? item[20] : "";
            cmd.Parameters.Add("@time_taken", SqlDbType.VarChar, 50).Value = item[21] != null ? item[21] : "";

            cmd.ExecuteNonQuery();
        }

        void OutputLog(string log)
        {
            this.textBox1.Invoke(new EventHandler(delegate(object o, EventArgs e)
            {
                this.textBox1.AppendText(log);
                this.textBox1.AppendText("\r\n");
            }));
        }
    }
}
