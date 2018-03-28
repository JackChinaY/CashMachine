using System;
using System.Windows;

namespace CashMachine.dialogs_local_en
{
    /// <summary>
    /// Moment_Department.xaml 的交互逻辑
    /// </summary>
    public partial class Moment_Cancel : Window
    {
        //设置的时间和Z号码参数
        //public string Date_Time { get; set; }
        public string Date_TimeStart { get; set; }
        public string Date_TimeEnd { get; set; }
        //public string Znumber { get; set; }


        //无参构造函数
        public Moment_Cancel()
        {
            InitializeComponent();
            //this.Date_Time = "";
            this.Date_TimeStart = "";
            this.Date_TimeEnd = "";
            //this.Znumber = "";
            DatePicker2.DisplayDateEnd = DateTime.Now;
        }
        /// <summary>
        /// 提交保存数据
        /// </summary>
        private void button_Click(object sender, RoutedEventArgs e)
        {
            //判断输入框内容是否为空
            if (DatePicker1.Text.Equals("") || DatePicker2.Text.Equals(""))
            {
                MessageBox.Show("存在部分数据未填写完整!", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            //不为空时
            else
            {
                this.Date_TimeStart = Convert.ToDateTime(DatePicker1.Text).ToString("yyyy-MM-dd");
                this.Date_TimeEnd = Convert.ToDateTime(DatePicker2.Text).ToString("yyyy-MM-dd");//因为windows系统日期格式设置显示了日期，会导致DatePicker显示方式与系统日历日期格式相同,在此需要转换和数据库一样的格式
                //this.Znumber = textBox1.Text;
                this.Close();
            }
        }
    }
}
