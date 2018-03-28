using Microsoft.Win32;
using System;
using System.Data;
using System.Windows;

namespace CheckProject.dialogs
{
    /// <summary>
    /// OpenFile.xaml 的交互逻辑
    /// </summary>
    public partial class Add_Local : Window
    {
        //设置的时间和Z号码参数
        public string fileName { get; set; }
        public bool headFlag { get; set; }
        public bool flag { get; set; }//确认是否点击了提交按钮
        //无参构造函数
        public Add_Local()
        {
            InitializeComponent();
            this.fileName = "";
            this.headFlag = true;
            this.flag = false;
            this.ResizeMode = ResizeMode.CanMinimize;//禁用最大化按钮
        }
        /// <summary>
        /// 选择文件
        /// </summary>
        private void button2_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;//该值确定是否可以选择多个文件
            dialog.Title = "请选择文件";
            dialog.Filter = "excel表(*.xls,*.xlsx*)|*.xls;*.xlsx";
            if (dialog.ShowDialog() == true)
            {
                textBox1.Text = dialog.FileName;
            }
        }
        /// <summary>
        /// 提交保存数据
        /// </summary>
        private void button_Click(object sender, RoutedEventArgs e)
        {
            //判断输入框内容是否为空
            if (textBox1.Text.Equals(""))
            {
                MessageBox.Show("请选择文件!", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            //判断选择框内容是否为空
            else if (comboBox1.SelectionBoxItem.ToString() == "")
            {
                MessageBox.Show("请选择下拉框内容!", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            //不为空时
            else
            {
                this.fileName = textBox1.Text;
                if (comboBox1.SelectionBoxItem.ToString().Equals("是"))
                {
                    this.headFlag = true;
                }else if (comboBox1.SelectionBoxItem.ToString().Equals("否"))
                {
                    this.headFlag = false;
                }
                this.flag = true;
                this.Close();
            }
        }
    }
}
