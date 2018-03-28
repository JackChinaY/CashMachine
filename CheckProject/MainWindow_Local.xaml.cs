using CheckProject.dialogs;
using CheckProject.entity;
using CheckProject.MysqlDB;
using CheckProject.utils;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace CheckProject
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow_Local : Window
    {
        /////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 公共变量
        /// </summary>
        private string currentUserName = "李四";//当前用户名
        private string currentUserId = "cbb418cc-8520-459f-ab02-ae3516388eb5";  //当前用户名Id，软件发布的时候把该字符内容删除掉
        MysqlDBHelper mysqlDBHelper = new MysqlDBHelper();//声明一个Mysql数据库，连接的是云端的数据库
        string[] shi = new string[] { "办公室", "法规", "绩效办", "督查办", "货劳税处", "所得税处", "收入核算处", "纳税服务处", "征管处", "财务处", "人事处", "教育处", "监察室", "大国处", "进出口税处", "机关党办", "离退休干部处", "信息中心", "服务中心", "稽查局" };
        string[] xian = new string[] { "办公室", "人教科", "财务科", "监察室", "征管科", "信息中心", "法规科", "收入核算科", "一分局", "稽查局", "风险应对分局" };
        ExportAttibute exportAttibute = new ExportAttibute();//导出excel表的参数
        /////////////////////////////////////////////////////////////////////////////////
        #region 构造函数、最大化、还原 动态改变表格大小
        /// <summary>
        /// 构造函数
        /// </summary>
        public MainWindow_Local()
        {
            //移交单位时将这些注释解封
            ////获取当前时间
            //long nowTime = (DateTime.Now.ToLocalTime().ToUniversalTime().Ticks - new DateTime(1970, 1, 1).Ticks) / 10000000;//2017-07-29 09:10:42.729 to 1454954944
            //Console.WriteLine(DateTime.Now.ToLocalTime().ToString());
            ////起始时间
            //long frontTime = (Convert.ToDateTime("2017-09-21 00:00:00.000").ToUniversalTime().Ticks - new DateTime(1970, 1, 1).Ticks) / 10000000;//2017-09-21 00:00:00.000 to 1454954944
            ////如果使用当天距离设定的起始时间超过5天，就强制退出
            //if (nowTime - frontTime >= 86400 * 5) //86400 * 5
            //{
            //    MessageBox.Show("此开发测试版本已过期！","消息", MessageBoxButton.OK, MessageBoxImage.Error);
            //    this.Close();
            //}
            //else//正常使用情况下
            //{
            InitializeComponent();
            //comboBox1_1_1_DropDownClosed();
            //comboBox1_2_1_DropDownClosed();
            //comboBox1_3_1_DropDownClosed();
            //}
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        public MainWindow_Local(string userId, string userName)
        {
            //移交单位时将这些注释解封
            ////获取当前时间
            //long nowTime = (DateTime.Now.ToLocalTime().ToUniversalTime().Ticks - new DateTime(1970, 1, 1).Ticks) / 10000000;//2017-07-29 09:10:42.729 to 1454954944
            //Console.WriteLine(DateTime.Now.ToLocalTime().ToString());
            ////起始时间
            //long frontTime = (Convert.ToDateTime("2017-09-21 00:00:00.000").ToUniversalTime().Ticks - new DateTime(1970, 1, 1).Ticks) / 10000000;//2017-09-21 00:00:00.000 to 1454954944
            ////如果使用当天距离设定的起始时间超过5天，就强制退出
            //if (nowTime - frontTime >= 86400 * 5) //86400 * 5
            //{
            //    MessageBox.Show("此开发测试版本已过期！","消息", MessageBoxButton.OK, MessageBoxImage.Error);
            //    this.Close();
            //}
            //else//正常使用情况下
            //{
            InitializeComponent();
            //comboBox1_1_1_DropDownClosed();
            //comboBox1_2_1_DropDownClosed();
            //comboBox1_3_1_DropDownClosed();
            //}
        }
        /// <summary>
        /// 动态改变表格大小
        /// </summary>
        private void gird1_1_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            dataGrid1_1.MaxHeight = gird1_1.ActualHeight;
            dataGrid1_1.MaxWidth = gird1_1.ActualWidth;
        }
        ///// <summary>
        ///// 动态改变表格大小
        ///// </summary>
        //private void gird1_2_SizeChanged(object sender, SizeChangedEventArgs e)
        //{
        //    dataGrid1_2.MaxHeight = gird1_2.ActualHeight;
        //    dataGrid1_2.MaxWidth = gird1_2.ActualWidth;
        //}
        ///// <summary>
        ///// 动态改变表格大小
        ///// </summary>
        //private void gird1_3_SizeChanged(object sender, SizeChangedEventArgs e)
        //{
        //    dataGrid1_3.MaxHeight = gird1_3.ActualHeight;
        //    dataGrid1_3.MaxWidth = gird1_3.ActualWidth;
        //}
        #endregion
        /////////////////////////////////////////////////////////////////////////////////
        #region 本3个函数控制树形菜单和右侧内容对应一一显示
        /// <summary>
        /// 本3个函数控制树形菜单和右侧内容对应一一显示
        /// </summary>
        //只显示part1_1的内容，其他部分的内容隐藏掉
        private void TreeViewItem_1_1_Selected(object sender, RoutedEventArgs e)
        {
            Grid grid = null;
            for (int i = 0; i < part.Children.Count; i++)
            {
                grid = part.Children[i] as Grid;//获取子孩子
                grid.Visibility = Visibility.Hidden;//设置其可见性为隐藏
            }
            part1_1.Visibility = Visibility.Visible;
            //try
            //{
            //    //刷新查询框内容
            //    //button1_1_2_Click();
            //    ////设置分页按钮是否可用
            //    //SetPageButtonEnabled();
            //    ////设置控件显示信息
            //    //SetPagerInfo(pageIndex, pageSize, pageCount, totalCount);
            //}
            //catch (Exception ee)
            //{
            //    MessageBox.Show("操作失败，请重试；可能原因:" + ee.Message, "消息", MessageBoxButton.OK, MessageBoxImage.Error);
            //}
        }
        //只显示part1_2的内容，其他部分的内容隐藏掉
        //private void TreeViewItem_1_2_Selected(object sender, RoutedEventArgs e)
        //{
        //    Grid grid = null;
        //    for (int i = 0; i < part.Children.Count; i++)
        //    {
        //        grid = part.Children[i] as Grid;//获取子孩子
        //        grid.Visibility = Visibility.Hidden;//设置其可见性为隐藏
        //    }
        //    part1_2.Visibility = Visibility.Visible;
        //}
        ////只显示part1_3的内容，其他部分的内容隐藏掉
        //private void TreeViewItem_1_3_Selected(object sender, RoutedEventArgs e)
        //{
        //    Grid grid = null;
        //    for (int i = 0; i < part.Children.Count; i++)
        //    {
        //        grid = part.Children[i] as Grid;//获取子孩子
        //        grid.Visibility = Visibility.Hidden;//设置其可见性为隐藏
        //    }
        //    part1_3.Visibility = Visibility.Visible;
        //}
        #endregion
        /////////////////////////////////////////////////////////////////////////////////
        #region part1_1 机关共性
        ///<summary>
        ///part1_1 添加
        ///</summary>
        private void button1_1_1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //声明一个添加信息的窗体
                Add_Local ins_plu = new Add_Local();
                //弹出窗体
                ins_plu.ShowDialog();
                //刷新查询框内容
                //button1_1_2_Click();
            }
            catch (Exception ex)
            {
                MessageBox.Show("操作失败，请重试；可能原因:" + ex.Message, "消息", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        ///<summary>
        ///part1_1 刷新
        ///</summary>
        private void button1_1_2_Click(object sender, RoutedEventArgs e)
        {
            //try
            //{
            //    button1_1_2_Click();
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show("操作失败，请重试；可能原因:" + ex.Message, "消息", MessageBoxButton.OK, MessageBoxImage.Error);
            //}
            DataTable dt_temp = new DataTable();//新建临时表
            dt_temp.Columns.Add(new DataColumn("序号"));//添加列
            dt_temp.Columns.Add(new DataColumn("会议级别"));//添加列
            dt_temp.Columns.Add(new DataColumn("会议时间"));//添加列
            dt_temp.Columns.Add(new DataColumn("会议地点"));//添加列
            dt_temp.Columns.Add(new DataColumn("与会人员"));//添加列
            dt_temp.Columns.Add(new DataColumn("会议议题"));//添加列
            dt_temp.Columns.Add(new DataColumn("审核状态"));//添加列
            dt_temp.Columns.Add(new DataColumn("记录员"));//添加列
            dt_temp.Rows.Add(dt_temp.NewRow());//添加行
            dt_temp.Rows[0]["序号"] = "1";//设置行内容
            dt_temp.Rows[0]["会议级别"] = "院级";//设置行内容
            dt_temp.Rows[0]["会议时间"] = "2018/03/15 09:00";//设置行内容
            dt_temp.Rows[0]["会议地点"] = "XX学院学术报告厅";//设置行内容
            dt_temp.Rows[0]["与会人员"] = "XXX XXX XXX";//设置行内容
            dt_temp.Rows[0]["会议议题"] = "关于XXXXXXXXXX的会议";//设置行内容
            dt_temp.Rows[0]["审核状态"] = "待审核";//设置行内容
            dt_temp.Rows[0]["记录员"] = "XXX";//设置行内容
            dataGrid1_1.ItemsSource = dt_temp.DefaultView;//把表格放到控件中
        }
        /////<summary>
        /////part1_1 查询 无参
        /////</summary>
        //private void button1_1_2_Click()
        //{
        //    //SQL查询语句
        //    string sql = "SELECT Id,Number,Name,Barcode,Price,Tax_Index,Stock_Control,Stock_Amount FROM goods_info"
        //        + " ORDER BY Number ASC LIMIT 1,100";
        //    //配置SQL查询语句里的参数
        //    MySqlParameter[] parameters = {
        //            new MySqlParameter("@UserId",comboBox1_1_1.SelectionBoxItem.ToString()),
        //            new MySqlParameter("@UserId",comboBox1_1_2.SelectionBoxItem.ToString()),
        //    };
        //    //执行查询，结果为DataTable类型
        //    DataTable dt = mysqlDBHelper.ExecuteDataTable(sql, null);
        //    //判断查询结果是否为0行
        //    if (dt.Rows.Count == 0)
        //    {
        //        dataGrid1_1.ItemsSource = null;//先清空表格内容
        //        DataTable dt_temp = new DataTable();//新建临时表
        //        dt_temp.Columns.Add(new DataColumn("提示"));//添加列
        //        dt_temp.Rows.Add(dt_temp.NewRow());//添加行
        //        dt_temp.Rows[0]["提示"] = "无查询结果！";//设置行内容
        //        dataGrid1_1.ItemsSource = dt_temp.DefaultView;//把表格放到控件中
        //        return;
        //    }
        //    //查询出来的表格的字段名为英文，但显示给用户的时候要为中文，所以在此添加若干个中文字段名显示给用户
        //    dt.Columns.Add(new DataColumn("编号"));
        //    dt.Columns.Add(new DataColumn("名称"));
        //    dt.Columns.Add(new DataColumn("条形码"));
        //    dt.Columns.Add(new DataColumn("单价"));
        //    dt.Columns.Add(new DataColumn("税种索引"));
        //    dt.Columns.Add(new DataColumn("库存控制标志位"));
        //    dt.Columns.Add(new DataColumn("库存总量"));
        //    //以下为给新添加的中文字段赋值
        //    for (int i = 0; i < dt.Rows.Count; i++)
        //    {
        //        dt.Rows[i]["编号"] = dt.Rows[i]["Number"];
        //        dt.Rows[i]["名称"] = dt.Rows[i]["Name"];
        //        dt.Rows[i]["条形码"] = dt.Rows[i]["Barcode"];
        //        dt.Rows[i]["单价"] = Convert.ToDouble(dt.Rows[i]["Price"]) / 100;
        //        dt.Rows[i]["税种索引"] = dt.Rows[i]["Tax_Index"];
        //        dt.Rows[i]["库存控制标志位"] = dt.Rows[i]["Stock_Control"];
        //        dt.Rows[i]["库存总量"] = Convert.ToDouble(dt.Rows[i]["Stock_Amount"]) / 10000;
        //    }
        //    dataGrid1_1.ItemsSource = dt.DefaultView;
        //    //count是dt表格中前若干列标题是英文的字段数，也就是select查询的的字段数
        //    int count = 8;
        //    //将英文字段隐藏掉
        //    for (int i = 0; i < count; i++)
        //    {
        //        this.dataGrid1_1.Columns[i].Visibility = System.Windows.Visibility.Hidden;
        //    }
        //    //为导出excel表做准备
        //    exportAttibute.dataTable = dt;
        //    exportAttibute.fileName = comboBox1_1_1.SelectionBoxItem.ToString() + "_" + comboBox1_1_2.SelectionBoxItem.ToString();
        //    exportAttibute.offset = count;//count的值

        //    //设置前台显示的字段宽度
        //    this.dataGrid1_1.Columns[count++].Width = 100;
        //    this.dataGrid1_1.Columns[count++].Width = 160;
        //    this.dataGrid1_1.Columns[count++].Width = 160;
        //    this.dataGrid1_1.Columns[count++].Width = 80;
        //    this.dataGrid1_1.Columns[count++].Width = 80;
        //    this.dataGrid1_1.Columns[count++].Width = 120;
        //    this.dataGrid1_1.Columns[count++].Width = 100;
        //}
        /////<summary>
        /////part1_1 删除
        /////</summary>
        //private void button1_1_3_Click(object sender, RoutedEventArgs e)
        //{
        //    //弹出提示框，未选择要删除的数据
        //    if (dataGrid1_1.SelectedItem == null)
        //    {
        //        MessageBox.Show("请先选中一条数据！", "消息", MessageBoxButton.OK, MessageBoxImage.Warning);
        //        return;
        //    }
        //    //选择了某一行数据时
        //    else
        //    {
        //        //获取选中行所有数据
        //        DataRowView mySelectedElement = (DataRowView)dataGrid1_1.SelectedItem;
        //        //获取必要信息
        //        string Number = mySelectedElement.Row["Number"].ToString();
        //        //弹出确认框，当为确定时
        //        if (MessageBox.Show("确认要删除该条记录 ( 编号:  " + mySelectedElement.Row["Number"].ToString() + " )？", "消息", MessageBoxButton.OKCancel, MessageBoxImage.Information) == MessageBoxResult.OK)
        //        {
        //            //获取必要信息
        //            string Id = mySelectedElement.Row["Id"].ToString();
        //            string UserId = this.currentUserId;
        //            //SQL语句
        //            string sql = "DELETE FROM goods_info WHERE Id=@Id AND UserId=@UserId";
        //            //配置SQL语句里的参数
        //            MySqlParameter[] parameters = {
        //            new MySqlParameter("@Id",Id),
        //            new MySqlParameter("@UserId",UserId)
        //        };
        //            //执行SQL，并做判断
        //            if (mysqlDBHelper.ExecuteNonQuery(sql, parameters) == 1)
        //            {
        //                //刷新查询框内容
        //                button1_1_2_Click();
        //            }
        //            else
        //            {
        //                MessageBox.Show("删除失败!", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
        //            }
        //        }
        //    }
        //}
        /////<summary>
        /////part1_1 修改
        /////</summary>
        //private void button1_1_4_Click(object sender, RoutedEventArgs e)
        //{
        //    //弹出提示框，未选择要删除的数据
        //    if (dataGrid1_1.SelectedItem == null)
        //    {
        //        MessageBox.Show("Please select one of the data you want to modify!", "Information", MessageBoxButton.OK, MessageBoxImage.Warning);
        //        return;
        //    }
        //    //选择了某一行数据时
        //    else
        //    {
        //        //获取选中行所有数据
        //        DataRowView mySelectedElement = (DataRowView)dataGrid1_1.SelectedItem;
        //        //声明一个变量
        //        //Plu plu = new Plu();
        //        ////变量赋值
        //        //plu.Number = mySelectedElement.Row["Number"].ToString();
        //        //plu.Name = mySelectedElement.Row["Name"].ToString();
        //        //plu.Barcode = mySelectedElement.Row["Barcode"].ToString();
        //        //plu.Price = Convert.ToDouble(mySelectedElement.Row["Price"].ToString());
        //        //plu.RRP = Convert.ToDouble(mySelectedElement.Row["RRP"].ToString());
        //        //plu.Tax_Index = Convert.ToInt32(mySelectedElement.Row["Tax Index"].ToString());
        //        //plu.Stock_Control = Convert.ToInt32(mySelectedElement.Row["Stock Control"]);
        //        //plu.Stock_Amount = Convert.ToDouble(mySelectedElement.Row["Stock Amount"].ToString());
        //        //plu.Tax_Code = mySelectedElement.Row["Tax Code"].ToString();
        //        ////声明一个修改信息的窗体
        //        //Update_Plu upd_plu = new Update_Plu(plu, goodsDB, systemDB);
        //        //弹出窗体
        //        //upd_plu.ShowDialog();
        //        //刷新查询框内容
        //        button1_1_2_Click();
        //    }
        //}
        /////<summary>
        /////part1_1 按条件查询
        /////</summary>
        //private void button1_1_5_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        button1_1_2_Click();
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("操作失败，请重试；可能原因:" + ex.Message, "消息", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //}
        /////<summary>
        /////part1_1 导入excel表
        /////</summary>
        //private void button1_1_8_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        //弹出导入向导框
        //        OpenFile openfile = new OpenFile();
        //        openfile.ShowDialog();
        //        //当点击了提交按钮时
        //        if (openfile.flag == true)
        //        {
        //            DataTable dt = null;
        //            dt = OperationExcel.ExcelToDataTable(openfile.fileName, openfile.headFlag);//fileName是文件的全路径名，如E:\docment\a.xls
        //            dataGrid1_1.ItemsSource = dt.DefaultView;
        //            //为导出excel表做准备
        //            exportAttibute.dataTable = dt;
        //            exportAttibute.fileName = System.IO.Path.GetFileNameWithoutExtension(openfile.fileName);
        //            exportAttibute.offset = 0;//默认从0开始
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("操作失败，请重试；可能原因:" + ex.Message, "消息", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //}
        /////<summary>
        /////part1_1 导出至excel表
        /////</summary>
        //private void button1_1_6_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        //判断是否导出成功
        //        if (OperationExcel.ExportToExcel(exportAttibute.dataTable, "导出文件//" + exportAttibute.fileName + ".xls", exportAttibute.offset) == true)
        //        {
        //            MessageBox.Show("数据导出成功！点击“打开”按钮可查看导出的表格文件。", "消息", MessageBoxButton.OK, MessageBoxImage.Information);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("操作失败，请重试；可能原因:" + ex.Message, "消息", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //}
        /////<summary>
        /////part1_1 打开导出文件的文件夹
        /////</summary>
        //private void button1_1_7_Click(object sender, RoutedEventArgs e)
        //{
        //    //打开数据库文件夹
        //    string filePathAndName = "导出文件";
        //    //判断文件夹是否存在
        //    if (!Directory.Exists(filePathAndName))
        //    {
        //        Directory.CreateDirectory(filePathAndName);//不存在就创建
        //    }
        //    System.Diagnostics.Process.Start("explorer.exe", filePathAndName);
        //}
        ///// <summary>
        ///// 下拉框改变事件：根据市县显示对应部门
        ///// </summary>
        //private void comboBox1_1_1_DropDownClosed(object sender, EventArgs e)
        //{
        //    if (comboBox1_1_1.SelectionBoxItem.ToString().Equals("市"))
        //    {
        //        //先清除原来内容
        //        comboBox1_1_2.Items.Clear();
        //        //将新的内容添加到下拉框中
        //        foreach (string item in this.shi)
        //        {
        //            comboBox1_1_2.Items.Add(item);
        //        }
        //        comboBox1_1_2.SelectedIndex = 0;
        //    }
        //    else if (comboBox1_1_1.SelectionBoxItem.ToString().Equals("县"))
        //    {
        //        //先清除原来内容
        //        comboBox1_1_2.Items.Clear();
        //        //将新的内容添加到下拉框中
        //        foreach (string item in this.xian)
        //        {
        //            comboBox1_1_2.Items.Add(item);
        //        }
        //        comboBox1_1_2.SelectedIndex = 0;
        //    }
        //}
        ///// <summary>
        ///// 下拉框自动填充内容 无参
        ///// </summary>
        //private void comboBox1_1_1_DropDownClosed()
        //{
        //    //先清除原来内容
        //    comboBox1_1_2.Items.Clear();
        //    //将新的内容添加到下拉框中
        //    foreach (string item in shi)
        //    {
        //        comboBox1_1_2.Items.Add(item);
        //    }
        //    comboBox1_1_2.SelectedIndex = 0;
        //}
        #endregion
        /////////////////////////////////////////////////////////////////////////////////
        //#region part1_2 机关个性
        ///// <summary>
        ///// 下拉框改变事件：根据市县显示对应部门
        ///// </summary>
        //private void comboBox1_2_1_DropDownClosed(object sender, EventArgs e)
        //{
        //    if (comboBox1_2_1.SelectionBoxItem.ToString().Equals("市"))
        //    {
        //        //先清除原来内容
        //        comboBox1_2_2.Items.Clear();
        //        //将新的内容添加到下拉框中
        //        foreach (string item in this.shi)
        //        {
        //            comboBox1_2_2.Items.Add(item);
        //        }
        //        comboBox1_2_2.SelectedIndex = 0;
        //    }
        //    else if (comboBox1_2_1.SelectionBoxItem.ToString().Equals("县"))
        //    {
        //        //先清除原来内容
        //        comboBox1_2_2.Items.Clear();
        //        //将新的内容添加到下拉框中
        //        foreach (string item in this.xian)
        //        {
        //            comboBox1_2_2.Items.Add(item);
        //        }
        //        comboBox1_2_2.SelectedIndex = 0;
        //    }
        //}
        ///// <summary>
        ///// 下拉框自动填充内容 无参
        ///// </summary>
        //private void comboBox1_2_1_DropDownClosed()
        //{
        //    //先清除原来内容
        //    comboBox1_2_2.Items.Clear();
        //    //将新的内容添加到下拉框中
        //    foreach (string item in this.shi)
        //    {
        //        comboBox1_2_2.Items.Add(item);
        //    }
        //    comboBox1_2_2.SelectedIndex = 0;
        //}
        //#endregion
        /////////////////////////////////////////////////////////////////////////////////
        //#region part1_3 绩效考核
        ///// <summary>
        ///// 下拉框自动填充内容 无参
        ///// </summary>
        //private void comboBox1_3_1_DropDownClosed()
        //{
        //    //先清除原来内容
        //    comboBox1_3_1.Items.Clear();
        //    //将新的内容添加到下拉框中
        //    foreach (string item in this.shi)
        //    {
        //        comboBox1_3_1.Items.Add(item);
        //    }
        //    comboBox1_3_1.SelectedIndex = 0;
        //}
        //#endregion
        /////////////////////////////////////////////////////////////////////////////////
    }
}
