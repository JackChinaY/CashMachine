using CashMachine.SQLiteDB;
using System;
using System.Data;
using System.Windows;

namespace CashMachine.dialogs_local_en
{
    /// <summary>
    /// Month_Department.xaml 的交互逻辑
    /// </summary>
    public partial class Month_Department : Window
    {
        //声明一个变量
        public string dataBase;
        //声明一个SQLite数据库
        SQLiteDBHelper sqliteDBHelper = null;
        //设置的时间和Z号码参数
        public string Date_Time { get; set; }
        public long Date_TimeStart { get; set; }
        public long Date_TimeEnd { get; set; }
        public string Znumber { get; set; }


        //无参构造函数
        public Month_Department()
        {
            InitializeComponent();
            this.Date_Time = "";
            this.Date_TimeStart = 0;
            this.Date_TimeEnd = 0;
            this.Znumber = "";
            Init(); //窗体数据初始化
        }
        //有参构造函数
        public Month_Department(string dataBase)
        {
            this.dataBase = dataBase;//赋值
            InitializeComponent();
            this.Date_Time = "";
            this.Date_TimeStart = 0;
            this.Date_TimeEnd = 0;
            this.Znumber = "";
            Init(); //窗体数据初始化
        }
        /// <summary>
        /// 窗体数据初始化
        /// </summary>
        public void Init()
        {
            //生成年的下拉框选项
            for (int i = 0; i < 84; i++)
            {
                comboBox1.Items.Add(2017 + i);
            }

            //设置Z号码 自动生成的
            //SQLite数据库，此处连接的是
            sqliteDBHelper = new SQLiteDBHelper(dataBase);
            //SQL语句
            string sql = "select DISTINCT Z_Number from sales_item";
            //执行查询，结果为DataTable类型
            DataTable dt = sqliteDBHelper.ExecuteDataTable(sql, null);
            //判断结果
            if (dt.Rows.Count == 0)//若为空表
            {
                //弹出提示框
                MessageBox.Show("当前没有Z号码值！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    comboBox3.Items.Add(dt.Rows[i]["Z_Number"]);
                }
            }
        }
        /// <summary>
        /// 提交保存数据
        /// </summary>
        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (comboBox3.SelectionBoxItem.ToString() == "")
            {
                MessageBox.Show("Z号码值不可为空!", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            //不为空时
            else
            {
                //获取时间 2017 / 06 
                this.Date_Time = comboBox1.SelectionBoxItem.ToString() + "年" + comboBox2.SelectionBoxItem.ToString() + "月";//因为windows系统日期格式设置显示了日期，会导致DatePicker显示方式与系统日历日期格式相同,在此需要转换和数据库一样的格式
                //将选择框的日期转换成时间戳 2017 / 07 / 22 星期二  1500652800
                this.Date_TimeStart = ((Convert.ToDateTime(comboBox1.SelectionBoxItem.ToString() + "/" + comboBox2.SelectionBoxItem.ToString() + "/01").ToUniversalTime().Ticks - new DateTime(1970, 1, 1).Ticks) / 10000000);//一天中的00:00:00
                //判断下一个月
                //如果是12月份就把年份加1
                if (comboBox2.SelectionBoxItem.ToString() == "12")
                {
                    string year = (Convert.ToInt32(comboBox1.SelectionBoxItem.ToString()) + 1).ToString();
                    this.Date_TimeEnd = ((Convert.ToDateTime(year + "/01/01").ToUniversalTime().Ticks - new DateTime(1970, 1, 1).Ticks) / 10000000) - 1;//一天中的00:00:00
                }
                //如果不是12月份就把月份加1
                else
                {
                    string month = (Convert.ToInt32(comboBox2.SelectionBoxItem.ToString()) + 1).ToString();
                    this.Date_TimeEnd = ((Convert.ToDateTime(comboBox1.SelectionBoxItem.ToString() + "/" + month + "/01").ToUniversalTime().Ticks - new DateTime(1970, 1, 1).Ticks) / 10000000) - 1;//一天中的00:00:00
                }
                //Console.WriteLine(this.Date_TimeStart + "  " + this.Date_TimeEnd);
                this.Znumber = comboBox3.SelectionBoxItem.ToString();
                this.Close();
            }
        }
    }
}
