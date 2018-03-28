using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CheckUtils;
using System.Net;
using System.Windows;
using MySql.Data.MySqlClient;
using CashMachine.MysqlDB;
using System.Data;
using log4net;
using System.Reflection;

namespace CashMachine.utils
{
    /// <summary>
    /// 以太网连接
    /// </summary>
    class EthernetConnection
    {
        /////////////////////////////////////////////////////////////////////////////////
        //一些公共变量
        private string host = null;//默认主机IP
        private int port;//默认端口号
        private string databaseURL = "D:\\database\\";//数据库地址统一开头

        Socket serverSocket = null;//服务器socket
        Thread serverThread = null;//
        //ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        /////////////////////////////////////////////////////////////////////////////////
        //无参构造函数
        public EthernetConnection()
        {
        }
        //有参构造函数
        public EthernetConnection(string host, int port)
        {
            this.host = host;
            this.port = port;
            Init();
        }
        /////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 初始化 创建服务器端Socket
        /// </summary>
        public void Init()
        {
            try
            {
                if (serverSocket == null)
                {
                    //ip地址
                    //IPAddress ip = IPAddress.Parse(host);
                    //端口号
                    IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, port);//服务器端监听任意端口
                    //使用IPv4地址，流式socket方式，tcp协议传递数据
                    serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    //创建好socket后，必须告诉socket绑定的IP地址和端口号 //socket监听哪个端口
                    serverSocket.Bind(ipEndPoint);
                    //同一个时间点过来10个客户端，排队 准备好接收连接
                    serverSocket.Listen(100);
                    Console.WriteLine("服务端已开启,等待客户端连接...");
                    //log.Info("服务端已开启,等待客户端连接...");
                    //sSocket = sSocket.Accept();
                    //开启线程
                    serverThread = new Thread(AcceptInfo);
                    serverThread.IsBackground = true;
                    serverThread.Start(serverSocket);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error:" + ex.Message, "Information", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        /// <summary>
        /// 关闭操作 将一些全局变量置0 防止系统紊乱
        /// </summary>
        public void Close()
        {
            //清除目录的内容
            dictionary.Clear();
            //清除服务器socket
            if (serverSocket != null)
            {
                serverSocket.Close();//
                serverSocket = null;
            }
            //清除服务器socket线程
            if (serverThread != null)
            {
                try
                {
                    serverThread.Abort();
                }
                catch (Exception)
                {
                }
                serverThread = null;
            }
        }
        //记录通信用的Socket
        //Dictionary<string, Socket> dictionary = new Dictionary<string, Socket>();
        //记录通信用的Socket里的实体
        Dictionary<string, SocketEntity> dictionary = new Dictionary<string, SocketEntity>();
        /// <summary>
        /// 服务器监听
        /// </summary>
        public void AcceptInfo(object obj)
        {
            Socket socket = obj as Socket;
            while (true)
            {
                Thread clientThread = null;
                //通信用socket
                try
                {
                    //创建通信用的Socket 接收连接 为新建的连接建立新的Socket目的为客户端将要建立连接
                    Socket clientSocket = socket.Accept();
                    //远程终点节 客户端的IP和端口号,如：192.168.1.120:8080
                    string remoteEndPoint = clientSocket.RemoteEndPoint.ToString();
                    //IPEndPoint endPoint = (IPEndPoint)client.RemoteEndPoint;
                    //string me = Dns.GetHostName();//得到本机名称
                    //MessageBox.Show(me);
                    Console.WriteLine("与客户端建立一个连接...");
                    //log.Info("与客户端建立一个连接...");
                    //cboIpPort.Items.Add(point);
                    //判断此客户端的IP和端口号是否已存在
                    if (!dictionary.ContainsKey(remoteEndPoint))//不存在
                    {
                        Console.WriteLine("开始向dictionary目录中添加一个记录。");
                        dictionary.Add(remoteEndPoint, new SocketEntity(clientSocket));
                    }
                    else//已存在，先移除在添加
                    {
                        Console.WriteLine("开始从dictionary目录删除原有的一个记录。");
                        dictionary.Remove(remoteEndPoint);
                        dictionary.Add(remoteEndPoint, new SocketEntity(clientSocket));
                    }
                    Console.WriteLine("一个实体创建成功！");
                    //为每个客户端开启线程 接收消息
                    clientThread = new Thread(ReceiveMsg);
                    clientThread.IsBackground = true;
                    clientThread.Start(clientSocket);
                }
                catch (Exception ex)
                {
                    //MessageBox.Show(ex.Message, "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                    Thread.CurrentThread.Abort();
                    Console.WriteLine("关闭最外层的线程，即关闭了所有的dictionary中元素！");
                    break;
                }
            }
            //socket.Close();
            //socket = null;
            //Console.WriteLine("关闭最外层的线程，即关闭了所有的dictionary中元素！");
        }

        //private byte[] tempAll_net = new byte[1024 * 20];// 每当端口有数据时就加入到此数组中
        //private volatile int iii = 0;// 负责tempAll数组的移位工作,volatile修饰后变量在所有线程中必须是同步的
        //private volatile int ppp = 0;// 工作指针
        //private byte[] receivedFile;//下位机要上传的文件缓存区
        //private volatile int receivedFileLength = 0;// 下位机要上传的文件的长度
        //private volatile int pppp = 0;// 下位机要上传的文件缓存区工作指针
        //private string receivedFileMD5Hash;//下位机要上传的文件的校验码
        /// <summary>
        /// 接收消息
        /// </summary>
        public void ReceiveMsg(object obj)
        {
            Socket clientSocket = obj as Socket;
            while (true)
            {
                //接收客户端发送过来的数据
                try
                {
                    //定义byte数组存放从客户端接收过来的数据
                    byte[] buffer = new byte[1024 * 2];
                    //将接收过来的数据放到buffer中，并返回实际接受数据的长度
                    int n = clientSocket.Receive(buffer);
                    //n为0时，说明断开连接
                    //string words = Encoding.Default.GetString(buffer, 0, n);
                    //Console.WriteLine(clientSocket.RemoteEndPoint.ToString() + ":" + words);

                    //如果接受长度为0，说明客户端已断开连接，此时需要及时断开和资源释放
                    if (n == 0)
                    {
                        Console.Write("客户端已断开连接！");
                        dictionary.Remove(clientSocket.RemoteEndPoint.ToString());
                        Console.Write("dictionary中移除了：" + clientSocket.RemoteEndPoint.ToString());
                        Console.WriteLine("，dictionary中还剩余：");
                        //log.Info("dictionary中还剩余：");
                        foreach (var item in dictionary)
                        {
                            Console.WriteLine(item.Key);
                            //log.Info(item.Key);
                        }
                        clientSocket.Close();
                        //Thread.CurrentThread.Abort();
                        break;
                    }

                    // //当缓存区存满时,清零
                    if (dictionary[clientSocket.RemoteEndPoint.ToString()].iii + n >= 1024 * 3 || dictionary[clientSocket.RemoteEndPoint.ToString()].iii < dictionary[clientSocket.RemoteEndPoint.ToString()].ppp || dictionary[clientSocket.RemoteEndPoint.ToString()].iii - dictionary[clientSocket.RemoteEndPoint.ToString()].ppp > 2000)
                    {
                        dictionary[clientSocket.RemoteEndPoint.ToString()].iii = 0;// 计数从头开始
                        dictionary[clientSocket.RemoteEndPoint.ToString()].ppp = 0;
                        break;
                    }

                    // 将本次接收的数据存到全局变量tempAll数组中
                    for (int w = 0; w < n; w++)
                    {
                        dictionary[clientSocket.RemoteEndPoint.ToString()].tempAll_net[dictionary[clientSocket.RemoteEndPoint.ToString()].iii] = buffer[w];
                        dictionary[clientSocket.RemoteEndPoint.ToString()].iii++;
                    }
                    //log.Info("本次数据由：" + clientSocket.RemoteEndPoint.ToString() + "发送来,长度：" + n + ",内容是：；" );
                    Console.Write("本次数据由：" + clientSocket.RemoteEndPoint.ToString() + "发送来,长度：" + n + ",内容是：；");
                    //Console.Write(byteToHexStr(buffer, n));//输出接收数据
                    Console.Write("此时，头位置指针 iii：" + dictionary[clientSocket.RemoteEndPoint.ToString()].iii + ", 工作指针 ppp ：" + dictionary[clientSocket.RemoteEndPoint.ToString()].ppp);

                    ////判断下位机最后一次修改系统的时间距离现在是否超时，如果超时则需要用户重新登录
                    //if ((DateTime.UtcNow.Ticks - new DateTime(1970, 1, 1).Ticks) / 10000000 - dictionary[clientSocket.RemoteEndPoint.ToString()].lastModifyTime >= 8)//单位为秒
                    //{
                    //    dictionary[clientSocket.RemoteEndPoint.ToString()].socket.Send(Result_Error_TimeOver());
                    //    dictionary[clientSocket.RemoteEndPoint.ToString()].ClearAll();
                    //}
                    ////设置下位机最后一次修改系统的时间，时间戳
                    //else
                    //{
                    //    dictionary[clientSocket.RemoteEndPoint.ToString()].lastModifyTime = (DateTime.UtcNow.Ticks - new DateTime(1970, 1, 1).Ticks) / 10000000;
                    //}

                    //Thread timeThread = new Thread(timeClear);
                    //timeThread.IsBackground = true;
                    //timeThread.Start(clientSocket.RemoteEndPoint.ToString());
                    //Console.WriteLine("timeClear函数已执行");


                    // 对接收下来的数据段进行处理
                    if (dictionary[clientSocket.RemoteEndPoint.ToString()].iii - dictionary[clientSocket.RemoteEndPoint.ToString()].ppp >= 12)
                    {
                        #region 前三位是02 00 a1下位机下载电脑上的文件 获取文件信息
                        // 判断前三位是02 00 a1
                        if (dictionary[clientSocket.RemoteEndPoint.ToString()].tempAll_net[dictionary[clientSocket.RemoteEndPoint.ToString()].ppp] == (byte)0x02
                                && dictionary[clientSocket.RemoteEndPoint.ToString()].tempAll_net[dictionary[clientSocket.RemoteEndPoint.ToString()].ppp + 1] == (byte)0x00
                                && dictionary[clientSocket.RemoteEndPoint.ToString()].tempAll_net[dictionary[clientSocket.RemoteEndPoint.ToString()].ppp + 2] == (byte)0xa1)
                        {
                            //判断len长度
                            byte[] len = new byte[4];//4字节长度
                            len[0] = dictionary[clientSocket.RemoteEndPoint.ToString()].tempAll_net[dictionary[clientSocket.RemoteEndPoint.ToString()].ppp + 3];
                            len[1] = dictionary[clientSocket.RemoteEndPoint.ToString()].tempAll_net[dictionary[clientSocket.RemoteEndPoint.ToString()].ppp + 4];
                            len[2] = (byte)0x00;
                            len[3] = (byte)0x00;
                            int dataLen = BitConverter.ToInt32(len, 0);//byte数组转int，DATA的长度
                                                                       //Console.WriteLine("DATA段的数据长度：" + dataLen);

                            //如果所有字节都接收完毕时
                            if (dictionary[clientSocket.RemoteEndPoint.ToString()].iii - dictionary[clientSocket.RemoteEndPoint.ToString()].ppp >= 11 + dataLen)
                            {
                                //LRC 校验
                                if (!dictionary[clientSocket.RemoteEndPoint.ToString()].tempAll_net[dictionary[clientSocket.RemoteEndPoint.ToString()].ppp + 5].Equals(LRCCheck.getLRCHash(dictionary[clientSocket.RemoteEndPoint.ToString()].tempAll_net, dictionary[clientSocket.RemoteEndPoint.ToString()].ppp, 5)))
                                {
                                    dictionary[clientSocket.RemoteEndPoint.ToString()].socket.Send(Result_Error_LRC());
                                    //Console.WriteLine("LEC 错误");
                                    dictionary[clientSocket.RemoteEndPoint.ToString()].iii = dictionary[clientSocket.RemoteEndPoint.ToString()].ppp = 0;
                                }
                                //CRC 校验
                                //else if (!CompareArray(Result_Array(tempAll_net, ppp + dataLen + 7, 4), BitConverter.GetBytes(CRC32.GetCRC32(tempAll_net, ppp, dataLen + 7))))
                                //{
                                //    dictionary[clientSocket.RemoteEndPoint.ToString()].Send(Result_Error_CRC());
                                //    Console.WriteLine(byteToHexStr(BitConverter.GetBytes(CRC32.GetCRC32(tempAll_net, ppp, dataLen + 7))));
                                //    Console.WriteLine("CRC 错误");
                                //    iii = ppp = 0;
                                //}
                                else
                                {
                                    //解析字节数组
                                    string machineType = System.Text.Encoding.Default.GetString(dictionary[clientSocket.RemoteEndPoint.ToString()].tempAll_net, dictionary[clientSocket.RemoteEndPoint.ToString()].ppp + 6, 10).Trim('\0');//10bytes机器标识
                                    string serialNumber = System.Text.Encoding.Default.GetString(dictionary[clientSocket.RemoteEndPoint.ToString()].tempAll_net, dictionary[clientSocket.RemoteEndPoint.ToString()].ppp + 16, 12).Trim('\0');//12bytes序列号
                                    string fileName = System.Text.Encoding.Default.GetString(dictionary[clientSocket.RemoteEndPoint.ToString()].tempAll_net, dictionary[clientSocket.RemoteEndPoint.ToString()].ppp + 28, 50).Trim('\0');//50bytes文件名
                                    Console.Write("机器标识：" + machineType + " 序列号：" + serialNumber + " 文件名：" + fileName);
                                    //如果userId为空，说明未登录验证，此时需要登录验证获取userId，验证serialNumber目的是：长时间过后，另外一个用户用了此相同的IP，此时就要重新验证
                                    if (dictionary[clientSocket.RemoteEndPoint.ToString()].userId == "" || dictionary[clientSocket.RemoteEndPoint.ToString()].serialNumber != serialNumber)
                                    {
                                        Console.WriteLine("用户开始登录...");
                                        //登录
                                        string resultLogin = login(machineType, serialNumber);
                                        //判断登录结果,验证成功时
                                        if (resultLogin != null)
                                        {
                                            dictionary[clientSocket.RemoteEndPoint.ToString()].userId = resultLogin;//登录成功后将ID写入实体
                                            dictionary[clientSocket.RemoteEndPoint.ToString()].serialNumber = serialNumber;//登录成功后将serialNumber写入实体
                                        }
                                        //验证不成功时
                                        else
                                        {
                                            dictionary[clientSocket.RemoteEndPoint.ToString()].socket.Send(Result_Error_UsernameOrPassword());//用户名或密码错误
                                        }
                                    }

                                    //回应下位机的请求
                                    sendFiletoXWJ_Get_File_Info_Net(clientSocket.RemoteEndPoint.ToString(), databaseURL + dictionary[clientSocket.RemoteEndPoint.ToString()].userId + "\\" + fileName);
                                    dictionary[clientSocket.RemoteEndPoint.ToString()].ppp += 11 + dataLen;
                                    if (dictionary[clientSocket.RemoteEndPoint.ToString()].iii == dictionary[clientSocket.RemoteEndPoint.ToString()].ppp)
                                    {
                                        dictionary[clientSocket.RemoteEndPoint.ToString()].iii = dictionary[clientSocket.RemoteEndPoint.ToString()].ppp = 0;
                                        //Console.WriteLine("  命令1响应发送成功，此时 ppp==iii==0。");
                                    }
                                }
                            }
                            else
                            {
                                continue;
                            }
                        }
                        #endregion

                        #region 前三位是02 00 a2 下位机下载电脑上的文件 下载文件
                        // 判断前三位是02 00 a2下载命令
                        else if (dictionary[clientSocket.RemoteEndPoint.ToString()].tempAll_net[dictionary[clientSocket.RemoteEndPoint.ToString()].ppp] == (byte)0x02
                                && dictionary[clientSocket.RemoteEndPoint.ToString()].tempAll_net[dictionary[clientSocket.RemoteEndPoint.ToString()].ppp + 1] == (byte)0x00
                                && dictionary[clientSocket.RemoteEndPoint.ToString()].tempAll_net[dictionary[clientSocket.RemoteEndPoint.ToString()].ppp + 2] == (byte)0xa2)
                        {
                            // 前三位是02 00 a2下载命令时，再判断len长度
                            byte[] len = new byte[4];//4字节长度
                            len[0] = dictionary[clientSocket.RemoteEndPoint.ToString()].tempAll_net[dictionary[clientSocket.RemoteEndPoint.ToString()].ppp + 3];
                            len[1] = dictionary[clientSocket.RemoteEndPoint.ToString()].tempAll_net[dictionary[clientSocket.RemoteEndPoint.ToString()].ppp + 4];
                            len[2] = (byte)0x00;
                            len[3] = (byte)0x00;
                            int dataLen = BitConverter.ToInt32(len, 0);//byte数组转int，DATA的长度
                            //Console.WriteLine("DATA数据长度：" + dataLen);
                            //如果缓存中字节长度和发送过来的数据预定长度一致的话，接下来就开始解析里面的文件并保存
                            if (dictionary[clientSocket.RemoteEndPoint.ToString()].iii - dictionary[clientSocket.RemoteEndPoint.ToString()].ppp >= 11 + dataLen)
                            {
                                //LR C校验
                                if (!dictionary[clientSocket.RemoteEndPoint.ToString()].tempAll_net[dictionary[clientSocket.RemoteEndPoint.ToString()].ppp + 5].Equals(LRCCheck.getLRCHash(dictionary[clientSocket.RemoteEndPoint.ToString()].tempAll_net, dictionary[clientSocket.RemoteEndPoint.ToString()].ppp, 5)))
                                {
                                    dictionary[clientSocket.RemoteEndPoint.ToString()].socket.Send(Result_Error_LRC());
                                    dictionary[clientSocket.RemoteEndPoint.ToString()].iii = dictionary[clientSocket.RemoteEndPoint.ToString()].ppp = 0;
                                }
                                //CRC 校验
                                //else if (!CompareArray(Result_Array(tempAll_net, ppp + dataLen + 7, 4), BitConverter.GetBytes(CRC32.GetCRC32(tempAll_net, ppp, dataLen + 7))))
                                //{
                                //    dictionary[clientSocket.RemoteEndPoint.ToString()].Send(Result_Error_CRC());
                                //}
                                else
                                {
                                    //解析字节数组
                                    string machineType = System.Text.Encoding.Default.GetString(dictionary[clientSocket.RemoteEndPoint.ToString()].tempAll_net, dictionary[clientSocket.RemoteEndPoint.ToString()].ppp + 6, 10).Trim('\0');//10bytes机器标识
                                    string serialNumber = System.Text.Encoding.Default.GetString(dictionary[clientSocket.RemoteEndPoint.ToString()].tempAll_net, dictionary[clientSocket.RemoteEndPoint.ToString()].ppp + 16, 12).Trim('\0');//12bytes序列号
                                    string fileName = System.Text.Encoding.Default.GetString(dictionary[clientSocket.RemoteEndPoint.ToString()].tempAll_net, dictionary[clientSocket.RemoteEndPoint.ToString()].ppp + 28, 50).Trim('\0');//50bytes文件名
                                    //获取取文件时的起始位置
                                    len[0] = dictionary[clientSocket.RemoteEndPoint.ToString()].tempAll_net[dictionary[clientSocket.RemoteEndPoint.ToString()].ppp + 78];
                                    len[1] = dictionary[clientSocket.RemoteEndPoint.ToString()].tempAll_net[dictionary[clientSocket.RemoteEndPoint.ToString()].ppp + 79];
                                    len[2] = dictionary[clientSocket.RemoteEndPoint.ToString()].tempAll_net[dictionary[clientSocket.RemoteEndPoint.ToString()].ppp + 80];
                                    len[3] = dictionary[clientSocket.RemoteEndPoint.ToString()].tempAll_net[dictionary[clientSocket.RemoteEndPoint.ToString()].ppp + 81];
                                    int offset = BitConverter.ToInt32(len, 0);//偏移量
                                    //本次要取的文件长度
                                    len[0] = dictionary[clientSocket.RemoteEndPoint.ToString()].tempAll_net[dictionary[clientSocket.RemoteEndPoint.ToString()].ppp + 82];
                                    len[1] = dictionary[clientSocket.RemoteEndPoint.ToString()].tempAll_net[dictionary[clientSocket.RemoteEndPoint.ToString()].ppp + 83];
                                    len[2] = dictionary[clientSocket.RemoteEndPoint.ToString()].tempAll_net[dictionary[clientSocket.RemoteEndPoint.ToString()].ppp + 84];
                                    len[3] = dictionary[clientSocket.RemoteEndPoint.ToString()].tempAll_net[dictionary[clientSocket.RemoteEndPoint.ToString()].ppp + 85];
                                    int fileLength = BitConverter.ToInt32(len, 0);//本次要取的文件长度
                                    Console.WriteLine("机器标识：" + machineType + " 序列号：" + serialNumber + " 文件名：" + fileName + " 偏移量：" + offset + " 数据长度：" + fileLength);
                                    //如果userId为空，说明未登录验证，此时需要登录验证获取userId，验证serialNumber目的是：长时间过后，另外一个用户用了此相同的IP，此时就要重新验证
                                    if (dictionary[clientSocket.RemoteEndPoint.ToString()].userId == "" || dictionary[clientSocket.RemoteEndPoint.ToString()].serialNumber != serialNumber)
                                    {
                                        //登录
                                        string resultLogin = login(machineType, serialNumber);
                                        //判断登录结果,验证成功时
                                        if (resultLogin != null)
                                        {
                                            dictionary[clientSocket.RemoteEndPoint.ToString()].userId = resultLogin;//登录成功后将ID写入实体
                                            dictionary[clientSocket.RemoteEndPoint.ToString()].serialNumber = serialNumber;//登录成功后将serialNumber写入实体
                                        }
                                        //验证不成功时
                                        else
                                        {
                                            dictionary[clientSocket.RemoteEndPoint.ToString()].socket.Send(Result_Error_UsernameOrPassword());//用户名或密码错误
                                        }
                                    }
                                    //回应下位机的请求
                                    sendFiletoXWJ_Get_File_Net(clientSocket.RemoteEndPoint.ToString(), databaseURL + dictionary[clientSocket.RemoteEndPoint.ToString()].userId + "\\" + fileName, offset, fileLength);
                                    dictionary[clientSocket.RemoteEndPoint.ToString()].ppp += 11 + dataLen;
                                    if (dictionary[clientSocket.RemoteEndPoint.ToString()].iii == dictionary[clientSocket.RemoteEndPoint.ToString()].ppp)
                                    {
                                        dictionary[clientSocket.RemoteEndPoint.ToString()].iii = dictionary[clientSocket.RemoteEndPoint.ToString()].ppp = 0;
                                        //Console.WriteLine("  命令2的响应发送成功，此时 ppp==iii==0");
                                        //Console.WriteLine();
                                    }
                                }
                            }
                            else
                            {
                                continue;
                            }
                        }
                        #endregion

                        #region 前三位是02 00 b1  下位机上传文件信息 3
                        // 判断前三位是02 00 b1下载命令
                        else if (dictionary[clientSocket.RemoteEndPoint.ToString()].tempAll_net[dictionary[clientSocket.RemoteEndPoint.ToString()].ppp] == (byte)0x02
                                && dictionary[clientSocket.RemoteEndPoint.ToString()].tempAll_net[dictionary[clientSocket.RemoteEndPoint.ToString()].ppp + 1] == (byte)0x00
                                && dictionary[clientSocket.RemoteEndPoint.ToString()].tempAll_net[dictionary[clientSocket.RemoteEndPoint.ToString()].ppp + 2] == (byte)0xb1)
                        {
                            // 前三位是02 00 b1下载命令时，再判断len长度
                            byte[] len = new byte[4];
                            len[0] = dictionary[clientSocket.RemoteEndPoint.ToString()].tempAll_net[dictionary[clientSocket.RemoteEndPoint.ToString()].ppp + 3];
                            len[1] = dictionary[clientSocket.RemoteEndPoint.ToString()].tempAll_net[dictionary[clientSocket.RemoteEndPoint.ToString()].ppp + 4];
                            len[2] = (byte)0x00;
                            len[3] = (byte)0x00;
                            int dataLen = BitConverter.ToInt32(len, 0);//byte数组转int，DATA的长度
                            //Console.WriteLine("DATA数据长度：" + dataLen);
                            //如果缓存中字节长度和发送过来的数据预定长度一致的话，接下来就开始解析里面的文件并保存
                            if (dictionary[clientSocket.RemoteEndPoint.ToString()].iii - dictionary[clientSocket.RemoteEndPoint.ToString()].ppp >= 11 + dataLen)
                            {
                                //LR C校验
                                if (!dictionary[clientSocket.RemoteEndPoint.ToString()].tempAll_net[dictionary[clientSocket.RemoteEndPoint.ToString()].ppp + 5].Equals(LRCCheck.getLRCHash(dictionary[clientSocket.RemoteEndPoint.ToString()].tempAll_net, dictionary[clientSocket.RemoteEndPoint.ToString()].ppp, 5)))
                                {
                                    dictionary[clientSocket.RemoteEndPoint.ToString()].socket.Send(Result_Error_LRC());
                                }
                                //CRC 校验
                                //else if (!CompareArray(Result_Array(tempAll_net, ppp + dataLen + 7, 4), BitConverter.GetBytes(CRC32.GetCRC32(tempAll_net, ppp, dataLen + 7))))
                                //{
                                //    //byte[] a = BitConverter.GetBytes(CRC32.GetCRC32(tempAll_net, ppp, dataLen + 7));
                                //    //foreach (var item in a)
                                //    //{
                                //    //    Console.WriteLine(item);
                                //    //}
                                //    dictionary[clientSocket.RemoteEndPoint.ToString()].Send(Result_Error_CRC());
                                //}
                                else
                                {
                                    //解析字节数组
                                    string machineType = System.Text.Encoding.Default.GetString(dictionary[clientSocket.RemoteEndPoint.ToString()].tempAll_net, dictionary[clientSocket.RemoteEndPoint.ToString()].ppp + 6, 10).Trim('\0');//10bytes机器标识
                                    string serialNumber = System.Text.Encoding.Default.GetString(dictionary[clientSocket.RemoteEndPoint.ToString()].tempAll_net, dictionary[clientSocket.RemoteEndPoint.ToString()].ppp + 16, 12).Trim('\0');//12bytes序列号
                                    string fileName = System.Text.Encoding.Default.GetString(dictionary[clientSocket.RemoteEndPoint.ToString()].tempAll_net, dictionary[clientSocket.RemoteEndPoint.ToString()].ppp + 28, 50).Trim('\0');//50bytes文件名
                                    len[0] = dictionary[clientSocket.RemoteEndPoint.ToString()].tempAll_net[dictionary[clientSocket.RemoteEndPoint.ToString()].ppp + 78];
                                    len[1] = dictionary[clientSocket.RemoteEndPoint.ToString()].tempAll_net[dictionary[clientSocket.RemoteEndPoint.ToString()].ppp + 79];
                                    len[2] = dictionary[clientSocket.RemoteEndPoint.ToString()].tempAll_net[dictionary[clientSocket.RemoteEndPoint.ToString()].ppp + 80];
                                    len[3] = dictionary[clientSocket.RemoteEndPoint.ToString()].tempAll_net[dictionary[clientSocket.RemoteEndPoint.ToString()].ppp + 81];
                                    int fileLength = BitConverter.ToInt32(len, 0);//该文件总长度
                                    string fileMD5Hash = System.Text.Encoding.Default.GetString(dictionary[clientSocket.RemoteEndPoint.ToString()].tempAll_net, 82, 32).Trim('\0');//32文件校验码
                                    Console.WriteLine("机器标识：" + machineType + " 序列号：" + serialNumber + " 文件名：" + fileName + " 该文件总长度：" + fileLength + " 文件校验码：" + fileMD5Hash);
                                    //如果userId为空，说明未登录验证，此时需要登录验证获取userId，验证serialNumber目的是：长时间过后，另外一个用户用了此相同的IP，此时就要重新验证
                                    if (dictionary[clientSocket.RemoteEndPoint.ToString()].userId == "" || dictionary[clientSocket.RemoteEndPoint.ToString()].serialNumber != serialNumber)
                                    {
                                        //登录
                                        string resultLogin = login(machineType, serialNumber);
                                        //判断登录结果,验证成功时
                                        if (resultLogin != null)
                                        {
                                            dictionary[clientSocket.RemoteEndPoint.ToString()].userId = resultLogin;//登录成功后将ID写入实体
                                            dictionary[clientSocket.RemoteEndPoint.ToString()].serialNumber = serialNumber;//登录成功后将serialNumber写入实体
                                        }
                                        //验证不成功时
                                        else
                                        {
                                            dictionary[clientSocket.RemoteEndPoint.ToString()].socket.Send(Result_Error_UsernameOrPassword());//用户名或密码错误
                                        }
                                    }
                                    //回应下位机的请求
                                    sendFiletoXWJ_Send_File_Info_Net(clientSocket.RemoteEndPoint.ToString(), fileLength, fileMD5Hash);
                                    dictionary[clientSocket.RemoteEndPoint.ToString()].ppp += 11 + dataLen;
                                    if (dictionary[clientSocket.RemoteEndPoint.ToString()].iii == dictionary[clientSocket.RemoteEndPoint.ToString()].ppp)
                                    {
                                        dictionary[clientSocket.RemoteEndPoint.ToString()].iii = dictionary[clientSocket.RemoteEndPoint.ToString()].ppp = 0;
                                        //Console.WriteLine("命令3的响应发送成功，此时 ppp==iii==0");
                                    }
                                }
                            }
                            else
                            {
                                continue;
                            }
                        }
                        #endregion

                        #region 前三位是02 00 b2 下位机上传文件 4
                        // 判断前三位是02 00 b2下载命令
                        else if (dictionary[clientSocket.RemoteEndPoint.ToString()].tempAll_net[dictionary[clientSocket.RemoteEndPoint.ToString()].ppp] == (byte)0x02
                                && dictionary[clientSocket.RemoteEndPoint.ToString()].tempAll_net[dictionary[clientSocket.RemoteEndPoint.ToString()].ppp + 1] == (byte)0x00
                                && dictionary[clientSocket.RemoteEndPoint.ToString()].tempAll_net[dictionary[clientSocket.RemoteEndPoint.ToString()].ppp + 2] == (byte)0xb2)
                        {
                            // 前三位是02 00 b2下载命令时，再判断len长度
                            byte[] len = new byte[4];
                            len[0] = dictionary[clientSocket.RemoteEndPoint.ToString()].tempAll_net[dictionary[clientSocket.RemoteEndPoint.ToString()].ppp + 3];
                            len[1] = dictionary[clientSocket.RemoteEndPoint.ToString()].tempAll_net[dictionary[clientSocket.RemoteEndPoint.ToString()].ppp + 4];
                            len[2] = (byte)0x00;
                            len[3] = (byte)0x00;
                            int dataLen = BitConverter.ToInt32(len, 0);//byte数组转int，DATA的长度
                            //Console.WriteLine("DATA数据长度：" + dataLen);
                            //如果缓存中字节长度和发送过来的数据预定长度一致的话，接下来就开始解析里面的文件并保存
                            if (dictionary[clientSocket.RemoteEndPoint.ToString()].iii - dictionary[clientSocket.RemoteEndPoint.ToString()].ppp >= 11 + dataLen)
                            {
                                //LRC 校验
                                if (!dictionary[clientSocket.RemoteEndPoint.ToString()].tempAll_net[dictionary[clientSocket.RemoteEndPoint.ToString()].ppp + 5].Equals(LRCCheck.getLRCHash(dictionary[clientSocket.RemoteEndPoint.ToString()].tempAll_net, dictionary[clientSocket.RemoteEndPoint.ToString()].ppp, 5)))
                                {
                                    dictionary[clientSocket.RemoteEndPoint.ToString()].socket.Send(Result_Error_LRC());
                                }
                                //CRC 校验
                                //else if (!CompareArray(Result_Array(tempAll_net, ppp + dataLen + 7, 4), BitConverter.GetBytes(CRC32.GetCRC32(tempAll_net, ppp, dataLen + 7))))
                                //{
                                //    Console.WriteLine(BitConverter.GetBytes(CRC32.GetCRC32(tempAll_net, ppp, dataLen + 7))); 
                                //    dictionary[clientSocket.RemoteEndPoint.ToString()].Send(Result_Error_CRC());
                                //}
                                else
                                {
                                    //解析字节数组
                                    string machineType = System.Text.Encoding.Default.GetString(dictionary[clientSocket.RemoteEndPoint.ToString()].tempAll_net, dictionary[clientSocket.RemoteEndPoint.ToString()].ppp + 6, 10).Trim('\0');//10bytes机器标识
                                    string serialNumber = System.Text.Encoding.Default.GetString(dictionary[clientSocket.RemoteEndPoint.ToString()].tempAll_net, dictionary[clientSocket.RemoteEndPoint.ToString()].ppp + 16, 12).Trim('\0');//12bytes序列号
                                    string fileName = System.Text.Encoding.Default.GetString(dictionary[clientSocket.RemoteEndPoint.ToString()].tempAll_net, dictionary[clientSocket.RemoteEndPoint.ToString()].ppp + 28, 50).Trim('\0');//50bytes文件名
                                    //获取取文件时的起始位置
                                    len[0] = dictionary[clientSocket.RemoteEndPoint.ToString()].tempAll_net[dictionary[clientSocket.RemoteEndPoint.ToString()].ppp + 78];
                                    len[1] = dictionary[clientSocket.RemoteEndPoint.ToString()].tempAll_net[dictionary[clientSocket.RemoteEndPoint.ToString()].ppp + 79];
                                    len[2] = dictionary[clientSocket.RemoteEndPoint.ToString()].tempAll_net[dictionary[clientSocket.RemoteEndPoint.ToString()].ppp + 80];
                                    len[3] = dictionary[clientSocket.RemoteEndPoint.ToString()].tempAll_net[dictionary[clientSocket.RemoteEndPoint.ToString()].ppp + 81];
                                    int offset = BitConverter.ToInt32(len, 0);//偏移量
                                    len[0] = dictionary[clientSocket.RemoteEndPoint.ToString()].tempAll_net[dictionary[clientSocket.RemoteEndPoint.ToString()].ppp + 82];
                                    len[1] = dictionary[clientSocket.RemoteEndPoint.ToString()].tempAll_net[dictionary[clientSocket.RemoteEndPoint.ToString()].ppp + 83];
                                    len[2] = dictionary[clientSocket.RemoteEndPoint.ToString()].tempAll_net[dictionary[clientSocket.RemoteEndPoint.ToString()].ppp + 84];
                                    len[3] = dictionary[clientSocket.RemoteEndPoint.ToString()].tempAll_net[dictionary[clientSocket.RemoteEndPoint.ToString()].ppp + 85];
                                    int fileLength = BitConverter.ToInt32(len, 0);//本次待接收的文件长度
                                    Console.WriteLine("机器标识：" + machineType + " 序列号：" + serialNumber + " 文件名：" + fileName + " 偏移量：" + offset + " 数据长度：" + fileLength);
                                    //如果userId为空，说明未登录验证，此时需要登录验证获取userId，验证serialNumber目的是：长时间过后，另外一个用户用了此相同的IP，此时就要重新验证
                                    if (dictionary[clientSocket.RemoteEndPoint.ToString()].userId == "" || dictionary[clientSocket.RemoteEndPoint.ToString()].serialNumber != serialNumber)
                                    {
                                        //登录
                                        string resultLogin = login(machineType, serialNumber);
                                        //判断登录结果,验证成功时
                                        if (resultLogin != null)
                                        {
                                            dictionary[clientSocket.RemoteEndPoint.ToString()].userId = resultLogin;//登录成功后将ID写入实体
                                            dictionary[clientSocket.RemoteEndPoint.ToString()].serialNumber = serialNumber;//登录成功后将serialNumber写入实体
                                        }
                                        //验证不成功时
                                        else
                                        {
                                            dictionary[clientSocket.RemoteEndPoint.ToString()].socket.Send(Result_Error_UsernameOrPassword());//用户名或密码错误
                                        }
                                    }
                                    //回应下位机的请求
                                    sendFiletoXWJ_Send_File_Net(clientSocket.RemoteEndPoint.ToString(), System.IO.Path.GetFileName(fileName), offset, fileLength);
                                    dictionary[clientSocket.RemoteEndPoint.ToString()].ppp += 11 + dataLen;
                                    if (dictionary[clientSocket.RemoteEndPoint.ToString()].iii == dictionary[clientSocket.RemoteEndPoint.ToString()].ppp)
                                    {
                                        dictionary[clientSocket.RemoteEndPoint.ToString()].iii = dictionary[clientSocket.RemoteEndPoint.ToString()].ppp = 0;
                                        //Console.WriteLine("  命令4的响应发送成功，此时 ppp==iii==0");
                                        //Console.WriteLine("pppp:" + pppp);
                                    }
                                }
                            }
                            else
                            {
                                continue;
                            }
                        }
                        #endregion

                        else
                        {
                            dictionary[clientSocket.RemoteEndPoint.ToString()].ppp++;//为了容错，ppp依次往后移动
                        }
                        //tempAll数组处理完后指针就归零
                        dictionary[clientSocket.RemoteEndPoint.ToString()].iii = dictionary[clientSocket.RemoteEndPoint.ToString()].ppp = 0;
                    }

                    ////定义byte数组存放从客户端接收过来的数据
                    //byte[] buffer = new byte[1024];
                    ////将接收过来的数据放到buffer中，并返回实际接受数据的长度
                    //int n = clientSocket.Receive(buffer);
                    //if (n == 0) break;
                    ////将字节转换成字符串
                    //string words = Encoding.Default.GetString(buffer, 0, n);
                    //Console.WriteLine(clientSocket.RemoteEndPoint.ToString() + ":" + words);
                    ////发送数据
                    //dictionary[clientSocket.RemoteEndPoint.ToString()].Send(Encoding.Default.GetBytes("12345qwert"));
                }
                catch (Exception ex)
                {
                    //log.Info("最内层的线程出错：" + ex.Message);
                    clientSocket.Close();
                    break;

                    //Thread.CurrentThread.Abort();
                    //if (clientSocket.Poll(-1, SelectMode.SelectRead))
                    //{
                    //    Console.WriteLine("客户端已断开连接！");
                    //    dictionary.Remove(clientSocket.RemoteEndPoint.ToString());
                    //    Console.WriteLine("dictionary中移除了：" + clientSocket.RemoteEndPoint.ToString());
                    //    Console.WriteLine("dictionary中还剩余：");
                    //    foreach (var item in dictionary)
                    //    {
                    //        Console.WriteLine(item.Key);
                    //    }
                    //    break;
                    //}
                    //MessageBox.Show(ex.Message, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    //Console.WriteLine("断开与客户端的连接！" + ex.Message);

                    //每次传输完成后及时将dictionary的该条记录删除，便于释放内存空间
                    //if (clientSocket.Poll(10, SelectMode.SelectRead))
                    //{
                    //    Console.Write("客户端已断开连接！");
                    //    dictionary.Remove(clientSocket.RemoteEndPoint.ToString());
                    //    Console.Write("dictionary中移除了：" + clientSocket.RemoteEndPoint.ToString());
                    //    Console.WriteLine("，dictionary中还剩余：");
                    //    log.Info("dictionary中还剩余：");
                    //    foreach (var item in dictionary)
                    //    {
                    //        Console.WriteLine(item.Key);
                    //        log.Info(item.Key);
                    //    }
                    //}
                    //break;
                }

                //if (clientSocket.Poll(5000, SelectMode.SelectRead))
                //{
                //    Console.Write("客户端已断开连接！");
                //    //dictionary.Remove(clientSocket.RemoteEndPoint.ToString());
                //    //Console.Write("dictionary中移除了：" + clientSocket.RemoteEndPoint.ToString());
                //    Console.WriteLine("，dictionary中还剩余：");
                //    //log.Info("dictionary中还剩余：");
                //    foreach (var item in dictionary)
                //    {
                //        Console.WriteLine(item.Key);
                //        //log.Info(item.Key);
                //    }
                //    break;
                //}

            }

            Console.WriteLine("关闭最内层的线程，即关闭了dictionary中的一个元素！");
            //log.Info("关闭最内层的线程，即关闭了dictionary中的一个元素！");
        }
        /// <summary>
        /// part1_1 以太网 下位机下载文件信息 0xa1 Get_File_Info 1
        /// </summary>
        public void sendFiletoXWJ_Get_File_Info_Net(string clientSocketId, string filePath)
        {
            //将文件转换成字节数组
            //byte[] data = OperateFile.FiletoByte(filePath);
            //DATA的长度
            int dataLength = 10 + 12 + 50 + 10 + 4 + 4 + 32;//122
            //总发送数据的长度 STX CLA INS LENL LENH LRC DATA ETX CRC
            int dataSendLength = 11 + dataLength;
            //总发送串
            byte[] dataSend = new byte[dataSendLength];
            //接下来拼接发送串
            dataSend[0] = 0x02;//起始码 STX
            dataSend[1] = 0x00;//通讯包标志 CLA
            dataSend[2] = 0xa1;//命令码 INS

            //将DATA的长度转换为byte[]数组，2位，低位在前 高位在后
            byte[] dataLengthBytes = BitConverter.GetBytes(dataLength);//90
            dataSend[3] = dataLengthBytes[0];//长度的低位 LENL
            dataSend[4] = dataLengthBytes[1];//长度的高位 LENH

            //计算LRC校验码
            byte xorResult = dataSend[0];
            // 求xor校验和，注意：XOR运算从第二元素开始
            for (int i = 1; i < 5; i++)
            {
                xorResult ^= dataSend[i];
            }
            dataSend[5] = xorResult;//LRC校验码

            //将10bytes机器标识、12bytes序列号、50bytes文件名写入发送串  DATA部分
            for (int i = 6; i < 6 + 72; i++)
            {
                dataSend[i] = dictionary[clientSocketId].tempAll_net[dictionary[clientSocketId].ppp + i];
            }
            //获取文件版本号并写入发送串,10字节
            byte[] fileVersionNumber = System.Text.Encoding.Default.GetBytes(OperateFile.GetFileVersionNumber(filePath));//文件版本号转换成字节数组
            for (int i = 78; i < 78 + fileVersionNumber.Length; i++)
            {
                dataSend[i] = fileVersionNumber[i - 78];
            }
            //获取文件最后一次修改日期并写入发送串,4字节
            //byte[] fileLastWriteTime = System.Text.Encoding.Default.GetBytes(OperateFile.GetFileLastWriteTime(filePath).ToString());//文件版本号转换成字节数组
            byte[] fileLastWriteTime = BitConverter.GetBytes(OperateFile.GetFileLastWriteTime(filePath));//文件版本号转换成字节数组
            for (int i = 88; i < 88 + fileLastWriteTime.Length; i++)
            {
                dataSend[i] = fileLastWriteTime[i - 88];
            }

            //获取文件字节大小并写入发送串,4字节
            byte[] fileLength = BitConverter.GetBytes(Convert.ToInt32(OperateFile.GetFileLength(filePath)));//文件版本号转换成字节数组
            for (int i = 92; i < 92 + fileLength.Length; i++)
            {
                dataSend[i] = fileLength[i - 92];
            }
            //获取文件校验码并写入发送串,32字节,CRC32
            byte[] fileMD5Hash = Encoding.Default.GetBytes(MD5Check.getMD5Hash(filePath));
            for (int i = 96; i < 96 + fileMD5Hash.Length; i++)
            {
                dataSend[i] = fileMD5Hash[i - 96];
            }
            //结束码
            dataSend[dataSendLength - 5] = 0x03;
            //计算CRC32校验码,4字节,的校验码写入发送串
            byte[] dataSendCRCBytes = BitConverter.GetBytes(CRC32.GetCRC32(dataSend, dataSendLength - 4));
            dataSend[dataSendLength - 4] = dataSendCRCBytes[0];
            dataSend[dataSendLength - 3] = dataSendCRCBytes[1];
            dataSend[dataSendLength - 2] = dataSendCRCBytes[2];
            dataSend[dataSendLength - 1] = dataSendCRCBytes[3];
            //发送
            dictionary[clientSocketId].socket.Send(dataSend);
            //Console.WriteLine(byteToHexStr(dataSend));//输出
            //Console.Write("  响应命令1的串的长度：" + dataSend.Length);
        }
        /// <summary>
        /// part1_1 以太网 下位机下载文件 0xa2 Get_File 2
        /// </summary>
        public void sendFiletoXWJ_Get_File_Net(string clientSocketId, string filePath, int offset, int fileLength)
        {
            //将文件转换成字节数组
            byte[] data = OperateFile.FiletoByte(filePath);
            //DATA的长度
            int dataLength = 10 + 12 + 50 + 4 + fileLength;//72 + N
            //总发送数据的长度 STX CLA INS LENL LENH LRC DATA ETX CRC
            int dataSendLength = 11 + dataLength;
            //总发送串
            byte[] dataSend = new byte[dataSendLength];
            //接下来拼接发送串
            dataSend[0] = 0x02;//起始码 STX
            dataSend[1] = 0x00;//通讯包标志 CLA
            dataSend[2] = 0xa2;//命令码 INS

            //将DATA的长度转换为byte[]数组，2位，低位在前 高位在后
            byte[] dataLengthBytes = BitConverter.GetBytes(dataLength);
            dataSend[3] = dataLengthBytes[0];//长度的低位 LENL
            dataSend[4] = dataLengthBytes[1];//长度的高位 LENH

            //计算LRC校验码
            byte xorResult = dataSend[0];
            // 求xor校验和，注意：XOR运算从第二元素开始
            for (int i = 1; i < 5; i++)
            {
                xorResult ^= dataSend[i];
            }
            dataSend[5] = xorResult;//LRC校验码

            //将10bytes机器标识、12bytes序列号、50bytes文件名写入发送串  DATA部分
            for (int i = 6; i < 6 + 72; i++)
            {
                dataSend[i] = dictionary[clientSocketId].tempAll_net[dictionary[clientSocketId].ppp + i];
            }
            //文件长度
            byte[] fileLengthBytes = BitConverter.GetBytes(fileLength);
            dataSend[78] = fileLengthBytes[0];
            dataSend[79] = fileLengthBytes[1];
            dataSend[80] = fileLengthBytes[2];
            dataSend[81] = fileLengthBytes[3];
            //将文件以字节的形式写入发送串 N字节
            for (int i = 82; i < 82 + fileLength; i++)
            {
                dataSend[i] = data[i - 82 + offset];
            }
            //结束码
            dataSend[dataSendLength - 5] = 0x03;
            //计算CRC32校验码,4字节,的校验码写入发送串
            byte[] dataSendCRCBytes = BitConverter.GetBytes(CRC32.GetCRC32(dataSend, dataSendLength - 4));
            dataSend[dataSendLength - 4] = dataSendCRCBytes[0];
            dataSend[dataSendLength - 3] = dataSendCRCBytes[1];
            dataSend[dataSendLength - 2] = dataSendCRCBytes[2];
            dataSend[dataSendLength - 1] = dataSendCRCBytes[3];
            //发送
            dictionary[clientSocketId].socket.Send(dataSend);
            //Console.WriteLine(byteToHexStr(dataSend));//输出
            //Console.Write("响应命令2串的长度：" + dataSend.Length);
        }
        /// <summary>
        /// part1_1 以太网 下位机上传文件信息 0xb1 Send_File_Info 3
        /// </summary>
        public void sendFiletoXWJ_Send_File_Info_Net(string clientSocketId, int fileLength, string fileMD5Hash)
        {
            //下位机上传的文件的总长度
            dictionary[clientSocketId].receivedFileLength = fileLength;
            //开辟一个缓存区来存放这个文件
            dictionary[clientSocketId].receivedFile = new byte[fileLength];
            //上传文件的MD5校验码
            dictionary[clientSocketId].receivedFileMD5Hash = fileMD5Hash;
            //DATA的长度
            int dataLength = 1;
            //总发送数据的长度 STX CLA INS LENL LENH LRC DATA ETX CRC
            int dataSendLength = 11 + dataLength;
            //总发送串
            byte[] dataSend = new byte[dataSendLength];
            //接下来拼接发送串
            dataSend[0] = 0x02;//起始码 STX
            dataSend[1] = 0x00;//通讯包标志 CLA
            dataSend[2] = 0xb1;//命令码 INS

            //将DATA的长度转换为byte[]数组，2位，低位在前 高位在后
            byte[] dataLengthBytes = BitConverter.GetBytes(dataLength);//98
            dataSend[3] = dataLengthBytes[0];//长度的低位 LENL
            dataSend[4] = dataLengthBytes[1];//长度的高位 LENH

            //计算LRC校验码
            byte xorResult = dataSend[0];
            // 求xor校验和，注意：XOR运算从第二元素开始
            for (int i = 1; i < 5; i++)
            {
                xorResult ^= dataSend[i];
            }
            dataSend[5] = xorResult;//LRC校验码
            //成功标志
            dataSend[6] = 0x00;
            //结束码
            dataSend[dataSendLength - 5] = 0x03;
            //计算CRC32校验码,4字节,的校验码写入发送串
            byte[] dataSendCRCBytes = BitConverter.GetBytes(CRC32.GetCRC32(dataSend, dataSendLength - 4));
            dataSend[dataSendLength - 4] = dataSendCRCBytes[0];
            dataSend[dataSendLength - 3] = dataSendCRCBytes[1];
            dataSend[dataSendLength - 2] = dataSendCRCBytes[2];
            dataSend[dataSendLength - 1] = dataSendCRCBytes[3];
            //发送
            dictionary[clientSocketId].socket.Send(dataSend);
            //Console.WriteLine(byteToHexStr(dataSend));//输出
            //Console.Write("  响应命令3串的长度：" + dataSend.Length);
        }
        /// <summary>
        /// part1_1 以太网 下位机上传文件 0xb1 Send_File 4
        /// </summary>
        public void sendFiletoXWJ_Send_File_Net(string clientSocketId, string fileName, int offset, int fileLength)
        {
            for (int i = 0; i < fileLength; i++, dictionary[clientSocketId].pppp++)
            {
                dictionary[clientSocketId].receivedFile[dictionary[clientSocketId].pppp] = dictionary[clientSocketId].tempAll_net[dictionary[clientSocketId].ppp + 86 + i];
            }
            //Console.WriteLine(CheckUtils.MD5Check.getMD5Hash(receivedFile));
            if (dictionary[clientSocketId].pppp == dictionary[clientSocketId].receivedFileLength && dictionary[clientSocketId].receivedFileMD5Hash.Equals(CheckUtils.MD5Check.getMD5Hash(dictionary[clientSocketId].receivedFile)))
            {
                //先移动旧版本的文件到备份文件夹中
                OperateFile.MoveFile(databaseURL + dictionary[clientSocketId].userId + "\\" + fileName, databaseURL + dictionary[clientSocketId].userId + "\\database.old\\" + fileName);
                //保存文件
                OperateFile.BytetoFile(dictionary[clientSocketId].receivedFile, 0, dictionary[clientSocketId].receivedFileLength, databaseURL + dictionary[clientSocketId].userId + "\\" + fileName);
                //重新置零
                dictionary[clientSocketId].pppp = dictionary[clientSocketId].receivedFileLength = 0;
                dictionary[clientSocketId].receivedFileMD5Hash = null;
                dictionary[clientSocketId].receivedFile = null;
                dictionary[clientSocketId].socket.Send(Result_Success());
                Console.WriteLine("The file is saved successfully!");
                //MessageBox.Show("The file is saved successfully!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            //发送
            dictionary[clientSocketId].socket.Send(Result_Success());
        }
        /// <summary>
        /// 以太网 返回成功的标志
        /// </summary>
        public byte[] Result_Success()
        {
            int dataLength = 1;//
            //总发送数据的长度 STX CLA INS LENL LENH LRC DATA ETX CRC
            int dataSendLength = 11 + dataLength;
            //总发送串
            byte[] dataSend = new byte[dataSendLength];
            //接下来拼接发送串
            dataSend[0] = 0x02;//起始码 STX
            dataSend[1] = 0x00;//通讯包标志 CLA
            dataSend[2] = 0xb2;//命令码 INS

            //将DATA的长度转换为byte[]数组，2位，低位在前 高位在后
            byte[] dataLengthBytes = BitConverter.GetBytes(dataLength);
            dataSend[3] = dataLengthBytes[0];//长度的低位 LENL
            dataSend[4] = dataLengthBytes[1];//长度的高位 LENH

            //计算LRC校验码
            byte xorResult = dataSend[0];
            // 求xor校验和，注意：XOR运算从第二元素开始
            for (int i = 1; i < 5; i++)
            {
                xorResult ^= dataSend[i];
            }
            dataSend[5] = xorResult;//LRC校验码
            //成功标志
            dataSend[6] = 0x00;

            //结束码
            dataSend[dataSendLength - 5] = 0x03;
            //计算CRC32校验码,4字节,的校验码写入发送串
            byte[] dataSendCRCBytes = BitConverter.GetBytes(CRC32.GetCRC32(dataSend, dataSendLength - 4));
            dataSend[dataSendLength - 4] = dataSendCRCBytes[0];
            dataSend[dataSendLength - 3] = dataSendCRCBytes[1];
            dataSend[dataSendLength - 2] = dataSendCRCBytes[2];
            dataSend[dataSendLength - 1] = dataSendCRCBytes[3];
            //Console.WriteLine(byteToHexStr(dataSend));//输出
            return dataSend;
        }
        /// <summary>
        /// 以太网 返回LRC error
        /// </summary>
        public byte[] Result_Error_LRC()
        {
            int dataLength = 1;
            //总发送数据的长度 STX CLA INS LENL LENH LRC DATA ETX CRC
            int dataSendLength = 11 + dataLength;
            //总发送串
            byte[] dataSend = new byte[dataSendLength];
            //接下来拼接发送串
            dataSend[0] = 0x02;//起始码 STX
            dataSend[1] = 0x00;//通讯包标志 CLA
            dataSend[2] = 0xb2;//命令码 INS

            //将DATA的长度转换为byte[]数组，2位，低位在前 高位在后
            byte[] dataLengthBytes = BitConverter.GetBytes(dataLength);
            dataSend[3] = dataLengthBytes[0];//长度的低位 LENL
            dataSend[4] = dataLengthBytes[1];//长度的高位 LENH

            //计算LRC校验码
            byte xorResult = dataSend[0];
            // 求xor校验和，注意：XOR运算从第二元素开始
            for (int i = 1; i < 5; i++)
            {
                xorResult ^= dataSend[i];
            }
            dataSend[5] = xorResult;//LRC校验码
            //LRC 错误
            dataSend[6] = 0x03;

            //结束码
            dataSend[dataSendLength - 5] = 0x03;
            //计算CRC32校验码,4字节,的校验码写入发送串
            byte[] dataSendCRCBytes = BitConverter.GetBytes(CRC32.GetCRC32(dataSend, dataSendLength - 4));
            dataSend[dataSendLength - 4] = dataSendCRCBytes[0];
            dataSend[dataSendLength - 3] = dataSendCRCBytes[1];
            dataSend[dataSendLength - 2] = dataSendCRCBytes[2];
            dataSend[dataSendLength - 1] = dataSendCRCBytes[3];
            //Console.WriteLine(byteToHexStr(dataSend));//输出
            return dataSend;
        }
        /// <summary>
        /// 以太网 返回CRC error
        /// </summary>
        public byte[] Result_Error_CRC()
        {
            int dataLength = 1;
            //总发送数据的长度 STX CLA INS LENL LENH LRC DATA ETX CRC
            int dataSendLength = 11 + dataLength;
            //总发送串
            byte[] dataSend = new byte[dataSendLength];
            //接下来拼接发送串
            dataSend[0] = 0x02;//起始码 STX
            dataSend[1] = 0x00;//通讯包标志 CLA
            dataSend[2] = 0xb2;//命令码 INS

            //将DATA的长度转换为byte[]数组，2位，低位在前 高位在后
            byte[] dataLengthBytes = BitConverter.GetBytes(dataLength);
            dataSend[3] = dataLengthBytes[0];//长度的低位 LENL
            dataSend[4] = dataLengthBytes[1];//长度的高位 LENH

            //计算LRC校验码
            byte xorResult = dataSend[0];
            // 求xor校验和，注意：XOR运算从第二元素开始
            for (int i = 1; i < 5; i++)
            {
                xorResult ^= dataSend[i];
            }
            dataSend[5] = xorResult;//LRC校验码
            //CRC 错误
            dataSend[6] = 0x07;
            //结束码
            dataSend[dataSendLength - 5] = 0x03;
            //计算CRC32校验码,4字节,的校验码写入发送串
            byte[] dataSendCRCBytes = BitConverter.GetBytes(CRC32.GetCRC32(dataSend, dataSendLength - 4));
            dataSend[dataSendLength - 4] = dataSendCRCBytes[0];
            dataSend[dataSendLength - 3] = dataSendCRCBytes[1];
            dataSend[dataSendLength - 2] = dataSendCRCBytes[2];
            dataSend[dataSendLength - 1] = dataSendCRCBytes[3];
            //Console.WriteLine(byteToHexStr(dataSend));//输出
            return dataSend;
        }
        /// <summary>
        /// 登录超时，请重新登录 error
        /// </summary>
        public byte[] Result_Error_TimeOver()
        {
            int dataLength = 1;
            //总发送数据的长度 STX CLA INS LENL LENH LRC DATA ETX CRC
            int dataSendLength = 11 + dataLength;
            //总发送串
            byte[] dataSend = new byte[dataSendLength];
            //接下来拼接发送串
            dataSend[0] = 0x02;//起始码 STX
            dataSend[1] = 0x00;//通讯包标志 CLA
            dataSend[2] = 0xb2;//命令码 INS

            //将DATA的长度转换为byte[]数组，2位，低位在前 高位在后
            byte[] dataLengthBytes = BitConverter.GetBytes(dataLength);
            dataSend[3] = dataLengthBytes[0];//长度的低位 LENL
            dataSend[4] = dataLengthBytes[1];//长度的高位 LENH

            //计算LRC校验码
            byte xorResult = dataSend[0];
            // 求xor校验和，注意：XOR运算从第二元素开始
            for (int i = 1; i < 5; i++)
            {
                xorResult ^= dataSend[i];
            }
            dataSend[5] = xorResult;//LRC校验码
            //CRC 错误
            dataSend[6] = 0x08;
            //结束码
            dataSend[dataSendLength - 5] = 0x03;
            //计算CRC32校验码,4字节,的校验码写入发送串
            byte[] dataSendCRCBytes = BitConverter.GetBytes(CRC32.GetCRC32(dataSend, dataSendLength - 4));
            dataSend[dataSendLength - 4] = dataSendCRCBytes[0];
            dataSend[dataSendLength - 3] = dataSendCRCBytes[1];
            dataSend[dataSendLength - 2] = dataSendCRCBytes[2];
            dataSend[dataSendLength - 1] = dataSendCRCBytes[3];
            //Console.WriteLine(byteToHexStr(dataSend));//输出
            return dataSend;
        }
        /// <summary>
        /// 登录时,账号或密码 error
        /// </summary>
        public byte[] Result_Error_UsernameOrPassword()
        {
            int dataLength = 1;
            //总发送数据的长度 STX CLA INS LENL LENH LRC DATA ETX CRC
            int dataSendLength = 11 + dataLength;
            //总发送串
            byte[] dataSend = new byte[dataSendLength];
            //接下来拼接发送串
            dataSend[0] = 0x02;//起始码 STX
            dataSend[1] = 0x00;//通讯包标志 CLA
            dataSend[2] = 0xb2;//命令码 INS

            //将DATA的长度转换为byte[]数组，2位，低位在前 高位在后
            byte[] dataLengthBytes = BitConverter.GetBytes(dataLength);
            dataSend[3] = dataLengthBytes[0];//长度的低位 LENL
            dataSend[4] = dataLengthBytes[1];//长度的高位 LENH

            //计算LRC校验码
            byte xorResult = dataSend[0];
            // 求xor校验和，注意：XOR运算从第二元素开始
            for (int i = 1; i < 5; i++)
            {
                xorResult ^= dataSend[i];
            }
            dataSend[5] = xorResult;//LRC校验码
            //CRC 错误
            dataSend[6] = 0x09;
            //结束码
            dataSend[dataSendLength - 5] = 0x03;
            //计算CRC32校验码,4字节,的校验码写入发送串
            byte[] dataSendCRCBytes = BitConverter.GetBytes(CRC32.GetCRC32(dataSend, dataSendLength - 4));
            dataSend[dataSendLength - 4] = dataSendCRCBytes[0];
            dataSend[dataSendLength - 3] = dataSendCRCBytes[1];
            dataSend[dataSendLength - 2] = dataSendCRCBytes[2];
            dataSend[dataSendLength - 1] = dataSendCRCBytes[3];
            //Console.WriteLine(byteToHexStr(dataSend));//输出
            return dataSend;
        }
        /// <summary>
        /// 将一个字节数组中指定长度的片段截取出来并返回
        /// buffer 待拆分字节数组,offset 偏移量,length 返回长度
        /// </summary>
        public byte[] Result_Array(byte[] buffer, int offset, int length)
        {
            byte[] result = new byte[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = buffer[i + offset];
            }
            //foreach (var item in result)
            //{
            //    Console.WriteLine(item);
            //}
            return result;
        }
        /// <summary>
        /// 数组比较是否相等
        /// </summary>
        /// <param name="bt1">数组1</param>
        /// <param name="bt2">数组2</param>
        /// <returns>true:相等，false:不相等</returns>
        public bool CompareArray(byte[] bt1, byte[] bt2)
        {
            int len1 = bt1.Length;
            int len2 = bt2.Length;
            if (len1 != len2)
            {
                return false;
            }
            for (int i = 0; i < len1; i++)
            {
                if (bt1[i] != bt2[i])
                    return false;
            }
            return true;
        }
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
                    returnStr += bytes[i].ToString("X2") + " ";
                }
            }
            return returnStr;
        }
        /// <summary>
        /// 字节数组转16进制字符串
        /// </summary>
        public string byteToHexStr(byte[] bytes, int length)
        {
            string returnStr = "";
            if (bytes != null)
            {
                for (int i = 0; i < length; i++)
                {
                    returnStr += bytes[i].ToString("X2") + " ";
                }
            }
            return returnStr;
        }
        /// <summary>
        /// 登录到服务器
        /// </summary>
        private string login(string MachineType, string MachineId)
        {
            string sql_mysql = "SELECT Id,Username from user_table WHERE MachineType=@MachineType and MachineId=@MachineId";
            //对密码进行加密
            //string MD5password = CommonUtils.getMD5Str(password);
            //System.Console.WriteLine(MD5password);
            MySqlParameter[] parameters = {
                    new MySqlParameter("@MachineType",MachineType),
                    new MySqlParameter("@MachineId",MachineId)
            };
            //声明一个Mysql数据库，连接的是云端的数据库
            MysqlDBHelper mysqlDBHelper = new MysqlDBHelper();
            try
            {
                //查询的结果
                DataTable dt = mysqlDBHelper.ExecuteDataTable(sql_mysql, parameters);
                //判断结果
                if (dt.Rows.Count == 0)
                {
                    //用户名或密码错误
                    return null;
                }
                //查询成功
                else if (dt.Rows.Count == 1)
                {
                    string userId = (string)dt.Rows[0]["Id"];
                    Console.WriteLine("登录成功！");
                    return userId;
                }
                return null;
            }
            //执行失败时
            catch (Exception ex)
            {
                return null;
            }
        }
        /// <summary>
        /// 每隔一段时间执行，检查dictionary目录中的到期情况
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        //public void timeClear(object ip)
        //{
        //    string clientSocket = ip as string;
        //    //callback委托将会在period时间间隔内重复执行，state参数可以传入想在callback委托中处理的对象，dueTime标识多久后callback开始执行，period标识多久执行一次callback
        //    Timer timer = new Timer(delegate
        //    {
        //        Console.WriteLine("开始检查dictionary的到期情况。");
        //        //foreach (var item in dictionary)
        //        //{
        //            if ((DateTime.UtcNow.Ticks - new DateTime(1970, 1, 1).Ticks) / 10000000 - dictionary[clientSocket].lastModifyTime > 10000)
        //            {
        //                dictionary.Remove(clientSocket);
        //            }
        //            //Console.WriteLine(item.Key);
        //        //}
        //        Console.WriteLine("dictionary中还剩余：");
        //        foreach (var item in dictionary)
        //        {
        //            Console.WriteLine(item.Key);
        //        }
        //    }, null, 2000, 5000);
        //}

    }

