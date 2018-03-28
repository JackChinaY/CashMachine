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

namespace CashMachine.utils
{
    /// <summary>
    /// 以太网连接
    /// </summary>
    class EthernetConnection
    {
        private string host = null;//默认主机IP
        private int port;//默认端口号

        Socket serverSocket = null;//服务器socket
        Thread serverThread = null;//
        public EthernetConnection()
        {
        }
        public EthernetConnection(string host, int port)
        {
            this.host = host;
            this.port = port;
            Init();
        }
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
                    IPAddress ip = IPAddress.Parse(host);

                    //端口号
                    IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, port);//服务器端监听任意端口
                    //使用IPv4地址，流式socket方式，tcp协议传递数据
                    serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    //创建好socket后，必须告诉socket绑定的IP地址和端口号 //socket监听哪个端口
                    serverSocket.Bind(ipEndPoint);
                    //同一个时间点过来10个客户端，排队 准备好接收连接
                    serverSocket.Listen(10);
                    //Console.WriteLine("监听已经打开...");
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
            this.iii = this.ppp = this.pppp = this.receivedFileLength = 0;
            receivedFileMD5Hash = "";
        }
        //记录通信用的Socket
        Dictionary<string, Socket> dictionary = new Dictionary<string, Socket>();
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
                    //创建通信用的Socket 接收连接
                    Socket clientSocket = socket.Accept();
                    //远程终点节 客户端的IP和端口号
                    string remoteEndPoint = clientSocket.RemoteEndPoint.ToString();
                    //IPEndPoint endPoint = (IPEndPoint)client.RemoteEndPoint;
                    //string me = Dns.GetHostName();//得到本机名称
                    //MessageBox.Show(me);
                    //Console.WriteLine("服务器接收中...");
                    //cboIpPort.Items.Add(point);
                    dictionary.Add(remoteEndPoint, clientSocket);
                    //为每个客户端开启线程 接收消息
                    clientThread = new Thread(ReceiveMsg);
                    clientThread.IsBackground = true;
                    clientThread.Start(clientSocket);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                    //Thread.CurrentThread.Abort();
                    break;
                }
            }
            //socket.Close();
            //socket = null;
            //Console.WriteLine("关闭主socket！");
        }

        private byte[] tempAll_net = new byte[1024 * 20];// 每当端口有数据时就加入到此数组中
        private volatile int iii = 0;// 负责tempAll数组的移位工作,volatile修饰后变量在所有线程中必须是同步的
        private volatile int ppp = 0;// 工作指针
        private byte[] receivedFile;//下位机要上传的文件缓存区
        private volatile int receivedFileLength = 0;// 下位机要上传的文件的长度
        private volatile int pppp = 0;// 下位机要上传的文件缓存区工作指针
        private string receivedFileMD5Hash;//下位机要上传的文件的校验码
        /// <summary>
        /// 接收消息
        /// </summary>
        public void ReceiveMsg(object o)
        {
            Socket clientSocket = o as Socket;
            while (true)
            {
                //接收客户端发送过来的数据
                try
                {
                    //定义byte数组存放从客户端接收过来的数据
                    byte[] buffer = new byte[1024 * 20];
                    //将接收过来的数据放到buffer中，并返回实际接受数据的长度
                    int n = clientSocket.Receive(buffer);
                    //n为0时，说明断开连接
                    //string words = Encoding.Default.GetString(buffer, 0, n);
                    //Console.WriteLine(clientSocket.RemoteEndPoint.ToString() + ":" + words);

                    if (n == 0) break;

                    // //当缓存区存满时,清零
                    if (iii + n >= 1024 * 19 || iii < ppp || iii - ppp > 20000)
                    {
                        iii = 0;// 计数从头开始
                        ppp = 0;
                        break;
                    }

                    // 将本次接收的数据存到全局变量tempAll数组中
                    for (int w = 0; w < n; w++)
                    {
                        tempAll_net[iii] = buffer[w];
                        iii++;
                    }

                    //Console.Write("本次接收到数据长度：" + n);
                    //Console.WriteLine(byteToHexStr(buffer, n));//输出接收数据
                    //Console.Write("  此时 ppp ：" + ppp + ", iii：" + iii);

                    // 对接收下来的数据段进行处理
                    if (iii - ppp >= 12)
                    {
                        #region 前三位是02 00 a1下载命令 文件信息
                        // 判断前三位是02 00 a1
                        if (tempAll_net[ppp] == (byte)0x02
                                && tempAll_net[ppp + 1] == (byte)0x00
                                && tempAll_net[ppp + 2] == (byte)0xa1)
                        {
                            //判断len长度
                            byte[] len = new byte[4];//4字节长度
                            len[0] = tempAll_net[ppp + 3];
                            len[1] = tempAll_net[ppp + 4];
                            len[2] = (byte)0x00;
                            len[3] = (byte)0x00;
                            int dataLen = BitConverter.ToInt32(len, 0);//byte数组转int，DATA的长度
                                                                       //Console.WriteLine("DATA段的数据长度：" + dataLen);

                            //如果所有字节都接收完毕时
                            if (iii - ppp >= 11 + dataLen)
                            {
                                //LRC 校验
                                if (!tempAll_net[ppp + 5].Equals(LRCCheck.getLRCHash(tempAll_net, ppp, 5)))
                                {
                                    dictionary[clientSocket.RemoteEndPoint.ToString()].Send(Result_Error_LRC());
                                    //Console.WriteLine("LEC 错误");
                                    iii = ppp = 0;
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
                                    string machineId = System.Text.Encoding.Default.GetString(tempAll_net, ppp + 6, 10).Trim('\0');//10bytes机器标识
                                    string serialNumber = System.Text.Encoding.Default.GetString(tempAll_net, ppp + 16, 12).Trim('\0');//12bytes序列号
                                    string fileName = System.Text.Encoding.Default.GetString(tempAll_net, ppp + 28, 50).Trim('\0');//50bytes文件名
                                    //Console.Write("机器标识：" + machineId + " 序列号：" + serialNumber + " 文件名：" + fileName);
                                    //回应下位机的请求
                                    sendFiletoXWJ_Get_File_Info_Net(clientSocket.RemoteEndPoint.ToString(), "database\\" + fileName);
                                    ppp += 11 + dataLen;
                                    if (iii == ppp)
                                    {
                                        iii = ppp = 0;
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

                        #region 前三位是02 00 a2 下载命令 文件
                        // 判断前三位是02 00 a2下载命令
                        else if (tempAll_net[ppp] == (byte)0x02
                                && tempAll_net[ppp + 1] == (byte)0x00
                                && tempAll_net[ppp + 2] == (byte)0xa2)
                        {
                            // 前三位是02 00 a2下载命令时，再判断len长度
                            byte[] len = new byte[4];//4字节长度
                            len[0] = tempAll_net[ppp + 3];
                            len[1] = tempAll_net[ppp + 4];
                            len[2] = (byte)0x00;
                            len[3] = (byte)0x00;
                            int dataLen = BitConverter.ToInt32(len, 0);//byte数组转int，DATA的长度
                            //Console.WriteLine("DATA数据长度：" + dataLen);
                            //如果缓存中字节长度和发送过来的数据预定长度一致的话，接下来就开始解析里面的文件并保存
                            if (iii - ppp >= 11 + dataLen)
                            {
                                //LR C校验
                                if (!tempAll_net[ppp + 5].Equals(LRCCheck.getLRCHash(tempAll_net, ppp, 5)))
                                {
                                    dictionary[clientSocket.RemoteEndPoint.ToString()].Send(Result_Error_LRC());
                                    iii = ppp = 0;
                                }
                                //CRC 校验
                                //else if (!CompareArray(Result_Array(tempAll_net, ppp + dataLen + 7, 4), BitConverter.GetBytes(CRC32.GetCRC32(tempAll_net, ppp, dataLen + 7))))
                                //{
                                //    dictionary[clientSocket.RemoteEndPoint.ToString()].Send(Result_Error_CRC());
                                //}
                                else
                                {
                                    //解析字节数组
                                    string machineId = System.Text.Encoding.Default.GetString(tempAll_net, ppp + 6, 10).Trim('\0');//10bytes机器标识
                                    string serialNumber = System.Text.Encoding.Default.GetString(tempAll_net, ppp + 16, 12).Trim('\0');//12bytes序列号
                                    string fileName = System.Text.Encoding.Default.GetString(tempAll_net, ppp + 28, 50).Trim('\0');//50bytes文件名
                                    len[0] = tempAll_net[ppp + 78];
                                    len[1] = tempAll_net[ppp + 79];
                                    len[2] = tempAll_net[ppp + 80];
                                    len[3] = tempAll_net[ppp + 81];
                                    int offset = BitConverter.ToInt32(len, 0);//偏移量
                                    len[0] = tempAll_net[ppp + 82];
                                    len[1] = tempAll_net[ppp + 83];
                                    len[2] = tempAll_net[ppp + 84];
                                    len[3] = tempAll_net[ppp + 85];
                                    int fileLength = BitConverter.ToInt32(len, 0);//本次要取的文件长度
                                    //Console.Write("机器标识：" + machineId + " 序列号：" + serialNumber + " 文件名：" + fileName + " 偏移量：" + offset + " 本次要取的文件长度：" + fileLength);
                                    //回应下位机的请求
                                    sendFiletoXWJ_Get_File_Net(clientSocket.RemoteEndPoint.ToString(), "database\\" + fileName, offset, fileLength);
                                    ppp += 11 + dataLen;
                                    if (iii == ppp)
                                    {
                                        iii = ppp = 0;
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
                        else if (tempAll_net[ppp] == (byte)0x02
                                && tempAll_net[ppp + 1] == (byte)0x00
                                && tempAll_net[ppp + 2] == (byte)0xb1)
                        {
                            // 前三位是02 00 b1下载命令时，再判断len长度
                            byte[] len = new byte[4];
                            len[0] = tempAll_net[ppp + 3];
                            len[1] = tempAll_net[ppp + 4];
                            len[2] = (byte)0x00;
                            len[3] = (byte)0x00;
                            int dataLen = BitConverter.ToInt32(len, 0);//byte数组转int，DATA的长度
                            //Console.WriteLine("DATA数据长度：" + dataLen);
                            //如果缓存中字节长度和发送过来的数据预定长度一致的话，接下来就开始解析里面的文件并保存
                            if (iii - ppp >= 11 + dataLen)
                            {
                                //LR C校验
                                if (!tempAll_net[ppp + 5].Equals(LRCCheck.getLRCHash(tempAll_net, ppp, 5)))
                                {
                                    dictionary[clientSocket.RemoteEndPoint.ToString()].Send(Result_Error_LRC());
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
                                    string machineId = System.Text.Encoding.Default.GetString(tempAll_net, ppp + 6, 10).Trim('\0');//10bytes机器标识
                                    string serialNumber = System.Text.Encoding.Default.GetString(tempAll_net, ppp + 16, 12).Trim('\0');//12bytes序列号
                                    string fileName = System.Text.Encoding.Default.GetString(tempAll_net, ppp + 28, 50).Trim('\0');//50bytes文件名
                                    len[0] = tempAll_net[ppp + 78];
                                    len[1] = tempAll_net[ppp + 79];
                                    len[2] = tempAll_net[ppp + 80];
                                    len[3] = tempAll_net[ppp + 81];
                                    int fileLength = BitConverter.ToInt32(len, 0);//该文件总长度
                                    string fileMD5Hash = System.Text.Encoding.Default.GetString(tempAll_net, 82, 32).Trim('\0');//32文件校验码
                                    //Console.Write("机器标识：" + machineId + " 序列号：" + serialNumber + " 文件名：" + fileName + " 该文件总长度：" + fileLength + " 文件校验码：" + fileMD5Hash);
                                    //回应下位机的请求
                                    sendFiletoXWJ_Send_File_Info_Net(clientSocket.RemoteEndPoint.ToString(), "database\\" + System.IO.Path.GetFileName(fileName), fileLength, fileMD5Hash);
                                    ppp += 11 + dataLen;
                                    if (iii == ppp)
                                    {
                                        iii = ppp = 0;
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
                        else if (tempAll_net[ppp] == (byte)0x02
                                && tempAll_net[ppp + 1] == (byte)0x00
                                && tempAll_net[ppp + 2] == (byte)0xb2)
                        {
                            // 前三位是02 00 b2下载命令时，再判断len长度
                            byte[] len = new byte[4];
                            len[0] = tempAll_net[ppp + 3];
                            len[1] = tempAll_net[ppp + 4];
                            len[2] = (byte)0x00;
                            len[3] = (byte)0x00;
                            int dataLen = BitConverter.ToInt32(len, 0);//byte数组转int，DATA的长度
                            //Console.WriteLine("DATA数据长度：" + dataLen);
                            //如果缓存中字节长度和发送过来的数据预定长度一致的话，接下来就开始解析里面的文件并保存
                            if (iii - ppp >= 11 + dataLen)
                            {
                                //LRC 校验
                                if (!tempAll_net[ppp + 5].Equals(LRCCheck.getLRCHash(tempAll_net, ppp, 5)))
                                {
                                    dictionary[clientSocket.RemoteEndPoint.ToString()].Send(Result_Error_LRC());
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
                                    string machineId = System.Text.Encoding.Default.GetString(tempAll_net, ppp + 6, 10).Trim('\0');//10bytes机器标识
                                    string serialNumber = System.Text.Encoding.Default.GetString(tempAll_net, ppp + 16, 12).Trim('\0');//12bytes序列号
                                    string fileName = System.Text.Encoding.Default.GetString(tempAll_net, ppp + 28, 50).Trim('\0');//50bytes文件名
                                    len[0] = tempAll_net[ppp + 78];
                                    len[1] = tempAll_net[ppp + 79];
                                    len[2] = tempAll_net[ppp + 80];
                                    len[3] = tempAll_net[ppp + 81];
                                    int offset = BitConverter.ToInt32(len, 0);//偏移量
                                    len[0] = tempAll_net[ppp + 82];
                                    len[1] = tempAll_net[ppp + 83];
                                    len[2] = tempAll_net[ppp + 84];
                                    len[3] = tempAll_net[ppp + 85];
                                    int fileLength = BitConverter.ToInt32(len, 0);//本次待接收的文件长度
                                    //Console.WriteLine("机器标识：" + machineId + " 序列号：" + serialNumber + " 文件名：" + fileName + " 偏移量：" + offset + " 本次待接收的文件长度：" + fileLength);
                                    //回应下位机的请求
                                    sendFiletoXWJ_Send_File_Net(clientSocket.RemoteEndPoint.ToString(), System.IO.Path.GetFileName(fileName), offset, fileLength);
                                    ppp += 11 + dataLen;
                                    if (iii == ppp)
                                    {
                                        iii = ppp = 0;
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
                            ppp++;//为了容错，ppp依次往后移动
                        }
                        //tempAll数组处理完后指针就归零
                        iii = ppp = 0;
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
                    if (clientSocket.Poll(-1, SelectMode.SelectRead))
                    {
                        //Console.WriteLine("客户端已断开连接！");
                        dictionary.Remove(clientSocket.RemoteEndPoint.ToString());
                        //foreach (var item in dictionary)
                        //{
                        //    Console.WriteLine(item.Key);
                        //}
                        break;
                    }
                    //MessageBox.Show(ex.Message, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    //break;
                }
            }
            //Console.WriteLine("关闭接收数据的线程！");
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
                dataSend[i] = tempAll_net[ppp + i];
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
            dictionary[clientSocketId].Send(dataSend);
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
                dataSend[i] = tempAll_net[ppp + i];
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
            dictionary[clientSocketId].Send(dataSend);
            //Console.WriteLine(byteToHexStr(dataSend));//输出
            //Console.Write("响应命令2串的长度：" + dataSend.Length);
        }
        /// <summary>
        /// part1_1 以太网 下位机上传文件信息 0xb1 Send_File_Info 3
        /// </summary>
        public void sendFiletoXWJ_Send_File_Info_Net(string clientSocketId, string filePath, int fileLength, string fileMD5Hash)
        {
            //下位机上传的文件的总长度
            receivedFileLength = fileLength;
            //开辟一个缓存区来存放这个文件
            receivedFile = new byte[fileLength];
            //上传文件的MD5校验码
            receivedFileMD5Hash = fileMD5Hash;
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
            dictionary[clientSocketId].Send(dataSend);
            //Console.WriteLine(byteToHexStr(dataSend));//输出
            //Console.Write("  响应命令3串的长度：" + dataSend.Length);
        }
        /// <summary>
        /// part1_1 以太网 下位机上传文件 0xb1 Send_File 4
        /// </summary>
        public void sendFiletoXWJ_Send_File_Net(string clientSocketId, string fileName, int offset, int fileLength)
        {
            for (int i = 0; i < fileLength; i++, pppp++)
            {
                receivedFile[pppp] = tempAll_net[ppp + 86 + i];
            }
            //Console.WriteLine(CheckUtils.MD5Check.getMD5Hash(receivedFile));
            if (pppp == receivedFileLength && receivedFileMD5Hash.Equals(CheckUtils.MD5Check.getMD5Hash(receivedFile)))
            {
                //先移动旧版本的文件到备份文件夹中
                OperateFile.MoveFile("database\\" + fileName, "database\\database.old\\" + fileName);
                //保存文件
                OperateFile.BytetoFile(receivedFile, 0, receivedFileLength, "database\\" + fileName);
                //重新置零
                pppp = receivedFileLength = 0;
                receivedFileMD5Hash = null;
                receivedFile = null;
                dictionary[clientSocketId].Send(Result_Success());
                MessageBox.Show("The file is saved successfully!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            //发送
            dictionary[clientSocketId].Send(Result_Success());
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
    }
}
