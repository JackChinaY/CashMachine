using System;
using System.Windows;

namespace CashMachine.dialogs_local_en
{
    /// <summary>
    /// Day_Moment.xaml 的交互逻辑
    /// </summary>
    public partial class Day_Moment : Window
    {
        //设置的时间和Z号码参数
        public string Date_Time { get; set; }
        public long Date_TimeStart { get; set; }
        public long Date_TimeEnd { get; set; }

        //无参构造函数
        public Day_Moment()
        {
            InitializeComponent();
            this.Date_Time = "";
            this.Date_TimeStart = 0;
            this.Date_TimeEnd = 0;
            this.ResizeMode = ResizeMode.CanMinimize;//禁用最大化按钮
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
            //不为空时
            else
            {
                //DatePicker.Text = 2017 / 06 / 27 星期二
                this.Date_Time = Convert.ToDateTime(DatePicker.Text).ToString("yyyy年MM月dd日");//因为windows系统日期格式设置显示了日期，会导致DatePicker显示方式与系统日历日期格式相同,在此需要转换和数据库一样的格式
                //将选择框的日期转换成时间戳 2017 / 07 / 22 星期二 -> 1500652800
                this.Date_TimeStart = ((DatePicker.SelectedDate.Value.ToUniversalTime().Ticks - new DateTime(1970, 1, 1).Ticks) / 10000000);//一天中的00:00:00
                this.Date_TimeEnd = this.Date_TimeStart + 86399;////一天中的23:59:59
                Console.WriteLine(this.Date_TimeStart + "  " + this.Date_TimeEnd);
                this.Close();
            }
        }
    }
}
