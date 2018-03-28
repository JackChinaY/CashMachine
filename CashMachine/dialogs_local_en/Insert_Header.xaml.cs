using CashMachine.entity_local;
using CashMachine.SQLiteDB;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Windows;

namespace CashMachine.dialogs_local_en
{
    /// <summary>
    /// Insert_Header.xaml的交互逻辑
    /// </summary>
    public partial class Insert_Header : Window
    {
        //声明一个变量
        public string dataBase;
        //声明一个SQLite数据库
        SQLiteDBHelper sqliteDBHelper = null;
        //无参构造函数
        public Insert_Header()
        {
            InitializeComponent();
        }

        //有参构造函数
        public Insert_Header(string dataBase)
        {
            this.dataBase = dataBase;//赋值
            InitializeComponent();//初始化窗体组件
            this.ResizeMode = ResizeMode.CanMinimize;//只能最小化和还原窗口。 同时显示“最小化”和“最大化”按钮，但只有“最小化”按钮处于启用状态
        }

        /// <summary>
        /// 提交保存数据
        /// </summary>
        private void button2_2_1_Click(object sender, RoutedEventArgs e)
        {
            //SQLite数据库，此处连接的是programmingDB.db
            sqliteDBHelper = new SQLiteDBHelper(dataBase);
            //删除数据
            //SQL语句
            string sql_sqlite_del = "DELETE FROM Company_Info_Table";
            //执行SQL
            sqliteDBHelper.ExecuteNonQuery(sql_sqlite_del,null);

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

            //SQL语句
            string sql_sqlite_ins = "INSERT INTO Company_Info_Table (id,Number,Line,Flag) values(@id,@Number,@Line,@Flag)";
            //受影响行数
            int p = 0;
            //配置SQL语句里的参数
            SQLiteParameter[] parameters = {
                        new SQLiteParameter("@id"),
                        new SQLiteParameter("@Number"),
                        new SQLiteParameter("@Line"),
                        new SQLiteParameter("@Flag")
            };
            //循环插入多条数据到本地数据库
            for (int j = 0; j < headerList.Count; j++)
            {
                parameters[0].Value = headerList[j].Id;
                parameters[1].Value = headerList[j].Number;
                parameters[2].Value = headerList[j].Line;
                parameters[3].Value = headerList[j].Flag;
                p += sqliteDBHelper.ExecuteNonQuery(sql_sqlite_ins, parameters);
            }
            //执行SQL，并判断成功与否
            try
            {
                //执行成功时
                if (p == 6)
                {
                    //弹出提示框
                    MessageBox.Show("数据提交至本地数据库成功!", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    //关闭弹出框
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
