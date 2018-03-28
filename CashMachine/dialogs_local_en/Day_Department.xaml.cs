using CashMachine.SQLiteDB;
using System;
using System.Data;
using System.Windows;

namespace CashMachine.dialogs_local_en
{
    /// <summary>
    /// Day_Department.xaml 的交互逻辑
    /// </summary>
    public partial class Day_Department : Window
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
        public Day_Department()
        {
            InitializeComponent();
            this.Date_Time = "";
            this.Date_TimeStart = 0;
            this.Date_TimeEnd = 0;
            this.Znumber = "";
            this.ResizeMode = ResizeMode.CanMinimize;//禁用最大化按钮
            Init(); //窗体数据初始化
        }
        //有参构造函数
        public Day_Department(string dataBase)
        {
            this.dataBase = dataBase;//赋值
            InitializeComponent();
            this.Date_Time = "";
            this.Date_TimeStart = 0;
            this.Date_TimeEnd = 0;
            this.Znumber = "";
            this.ResizeMode = ResizeMode.CanMinimize;//禁用最大化按钮
            Init(); //窗体数据初始化
        }
        /// <summary>
        /// 窗体数据初始化
        /// </summary>
        public void Init()
        {
            //设置Z号码 自动生成的
            //SQLite数据库，此处连接的是
            sqliteDBHelper = new SQLiteDBHelper(dataBase);
            //SQL语句
            string sql = "SELECT DISTINCT Z_Number FROM sales_item";
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
                    comboBox1.Items.Add(dt.Rows[i]["Z_Number"]);
                }
            }
        }
        /// <summary>
        /// 提交保存数据
        /// </summary>
        private void button_Click(object sender, RoutedEventArgs e)
        {
            //判断输入框内容是否为空
            if (DatePicker.Text.Equals(""))
            {
                MessageBox.Show("请选择日期!", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            //判断输入框内容是否为空
            else if (comboBox1.SelectionBoxItem.ToString() == "")
            {
                MessageBox.Show("Z号码值不可为空!", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            //不为空时
            else
            {
                //DatePicker.Text == 2017 / 06 / 27 星期二
                this.Date_Time = Convert.ToDateTime(DatePicker.Text).ToString("yyyy年MM月dd日");//因为windows系统日期格式设置显示了日期，会导致DatePicker显示方式与系统日历日期格式相同,在此需要转换和数据库一样的格式
                //将选择框的日期转换成时间戳 2017 / 07 / 22 星期二 -> 1500652800
                this.Date_TimeStart = ((DatePicker.SelectedDate.Value.ToUniversalTime().Ticks - new DateTime(1970, 1, 1).Ticks) / 10000000);//一天中的00:00:00
                this.Date_TimeEnd = this.Date_TimeStart + 86399;////一天中的23:59:59
                Console.WriteLine(this.Date_TimeStart + "  " + this.Date_TimeEnd);
                this.Znumber = comboBox1.SelectionBoxItem.ToString();
                this.Close();
            }
        }
    }
}
