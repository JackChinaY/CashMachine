using CashMachine.entity_local;
using CashMachine.SQLiteDB;
using System;
using System.Windows;
using System.Data.SQLite;
using System.Data;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace CashMachine.dialogs_local_en
{
    /// <summary>
    /// Update_Buyer.xaml 的交互逻辑
    /// </summary>
    public partial class Update_Buyer : Window
    {
        //声明一个变量
        public string dataBase;
        //声明一个SQLite数据库
        SQLiteDBHelper sqliteDBHelper = null;
        Buyer buyer = new Buyer();
        //无参构造函数
        public Update_Buyer()
        {
            InitializeComponent();
        }

        //有参构造函数
        public Update_Buyer(string dataBase, Buyer buyer)
        {
            this.dataBase = dataBase;//赋值
            this.buyer = buyer;//赋值
            InitializeComponent();//初始化窗体组件
            this.ResizeMode = ResizeMode.CanMinimize;//禁用“最大化”按钮
            Init();//初始化窗体里的数据
            sqliteDBHelper = new SQLiteDBHelper(dataBase);//SQLite数据库
        }

        /// <summary>
        /// 窗体数据初始化
        /// </summary>
        public void Init()
        {
            textBlock1.Text = buyer.Number;
            textBox1.Text = buyer.Name;
            textBox2.Text = buyer.BPN;
            textBox5.Text = buyer.VAT;
            textBox3.Text = buyer.Address;
            textBox4.Text = buyer.Tel;
        }
        /// <summary>
        /// 当输入完TPIN后就开始验证其唯一性
        /// </summary>
        private void textBox2_LostFocus(object sender, RoutedEventArgs e)
        {
            //固定10位
            if (textBox2.Text.Length == 10)
            {
                //SQL语句
                string sql = "SELECT COUNT(*) AS COUNTS FROM Buyer_Info WHERE BPN=@BPN";
                //配置SQL语句里的参数
                SQLiteParameter[] parameter = {
                    new SQLiteParameter("@BPN",textBox2.Text),
                };
                //执行查询，结果为DataTable类型
                DataTable dt = sqliteDBHelper.ExecuteDataTable(sql, parameter);
                //判断结果
                if (Convert.ToInt32(dt.Rows[0]["COUNTS"]) == 0 || textBox2.Text == this.buyer.BPN)//若数量为0
                {
                    textBlock2.Text = "available";
                }
                else
                {
                    textBlock2.Text = "already exists";
                }
            }
        }
        /// <summary>
        /// TPIN 只允许输入0-9数字
        /// </summary>
        private void textBox2_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textbox = sender as TextBox;
            //正则判断
            Regex regex = new Regex(@"^([0-9]{0,})$");//需要加上@
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
        /// 客户联系电话 只允许输入 0-9 空格 ( ) + -
        /// </summary>
        private void textBox4_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textbox = sender as TextBox;
            //正则判断
            Regex regex = new Regex(@"^([\\(,\\),\+,\-,\s,0-9]{0,18})$");//需要加上@
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
        /// 提交保存数据
        /// </summary>
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            //判断输入框内容是否为空
            if (textBox1.Text.Equals(""))
            {
                MessageBox.Show("The Name is required!", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (textBox1.Text.Trim().Equals(""))
            {
                MessageBox.Show("The Name can not be all spaces!", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (textBox2.Text.Equals(""))
            {
                MessageBox.Show("The TPIN is required!", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (textBox2.Text.Length != 10)
            {
                MessageBox.Show("The length of the TPIN is fixed 10 numbers!", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (textBlock2.Text.Equals("already exists"))
            {
                MessageBox.Show("The TPIN already exists in the database. Please re-enter it!", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (textBox5.Text.Equals(""))
            {
                MessageBox.Show("The TAX ACC Name is required!", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (textBox5.Text.Trim().Equals(""))
            {
                MessageBox.Show("The TAX ACC Name can not be all spaces!", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (textBox4.Text.Length != 0 && textBox4.Text.Trim().Equals(""))
            {
                MessageBox.Show("The Tel can not be all spaces!", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (textBox3.Text.Length != 0 && textBox3.Text.Trim().Equals(""))
            {
                MessageBox.Show("The Address can not be all spaces!", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            //else if (textBlock3.Text.Equals("syntax error"))
            //{
            //    MessageBox.Show("Syntax error：The characters of the Tel allowed only include 0-9 spaces () + - , and it can not be all spaces!", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
            //    return;
            //}
            else
            {
                //声明一个变量
                Buyer buyer = new Buyer();
                //变量赋值
                buyer.Number = textBlock1.Text;
                buyer.Name = textBox1.Text;
                buyer.BPN = textBox2.Text;
                buyer.VAT = textBox5.Text;
                buyer.Address = textBox3.Text;
                buyer.Tel = textBox4.Text;
                //SQL语句
                string sql = "UPDATE Buyer_Info SET Name=@Name,BPN=@BPN,VAT=@VAT,Address=@Address,"
                    + "Tel=@Tel WHERE Number=@Number";
                //配置SQL语句里的参数
                SQLiteParameter[] parameters = {
                    new SQLiteParameter("@Number",buyer.Number),
                    new SQLiteParameter("@Name",buyer.Name),
                    new SQLiteParameter("@BPN",buyer.BPN),
                    new SQLiteParameter("@VAT",buyer.VAT),
                    new SQLiteParameter("@Address",buyer.Address),
                    new SQLiteParameter("@Tel",buyer.Tel),
                };
                //SQLite数据库
                //sqliteDBHelper = new SQLiteDBHelper(dataBase);
                //执行SQL
                try
                {
                    if (sqliteDBHelper.ExecuteNonQuery(sql, parameters) == 1)//执行成功时
                    {
                        //弹出提示框
                        MessageBox.Show("Submit successfully!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                        //关闭添加框
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
