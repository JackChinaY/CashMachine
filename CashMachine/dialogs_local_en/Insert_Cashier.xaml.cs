using CashMachine.entity_local;
using CashMachine.SQLiteDB;
using System;
using System.Windows;
using System.Data.SQLite;
using System.Data;
using System.Windows.Input;
using System.Windows.Controls;
using System.Text.RegularExpressions;

namespace CashMachine.dialogs_local_en
{
    /// <summary>
    /// Insert_Cashier.xaml 的交互逻辑
    /// </summary>
    public partial class Insert_Cashier : Window
    {
        //声明一个变量
        public Cashier cashier = new Cashier();
        //声明一个变量
        public string dataBase;
        //声明一个SQLite数据库
        SQLiteDBHelper sqliteDBHelper = null;

        //无参构造函数
        public Insert_Cashier()
        {
            InitializeComponent();
        }

        //有参构造函数
        public Insert_Cashier(string dataBase)
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
            string sql_Number = "SELECT MAX(Number) AS MAXNUM FROM Cashier_Table";
            //执行查询，结果为DataTable类型
            DataTable dt_Number = sqliteDBHelper.ExecuteDataTable(sql_Number, null);
            //判断结果
            if (dt_Number.Rows[0]["MAXNUM"].ToString() == "" || dt_Number.Rows[0]["MAXNUM"] == null)//若为空表，则序号从1开始
            {

                textBlock2_3_2.Text = "1";
            }
            else
            {
                textBlock2_3_2.Text = (Convert.ToInt32(dt_Number.Rows[0]["MAXNUM"].ToString()) + 1).ToString();//最大值加1
            }
            //设置收银员代码 自动生成的
            //SQL语句
            string sql = "SELECT MAX(Code) AS MAXNUM FROM Cashier_Table";
            //执行查询，结果为DataTable类型
            DataTable dt = sqliteDBHelper.ExecuteDataTable(sql, null);
            //判断结果
            if (dt.Rows[0]["MAXNUM"].ToString() == "" || dt.Rows[0]["MAXNUM"] == null)//若为空表，则序号从1开始
            {

                textBox2_3_1.Text = "001";
            }
            else
            {
                //将数字转固定长度的字符串 比如2变成002  用0填充
                textBox2_3_1.Text = String.Format("{0:000}", Convert.ToInt32(dt.Rows[0]["MAXNUM"].ToString()) + 1); //(Convert.ToInt32(dt.Rows[0]["MAXNUM"].ToString()) + 1).ToString();//最大值加1
            }
            textBlock2_3_1.Text = "available";
        }
        /// <summary>
        /// Code 只允许输入0-9数字 backspace
        /// </summary>
        public void textBox2_3_1_TextChanged(object sender, TextChangedEventArgs e)
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
        //private void textBox2_3_1_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.Key == Key.Back || e.Key == Key.D0 || e.Key == Key.D1 || e.Key == Key.D2 || e.Key == Key.D3 || e.Key == Key.D4 || e.Key == Key.D5 || e.Key == Key.D6 || e.Key == Key.D7 || e.Key == Key.D8 || e.Key == Key.D9 || e.Key == Key.NumPad0 || e.Key == Key.NumPad1 || e.Key == Key.NumPad2 || e.Key == Key.NumPad3 || e.Key == Key.NumPad4 || e.Key == Key.NumPad5 || e.Key == Key.NumPad6 || e.Key == Key.NumPad7 || e.Key == Key.NumPad8 || e.Key == Key.NumPad9)
        //    {
        //        e.Handled = false;//可以接受该事件
        //    }
        //    else
        //    {
        //        e.Handled = true;//为true时表示已经处理了事件（即不处理当前键盘事件  不接受）
        //    }
        //}
        /// <summary>
        /// 当输入收银员代码后就开始验证其正确性和唯一性
        /// </summary>
        public void textBox2_3_1_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!textBox2_3_1.Text.Equals(""))
            {
                if (textBox2_3_1.Text.Length != 3)
                {
                    //弹出提示框
                    MessageBox.Show("The length of the Code is fixed 3 numbers！", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else if (textBox2_3_1.Text.Equals("000"))
                {
                    //弹出提示框
                    MessageBox.Show("The Code can not be the '000'！", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                    textBlock2_3_1.Text = "unavailable";
                }
                else
                {
                    //SQL语句
                    string sql = "SELECT COUNT(*) AS COUNTS FROM Cashier_Table WHERE Code=@Code";
                    //配置SQL语句里的参数
                    SQLiteParameter[] parameter = {
                    new SQLiteParameter("@Code",textBox2_3_1.Text),
                    };
                    //执行查询，结果为DataTable类型
                    DataTable dt = sqliteDBHelper.ExecuteDataTable(sql, parameter);
                    //判断结果
                    if (Convert.ToInt32(dt.Rows[0]["COUNTS"]) == 0)//若数量为0
                    {
                        textBlock2_3_1.Text = "available";
                    }
                    else
                    {
                        textBlock2_3_1.Text = "already exists";
                    }
                }
            }
        }
        /// <summary>
        /// 提交保存数据
        /// </summary>
        public void button2_3_1_Click(object sender, RoutedEventArgs e)
        {
            //判断输入框内容是否为空
            if (textBox2_3_1.Text.Equals(""))
            {
                MessageBox.Show("The Code is required!", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (textBlock2_3_1.Text.Equals("already exists"))
            {
                MessageBox.Show("The Code already existed in the database. Please re-enter it!", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (textBlock2_3_1.Text.Equals("unavailable"))
            {
                MessageBox.Show("The Code can not be the '000'!", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (textBox2_3_2.Text.Equals(""))
            {
                MessageBox.Show("The Name is required!", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (textBox2_3_2.Text.Trim().Equals(""))
            {
                MessageBox.Show("The Name can not be all spaces!", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (textBox2_3_3.Password.Equals(""))
            {
                MessageBox.Show("Password is required!", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (textBox2_3_3.Password.Length != 6)
            {
                MessageBox.Show("The length of the Password must be 6 numbers!", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (textBox2_3_3.Password.Contains(" "))
            {
                MessageBox.Show("The Password can not contain the space, it must be the number 0-9 !", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (textBox2_3_4.Password.Equals(""))
            {
                MessageBox.Show("Confirm Password is required!", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (textBox2_3_4.Password.Length != 6)
            {
                MessageBox.Show("The length of the Confirm Password must be 6 numbers!", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (textBox2_3_4.Password.Contains(" "))
            {
                MessageBox.Show("The Confirm Password can not contain the space, it must be the number 0-9 .!", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (!textBox2_3_3.Password.Equals(textBox2_3_4.Password))
            {
                MessageBox.Show("The Password and Confirm Password are inconsistent!", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            //所有条件都满足时
            else
            {
                Regex regex = new Regex(@"^([0-9]{6,6})$");//需要加上@
                if (regex.Matches(textBox2_3_3.Password).Count > 0)
                {
                }
                //如果不匹配
                else
                {
                    MessageBox.Show("The Password contains illegal characters, it must be the number 0-9 .", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (regex.Matches(textBox2_3_4.Password).Count > 0)
                {
                }
                //如果不匹配
                else
                {
                    MessageBox.Show("The Confirm Password contains illegal characters, it must be the number 0-9 .", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                //声明一个变量
                Cashier cashier = new Cashier();
                //变量赋值
                cashier.Number = Convert.ToInt32(textBlock2_3_2.Text);
                cashier.Code = textBox2_3_1.Text;
                cashier.Name = textBox2_3_2.Text;
                cashier.Password = textBox2_3_3.Password;
                cashier.Flag = 1;
                //SQL语句
                string sql = "INSERT INTO Cashier_Table (Number,Name,Code,Password,Flag) VALUES(@Number,@Name,@Code,@Password,@Flag)";
                //配置SQL查询语句里的参数
                SQLiteParameter[] parameters = {
                    new SQLiteParameter("@Number",cashier.Number),
                    new SQLiteParameter("@Name",cashier.Name),
                    new SQLiteParameter("@Code",cashier.Code),
                    new SQLiteParameter("@Password",cashier.Password),
                    new SQLiteParameter("@Flag",cashier.Flag)
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
