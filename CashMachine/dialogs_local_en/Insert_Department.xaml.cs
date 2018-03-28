using CashMachine.entity_local;
using CashMachine.SQLiteDB;
using System;
using System.Windows;
using System.Data.SQLite;

namespace CashMachine.dialogs_local_en
{
    /// <summary>
    /// Insert_Department.xaml 的交互逻辑
    /// </summary>
    public partial class Insert_Department : Window
    {
        //声明一个变量
        public string dataBase;
        //声明一个SQLite数据库
        SQLiteDBHelper sqliteDBHelper = null;
        public string Number { get; set; }
        //无参构造函数
        public Insert_Department()
        {
            InitializeComponent();
        }

        //有参构造函数
        public Insert_Department(string dataBase)
        {
            this.dataBase = dataBase;//赋值
            InitializeComponent();//初始化窗体组件
            this.ResizeMode = ResizeMode.CanMinimize;//只能最小化和还原窗口。 同时显示“最小化”和“最大化”按钮，但只有“最小化”按钮处于启用状态
        }

        /// <summary>
        /// 浏览按钮
        /// </summary>
        private void button2_5_2_Click(object sender, RoutedEventArgs e)
        {
            //声明一个添加信息的窗体
            Department_Good_Choice choice = new Department_Good_Choice(this.dataBase);
            //显示对话框
            choice.ShowDialog();
            //商品编号对话框内容赋值
            if (!choice.Number.Equals(string.Empty))
            {
                textBox2_5_2.Text = choice.Number;
            }
        }
        /// <summary>
        /// 提交保存数据
        /// </summary>
        private void button2_5_1_Click(object sender, RoutedEventArgs e)
        {
            //判断输入框内容是否为空
            if (textBox2_5_1.Text.Equals("") || textBox2_5_2.Text.Equals(""))
            {
                MessageBox.Show("存在部分数据未填写完整!", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            //不为空时
            else
            {
                //声明一个变量
                Department department = new Department();
                //变量赋值
                department.Dept_No = textBox2_5_1.Text;
                department.PLU_No = textBox2_5_2.Text;

                //Console.WriteLine(department.ToString());
                //SQL语句
                string sql = "INSERT INTO Department_Associate (Dept_No,PLU_No) VALUES(@Dept_No,@PLU_No)";
                //配置SQL语句里的参数
                SQLiteParameter[] parameters = {
                    new SQLiteParameter("@Dept_No",department.Dept_No),
                    new SQLiteParameter("@PLU_No",department.PLU_No),
                };
                //SQLite数据库，此处连接的是goodsDB
                sqliteDBHelper = new SQLiteDBHelper(dataBase);
                try
                {
                    //执行SQL
                    if (sqliteDBHelper.ExecuteNonQuery(sql, parameters) == 1)//执行成功时
                    {
                        //弹出提示框
                        MessageBox.Show("数据提交至本地数据库成功!", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("数据提交失败!", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                //执行失败时
                catch (Exception ee)
                {
                    //弹出提示框
                    MessageBox.Show("数据提交失败!可能原因：" + ee.Message, "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
