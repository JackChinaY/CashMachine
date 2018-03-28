using CashMachine.SQLiteDB;
using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Windows;
using CheckUtils;
using System.Windows.Controls;
using CashMachine.entity_local;
using CashMachine.dialogs_local_en;
using CashMachine.utils;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace CashMachine
{
    /// <summary>
    /// MainWindow_Local_EN.xaml 的交互逻辑 英文版
    /// </summary>
    public partial class MainWindow_Local_EN : Window
    {
        //private string currentUserName = "李四";//当前用户名
        //private string currentUserId = "cbb418cc-8520-459f-ab02-ae3516388eb5";//当前用户名Id，软件发布的时候把该字符内容删除掉
        private static string programmingDB = "database/programmingDB.db"; //连接的是programmingDB.db
        private static string dmDB = "database/dmDB.db";                   //连接的是dmDB.db
        private static string goodsDB = "database/goodsDB.db";             //连接的是goodsDB.db  单品
        private static string systemDB = "database/systemDB.db";           //连接的是systemDB.db
        private static string buyerDB = "database/buyerDB.db";             //连接的是buyerDB.db 
        private static string currenylistDB = "database/currencylistDB.db"; //连接的是currencylistDB.db 

        SQLiteDBHelper sqliteDBHelper_programmingDB = null;//声明一个SQLite数据库
        SQLiteDBHelper sqliteDBHelper_goodsDB = null;
        SQLiteDBHelper sqliteDBHelper_dmDB = null;
        SQLiteDBHelper sqliteDBHelper_systemDB = null;
        SQLiteDBHelper sqliteDBHelper_buyerDB = null;

        /////////////////////////////////////////////////////////////////////////////////
        #region 构造函数、最大化、还原 动态改变表格大小-商品表/客户表
        //无参构造函数
        public MainWindow_Local_EN()
        {
            InitializeComponent();//初始化窗体组件
            listAvailablePorts();//打开软件自动检测,软件发布的时候把该行取消注释
            //boottextBlock1.Text = "系统当前用户：" + currentUserName + "  ";
            //statusBarItem1.Content = "SerialPortConnection：" + "Closed  ";
            statusBarItem2.Content = "EthernetConnection：" + "Closed  ";

            sqliteDBHelper_programmingDB = new SQLiteDBHelper(programmingDB);//声明一个SQLite数据库
            sqliteDBHelper_goodsDB = new SQLiteDBHelper(goodsDB);
            sqliteDBHelper_dmDB = new SQLiteDBHelper(dmDB);
            sqliteDBHelper_systemDB = new SQLiteDBHelper(systemDB);
            sqliteDBHelper_buyerDB = new SQLiteDBHelper(buyerDB);
        }

        ////有参构造函数
        //public MainWindow_Local(string userName, string userId)
        //{
        //    InitializeComponent();//初始化窗体组件
        //    //listAvailablePorts();//打开软件自动检测,软件发布的时候把该行取消注释
        //    this.currentUserName = userName;//赋值
        //    this.currentUserId = userId;
        //}

        /// <summary>
        /// 动态改变表格大小 商品表
        /// </summary>
        private void gird2_8_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            dataGrid2_8.MaxHeight = gird2_8.ActualHeight;
            dataGrid2_8.MaxWidth = gird2_8.ActualWidth;
        }
        /// <summary>
        /// 动态改变表格大小 客户表
        /// </summary>
        private void gird4_1_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //Grid grid = (Grid)sender;
            dataGrid4_1.MaxHeight = gird4_1.ActualHeight;
            dataGrid4_1.MaxWidth = gird4_1.ActualWidth;
        }
        #endregion
        /////////////////////////////////////////////////////////////////////////////////
        #region part1_1 串口 此部分代码离线版和在线版相同
        /// <summary>
        /// 检测可用端口 按钮
        /// </summary>
        private void listAvailablePorts_Click(object sender, RoutedEventArgs e)
        {
            listAvailablePorts();
        }
        /// <summary>
        /// 检测可用端口 无参
        /// </summary>
        private void listAvailablePorts()
        {
            //先清空下拉框中原有数据
            chuankou.Items.Clear();
            //遍历端口
            foreach (string s in SerialPort.GetPortNames())
            {
                SerialPort serialPort = new SerialPort(s);
                try
                {
                    //尝试打开此端口
                    serialPort.Open();
                }
                catch (Exception)
                {
                    //若该端口正在被使用则检测下一个端口
                    continue;
                }
                //若此端口被尝试打开成功，则关闭此端口
                serialPort.Close();
                //将此端口添加进下来框中
                chuankou.Items.Add(s);
            }
        }

        /////////////////////////////////////////////////////////////////////////////////
        //声明一些参数
        private SerialPortConnection serialPortConnection = null;//串口
        private string serialPortName = null;//串口名
        private int baudRate;//波特率
        private Parity parity;//校验位
        private int dataBits;//数据位
        private StopBits stopBits;//停止位

        /// <summary>
        /// part1_1 打开端口
        /// </summary>
        private void openPort_Click(object sender, RoutedEventArgs e)
        {
            if (openPort_button.Content.Equals("OPEN"))
            {
                if (chuankou.SelectionBoxItem.ToString().Equals(""))
                {
                    //弹出提示
                    MessageBox.Show("Serial number is not selected!", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    //通信参数前后台转换
                    transport_args();
                    //设置通信，参数使用指定的端口名称、波特率、奇偶校验位、数据位和停止位
                    serialPortConnection = new SerialPortConnection(serialPortName, baudRate, parity, dataBits, stopBits);
                    //打开串口
                    serialPortConnection.Open();
                    //串口打开后就改变按钮内容
                    openPort_button.Content = "CLOSE";
                    //弹出提示
                    //MessageBox.Show("The serial port " + serialPortName + "is opened!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    //修改底部状态栏信息
                    statusBarItem1.Content = "SerialPortConnection：" + serialPortName + "Opened   ";
                }
            }
            else
            {
                //关闭串口
                serialPortConnection.Close();
                serialPortConnection = null;
                //串口关闭后就改变按钮内容
                openPort_button.Content = "OPEN";
                //弹出提示
                //MessageBox.Show(serialPortName + "The serial port is closed!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                //修改底部状态栏信息
                statusBarItem1.Content = "SerialPortConnection：" + serialPortName + "Closed   ";
            }
        }

        /// <summary>
        /// part1_1 通信参数前后台转换
        /// </summary>
        private void transport_args()
        {
            //端口名称
            serialPortName = chuankou.SelectedValue.ToString();
            //波特率
            if (botelv.SelectionBoxItem.ToString().Equals("115200"))
            {
                baudRate = 115200;//115200
            }
            //else if (botelv.SelectionBoxItem.ToString().Equals("19200"))
            //{
            //    baudRate = 19200;//19200波特率
            //}
            //奇偶校验位
            if (checkFlag.SelectionBoxItem.ToString().Equals("None"))
            {
                parity = Parity.None;//无校验
            }
            else if (checkFlag.SelectionBoxItem.ToString().Equals("Odd"))
            {
                parity = Parity.Odd;//奇校验
            }
            else if (checkFlag.SelectionBoxItem.ToString().Equals("Even"))
            {
                parity = Parity.Even;//偶校验
            }
            //数据位
            if (dataFlag.SelectionBoxItem.ToString().Equals("8"))
            {
                dataBits = 8;//8位
            }
            //停止位
            if (stopFlag.SelectionBoxItem.ToString().Equals("1"))
            {
                stopBits = StopBits.One;//1位
            }
            else if (stopFlag.SelectionBoxItem.ToString().Equals("0"))
            {
                stopBits = StopBits.None;//0位
            }
        }

        /////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// part1_1 向下位机发送文件 pluDB.db 点击事件 STX CLA INS LENL LENH LRC DATA ETX CRC
        /// </summary>
        private void sendData_pluDB_Click(object sender, RoutedEventArgs e)
        {
            serialPortConnection.sendFiletoXWJ_TEST4("database\\buyer123.txt");//pluDB.db
        }

        /// <summary>
        /// part1_1 向下位机发送文件 programmingDB.db 点击事件 STX CLA INS LENL LENH LRC DATA ETX CRC
        /// </summary>
        private void sendData_programmingDB_Click(object sender, RoutedEventArgs e)
        {
            //sendFiletoXWJ_Get_File_Info("database\\programmingDB.db");
        }

        ///<summary>
        ///part1_1 向下位机发送文件 dmDB_factory.db
        ///</summary>
        private void sendData_dmDB_factory_Click(object sender, RoutedEventArgs e)
        {
            //sendFiletoXWJ_Get_File_Info("database\\dmDB_factory.db");
        }
        /////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 字节数组转16进制字符串
        /// </summary>
        public string byteToHexStr(byte[] bytes)
        {
            string returnStr = "";
            if (bytes != null)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    returnStr += bytes[i].ToString("X2") + "  ";
                }
            }
            return returnStr;
        }
        #endregion
        /////////////////////////////////////////////////////////////////////////////////
        #region part1_1 以太网连接方式 此部分代码离线版和在线版相同
        private string host = "127.0.0.1";//默认主机IP
        private int port = 6001;//默认端口号
        private EthernetConnection ethernetConnection = null;
        //Thread th = null;//
        private void button1_1_2_1_Click(object sender, RoutedEventArgs e)
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
                    statusBarItem2.Content = "EthernetConnection：Opened  ";
                }
            }
            else
            {
                ethernetConnection.Close();
                button1_1_2_1.Content = "OPEN";
                statusBarItem2.Content = "EthernetConnection：Closed  ";
            }
        }
        /// <summary>
        /// 只能输入0-9数字
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        #endregion
        /////////////////////////////////////////////////////////////////////////////////
        #region 本13个函数控制树形菜单和右侧内容对应一一显示 此部分代码离线版和在线版相同
        /// <summary>
        /// 本13个函数控制树形菜单和右侧内容对应一一显示
        /// </summary>
        //只显示part1_1的内容，其他部分的内容隐藏掉 基本信息设置
        private void TreeViewItem_1_1_Selected(object sender, RoutedEventArgs e)
        {
            Grid grid = null;
            for (int i = 0; i < part.Children.Count; i++)
            {
                grid = part.Children[i] as Grid;//获取子孩子
                grid.Visibility = Visibility.Hidden;//设置其可见性为隐藏
            }
            part1_1.Visibility = Visibility.Visible;//将要显示的部分设为可见
            try
            {
                //加载可用串口
                //listAvailablePorts();
                //获取本机所有IP4
                textBox1_1_2_1.Text = GetLocalIpv4();
            }
            catch (Exception ee)
            {
                MessageBox.Show("The operation failed, please try again. Possible causes:" + ee.Message, "Information", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        //只显示part2_1的内容，其他部分的内容隐藏掉 机型选择
        private void TreeViewItem_2_1_Selected(object sender, RoutedEventArgs e)
        {
            Grid grid = null;
            for (int i = 0; i < part.Children.Count; i++)
            {
                grid = part.Children[i] as Grid;//获取子孩子
                grid.Visibility = Visibility.Hidden;//设置其可见性为隐藏
            }
            part2_1.Visibility = Visibility.Visible;//将要显示的部分设为可见
        }
        //只显示part2_2的内容，其他部分的内容隐藏掉 发票抬头
        private void TreeViewItem_2_2_Selected(object sender, RoutedEventArgs e)
        {
            Grid grid = null;
            for (int i = 0; i < part.Children.Count; i++)
            {
                grid = part.Children[i] as Grid;//获取子孩子
                grid.Visibility = Visibility.Hidden;//设置其可见性为隐藏
            }
            part2_2.Visibility = Visibility.Visible;
            try
            {
                button2_2_2_Click();
            }
            catch (Exception ee)
            {
                MessageBox.Show("The operation failed, please try again. Possible causes:" + ee.Message, "Information", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        //只显示part2_3的内容，其他部分的内容隐藏掉 收银员
        private void TreeViewItem_2_3_Selected(object sender, RoutedEventArgs e)
        {
            Grid grid = null;
            for (int i = 0; i < part.Children.Count; i++)
            {
                grid = part.Children[i] as Grid;//获取子孩子
                grid.Visibility = Visibility.Hidden;//设置其可见性为隐藏
            }
            part2_3.Visibility = Visibility.Visible;
            try
            {
                //刷新查询框内容
                button2_3_2_Click();
            }
            catch (Exception ee)
            {
                MessageBox.Show("The operation failed, please try again. Possible causes:" + ee.Message, "Information", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        //只显示part2_4的内容，其他部分的内容隐藏掉 税率
        private void TreeViewItem_2_4_Selected(object sender, RoutedEventArgs e)
        {
            Grid grid = null;
            for (int i = 0; i < part.Children.Count; i++)
            {
                grid = part.Children[i] as Grid;//获取子孩子
                grid.Visibility = Visibility.Hidden;//设置其可见性为隐藏
            }
            part2_4.Visibility = Visibility.Visible;
            try
            {
                button2_4_2_Click();
            }
            catch (Exception ee)
            {
                MessageBox.Show("The operation failed, please try again. Possible causes:" + ee.Message, "Information", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        //只显示part2_5的内容，其他部分的内容隐藏掉 部类
        private void TreeViewItem_2_5_Selected(object sender, RoutedEventArgs e)
        {
            Grid grid = null;
            for (int i = 0; i < part.Children.Count; i++)
            {
                grid = part.Children[i] as Grid;//获取子孩子
                grid.Visibility = Visibility.Hidden;//设置其可见性为隐藏
            }
            part2_5.Visibility = Visibility.Visible;
            try
            {
                //刷新查询框内容
                button2_5_2_Click();
            }
            catch (Exception ee)
            {
                MessageBox.Show("The operation failed, please try again. Possible causes:" + ee.Message, "Information", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        //只显示part2_6的内容，其他部分的内容隐藏掉 折扣加成及发票限额
        private void TreeViewItem_2_6_Selected(object sender, RoutedEventArgs e)
        {
            Grid grid = null;
            for (int i = 0; i < part.Children.Count; i++)
            {
                grid = part.Children[i] as Grid;//获取子孩子
                grid.Visibility = Visibility.Hidden;//设置其可见性为隐藏
            }
            part2_6.Visibility = Visibility.Visible;
            //button2_6_2_Click();
        }
        //只显示part2_7的内容，其他部分的内容隐藏掉 外币
        private void TreeViewItem_2_7_Selected(object sender, RoutedEventArgs e)
        {
            Grid grid = null;
            for (int i = 0; i < part.Children.Count; i++)
            {
                grid = part.Children[i] as Grid;//获取子孩子
                grid.Visibility = Visibility.Hidden;//设置其可见性为隐藏
            }
            part2_7.Visibility = Visibility.Visible;
            try
            {
                //刷新查询框内容
                button2_7_2_Click();
            }
            catch (Exception ee)
            {
                MessageBox.Show("The operation failed, please try again. Possible causes:" + ee.Message, "Information", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        //只显示part2_8的内容，其他部分的内容隐藏掉 单品
        private void TreeViewItem_2_8_Selected(object sender, RoutedEventArgs e)
        {
            Grid grid = null;
            for (int i = 0; i < part.Children.Count; i++)
            {
                grid = part.Children[i] as Grid;//获取子孩子
                grid.Visibility = Visibility.Hidden;//设置其可见性为隐藏
            }
            part2_8.Visibility = Visibility.Visible;
            try
            {
                //刷新查询框内容
                button2_8_2_Click();
                //设置分页按钮是否可用
                SetPageButtonEnabled();
                //设置控件显示信息
                SetPagerInfo(pageIndex, pageSize, pageCount, totalCount);
            }
            catch (Exception ee)
            {
                MessageBox.Show("The operation failed, please try again. Possible causes:" + ee.Message, "Information", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        //只显示part3_1的内容，其他部分的内容隐藏掉 日报表
        private void TreeViewItem_3_1_Selected(object sender, RoutedEventArgs e)
        {
            Grid grid = null;
            for (int i = 0; i < part.Children.Count; i++)
            {
                grid = part.Children[i] as Grid;//获取子孩子
                grid.Visibility = Visibility.Hidden;//设置其可见性为隐藏
            }
            part3_1.Visibility = Visibility.Visible;
        }
        //只显示part3_2的内容，其他部分的内容隐藏掉 周期报表
        private void TreeViewItem_3_2_Selected(object sender, RoutedEventArgs e)
        {
            Grid grid = null;
            for (int i = 0; i < part.Children.Count; i++)
            {
                grid = part.Children[i] as Grid;//获取子孩子
                grid.Visibility = Visibility.Hidden;//设置其可见性为隐藏
            }
            part3_2.Visibility = Visibility.Visible;
        }
        //只显示part3_3的内容，其他部分的内容隐藏掉 月报表
        private void TreeViewItem_3_3_Selected(object sender, RoutedEventArgs e)
        {
            Grid grid = null;
            for (int i = 0; i < part.Children.Count; i++)
            {
                grid = part.Children[i] as Grid;//获取子孩子
                grid.Visibility = Visibility.Hidden;//设置其可见性为隐藏
            }
            part3_3.Visibility = Visibility.Visible;
        }
        //只显示part3_4的内容，其他部分的内容隐藏掉 总报表
        private void TreeViewItem_3_4_Selected(object sender, RoutedEventArgs e)
        {
            Grid grid = null;
            for (int i = 0; i < part.Children.Count; i++)
            {
                grid = part.Children[i] as Grid;//获取子孩子
                grid.Visibility = Visibility.Hidden;//设置其可见性为隐藏
            }
            part3_4.Visibility = Visibility.Visible;
        }
        //只显示part4_1的内容，其他部分的内容隐藏掉 客户管理
        private void TreeViewItem_4_1_Selected(object sender, RoutedEventArgs e)
        {
            Grid grid = null;
            for (int i = 0; i < part.Children.Count; i++)
            {
                grid = part.Children[i] as Grid;//获取子孩子
                grid.Visibility = Visibility.Hidden;//设置其可见性为隐藏
            }
            part4_1.Visibility = Visibility.Visible;
            try
            {
                //刷新查询框内容
                button4_1_2_Click();
                //设置分页按钮是否可用
                SetPageButtonEnabled_buyer();
                //设置控件显示信息
                SetPagerInfo_buyer(pageIndex_buyer, pageSize_buyer, pageCount_buyer, totalCount_buyer);
            }
            catch (Exception ee)
            {
                MessageBox.Show("The operation failed, please try again. Possible causes:" + ee.Message, "Information", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion
        /////////////////////////////////////////////////////////////////////////////////
        #region part2_2 发票抬头 本地数据库
        ///<summary>
        ///part2_2 发票抬头 添加至本地数据库
        ///</summary>
        private void button2_2_1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //判断查询结果是否为0行
                //SQL语句
                string sql_sqlite = "SELECT COUNT(*) AS COUNTS FROM Company_Info_Table";
                //执行查询，结果为DataTable类型
                DataTable dt = sqliteDBHelper_programmingDB.ExecuteDataTable(sql_sqlite, null);
                //判断查询结果
                if (Convert.ToInt32(dt.Rows[0]["COUNTS"]) > 0)
                {
                    //弹出提示框
                    MessageBox.Show("数据库中已有此数据，请点击“修改”按钮进行修改数据即可!!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                else
                {
                    //声明一个修改信息的窗体
                    Insert_Header Ins_header = new Insert_Header(programmingDB);
                    //弹出窗体
                    Ins_header.ShowDialog();
                    //刷新查询框内容
                    button2_2_2_Click();
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show("The operation failed, please try again. Possible causes:" + ee.Message, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        ///<summary>
        ///part2_2 发票抬头 查询本地数据库  使用DataTable
        ///</summary>
        private void button2_2_2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                button2_2_2_Click();//查询
            }
            catch (Exception ee)
            {
                MessageBox.Show("The operation failed, please try again. Possible causes:" + ee.Message, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        ///<summary>
        ///part2_2 发票抬头 查询本地数据库  使用DataTable 无参的
        ///</summary>
        private void button2_2_2_Click()
        {
            //SQL语句
            string sql_sqlite = "SELECT id,Number,Line,Flag FROM Company_Info_Table ORDER BY Number ASC";
            //执行查询，结果为DataTable类型
            DataTable dt = sqliteDBHelper_programmingDB.ExecuteDataTable(sql_sqlite, null);
            //判断查询结果是否为0行
            if (dt.Rows.Count == 0)
            {
                //弹出提示框
                //MessageBox.Show("No result!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                dataGrid2_2.ItemsSource = null;//先清空表格内容
                DataTable dt_temp = new DataTable();//新建临时表
                dt_temp.Columns.Add(new DataColumn("Tip"));//添加列
                dt_temp.Rows.Add(dt_temp.NewRow());//添加行
                dt_temp.Rows[0]["Tip"] = "No result！";//设置行内容
                dataGrid2_2.ItemsSource = dt_temp.DefaultView;//把表格放到控件中
                return;
            }
            //查询出来的表格的字段名为英文，但显示给用户的时候要为中文，所以在此添加4个中文字段名显示给用户
            dt.Columns.Add(new DataColumn("序号"));
            dt.Columns.Add(new DataColumn("内容"));
            dt.Columns.Add(new DataColumn("标志位"));
            //以下为给新添加的中文字段赋值
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dt.Rows[i]["序号"] = dt.Rows[i]["Number"];
                dt.Rows[i]["内容"] = dt.Rows[i]["Line"];
                dt.Rows[i]["标志位"] = dt.Rows[i]["Flag"];
            }
            //将表格显示到窗体表格控件中
            dataGrid2_2.ItemsSource = dt.DefaultView;
            //count是dt表格中前若干列标题是英文的字段数，也就是select查询的的字段数
            int count = 4;
            //将英文字段隐藏掉
            for (int i = 0; i < count; i++)
            {
                this.dataGrid2_2.Columns[i].Visibility = System.Windows.Visibility.Hidden;
            }
            //以下为设置字段宽度
            this.dataGrid2_2.Columns[count++].Width = 90;
            this.dataGrid2_2.Columns[count++].Width = 220;
            this.dataGrid2_2.Columns[count++].Width = 100;
        }

        ///<summary>
        ///part2_2 发票抬头 修改本地数据库
        ///</summary>
        private void button2_2_3_Click(object sender, RoutedEventArgs e)
        {
            //判断查询结果是否为0行
            //SQL语句
            string sql_sqlite = "SELECT id,Number,Line,Flag FROM company_info_table ORDER BY Number ASC";
            //执行查询，结果为DataTable类型
            DataTable dt = sqliteDBHelper_programmingDB.ExecuteDataTable(sql_sqlite, null);
            //判断查询结果是否为0行
            if (dt.Rows.Count == 0)
            {
                //弹出提示框
                MessageBox.Show("数据库中无此数据，请先点击“添加”按钮进行数据添加!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            //声明一个修改信息的窗体
            Update_Header header_udp = new Update_Header(programmingDB);
            //弹出窗体
            header_udp.ShowDialog();
            //刷新查询框内容
            button2_2_2_Click();
        }
        #endregion
        /////////////////////////////////////////////////////////////////////////////////
        #region part2_3 收银员 本地数据库
        ///<summary>
        ///part2_3 收银员 添加至本地数据库
        ///</summary>
        private void button2_3_1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //声明一个修改信息的窗体
                Insert_Cashier Ins_cashier = new Insert_Cashier(programmingDB);
                //弹出窗体
                Ins_cashier.ShowDialog();
                //刷新查询框内容
                button2_3_2_Click();
            }
            catch (Exception ee)
            {
                MessageBox.Show("The operation failed, please try again. Possible causes:" + ee.Message, "Information", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        ///<summary>
        ///part2_3 收银员 查询本地数据库
        ///</summary>
        private void button2_3_2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //查询数据
                button2_3_2_Click();
            }
            catch (Exception ee)
            {
                MessageBox.Show("The operation failed, please try again. Possible causes:" + ee.Message, "Information", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        ///<summary>
        ///part2_3 收银员 查询本地数据库 无参
        ///</summary>
        private void button2_3_2_Click()
        {
            //SQL查询语句
            string sql = "SELECT Number,Name,Code,Password FROM Cashier_Table ORDER BY Number ASC";
            //执行查询，结果为DataTable类型
            DataTable dt = sqliteDBHelper_programmingDB.ExecuteDataTable(sql, null);
            //判断查询结果是否为0行
            if (dt.Rows.Count == 0)
            {
                dataGrid2_3.ItemsSource = null;//先清空表格内容
                DataTable dt_temp = new DataTable();//新建临时表
                dt_temp.Columns.Add(new DataColumn("No data！"));//添加列
                                                                //dt_temp.Rows.Add(dt_temp.NewRow());//添加行
                                                                //dt_temp.Rows[0]["Tip"] = "No result！";//设置行内容
                dataGrid2_3.ItemsSource = dt_temp.DefaultView;//把表格放到控件中
                return;
            }
            //将结果放到前台控件中
            dataGrid2_3.ItemsSource = dt.DefaultView;
            int count = 0;
            //以下是设置中文字段宽度
            this.dataGrid2_3.Columns[count++].Width = 90;
            this.dataGrid2_3.Columns[count++].Width = 150;
            this.dataGrid2_3.Columns[count++].Width = 150;
            this.dataGrid2_3.Columns[count++].Width = 150;
        }
        ///<summary>
        ///part2_3 收银员 删除
        ///</summary>
        private void button2_3_3_Click(object sender, RoutedEventArgs e)
        {
            //未选择一条数据时 弹出提示框
            if (dataGrid2_3.SelectedItem == null)
            {
                MessageBox.Show("Please select one record to delete first!", "Information", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            //选择了某一行数据时
            else
            {
                //获取选中行所有数据
                DataRowView mySelectedElement = (DataRowView)dataGrid2_3.SelectedItem;
                //弹出确认框，当为确定时
                if (MessageBox.Show("Confirm that you want to delete the record ( the Code: " + mySelectedElement.Row["Code"].ToString() + " )？", "Information", MessageBoxButton.OKCancel, MessageBoxImage.Information) == MessageBoxResult.OK)
                {
                    //获取必要信息
                    string Code = mySelectedElement.Row["Code"].ToString();
                    //SQL语句
                    string sql = "DELETE FROM Cashier_Table WHERE Code=@Code";
                    //配置SQL语句里的参数
                    SQLiteParameter[] parameters = {
                        new SQLiteParameter("@Code",Code)
                    };
                    //执行SQL，并做判断
                    if (sqliteDBHelper_programmingDB.ExecuteNonQuery(sql, parameters) == 1)
                    {
                        //刷新查询框内容
                        button2_3_2_Click();
                    }
                    else
                    {
                        MessageBox.Show("Delete failed!", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        ///<summary>
        ///part2_3 收银员 修改并提交到本地数据库
        ///</summary>
        private void button2_3_4_Click(object sender, RoutedEventArgs e)
        {
            //未选择一条数据时 弹出提示框
            if (dataGrid2_3.SelectedItem == null)
            {
                MessageBox.Show("Please select one of the data you want to modify!", "Information", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            //选择了某一行数据时
            else
            {
                //获取选中行所有数据
                DataRowView mySelectedElement = (DataRowView)dataGrid2_3.SelectedItem;
                //声明一个变量
                Cashier cashier = new Cashier();
                //变量赋值
                cashier.Number = Convert.ToInt32(mySelectedElement.Row["Number"]);
                cashier.Name = mySelectedElement.Row["Name"].ToString();
                cashier.Code = mySelectedElement.Row["Code"].ToString();
                cashier.Password = mySelectedElement.Row["Password"].ToString();
                //cashier.Flag = Convert.ToInt32(mySelectedElement.Row["Flag"]);
                //声明一个修改信息的窗体
                Update_Cashier upd_cashier = new Update_Cashier(cashier, programmingDB);
                //弹出窗体
                upd_cashier.ShowDialog();
                //刷新查询框内容
                button2_3_2_Click();
            }
        }
        #endregion
        /////////////////////////////////////////////////////////////////////////////////
        #region part2_4 税率 本地数据库
        ///<summary>
        ///part2_4 税率 添加到本地数据库
        ///</summary>
        private void button2_4_1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //声明一个添加信息的窗体
                Insert_Tax ins_tax = new Insert_Tax(systemDB);
                //弹出窗体
                ins_tax.ShowDialog();
                //刷新查询框内容
                button2_4_2_Click();
            }
            catch (Exception ee)
            {
                MessageBox.Show("The operation failed, please try again. Possible causes:" + ee.Message, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        ///<summary>
        ///part2_4 税率 查询本地数据库
        ///</summary>
        private void button2_4_2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                button2_4_2_Click();
            }
            catch (Exception ee)
            {
                MessageBox.Show("The operation failed, please try again. Possible causes:" + ee.Message, "Information", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        ///<summary>
        ///part2_4 税率 查询本地数据库
        ///</summary>
        private void button2_4_2_Click()
        {
            //SQL查询语句
            //string sql = "SELECT Number,Invoice_Code,Invoice_Name,Tax_Code,Tax_Name,Tax_Rate,Exempt_Flag,CRC32 FROM Tax_Tariff ORDER BY Number ASC";
            string sql = "SELECT Number,Tax_Code,Tax_Name,Tax_Rate FROM Tax_Tariff ORDER BY Number ASC";
            //执行查询，结果为DataTable类型
            DataTable dt = sqliteDBHelper_systemDB.ExecuteDataTable(sql, null);
            //判断查询结果是否为0行
            if (dt.Rows.Count == 0)
            {
                //弹出提示框
                //MessageBox.Show("No result!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                dataGrid2_3.ItemsSource = null;//先清空表格内容
                DataTable dt_temp = new DataTable();//新建临时表
                dt_temp.Columns.Add(new DataColumn("Tip"));//添加列
                dt_temp.Rows.Add(dt_temp.NewRow());//添加行
                dt_temp.Rows[0]["Tip"] = "No result！";//设置行内容
                dataGrid2_3.ItemsSource = dt_temp.DefaultView;//把表格放到控件中
                return;
            }
            //查询出来的表格的字段名为英文，但显示给用户的时候要为中文，所以在此添加若干个中文字段名显示给用户
            dt.Columns.Add(new DataColumn("Tax Rate"));
            //dt.Columns.Add(new DataColumn("税率代码"));
            //dt.Columns.Add(new DataColumn("姓名"));
            //dt.Columns.Add(new DataColumn("密码"));
            //dt.Columns.Add(new DataColumn("标志位"));
            //以下为给新添加的中文字段赋值
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dt.Rows[i]["Tax Rate"] = (Convert.ToDouble(dt.Rows[i]["Tax_Rate"].ToString()) / 10000).ToString("0.00%");
            }
            //string result = JsonConvert.SerializeObject(dt);//将DataTable类型的变量序列化成json字符串
            //Console.WriteLine(result);
            //将结果放到前台控件中
            dataGrid2_4.ItemsSource = dt.DefaultView;
            //count是dt表格中前若干列标题是英文的字段数，也就是select查询的的字段数
            int count = 0;
            //将英文字段隐藏掉
            //for (int i = 0; i < count; i++)
            //{
            this.dataGrid2_4.Columns[3].Visibility = System.Windows.Visibility.Hidden;
            //}
            //以下是设置中文字段宽度
            this.dataGrid2_4.Columns[count++].Width = 90;
            this.dataGrid2_4.Columns[count++].Width = 140;
            this.dataGrid2_4.Columns[count++].Width = 190;
            this.dataGrid2_4.Columns[count++].Width = 100;
            this.dataGrid2_4.Columns[count++].Width = 130;
            //this.dataGrid2_4.Columns[count++].Width = 100;
            //this.dataGrid2_4.Columns[count++].Width = 90;
            //this.dataGrid2_4.Columns[count++].Width = 90;
            //this.dataGrid2_4.Columns[count++].Width = 100;
        }

        ///<summary>
        ///part2_4 税率 删除
        ///</summary>
        private void button2_4_3_Click(object sender, RoutedEventArgs e)
        {
            //未选择一条数据时 弹出提示框
            if (dataGrid2_4.SelectedItem == null)
            {
                MessageBox.Show("Please select one record to delete first!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            //选择了某一行数据时
            else
            {
                //获取选中行所有数据
                DataRowView mySelectedElement = (DataRowView)dataGrid2_4.SelectedItem;
                //判断是否是前6位固定不可改税率
                int Number = Convert.ToInt32(mySelectedElement.Row["Number"].ToString());
                if (Number <= 6)
                {
                    MessageBox.Show("This Fiscal can not be deleted !", "Information", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    //弹出确认框，当为确定时
                    if (MessageBox.Show("Confirm that you want to delete the record ( the Number: " + mySelectedElement.Row["Number"].ToString() + " )？", "Information", MessageBoxButton.OKCancel, MessageBoxImage.Information) == MessageBoxResult.OK)
                    {
                        //获取必要信息
                        string Id = mySelectedElement.Row["Number"].ToString();
                        //SQL语句
                        string sql = "DELETE FROM Tax_Tariff WHERE Number=@Number";
                        //配置SQL语句里的参数
                        SQLiteParameter[] parameters = {
                            new SQLiteParameter("@Number",Id)
                        };
                        //执行SQL，并做判断
                        if (sqliteDBHelper_systemDB.ExecuteNonQuery(sql, parameters) == 1)
                        {
                            //将商品中关联此税率的商品税率信息置为空,SQL语句
                            string sql_goods = "UPDATE Goods_Info SET Tax_Index='' WHERE Tax_Index=@Tax_Index";
                            //配置SQL语句里的参数
                            SQLiteParameter[] parameter = {
                            new SQLiteParameter("@Tax_Index",Id),
                        };
                            //执行SQL
                            sqliteDBHelper_goodsDB.ExecuteNonQuery(sql_goods, parameter);
                            //刷新查询框内容
                            button2_4_2_Click();
                        }
                    }
                }
            }
        }

        ///<summary>
        ///part2_4 税率 修改本地数据库
        ///</summary>
        private void button2_4_4_Click(object sender, RoutedEventArgs e)
        {
            //未选择一条数据时 弹出提示框
            if (dataGrid2_4.SelectedItem == null)
            {
                MessageBox.Show("Please select one of the data you want to modify!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            //选择了某一行数据时
            else
            {
                //获取选中行所有数据
                DataRowView mySelectedElement = (DataRowView)dataGrid2_4.SelectedItem;
                //声明一个变量
                Tax tax = new Tax();
                //变量赋值
                //tax.Id = mySelectedElement.Row["Id"].ToString();
                tax.Number = mySelectedElement.Row["Number"].ToString();
                tax.Code = mySelectedElement.Row["Tax_Code"].ToString();
                tax.Name = mySelectedElement.Row["Tax_Name"].ToString();
                tax.Rate = mySelectedElement.Row["Tax_Rate"].ToString();
                //判断是否是前6位固定不可改税率
                int Number = Convert.ToInt32(tax.Number);
                if (Number <= 6)
                {
                    MessageBox.Show("This Fiscal can not be modified!", "Information", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    //声明一个修改信息的窗体
                    Update_Tax upd_tax = new Update_Tax(tax, systemDB);
                    //弹出窗体
                    upd_tax.ShowDialog();
                    //刷新查询框内容
                    button2_4_2_Click();
                }
            }
        }
        #endregion
        /////////////////////////////////////////////////////////////////////////////////
        #region part2_5 部类 本地数据库
        ///<summary>
        ///part2_5 部类 添加到本地数据库
        ///</summary>
        private void button2_5_1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //声明一个添加信息的窗体
                Insert_Department ins_dep = new Insert_Department(goodsDB);
                //弹出窗体
                ins_dep.ShowDialog();
                //刷新查询框内容
                button2_5_2_Click();
            }
            catch (Exception ee)
            {
                MessageBox.Show("The operation failed, please try again. Possible causes:" + ee.Message, "Information", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        ///<summary>
        ///part2_5 部类 查询本地数据库
        ///</summary>
        private void button2_5_2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //查询
                button2_5_2_Click();
            }
            catch (Exception ee)
            {
                MessageBox.Show("The operation failed, please try again. Possible causes:" + ee.Message, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        ///<summary>
        ///part2_5 部类 查询本地数据库
        ///</summary>
        private void button2_5_2_Click()
        {
            //SQL查询语句
            string sql = "SELECT id,Dept_No,PLU_No FROM Department_Associate ORDER BY Dept_No ASC";
            //执行查询，结果为DataTable类型
            DataTable dt = sqliteDBHelper_goodsDB.ExecuteDataTable(sql, null);
            //判断查询结果是否为0行
            if (dt.Rows.Count == 0)
            {
                //弹出提示框
                //MessageBox.Show("No result!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                dataGrid2_5.ItemsSource = null;//先清空表格内容
                DataTable dt_temp = new DataTable();//新建临时表
                dt_temp.Columns.Add(new DataColumn("Tip"));//添加列
                dt_temp.Rows.Add(dt_temp.NewRow());//添加行
                dt_temp.Rows[0]["Tip"] = "No result！";//设置行内容
                dataGrid2_5.ItemsSource = dt_temp.DefaultView;//把表格放到控件中
                return;
            }
            //查询出来的表格的字段名为英文，但显示给用户的时候要为中文，所以在此添加若干个中文字段名显示给用户
            dt.Columns.Add(new DataColumn("Number"));
            dt.Columns.Add(new DataColumn("Dept No"));
            dt.Columns.Add(new DataColumn("PLU No"));
            //以下为给新添加的中文字段赋值
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dt.Rows[i]["Number"] = dt.Rows[i]["id"];
                dt.Rows[i]["Dept No"] = dt.Rows[i]["Dept_No"];
                dt.Rows[i]["PLU No"] = dt.Rows[i]["PLU_No"];
            }
            dataGrid2_5.ItemsSource = dt.DefaultView;
            //count是dt表格中前若干列标题是英文的字段数，也就是select查询的的字段数
            int count = 3;
            //将英文字段隐藏掉
            for (int i = 0; i < count; i++)
            {
                this.dataGrid2_5.Columns[i].Visibility = System.Windows.Visibility.Hidden;
            }
            //以下为设置字段宽度
            this.dataGrid2_5.Columns[count++].Width = 100;
            this.dataGrid2_5.Columns[count++].Width = 160;
            this.dataGrid2_5.Columns[count++].Width = 160;
        }

        ///<summary>
        ///part2_5 部类 删除
        ///</summary>
        private void button2_5_3_Click(object sender, RoutedEventArgs e)
        {
            //未选择一条数据时 弹出提示框
            if (dataGrid2_5.SelectedItem == null)
            {
                MessageBox.Show("Please select one record to delete first!", "Information", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            //选择了某一行数据时
            else
            {
                //获取选中行所有数据
                DataRowView mySelectedElement = (DataRowView)dataGrid2_5.SelectedItem;
                //弹出确认框，当为确定时
                if (MessageBox.Show("确认要删除序号为： " + mySelectedElement.Row["序号"].ToString() + "  这条记录吗？", "Information", MessageBoxButton.OKCancel, MessageBoxImage.Information) == MessageBoxResult.OK)
                {
                    //获取必要信息
                    string Id = mySelectedElement.Row["id"].ToString();
                    //SQL语句
                    string sql = "DELETE FROM Department_Associate WHERE id=@id";
                    //配置SQL语句里的参数
                    SQLiteParameter[] parameters = {
                    new SQLiteParameter("@id",Id),
                };
                    //执行SQL，并做判断
                    if (sqliteDBHelper_goodsDB.ExecuteNonQuery(sql, parameters) == 1)
                    {
                        //刷新查询框内容
                        button2_5_2_Click();
                    }
                    else
                    {
                        MessageBox.Show("Delete failed!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
        }

        ///<summary>
        ///part2_5 部类 修改本地数据库
        ///</summary>
        private void button2_5_4_Click(object sender, RoutedEventArgs e)
        {
            //未选择一条数据时 弹出提示框
            if (dataGrid2_5.SelectedItem == null)
            {
                MessageBox.Show("Please select one of the data you want to modify!", "Information", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            //选择了某一行数据时
            else
            {
                //获取选中行所有数据
                DataRowView mySelectedElement = (DataRowView)dataGrid2_5.SelectedItem;
                //声明一个变量
                Department department = new Department();
                //变量赋值
                department.Id = mySelectedElement.Row["id"].ToString();
                department.Dept_No = mySelectedElement.Row["Dept_No"].ToString();
                department.PLU_No = mySelectedElement.Row["PLU_No"].ToString();

                //声明一个修改信息的窗体
                Update_Department upd_dep = new Update_Department(department, goodsDB);
                //弹出窗体
                upd_dep.ShowDialog();
                //刷新查询框内容
                button2_5_2_Click();
            }
        }
        #endregion
        /////////////////////////////////////////////////////////////////////////////////
        #region part2_6 折扣加成及发票限额 本地数据库

        #endregion
        /////////////////////////////////////////////////////////////////////////////////
        #region part2_7 外币 本地数据库
        ///<summary>
        ///part2_7 外币 添加到本地数据库
        ///</summary>
        private void button2_7_1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //声明一个添加信息的窗体
                Insert_ForeignCurrency ins_foreignCurrency = new Insert_ForeignCurrency(programmingDB);
                //弹出窗体
                ins_foreignCurrency.ShowDialog();
                //刷新查询框内容
                button2_7_2_Click();
            }
            catch (Exception ee)
            {
                MessageBox.Show("The operation failed, please try again. Possible causes:" + ee.Message, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        ///<summary>
        ///part2_7 外币 查询本地数据库
        ///</summary>
        private void button2_7_2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                button2_7_2_Click();
            }
            catch (Exception ee)
            {
                MessageBox.Show("The operation failed, please try again. Possible causes:" + ee.Message, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        ///<summary>
        ///part2_7 外币 查询本地数据库
        ///</summary>
        private void button2_7_2_Click()
        {
            //SQL查询语句
            string sql = "SELECT Number,Name,Abbreviation,Symbol,Exchange_Rate,Current FROM Currency_Table ORDER BY Number ASC;";
            //执行查询，结果为DataTable类型
            DataTable dt = sqliteDBHelper_programmingDB.ExecuteDataTable(sql, null);
            //判断查询结果是否为0行
            if (dt.Rows.Count == 0)
            {
                dataGrid2_7.ItemsSource = null;//先清空表格内容
                DataTable dt_temp = new DataTable();//新建临时表
                dt_temp.Columns.Add(new DataColumn("Tip"));//添加列
                dt_temp.Rows.Add(dt_temp.NewRow());//添加行
                dt_temp.Rows[0]["Tip"] = "No result！";//设置行内容
                dataGrid2_7.ItemsSource = dt_temp.DefaultView;//把表格放到控件中
                return;
            }
            //dt.Columns.Add(new DataColumn("Exchange Rate"));
            //以下为给新添加的中文字段赋值
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dt.Rows[i]["Exchange_Rate"] = Convert.ToDouble(dt.Rows[i]["Exchange_Rate"].ToString()) / 10000;
            }
            dataGrid2_7.ItemsSource = dt.DefaultView;
            //count是dt表格中前若干列标题是英文的字段数，也就是select查询的的字段数
            int count = 0;
            //以下是设置中文字段宽度
            this.dataGrid2_7.Columns[count++].Width = 90;
            this.dataGrid2_7.Columns[count++].Width = 180;
            this.dataGrid2_7.Columns[count++].Width = 120;
            this.dataGrid2_7.Columns[count++].Width = 90;
            this.dataGrid2_7.Columns[count++].Width = 120;
            this.dataGrid2_7.Columns[count++].Width = 100;
            //隐藏掉Exchange_Rate列
            //this.dataGrid2_7.Columns[2].Visibility = System.Windows.Visibility.Hidden;
        }

        ///<summary>
        ///part2_7 外币 删除
        ///</summary>
        private void button2_7_3_Click(object sender, RoutedEventArgs e)
        {
            //弹出提示框，未选择要删除的数据
            if (dataGrid2_7.SelectedItem == null)
            {
                MessageBox.Show("Please select one record to delete first !", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            //选择了某一行数据时
            else
            {
                //获取选中行所有数据
                DataRowView mySelectedElement = (DataRowView)dataGrid2_7.SelectedItem;
                //弹出确认框，当为确定时
                if (MessageBox.Show("Confirm that you want to delete the record ( the Number: " + mySelectedElement.Row["Number"].ToString() + " )？", "Information", MessageBoxButton.OKCancel, MessageBoxImage.Information) == MessageBoxResult.OK)
                {
                    //获取必要信息
                    string Number = mySelectedElement.Row["Number"].ToString();
                    //SQL语句
                    string sql = "DELETE FROM Currency_Table WHERE Number=@Number";
                    //配置SQL语句里的参数
                    SQLiteParameter[] parameters = {
                    new SQLiteParameter("@Number",Number),
                };
                    //执行SQL，并做判断
                    if (sqliteDBHelper_programmingDB.ExecuteNonQuery(sql, parameters) == 1)
                    {
                        //刷新查询框内容
                        button2_7_2_Click();
                    }
                    else
                    {
                        MessageBox.Show("Delete failed!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
        }

        ///<summary>
        ///part2_7 外币 修改本地数据库
        ///</summary>
        private void button2_7_4_Click(object sender, RoutedEventArgs e)
        {
            //弹出提示框，未选择要删除的数据
            if (dataGrid2_7.SelectedItem == null)
            {
                MessageBox.Show("Please select one of the data you want to modify!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            //选择了某一行数据时
            else
            {
                //获取选中行所有数据
                DataRowView mySelectedElement = (DataRowView)dataGrid2_7.SelectedItem;
                //声明一个变量
                ForeignCurrency foreignCurrency = new ForeignCurrency();
                //变量赋值
                //foreignCurrency.Id = mySelectedElement.Row["id"].ToString();
                foreignCurrency.Number = mySelectedElement.Row["Number"].ToString();
                foreignCurrency.Name = mySelectedElement.Row["Name"].ToString();
                foreignCurrency.Abbreviation = mySelectedElement.Row["Abbreviation"].ToString();
                foreignCurrency.Symbol = mySelectedElement.Row["Symbol"].ToString();
                //foreignCurrency.Symbol_Direction = mySelectedElement.Row["Symbol_Direction"].ToString();
                //foreignCurrency.Thousand_Separator = mySelectedElement.Row["Thousand_Separator"].ToString();
                //foreignCurrency.Cent_Separator = mySelectedElement.Row["Cent_Separator"].ToString();
                //foreignCurrency.Decimal_Places = mySelectedElement.Row["Decimal_Places"].ToString();
                foreignCurrency.Exchange_Rate = Convert.ToDouble(mySelectedElement.Row["Exchange_Rate"].ToString());
                foreignCurrency.Current = Convert.ToInt32(mySelectedElement.Row["Current"].ToString());
                //声明一个修改信息的窗体
                Update_ForeignCurrency upd_foreignCurrency = new Update_ForeignCurrency(foreignCurrency, programmingDB, currenylistDB, goodsDB);
                //弹出窗体
                upd_foreignCurrency.ShowDialog();
                //刷新查询框内容
                button2_7_2_Click();
            }
        }
        #endregion
        /////////////////////////////////////////////////////////////////////////////////
        #region part2_8 单品 本地数据库
        ///<summary>
        ///part2_8 单品 添加到本地数据库
        ///</summary>
        private void button2_8_1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //声明一个添加信息的窗体
                Insert_Plu ins_plu = new Insert_Plu(goodsDB, systemDB, programmingDB);
                //弹出窗体
                ins_plu.ShowDialog();
                //刷新查询框内容
                button2_8_2_Click();
                //设置分页按钮是否可用
                SetPageButtonEnabled();
                //设置控件显示信息
                SetPagerInfo(pageIndex, pageSize, pageCount, totalCount);
            }
            catch (Exception ee)
            {
                MessageBox.Show("The operation failed, please try again. Possible causes:" + ee.Message, "Information", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        ///<summary>
        ///part2_8 单品 查询本地数据库
        ///</summary>
        //private void button2_8_2_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        pageIndex = 1;
        //        //查询 设置 pageCount totalCount ；显示数据
        //        button2_8_2_Click();
        //        //设置分页按钮是否可用
        //        SetPageButtonEnabled();
        //        //设置控件显示信息
        //        SetPagerInfo(pageIndex, pageSize, pageCount, totalCount);
        //    }
        //    catch (Exception ee)
        //    {
        //        MessageBox.Show("The operation failed, please try again. Possible causes:" + ee.Message, "Information", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //}

        private int pageIndex = 1;// 当前页码
        private int pageSize = 100;// 分页大小
        private int totalCount = 0;// 记录总数
        private int pageCount = 0;// 总页数

        ///<summary>
        ///part2_8 单品 查询本地数据库 带分页
        ///</summary>
        private void button2_8_2_Click()
        {
            //SQL语句
            string sql_count = "SELECT count(*) AS COUNTS FROM Goods_Info";
            //执行查询，结果为DataTable类型
            DataTable dt_count = sqliteDBHelper_goodsDB.ExecuteDataTable(sql_count, null);
            // 记录总数
            totalCount = Convert.ToInt32(dt_count.Rows[0]["COUNTS"]);
            // 总页数
            pageCount = (int)Math.Ceiling((double)totalCount / pageSize);
            //SQL查询语句
            string sql = "SELECT Number AS value1,Name AS value2,Barcode AS value3,Price AS value4,RRP AS value5,Tax_Index AS value6,Stock_Control AS value7,Stock_Amount AS value8,Currency AS value9 FROM "
                + "(SELECT * FROM Goods_Info ORDER BY Number ASC LIMIT @pageSize*@pageIndex) LIMIT @pageSize offset @pageSize*@pageIndexbefore;";
            //配置SQL语句里的参数
            SQLiteParameter[] parameters = {
                    new SQLiteParameter("@pageSize",pageSize),
                    new SQLiteParameter("@pageIndex",pageIndex),
                    new SQLiteParameter("@pageIndexbefore",pageIndex-1),
                };
            //执行查询，结果为DataTable类型
            DataTable dt = sqliteDBHelper_goodsDB.ExecuteDataTable(sql, parameters);
            //查询税率
            string sql_tax = "SELECT Number,Tax_Code,Tax_Name,Tax_Rate FROM Tax_Tariff ORDER BY Number ASC";
            //执行查询
            DataTable dt_tax = sqliteDBHelper_systemDB.ExecuteDataTable(sql_tax, null);
            //判断查询结果是否为0行
            if (dt.Rows.Count == 0)
            {
                dataGrid2_8.ItemsSource = null;//先清空表格内容
                DataTable dt_temp = new DataTable();//新建临时表
                dt_temp.Columns.Add(new DataColumn("No data！"));//添加列
                                                                //dt_temp.Rows.Add(dt_temp.NewRow());//添加行
                                                                //dt_temp.Rows[0]["Tip"] = "No result！";//设置行内容
                dataGrid2_8.ItemsSource = dt_temp.DefaultView;//把表格放到控件中
                return;
            }
            //添加新的字段
            dt.Columns.Add(new DataColumn("Number", Type.GetType("System.Decimal")));
            dt.Columns.Add(new DataColumn("Name"));
            dt.Columns.Add(new DataColumn("Barcode"));
            dt.Columns.Add(new DataColumn("Price", Type.GetType("System.Decimal")));
            dt.Columns.Add(new DataColumn("RRP", Type.GetType("System.Decimal")));
            dt.Columns.Add(new DataColumn("Tax Index"));//, Type.GetType("System.Decimal")
            dt.Columns.Add(new DataColumn("Tax Code"));
            dt.Columns.Add(new DataColumn("Tax Name"));
            dt.Columns.Add(new DataColumn("Tax Rate"));
            dt.Columns.Add(new DataColumn("Stock Control"));
            dt.Columns.Add(new DataColumn("Stock Amount", Type.GetType("System.Decimal")));
            dt.Columns.Add(new DataColumn("Currency"));
            int j;
            DataRow[] dr;
            //以下为给新添加的中文字段赋值
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dt.Rows[i]["Number"] = dt.Rows[i]["value1"];
                dt.Rows[i]["Name"] = dt.Rows[i]["value2"].ToString();
                dt.Rows[i]["Barcode"] = dt.Rows[i]["value3"];
                //Console.WriteLine("Barcode=" + dt.Rows[i]["value3"].ToString());
                dt.Rows[i]["Price"] = Convert.ToDouble(dt.Rows[i]["value4"].ToString()) / 100;
                dt.Rows[i]["RRP"] = Convert.ToDouble(dt.Rows[i]["value5"].ToString()) / 100;
                dt.Rows[i]["Tax Index"] = dt.Rows[i]["value6"].ToString();
                //Console.WriteLine("Tax Index=" + dt.Rows[i]["value6"].ToString());
                //如果该商品的税率索引为0，则税率索引为空,因为数据库中该字段为NUMERIC类型，所以当该字段数据为空，查出来的时候默认是0
                if (dt.Rows[i]["value6"].ToString().Equals("0"))
                {
                    dt.Rows[i]["Tax Index"] = "0";
                }
                //如果该商品的税率索引不为0，则添加税率信息,因为数据库中该字段为NUMERIC类型，所以当该字段数据为空，查出来的时候默认是0
                else
                {
                    //根据Tax Index的值在dt_tax表中查找那一行的数据
                    dr = dt_tax.Select("Number='" + dt.Rows[i]["value6"].ToString() + "'");
                    j = dt_tax.Rows.IndexOf(dr[0]);//Number为dt_tax表中第一个字段
                    dt.Rows[i]["Tax Code"] = dt_tax.Rows[j]["Tax_Code"].ToString();
                    dt.Rows[i]["Tax Name"] = dt_tax.Rows[j]["Tax_Name"].ToString();
                    dt.Rows[i]["Tax Rate"] = (Convert.ToDouble(dt_tax.Rows[j]["Tax_Rate"].ToString()) / 10000).ToString("0.00%");
                    dr = null;
                }
                dt.Rows[i]["Stock Control"] = dt.Rows[i]["value7"].ToString();
                dt.Rows[i]["Stock Amount"] = Convert.ToDouble(dt.Rows[i]["value8"].ToString()) / 10000;
                dt.Rows[i]["Currency"] = dt.Rows[i]["value9"].ToString();
            }
            dataGrid2_8.ItemsSource = dt.DefaultView;
            //count是dt表格中前若干列标题是英文的字段数，也就是select查询的的字段数
            int count = 9;
            //将英文字段隐藏掉
            for (int i = 0; i < count; i++)
            {
                this.dataGrid2_8.Columns[i].Visibility = System.Windows.Visibility.Hidden;
            }
            //以下为设置字段宽度
            this.dataGrid2_8.Columns[count++].Width = 70;
            this.dataGrid2_8.Columns[count++].Width = 90;
            this.dataGrid2_8.Columns[count++].Width = 90;
            this.dataGrid2_8.Columns[count++].Width = 60;
            this.dataGrid2_8.Columns[count++].Width = 60;
            this.dataGrid2_8.Columns[count++].Width = 70;
            this.dataGrid2_8.Columns[count++].Width = 70;
            this.dataGrid2_8.Columns[count++].Width = 100;
            this.dataGrid2_8.Columns[count++].Width = 80;
            this.dataGrid2_8.Columns[count++].Width = 100;
            this.dataGrid2_8.Columns[count++].Width = 110;
            this.dataGrid2_8.Columns[count++].Width = 80;
        }
        ///<summary>
        ///part2_8 单品 删除
        ///</summary>
        private void button2_8_3_Click(object sender, RoutedEventArgs e)
        {
            //弹出提示框，未选择要删除的数据
            if (dataGrid2_8.SelectedItem == null)
            {
                MessageBox.Show("Please select one record to delete first!", "Information", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            //选择了某一行数据时
            else
            {
                //获取选中行所有数据
                DataRowView mySelectedElement = (DataRowView)dataGrid2_8.SelectedItem;
                //获取必要信息
                string Number = mySelectedElement.Row["Number"].ToString();
                //查看部类商品关联表 此商品是否被关联
                string sql_count = "SELECT COUNT(*) AS COUNTS FROM Department_Associate WHERE PLU_No=@PLU_No";
                //配置SQL语句里的参数
                SQLiteParameter[] parameter_count = {
                    new SQLiteParameter("@PLU_No",Number),
                        };
                //执行查询，结果为DataTable类型
                DataTable dt_count = sqliteDBHelper_goodsDB.ExecuteDataTable(sql_count, parameter_count);
                //判断结果,无部类关联
                if (Convert.ToInt32(dt_count.Rows[0]["COUNTS"]) == 0)//若数量为0
                {
                    //弹出确认框，当为确定时
                    if (MessageBox.Show("Confirm that you want to delete the record ( the Number:  " + mySelectedElement.Row["Number"].ToString() + " )？", "Information", MessageBoxButton.OKCancel, MessageBoxImage.Information) == MessageBoxResult.OK)
                    {
                        //SQL语句
                        //string sql = "DELETE FROM Goods_Info WHERE Number=@Number";
                        string sql = "UPDATE Goods_Info SET Name='',Barcode='',Price=0,Tax_Index=0,RRP=0,Stock_Control=0,Stock_Amount=0,Currency='',Used=0 WHERE Number=@Number";
                        //配置SQL语句里的参数
                        SQLiteParameter[] parameters = {
                            new SQLiteParameter("@Number",Number)
                        };
                        //执行SQL，并做判断
                        if (sqliteDBHelper_goodsDB.ExecuteNonQuery(sql, parameters) == 1)
                        {
                            //刷新查询框内容
                            button2_8_2_Click();
                            //设置分页按钮是否可用
                            SetPageButtonEnabled();
                            //设置控件显示信息
                            SetPagerInfo(pageIndex, pageSize, pageCount, totalCount);
                        }
                        else
                        {
                            MessageBox.Show("Delete unsuccessfully!", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                else//有部类关联
                {
                    //弹出确认框，当为确定时
                    if (MessageBox.Show("Confirm that you want to delete the record ( the Number:  " + mySelectedElement.Row["Number"].ToString() + " ) and the the relationship in Department-Associate that you have set in Department-Setting before？", "Information", MessageBoxButton.OKCancel, MessageBoxImage.Information) == MessageBoxResult.OK)
                    {
                        //SQL语句
                        string sql = "DELETE FROM Goods_Info WHERE Number=@Number";
                        //配置SQL语句里的参数
                        SQLiteParameter[] parameters = {
                            new SQLiteParameter("@Number",Number)
                        };
                        //执行SQL，并做判断
                        if (sqliteDBHelper_goodsDB.ExecuteNonQuery(sql, parameters) == 1)
                        {
                            //刷新查询框内容
                            button2_8_2_Click();
                            //设置分页按钮是否可用
                            SetPageButtonEnabled();
                            //设置控件显示信息
                            SetPagerInfo(pageIndex, pageSize, pageCount, totalCount);
                        }
                        else
                        {
                            MessageBox.Show("Delete this good unsuccessfully!", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        //删除部类管理
                        string sql_update = "UPDATE Department_Associate SET PLU_No='0' WHERE PLU_No=@PLU_No";
                        //配置SQL语句里的参数
                        SQLiteParameter[] parameter_update = {
                                new SQLiteParameter("@PLU_No",Number),
                            };
                        //执行查询，结果为DataTable类型
                        ;
                        if (sqliteDBHelper_goodsDB.ExecuteNonQuery(sql_update, parameter_update) >= 1)
                        {
                            //MessageBox.Show("This good was set in Department Associate before, the relationship has been lifted successfully now!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("The relationship in Department-Associate is lifted unsuccessfully!", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
        }
        ///<summary>
        ///part2_8 单品 修改本地数据库
        ///</summary>
        private void button2_8_4_Click(object sender, RoutedEventArgs e)
        {
            //弹出提示框，未选择要删除的数据
            if (dataGrid2_8.SelectedItem == null)
            {
                MessageBox.Show("Please select one of the data you want to modify!", "Information", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            //选择了某一行数据时
            else
            {
                //获取选中行所有数据
                DataRowView mySelectedElement = (DataRowView)dataGrid2_8.SelectedItem;
                //声明一个变量
                Plu plu = new Plu();
                //变量赋值
                plu.Number = mySelectedElement.Row["Number"].ToString();
                plu.Name = mySelectedElement.Row["Name"].ToString();
                plu.Barcode = mySelectedElement.Row["Barcode"].ToString();
                plu.Price = Convert.ToDouble(mySelectedElement.Row["Price"].ToString());
                plu.RRP = Convert.ToDouble(mySelectedElement.Row["RRP"].ToString());
                plu.Tax_Index = mySelectedElement.Row["Tax Index"].ToString();
                plu.Stock_Control = Convert.ToInt32(mySelectedElement.Row["Stock Control"]);
                plu.Stock_Amount = Convert.ToDouble(mySelectedElement.Row["Stock Amount"].ToString());
                plu.Tax_Code = mySelectedElement.Row["Tax Code"].ToString();
                //声明一个修改信息的窗体
                Update_Plu upd_plu = new Update_Plu(plu, goodsDB, systemDB, programmingDB);
                //弹出窗体
                upd_plu.ShowDialog();
                //刷新查询框内容
                button2_8_2_Click();
                //设置分页按钮是否可用
                SetPageButtonEnabled();
                //设置控件显示信息
                SetPagerInfo(pageIndex, pageSize, pageCount, totalCount);
            }
        }
        /// <summary>
        /// 设置分页按钮是否可用
        /// </summary>
        private void SetPageButtonEnabled()
        {
            //确定分页按钮的是否可用
            if (pageCount <= 1)
            {
                btnPageDown.IsEnabled = false;
                btnPageUp.IsEnabled = false;
                btnEndPage.IsEnabled = false;
            }
            else
            {
                if (pageIndex == pageCount)
                {
                    btnPageDown.IsEnabled = false;
                    btnPageUp.IsEnabled = true;
                    btnEndPage.IsEnabled = false;
                }
                else if (pageIndex <= 1)
                {
                    btnPageDown.IsEnabled = true;
                    btnPageUp.IsEnabled = false;
                    btnEndPage.IsEnabled = true;
                }
                else
                {
                    btnPageDown.IsEnabled = true;
                    btnPageUp.IsEnabled = true;
                    btnEndPage.IsEnabled = true;
                }
            }
        }
        /// <summary>
        /// 设置控件显示信息
        /// </summary>
        /// <param name="pageIndex">当前页码</param>
        /// <param name="pageCount">共有页数</param>
        /// <param name="totalCount">总记录条数</param>
        private void SetPagerInfo(int pageIndex, int pageSize, int pageCount, int totalCount)
        {
            //PER PAGE: 15   ALL: 5   PAGE: 1 / 1
            //txtPagerInfo.Text = String.Format("The current page is【{0}】, each page shows【{1}】records, a total of【{2}】page, a total of【{3}】records. ", pageIndex, pageSize, pageCount, totalCount);
            txtPagerInfo.Text = String.Format("PER PAGE: {0}   ALL: {1}   PAGE: {2} / {3} ", pageSize, totalCount, pageIndex, pageCount);
        }
        /// <summary>
        /// 首页按钮事件
        /// </summary>
        private void btnFirstPage_Click(object sender, RoutedEventArgs e)
        {
            pageIndex = 1;
            button2_8_2_Click();//查询 设置 pageCount totalCount ；显示数据
            SetPageButtonEnabled();//设置分页按钮是否可用
            SetPagerInfo(pageIndex, pageSize, pageCount, totalCount);//设置控件显示信息
        }
        /// <summary>
        /// 下一页按钮事件
        /// </summary>
        private void btnPageDown_Click(object sender, RoutedEventArgs e)
        {
            pageIndex++;
            button2_8_2_Click();//查询 设置 pageCount totalCount ；显示数据
            SetPageButtonEnabled();//设置分页按钮是否可用
            SetPagerInfo(pageIndex, pageSize, pageCount, totalCount);//设置控件显示信息
        }
        /// <summary>
        /// 上一页按钮事件
        /// </summary>
        private void btnPageUp_Click(object sender, RoutedEventArgs e)
        {
            pageIndex--;
            button2_8_2_Click();//查询 设置 pageCount totalCount ；显示数据
            SetPageButtonEnabled();//设置分页按钮是否可用
            SetPagerInfo(pageIndex, pageSize, pageCount, totalCount);//设置控件显示信息
        }
        /// <summary>
        /// 尾页按钮事件
        /// </summary>
        private void btnEndPage_Click(object sender, RoutedEventArgs e)
        {
            pageIndex = pageCount;
            button2_8_2_Click();//查询 设置 pageCount totalCount ；显示数据
            SetPageButtonEnabled();//设置分页按钮是否可用
            SetPagerInfo(pageIndex, pageSize, pageCount, totalCount);//设置控件显示信息
        }
        ///<summary>
        ///part2_8 单品 按条件查询
        ///</summary>
        private void button2_8_5_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //声明一个添加信息的窗体
                Good_Query good = new Good_Query(goodsDB, systemDB, programmingDB);
                //弹出窗体
                good.ShowDialog();
                //从第一页开始查
                pageIndex = 1;
                //刷新查询框内容
                button2_8_2_Click();
                //设置分页按钮是否可用
                SetPageButtonEnabled();
                //设置控件显示信息
                SetPagerInfo(pageIndex, pageSize, pageCount, totalCount);
            }
            catch (Exception ee)
            {
                MessageBox.Show("The operation failed, please try again. Possible causes:" + ee.Message, "Information", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        ///<summary>
        ///part2_8 单品 导入
        ///</summary>
        private void button2_8_6_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //声明一个添加信息的窗体
                OpenFile openFile = new OpenFile(goodsDB);
                //弹出窗体
                openFile.ShowDialog();
                //刷新查询框内容
                button2_8_2_Click();
                //设置分页按钮是否可用
                SetPageButtonEnabled();
                //设置控件显示信息
                SetPagerInfo(pageIndex, pageSize, pageCount, totalCount);
            }
            catch (Exception ee)
            {
                MessageBox.Show("The operation failed, please try again. Possible causes:" + ee.Message, "Information", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        ///<summary>
        ///part2_8 单品 云端数据库  打开数据库文件夹
        ///</summary>
        private void button2_8_7_Click(object sender, RoutedEventArgs e)
        {
            //打开数据库文件夹
            string filePathAndName = Environment.CurrentDirectory + "\\database\\" + "goodsDB.db";
            if (!System.IO.File.Exists(filePathAndName))
            {
                MessageBox.Show("The file does not exist！", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            else
            {
                //System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
                //psi.Arguments = " /select," + "goodsDB.db";
                System.Diagnostics.Process.Start("explorer.exe", "/select," + filePathAndName);
            }
        }
        #endregion
        /////////////////////////////////////////////////////////////////////////////////
        #region part3_1 日报表 本地数据库 在此说明一下 本程序中，变量totalNumber（totalAmount）指销售数量，totalMoney指销售金额；而在数据库中Quantity指销售数量，Amount指销售金额，请注意区分
        ///<summary>
        ///part3_1 日报表 查询本地数据库
        ///</summary>
        private void button3_1_1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                button3_1_1_Click();
            }
            catch (Exception ee)
            {
                MessageBox.Show("The operation failed, please try again. Possible causes:" + ee.Message, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        ///<summary>
        ///part3_1 日报表 查询本地数据库
        ///</summary>
        private void button3_1_1_Click()
        {
            if (ComboBox3_1.SelectionBoxItem.ToString().Equals("部类日报表"))
            {
                #region part3_1 日报表 部类日报表
                //获取查询参数：时间、Z号码
                Day_Department day_dep = new Day_Department(dmDB);
                day_dep.ShowDialog();
                //若没有输入任何内容则退出
                if (day_dep.Date_Time.Equals("") || day_dep.Znumber.Equals(""))
                {
                    return;
                }
                //SQL查询语句
                string sql = "SELECT Dept_Index, Name, SUM(Item_Sum) AS totalMoney, SUM(Quantity) AS totalNumber FROM Sales_Item WHERE Date_Time >=@Date_TimeStart AND Date_Time <=@Date_TimeEnd "
                    + "AND Z_Number = @Z_Number GROUP BY Dept_Index  ORDER BY Dept_Index ASC";
                //配置SQL查询语句里的参数
                SQLiteParameter[] parameters = {
                    new SQLiteParameter("@Date_TimeStart",day_dep.Date_TimeStart),
                    new SQLiteParameter("@Date_TimeEnd",day_dep.Date_TimeEnd),
                    new SQLiteParameter("@Z_Number",day_dep.Znumber)
                };
                //执行查询，结果为DataTable类型
                DataTable dt = sqliteDBHelper_dmDB.ExecuteDataTable(sql, parameters);
                //判断查询结果是否为0行
                if (dt.Rows.Count == 0)
                {
                    //弹出提示框
                    //MessageBox.Show("No result!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    dataGrid3_1.ItemsSource = null;//先清空表格内容
                    DataTable dt_temp = new DataTable();//新建临时表
                    dt_temp.Columns.Add(new DataColumn("Tip"));//添加列
                    dt_temp.Rows.Add(dt_temp.NewRow());//添加行
                    dt_temp.Rows[0]["Tip"] = "No result！";//设置行内容
                    dataGrid3_1.ItemsSource = dt_temp.DefaultView;//把表格放到控件中
                    return;
                }
                //查询出来的表格的字段名为英文，但显示给用户的时候要为中文，所以在此添加若干个中文字段名显示给用户
                dt.Columns.Add(new DataColumn("部门编号"));
                dt.Columns.Add(new DataColumn("部门名称"));
                dt.Columns.Add(new DataColumn("总销售量"));
                dt.Columns.Add(new DataColumn("总销售额"));
                dt.Columns.Add(new DataColumn("时间"));
                dt.Columns.Add(new DataColumn("Z号码"));
                //以下为给新添加的中文字段赋值
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["部门编号"] = dt.Rows[i]["Dept_Index"];
                    dt.Rows[i]["部门名称"] = dt.Rows[i]["Name"];
                    dt.Rows[i]["总销售量"] = dt.Rows[i]["totalNumber"];
                    dt.Rows[i]["总销售额"] = dt.Rows[i]["totalMoney"];
                    dt.Rows[i]["时间"] = day_dep.Date_Time;
                    dt.Rows[i]["Z号码"] = day_dep.Znumber;
                    //DateTime.Now.ToLongDateString().ToString() 或者 DateTime.Now.ToString("yyyy年MM月dd日")
                }
                //string result = JsonConvert.SerializeObject(dt);//将DataTable类型的变量序列化成json字符串
                //Console.WriteLine(result);
                dataGrid3_1.ItemsSource = dt.DefaultView;
                //以下为设置前若干个标题是英文字段 隐藏， 数量为select查询的的字段数
                int count = 4;//设置查询结果的字段数
                for (int i = 0; i < count; i++)
                {
                    this.dataGrid3_1.Columns[i].Visibility = System.Windows.Visibility.Hidden;
                }
                //以下为设置字段宽度
                this.dataGrid3_1.Columns[count++].Width = 90;
                this.dataGrid3_1.Columns[count++].Width = 150;
                this.dataGrid3_1.Columns[count++].Width = 150;
                this.dataGrid3_1.Columns[count++].Width = 150;
                this.dataGrid3_1.Columns[count++].Width = 150;
                this.dataGrid3_1.Columns[count++].Width = 90;
                #endregion
            }
            else if (ComboBox3_1.SelectionBoxItem.ToString().Equals("营业员支付总额日报表"))
            {
                #region part3_1 日报表 营业员支付总额日报表
                //获取查询参数：时间
                Day_Moment day_mom = new Day_Moment();
                //弹出对话框
                day_mom.ShowDialog();
                //若没有输入任何内容则退出
                if (day_mom.Date_Time.Equals(""))
                {
                    return;
                }
                //SQL查询语句
                string sql = "SELECT strftime ( '%Y-%m-%d %H', Date_Time, 'unixepoch', 'localtime' ) AS YMDH, SUM(Item_Sum) AS totalMoney, SUM(Quantity) AS totalNumber FROM Sales_Item WHERE Date_Time >= @Date_TimeStart AND Date_Time <= @Date_TimeEnd GROUP BY strftime ( '%Y-%m-%d %H', Date_Time, 'unixepoch', 'localtime' )";
                //配置SQL查询语句里的参数
                SQLiteParameter[] parameters = {
                    new SQLiteParameter("@Date_TimeStart",day_mom.Date_TimeStart),
                    new SQLiteParameter("@Date_TimeEnd",day_mom.Date_TimeEnd),
                };
                //执行查询，结果为DataTable类型
                DataTable dt = sqliteDBHelper_dmDB.ExecuteDataTable(sql, parameters);
                //判断查询结果是否为0行
                if (dt.Rows.Count == 0)
                {
                    dataGrid3_1.ItemsSource = null;//先清空表格内容
                    DataTable dt_temp = new DataTable();//新建临时表
                    dt_temp.Columns.Add(new DataColumn("Tip"));//添加列
                    dt_temp.Rows.Add(dt_temp.NewRow());//添加行
                    dt_temp.Rows[0]["Tip"] = "No result！";//设置行内容
                    dataGrid3_1.ItemsSource = dt_temp.DefaultView;//把表格放到控件中
                    return;
                }
                //查询出来的表格的字段名为英文，但显示给用户的时候要为中文，所以在此添加若干个中文字段名显示给用户
                dt.Columns.Add(new DataColumn("序号"));
                //dt.Columns.Add(new DataColumn("机器编号"));
                //dt.Columns.Add(new DataColumn("日期"));
                dt.Columns.Add(new DataColumn("时间段（09表示：9点-10点时间段）"));
                dt.Columns.Add(new DataColumn("总销售量"));
                dt.Columns.Add(new DataColumn("总销售额"));
                //以下为给新添加的中文字段赋值
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["序号"] = i + 1;
                    //dt.Rows[i]["机器编号"] = dt.Rows[i]["MachineId"];
                    //dt.Rows[i]["日期"] = day_mom.Date_Time;
                    dt.Rows[i]["时间段（09表示：9点-10点时间段）"] = dt.Rows[i]["YMDH"];
                    dt.Rows[i]["总销售量"] = Convert.ToDouble(dt.Rows[i]["totalNumber"]) / 10000;
                    dt.Rows[i]["总销售额"] = Convert.ToDouble(dt.Rows[i]["totalMoney"]) / 100;
                    //DateTime.Now.ToLongDateString().ToString() 或者 DateTime.Now.ToString("yyyy年MM月dd日")
                }
                dataGrid3_1.ItemsSource = dt.DefaultView;
                //以下为设置前若干个标题是英文字段 隐藏， 数量为select查询的的字段数
                int count = 3;//设置查询结果的字段数
                for (int i = 0; i < count; i++)
                {
                    this.dataGrid3_1.Columns[i].Visibility = System.Windows.Visibility.Hidden;
                }
                //以下为设置字段宽度
                this.dataGrid3_1.Columns[count++].Width = 100;
                //this.dataGrid3_1.Columns[count++].Width = 120;
                //this.dataGrid3_1.Columns[count++].Width = 120;
                this.dataGrid3_1.Columns[count++].Width = 350;
                this.dataGrid3_1.Columns[count++].Width = 100;
                this.dataGrid3_1.Columns[count++].Width = 100;
                #endregion
            }
            else if (ComboBox3_1.SelectionBoxItem.ToString().Equals("时段报表"))
            {
                #region part3_1 日报表 时段报表 ok
                //获取查询参数：时间
                Day_Moment day_mom = new Day_Moment();
                //弹出对话框
                day_mom.ShowDialog();
                //若没有输入任何内容则退出
                if (day_mom.Date_Time.Equals(""))
                {
                    return;
                }
                //SQL查询语句
                string sql = "SELECT strftime ( '%Y-%m-%d %H', Date_Time, 'unixepoch', 'localtime' ) AS YMDH, SUM(Item_Sum) AS totalMoney, SUM(Quantity) AS totalNumber FROM Sales_Item WHERE Date_Time >= @Date_TimeStart AND Date_Time <= @Date_TimeEnd GROUP BY strftime ( '%Y-%m-%d %H', Date_Time, 'unixepoch', 'localtime' )";
                //配置SQL查询语句里的参数
                SQLiteParameter[] parameters = {
                    new SQLiteParameter("@Date_TimeStart",day_mom.Date_TimeStart),
                    new SQLiteParameter("@Date_TimeEnd",day_mom.Date_TimeEnd),
                };
                //执行查询，结果为DataTable类型
                DataTable dt = sqliteDBHelper_dmDB.ExecuteDataTable(sql, parameters);
                //判断查询结果是否为0行
                if (dt.Rows.Count == 0)
                {
                    dataGrid3_1.ItemsSource = null;//先清空表格内容
                    DataTable dt_temp = new DataTable();//新建临时表
                    dt_temp.Columns.Add(new DataColumn("Tip"));//添加列
                    dt_temp.Rows.Add(dt_temp.NewRow());//添加行
                    dt_temp.Rows[0]["Tip"] = "No result！";//设置行内容
                    dataGrid3_1.ItemsSource = dt_temp.DefaultView;//把表格放到控件中
                    return;
                }
                //查询出来的表格的字段名为英文，但显示给用户的时候要为中文，所以在此添加若干个中文字段名显示给用户
                dt.Columns.Add(new DataColumn("序号"));
                //dt.Columns.Add(new DataColumn("机器编号"));
                //dt.Columns.Add(new DataColumn("日期"));
                dt.Columns.Add(new DataColumn("时间段（09表示：9点-10点时间段）"));
                dt.Columns.Add(new DataColumn("总销售量"));
                dt.Columns.Add(new DataColumn("总销售额"));
                //以下为给新添加的中文字段赋值
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["序号"] = i + 1;
                    //dt.Rows[i]["机器编号"] = dt.Rows[i]["MachineId"];
                    //dt.Rows[i]["日期"] = day_mom.Date_Time;
                    dt.Rows[i]["时间段（09表示：9点-10点时间段）"] = dt.Rows[i]["YMDH"];
                    dt.Rows[i]["总销售量"] = Convert.ToDouble(dt.Rows[i]["totalNumber"]) / 10000;
                    dt.Rows[i]["总销售额"] = Convert.ToDouble(dt.Rows[i]["totalMoney"]) / 100;
                    //DateTime.Now.ToLongDateString().ToString() 或者 DateTime.Now.ToString("yyyy年MM月dd日")
                }
                dataGrid3_1.ItemsSource = dt.DefaultView;
                //以下为设置前若干个标题是英文字段 隐藏， 数量为select查询的的字段数
                int count = 3;//设置查询结果的字段数
                for (int i = 0; i < count; i++)
                {
                    this.dataGrid3_1.Columns[i].Visibility = System.Windows.Visibility.Hidden;
                }
                //以下为设置字段宽度
                this.dataGrid3_1.Columns[count++].Width = 100;
                //this.dataGrid3_1.Columns[count++].Width = 120;
                //this.dataGrid3_1.Columns[count++].Width = 120;
                this.dataGrid3_1.Columns[count++].Width = 350;
                this.dataGrid3_1.Columns[count++].Width = 100;
                this.dataGrid3_1.Columns[count++].Width = 100;
                #endregion
            }
            else if (ComboBox3_1.SelectionBoxItem.ToString().Equals("营业员取消日报表"))
            {
                #region part3_1 日报表 营业员取消日报表
                //获取查询参数：时间
                Day_Moment day_mom = new Day_Moment();
                //弹出对话框
                day_mom.ShowDialog();
                //若没有输入任何内容则退出
                if (day_mom.Date_Time.Equals(""))
                {
                    return;
                }
                //SQL查询语句
                string sql_mysql = "SELECT b.EJ_No, c.`Name`, b.totalAmount, b.totalMoney FROM ( SELECT a.EJ_No, SUM( a.Cancellation_Total_Amount ) AS totalAmount, "
                    + "SUM(a.Cancellation_Total_QTY) AS totalMoney FROM z_report_data AS a WHERE a.UserId = @UserId AND DATE_FORMAT(a.Date_Time, '%Y-%m-%d') = @Date_Time "
                    + "GROUP BY a.EJ_No ) AS b, cashier_table AS c WHERE b.EJ_No = c.Number ORDER BY b.EJ_No ASC";
                //配置SQL查询语句里的参数
                SQLiteParameter[] parameters = {
                    new SQLiteParameter("@Date_Time",day_mom.Date_Time),
                };
                //执行查询，结果为DataTable类型
                DataTable dt = sqliteDBHelper_dmDB.ExecuteDataTable(sql_mysql, parameters);
                //判断查询结果是否为0行
                if (dt.Rows.Count == 0)
                {
                    dataGrid3_1.ItemsSource = null;//先清空表格内容
                    DataTable dt_temp = new DataTable();//新建临时表
                    dt_temp.Columns.Add(new DataColumn("Tip"));//添加列
                    dt_temp.Rows.Add(dt_temp.NewRow());//添加行
                    dt_temp.Rows[0]["Tip"] = "No result！";//设置行内容
                    dataGrid3_1.ItemsSource = dt_temp.DefaultView;//把表格放到控件中
                    return;
                }
                //查询出来的表格的字段名为英文，但显示给用户的时候要为中文，所以在此添加若干个中文字段名显示给用户
                dt.Columns.Add(new DataColumn("收银员编号"));
                dt.Columns.Add(new DataColumn("收银员名称"));
                dt.Columns.Add(new DataColumn("日期"));
                dt.Columns.Add(new DataColumn("总取消数量"));
                dt.Columns.Add(new DataColumn("总取消金额"));
                //以下为给新添加的中文字段赋值
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["收银员编号"] = dt.Rows[i]["EJ_No"];
                    dt.Rows[i]["收银员名称"] = dt.Rows[i]["Name"];
                    dt.Rows[i]["日期"] = day_mom.Date_Time;
                    dt.Rows[i]["总取消数量"] = dt.Rows[i]["totalAmount"];
                    dt.Rows[i]["总取消金额"] = dt.Rows[i]["totalMoney"];
                    //DateTime.Now.ToLongDateString().ToString() 或者 DateTime.Now.ToString("yyyy年MM月dd日")
                }
                dataGrid3_1.ItemsSource = dt.DefaultView;
                //以下为设置前若干个标题是英文字段 隐藏， 数量为select查询的的字段数
                int count = 4;//设置查询结果的字段数
                for (int i = 0; i < count; i++)
                {
                    this.dataGrid3_1.Columns[i].Visibility = System.Windows.Visibility.Hidden;
                }
                //以下为设置字段宽度
                this.dataGrid3_1.Columns[count++].Width = 120;
                this.dataGrid3_1.Columns[count++].Width = 120;
                this.dataGrid3_1.Columns[count++].Width = 120;
                this.dataGrid3_1.Columns[count++].Width = 120;
                this.dataGrid3_1.Columns[count++].Width = 100;
                #endregion
            }
            else if (ComboBox3_1.SelectionBoxItem.ToString().Equals("营业员退货日报表"))
            {
                #region part3_1 日报表 营业员退货日报表
                //获取查询参数：时间
                Day_Moment day_mom = new Day_Moment();
                //弹出对话框
                day_mom.ShowDialog();
                //若没有输入任何内容则退出
                if (day_mom.Date_Time.Equals(""))
                {
                    return;
                }
                //SQL查询语句
                string sql_mysql = "SELECT b.EJ_No, c.`Name`, b.totalAmount, b.totalMoney FROM ( SELECT a.EJ_No, SUM( a.Decrease_Total_Amount ) AS totalAmount, "
                    + "SUM(a.Decrease_Total_QTY) AS totalMoney FROM z_report_data AS a WHERE a.UserId = @UserId AND DATE_FORMAT(a.Date_Time, '%Y-%m-%d') = @Date_Time "
                    + "GROUP BY a.EJ_No ) AS b, cashier_table AS c WHERE b.EJ_No = c.Number ORDER BY b.EJ_No ASC";
                //配置SQL查询语句里的参数
                SQLiteParameter[] parameters = {
                    new SQLiteParameter("@Date_Time",day_mom.Date_Time),
                };
                //执行查询，结果为DataTable类型
                DataTable dt = sqliteDBHelper_dmDB.ExecuteDataTable(sql_mysql, parameters);
                //判断查询结果是否为0行
                if (dt.Rows.Count == 0)
                {
                    //弹出Tip框
                    //MessageBox.Show("No result!", "Tip", MessageBoxButton.OK, MessageBoxImage.Information);
                    dataGrid3_1.ItemsSource = null;//先清空表格内容
                    DataTable dt_temp = new DataTable();//新建临时表
                    dt_temp.Columns.Add(new DataColumn("Tip"));//添加列
                    dt_temp.Rows.Add(dt_temp.NewRow());//添加行
                    dt_temp.Rows[0]["Tip"] = "No result！";//设置行内容
                    dataGrid3_1.ItemsSource = dt_temp.DefaultView;//把表格放到控件中
                    return;
                }
                //查询出来的表格的字段名为英文，但显示给用户的时候要为中文，所以在此添加若干个中文字段名显示给用户
                dt.Columns.Add(new DataColumn("收银员编号"));
                dt.Columns.Add(new DataColumn("收银员名称"));
                dt.Columns.Add(new DataColumn("日期"));
                dt.Columns.Add(new DataColumn("总退货数量"));
                dt.Columns.Add(new DataColumn("总退货金额"));
                //以下为给新添加的中文字段赋值
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["收银员编号"] = dt.Rows[i]["EJ_No"];
                    dt.Rows[i]["收银员名称"] = dt.Rows[i]["Name"];
                    dt.Rows[i]["日期"] = day_mom.Date_Time;
                    dt.Rows[i]["总退货数量"] = dt.Rows[i]["totalAmount"];
                    dt.Rows[i]["总退货金额"] = dt.Rows[i]["totalMoney"];
                    //DateTime.Now.ToLongDateString().ToString() 或者 DateTime.Now.ToString("yyyy年MM月dd日")
                }
                //string result = JsonConvert.SerializeObject(dt);//将DataTable类型的变量序列化成json字符串
                //Console.WriteLine(result);
                dataGrid3_1.ItemsSource = dt.DefaultView;
                //以下为设置前若干个标题是英文字段 隐藏， 数量为select查询的的字段数
                int count = 4;//设置查询结果的字段数
                for (int i = 0; i < count; i++)
                {
                    this.dataGrid3_1.Columns[i].Visibility = System.Windows.Visibility.Hidden;
                }
                //以下为设置字段宽度
                this.dataGrid3_1.Columns[count++].Width = 120;
                this.dataGrid3_1.Columns[count++].Width = 120;
                this.dataGrid3_1.Columns[count++].Width = 120;
                this.dataGrid3_1.Columns[count++].Width = 120;
                this.dataGrid3_1.Columns[count++].Width = 100;
                #endregion
            }
            else if (ComboBox3_1.SelectionBoxItem.ToString().Equals("营业员更正日报表"))
            {
                #region part3_1 日报表 营业员更正日报表
                //获取查询参数：时间
                Day_Moment day_mom = new Day_Moment();
                //弹出对话框
                day_mom.ShowDialog();
                //若没有输入任何内容则退出
                if (day_mom.Date_Time.Equals(""))
                {
                    return;
                }
                //SQL查询语句
                string sql_mysql = "SELECT b.EJ_No, c.`Name`, b.totalAmount, b.totalMoney FROM ( SELECT a.EJ_No, SUM( a.Error_Correction_Total_Amount ) AS totalAmount, "
                    + "SUM(a.Error_Correction_Total_QTY) AS totalMoney FROM z_report_data AS a WHERE a.UserId = @UserId AND DATE_FORMAT(a.Date_Time, '%Y-%m-%d') = @Date_Time "
                    + "GROUP BY a.EJ_No ) AS b, cashier_table AS c WHERE b.EJ_No = c.Number ORDER BY b.EJ_No ASC";
                //配置SQL查询语句里的参数
                SQLiteParameter[] parameters = {
                    new SQLiteParameter("@Date_Time",day_mom.Date_Time),
                };
                //执行查询，结果为DataTable类型
                DataTable dt = sqliteDBHelper_dmDB.ExecuteDataTable(sql_mysql, parameters);
                //判断查询结果是否为0行
                if (dt.Rows.Count == 0)
                {
                    //弹出提示框
                    //MessageBox.Show("No result!", "Tip", MessageBoxButton.OK, MessageBoxImage.Information);
                    dataGrid3_1.ItemsSource = null;//先清空表格内容
                    DataTable dt_temp = new DataTable();//新建临时表
                    dt_temp.Columns.Add(new DataColumn("Tip"));//添加列
                    dt_temp.Rows.Add(dt_temp.NewRow());//添加行
                    dt_temp.Rows[0]["Tip"] = "No result！";//设置行内容
                    dataGrid3_1.ItemsSource = dt_temp.DefaultView;//把表格放到控件中
                    return;
                }
                //查询出来的表格的字段名为英文，但显示给用户的时候要为中文，所以在此添加若干个中文字段名显示给用户
                dt.Columns.Add(new DataColumn("收银员编号"));
                dt.Columns.Add(new DataColumn("收银员名称"));
                dt.Columns.Add(new DataColumn("日期"));
                dt.Columns.Add(new DataColumn("总更正数量"));
                dt.Columns.Add(new DataColumn("总更正金额"));
                //以下为给新添加的中文字段赋值
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["收银员编号"] = dt.Rows[i]["EJ_No"];
                    dt.Rows[i]["收银员名称"] = dt.Rows[i]["Name"];
                    dt.Rows[i]["日期"] = day_mom.Date_Time;
                    dt.Rows[i]["总更正数量"] = dt.Rows[i]["totalAmount"];
                    dt.Rows[i]["总更正金额"] = dt.Rows[i]["totalMoney"];
                }
                dataGrid3_1.ItemsSource = dt.DefaultView;
                //以下为设置前若干个标题是英文字段 隐藏， 数量为select查询的的字段数
                int count = 4;//设置查询结果的字段数
                for (int i = 0; i < count; i++)
                {
                    this.dataGrid3_1.Columns[i].Visibility = System.Windows.Visibility.Hidden;
                }
                //以下为设置字段宽度
                this.dataGrid3_1.Columns[count++].Width = 120;
                this.dataGrid3_1.Columns[count++].Width = 120;
                this.dataGrid3_1.Columns[count++].Width = 120;
                this.dataGrid3_1.Columns[count++].Width = 120;
                this.dataGrid3_1.Columns[count++].Width = 100;
                #endregion
            }
            else if (ComboBox3_1.SelectionBoxItem.ToString().Equals("税率日报表"))
            {
                #region part3_1 日报表 税率日报表
                //获取查询参数：时间
                Day_Moment day_mom = new Day_Moment();
                //弹出对话框
                day_mom.ShowDialog();
                //若没有输入任何内容则退出
                if (day_mom.Date_Time.Equals(""))
                {
                    return;
                }
                //SQL查询语句
                string sql_mysql = "SELECT ifnull(SUM(ifnull(Daily_Total_Sales,0)),0) AS totalMoney, ifnull(SUM(ifnull(Daily_Total_VAT,0)),0) AS totalVat FROM z_report_data WHERE UserId = @UserId AND DATE_FORMAT(Date_Time, '%Y-%m-%d') = @Date_Time";
                //配置SQL查询语句里的参数
                SQLiteParameter[] parameters = {
                    new SQLiteParameter("@Date_Time",day_mom.Date_Time),
                };
                //执行查询，结果为DataTable类型
                DataTable dt = sqliteDBHelper_dmDB.ExecuteDataTable(sql_mysql, parameters);
                //判断查询结果是否为0行
                if (dt.Rows.Count == 0)
                {
                    //弹出提示框
                    //MessageBox.Show("No result!", "Tip", MessageBoxButton.OK, MessageBoxImage.Information);
                    dataGrid3_1.ItemsSource = null;//先清空表格内容
                    DataTable dt_temp = new DataTable();//新建临时表
                    dt_temp.Columns.Add(new DataColumn("Tip"));//添加列
                    dt_temp.Rows.Add(dt_temp.NewRow());//添加行
                    dt_temp.Rows[0]["Tip"] = "No result！";//设置行内容
                    dataGrid3_1.ItemsSource = dt_temp.DefaultView;//把表格放到控件中
                    return;
                }
                //查询出来的表格的字段名为英文，但显示给用户的时候要为中文，所以在此添加若干个中文字段名显示给用户
                //dt.Columns.Add(new DataColumn("税率"));
                dt.Columns.Add(new DataColumn("日期"));
                dt.Columns.Add(new DataColumn("总额"));
                dt.Columns.Add(new DataColumn("税额"));
                //以下为给新添加的中文字段赋值
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    //dt.Rows[i]["税率"] = dt.Rows[i]["EJ_No"];
                    dt.Rows[i]["日期"] = day_mom.Date_Time;
                    dt.Rows[i]["总额"] = dt.Rows[i]["totalMoney"];
                    dt.Rows[i]["税额"] = dt.Rows[i]["totalVat"];
                }
                dataGrid3_1.ItemsSource = dt.DefaultView;
                //以下为设置前若干个标题是英文字段 隐藏， 数量为select查询的的字段数
                int count = 2;//设置查询结果的字段数
                for (int i = 0; i < count; i++)
                {
                    this.dataGrid3_1.Columns[i].Visibility = System.Windows.Visibility.Hidden;
                }
                //以下为设置字段宽度
                //this.dataGrid3_1.Columns[count++].Width = 120;
                this.dataGrid3_1.Columns[count++].Width = 120;
                this.dataGrid3_1.Columns[count++].Width = 120;
                this.dataGrid3_1.Columns[count++].Width = 120;
                #endregion
            }
        }
        #endregion
        /////////////////////////////////////////////////////////////////////////////////
        #region part3_2 周报表 本地数据库
        ///<summary>
        ///part3_2 周报表 查询本地数据库
        ///</summary>
        private void button3_2_1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                button3_2_1_Click();
            }
            catch (Exception ee)
            {
                MessageBox.Show("The operation failed, please try again. Possible causes:" + ee.Message, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }

        }
        ///<summary>
        ///part3_2 周报表 查询本地数据库
        ///</summary>
        private void button3_2_1_Click()
        {
            if (ComboBox3_2.SelectionBoxItem.ToString().Equals("部类周报表"))
            {
                #region part3_2 周报表 部类周报表
                //获取查询参数：时间、Z号码
                Week_Department week_dep = new Week_Department(dmDB);
                week_dep.ShowDialog();
                //若没有输入任何内容则退出
                if (week_dep.Date_TimeStart.Equals("") || week_dep.Date_TimeEnd.Equals("") || week_dep.Znumber.Equals(""))
                {
                    return;
                }
                //SQL查询语句  BETWEEN @Date_TimeStart AND @Date_TimeEnd
                string sql = "SELECT Dept_Index, Name, SUM(Item_Sum) AS totalMoney, SUM(Quantity) AS totalNumber FROM sales_item WHERE Date_Time >=@Date_TimeStart AND Date_Time <=@Date_TimeEnd "
                    + "AND Z_Number = @Z_Number GROUP BY Dept_Index  ORDER BY Dept_Index ASC";
                //配置SQL查询语句里的参数
                SQLiteParameter[] parameters = {
                    new SQLiteParameter("@Date_TimeStart",week_dep.Date_TimeStart),
                    new SQLiteParameter("@Date_TimeEnd",week_dep.Date_TimeEnd),
                    new SQLiteParameter("@Z_Number",week_dep.Znumber)
                };
                //执行查询，结果为DataTable类型
                DataTable dt = sqliteDBHelper_dmDB.ExecuteDataTable(sql, parameters);
                //判断查询结果是否为0行
                if (dt.Rows.Count == 0)
                {
                    //弹出提示框
                    //MessageBox.Show("No result!", "Tip", MessageBoxButton.OK, MessageBoxImage.Information);
                    dataGrid3_2.ItemsSource = null;//先清空表格内容
                    DataTable dt_temp = new DataTable();//新建临时表
                    dt_temp.Columns.Add(new DataColumn("Tip"));//添加列
                    dt_temp.Rows.Add(dt_temp.NewRow());//添加行
                    dt_temp.Rows[0]["Tip"] = "No result！";//设置行内容
                    dataGrid3_2.ItemsSource = dt_temp.DefaultView;//把表格放到控件中
                    return;
                }
                //查询出来的表格的字段名为英文，但显示给用户的时候要为中文，所以在此添加若干个中文字段名显示给用户
                dt.Columns.Add(new DataColumn("部门编号"));
                dt.Columns.Add(new DataColumn("部门名称"));
                dt.Columns.Add(new DataColumn("起始时间"));
                dt.Columns.Add(new DataColumn("结束时间"));
                dt.Columns.Add(new DataColumn("总销售量"));
                dt.Columns.Add(new DataColumn("总销售额"));
                dt.Columns.Add(new DataColumn("Z号码"));
                //以下为给新添加的中文字段赋值
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["部门编号"] = dt.Rows[i]["Dept_Index"];
                    dt.Rows[i]["部门名称"] = dt.Rows[i]["Name"];
                    dt.Rows[i]["起始时间"] = week_dep.Date_Time1;
                    dt.Rows[i]["结束时间"] = week_dep.Date_Time2;
                    dt.Rows[i]["总销售量"] = dt.Rows[i]["totalNumber"];
                    dt.Rows[i]["总销售额"] = dt.Rows[i]["totalMoney"];
                    dt.Rows[i]["Z号码"] = week_dep.Znumber;
                    //DateTime.Now.ToLongDateString().ToString() 或者 DateTime.Now.ToString("yyyy年MM月dd日")
                }
                //string result = JsonConvert.SerializeObject(dt);//将DataTable类型的变量序列化成json字符串
                //Console.WriteLine(result);
                dataGrid3_2.ItemsSource = dt.DefaultView;
                //以下为设置前若干个标题是英文字段 隐藏， 数量为select查询的的字段数
                int count = 4;//设置查询结果的字段数
                for (int i = 0; i < count; i++)
                {
                    this.dataGrid3_2.Columns[i].Visibility = System.Windows.Visibility.Hidden;
                }
                //以下为设置字段宽度
                this.dataGrid3_2.Columns[count++].Width = 80;
                this.dataGrid3_2.Columns[count++].Width = 150;
                this.dataGrid3_2.Columns[count++].Width = 180;
                this.dataGrid3_2.Columns[count++].Width = 180;
                this.dataGrid3_2.Columns[count++].Width = 120;
                this.dataGrid3_2.Columns[count++].Width = 120;
                this.dataGrid3_2.Columns[count++].Width = 90;
                #endregion
            }
            else if (ComboBox3_2.SelectionBoxItem.ToString().Equals("营业员支付总额周报表"))
            {

            }
            else if (ComboBox3_2.SelectionBoxItem.ToString().Equals("营业员取消周报表"))
            {
                #region part3_2 周报表 营业员取消周报表
                //获取查询参数：时间
                Moment_Cancel moment_can = new Moment_Cancel();
                //弹出对话框
                moment_can.ShowDialog();
                //若没有输入任何内容则退出
                if (moment_can.Date_TimeStart.Equals("") || moment_can.Date_TimeEnd.Equals(""))
                {
                    return;
                }
                //SQL查询语句
                string sql_mysql = "SELECT b.EJ_No, c.`Name`, b.totalAmount, b.totalMoney FROM ( SELECT a.EJ_No, SUM( a.Cancellation_Total_Amount ) AS totalAmount, "
                    + "SUM(a.Cancellation_Total_QTY) AS totalMoney FROM z_report_data AS a WHERE a.UserId = @UserId AND DATE_FORMAT(a.Date_Time, '%Y-%m-%d') BETWEEN @Date_TimeStart AND @Date_TimeEnd "
                    + "GROUP BY a.EJ_No ) AS b, cashier_table AS c WHERE b.EJ_No = c.Number ORDER BY b.EJ_No ASC";
                //配置SQL查询语句里的参数
                SQLiteParameter[] parameters = {
                    new SQLiteParameter("@Date_TimeStart",moment_can.Date_TimeStart),
                    new SQLiteParameter("@Date_TimeEnd",moment_can.Date_TimeEnd),
                };
                //执行查询，结果为DataTable类型
                DataTable dt = sqliteDBHelper_dmDB.ExecuteDataTable(sql_mysql, parameters);
                //判断查询结果是否为0行
                if (dt.Rows.Count == 0)
                {
                    //弹出提示框
                    //MessageBox.Show("No result!", "Tip", MessageBoxButton.OK, MessageBoxImage.Information);
                    dataGrid3_2.ItemsSource = null;//先清空表格内容
                    DataTable dt_temp = new DataTable();//新建临时表
                    dt_temp.Columns.Add(new DataColumn("Tip"));//添加列
                    dt_temp.Rows.Add(dt_temp.NewRow());//添加行
                    dt_temp.Rows[0]["Tip"] = "No result！";//设置行内容
                    dataGrid3_2.ItemsSource = dt_temp.DefaultView;//把表格放到控件中
                    return;
                }
                //查询出来的表格的字段名为英文，但显示给用户的时候要为中文，所以在此添加若干个中文字段名显示给用户
                dt.Columns.Add(new DataColumn("收银员编号"));
                dt.Columns.Add(new DataColumn("收银员名称"));
                dt.Columns.Add(new DataColumn("起始时间"));
                dt.Columns.Add(new DataColumn("结束时间"));
                dt.Columns.Add(new DataColumn("总取消数量"));
                dt.Columns.Add(new DataColumn("总取消金额"));
                //以下为给新添加的中文字段赋值
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["收银员编号"] = dt.Rows[i]["EJ_No"];
                    dt.Rows[i]["收银员名称"] = dt.Rows[i]["Name"];
                    dt.Rows[i]["起始时间"] = moment_can.Date_TimeStart;
                    dt.Rows[i]["结束时间"] = moment_can.Date_TimeEnd;
                    dt.Rows[i]["总取消数量"] = dt.Rows[i]["totalAmount"];
                    dt.Rows[i]["总取消金额"] = dt.Rows[i]["totalMoney"];
                    //DateTime.Now.ToLongDateString().ToString() 或者 DateTime.Now.ToString("yyyy年MM月dd日")
                }
                //string result = JsonConvert.SerializeObject(dt);//将DataTable类型的变量序列化成json字符串
                //Console.WriteLine(result);
                dataGrid3_2.ItemsSource = dt.DefaultView;
                //以下为设置前若干个标题是英文字段 隐藏， 数量为select查询的的字段数
                int count = 4;//设置查询结果的字段数
                for (int i = 0; i < count; i++)
                {
                    this.dataGrid3_2.Columns[i].Visibility = System.Windows.Visibility.Hidden;
                }
                //以下为设置字段宽度
                this.dataGrid3_2.Columns[count++].Width = 120;
                this.dataGrid3_2.Columns[count++].Width = 120;
                this.dataGrid3_2.Columns[count++].Width = 120;
                this.dataGrid3_2.Columns[count++].Width = 120;
                this.dataGrid3_2.Columns[count++].Width = 100;
                this.dataGrid3_2.Columns[count++].Width = 100;
                #endregion
            }
            else if (ComboBox3_2.SelectionBoxItem.ToString().Equals("营业员退货周报表"))
            {
                #region part3_2 周报表 营业员退货周报表
                //获取查询参数：时间
                Moment_Cancel moment_can = new Moment_Cancel();
                //弹出对话框
                moment_can.ShowDialog();
                //若没有输入任何内容则退出
                if (moment_can.Date_TimeStart.Equals("") || moment_can.Date_TimeEnd.Equals(""))
                {
                    return;
                }
                //SQL查询语句
                string sql_mysql = "SELECT b.EJ_No, c.`Name`, b.totalAmount, b.totalMoney FROM ( SELECT a.EJ_No, SUM( a.Decrease_Total_Amount ) AS totalAmount, "
                    + "SUM(a.Decrease_Total_QTY) AS totalMoney FROM z_report_data AS a WHERE a.UserId = @UserId AND DATE_FORMAT(a.Date_Time, '%Y-%m-%d') BETWEEN @Date_TimeStart AND @Date_TimeEnd "
                    + "GROUP BY a.EJ_No ) AS b, cashier_table AS c WHERE b.EJ_No = c.Number ORDER BY b.EJ_No ASC";
                //配置SQL查询语句里的参数
                SQLiteParameter[] parameters = {
                    new SQLiteParameter("@Date_TimeStart",moment_can.Date_TimeStart),
                    new SQLiteParameter("@Date_TimeEnd",moment_can.Date_TimeEnd),
                };
                //执行查询，结果为DataTable类型
                DataTable dt = sqliteDBHelper_dmDB.ExecuteDataTable(sql_mysql, parameters);
                //判断查询结果是否为0行
                if (dt.Rows.Count == 0)
                {
                    //弹出提示框
                    //MessageBox.Show("No result!", "Tip", MessageBoxButton.OK, MessageBoxImage.Information);
                    dataGrid3_2.ItemsSource = null;//先清空表格内容
                    DataTable dt_temp = new DataTable();//新建临时表
                    dt_temp.Columns.Add(new DataColumn("Tip"));//添加列
                    dt_temp.Rows.Add(dt_temp.NewRow());//添加行
                    dt_temp.Rows[0]["Tip"] = "No result！";//设置行内容
                    dataGrid3_2.ItemsSource = dt_temp.DefaultView;//把表格放到控件中
                    return;
                }
                //查询出来的表格的字段名为英文，但显示给用户的时候要为中文，所以在此添加若干个中文字段名显示给用户
                dt.Columns.Add(new DataColumn("收银员编号"));
                dt.Columns.Add(new DataColumn("收银员名称"));
                dt.Columns.Add(new DataColumn("起始时间"));
                dt.Columns.Add(new DataColumn("结束时间"));
                dt.Columns.Add(new DataColumn("总退货数量"));
                dt.Columns.Add(new DataColumn("总退货金额"));
                //以下为给新添加的中文字段赋值
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["收银员编号"] = dt.Rows[i]["EJ_No"];
                    dt.Rows[i]["收银员名称"] = dt.Rows[i]["Name"];
                    dt.Rows[i]["起始时间"] = moment_can.Date_TimeStart;
                    dt.Rows[i]["结束时间"] = moment_can.Date_TimeEnd;
                    dt.Rows[i]["总退货数量"] = dt.Rows[i]["totalAmount"];
                    dt.Rows[i]["总退货金额"] = dt.Rows[i]["totalMoney"];
                    //DateTime.Now.ToLongDateString().ToString() 或者 DateTime.Now.ToString("yyyy年MM月dd日")
                }
                //string result = JsonConvert.SerializeObject(dt);//将DataTable类型的变量序列化成json字符串
                //Console.WriteLine(result);
                dataGrid3_2.ItemsSource = dt.DefaultView;
                //以下为设置前若干个标题是英文字段 隐藏， 数量为select查询的的字段数
                int count = 4;//设置查询结果的字段数
                for (int i = 0; i < count; i++)
                {
                    this.dataGrid3_2.Columns[i].Visibility = System.Windows.Visibility.Hidden;
                }
                //以下为设置字段宽度
                this.dataGrid3_2.Columns[count++].Width = 120;
                this.dataGrid3_2.Columns[count++].Width = 120;
                this.dataGrid3_2.Columns[count++].Width = 120;
                this.dataGrid3_2.Columns[count++].Width = 120;
                this.dataGrid3_2.Columns[count++].Width = 100;
                this.dataGrid3_2.Columns[count++].Width = 100;
                #endregion
            }
            else if (ComboBox3_2.SelectionBoxItem.ToString().Equals("营业员更正周报表"))
            {
                #region part3_2 周报表 营业员更正周报表
                //获取查询参数：时间
                Moment_Cancel moment_can = new Moment_Cancel();
                //弹出对话框
                moment_can.ShowDialog();
                //若没有输入任何内容则退出
                if (moment_can.Date_TimeStart.Equals("") || moment_can.Date_TimeEnd.Equals(""))
                {
                    return;
                }
                //SQL查询语句
                string sql_mysql = "SELECT b.EJ_No, c.`Name`, b.totalAmount, b.totalMoney FROM ( SELECT a.EJ_No, SUM( a.Error_Correction_Total_Amount ) AS totalAmount, "
                    + "SUM(a.Error_Correction_Total_QTY) AS totalMoney FROM z_report_data AS a WHERE a.UserId = @UserId AND DATE_FORMAT(a.Date_Time, '%Y-%m-%d') BETWEEN @Date_TimeStart AND @Date_TimeEnd "
                    + "GROUP BY a.EJ_No ) AS b, cashier_table AS c WHERE b.EJ_No = c.Number ORDER BY b.EJ_No ASC";
                //配置SQL查询语句里的参数
                SQLiteParameter[] parameters = {
                    new SQLiteParameter("@Date_TimeStart",moment_can.Date_TimeStart),
                    new SQLiteParameter("@Date_TimeEnd",moment_can.Date_TimeEnd),
                };
                //执行查询，结果为DataTable类型
                DataTable dt = sqliteDBHelper_dmDB.ExecuteDataTable(sql_mysql, parameters);
                //判断查询结果是否为0行
                if (dt.Rows.Count == 0)
                {
                    //弹出提示框
                    //MessageBox.Show("No result!", "Tip", MessageBoxButton.OK, MessageBoxImage.Information);
                    dataGrid3_2.ItemsSource = null;//先清空表格内容
                    DataTable dt_temp = new DataTable();//新建临时表
                    dt_temp.Columns.Add(new DataColumn("Tip"));//添加列
                    dt_temp.Rows.Add(dt_temp.NewRow());//添加行
                    dt_temp.Rows[0]["Tip"] = "No result！";//设置行内容
                    dataGrid3_2.ItemsSource = dt_temp.DefaultView;//把表格放到控件中
                    return;
                }
                //查询出来的表格的字段名为英文，但显示给用户的时候要为中文，所以在此添加若干个中文字段名显示给用户
                dt.Columns.Add(new DataColumn("收银员编号"));
                dt.Columns.Add(new DataColumn("收银员名称"));
                dt.Columns.Add(new DataColumn("起始时间"));
                dt.Columns.Add(new DataColumn("结束时间"));
                dt.Columns.Add(new DataColumn("总更正数量"));
                dt.Columns.Add(new DataColumn("总更正金额"));
                //以下为给新添加的中文字段赋值
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["收银员编号"] = dt.Rows[i]["EJ_No"];
                    dt.Rows[i]["收银员名称"] = dt.Rows[i]["Name"];
                    dt.Rows[i]["起始时间"] = moment_can.Date_TimeStart;
                    dt.Rows[i]["结束时间"] = moment_can.Date_TimeEnd;
                    dt.Rows[i]["总更正数量"] = dt.Rows[i]["totalAmount"];
                    dt.Rows[i]["总更正金额"] = dt.Rows[i]["totalMoney"];
                }
                dataGrid3_2.ItemsSource = dt.DefaultView;
                //以下为设置前若干个标题是英文字段 隐藏， 数量为select查询的的字段数
                int count = 4;//设置查询结果的字段数
                for (int i = 0; i < count; i++)
                {
                    this.dataGrid3_2.Columns[i].Visibility = System.Windows.Visibility.Hidden;
                }
                //以下为设置字段宽度
                this.dataGrid3_2.Columns[count++].Width = 120;
                this.dataGrid3_2.Columns[count++].Width = 120;
                this.dataGrid3_2.Columns[count++].Width = 120;
                this.dataGrid3_2.Columns[count++].Width = 120;
                this.dataGrid3_2.Columns[count++].Width = 120;
                this.dataGrid3_2.Columns[count++].Width = 100;
                #endregion
            }
            else if (ComboBox3_2.SelectionBoxItem.ToString().Equals("税率周报表"))
            {
                #region part3_2 周报表 税率周报表
                //获取查询参数：时间
                Moment_Cancel moment_can = new Moment_Cancel();
                //弹出对话框
                moment_can.ShowDialog();
                //若没有输入任何内容则退出
                if (moment_can.Date_TimeStart.Equals("") || moment_can.Date_TimeEnd.Equals(""))
                {
                    return;
                }
                //SQL查询语句
                string sql_mysql = "SELECT ifnull(SUM(ifnull(Daily_Total_Sales,0)),0) AS totalMoney, ifnull(SUM(ifnull(Daily_Total_VAT,0)),0) AS totalVat FROM z_report_data WHERE UserId = @UserId AND DATE_FORMAT(Date_Time, '%Y-%m-%d') BETWEEN @Date_TimeStart AND @Date_TimeEnd";
                //配置SQL查询语句里的参数
                SQLiteParameter[] parameters = {
                    new SQLiteParameter("@Date_TimeStart",moment_can.Date_TimeStart),
                    new SQLiteParameter("@Date_TimeEnd",moment_can.Date_TimeEnd),
                };
                //执行查询，结果为DataTable类型
                DataTable dt = sqliteDBHelper_dmDB.ExecuteDataTable(sql_mysql, parameters);
                //判断查询结果是否为0行
                if (dt.Rows.Count == 0)
                {
                    //弹出提示框
                    //MessageBox.Show("No result!", "Tip", MessageBoxButton.OK, MessageBoxImage.Information);
                    dataGrid3_2.ItemsSource = null;//先清空表格内容
                    DataTable dt_temp = new DataTable();//新建临时表
                    dt_temp.Columns.Add(new DataColumn("Tip"));//添加列
                    dt_temp.Rows.Add(dt_temp.NewRow());//添加行
                    dt_temp.Rows[0]["Tip"] = "No result！";//设置行内容
                    dataGrid3_2.ItemsSource = dt_temp.DefaultView;//把表格放到控件中
                    return;
                }
                //查询出来的表格的字段名为英文，但显示给用户的时候要为中文，所以在此添加若干个中文字段名显示给用户
                //dt.Columns.Add(new DataColumn("税率"));
                dt.Columns.Add(new DataColumn("起始时间"));
                dt.Columns.Add(new DataColumn("结束时间"));
                dt.Columns.Add(new DataColumn("总额"));
                dt.Columns.Add(new DataColumn("税额"));
                //以下为给新添加的中文字段赋值
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    //dt.Rows[i]["税率"] = dt.Rows[i]["EJ_No"];
                    dt.Rows[i]["起始时间"] = moment_can.Date_TimeStart;
                    dt.Rows[i]["结束时间"] = moment_can.Date_TimeEnd;
                    dt.Rows[i]["总额"] = dt.Rows[i]["totalMoney"];
                    dt.Rows[i]["税额"] = dt.Rows[i]["totalVat"];
                }
                dataGrid3_2.ItemsSource = dt.DefaultView;
                //以下为设置前若干个标题是英文字段 隐藏， 数量为select查询的的字段数
                int count = 2;//设置查询结果的字段数
                for (int i = 0; i < count; i++)
                {
                    this.dataGrid3_2.Columns[i].Visibility = System.Windows.Visibility.Hidden;
                }
                //以下为设置字段宽度
                //this.dataGrid3_2.Columns[count++].Width = 120;
                this.dataGrid3_2.Columns[count++].Width = 120;
                this.dataGrid3_2.Columns[count++].Width = 120;
                this.dataGrid3_2.Columns[count++].Width = 120;
                this.dataGrid3_2.Columns[count++].Width = 120;
                #endregion
            }
        }
        #endregion
        /////////////////////////////////////////////////////////////////////////////////
        #region part3_3 月报表 本地数据库
        ///<summary>
        ///part3_3 月报表 查询本地数据库
        ///</summary>
        private void button3_3_1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                button3_3_1_Click();
            }
            catch (Exception ee)
            {
                MessageBox.Show("The operation failed, please try again. Possible causes:" + ee.Message, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }

        }
        ///<summary>
        ///part3_3 月报表 查询本地数据库
        ///</summary>
        private void button3_3_1_Click()
        {
            if (ComboBox3_3.SelectionBoxItem.ToString().Equals("部类月报表"))
            {
                #region part3_3 月报表 部类月报表
                //获取查询参数：时间、Z号码
                Month_Department month_dep = new Month_Department(dmDB);
                month_dep.ShowDialog();
                //return;
                //若没有输入任何内容则退出
                if (month_dep.Date_TimeStart.Equals("") || month_dep.Date_TimeEnd.Equals("") || month_dep.Znumber.Equals(""))
                {
                    return;
                }
                //Console.WriteLine(month_dep.Date_TimeStart + "  " + month_dep.Date_TimeEnd+ " "+ month_dep.Znumber);
                //SQL查询语句
                string sql = "SELECT Dept_Index, Name, SUM(Item_Sum) AS totalMoney, SUM(Quantity) AS totalNumber FROM Sales_Item WHERE Date_Time >= @Date_TimeStart AND Date_Time <= @Date_TimeEnd "
                    + "AND Z_Number = @Z_Number GROUP BY Dept_Index ORDER BY Dept_Index ASC";
                //配置SQL查询语句里的参数
                SQLiteParameter[] parameters = {
                    new SQLiteParameter("@Date_TimeStart",month_dep.Date_TimeStart),
                    new SQLiteParameter("@Date_TimeEnd",month_dep.Date_TimeEnd),
                    new SQLiteParameter("@Z_Number",month_dep.Znumber)
                };
                //执行查询，结果为DataTable类型
                DataTable dt = sqliteDBHelper_dmDB.ExecuteDataTable(sql, parameters);
                //判断查询结果是否为0行
                if (dt.Rows.Count == 0)
                {
                    //弹出提示框
                    //MessageBox.Show("No result!", "Tip", MessageBoxButton.OK, MessageBoxImage.Information);
                    dataGrid3_3.ItemsSource = null;//先清空表格内容
                    DataTable dt_temp = new DataTable();//新建临时表
                    dt_temp.Columns.Add(new DataColumn("Tip"));//添加列
                    dt_temp.Rows.Add(dt_temp.NewRow());//添加行
                    dt_temp.Rows[0]["Tip"] = "No result！";//设置行内容
                    dataGrid3_3.ItemsSource = dt_temp.DefaultView;//把表格放到控件中
                    return;
                }
                //查询出来的表格的字段名为英文，但显示给用户的时候要为中文，所以在此添加若干个中文字段名显示给用户
                dt.Columns.Add(new DataColumn("部门编号"));
                dt.Columns.Add(new DataColumn("部门名称"));
                dt.Columns.Add(new DataColumn("统计月份"));
                dt.Columns.Add(new DataColumn("总销售量"));
                dt.Columns.Add(new DataColumn("总销售额"));
                dt.Columns.Add(new DataColumn("Z号码"));
                //以下为给新添加的中文字段赋值
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["部门编号"] = dt.Rows[i]["Dept_Index"];
                    dt.Rows[i]["部门名称"] = dt.Rows[i]["Name"];
                    dt.Rows[i]["统计月份"] = month_dep.Date_Time;
                    dt.Rows[i]["总销售量"] = dt.Rows[i]["totalNumber"];
                    dt.Rows[i]["总销售额"] = dt.Rows[i]["totalMoney"];
                    dt.Rows[i]["Z号码"] = month_dep.Znumber;
                    //DateTime.Now.ToLongDateString().ToString() 或者 DateTime.Now.ToString("yyyy年MM月dd日")
                }
                //string result = JsonConvert.SerializeObject(dt);//将DataTable类型的变量序列化成json字符串
                //Console.WriteLine(result);
                dataGrid3_3.ItemsSource = dt.DefaultView;
                //以下为设置前若干个标题是英文字段 隐藏， 数量为select查询的的字段数
                int count = 4;//设置查询结果的字段数
                for (int i = 0; i < count; i++)
                {
                    this.dataGrid3_3.Columns[i].Visibility = System.Windows.Visibility.Hidden;
                }
                //以下为设置字段宽度
                this.dataGrid3_3.Columns[count++].Width = 90;
                this.dataGrid3_3.Columns[count++].Width = 150;
                this.dataGrid3_3.Columns[count++].Width = 120;
                this.dataGrid3_3.Columns[count++].Width = 120;
                this.dataGrid3_3.Columns[count++].Width = 120;
                this.dataGrid3_3.Columns[count++].Width = 90;
                #endregion
            }
            else if (ComboBox3_3.SelectionBoxItem.ToString().Equals("营业员支付总额月报表"))
            {

            }
            else if (ComboBox3_3.SelectionBoxItem.ToString().Equals("营业员取消月报表"))
            {
                #region part3_3 月报表 营业员取消月报表
                //获取查询参数：时间
                Moment_Cancel moment_can = new Moment_Cancel();
                //弹出对话框
                moment_can.ShowDialog();
                //若没有输入任何内容则退出
                if (moment_can.Date_TimeStart.Equals("") || moment_can.Date_TimeEnd.Equals(""))
                {
                    return;
                }
                //SQL查询语句
                string sql_mysql = "SELECT b.EJ_No, c.`Name`, b.totalAmount, b.totalMoney FROM ( SELECT a.EJ_No, SUM( a.Cancellation_Total_Amount ) AS totalAmount, "
                    + "SUM(a.Cancellation_Total_QTY) AS totalMoney FROM z_report_data AS a WHERE a.UserId = @UserId AND DATE_FORMAT(a.Date_Time, '%Y-%m-%d') BETWEEN @Date_TimeStart AND @Date_TimeEnd "
                    + "GROUP BY a.EJ_No ) AS b, cashier_table AS c WHERE b.EJ_No = c.Number ORDER BY b.EJ_No ASC";
                //配置SQL查询语句里的参数
                SQLiteParameter[] parameters = {
                    new SQLiteParameter("@Date_TimeStart",moment_can.Date_TimeStart),
                    new SQLiteParameter("@Date_TimeEnd",moment_can.Date_TimeEnd),
                };
                //执行查询，结果为DataTable类型
                DataTable dt = sqliteDBHelper_dmDB.ExecuteDataTable(sql_mysql, parameters);
                //判断查询结果是否为0行
                if (dt.Rows.Count == 0)
                {
                    //弹出提示框
                    //MessageBox.Show("No result!", "Tip", MessageBoxButton.OK, MessageBoxImage.Information);
                    dataGrid3_3.ItemsSource = null;//先清空表格内容
                    DataTable dt_temp = new DataTable();//新建临时表
                    dt_temp.Columns.Add(new DataColumn("Tip"));//添加列
                    dt_temp.Rows.Add(dt_temp.NewRow());//添加行
                    dt_temp.Rows[0]["Tip"] = "No result！";//设置行内容
                    dataGrid3_3.ItemsSource = dt_temp.DefaultView;//把表格放到控件中
                    return;
                }
                //查询出来的表格的字段名为英文，但显示给用户的时候要为中文，所以在此添加若干个中文字段名显示给用户
                dt.Columns.Add(new DataColumn("收银员编号"));
                dt.Columns.Add(new DataColumn("收银员名称"));
                dt.Columns.Add(new DataColumn("起始时间"));
                dt.Columns.Add(new DataColumn("结束时间"));
                dt.Columns.Add(new DataColumn("总取消数量"));
                dt.Columns.Add(new DataColumn("总取消金额"));
                //以下为给新添加的中文字段赋值
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["收银员编号"] = dt.Rows[i]["EJ_No"];
                    dt.Rows[i]["收银员名称"] = dt.Rows[i]["Name"];
                    dt.Rows[i]["起始时间"] = moment_can.Date_TimeStart;
                    dt.Rows[i]["结束时间"] = moment_can.Date_TimeEnd;
                    dt.Rows[i]["总取消数量"] = dt.Rows[i]["totalAmount"];
                    dt.Rows[i]["总取消金额"] = dt.Rows[i]["totalMoney"];
                    //DateTime.Now.ToLongDateString().ToString() 或者 DateTime.Now.ToString("yyyy年MM月dd日")
                }
                //string result = JsonConvert.SerializeObject(dt);//将DataTable类型的变量序列化成json字符串
                //Console.WriteLine(result);
                dataGrid3_3.ItemsSource = dt.DefaultView;
                //以下为设置前若干个标题是英文字段 隐藏， 数量为select查询的的字段数
                int count = 4;//设置查询结果的字段数
                for (int i = 0; i < count; i++)
                {
                    this.dataGrid3_3.Columns[i].Visibility = System.Windows.Visibility.Hidden;
                }
                //以下为设置字段宽度
                this.dataGrid3_3.Columns[count++].Width = 120;
                this.dataGrid3_3.Columns[count++].Width = 120;
                this.dataGrid3_3.Columns[count++].Width = 120;
                this.dataGrid3_3.Columns[count++].Width = 120;
                this.dataGrid3_3.Columns[count++].Width = 100;
                this.dataGrid3_3.Columns[count++].Width = 100;
                #endregion
            }
            else if (ComboBox3_3.SelectionBoxItem.ToString().Equals("营业员退货月报表"))
            {
                #region part3_3 月报表 营业员退货月报表
                //获取查询参数：时间
                Moment_Cancel moment_can = new Moment_Cancel();
                //弹出对话框
                moment_can.ShowDialog();
                //若没有输入任何内容则退出
                if (moment_can.Date_TimeStart.Equals("") || moment_can.Date_TimeEnd.Equals(""))
                {
                    return;
                }
                //SQL查询语句
                string sql_mysql = "SELECT b.EJ_No, c.`Name`, b.totalAmount, b.totalMoney FROM ( SELECT a.EJ_No, SUM( a.Decrease_Total_Amount ) AS totalAmount, "
                    + "SUM(a.Decrease_Total_QTY) AS totalMoney FROM z_report_data AS a WHERE a.UserId = @UserId AND DATE_FORMAT(a.Date_Time, '%Y-%m-%d') BETWEEN @Date_TimeStart AND @Date_TimeEnd "
                    + "GROUP BY a.EJ_No ) AS b, cashier_table AS c WHERE b.EJ_No = c.Number ORDER BY b.EJ_No ASC";
                //配置SQL查询语句里的参数
                SQLiteParameter[] parameters = {
                    new SQLiteParameter("@Date_TimeStart",moment_can.Date_TimeStart),
                    new SQLiteParameter("@Date_TimeEnd",moment_can.Date_TimeEnd),
                };
                //执行查询，结果为DataTable类型
                DataTable dt = sqliteDBHelper_dmDB.ExecuteDataTable(sql_mysql, parameters);
                //判断查询结果是否为0行
                if (dt.Rows.Count == 0)
                {
                    //弹出提示框
                    //MessageBox.Show("No result!", "Tip", MessageBoxButton.OK, MessageBoxImage.Information);
                    dataGrid3_3.ItemsSource = null;//先清空表格内容
                    DataTable dt_temp = new DataTable();//新建临时表
                    dt_temp.Columns.Add(new DataColumn("Tip"));//添加列
                    dt_temp.Rows.Add(dt_temp.NewRow());//添加行
                    dt_temp.Rows[0]["Tip"] = "No result！";//设置行内容
                    dataGrid3_3.ItemsSource = dt_temp.DefaultView;//把表格放到控件中
                    return;
                }
                //查询出来的表格的字段名为英文，但显示给用户的时候要为中文，所以在此添加若干个中文字段名显示给用户
                dt.Columns.Add(new DataColumn("收银员编号"));
                dt.Columns.Add(new DataColumn("收银员名称"));
                dt.Columns.Add(new DataColumn("起始时间"));
                dt.Columns.Add(new DataColumn("结束时间"));
                dt.Columns.Add(new DataColumn("总退货数量"));
                dt.Columns.Add(new DataColumn("总退货金额"));
                //以下为给新添加的中文字段赋值
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["收银员编号"] = dt.Rows[i]["EJ_No"];
                    dt.Rows[i]["收银员名称"] = dt.Rows[i]["Name"];
                    dt.Rows[i]["起始时间"] = moment_can.Date_TimeStart;
                    dt.Rows[i]["结束时间"] = moment_can.Date_TimeEnd;
                    dt.Rows[i]["总退货数量"] = dt.Rows[i]["totalAmount"];
                    dt.Rows[i]["总退货金额"] = dt.Rows[i]["totalMoney"];
                    //DateTime.Now.ToLongDateString().ToString() 或者 DateTime.Now.ToString("yyyy年MM月dd日")
                }
                //string result = JsonConvert.SerializeObject(dt);//将DataTable类型的变量序列化成json字符串
                //Console.WriteLine(result);
                dataGrid3_3.ItemsSource = dt.DefaultView;
                //以下为设置前若干个标题是英文字段 隐藏， 数量为select查询的的字段数
                int count = 4;//设置查询结果的字段数
                for (int i = 0; i < count; i++)
                {
                    this.dataGrid3_3.Columns[i].Visibility = System.Windows.Visibility.Hidden;
                }
                //以下为设置字段宽度
                this.dataGrid3_3.Columns[count++].Width = 120;
                this.dataGrid3_3.Columns[count++].Width = 120;
                this.dataGrid3_3.Columns[count++].Width = 120;
                this.dataGrid3_3.Columns[count++].Width = 120;
                this.dataGrid3_3.Columns[count++].Width = 100;
                this.dataGrid3_3.Columns[count++].Width = 100;
                #endregion
            }
            else if (ComboBox3_3.SelectionBoxItem.ToString().Equals("营业员更正月报表"))
            {
                #region part3_3 月报表 营业员更正月报表
                //获取查询参数：时间
                Moment_Cancel moment_can = new Moment_Cancel();
                //弹出对话框
                moment_can.ShowDialog();
                //若没有输入任何内容则退出
                if (moment_can.Date_TimeStart.Equals("") || moment_can.Date_TimeEnd.Equals(""))
                {
                    return;
                }
                //SQL查询语句
                string sql_mysql = "SELECT b.EJ_No, c.`Name`, b.totalAmount, b.totalMoney FROM ( SELECT a.EJ_No, SUM( a.Error_Correction_Total_Amount ) AS totalAmount, "
                    + "SUM(a.Error_Correction_Total_QTY) AS totalMoney FROM z_report_data AS a WHERE a.UserId = @UserId AND DATE_FORMAT(a.Date_Time, '%Y-%m-%d') BETWEEN @Date_TimeStart AND @Date_TimeEnd "
                    + "GROUP BY a.EJ_No ) AS b, cashier_table AS c WHERE b.EJ_No = c.Number ORDER BY b.EJ_No ASC";
                //配置SQL查询语句里的参数
                SQLiteParameter[] parameters = {
                    new SQLiteParameter("@Date_TimeStart",moment_can.Date_TimeStart),
                    new SQLiteParameter("@Date_TimeEnd",moment_can.Date_TimeEnd),
                };
                //执行查询，结果为DataTable类型
                DataTable dt = sqliteDBHelper_dmDB.ExecuteDataTable(sql_mysql, parameters);
                //判断查询结果是否为0行
                if (dt.Rows.Count == 0)
                {
                    //弹出提示框
                    //MessageBox.Show("No result!", "Tip", MessageBoxButton.OK, MessageBoxImage.Information);
                    dataGrid3_3.ItemsSource = null;//先清空表格内容
                    DataTable dt_temp = new DataTable();//新建临时表
                    dt_temp.Columns.Add(new DataColumn("Tip"));//添加列
                    dt_temp.Rows.Add(dt_temp.NewRow());//添加行
                    dt_temp.Rows[0]["Tip"] = "No result！";//设置行内容
                    dataGrid3_3.ItemsSource = dt_temp.DefaultView;//把表格放到控件中
                    return;
                }
                //查询出来的表格的字段名为英文，但显示给用户的时候要为中文，所以在此添加若干个中文字段名显示给用户
                dt.Columns.Add(new DataColumn("收银员编号"));
                dt.Columns.Add(new DataColumn("收银员名称"));
                dt.Columns.Add(new DataColumn("起始时间"));
                dt.Columns.Add(new DataColumn("结束时间"));
                dt.Columns.Add(new DataColumn("总更正数量"));
                dt.Columns.Add(new DataColumn("总更正金额"));
                //以下为给新添加的中文字段赋值
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["收银员编号"] = dt.Rows[i]["EJ_No"];
                    dt.Rows[i]["收银员名称"] = dt.Rows[i]["Name"];
                    dt.Rows[i]["起始时间"] = moment_can.Date_TimeStart;
                    dt.Rows[i]["结束时间"] = moment_can.Date_TimeEnd;
                    dt.Rows[i]["总更正数量"] = dt.Rows[i]["totalAmount"];
                    dt.Rows[i]["总更正金额"] = dt.Rows[i]["totalMoney"];
                }
                dataGrid3_3.ItemsSource = dt.DefaultView;
                //以下为设置前若干个标题是英文字段 隐藏， 数量为select查询的的字段数
                int count = 4;//设置查询结果的字段数
                for (int i = 0; i < count; i++)
                {
                    this.dataGrid3_3.Columns[i].Visibility = System.Windows.Visibility.Hidden;
                }
                //以下为设置字段宽度
                this.dataGrid3_3.Columns[count++].Width = 120;
                this.dataGrid3_3.Columns[count++].Width = 120;
                this.dataGrid3_3.Columns[count++].Width = 120;
                this.dataGrid3_3.Columns[count++].Width = 120;
                this.dataGrid3_3.Columns[count++].Width = 120;
                this.dataGrid3_3.Columns[count++].Width = 100;
                #endregion
            }
            else if (ComboBox3_3.SelectionBoxItem.ToString().Equals("税率月报表"))
            {
                #region part3_3 月报表 税率月报表
                //获取查询参数：时间
                Moment_Cancel moment_can = new Moment_Cancel();
                //弹出对话框
                moment_can.ShowDialog();
                //若没有输入任何内容则退出
                if (moment_can.Date_TimeStart.Equals("") || moment_can.Date_TimeEnd.Equals(""))
                {
                    return;
                }
                //SQL查询语句
                string sql_mysql = "SELECT ifnull(SUM(ifnull(Daily_Total_Sales,0)),0) AS totalMoney, ifnull(SUM(ifnull(Daily_Total_VAT,0)),0) AS totalVat FROM z_report_data WHERE UserId = @UserId AND DATE_FORMAT(Date_Time, '%Y-%m-%d') BETWEEN @Date_TimeStart AND @Date_TimeEnd";
                //配置SQL查询语句里的参数
                SQLiteParameter[] parameters = {
                    new SQLiteParameter("@Date_TimeStart",moment_can.Date_TimeStart),
                    new SQLiteParameter("@Date_TimeEnd",moment_can.Date_TimeEnd),
                };
                //执行查询，结果为DataTable类型
                DataTable dt = sqliteDBHelper_dmDB.ExecuteDataTable(sql_mysql, parameters);
                //判断查询结果是否为0行
                if (dt.Rows.Count == 0)
                {
                    //弹出提示框
                    //MessageBox.Show("No result!", "Tip", MessageBoxButton.OK, MessageBoxImage.Information);
                    dataGrid3_3.ItemsSource = null;//先清空表格内容
                    DataTable dt_temp = new DataTable();//新建临时表
                    dt_temp.Columns.Add(new DataColumn("Tip"));//添加列
                    dt_temp.Rows.Add(dt_temp.NewRow());//添加行
                    dt_temp.Rows[0]["Tip"] = "No result！";//设置行内容
                    dataGrid3_3.ItemsSource = dt_temp.DefaultView;//把表格放到控件中
                    return;
                }
                //查询出来的表格的字段名为英文，但显示给用户的时候要为中文，所以在此添加若干个中文字段名显示给用户
                //dt.Columns.Add(new DataColumn("税率"));
                dt.Columns.Add(new DataColumn("起始时间"));
                dt.Columns.Add(new DataColumn("结束时间"));
                dt.Columns.Add(new DataColumn("总额"));
                dt.Columns.Add(new DataColumn("税额"));
                //以下为给新添加的中文字段赋值
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    //dt.Rows[i]["税率"] = dt.Rows[i]["EJ_No"];
                    dt.Rows[i]["起始时间"] = moment_can.Date_TimeStart;
                    dt.Rows[i]["结束时间"] = moment_can.Date_TimeEnd;
                    dt.Rows[i]["总额"] = dt.Rows[i]["totalMoney"];
                    dt.Rows[i]["税额"] = dt.Rows[i]["totalVat"];
                }
                dataGrid3_3.ItemsSource = dt.DefaultView;
                //以下为设置前若干个标题是英文字段 隐藏， 数量为select查询的的字段数
                int count = 2;//设置查询结果的字段数
                for (int i = 0; i < count; i++)
                {
                    this.dataGrid3_3.Columns[i].Visibility = System.Windows.Visibility.Hidden;
                }
                //以下为设置字段宽度
                //this.dataGrid3_3.Columns[count++].Width = 120;
                this.dataGrid3_3.Columns[count++].Width = 120;
                this.dataGrid3_3.Columns[count++].Width = 120;
                this.dataGrid3_3.Columns[count++].Width = 120;
                this.dataGrid3_3.Columns[count++].Width = 120;
                #endregion
            }
        }
        #endregion
        /////////////////////////////////////////////////////////////////////////////////
        #region part3_4 总报表 本地数据库
        ///<summary>
        ///part3_4 总报表 查询本地数据库
        ///</summary>
        private void button3_4_1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                button3_4_1_Click();
            }
            catch (Exception ee)
            {
                MessageBox.Show("The operation failed, please try again. Possible causes:" + ee.Message, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        ///<summary>
        ///part3_4 总报表 查询本地数据库
        ///</summary>
        private void button3_4_1_Click()
        {
            if (ComboBox3_4.SelectionBoxItem.ToString().Equals("部门总报表"))
            {
                #region part3_4 总报表 部门总报表
                //SQL查询语句
                string sql = "SELECT Dept_Index, Name, SUM(Item_Sum) AS totalMoney, SUM(Quantity) AS totalNumber FROM Sales_Item "
                    + "GROUP BY Dept_Index ORDER BY Dept_Index ASC";
                //配置SQL查询语句里的参数
                //SQLiteParameter[] parameters = {
                //    new SQLiteParameter("@Z_Number",month_dep.Znumber)
                //};
                //执行查询，结果为DataTable类型
                DataTable dt = sqliteDBHelper_dmDB.ExecuteDataTable(sql, null);
                //判断查询结果是否为0行
                if (dt.Rows.Count == 0)
                {
                    //弹出提示框
                    //MessageBox.Show("No result!", "Tip", MessageBoxButton.OK, MessageBoxImage.Information);
                    dataGrid3_4.ItemsSource = null;//先清空表格内容
                    DataTable dt_temp = new DataTable();//新建临时表
                    dt_temp.Columns.Add(new DataColumn("Tip"));//添加列
                    dt_temp.Rows.Add(dt_temp.NewRow());//添加行
                    dt_temp.Rows[0]["Tip"] = "No result！";//设置行内容
                    dataGrid3_4.ItemsSource = dt_temp.DefaultView;//把表格放到控件中
                    return;
                }
                //查询出来的表格的字段名为英文，但显示给用户的时候要为中文，所以在此添加若干个中文字段名显示给用户
                dt.Columns.Add(new DataColumn("部门编号"));
                dt.Columns.Add(new DataColumn("部门名称"));
                dt.Columns.Add(new DataColumn("总销售量"));
                dt.Columns.Add(new DataColumn("总销售额"));
                //以下为给新添加的中文字段赋值
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["部门编号"] = dt.Rows[i]["Dept_Index"];
                    dt.Rows[i]["部门名称"] = dt.Rows[i]["Name"];
                    dt.Rows[i]["总销售量"] = dt.Rows[i]["totalNumber"];
                    dt.Rows[i]["总销售额"] = dt.Rows[i]["totalMoney"];
                    //DateTime.Now.ToLongDateString().ToString() 或者 DateTime.Now.ToString("yyyy年MM月dd日")
                }
                //string result = JsonConvert.SerializeObject(dt);//将DataTable类型的变量序列化成json字符串
                //Console.WriteLine(result);
                dataGrid3_4.ItemsSource = dt.DefaultView;
                //以下为设置前若干个标题是英文字段 隐藏， 数量为select查询的的字段数
                int count = 4;//设置查询结果的字段数
                for (int i = 0; i < count; i++)
                {
                    this.dataGrid3_4.Columns[i].Visibility = System.Windows.Visibility.Hidden;
                }
                //以下为设置字段宽度
                this.dataGrid3_4.Columns[count++].Width = 90;
                this.dataGrid3_4.Columns[count++].Width = 150;
                this.dataGrid3_4.Columns[count++].Width = 120;
                this.dataGrid3_4.Columns[count++].Width = 120;
                #endregion
            }
            else if (ComboBox3_4.SelectionBoxItem.ToString().Equals("营业员取消总报表"))
            {
                #region part3_4 总报表 营业员取消总报表
                //SQL查询语句
                string sql_mysql = "SELECT b.EJ_No, c.`Name`, b.totalAmount, b.totalMoney FROM ( SELECT a.EJ_No, SUM( a.Cancellation_Total_Amount ) AS totalAmount, "
                    + "SUM(a.Cancellation_Total_QTY) AS totalMoney FROM z_report_data AS a WHERE a.UserId = @UserId "
                    + "GROUP BY a.EJ_No ) AS b, cashier_table AS c WHERE b.EJ_No = c.Number ORDER BY b.EJ_No ASC";
                //配置SQL查询语句里的参数
                SQLiteParameter[] parameters = {
                };
                //执行查询，结果为DataTable类型
                DataTable dt = sqliteDBHelper_dmDB.ExecuteDataTable(sql_mysql, parameters);
                //判断查询结果是否为0行
                if (dt.Rows.Count == 0)
                {
                    //弹出提示框
                    //MessageBox.Show("No result!", "Tip", MessageBoxButton.OK, MessageBoxImage.Information);
                    dataGrid3_4.ItemsSource = null;//先清空表格内容
                    DataTable dt_temp = new DataTable();//新建临时表
                    dt_temp.Columns.Add(new DataColumn("Tip"));//添加列
                    dt_temp.Rows.Add(dt_temp.NewRow());//添加行
                    dt_temp.Rows[0]["Tip"] = "No result！";//设置行内容
                    dataGrid3_4.ItemsSource = dt_temp.DefaultView;//把表格放到控件中
                    return;
                }
                //查询出来的表格的字段名为英文，但显示给用户的时候要为中文，所以在此添加若干个中文字段名显示给用户
                dt.Columns.Add(new DataColumn("收银员编号"));
                dt.Columns.Add(new DataColumn("收银员名称"));
                dt.Columns.Add(new DataColumn("总取消数量"));
                dt.Columns.Add(new DataColumn("总取消金额"));
                //以下为给新添加的中文字段赋值
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["收银员编号"] = dt.Rows[i]["EJ_No"];
                    dt.Rows[i]["收银员名称"] = dt.Rows[i]["Name"];
                    dt.Rows[i]["总取消数量"] = dt.Rows[i]["totalAmount"];
                    dt.Rows[i]["总取消金额"] = dt.Rows[i]["totalMoney"];
                    //DateTime.Now.ToLongDateString().ToString() 或者 DateTime.Now.ToString("yyyy年MM月dd日")
                }
                //string result = JsonConvert.SerializeObject(dt);//将DataTable类型的变量序列化成json字符串
                //Console.WriteLine(result);
                dataGrid3_4.ItemsSource = dt.DefaultView;
                //以下为设置前若干个标题是英文字段 隐藏， 数量为select查询的的字段数
                int count = 4;//设置查询结果的字段数
                for (int i = 0; i < count; i++)
                {
                    this.dataGrid3_4.Columns[i].Visibility = System.Windows.Visibility.Hidden;
                }
                //以下为设置字段宽度
                this.dataGrid3_4.Columns[count++].Width = 120;
                this.dataGrid3_4.Columns[count++].Width = 120;
                this.dataGrid3_4.Columns[count++].Width = 100;
                this.dataGrid3_4.Columns[count++].Width = 100;
                #endregion
            }
            else if (ComboBox3_4.SelectionBoxItem.ToString().Equals("营业员退货总报表"))
            {
                #region part3_4 总报表 营业员退货总报表
                //SQL查询语句
                string sql_mysql = "SELECT b.EJ_No, c.`Name`, b.totalAmount, b.totalMoney FROM ( SELECT a.EJ_No, SUM( a.Decrease_Total_Amount ) AS totalAmount, "
                    + "SUM(a.Decrease_Total_QTY) AS totalMoney FROM z_report_data AS a WHERE a.UserId = @UserId "
                    + "GROUP BY a.EJ_No ) AS b, cashier_table AS c WHERE b.EJ_No = c.Number ORDER BY b.EJ_No ASC";
                //配置SQL查询语句里的参数
                SQLiteParameter[] parameters = {
                };
                //执行查询，结果为DataTable类型
                DataTable dt = sqliteDBHelper_dmDB.ExecuteDataTable(sql_mysql, parameters);
                //判断查询结果是否为0行
                if (dt.Rows.Count == 0)
                {
                    //弹出提示框
                    //MessageBox.Show("No result!", "Tip", MessageBoxButton.OK, MessageBoxImage.Information);
                    dataGrid3_4.ItemsSource = null;//先清空表格内容
                    DataTable dt_temp = new DataTable();//新建临时表
                    dt_temp.Columns.Add(new DataColumn("Tip"));//添加列
                    dt_temp.Rows.Add(dt_temp.NewRow());//添加行
                    dt_temp.Rows[0]["Tip"] = "No result！";//设置行内容
                    dataGrid3_4.ItemsSource = dt_temp.DefaultView;//把表格放到控件中
                    return;
                }
                //查询出来的表格的字段名为英文，但显示给用户的时候要为中文，所以在此添加若干个中文字段名显示给用户
                dt.Columns.Add(new DataColumn("收银员编号"));
                dt.Columns.Add(new DataColumn("收银员名称"));
                dt.Columns.Add(new DataColumn("总退货数量"));
                dt.Columns.Add(new DataColumn("总退货金额"));
                //以下为给新添加的中文字段赋值
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["收银员编号"] = dt.Rows[i]["EJ_No"];
                    dt.Rows[i]["收银员名称"] = dt.Rows[i]["Name"];
                    dt.Rows[i]["总退货数量"] = dt.Rows[i]["totalAmount"];
                    dt.Rows[i]["总退货金额"] = dt.Rows[i]["totalMoney"];
                    //DateTime.Now.ToLongDateString().ToString() 或者 DateTime.Now.ToString("yyyy年MM月dd日")
                }
                //string result = JsonConvert.SerializeObject(dt);//将DataTable类型的变量序列化成json字符串
                //Console.WriteLine(result);
                dataGrid3_4.ItemsSource = dt.DefaultView;
                //以下为设置前若干个标题是英文字段 隐藏， 数量为select查询的的字段数
                int count = 4;//设置查询结果的字段数
                for (int i = 0; i < count; i++)
                {
                    this.dataGrid3_4.Columns[i].Visibility = System.Windows.Visibility.Hidden;
                }
                //以下为设置字段宽度
                this.dataGrid3_4.Columns[count++].Width = 120;
                this.dataGrid3_4.Columns[count++].Width = 120;
                this.dataGrid3_4.Columns[count++].Width = 120;
                this.dataGrid3_4.Columns[count++].Width = 120;
                #endregion
            }
            else if (ComboBox3_4.SelectionBoxItem.ToString().Equals("营业员更正总报表"))
            {
                #region part3_4 总报表 营业员更正周报表
                //SQL查询语句
                string sql_mysql = "SELECT b.EJ_No, c.`Name`, b.totalAmount, b.totalMoney FROM ( SELECT a.EJ_No, SUM( a.Error_Correction_Total_Amount ) AS totalAmount, "
                    + "SUM(a.Error_Correction_Total_QTY) AS totalMoney FROM z_report_data AS a WHERE a.UserId = @UserId "
                    + "GROUP BY a.EJ_No ) AS b, cashier_table AS c WHERE b.EJ_No = c.Number ORDER BY b.EJ_No ASC";
                //配置SQL查询语句里的参数
                SQLiteParameter[] parameters = {
                };
                //执行查询，结果为DataTable类型
                DataTable dt = sqliteDBHelper_dmDB.ExecuteDataTable(sql_mysql, parameters);
                //判断查询结果是否为0行
                if (dt.Rows.Count == 0)
                {
                    //弹出提示框
                    //MessageBox.Show("No result!", "Tip", MessageBoxButton.OK, MessageBoxImage.Information);
                    dataGrid3_4.ItemsSource = null;//先清空表格内容
                    DataTable dt_temp = new DataTable();//新建临时表
                    dt_temp.Columns.Add(new DataColumn("Tip"));//添加列
                    dt_temp.Rows.Add(dt_temp.NewRow());//添加行
                    dt_temp.Rows[0]["Tip"] = "No result！";//设置行内容
                    dataGrid3_4.ItemsSource = dt_temp.DefaultView;//把表格放到控件中
                    return;
                }
                //查询出来的表格的字段名为英文，但显示给用户的时候要为中文，所以在此添加若干个中文字段名显示给用户
                dt.Columns.Add(new DataColumn("收银员编号"));
                dt.Columns.Add(new DataColumn("收银员名称"));
                dt.Columns.Add(new DataColumn("总更正数量"));
                dt.Columns.Add(new DataColumn("总更正金额"));
                //以下为给新添加的中文字段赋值
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["收银员编号"] = dt.Rows[i]["EJ_No"];
                    dt.Rows[i]["收银员名称"] = dt.Rows[i]["Name"];
                    dt.Rows[i]["总更正数量"] = dt.Rows[i]["totalAmount"];
                    dt.Rows[i]["总更正金额"] = dt.Rows[i]["totalMoney"];
                }
                dataGrid3_4.ItemsSource = dt.DefaultView;
                //以下为设置前若干个标题是英文字段 隐藏， 数量为select查询的的字段数
                int count = 4;//设置查询结果的字段数
                for (int i = 0; i < count; i++)
                {
                    this.dataGrid3_4.Columns[i].Visibility = System.Windows.Visibility.Hidden;
                }
                //以下为设置字段宽度
                this.dataGrid3_4.Columns[count++].Width = 120;
                this.dataGrid3_4.Columns[count++].Width = 120;
                this.dataGrid3_4.Columns[count++].Width = 120;
                this.dataGrid3_4.Columns[count++].Width = 100;
                #endregion
            }
            else if (ComboBox3_4.SelectionBoxItem.ToString().Equals("营业员税总报表"))
            {
                #region part3_4 总报表 营业员税总报表
                //SQL查询语句
                string sql_mysql = "SELECT ifnull(SUM(ifnull(Daily_Total_Sales,0)),0) AS totalMoney, ifnull(SUM(ifnull(Daily_Total_VAT,0)),0) AS totalVat FROM z_report_data WHERE UserId = @UserId";
                //配置SQL查询语句里的参数
                SQLiteParameter[] parameters = {
                };
                //执行查询，结果为DataTable类型
                DataTable dt = sqliteDBHelper_dmDB.ExecuteDataTable(sql_mysql, parameters);
                //判断查询结果是否为0行
                if (dt.Rows.Count == 0)
                {
                    //弹出提示框
                    //MessageBox.Show("No result!", "Tip", MessageBoxButton.OK, MessageBoxImage.Information);
                    dataGrid3_4.ItemsSource = null;//先清空表格内容
                    DataTable dt_temp = new DataTable();//新建临时表
                    dt_temp.Columns.Add(new DataColumn("Tip"));//添加列
                    dt_temp.Rows.Add(dt_temp.NewRow());//添加行
                    dt_temp.Rows[0]["Tip"] = "No result！";//设置行内容
                    dataGrid3_4.ItemsSource = dt_temp.DefaultView;//把表格放到控件中
                    return;
                }
                //查询出来的表格的字段名为英文，但显示给用户的时候要为中文，所以在此添加若干个中文字段名显示给用户
                //dt.Columns.Add(new DataColumn("税率"));
                dt.Columns.Add(new DataColumn("总额"));
                dt.Columns.Add(new DataColumn("税额"));
                //以下为给新添加的中文字段赋值
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    //dt.Rows[i]["税率"] = dt.Rows[i]["EJ_No"];
                    dt.Rows[i]["总额"] = dt.Rows[i]["totalMoney"];
                    dt.Rows[i]["税额"] = dt.Rows[i]["totalVat"];
                }
                dataGrid3_4.ItemsSource = dt.DefaultView;
                //以下为设置前若干个标题是英文字段 隐藏， 数量为select查询的的字段数
                int count = 2;//设置查询结果的字段数
                for (int i = 0; i < count; i++)
                {
                    this.dataGrid3_4.Columns[i].Visibility = System.Windows.Visibility.Hidden;
                }
                //以下为设置字段宽度
                //this.dataGrid3_4.Columns[count++].Width = 120;
                this.dataGrid3_4.Columns[count++].Width = 120;
                this.dataGrid3_4.Columns[count++].Width = 120;
                #endregion
            }
            else if (ComboBox3_4.SelectionBoxItem.ToString().Equals("单品总报表"))
            {
                #region part3_4 总报表 单品总报表
                //SQL查询语句
                string sql_mysql = "SELECT Number, Barcode, NAME, Department, Price, Total_Sales_Amount, Stock_Amount, Total_Sales_Qty FROM plu_info WHERE UserId = @UserId";
                //配置SQL查询语句里的参数
                SQLiteParameter[] parameters = {
                };
                //执行查询，结果为DataTable类型
                DataTable dt = sqliteDBHelper_dmDB.ExecuteDataTable(sql_mysql, parameters);
                //判断查询结果是否为0行
                if (dt.Rows.Count == 0)
                {
                    //弹出提示框
                    //MessageBox.Show("No result!", "Tip", MessageBoxButton.OK, MessageBoxImage.Information);
                    dataGrid3_4.ItemsSource = null;//先清空表格内容
                    DataTable dt_temp = new DataTable();//新建临时表
                    dt_temp.Columns.Add(new DataColumn("Tip"));//添加列
                    dt_temp.Rows.Add(dt_temp.NewRow());//添加行
                    dt_temp.Rows[0]["Tip"] = "No result！";//设置行内容
                    dataGrid3_4.ItemsSource = dt_temp.DefaultView;//把表格放到控件中
                    return;
                }
                //查询出来的表格的字段名为英文，但显示给用户的时候要为中文，所以在此添加若干个中文字段名显示给用户
                dt.Columns.Add(new DataColumn("单品编号"));
                dt.Columns.Add(new DataColumn("单品条形码"));
                dt.Columns.Add(new DataColumn("单品名称"));
                dt.Columns.Add(new DataColumn("单品关联部门"));
                dt.Columns.Add(new DataColumn("单价"));
                dt.Columns.Add(new DataColumn("销售数量（件）"));
                dt.Columns.Add(new DataColumn("销量总额"));
                dt.Columns.Add(new DataColumn("库存（件）"));
                //以下为给新添加的中文字段赋值
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["单品编号"] = dt.Rows[i]["Number"];
                    dt.Rows[i]["单品条形码"] = dt.Rows[i]["Barcode"];
                    dt.Rows[i]["单品名称"] = dt.Rows[i]["NAME"];
                    dt.Rows[i]["单品关联部门"] = dt.Rows[i]["Department"];
                    dt.Rows[i]["单价"] = dt.Rows[i]["Price"];
                    dt.Rows[i]["销售数量（件）"] = dt.Rows[i]["Total_Sales_Amount"];
                    dt.Rows[i]["销量总额"] = dt.Rows[i]["Total_Sales_Qty"];
                    dt.Rows[i]["库存（件）"] = dt.Rows[i]["Stock_Amount"];

                }
                dataGrid3_4.ItemsSource = dt.DefaultView;
                //以下为设置前若干个标题是英文字段 隐藏， 数量为select查询的的字段数
                int count = 8;//设置查询结果的字段数
                for (int i = 0; i < count; i++)
                {
                    this.dataGrid3_4.Columns[i].Visibility = System.Windows.Visibility.Hidden;
                }
                //以下为设置字段宽度
                this.dataGrid3_4.Columns[count++].Width = 90;
                this.dataGrid3_4.Columns[count++].Width = 160;
                this.dataGrid3_4.Columns[count++].Width = 120;
                this.dataGrid3_4.Columns[count++].Width = 110;
                this.dataGrid3_4.Columns[count++].Width = 80;
                this.dataGrid3_4.Columns[count++].Width = 110;
                this.dataGrid3_4.Columns[count++].Width = 100;
                this.dataGrid3_4.Columns[count++].Width = 100;
                #endregion
            }
        }
        #endregion
        /////////////////////////////////////////////////////////////////////////////////
        #region part4_1 客户管理 本地数据库
        ///<summary>
        ///part4_1 客户管理 添加到本地数据库
        ///</summary>
        private void button4_1_1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //声明一个添加信息的窗体
                Insert_Buyer ins_buyer = new Insert_Buyer(buyerDB);
                //弹出窗体
                ins_buyer.ShowDialog();
                //刷新查询框内容
                button4_1_2_Click();
                //设置分页按钮是否可用
                SetPageButtonEnabled_buyer();
                //设置控件显示信息
                SetPagerInfo_buyer(pageIndex_buyer, pageSize_buyer, pageCount_buyer, totalCount_buyer);
            }
            catch (Exception ee)
            {
                MessageBox.Show("The operation failed, please try again. Possible causes:" + ee.Message, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        ///<summary>
        ///part4_1 客户管理 查询本地数据库
        ///</summary>
        //private void button4_1_2_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        pageIndex_buyer = 1;
        //        //查询
        //        button4_1_2_Click();
        //        //设置分页按钮是否可用
        //        SetPageButtonEnabled_buyer();
        //        //设置控件显示信息
        //        SetPagerInfo_buyer(pageIndex_buyer, pageSize_buyer, pageCount_buyer, totalCount_buyer);
        //    }
        //    catch (Exception ee)
        //    {
        //        MessageBox.Show("The operation failed, please try again. Possible causes:" + ee.Message, "Information", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //}
        private int pageIndex_buyer = 1;// 当前页码
        private int pageSize_buyer = 30;// 分页大小
        private int totalCount_buyer = 0;// 记录总数
        private int pageCount_buyer = 0;// 总页数
        ///<summary>
        ///part4_1 客户管理 查询本地数据库 带分页
        ///</summary>
        private void button4_1_2_Click()
        {
            //SQL语句
            string sql_count = "SELECT COUNT(*) AS COUNTS FROM Buyer_Info";
            //执行查询，结果为DataTable类型
            DataTable dt_count = sqliteDBHelper_buyerDB.ExecuteDataTable(sql_count, null);
            // 记录总数
            totalCount_buyer = Convert.ToInt32(dt_count.Rows[0]["COUNTS"]);
            // 总页数
            pageCount_buyer = (int)Math.Ceiling((double)totalCount_buyer / pageSize_buyer);
            //SQL查询语句
            string sql = "SELECT Number,Name,BPN AS TPIN,VAT AS TAX_ACC_Name,Address,Tel FROM "
                + "(SELECT * FROM Buyer_Info ORDER BY Number ASC LIMIT @pageSize*@pageIndex) LIMIT @pageSize offset @pageSize*@pageIndexbefore;";
            //配置SQL语句里的参数
            SQLiteParameter[] parameters = {
                    new SQLiteParameter("@pageSize",pageSize_buyer),
                    new SQLiteParameter("@pageIndex",pageIndex_buyer),
                    new SQLiteParameter("@pageIndexbefore",pageIndex_buyer - 1),
                };
            //执行查询，结果为DataTable类型
            DataTable dt = sqliteDBHelper_buyerDB.ExecuteDataTable(sql, parameters);
            //判断查询结果是否为0行
            if (dt.Rows.Count == 0)
            {
                dataGrid4_1.ItemsSource = null;//先清空表格内容
                DataTable dt_temp = new DataTable();//新建临时表
                dt_temp.Columns.Add(new DataColumn("No data！"));//添加列
                                                                //dt_temp.Rows.Add(dt_temp.NewRow());//添加行
                                                                //dt_temp.Rows[0]["Tip"] = "No result！";//设置行内容
                dataGrid4_1.ItemsSource = dt_temp.DefaultView;//把表格放到控件中
                return;
            }
            //设置列类型
            dataGrid4_1.ItemsSource = dt.DefaultView;
            //count从0开始
            int count = 0;
            //以下为设置字段宽度
            this.dataGrid4_1.Columns[count++].Width = 70;
            this.dataGrid4_1.Columns[count++].Width = 160;
            this.dataGrid4_1.Columns[count++].Width = 160;
            this.dataGrid4_1.Columns[count++].Width = 160;
            this.dataGrid4_1.Columns[count++].Width = 200;
            this.dataGrid4_1.Columns[count++].Width = 120;
        }
        /// <summary>
        /// 设置分页按钮是否可用
        /// </summary>
        private void SetPageButtonEnabled_buyer()
        {
            //确定分页按钮的是否可用
            if (pageCount_buyer <= 1)
            {
                btnPageDown_buyer.IsEnabled = false;
                btnPageUp_buyer.IsEnabled = false;
                btnEndPage_buyer.IsEnabled = false;
            }
            else
            {
                if (pageIndex_buyer == pageCount_buyer)
                {
                    btnPageDown_buyer.IsEnabled = false;
                    btnPageUp_buyer.IsEnabled = true;
                    btnEndPage_buyer.IsEnabled = false;
                }
                else if (pageIndex_buyer <= 1)
                {
                    btnPageDown_buyer.IsEnabled = true;
                    btnPageUp_buyer.IsEnabled = false;
                    btnEndPage_buyer.IsEnabled = true;
                }
                else
                {
                    btnPageDown_buyer.IsEnabled = true;
                    btnPageUp_buyer.IsEnabled = true;
                    btnEndPage_buyer.IsEnabled = true;
                }
            }
        }
        /// <summary>
        /// 设置控件显示信息
        /// </summary>
        /// <param name="pageIndex">当前页码</param>
        /// <param name="pageCount">共有页数</param>
        /// <param name="totalCount">总记录条数</param>
        private void SetPagerInfo_buyer(int pageIndex, int pageSize, int pageCount, int totalCount)
        {
            txtPagerInfo_buyer.Text = String.Format("PER PAGE: {0}   ALL: {1}   PAGE: {2} / {3} ", pageSize_buyer, totalCount_buyer, pageIndex_buyer, pageCount_buyer);
        }
        /// <summary>
        /// 首页按钮事件
        /// </summary>
        private void btnFirstPage_buyer_Click(object sender, RoutedEventArgs e)
        {
            pageIndex_buyer = 1;
            button4_1_2_Click();//查询
            SetPageButtonEnabled_buyer();//设置分页按钮是否可用
            SetPagerInfo_buyer(pageIndex_buyer, pageSize_buyer, pageCount_buyer, totalCount_buyer);//设置控件显示信息
        }
        /// <summary>
        /// 下一页按钮事件
        /// </summary>
        private void btnPageDown_buyer_Click(object sender, RoutedEventArgs e)
        {
            pageIndex_buyer++;
            button4_1_2_Click();//查询
            SetPageButtonEnabled_buyer();//设置分页按钮是否可用
            SetPagerInfo_buyer(pageIndex_buyer, pageSize_buyer, pageCount_buyer, totalCount_buyer);//设置控件显示信息
        }
        /// <summary>
        /// 上一页按钮事件
        /// </summary>
        private void btnPageUp_buyer_Click(object sender, RoutedEventArgs e)
        {
            pageIndex_buyer--;
            button4_1_2_Click();//查询
            SetPageButtonEnabled_buyer();//设置分页按钮是否可用
            SetPagerInfo_buyer(pageIndex_buyer, pageSize_buyer, pageCount_buyer, totalCount_buyer);//设置控件显示信息
        }
        /// <summary>
        /// 尾页按钮事件
        /// </summary>
        private void btnEndPage_buyer_Click(object sender, RoutedEventArgs e)
        {
            pageIndex_buyer = pageCount_buyer;
            button4_1_2_Click();//查询
            SetPageButtonEnabled_buyer();//设置分页按钮是否可用
            SetPagerInfo_buyer(pageIndex_buyer, pageSize_buyer, pageCount_buyer, totalCount_buyer);//设置控件显示信息
        }
        ///<summary>
        ///part4_1 客户管理 删除
        ///</summary>
        private void button4_1_3_Click(object sender, RoutedEventArgs e)
        {
            //弹出提示框，未选择要删除的数据
            if (dataGrid4_1.SelectedItem == null)
            {
                MessageBox.Show("Please select one record to delete first!", "Information", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            //选择了某一行数据时
            else
            {
                //获取选中行所有数据
                DataRowView mySelectedElement = (DataRowView)dataGrid4_1.SelectedItem;
                //弹出确认框，当为确定时
                if (MessageBox.Show("Confirm that you want to delete the record ( the Number： " + mySelectedElement.Row["Number"].ToString() + " )？", "Information", MessageBoxButton.OKCancel, MessageBoxImage.Information) == MessageBoxResult.OK)
                {
                    //获取必要信息
                    string Number = mySelectedElement.Row["Number"].ToString();
                    //SQL语句
                    string sql = "DELETE FROM Buyer_Info WHERE Number=@Number";
                    //配置SQL语句里的参数
                    SQLiteParameter[] parameters = {
                    new SQLiteParameter("@Number",Number)
                    };
                    //执行SQL，并做判断
                    if (sqliteDBHelper_buyerDB.ExecuteNonQuery(sql, parameters) == 1)
                    {
                        //刷新查询框内容
                        button4_1_2_Click();
                        //设置分页按钮是否可用
                        SetPageButtonEnabled_buyer();
                        //设置控件显示信息
                        SetPagerInfo_buyer(pageIndex_buyer, pageSize_buyer, pageCount_buyer, totalCount_buyer);
                    }
                    else
                    {
                        MessageBox.Show("Delete failed!", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
        ///<summary>
        ///part4_1 客户管理 修改本地数据库
        ///</summary>
        private void button4_1_4_Click(object sender, RoutedEventArgs e)
        {
            //弹出提示框，未选择要删除的数据
            if (dataGrid4_1.SelectedItem == null)
            {
                MessageBox.Show("Please select one of the data you want to modify!", "Information", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            //选择了某一行数据时
            else
            {
                //获取选中行所有数据
                DataRowView mySelectedElement = (DataRowView)dataGrid4_1.SelectedItem;
                //声明一个变量
                Buyer buyer = new Buyer();
                //变量赋值
                buyer.Number = mySelectedElement.Row["Number"].ToString();
                buyer.Name = mySelectedElement.Row["Name"].ToString();
                buyer.BPN = mySelectedElement.Row["TPIN"].ToString();
                buyer.Address = mySelectedElement.Row["Address"].ToString();
                buyer.Tel = mySelectedElement.Row["Tel"].ToString();
                buyer.VAT = mySelectedElement.Row["TAX_ACC_Name"].ToString();
                //声明一个修改信息的窗体
                Update_Buyer upd_buyer = new Update_Buyer(buyerDB, buyer);
                //弹出窗体
                upd_buyer.ShowDialog();
                //刷新查询框内容
                button4_1_2_Click();
                //设置分页按钮是否可用
                SetPageButtonEnabled_buyer();
                //设置控件显示信息
                SetPagerInfo_buyer(pageIndex_buyer, pageSize_buyer, pageCount_buyer, totalCount_buyer);
            }
        }
        ///<summary>
        ///part4_1 客户管理 查询 按条件
        ///</summary>
        private void button4_1_5_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //声明一个添加信息的窗体
                Buyer_Query ins_buyer = new Buyer_Query(buyerDB);
                //弹出窗体
                ins_buyer.ShowDialog();
                //从第一页开始查
                pageIndex_buyer = 1;
                //查询
                button4_1_2_Click();
                //设置分页按钮是否可用
                SetPageButtonEnabled_buyer();
                //设置控件显示信息
                SetPagerInfo_buyer(pageIndex_buyer, pageSize_buyer, pageCount_buyer, totalCount_buyer);
            }
            catch (Exception ee)
            {
                MessageBox.Show("The operation failed, please try again. Possible causes:" + ee.Message, "Information", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        #endregion
        /////////////////////////////////////////////////////////////////////////////////
    }
}
