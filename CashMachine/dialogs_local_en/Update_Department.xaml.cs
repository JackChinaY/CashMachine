using CashMachine.entity_local;
using CashMachine.SQLiteDB;
using System;
using System.Windows;
using System.Data.SQLite;

namespace CashMachine.dialogs_local_en
{
    /// <summary>
    /// Update_Department.xaml 的交互逻辑
    /// </summary>
    public partial class Update_Department : Window
    {
        //声明一个变量
        Department department = new Department();
        //声明一个变量
        public string dataBase;
        //声明一个SQLite数据库
        SQLiteDBHelper sqliteDBHelper = null;

        //无参构造函数
        public Update_Department()
        {
            InitializeComponent();
        }
       
        //有参构造函数
        public Update_Department(Department department, string dataBase)
        {
            this.department = department;//赋值
            this.dataBase = dataBase;//赋值
            InitializeComponent();//初始化窗体组件
            this.ResizeMode = ResizeMode.CanMinimize;//禁用“最大化”按钮
            Init();
        }

        /// <summary>
        /// 窗体数据初始化
        /// </summary>
        public void Init()
        {
            textBox2_5_1.Text = this.department.Dept_No;
            textBox2_5_2.Text = this.department.PLU_No;
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
            if (textBox2_5_2.Text.Equals(""))
            {
                MessageBox.Show("The Plu No is required!", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            //不为空时
            else
            {
                //声明一个变量
                Department department = new Department();
                //变量赋值
                department.Id = this.department.Id;
                department.Dept_No = textBox2_5_1.Text;
                department.PLU_No = textBox2_5_2.Text;
                //Console.WriteLine(department.ToString());
                //SQL语句
                string sql = "UPDATE Department_Associate SET Dept_No=@Dept_No,PLU_No=@PLU_No WHERE id=@id";
                //配置SQL语句里的参数
                SQLiteParameter[] parameters = {
                    new SQLiteParameter("@Dept_No",department.Dept_No),
                    new SQLiteParameter("@PLU_No",department.PLU_No),
                    new SQLiteParameter("@id",department.Id),
                };
                //SQLite数据库，此处连接的是goodsDB
                sqliteDBHelper = new SQLiteDBHelper(dataBase);
                try
                {
                    //执行SQL
                    if (sqliteDBHelper.ExecuteNonQuery(sql, parameters) == 1)//执行成功时
                    {
                        //弹出提示框
                        MessageBox.Show("Submit successfully!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                        //关闭弹出框
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Submission failure!", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                //执行失败时
                catch (Exception ee)
                {
                    //弹出提示框
                    MessageBox.Show("Submission failure! Possible causes:" + ee.Message, "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
