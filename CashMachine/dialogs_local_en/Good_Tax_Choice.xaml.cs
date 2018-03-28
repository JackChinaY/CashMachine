using CashMachine.SQLiteDB;
using System;
using System.Windows;
using System.Data.SQLite;
using System.Data;

namespace CashMachine.dialogs_local_en
{
    /// <summary>
    /// Good_Tax_Choice.xaml 的交互逻辑
    /// </summary>
    public partial class Good_Tax_Choice : Window
    {
        //声明一个变量
        public string dataBase;
        //声明一个SQLite数据库
        SQLiteDBHelper sqliteDBHelper = null;
        public string Number { get; set; }
        public string Tax_Code { get; set; }
        //无参构造函数
        public Good_Tax_Choice()
        {
            InitializeComponent();   
            
        }
        //有参构造函数
        public Good_Tax_Choice(string dataBase)
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
            string sql = "SELECT Number AS value1,Tax_Code AS value2,Tax_Name AS value3,Tax_Rate AS value4 FROM Tax_Tariff ORDER BY Number ASC";
            //SQLite数据库，此处连接的是goodsDB
            sqliteDBHelper = new SQLiteDBHelper(dataBase);
            //执行查询，结果为DataTable类型
            DataTable dt = sqliteDBHelper.ExecuteDataTable(sql, null);
            //判断查询结果是否为0行
            if (dt.Rows.Count == 0)
            {
                dataGrid.ItemsSource = null;//先清空表格内容
                DataTable dt_temp = new DataTable();//新建临时表
                dt_temp.Columns.Add(new DataColumn("Tip"));//添加列
                dt_temp.Rows.Add(dt_temp.NewRow());//添加行
                dt_temp.Rows[0]["Tip"] = "No result！";//设置行内容
                dataGrid.ItemsSource = dt_temp.DefaultView;//把表格放到控件中
                return;
            }
            //查询出来的表格的字段名为英文，但显示给用户的时候要为中文，所以在此添加若干个中文字段名显示给用户
            dt.Columns.Add(new DataColumn("Number"));
            dt.Columns.Add(new DataColumn("Tax Code"));
            dt.Columns.Add(new DataColumn("Tax Name"));
            dt.Columns.Add(new DataColumn("Tax Rate"));
            //以下为给新添加的中文字段赋值
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dt.Rows[i]["Number"] = dt.Rows[i]["value1"];
                dt.Rows[i]["Tax Code"] = dt.Rows[i]["value2"];
                dt.Rows[i]["Tax Name"] = dt.Rows[i]["value3"];
                dt.Rows[i]["Tax Rate"] = (Convert.ToDouble(dt.Rows[i]["value4"]) / 100).ToString("f2") + "%";
            }
            //将结果集绑定到DataGrid
            dataGrid.ItemsSource = dt.DefaultView;
        }
        /// <summary>
        /// 元素即将要被渲染时触发，此函数用在表格所在对话框还没弹出的情况中使用，此函数在控件初始化后自动加载，获取对控件属性的控制
        /// </summary>
        private void dataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            //count是dt表格中前若干列标题是英文的字段数，也就是select查询的的字段数
            int count = 4;
            //将英文字段隐藏掉
            for (int i = 0; i < count; i++)
            {
                dataGrid.Columns[i].Visibility = System.Windows.Visibility.Hidden;
            }
            //以下为设置字段宽度
            this.dataGrid.Columns[count++].Width = 70;
            this.dataGrid.Columns[count++].Width = 80;
            this.dataGrid.Columns[count++].Width = 130;
            this.dataGrid.Columns[count++].Width = 100;
        }

        /// <summary>
        /// 提交
        /// </summary>
        private void button2_5_1_Click(object sender, RoutedEventArgs e)
        {
            //未选择一条数据时 弹出提示框
            if (dataGrid.SelectedItem == null)
            {
                MessageBox.Show("Please select one of the data!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            //选择了某一行数据时
            else
            {
                //获取选中行所有数据
                DataRowView mySelectedElement = (DataRowView)dataGrid.SelectedItem;
                //变量赋值
                this.Number = mySelectedElement.Row["Number"].ToString();
                this.Tax_Code = mySelectedElement.Row["Tax Code"].ToString();
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
