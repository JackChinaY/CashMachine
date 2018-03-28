using CashMachine.entity_local;
using CashMachine.SQLiteDB;
using System;
using System.Data;
using System.Data.SQLite;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace CashMachine.dialogs_local_en
{
    /// <summary>
    /// Insert_Tax.xaml 的交互逻辑
    /// </summary>
    public partial class Insert_Tax : Window
    {
        //声明一个变量
        public Tax tax = new Tax();
        //声明一个变量
        public string dataBase;
        //声明一个SQLite数据库
        SQLiteDBHelper sqliteDBHelper = null;

        //无参构造函数
        public Insert_Tax()
        {
            InitializeComponent();
        }

        //有参构造函数
        public Insert_Tax(string dataBase)
        {
            this.dataBase = dataBase;//赋值
            InitializeComponent();//初始化窗体组件
            this.ResizeMode = ResizeMode.CanMinimize;//禁用“最大化”按钮
            Init();//初始化窗体里的数据
        }
        /// <summary>
        /// 窗体数据初始化
        /// </summary>
        public void Init()
        {
            //SQLite数据库
            sqliteDBHelper = new SQLiteDBHelper(dataBase);
            //设置编号 自动生成的
            //SQL语句
            string sql_Number = "SELECT MAX(Number) AS MAXNUM FROM Tax_Tariff";
            //执行查询，结果为DataTable类型
            DataTable dt_Number = sqliteDBHelper.ExecuteDataTable(sql_Number, null);
            //判断结果
            if (dt_Number.Rows[0]["MAXNUM"].ToString() == "" || dt_Number.Rows[0]["MAXNUM"] == null)//若为空表，则序号从1开始
            {

                textBox1.Text = "1";
            }
            else
            {
                textBox1.Text = (Convert.ToInt32(dt_Number.Rows[0]["MAXNUM"].ToString()) + 1).ToString();//最大值加1
            }
        }

        /// <summary>
        /// 当输入Tax Code后就开始验证其唯一性
        /// </summary>
        private void textBox2_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!textBox2.Text.Equals(""))
            {
                //SQL语句
                string sql = "SELECT COUNT(*) AS COUNTS FROM Tax_Tariff WHERE Tax_Code=@Tax_Code";
                //配置SQL语句里的参数
                SQLiteParameter[] parameter = {
                    new SQLiteParameter("@Tax_Code",textBox2.Text),
                    };
                //执行查询，结果为DataTable类型
                DataTable dt = sqliteDBHelper.ExecuteDataTable(sql, parameter);
                //判断结果
                if (Convert.ToInt32(dt.Rows[0]["COUNTS"]) == 0)//若数量为0
                {
                    code_flag1.Text = "available";
                }
                else
                {
                    code_flag1.Text = "already exists";
                }
            }
        }
        /// <summary>
        /// Tax Code 只允许输入0-9a-zA-Z
        /// </summary>
        private void textBox2_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textbox = sender as TextBox;
            //正则判断
            Regex regex = new Regex(@"^([0-9a-zA-Z]{0,})$");//需要加上@
            if (regex.Matches(textbox.Text).Count > 0)
            {
            }
            //如果不匹配
            else
            {
                //如果长度>=1则删除刚刚输入的一个字符，如果长度为0，按删除键会报错
                if (textbox.Text.Length >= 1)
                {
                    textbox.Text = textbox.Text.Remove(textbox.Text.Length - 1, 1);
                    textbox.SelectionStart = textbox.Text.Length;
                }
            }
        }
        /// <summary>
        /// Tax Name 只允许输入0-9a-zA-Z,\\(,\\),\+,\-,\s
        /// </summary>
        private void textBox3_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textbox = sender as TextBox;
            //正则判断
            Regex regex = new Regex(@"^([0-9a-zA-Z,\\(,\\),\+,\-,\s]{0,})$");//需要加上@
            if (regex.Matches(textbox.Text).Count > 0)
            {
            }
            //如果不匹配
            else
            {
                //如果长度>=1则删除刚刚输入的一个字符，如果长度为0，按删除键会报错
                if (textbox.Text.Length >= 1)
                {
                    textbox.Text = textbox.Text.Remove(textbox.Text.Length - 1, 1);
                    textbox.SelectionStart = textbox.Text.Length;
                }
            }
        }
        /// <summary>
        /// Tax Rate 只允许输入0-9 .
        /// </summary>
        private void textBox4_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textbox = sender as TextBox;
            //正则判断
            Regex regex = new Regex(@"^(([0]{1,1}([\.]{0,1})([0-9]{0,9}))|([1-9]{1,1})([0-9]{0,8})([\.]{0,1})([0-9]{0,9}))$");//需要加上@
            if (regex.Matches(textbox.Text).Count > 0)
            {
                //如果误输入00，则换成0
                if (textbox.Text.Equals("00") || textbox.Text.Equals("01") || textbox.Text.Equals("02") || textbox.Text.Equals("03") || textbox.Text.Equals("04") || textbox.Text.Equals("05") || textbox.Text.Equals("06") || textbox.Text.Equals("07") || textbox.Text.Equals("08") || textbox.Text.Equals("09"))
                {
                    textbox.Text = textbox.Text.Remove(0, 1);
                    textbox.SelectionStart = textbox.Text.Length;
                }
            }
            //如果不匹配
            else
            {
                //如果长度>=1则删除刚刚输入的一个字符，如果长度为0，按删除键会报错
                if (textbox.Text.Length >= 1)
                {
                    textbox.Text = textbox.Text.Remove(textbox.Text.Length - 1, 1);
                    textbox.SelectionStart = textbox.Text.Length;
                }
            }
        }

        /// <summary>
        /// 提交保存数据
        /// </summary>
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            //判断输入框内容是否为空
            if (textBox2.Text.Equals(""))
            {
                MessageBox.Show("The Tax Code is required!", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (code_flag1.Text.Equals("already exists"))
            {
                MessageBox.Show("The Tax Code already existed in the database. Please re-enter it!", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (textBox3.Text.Equals(""))
            {
                MessageBox.Show("The Tax Name is required!", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (textBox3.Text.Trim().Equals(""))
            {
                MessageBox.Show("The Tax Name can not be all spaces!", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (textBox4.Text.Equals(""))
            {
                MessageBox.Show("The Tax Rate is required!", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            //所有条件都满足时
            else
            {
                //声明一个变量
                Tax tax = new Tax();
                //变量赋值
                tax.Number = textBox1.Text;
                tax.Code = textBox2.Text;
                tax.Name = textBox3.Text;
                tax.Rate = Math.Round(Convert.ToDouble(textBox4.Text) * 10000, 0).ToString();
                tax.Invoice_Name = "";
                tax.Invoice_Code = "";
                tax.Exempt_Flag = "0";
                tax.Invoice_Name = "0";

                //SQL语句
                string sql = "INSERT INTO Tax_Tariff (Number,Invoice_Code,Invoice_Name,Tax_Code,Tax_Name,Tax_Rate,Exempt_Flag,CRC32) VALUES(@Number,@Invoice_Code,@Invoice_Name,@Tax_Code,@Tax_Name,@Tax_Rate,@Exempt_Flag,@CRC32)";
                //配置SQL查询语句里的参数
                SQLiteParameter[] parameters = {
                    new SQLiteParameter("@Number",tax.Number),
                    new SQLiteParameter("@Invoice_Code",tax.Invoice_Code),
                    new SQLiteParameter("@Invoice_Name",tax.Invoice_Name),
                    new SQLiteParameter("@Tax_Code",tax.Code),
                    new SQLiteParameter("@Tax_Name",tax.Name),
                    new SQLiteParameter("@Tax_Rate",tax.Rate),
                    new SQLiteParameter("@Exempt_Flag",tax.Exempt_Flag),
                    new SQLiteParameter("@CRC32",tax.CRC32)
                };
                //执行SQL
                try
                {
                    if (sqliteDBHelper.ExecuteNonQuery(sql, parameters) == 1)//执行成功时
                    {
                        //弹出提示框
                        MessageBox.Show("Submit successfully!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                        //关闭弹出框
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Submission failure!", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                //执行失败时
                catch (Exception ee)
                {
                    //弹出提示框
                    MessageBox.Show("Submission failure! Possible causes:" + ee.Message, "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}