    /// <summary>
    /// Socket里的实体
    /// </summary>
    public class SocketEntity
    {
        public Socket socket { get; set; }//
        public byte[] tempAll_net { get; set; }// 每当端口有数据时就加入到此数组中
        public int iii { get; set; }// 负责tempAll数组的移位工作,volatile修饰后变量在所有线程中必须是同步的
        public int ppp { get; set; }// 工作指针
        public byte[] receivedFile;//下位机要上传的文件缓存区
        public int receivedFileLength { get; set; }// 下位机要上传的文件的长度
        public int pppp { get; set; }// 下位机要上传的文件缓存区工作指针
        public string receivedFileMD5Hash { get; set; }//下位机要上传的文件的校验码
        public string machineType { get; set; }//机器型号
        public string serialNumber { get; set; }//机器序列号SN
        public string userId { get; set; }//下位机要上传的文件的校验码
        public long lastModifyTime { get; set; }//最后一次离开时间，时间戳，如1509438332，代表2017/10/31 16:25:32
        /// <summary>
        /// 无参构造函数
        /// </summary>
        public SocketEntity()
        {
            tempAll_net = new byte[1024 * 4];
            iii = 0;
            ppp = 0;
            receivedFileLength = 0;
            pppp = 0;
            machineType = "";
            serialNumber = "";
            userId = "";
        }
        /// <summary>
        /// 有参构造函数
        /// </summary>
        /// <param name="socket"></param>
        public SocketEntity(Socket socket)
        {
            tempAll_net = new byte[1024 * 4];
            iii = 0;
            ppp = 0;
            receivedFileLength = 0;
            pppp = 0;
            machineType = "";
            serialNumber = "";
            userId = "";
            this.socket = socket;
            //lastModifyTime = (DateTime.UtcNow.Ticks - new DateTime(1970, 1, 1).Ticks) / 10000000;
        }
        /// <summary>
        /// 重置所有参数
        /// </summary>
        /// <param name="socket"></param>
        public void ClearAll()
        {
            iii = 0;
            ppp = 0;
            receivedFileLength = 0;
            pppp = 0;
            machineType = "";
            serialNumber = "";
            userId = "";
            //lastModifyTime = (DateTime.UtcNow.Ticks - new DateTime(1970, 1, 1).Ticks) / 10000000;
        }
    }
}
