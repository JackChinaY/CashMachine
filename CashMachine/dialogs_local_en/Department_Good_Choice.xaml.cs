using CashMachine.entity_local;
using CashMachine.SQLiteDB;
using System;
using System.Windows;
using System.Data.SQLite;
using System.Data;

namespace CashMachine.dialogs_local_en
{
    /// <summary>
    /// Department_Good_Choice.xaml 的交互逻辑
    /// </summary>
    public partial class Department_Good_Choice : Window
    {
        //声明一个变量
        public string dataBase;
        //声明一个SQLite数据库
        SQLiteDBHelper sqliteDBHelper = null;
        public string Number { get; set; }
        public bool isHaveData = false;
        //无参构造函数
        public Department_Good_Choice()
        {
            InitializeComponent();

        }
        //有参构造函数
        public Department_Good_Choice(string dataBase)
        {
            this.dataBase = dataBase;//赋值
            InitializeComponent();//初始化窗体组件
            this.ResizeMode = ResizeMode.CanMinimize;//禁用“最大化”按钮
            Init();//初始化窗体里的数据
        }
        /// <summary>
        /// 窗体数据初始化
        /// </summary>
        public void Init()
        {
            //默认赋空值
            this.Number = string.Empty;//""

            //SQL查询语句
            string sql = "SELECT Number,Name,Barcode,Price FROM Goods_Info ORDER BY Number ASC";
            //SQLite数据库，此处连接的是goodsDB
            sqliteDBHelper = new SQLiteDBHelper(dataBase);
            //执行查询，结果为DataTable类型
            DataTable dt = sqliteDBHelper.ExecuteDataTable(sql, null);
            //判断查询结果是否为0行
            if (dt.Rows.Count == 0)
            {
                dataGrid.ItemsSource = null;//先清空表格内容
                DataTable dt_temp = new DataTable();//新建临时表
                dt_temp.Columns.Add(new DataColumn("No data！"));//添加列
                //dt_temp.Rows.Add(dt_temp.NewRow());//添加行
                //dt_temp.Rows[0]["Tip"] = "No result！";//设置行内容
                dataGrid.ItemsSource = dt_temp.DefaultView;//把表格放到控件中
                isHaveData = false;
                return;
            }
            //以下为给新添加的中文字段赋值
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dt.Rows[i]["Price"] = Convert.ToDouble(dt.Rows[i]["Price"]) / 100;
            }
            //将结果集绑定到DataGrid
            dataGrid.ItemsSource = dt.DefaultView;
            isHaveData = true;
        }
        /// <summary>
        /// 元素即将要被渲染时触发，此函数用在表格所在对话框还没弹出的情况中使用，此函数在控件初始化后自动加载，获取对控件属性的控制
        /// </summary>
        private void dataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if (isHaveData)
            {
                //count是dt表格中前若干列标题是英文的字段数，也就是select查询的的字段数
                int count = 0;
                //以下为设置字段宽度
                this.dataGrid.Columns[count++].Width = 60;
                this.dataGrid.Columns[count++].Width = 120;
                this.dataGrid.Columns[count++].Width = 110;
                this.dataGrid.Columns[count++].Width = 60;
            }
        }
        /// <summary>
        /// 提交
        /// </summary>
        private void button2_5_1_Click(object sender, RoutedEventArgs e)
        {
            //未选择一条数据时 弹出提示框
            if (dataGrid.SelectedItem == null)
            {
                MessageBox.Show("Please select one record first!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            //选择了某一行数据时
            else
            {
                //获取选中行所有数据
                DataRowView mySelectedElement = (DataRowView)dataGrid.SelectedItem;
                //变量赋值
                this.Number = mySelectedElement.Row["Number"].ToString();
                //关闭对话框
                this.Close();
            }
        }
        /// <summary>
        /// 取消
        /// </summary>
        private void button2_5_2_Click(object sender, RoutedEventArgs e)
        {
            //关闭对话框
            this.Close();
        }
    }
}
