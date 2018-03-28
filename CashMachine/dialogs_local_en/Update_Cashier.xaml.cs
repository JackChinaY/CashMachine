using CashMachine.entity_local;
using CashMachine.SQLiteDB;
using System.Windows;
using System.Data.SQLite;
using System;
using System.Windows.Input;
using System.Text.RegularExpressions;

namespace CashMachine.dialogs_local_en
{
    /// <summary>
    /// Update_Cashier.xaml 的交互逻辑
    /// </summary>
    public partial class Update_Cashier : Window
    {
        
        //声明一个变量
        public Cashier cashier = new Cashier();
        //声明一个变量
        public string dataBase;
        //声明一个SQLite数据库
        SQLiteDBHelper sqliteDBHelper = null;
        //无参构造函数
        public Update_Cashier()
        {
            InitializeComponent();
        }
       
        //有参构造函数
        public Update_Cashier(Cashier cashier, string dataBase)
        {
            this.cashier = cashier;//赋值
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
            textBlock2_3_2.Text = this.cashier.Number.ToString();//编号
            textBox2_3_1.Text = this.cashier.Code;//代码
            textBox2_3_2.Text = this.cashier.Name;//姓名
            textBox2_3_3.Password = this.cashier.Password;
            textBox2_3_4.Password = this.cashier.Password;
        }
        /// <summary>
        /// 提交保存数据
        /// </summary>
        private void button2_3_1_Click(object sender, RoutedEventArgs e)
        {
            //判断输入框中内容
            if (textBox2_3_2.Text.Equals(""))
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
                cashier.Name = textBox2_3_2.Text;
                cashier.Password = textBox2_3_3.Password;
                //SQLite数据库，此处连接的是programmingDB.db
                sqliteDBHelper = new SQLiteDBHelper(dataBase);
                //SQL语句
                string sql = "UPDATE Cashier_Table SET Name=@Name,Password=@Password WHERE Code=@Code";
                //配置SQL语句里的参数
                SQLiteParameter[] parameters = {
                    new SQLiteParameter("@Name",cashier.Name),
                    new SQLiteParameter("@Password",cashier.Password),
                    new SQLiteParameter("@Code",this.cashier.Code),
                };
                //执行SQL
                try
                {
                    //执行成功时
                    if (sqliteDBHelper.ExecuteNonQuery(sql, parameters) == 1)
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
