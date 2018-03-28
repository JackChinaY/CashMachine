using MySql.Data.MySqlClient;
using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using System.Security.Cryptography;
using System.Text;
using System.Data;
using CheckProject.MysqlDB;
using System;
using CheckProject.utils;

namespace CheckProject
{
    /// <summary>
    /// Login.xaml 的交互逻辑
    /// </summary>
    public partial class Login : Window
    {
        public static string userName;
        public static string userId;

        public Login()
        {
            InitializeComponent();
            this.ResizeMode = ResizeMode.CanMinimize;//禁用“最大化”按钮
        }
        /// <summary>
        /// 登录到本地
        /// </summary>
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if (comboBox2.SelectionBoxItem.ToString().Equals("院办事员"))
            {
                MainWindow_Local mainWindow = new MainWindow_Local(userId, userName);
                mainWindow.Show();
                this.Close();
            }

            //if (textBox1.Text.Equals("") || textBox2.Password.Equals(""))
            //{
            //    MessageBox.Show("用户名和密码不可为空!", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            //    return;
            //}
            //string sql_mysql = "select Id,Username from user_table where Username=@Username and Password=@Password";
            ////string password = textBox2.Password;
            ////对密码进行加密
            //string MD5password = CommonUtils.getMD5Str(textBox2.Password);
            ////System.Console.WriteLine(MD5password);
            //MySqlParameter[] parameters = {
            //        new MySqlParameter("@Username",textBox1.Text),
            //        new MySqlParameter("@Password",MD5password)
            //};
            ////声明一个Mysql数据库，连接的是云端的数据库
            //MysqlDBHelper mysqlDBHelper = new MysqlDBHelper();
            //try
            //{
            //    //查询的结果
            //    DataTable dt = mysqlDBHelper.ExecuteDataTable(sql_mysql, parameters);
            //    //判断结果
            //    if (dt.Rows.Count == 0)
            //    {
            //        MessageBox.Show("用户名或密码错误!", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            //        return;
            //    }
            //    //查询成功
            //    if (dt.Rows.Count == 1)
            //    {
            //        string userId = (string)dt.Rows[0]["Id"];
            //        string userName = (string)dt.Rows[0]["Username"];
            //        //System.Console.WriteLine(dt.Rows.Count+"  "+userId + "  " +userName);
            //        MainWindow mainWindow = new MainWindow(userId, userName);
            //        mainWindow.Show();
            //        this.Close();
            //        //MessageBox.Show("登录成功!", "提示", MessageBoxButton.OK, MessageBoxImage.Information);

            //    }
            //}
            ////执行失败时
            //catch (Exception ee)
            //{
            //    //弹出提示框
            //    MessageBox.Show("数据提交失败!可能原因：" + ee.Message, "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            //}
        }
        /// <summary>
        /// 登录到服务器
        /// </summary>
        private void button2_Click(object sender, RoutedEventArgs e)
        {

            //if (textBox1.Text.Equals("") || textBox2.Password.Equals(""))
            //{
            //    MessageBox.Show("用户名和密码不可为空!", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            //    return;
            //}
            //string sql_mysql = "select Id,Username from user_table where Username=@Username and Password=@Password";
            ////string password = textBox2.Password;
            ////对密码进行加密
            //string MD5password = CommonUtils.getMD5Str(textBox2.Password);
            ////System.Console.WriteLine(MD5password);
            //MySqlParameter[] parameters = {
            //        new MySqlParameter("@Username",textBox1.Text),
            //        new MySqlParameter("@Password",MD5password)
            //};
            ////声明一个Mysql数据库，连接的是云端的数据库
            //MysqlDBHelper mysqlDBHelper = new MysqlDBHelper();
            //try
            //{
            //    //查询的结果
            //    DataTable dt = mysqlDBHelper.ExecuteDataTable(sql_mysql, parameters);
            //    //判断结果
            //    if (dt.Rows.Count == 0)
            //    {
            //        MessageBox.Show("用户名或密码错误!", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            //        return;
            //    }
            //    //查询成功
            //    if (dt.Rows.Count == 1)
            //    {
            //        string userId = (string)dt.Rows[0]["Id"];
            //        string userName = (string)dt.Rows[0]["Username"];
            //        //System.Console.WriteLine(dt.Rows.Count+"  "+userId + "  " +userName);
            //        MainWindow mainWindow = new MainWindow(userId, userName);
            //        mainWindow.Show();
            //        this.Close();
            //        //MessageBox.Show("登录成功!", "提示", MessageBoxButton.OK, MessageBoxImage.Information);

            //    }
            //}
            ////执行失败时
            //catch (Exception ee)
            //{
            //    //弹出提示框
            //    MessageBox.Show("数据提交失败!可能原因：" + ee.Message, "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            //}
            if (comboBox4.SelectionBoxItem.ToString().Equals("校管理员"))
            {
                MainWindow_Online_xgly mainWindow = new MainWindow_Online_xgly(userId, userName);
                mainWindow.Show();
                this.Close();
            }
            else if(comboBox4.SelectionBoxItem.ToString().Equals("院审核员"))
            {
                MainWindow_Online_yshy mainWindow = new MainWindow_Online_yshy(userId, userName);
                mainWindow.Show();
                this.Close();
            }
            else if (comboBox4.SelectionBoxItem.ToString().Equals("院办事员"))
            {
                MainWindow_Online_ybsy mainWindow = new MainWindow_Online_ybsy(userId, userName);
                mainWindow.Show();
                this.Close();
            }

        }
        ///// <summary>
        ///// 对字符串进行MD5加密，返回值为字符串
        ///// </summary>
        //public string getMD5Str(string ConvertString)
        //{
        //    MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
        //    byte[] result = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(ConvertString));

        //    StringBuilder sb = new StringBuilder();
        //    for (int i = 0; i < result.Length; i++)
        //    {
        //        sb.Append(result[i].ToString("x2")); //数值的16进制表示,X后跟数字表示用几位表示
        //    }
        //    return sb.ToString().ToLower();
        //}
        /// <summary>
        /// 本地登录
        /// </summary>
        private void LoginLocal_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
        /// <summary>
        /// 网页登录
        /// </summary>
        private void LoginOnline_Click(object sender, RoutedEventArgs e)
        {
            Hyperlink link = sender as Hyperlink;
            Process.Start(new ProcessStartInfo(link.NavigateUri.AbsoluteUri));
        }
        /// <summary>
        /// 网页注册
        /// </summary>
        private void Register_Click(object sender, RoutedEventArgs e)
        {
            Hyperlink link = sender as Hyperlink;
            Process.Start(new ProcessStartInfo(link.NavigateUri.AbsoluteUri));
        }

        /// <summary>
        /// 网页找回密码
        /// </summary>
        private void Backpassword_Click(object sender, RoutedEventArgs e)
        {
            Hyperlink link = sender as Hyperlink;
            Process.Start(new ProcessStartInfo(link.NavigateUri.AbsoluteUri));
        }

    }
}
