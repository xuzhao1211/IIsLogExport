using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Data.SqlClient;

namespace LogExport
{
    public partial class DbSettings : Form
    {
        public DbSettings()
        {
            InitializeComponent();

#if DEBUG
            this.comboBox1.Text = "MS SQLServer";
            this.textBox1.Text = "192.168.1.102";
            this.textBox2.Text = "sa";
            this.textBox3.Text = "juju9";
            this.textBox4.Text = "tempdb";
            this.textBox5.Text = "iislog";
#endif
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (DBType ==  DBType.MySql)
            {
                MySqlConnection mysqlConn = new MySqlConnection();
                mysqlConn.ConnectionString = string.Format("Data Source={0};Initial Catalog={1};User ID={2};Password={3}",
                    this.DBAddr,
                    this.DBName,
                    this.DBUser,
                    this.DBPass);
                try
                {
                    mysqlConn.Open();
                    mysqlConn.Close();
                    DialogResult = DialogResult.OK;
                }
                catch 
                {
                    MessageBox.Show("无法连接数据库");
                }
            }
            else if (DBType== DBType.MSSql)
            {
                SqlConnection sqlConn = new SqlConnection();
                sqlConn.ConnectionString = string.Format("Data Source={0};Initial Catalog={1};User ID={2};Password={3}",
                    this.DBAddr,
                    this.DBName,
                    this.DBUser,
                    this.DBPass);
                try
                {
                    sqlConn.Open();
                    sqlConn.Close();
                    DialogResult = DialogResult.OK;
                }
                catch
                {
                    MessageBox.Show("无法连接数据库");
                }
            }

            if (DialogResult == DialogResult.OK)
            {
                Close();
            }
        }

        public DBType DBType
        {
            get {
                switch (comboBox1.Text)
                {
                    case "MS SQLServer":
                        return DBType.MSSql;
                    case "MySql":
                        return DBType.MySql;
                }            
                return DBType.None;
            }
        }
        public String DBAddr
        {
            get { return textBox1.Text; }
        }
        public String DBUser
        {
            get { return textBox2.Text; }
        }
        public String DBPass
        {
            get { return textBox3.Text; }
        }
        public String DBName
        {
            get { return textBox4.Text; }
        }
        public String DBTNam
        {
            get { return textBox5.Text; }
        }
    }
    public enum DBType
    {
        None = 0,
        MySql = 1,
        MSSql = 2
    }
}
