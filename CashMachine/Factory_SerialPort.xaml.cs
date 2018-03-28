using System;
using System.IO.Ports;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace CashMachine
{
    /// <summary>
    /// Factory_SerialPort.xaml 的交互逻辑
    /// </summary>
    public partial class Factory_SerialPort : Window
    {
        public Factory_SerialPort()
        {
            InitializeComponent();
            this.ResizeMode = ResizeMode.CanMinimize;//禁用“最大化”按钮
            listAvailablePorts();
            button1.IsEnabled = button2.IsEnabled = button3.IsEnabled = false;
        }
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
        private string serialPortName = null;//串口名
        private int baudRate;//波特率
        private Parity parity;//校验位
        private int dataBits;//数据位
        private StopBits stopBits;//停止位
        private SerialPort serialPort;//串口
        /// <summary>
        /// part1_1 打开端口
        /// </summary>
        private void openPort_Click(object sender, RoutedEventArgs e)
        {
            if (openPort_button.Content.Equals("打开"))
            {
                if (chuankou.SelectionBoxItem.ToString().Equals(""))
                {
                    //弹出提示
                    MessageBox.Show("请选择串口号!", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    //通信参数前后台转换
                    transport_args();
                    //设置通信，参数使用指定的端口名称、波特率、奇偶校验位、数据位和停止位
                    serialPort = new SerialPort(serialPortName, baudRate, parity, dataBits, stopBits);
                    //打开串口
                    serialPort.Open();
                    //设置监听事件，当端口一有数据就执行此函数
                    serialPort.DataReceived += COM_DataReceived;
                    //串口打开后就改变按钮内容
                    openPort_button.Content = "关闭";
                    button1.IsEnabled = button2.IsEnabled = button3.IsEnabled = true;
                }
            }
            else
            {
                //关闭串口
                serialPort.Close();
                serialPort = null;
                iii = ppp = 0;
                //串口关闭后就改变按钮内容
                openPort_button.Content = "打开";
                button1.IsEnabled = button2.IsEnabled = button3.IsEnabled = false;
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

        private byte[] tempAll_net = new byte[1024];// 每当端口有数据时就加入到此数组中
        private volatile int iii = 0;// 负责tempAll数组的移位工作,volatile修饰后变量在所有线程中必须是同步的
        private volatile int ppp = 0;// 工作指针

        ///<summary>
        ///part1_1 监听事件，当端口一有数据就执行此函数
        ///</summary>
        private void COM_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                int n = serialPort.BytesToRead;//先记录下来，避免某种原因，人为的原因，操作几次之间时间长，缓存不一致
                byte[] buffer = new byte[n];//声明一个临时数组存储当前来的串口数据
                serialPort.Read(buffer, 0, n);//读取缓冲数据

                //当缓存区存满时,清零
                if (iii + n >= 1024 || iii < ppp || iii - ppp > 1000)
                {
                    iii = 0;// 计数从头开始
                    ppp = 0;
                    return;
                }
                //Console.WriteLine("本次接收到数据长度：" + n);
                //Console.WriteLine(byteToHexStr(buffer));

                // 将本次接收的数据存到全局变量tempAll数组中
                for (int w = 0; w < n; w++)
                {
                    tempAll_net[iii] = buffer[w];
                    iii++;
                }
                //Console.Write("  此时 ppp ：" + ppp + ", iii：" + iii);
                // 对接收下来的数据段进行处理
                if (iii - ppp >= 3)
                {
                    #region 前三位是02 31
                    // 判断前三位是02 31
                    if (tempAll_net[ppp] == (byte)0x02
                            && tempAll_net[ppp + 1] == (byte)0x31)
                    {
                        //判断len长度
                        byte[] len = new byte[4];//4字节长度
                        len[0] = tempAll_net[ppp + 2];
                        len[1] = (byte)0x00;
                        len[2] = (byte)0x00;
                        len[3] = (byte)0x00;
                        int dataLen = BitConverter.ToInt32(len, 0);//byte数组转int，DATA的长度
                        //Console.WriteLine("DATA段的数据长度：" + dataLen);
                        //如果所有字节都接收完毕时
                        if (iii - ppp >= 5 + dataLen)
                        {
                            //LR C校验
                            if (!tempAll_net[ppp + 3 + dataLen].Equals(getLRCHash(tempAll_net, ppp, 3 + dataLen)))
                            {
                                //Console.WriteLine("READ_IMEI LRC校验失败");
                                MessageBox.Show("LRC校验失败！", "提示", MessageBoxButton.OK, MessageBoxImage.Error);

                            }
                            else
                            {
                                //解析字节数组
                                string serialNumber = Encoding.Default.GetString(tempAll_net, ppp + 3, dataLen).Trim('\0');//17bytes序列号
                                //Console.WriteLine("机器IMEI：" + serialNumber);
                                interfaceUpdateHandle = new HandleInterfaceUpdateDelagate(UpdateTextBox);//实例化委托对象
                                Dispatcher.Invoke(interfaceUpdateHandle, serialNumber);   //因为要访问ui资源，所以需要使用invoke方式同步ui
                                iii = ppp = 0;
                                //Console.WriteLine("ppp iii此时置0了.");
                            }
                        }
                        else
                        {
                            return;
                        }
                    }
                    #endregion
                    else
                    {
                        ppp++;//为了容错，ppp依次往后移动
                    }
                }
            }
            catch (Exception ex)
            {
                iii = ppp = 0;
                return;
            }
        }
        //委托，将接收的内容显示到输入框中
        delegate void HandleInterfaceUpdateDelagate(string text);//委托
        HandleInterfaceUpdateDelagate interfaceUpdateHandle;
        ///<summary>
        ///part1_1 更新显示框中的数据，委托对象
        ///</summary>
        private void UpdateTextBox(string msg)
        {
            textBox3.Text = msg;
        }
        /// <summary>
        /// 发送SN
        /// </summary>
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if (textBox1.Text.Length != 12)
            {
                MessageBox.Show("SN 的长度固定为12位！", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                sendFiletoXWJ_TERMINAL_SN(textBox1.Text);
                //SN自动加1
                textBox1.Text = (Convert.ToDouble(textBox1.Text) + 1).ToString();
            }
        }
        /// <summary>
        /// 发送MAC
        /// </summary>
        private void button2_Click(object sender, RoutedEventArgs e)
        {
            //正则判断
            Regex regex = new Regex(@"^(([A-Fa-f0-9]{2}:){5}[A-Fa-f0-9]{2})$");//需要加上@
            if (regex.Matches(textBox2.Text).Count > 0)
            {
                sendFiletoXWJ_ETHERNET_MAC(textBox2.Text);
                //MAC 自动加1
                textBox2.Text = MAC_plus1(textBox2.Text);
            }
            //如果不匹配
            else
            {
                MessageBox.Show("MAC地址的格式不正确！", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        /// <summary>
        /// 读取IMEI
        /// </summary>
        private void button3_Click(object sender, RoutedEventArgs e)
        {
            sendFiletoXWJ_READ_IMEI();
        }
        /// <summary>
        /// 只允许输入0-9数字
        /// </summary>
        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back || e.Key == Key.D0 || e.Key == Key.D1 || e.Key == Key.D2 || e.Key == Key.D3 || e.Key == Key.D4 || e.Key == Key.D5 || e.Key == Key.D6 || e.Key == Key.D7 || e.Key == Key.D8 || e.Key == Key.D9 || e.Key == Key.NumPad0 || e.Key == Key.NumPad1 || e.Key == Key.NumPad2 || e.Key == Key.NumPad3 || e.Key == Key.NumPad4 || e.Key == Key.NumPad5 || e.Key == Key.NumPad6 || e.Key == Key.NumPad7 || e.Key == Key.NumPad8 || e.Key == Key.NumPad9)
            {
                e.Handled = false;//可以接受该事件
            }
            else
            {
                e.Handled = true;//为true时表示已经处理了事件（即不处理当前键盘事件  不接受）
            }
        }
        /////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// WRITE_TERMINAL_SN
        /// </summary>
        public void sendFiletoXWJ_TERMINAL_SN(string sn)
        {
            //DATA的长度
            int dataLength = 12;
            //总发送数据的长度
            int dataSendLength = 5 + dataLength;
            //总发送串
            byte[] dataSend = new byte[dataSendLength];
            //接下来拼接发送串
            dataSend[0] = 0x02;//起始码 STX
            dataSend[1] = 0x20;//命令码 INS

            //将DATA的长度转换为byte[]数组，2位，低位在前 高位在后
            byte[] dataLengthBytes = BitConverter.GetBytes(dataLength);
            dataSend[2] = dataLengthBytes[0];//长度的低位 LENL
            //dataSend[4] = dataLengthBytes[1];//长度的高位 LENH

            //将SN写入发送串  DATA部分
            byte[] machineSN = Encoding.Default.GetBytes(sn);//8
            for (int i = 3; i < 3 + machineSN.Length; i++)
            {
                dataSend[i] = machineSN[i - 3];
            }

            //计算LRC校验码
            byte xorResult = dataSend[0];
            // 求xor校验和，注意：XOR运算从第二元素开始
            for (int i = 1; i < 3 + 12; i++)
            {
                xorResult ^= dataSend[i];
            }
            dataSend[dataSendLength - 2] = xorResult;//LRC校验码

            //结束码
            dataSend[dataSendLength - 1] = 0x03;
            //发送
            serialPort.Write(dataSend, 0, dataSendLength);
            //Console.WriteLine("电脑端发送了：" + byteToHexStr(dataSend));
        }
        /// <summary>
        /// WRITE_ETHERNET_MAC
        /// </summary>
        public void sendFiletoXWJ_ETHERNET_MAC(string mac)
        {
            //DATA的长度
            int dataLength = 17;
            //总发送数据的长度
            int dataSendLength = 5 + dataLength;
            //总发送串
            byte[] dataSend = new byte[dataSendLength];
            //接下来拼接发送串
            dataSend[0] = 0x02;//起始码 STX
            dataSend[1] = 0x22;//命令码 INS

            //将DATA的长度转换为byte[]数组，2位，低位在前 高位在后
            byte[] dataLengthBytes = BitConverter.GetBytes(dataLength);
            dataSend[2] = dataLengthBytes[0];//长度的低位 LENL
            //dataSend[4] = dataLengthBytes[1];//长度的高位 LENH

            //将SN写入发送串  DATA部分
            byte[] machineSN = Encoding.Default.GetBytes(mac);
            for (int i = 3; i < 3 + machineSN.Length; i++)
            {
                dataSend[i] = machineSN[i - 3];
            }

            //计算LRC校验码
            byte xorResult = dataSend[0];
            // 求xor校验和，注意：XOR运算从第二元素开始
            for (int i = 1; i < 3 + 17; i++)
            {
                xorResult ^= dataSend[i];
            }
            dataSend[dataSendLength - 2] = xorResult;//LRC校验码

            //结束码
            dataSend[dataSendLength - 1] = 0x03;
            //发送
            serialPort.Write(dataSend, 0, dataSendLength);
            //Console.WriteLine("电脑端发送了：" + byteToHexStr(dataSend));
        }
        /// <summary>
        /// READ_IMEI
        /// </summary>
        public void sendFiletoXWJ_READ_IMEI()
        {
            //DATA的长度
            int dataLength = 1;
            //总发送数据的长度
            int dataSendLength = 5 + dataLength;
            //总发送串
            byte[] dataSend = new byte[dataSendLength];
            //接下来拼接发送串
            dataSend[0] = 0x02;//起始码 STX
            dataSend[1] = 0x31;//命令码 INS

            //将DATA的长度转换为byte[]数组，2位，低位在前 高位在后
            byte[] dataLengthBytes = BitConverter.GetBytes(dataLength);
            dataSend[2] = dataLengthBytes[0];//长度的低位 LENL
            dataSend[3] = 0x00;
            //计算LRC校验码
            byte xorResult = dataSend[0];
            // 求xor校验和，注意：XOR运算从第二元素开始
            for (int i = 1; i < 3 + 1; i++)
            {
                xorResult ^= dataSend[i];
            }
            dataSend[dataSendLength - 2] = xorResult;//LRC校验码
            //结束码
            dataSend[dataSendLength - 1] = 0x03;
            //发送
            serialPort.Write(dataSend, 0, dataSendLength);
            //Console.WriteLine("电脑端发送了：" + byteToHexStr(dataSend));
        }
        /// <summary>
        /// 点击串口的时候自动刷新串口号
        /// </summary>
        public void chuankou_DropDownOpened(object sender, EventArgs e)
        {
            listAvailablePorts();
        }
        /// <summary>
        /// MAC号自动加1
        /// 输入old 的MAC号，返回自增1后的MAC号
        /// </summary>
        public string MAC_plus1(string MAC)
        {
            string mac = MAC;
            mac = mac.Replace(":", "");//去掉：号
            string[] temp = new string[6];
            //将66:BB:86:2A:9A:7A分别赋值到字符串数组中
            for (int i = 0; i < temp.Length; i++)
            {
                temp[i] = mac.Substring(i * 2, 2);
            }

            int[] temp2 = new int[6];
            for (int i = 0; i < 6; i++)
            {
                temp2[i] = int.Parse(temp[i], System.Globalization.NumberStyles.AllowHexSpecifier);
            }
            //自增处理
            if (temp2[5] != 255)//当末尾不是FF时
            {
                temp2[5]++;
            }
            else//当末尾是FF时
            {
                temp2[5] = 0;
                if (temp2[4] != 255)//不是FF时
                {
                    temp2[4]++;
                }
                else
                {
                    temp2[4] = 0;
                    if (temp2[3] != 255)//不是FF时
                    {
                        temp2[3]++;
                    }
                    else
                    {
                        temp2[3] = 0;
                        if (temp2[2] != 255)//不是FF时
                        {
                            temp2[2]++;
                        }
                        else
                        {
                            temp2[2] = 0;
                            if (temp2[1] != 255)//不是FF时
                            {
                                temp2[1]++;
                            }
                            else
                            {
                                temp2[1] = 0;
                                if (temp2[0] != 255)//不是FF时
                                {
                                    temp2[0]++;
                                }
                                else
                                {
                                    temp2[0] = 0;
                                }
                            }
                        }
                    }
                }
            }
            //十进制转为十六进制
            for (int i = 0; i < 6; i++)
            {
                temp[i] = temp2[i].ToString("X2");
            }
            string result = "";
            for (int i = 0; i < 6; i++)//将结果拼接成一个字符串
            {
                result += temp[i];
                if (i < 5)
                {
                    result += ":";
                }
            }
            return result;
        }
        /// <summary>
        /// 字节数组转16进制字符串
        /// </summary>
        public static string byteToHexStr(byte[] bytes)
        {
            string returnStr = "";
            if (bytes != null)
            {
                //bytes.Length
                for (int i = 0; i < bytes.Length; i++)
                {
                    returnStr += bytes[i].ToString("X2") + "  ";//2代表2位
                }
            }
            return returnStr;
        }
        /// <summary>  
        /// LRC校验 异或值
        /// buffer 待校验字节数组,offset 偏移量,length 校验长度
        /// 返回值是一个字节对象
        /// </summary>
        public static byte getLRCHash(byte[] buffer, int offset, int length)
        {
            //计算LRC校验码
            byte xorResult = buffer[offset];
            // 求xor校验和，注意：XOR运算从第二元素开始
            for (int i = offset + 1; i < offset + length; i++)
            {
                xorResult ^= buffer[i];
            }
            return xorResult;//LRC校验码
        }
        /////////////////////////////////////////////////////////////////////////////////
        #endregion
    }
}