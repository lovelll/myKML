using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.OleDb;
using System.IO;
using ADOX;


namespace myAccessKML
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        string sPath;
        string[] fPathFirst;
        string[] fPathLast;
        string[] KMLPath;
        
        DataTable[] dt;
        string[] sName;

        private void btSelectfolder_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.Description = "请选择文件夹";
            if(folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                sPath = folderBrowserDialog1.SelectedPath;
                textBox1.Text = sPath;
                if(sPath.Length > 0)
                {
                    int count = Directory.GetFiles(sPath, "*.mdb", SearchOption.AllDirectories).Length;
                    //fPathFirst = new string[count];


                    fPathFirst = Directory.GetFiles(sPath, "*.mdb", SearchOption.AllDirectories);
                    foreach (string path in fPathFirst)
                    {
                        ListViewItem item = new ListViewItem() { Text = path };
                        listView1.Items.Add(item);
                    }
                }
            }

        }


         // 读取mdb数据   

        public static DataTable ReadData(OleDbConnection dbconn)
        {
            OleDbCommand dbCommand = dbconn.CreateCommand();
            DataTable dt = new DataTable();
            DataRow dr;

            dbCommand.CommandText = "select * from Data ";
            //建立读取 
            OleDbDataReader dbReader = dbCommand.ExecuteReader();
            //查询数据
            int size = dbReader.FieldCount;
            //提取列名
            for(int i = 0;i < size;i++)
            {
                DataColumn dc;

                dc = new DataColumn(dbReader.GetName(i));

                dt.Columns.Add(dc);
                
            }
            //提取每行的数据
            while (dbReader.Read())
            {
                dr = dt.NewRow();

                for(int i = 0;i < size;i++)
                {
                    dr[dbReader.GetName(i)] = dbReader[dbReader.GetName(i)].ToString();

                }
                dt.Rows.Add(dr);

            }
            dbReader.Close();
            
           
            return dt;
        }

        private void btCreateKML_Click(object sender, EventArgs e)
        {
            
            int count = listView1.Items.Count;
            KMLPath = new string[count];
            sName = new string[count];
            dt = new DataTable[count];
            fPathLast = new string[count];

            //最终选取的路径
            for (int i = 0; i < count; i++)
            {
                fPathLast[i] = listView1.Items[i].Text;
                textBox1.Text = fPathLast[i];
            }

            //获取除去文件名的路径 Split()方法分割字符串
            for (int j = 0; j < count; j++)
            {
                string[] str = fPathLast[j].Split('\\');
                string p = "";
                for (int i = 0; i < str.Length - 1; i++)
                {
                    p += str[i] + "\\";
                }
                KMLPath[j] = p;
                sName[j] = str[str.Length - 2];

            }

            //连接数据库并读数据
            for (int j = 0; j < count; j++)
            {
                OleDbConnection dbconn = new OleDbConnection();
                string connectionStr = "Provider=Microsoft.Jet.OLEDB.4.0;Data source= " + fPathLast[j];
                //连接数据库并将数据库数据导入DataTable
                try
                {
                    dbconn.ConnectionString = connectionStr;
                    dbconn.Open();

                    dt[j] = ReadData(dbconn);
                    //MessageBox.Show("数据库导入成功！");
                    dbconn.Close();
                    dbconn.Dispose();

                }
                catch (OleDbException ee)
                {
                    MessageBox.Show(fPathLast[j] + "数据库连接不成功!");
                    dbconn.Close();
                    dbconn.Dispose();
                }
            }

            //生成KML
            for (int j = 0; j < count; j++)
            {
                CreateKML.CreateXML(dt[j], KMLPath[j] + "\\" + sName[j] + ".kml");               

            }
            MessageBox.Show(count + "个KML生成成功！在" + sPath + "对应的子文件下");
            this.Close();
        }

        private void DataCombine()
        {
            ADOX.CatalogClass cat = new ADOX.CatalogClass();
            cat.Create("Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=D:\\NewMDB.mdb;" + "Jet OLEDB:Engine Type=5");

        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                listView1.Items.Remove(item);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //DataCombine();
        }

    }
}
