using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Windows;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            //1503014881 2017-08-18 08:08:01          UTC     1503014881          local     1503072481
            Console.WriteLine(DateTime.UtcNow.Ticks/ 10000000);
            Console.WriteLine((DateTime.UtcNow.Ticks - new DateTime(1970, 1, 1).Ticks) / 10000000);//2017-07-29 09:10:42.729 to 1454954944);root/sysm/
            //Console.WriteLine(TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1)).AddSeconds(Convert.ToDouble(1503014881)).ToString("yyyy-MM-dd HH:mm:ss"));
            //时间戳转成长时间123456783333 -> 2017-07-29 09:10:42.729
            //IsDirectoryExists("database\\database.old");
            //MoveFile("database\\11.txt" , "database\\database.old\\11.txt");
            //    IPHostEntry ipe = Dns.GetHostEntry(Dns.GetHostName());
            //    IPAddress[] localIpGroup = ipe.AddressList;
            //    //char[] a = new char[10];
            //    foreach (var item in localIpGroup)
            //    {
            //        Console.WriteLine(item.ToString());
            //    }
            //    Console.WriteLine(localIpGroup.Length);
            //    //Console.WriteLine();
            //    //Console.WriteLine();
            Console.ReadKey();
            //}
            //private static void MoveFile(string fileOldPath, string fileNewPath)
            //{
            //    if (File.Exists(fileOldPath))
            //    {
            //        //参数1：要移动的源文件路径，参数2：移动后的目标文件路径
            //        File.Move(fileOldPath, fileNewPath);
            //        MessageBox.Show("移动文件成功！");
            //    }
            //    else
            //    {
            //        MessageBox.Show("文件不存在！");
            //    }
            //}
            //public static void IsDirectoryExists(string filePath)
            //{
            //    if (!Directory.Exists(filePath))
            //    {
            //        Directory.CreateDirectory(filePath);
            //    }
        }
    }
}
