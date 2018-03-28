using CashMachine.entity_local;
using CashMachine.SQLiteDB;
using System;
using System.Data.SQLite;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace CashMachine.dialogs_local_en
{
    /// <summary>
    /// Dialog_Cashier.xaml 的交互逻辑
    /// </summary>
    public partial class Update_Tax : Window
    {
        //声明一个变量
        public Tax tax = new Tax();
        //声明一个变量
        public string dataBase;
        //声明一个SQLite数据库
        SQLiteDBHelper sqliteDBHelper = null;

        //无参构造函数
        public Update_Tax()
        {
            InitializeComponent();
        }

        //有参构造函数
        public Update_Tax(Tax tax, string dataBase)
        {
            this.tax = tax;//赋值
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
            textBox1.Text = this.tax.Number;
            textBox2.Text = this.tax.Code;
            textBox3.Text = this.tax.Name;
            textBox4.Text = (Convert.ToDouble(this.tax.Rate) / 10000).ToString();

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
            if (textBox3.Text.Equals(""))
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
                string sql = "UPDATE Tax_Tariff SET Tax_Name=@Tax_Name,Tax_Rate=@Tax_Rate WHERE Number=@Number";
                //配置SQL查询语句里的参数
                SQLiteParameter[] parameters = {
                    new SQLiteParameter("@Number",tax.Number),
                    new SQLiteParameter("@Tax_Name",tax.Name),
                    new SQLiteParameter("@Tax_Rate",tax.Rate)
                };
                //SQLite数据库
                sqliteDBHelper = new SQLiteDBHelper(dataBase);
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
