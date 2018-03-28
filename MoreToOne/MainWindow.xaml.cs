using CashMachine.utils;
using log4net;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MoreToOne
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            //获取本机所有IP4
            textBox1_1_2_1.Text = GetLocalIpv4();
            this.ResizeMode = ResizeMode.CanMinimize;//禁用“最大化”按钮
            button1_1_2_1_Click();//初始化时打开端口
            DispatcherTime();//开启定时程序
        }

        /////////////////////////////////////////////////////////////////////////////////
        #region part1_1 以太网连接方式
        private string host = "127.0.0.1";//默认主机IP
        private int port = 7001;//默认端口号
        private EthernetConnection ethernetConnection = null;
        private DispatcherTimer dispatcherTimer = null;
        //ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        /// <summary>
        /// 开启端口监听
        /// </summary>
        private void button1_1_2_1_Click(object sender, RoutedEventArgs e)
        {
            button1_1_2_1_Click();
        }
        /// <summary>
        /// 开启端口监听 无参
        /// </summary>
        private void button1_1_2_1_Click()
        {
            if (button1_1_2_1.Content.Equals("OPEN"))
            {
                //if (textBox1_1_2_1.Text.Equals(""))
                //{
                //    MessageBox.Show("IP is required!", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                //}
                //else
                if (textBox1_1_2_2.Text.Equals(""))
                {
                    MessageBox.Show("Port is required!", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else if (Convert.ToInt32(textBox1_1_2_2.Text) > 65535)
                {
                    MessageBox.Show("The number of Port must be less than or equal to 65535!", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    //host = textBox1_1_2_1.Text;
                    port = Convert.ToInt32(textBox1_1_2_2.Text);
                    if (ethernetConnection == null)
                    {
                        ethernetConnection = new EthernetConnection(host, port);//开启以太网连接
                    }
                    button1_1_2_1.Content = "CLOSE";//按钮变成关闭
                    //statusBarItem2.Content = "EthernetConnection：Opened  ";
                }
            }
            else
            {
                ethernetConnection.Close();
                ethernetConnection = null;
                button1_1_2_1.Content = "OPEN";
                //statusBarItem2.Content = "EthernetConnection：Closed  ";
            }
        }
        /// <summary>
        /// 只能输入0-9数字
        /// </summary>
        private void textBox1_1_2_2_TextChanged(object sender, TextChangedEventArgs e)
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
        /// 获取本机所有IP4
        /// </summary>
        /// <returns></returns>
        public string GetLocalIpv4()
        {
            //事先不知道ip的个数，数组长度未知，因此用StringCollection储存  
            IPAddress[] localIPs;
            localIPs = Dns.GetHostAddresses(Dns.GetHostName());
            StringCollection IpCollection = new StringCollection();
            foreach (IPAddress ip in localIPs)
            {
                //根据AddressFamily判断是否为ipv4,如果是InterNetWorkV6则为ipv6  
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    IpCollection.Add(ip.ToString());
            }
            string[] IpArray = new string[IpCollection.Count];
            IpCollection.CopyTo(IpArray, 0);
            string result = string.Empty;
            foreach (string ip in IpArray)
            {
                result += ip.ToString() + "\n";
            }
            return result;
        }
        /// <summary>
        /// 定时器函数
        /// </summary>
        public void DispatcherTime()
        {
            dispatcherTimer = new DispatcherTimer();
            //定时查询-定时器
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 30, 0);//每隔0.5小时重启一次程序
            dispatcherTimer.Start();
            Console.WriteLine("定时器开启！");
        }
        /// <summary>
        /// 定时器回调函数，每隔一段时间重启一次程序
        /// </summary>
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            //
            if (ethernetConnection != null)
            {
                ethernetConnection.Close();
                ethernetConnection = null;
                ethernetConnection = new EthernetConnection(host, port);//开启以太网连接
            }
            else
            {
                ethernetConnection = new EthernetConnection(host, port);//开启以太网连接
                button1_1_2_1.Content = "CLOSE";//按钮变成关闭
            }
            Console.WriteLine("定时器程序执行一次！");
            //log.Info("定时器程序执行一次！");
        }
        #endregion
        /////////////////////////////////////////////////////////////////////////////////
    }
}
