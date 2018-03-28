using CashMachine.entity_local;
using CashMachine.SQLiteDB;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Data.SQLite;

namespace CashMachine.dialogs_local_en
{
    /// <summary>
    /// Update_Header.xaml的交互逻辑
    /// </summary>
    public partial class Update_Header : Window
    {
        //声明一个变量
        public string dataBase;
        //声明一个SQLite数据库
        SQLiteDBHelper sqliteDBHelper = null;

        //有参构造函数
        public Update_Header(string dataBase)
        {
            this.dataBase = dataBase;//赋值
            InitializeComponent();//初始化窗体组件
            Init();//初始化窗体里的数据
        }
        /// <summary>
        /// 窗体数据初始化
        /// </summary>
        public void Init()
        {
            //SQLite数据库，此处连接的是programmingDB.db
            sqliteDBHelper = new SQLiteDBHelper(dataBase);
            //SQL语句
            string sql_sqlite = "SELECT id,Number,Line,Flag FROM company_info_table order by Number ASC";
            //执行查询，结果为DataTable类型
            DataTable dt = sqliteDBHelper.ExecuteDataTable(sql_sqlite, null);
            if (dt.Rows.Count == 1)
            {
                //窗体数据初始化 输入框
                textBox2_2_1.Text = dt.Rows[0]["Line"].ToString();
                //窗体数据初始化 复选框
                if (dt.Rows[0]["Flag"].ToString() == "1")
                {
                    checkBox2_2_1.IsChecked = true;
                }
            }
            else if (dt.Rows.Count == 2)
            {
                //窗体数据初始化 输入框
                textBox2_2_1.Text = dt.Rows[0]["Line"].ToString();
                textBox2_2_2.Text = dt.Rows[1]["Line"].ToString();
                //窗体数据初始化 复选框
                if (dt.Rows[0]["Flag"].ToString() == "1")
                {
                    checkBox2_2_1.IsChecked = true;
                }
                if (dt.Rows[1]["Flag"].ToString() == "1")
                {
                    checkBox2_2_2.IsChecked = true;
                }
            }
            else if (dt.Rows.Count == 3)
            {
                //窗体数据初始化 输入框
                textBox2_2_1.Text = dt.Rows[0]["Line"].ToString();
                textBox2_2_2.Text = dt.Rows[1]["Line"].ToString();
                textBox2_2_3.Text = dt.Rows[2]["Line"].ToString();
                //窗体数据初始化 复选框
                if (dt.Rows[0]["Flag"].ToString() == "1")
                {
                    checkBox2_2_1.IsChecked = true;
                }
                if (dt.Rows[1]["Flag"].ToString() == "1")
                {
                    checkBox2_2_2.IsChecked = true;
                }
                if (dt.Rows[2]["Flag"].ToString() == "1")
                {
                    checkBox2_2_3.IsChecked = true;
                }
            }
            else if (dt.Rows.Count == 4)
            {
                //窗体数据初始化 输入框
                textBox2_2_1.Text = dt.Rows[0]["Line"].ToString();
                textBox2_2_2.Text = dt.Rows[1]["Line"].ToString();
                textBox2_2_3.Text = dt.Rows[2]["Line"].ToString();
                textBox2_2_4.Text = dt.Rows[3]["Line"].ToString();
                //窗体数据初始化 复选框
                if (dt.Rows[0]["Flag"].ToString() == "1")
                {
                    checkBox2_2_1.IsChecked = true;
                }
                if (dt.Rows[1]["Flag"].ToString() == "1")
                {
                    checkBox2_2_2.IsChecked = true;
                }
                if (dt.Rows[2]["Flag"].ToString() == "1")
                {
                    checkBox2_2_3.IsChecked = true;
                }
                if (dt.Rows[3]["Flag"].ToString() == "1")
                {
                    checkBox2_2_4.IsChecked = true;
                }
            }
            else if (dt.Rows.Count == 5)
            {
                //窗体数据初始化 输入框
                textBox2_2_1.Text = dt.Rows[0]["Line"].ToString();
                textBox2_2_2.Text = dt.Rows[1]["Line"].ToString();
                textBox2_2_3.Text = dt.Rows[2]["Line"].ToString();
                textBox2_2_4.Text = dt.Rows[3]["Line"].ToString();
                textBox2_2_5.Text = dt.Rows[4]["Line"].ToString();
                //窗体数据初始化 复选框
                if (dt.Rows[0]["Flag"].ToString() == "1")
                {
                    checkBox2_2_1.IsChecked = true;
                }
                if (dt.Rows[1]["Flag"].ToString() == "1")
                {
                    checkBox2_2_2.IsChecked = true;
                }
                if (dt.Rows[2]["Flag"].ToString() == "1")
                {
                    checkBox2_2_3.IsChecked = true;
                }
                if (dt.Rows[3]["Flag"].ToString() == "1")
                {
                    checkBox2_2_4.IsChecked = true;
                }
                if (dt.Rows[4]["Flag"].ToString() == "1")
                {
                    checkBox2_2_5.IsChecked = true;
                }
            }
            else if (dt.Rows.Count == 6)
            {
                //窗体数据初始化 输入框
                textBox2_2_1.Text = dt.Rows[0]["Line"].ToString();
                textBox2_2_2.Text = dt.Rows[1]["Line"].ToString();
                textBox2_2_3.Text = dt.Rows[2]["Line"].ToString();
                textBox2_2_4.Text = dt.Rows[3]["Line"].ToString();
                textBox2_2_5.Text = dt.Rows[4]["Line"].ToString();
                textBox2_2_6.Text = dt.Rows[5]["Line"].ToString();
                //窗体数据初始化 复选框
                if (dt.Rows[0]["Flag"].ToString() == "1")
                {
                    checkBox2_2_1.IsChecked = true;
                }
                if (dt.Rows[1]["Flag"].ToString() == "1")
                {
                    checkBox2_2_2.IsChecked = true;
                }
                if (dt.Rows[2]["Flag"].ToString() == "1")
                {
                    checkBox2_2_3.IsChecked = true;
                }
                if (dt.Rows[3]["Flag"].ToString() == "1")
                {
                    checkBox2_2_4.IsChecked = true;
                }
                if (dt.Rows[4]["Flag"].ToString() == "1")
                {
                    checkBox2_2_5.IsChecked = true;
                }
                if (dt.Rows[5]["Flag"].ToString() == "1")
                {
                    checkBox2_2_6.IsChecked = true;
                }
            }
        }

