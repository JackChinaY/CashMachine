using System;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Controls;

namespace CheckProject.utils
{
    class CommonUtils
    {
        /// <summary>
        /// dataGrid转换dataTable
        /// </summary>
        public DataTable dataGridToDataTable(DataGrid dataGrid)
        {
            try
            {
                DataTable dt = null;

                if (dataGrid.ItemsSource is DataView)
                {
                    dt = (dataGrid.ItemsSource as DataView).Table;
                }
                else if (dataGrid.ItemsSource is DataTable)
                {
                    dt = dataGrid.ItemsSource as DataTable;
                }
                else if (dataGrid.ItemsSource is DataSet)
                {
                    dt = (dataGrid.ItemsSource as DataSet).Tables[0];
                }
                return dt;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 对字符串进行MD5加密，返回值为字符串
        /// </summary>
        public static string getMD5Str(string ConvertString)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] result = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(ConvertString));

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < result.Length; i++)
            {
                sb.Append(result[i].ToString("x2")); //数值的16进制表示,X后跟数字表示用几位表示
            }
            return sb.ToString().ToLower();
        }

        //DateTime.Now

        /// <summary>  
        /// 时间戳转日期 时间戳单位为秒 1498636046 to 2017/06/28 星期三 15:47:26
        /// </summary>
        public static DateTime LongToDateTime(long timestamp)
        {
            return TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1)).AddSeconds(timestamp);
        }
        /// <summary>  
        /// 日期转时间戳 时间戳单位为秒 2017/06/28 星期三 15:47:26 to 1498636046
        /// </summary>
        public static long DateTimeToLong(DateTime datetime)
        {
            return ((datetime.ToUniversalTime().Ticks - new DateTime(1970, 1, 1).Ticks) / 10000000);
        }
    }
}
