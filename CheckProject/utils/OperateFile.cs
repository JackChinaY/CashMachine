using CheckProject.utils;
using System;
using System.IO;
using System.Windows;

namespace CashMachine.utils
{
    /// <summary>
    /// 获取文件大小、删除文件、创建文件夹、文件转化成byte[]数组、字节数组byte[]转文件
    /// </summary>
    class OperateFile
    {
        //测试代码
        //byte[] buffur = FiletoByte("database\\11.txt");
        //Console.WriteLine("文件的字节大小：" + buffur.Length);
        //Console.WriteLine(byteToHexStr(buffur));
        //    if (BytetoFile(buffur, "database\\22.txt"))
        //    {
        //        Console.WriteLine("文件生成成功");
        //    }
        //pluDB programmingDB
        //Console.WriteLine(GetFileLength("database\\pluDB.db"));

        //Console.WriteLine(DeleteFile("database\\pluDB.db"));



        /// <summary>
        /// 获取文件字节大小
        /// </summary>
        public static long GetFileLength(string filePath)
        {
            //string basePath = Environment.CurrentDirectory.ToString() + "\\";//获取exe所在文件夹的完整路径
            if (!File.Exists(filePath))
            {
                MessageBox.Show("文件不存在!", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                //Console.WriteLine("文件不存在！");
                return 0;
            }
            else
            {
                //定义一个FileInfo对象
                FileInfo fileInfo = new FileInfo(filePath);
                return fileInfo.Length;
            }
        }

        /// <summary>
        /// 获取文件版本号
        /// </summary>
        public static string GetFileVersionNumber(string filePath)
        {
            if (!File.Exists(filePath))
            {
                MessageBox.Show("文件不存在!", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return null;
            }
            else
            {
                string fileVersionNumber = "";
                //获得文件的版本号
                System.Diagnostics.FileVersionInfo file = System.Diagnostics.FileVersionInfo.GetVersionInfo(filePath);
                //fileVersionNumber = file.FileVersion;
                return fileVersionNumber;
            }
        }

        /// <summary>
        /// 获取文件最后一次修改日期 返回时间戳格式 如：1498636046
        /// </summary>
        public static long GetFileLastWriteTime(string filePath)
        {
            if (!File.Exists(filePath))
            {
                MessageBox.Show("文件不存在!", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return 0;
            }
            else
            {
                //定义一个FileInfo对象
                FileInfo fileInfo = new FileInfo(filePath);
                return CommonUtils.DateTimeToLong(fileInfo.LastWriteTimeUtc);//返回时间戳格式
            }
        }



        /// <summary>
        /// 将文件转化成byte[]数组
        /// </summary>
        public static byte[] FiletoByte(string filePath)
        {
            //判断文件是否存在
            if (!File.Exists(filePath))
            {
                MessageBox.Show("文件不存在!", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return null;
            }
            else
            {
                FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                try
                {
                    byte[] buffur = new byte[fileStream.Length];
                    fileStream.Read(buffur, 0, (int)fileStream.Length);
                    return buffur;
                }
                catch (Exception e)
                {
                    MessageBox.Show("文件读取失败,可能原因：" + e.Message, "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    //Console.WriteLine(e.Message);
                    return null;
                }
                finally
                {
                    if (fileStream != null)
                    {
                        //关闭资源
                        fileStream.Close();
                    }
                }
            }
            
        }

        /// <summary>
        /// 字节数组byte[]转文件 如果文件夹中存在文件名相同的文件，则覆盖掉源文件
        /// 参数说明 fileByte：字节数组，offset：偏移量，count:接收的长度，filePath：保存的文件路径和名字（如："database\\22.txt"）
        /// </summary>
        public static void BytetoFile(byte[] fileByte, int offset, int count, string filePath)
        {
            //先删除就版本
            //DeleteFile(filePath);
            FileStream fileStream = null;
            try
            {
                fileStream = new FileStream(filePath, FileMode.Create);
                fileStream.Write(fileByte, offset, count);
            }
            catch (Exception e)
            {
                MessageBox.Show("Cause：" + e.Message, "Information", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (fileStream != null)
                    fileStream.Close();//关闭资源
            }
        }
        /// <summary>
        /// 通过字节数组已追加的方式保存文件
        /// </summary>
        private void AppendFileByBytes(byte[] fileByte, int offset, int count, string filePath)
        {
            FileStream fileStream = null;
            try
            {
                fileStream = new FileStream(filePath, FileMode.Append);
                fileStream.Write(fileByte, offset, count);
            }
            catch (Exception e)
            {
                MessageBox.Show("Cause：" + e.Message, "Information", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (fileStream != null)
                    fileStream.Close();//关闭资源
            }
        }
        /// <summary>
        /// 删除文件夹中指定的文件 如果文件存在则删除，不存在就不操作
        /// </summary>
        public static void DeleteFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        /// <summary>
        /// 判断指定文件夹是否存在，不存在就创建
        /// </summary>
        public static void IsDirectoryExists(string filePath)
        {
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
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
        /// 将一个文件移动到另一个地方  新文件夹中若存在原来的文件则会事先先删除掉 （只能在同个盘符进行操作)
        /// </summary>
        public static void MoveFile(string fileOldPath, string fileNewPath)
        {
            if (File.Exists(fileOldPath))
            {
                //先删除目标文件中已存在的文件
                DeleteFile(fileNewPath);
                //参数1：要移动的源文件路径，参数2：移动后的目标文件路径
                File.Move(fileOldPath, fileNewPath);
            }
        }
        /// <summary>
        /// 复制文件方法(只能在同个盘符进行操作)
        /// </summary>
        public static void CopyFile(string fileResourcePath, string fileTargetPath)
        {
            if (File.Exists(fileResourcePath))
            {
                //参数1：要复制的源文件路径，参数2：复制后的目标文件路径，参数3：是否覆盖相同文件名
                File.Copy(fileResourcePath, fileTargetPath, true);
            }
        }
    }
}
