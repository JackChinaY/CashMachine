using CheckUtils;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashMachine.utils
{
    /// <summary>
    /// 串口连接
    /// </summary>
    class SerialPortConnection
    {
        //声明一些参数
        private SerialPort serialPort;//串口
        private string serialPortName = null;//串口名
        private int baudRate;//波特率
        private Parity parity;//校验位
        private int dataBits;//数据位
        private StopBits stopBits;//停止位

        public SerialPortConnection() { }
        public SerialPortConnection(string serialPortName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
        {
            this.serialPortName = serialPortName;
            this.baudRate = baudRate;
            this.parity = parity;
            this.dataBits = dataBits;
            this.stopBits = stopBits;
        }
        /// <summary>
        /// 打开串口
        /// </summary>
        public void Open()
        {
            serialPort = new SerialPort(serialPortName, baudRate, parity, dataBits, stopBits);
            //打开串口
            serialPort.Open();
            //设置监听事件，当端口一有数据就执行此函数
            serialPort.DataReceived += COM_DataReceived;
        }
        /// <summary>
        /// 关闭串口
        /// </summary>
        public void Close()
        {
            serialPort.Close();
        }

        private byte[] tempAll_net = new byte[1024 * 20];// 每当端口有数据时就加入到此数组中
        private volatile int iii = 0;// 负责tempAll数组的移位工作,volatile修饰后变量在所有线程中必须是同步的
        private volatile int ppp = 0;// 工作指针
        private byte[] receivedFile;//下位机要上传的文件缓存区
        private volatile int receivedFileLength = 0;// 下位机要上传的文件的长度
        private volatile int pppp = 0;// 下位机要上传的文件缓存区工作指针
        private string receivedFileMD5Hash;//下位机要上传的文件的校验码

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

                //因为要访问ui资源，所以需要使用invoke方式同步ui
                //interfaceUpdateHandle = new HandleInterfaceUpdateDelagate(UpdateTextBox);//实例化委托对象
                //Dispatcher.Invoke(interfaceUpdateHandle, new string[] { Encoding.Default.GetString(buf) });

                // //当缓存区存满时,清零
                if (iii + n >= 1024 * 19 || iii < ppp || iii - ppp > 10240)
                {
                    iii = 0;// 计数从头开始
                    ppp = 0;
                    return;
                }

                Console.WriteLine("本次接收到数据长度：" + n);

                // 将本次接收的数据存到全局变量tempAll数组中
                for (int w = 0; w < n; w++)
                {
                    tempAll_net[iii] = buffer[w];
                    iii++;
                }
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
                        Console.WriteLine("DATA段的数据长度：" + dataLen);
                        //如果所有字节都接收完毕时
                        if (iii - ppp >= 11 + dataLen)
                        {
                            //LR C校验
                            if (!tempAll_net[ppp + 5].Equals(LRCCheck.getLRCHash(tempAll_net, ppp, 5)))
                            {
                                serialPort.Write(Result_Error_LRC(), 0, Result_Error_LRC().Length);
                            }
                            //CRC 校验
                            else if (!CompareArray(Result_Array(tempAll_net, ppp + dataLen + 7, 4), BitConverter.GetBytes(CRC32.GetCRC32(tempAll_net, ppp, dataLen + 7))))
                            {
                                serialPort.Write(Result_Error_CRC(), 0, Result_Error_CRC().Length);
                            }
                            else
                            {
                                //解析字节数组
                                string machineId = System.Text.Encoding.Default.GetString(tempAll_net, ppp + 6, 10).Trim('\0');//10bytes机器标识
                                string serialNumber = System.Text.Encoding.Default.GetString(tempAll_net, ppp + 16, 12).Trim('\0');//12bytes序列号
                                string fileName = System.Text.Encoding.Default.GetString(tempAll_net, ppp + 28, 50).Trim('\0');//50bytes文件名
                                Console.WriteLine("机器标识：" + machineId + " 序列号：" + serialNumber + " 文件名：" + fileName);
                                //回应下位机的请求
                                sendFiletoXWJ_Get_File_Info_Net("database\\" + fileName);
                                ppp += 11 + dataLen;
                                if (iii == ppp)
                                {
                                    iii = ppp = 0;
                                    //Console.WriteLine("ppp iii此时置0了：");
                                }
                            }
                        }
                        else
                        {
                            return;
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
                        Console.WriteLine("DATA数据长度：" + dataLen);
                        //如果缓存中字节长度和发送过来的数据预定长度一致的话，接下来就开始解析里面的文件并保存
                        if (iii - ppp >= 11 + dataLen)
                        {
                            //LR C校验
                            if (!tempAll_net[ppp + 5].Equals(LRCCheck.getLRCHash(tempAll_net, ppp, 5)))
                            {
                                serialPort.Write(Result_Error_LRC(), 0, Result_Error_LRC().Length);
                            }
                            //CRC 校验
                            else if (!CompareArray(Result_Array(tempAll_net, ppp + dataLen + 7, 4), BitConverter.GetBytes(CRC32.GetCRC32(tempAll_net, ppp, dataLen + 7))))
                            {
                                serialPort.Write(Result_Error_CRC(), 0, Result_Error_CRC().Length);
                            }
                            else
                            {
                                //解析字节数组
                                string machineId = System.Text.Encoding.Default.GetString(tempAll_net, ppp + 6, 10).Trim('\0');//10bytes机器标识
                                string serialNumber = System.Text.Encoding.Default.GetString(tempAll_net, ppp + 16, 12).Trim('\0');//12bytes序列号
                                string fileName = System.Text.Encoding.Default.GetString(tempAll_net, ppp + 28, 50).Trim('\0');//50bytes文件名
                                len[0] = tempAll_net[ppp + 78];
                                len[1] = tempAll_net[ppp + 79];
                                int offset = BitConverter.ToInt32(len, 0);//偏移量
                                len[0] = tempAll_net[ppp + 80];
                                len[1] = tempAll_net[ppp + 81];
                                int fileLength = BitConverter.ToInt32(len, 0);//本次要取的文件长度
                                Console.WriteLine("机器标识：" + machineId + " 序列号：" + serialNumber + " 文件名：" + fileName + " 偏移量：" + offset + " 本次要取的文件长度：" + fileLength);
                                //回应下位机的请求
                                sendFiletoXWJ_Get_File_Net("database\\" + fileName, offset, fileLength);
                                ppp += 11 + dataLen;
                                if (iii == ppp)
                                {
                                    iii = ppp = 0;
                                    //Console.WriteLine("ppp iii此时置0了：");
                                }
                            }
                        }
                        else
                        {
                            return;
                        }
                    }
                    #endregion

                    #region 前三位是02 00 b1  上传命令 文件信息
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
                        Console.WriteLine("DATA数据长度：" + dataLen);
                        //如果缓存中字节长度和发送过来的数据预定长度一致的话，接下来就开始解析里面的文件并保存
                        if (iii - ppp >= 11 + dataLen)
                        {
                            //LR C校验
                            if (!tempAll_net[ppp + 5].Equals(LRCCheck.getLRCHash(tempAll_net, ppp, 5)))
                            {
                                serialPort.Write(Result_Error_LRC(), 0, Result_Error_LRC().Length);
                            }
                            //CRC 校验
                            else if (!CompareArray(Result_Array(tempAll_net, ppp + dataLen + 7, 4), BitConverter.GetBytes(CRC32.GetCRC32(tempAll_net, ppp, dataLen + 7))))
                            {
                                serialPort.Write(Result_Error_CRC(), 0, Result_Error_CRC().Length);
                            }
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
                                Console.WriteLine("机器标识：" + machineId + " 序列号：" + serialNumber + " 文件名：" + fileName + " 该文件总长度：" + fileLength + " 文件校验码：" + fileMD5Hash);
                                //回应下位机的请求
                                sendFiletoXWJ_Send_File_Info_Net("database\\" + fileName, fileLength, fileMD5Hash);
                                ppp += 11 + dataLen;
                                if (iii == ppp)
                                {
                                    iii = ppp = 0;
                                    //Console.WriteLine("ppp iii此时置0了：");
                                }
                            }
                        }
                        else
                        {
                            return;
                        }
                    }
                    #endregion

                    #region 前三位是02 00 b2 上传命令 文件
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
                        Console.WriteLine("DATA数据长度：" + dataLen);
                        //如果缓存中字节长度和发送过来的数据预定长度一致的话，接下来就开始解析里面的文件并保存
                        if (iii - ppp >= 11 + dataLen)
                        {
                            //LRC 校验
                            if (!tempAll_net[ppp + 5].Equals(LRCCheck.getLRCHash(tempAll_net, ppp, 5)))
                            {
                                serialPort.Write(Result_Error_LRC(), 0, Result_Error_LRC().Length);
                            }
                            //CRC 校验
                            else if (!CompareArray(Result_Array(tempAll_net, ppp + dataLen + 7, 4), BitConverter.GetBytes(CRC32.GetCRC32(tempAll_net, ppp, dataLen + 7))))
                            {
                                serialPort.Write(Result_Error_CRC(), 0, Result_Error_CRC().Length);
                            }
                            else
                            {
                                //解析字节数组
                                string machineId = System.Text.Encoding.Default.GetString(tempAll_net, ppp + 6, 10).Trim('\0');//10bytes机器标识
                                string serialNumber = System.Text.Encoding.Default.GetString(tempAll_net, ppp + 16, 12).Trim('\0');//12bytes序列号
                                string fileName = System.Text.Encoding.Default.GetString(tempAll_net, ppp + 28, 50).Trim('\0');//50bytes文件名
                                len[0] = tempAll_net[ppp + 78];
                                len[1] = tempAll_net[ppp + 79];
                                int offset = BitConverter.ToInt32(len, 0);//偏移量
                                len[0] = tempAll_net[ppp + 80];
                                len[1] = tempAll_net[ppp + 81];
                                int fileLength = BitConverter.ToInt32(len, 0);//本次待接收的文件长度
                                Console.WriteLine("机器标识：" + machineId + " 序列号：" + serialNumber + " 文件名：" + fileName + " 偏移量：" + offset + " 本次待接收的文件长度：" + fileLength);
                                //回应下位机的请求
                                sendFiletoXWJ_Send_File_Net(fileName, offset, fileLength);
                                ppp += 11 + dataLen;
                                if (iii == ppp)
                                {
                                    iii = ppp = 0;
                                    //Console.WriteLine("ppp iii此时置0了：");
                                }
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
                //MessageBox.Show(ex.Message, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
        }
        /// <summary>
        /// part1_1 以太网 向下位机发送 响应发送 0xa1 Get_File_Info
        /// STX CLA INS LENL LENH LRC DATA ETX CRC
        /// </summary>
        public void sendFiletoXWJ_Get_File_Info_Net(string filePath)
        {
            //将文件转换成字节数组
            //byte[] data = OperateFile.FiletoByte(filePath);
            //DATA的长度
            int dataLength = 10 + 12 + 50 + 10 + 4 + 4 + 32;//94
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
            byte[] fileLastWriteTime = System.Text.Encoding.Default.GetBytes(OperateFile.GetFileLastWriteTime(filePath).ToString());//文件版本号转换成字节数组
            for (int i = 88; i < 88 + fileLastWriteTime.Length; i++)
            {
                dataSend[i] = fileLastWriteTime[i - 88];
            }

            //获取文件字节大小并写入发送串,4字节
            byte[] fileLength = BitConverter.GetBytes(OperateFile.GetFileLength(filePath));//文件版本号转换成字节数组
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
            serialPort.Write(dataSend, 0, dataSendLength);
        }
        /// <summary>
        /// part1_1 以太网 向下位机发送 响应发送 0xa2 Get_File
        /// STX CLA INS LENL LENH LRC DATA ETX CRC
        /// </summary>
        public void sendFiletoXWJ_Get_File_Net(string filePath, int offset, int fileLength)
        {
            //将文件转换成字节数组
            byte[] data = OperateFile.FiletoByte(filePath);
            //DATA的长度
            int dataLength = 10 + 12 + 50 + fileLength;//72 + N
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

            //将10bytes机器标识、8bytes序列号、50bytes文件名写入发送串  DATA部分
            for (int i = 6; i < 6 + 72; i++)
            {
                dataSend[i] = tempAll_net[ppp + i];
            }
            //将文件以字节的形式写入发送串 N字节
            for (int i = 78; i < 78 + fileLength; i++)
            {
                dataSend[i] = data[i - 78 + offset];
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
            serialPort.Write(dataSend, 0, dataSendLength);
        }
        /// <summary>
        /// part1_1 以太网 向下位机发送 响应发送 0xb1 Send_File_Info
        /// STX CLA INS LENL LENH LRC DATA ETX CRC
        /// </summary>
        public void sendFiletoXWJ_Send_File_Info_Net(string filePath, int fileLength, string fileMD5Hash)
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
            serialPort.Write(dataSend, 0, dataSendLength);
        }

        /// <summary>
        /// part1_1 以太网 向下位机发送 响应发送 0xb1  Send_File
        /// STX CLA INS LENL LENH LRC DATA ETX CRC
        /// </summary>
        public void sendFiletoXWJ_Send_File_Net(string filePath, int offset, int fileLength)
        {
            //
            for (int i = 0; i < fileLength; i++, pppp++)
            {
                receivedFile[pppp] = tempAll_net[ppp + 82 + i];
            }
            if (pppp == receivedFileLength && receivedFileMD5Hash.Equals(CheckUtils.MD5Check.getMD5Hash(receivedFile)))
            {
                //保存文件
                OperateFile.BytetoFile(receivedFile, 0, receivedFileLength, "database\\副本" + filePath);
                //重新置零
                pppp = receivedFileLength = 0;
                receivedFileMD5Hash = null;
                receivedFile = null;

            }
            serialPort.Write(Result_Success(), 0, Result_Success().Length);
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
        /////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 测试用 模拟下位机向上位机发送命令 Get_File_Info
        /// STX CLA INS LENL LENH LRC DATA ETX CRC
        /// </summary>
        public void sendFiletoXWJ_TEST1(string filePath)
        {
            //byte[] data = System.Text.Encoding.Default.GetBytes(txtSend.Text);//获取输入框内容并转换成字节
            //int dataLength = data.Length;//要发送的文件的长度
            //Console.WriteLine(OperateFile.GetFileVersionNumber(filePath));
            //return;

            //将文件转换成字节数组
            //byte[] data = OperateFile.FiletoByte(filePath);
            //DATA的长度
            int dataLength = 10 + 12 + 50;//72
            //总发送数据的长度79 STX CLA INS LENL LENH LRC DATA ETX CRC
            int dataSendLength = 11 + dataLength;
            //总发送串
            byte[] dataSend = new byte[dataSendLength];
            //接下来拼接发送串
            dataSend[0] = 0x02;//起始码 STX
            dataSend[1] = 0x00;//通讯包标志 CLA
            dataSend[2] = 0xa1;//命令码 INS

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

            //将10bytes机器标识、12bytes序列号、50bytes文件名写入发送串  DATA部分
            byte[] machineId = System.Text.Encoding.Default.GetBytes("HGT-752");//10
            for (int i = 6; i < 6 + machineId.Length; i++)
            {
                dataSend[i] = machineId[i - 6];
            }

            byte[] machineSN = System.Text.Encoding.Default.GetBytes("170712120001");//12
            for (int i = 16; i < 6 + 10 + machineSN.Length; i++)
            {
                dataSend[i] = machineSN[i - 16];
            }

            byte[] fileName = System.Text.Encoding.Default.GetBytes("buyer123.txt");//文件名50
            for (int i = 28; i < 6 + 10 + 12 + fileName.Length; i++)
            {
                dataSend[i] = fileName[i - 28];
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
            serialPort.Write(dataSend, 0, dataSendLength);

            //Console.WriteLine(OperateFile.byteToHexStr(dataSend));
            //OperateFile.BytetoFile(data, 0,data.Length, "database\\22.txt");
        }

        /// <summary>
        /// 测试用 模拟下位机向上位机发送命令 Get_File
        /// STX CLA INS LENL LENH LRC DATA ETX CRC
        /// </summary>
        public void sendFiletoXWJ_TEST2(string filePath)
        {
            //将文件转换成字节数组
            byte[] data = OperateFile.FiletoByte(filePath);
            //DATA的长度
            int dataLength = 10 + 12 + 50 + 2 + 2;//76
            //总发送数据的长度83 STX CLA INS LENL LENH LRC DATA ETX CRC
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

            //将10bytes机器标识、8bytes序列号、50bytes文件名写入发送串  DATA部分
            //机器标识 10位
            byte[] machineId = System.Text.Encoding.Default.GetBytes("HGT-752");
            for (int i = 6; i < 6 + machineId.Length; i++)
            {
                dataSend[i] = machineId[i - 6];
            }
            //序列号 8位
            byte[] machineSN = System.Text.Encoding.Default.GetBytes("170712120001");
            for (int i = 16; i < 6 + 10 + machineSN.Length; i++)
            {
                dataSend[i] = machineSN[i - 16];
            }
            //文件名 50位
            byte[] fileName = System.Text.Encoding.Default.GetBytes("buyer123.txt");
            for (int i = 28; i < 6 + 10 + 12 + fileName.Length; i++)
            {
                dataSend[i] = fileName[i - 28];
            }
            //偏移量 2位
            dataSend[78] = 0x00;
            dataSend[79] = 0x00;
            //文件长度 2位
            byte[] dataLengths = BitConverter.GetBytes(data.Length);
            dataSend[80] = dataLengths[0];//长度的低位 LENL
            dataSend[81] = dataLengths[1];//长度的高位 LENH

            //结束码
            dataSend[dataSendLength - 5] = 0x03;
            //计算CRC32校验码,4字节,的校验码写入发送串
            byte[] dataSendCRCBytes = BitConverter.GetBytes(CRC32.GetCRC32(dataSend, dataSendLength - 4));
            dataSend[dataSendLength - 4] = dataSendCRCBytes[0];
            dataSend[dataSendLength - 3] = dataSendCRCBytes[1];
            dataSend[dataSendLength - 2] = dataSendCRCBytes[2];
            dataSend[dataSendLength - 1] = dataSendCRCBytes[3];
            //发送
            serialPort.Write(dataSend, 0, dataSendLength);

            //Console.WriteLine(OperateFile.byteToHexStr(dataSend));
            //OperateFile.BytetoFile(data, 0,data.Length, "database\\22.txt");
        }

        /// <summary>
        /// 测试用 模拟下位机向上位机发送命令 Send_File_Info
        /// STX CLA INS LENL LENH LRC DATA ETX CRC
        /// </summary>
        public void sendFiletoXWJ_TEST3(string filePath)
        {
            //将文件转换成字节数组
            byte[] data = OperateFile.FiletoByte(filePath);
            //DATA的长度
            int dataLength = 10 + 12 + 50 + 4 + 32;
            //总发送数据的长度 STX CLA INS LENL LENH LRC DATA ETX CRC
            int dataSendLength = 11 + dataLength;
            //总发送串
            byte[] dataSend = new byte[dataSendLength];
            //接下来拼接发送串
            dataSend[0] = 0x02;//起始码 STX
            dataSend[1] = 0x00;//通讯包标志 CLA
            dataSend[2] = 0xb1;//命令码 INS

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
            //机器标识 10位
            byte[] machineId = System.Text.Encoding.Default.GetBytes("HGT-752");
            for (int i = 6; i < 6 + machineId.Length; i++)
            {
                dataSend[i] = machineId[i - 6];
            }
            //序列号 12位
            byte[] machineSN = System.Text.Encoding.Default.GetBytes("170712120001");
            for (int i = 16; i < 6 + 10 + machineSN.Length; i++)
            {
                dataSend[i] = machineSN[i - 16];
            }
            //文件名 50位
            byte[] fileName = System.Text.Encoding.Default.GetBytes("buyer123.txt");
            for (int i = 28; i < 6 + 10 + 12 + fileName.Length; i++)
            {
                dataSend[i] = fileName[i - 28];
            }
            //文件长度 4
            byte[] fileLength = BitConverter.GetBytes(data.Length);
            for (int i = 78; i < 78 + fileLength.Length; i++)
            {
                dataSend[i] = fileLength[i - 78];
            }

            //获取文件校验码并写入发送串,32
            byte[] fileMD5Hash = Encoding.Default.GetBytes(MD5Check.getMD5Hash(filePath));
            for (int i = 82; i < 82 + fileMD5Hash.Length; i++)
            {
                dataSend[i] = fileMD5Hash[i - 82];
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
            serialPort.Write(dataSend, 0, dataSendLength);

            //Console.WriteLine(OperateFile.byteToHexStr(dataSend));
            //OperateFile.BytetoFile(data, 0,data.Length, "database\\22.txt");
        }

        /// <summary>
        /// 测试用 模拟下位机向上位机发送命令参 Send_File
        /// STX CLA INS LENL LENH LRC DATA ETX CRC
        /// </summary>
        public void sendFiletoXWJ_TEST4(string filePath)
        {
            //将文件转换成字节数组
            byte[] data = OperateFile.FiletoByte(filePath);
            //DATA的长度
            int dataLength = 10 + 12 + 50 + 2 + 2 + data.Length;//72+N
            //总发送数据的长度 STX CLA INS LENL LENH LRC DATA ETX CRC
            int dataSendLength = 11 + dataLength;
            //总发送串
            byte[] dataSend = new byte[dataSendLength];
            //接下来拼接发送串
            dataSend[0] = 0x02;//起始码 STX
            dataSend[1] = 0x00;//通讯包标志 CLA
            dataSend[2] = 0xb2;//命令码 INS

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

            //将10bytes机器标识、12bytes序列号、50bytes文件名写入发送串  DATA部分
            //机器标识 10位
            byte[] machineId = System.Text.Encoding.Default.GetBytes("HGT-752");
            for (int i = 6; i < 6 + machineId.Length; i++)
            {
                dataSend[i] = machineId[i - 6];
            }
            //序列号 12位
            byte[] machineSN = System.Text.Encoding.Default.GetBytes("170712120001");
            for (int i = 16; i < 6 + 10 + machineSN.Length; i++)
            {
                dataSend[i] = machineSN[i - 16];
            }
            //文件名 50位
            byte[] fileName = System.Text.Encoding.Default.GetBytes("buyer123.txt");
            for (int i = 28; i < 6 + 10 + 12 + fileName.Length; i++)
            {
                dataSend[i] = fileName[i - 28];
            }
            //偏移量 2位
            dataSend[78] = 0x00;
            dataSend[79] = 0x00;
            //文件长度 2位
            byte[] dataLengths = BitConverter.GetBytes(data.Length);
            dataSend[80] = dataLengths[0];//长度的低位 LENL
            dataSend[81] = dataLengths[1];//长度的高位 LENH
            //将文件以字节的形式写入发送串 N字节
            for (int i = 82; i < 82 + data.Length; i++)
            {
                dataSend[i] = data[i - 82];
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
            serialPort.Write(dataSend, 0, dataSendLength);

            //Console.WriteLine(OperateFile.byteToHexStr(dataSend));
            //OperateFile.BytetoFile(data, 0,data.Length, "database\\22.txt");
        }
    }
}