        /// <summary>
        /// 提交保存数据
        /// </summary>
        private void button2_2_1_Click(object sender, RoutedEventArgs e)
        {
            //删除数据
            //SQL语句
            string sql_sqlite_del = "DELETE FROM Company_Info_Table";
            //执行SQL
            sqliteDBHelper.ExecuteNonQuery(sql_sqlite_del, null);

            //SQL插入语句

            #region 将页面中的输入值采集到一个集合中 不管页面中输入几条，数据库中都保存6条
            //计数器,用于id和number自增
            int temp = 1;
            //声明一个集合
            List<HeaderOfInvoice> headerList = new List<HeaderOfInvoice>();
            //发票抬头
            {
                HeaderOfInvoice header = new HeaderOfInvoice();
                header.Id = temp;
                header.Number = temp;
                temp++;
                header.Line = textBox2_2_1.Text;
                if (checkBox2_2_1.IsChecked == true) { header.Flag = 1; } else { header.Flag = 0; };
                headerList.Add(header);
            }
            //税号
            {
                HeaderOfInvoice header = new HeaderOfInvoice();
                header.Id = temp;
                header.Number = temp;
                temp++;
                header.Line = textBox2_2_2.Text;
                if (checkBox2_2_2.IsChecked == true) { header.Flag = 1; } else { header.Flag = 0; };
                headerList.Add(header);
            }
            //地址
            {
                HeaderOfInvoice header = new HeaderOfInvoice();
                header.Id = temp;
                header.Number = temp;
                temp++;
                header.Line = textBox2_2_3.Text;
                if (checkBox2_2_3.IsChecked == true) { header.Flag = 1; } else { header.Flag = 0; };
                headerList.Add(header);
            }
            //电话
            {
                HeaderOfInvoice header = new HeaderOfInvoice();
                header.Id = temp;
                header.Number = temp;
                temp++;
                header.Line = textBox2_2_4.Text;
                if (checkBox2_2_4.IsChecked == true) { header.Flag = 1; } else { header.Flag = 0; };
                headerList.Add(header);
            }
            //开户行
            {
                HeaderOfInvoice header = new HeaderOfInvoice();
                header.Id = temp;
                header.Number = temp;
                temp++;
                header.Line = textBox2_2_5.Text;
                if (checkBox2_2_5.IsChecked == true) { header.Flag = 1; } else { header.Flag = 0; };
                headerList.Add(header);
            }
            //银行账号
            {
                HeaderOfInvoice header = new HeaderOfInvoice();
                header.Id = temp;
                header.Number = temp;
                temp++;
                header.Line = textBox2_2_6.Text;
                if (checkBox2_2_6.IsChecked == true) { header.Flag = 1; } else { header.Flag = 0; };
                headerList.Add(header);
            }
            #endregion

            //SQL语句  插入
            string sql_sqlite_ins = "INSERT INTO Company_Info_Table (id,Number,Line,Flag) VALUES(@id,@Number,@Line,@Flag)";
            //声明一个集合
            List<SQLiteParameter[]> parametersList = new List<SQLiteParameter[]>();
            //循环插入多条数据到本地数据库
            for (int j = 0; j < headerList.Count; j++)
            {
                //配置SQL语句里的参数
                SQLiteParameter[] parameters = {
                            new SQLiteParameter("@id",headerList[j].Id),
                            new SQLiteParameter("@Number",headerList[j].Number),
                            new SQLiteParameter("@Line",headerList[j].Line),
                            new SQLiteParameter("@Flag",headerList[j].Flag),
                        };
                parametersList.Add(parameters);
            }
            //执行SQL，并判断成功与否
            try
            {
                if (sqliteDBHelper.ExecuteNonQueryList(sql_sqlite_ins, parametersList) == 6)//执行成功时
                {
                    //弹出提示框
                    MessageBox.Show("数据提交至本地数据库成功!", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    //关闭添加框
                    this.Close();
                }
                else
                {
                    MessageBox.Show("数据提交失败!", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            //执行失败时
            catch (Exception ee)
            {
                //弹出提示框
                MessageBox.Show("数据提交失败!可能原因：" + ee.Message, "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
