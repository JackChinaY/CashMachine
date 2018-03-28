using CashMachine.entity_local;
using CashMachine.SQLiteDB;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CashMachine.dialogs_local_en
{
    /// <summary>
    /// Good_Selection.xaml 的交互逻辑
    /// </summary>
    public partial class Buyer_Query : Window
    {
        private string buyerDB;
        SQLiteDBHelper sqliteDBHelper_buyerDB = null;
        /////////////////////////////////////////////////////////////////////////////////
        public Buyer_Query(string buyerDB)
        {
            this.buyerDB = buyerDB;
            InitializeComponent();
            sqliteDBHelper_buyerDB = new SQLiteDBHelper(buyerDB);

            btnPageDown_buyer.IsEnabled = false;
            btnPageUp_buyer.IsEnabled = false;
            btnEndPage_buyer.IsEnabled = false;
            btnFirstPage_buyer.IsEnabled = false;
        }
        /////////////////////////////////////////////////////////////////////////////////
        #region part4_1 客户管理 本地数据库
        ///<summary>
        ///part4_1 客户管理 查询本地数据库
        ///</summary>
        private void button4_1_2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                pageIndex_buyer = 1;
                //查询
                button4_1_2_Click();
                //设置分页按钮是否可用
                SetPageButtonEnabled_buyer();
                //设置控件显示信息
                SetPagerInfo_buyer(pageIndex_buyer, pageSize_buyer, pageCount_buyer, totalCount_buyer);
            }
            catch (Exception ee)
            {
                MessageBox.Show("The operation failed, please try again. Possible causes:" + ee.Message, "Information", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private int pageIndex_buyer = 1;// 当前页码
        private int pageSize_buyer = 30;// 分页大小
        private int totalCount_buyer = 0;// 记录总数
        private int pageCount_buyer = 0;// 总页数

        ///<summary>
        ///part2_8 单品 查询本地数据库 带分页
        ///</summary>
        private void button4_1_2_Click()
        {
            btnFirstPage_buyer.IsEnabled = true;
            //查询条件
            string option = comboBox1.SelectionBoxItem.ToString();
            //查询内容
            string content = textBox4_1_1.Text;
            //按Number查询
            if (option.Equals("Number"))
            {
                #region
                if (!content.Equals(""))
                {
                    //SQL语句
                    string sql_count = "SELECT COUNT(*) AS COUNTS FROM Buyer_Info WHERE Number=@Number";
                    //配置SQL语句里的参数
                    SQLiteParameter[] parameters_count = {
                        new SQLiteParameter("@Number",content),
                     };
                    //执行查询，结果为DataTable类型
                    DataTable dt_count = sqliteDBHelper_buyerDB.ExecuteDataTable(sql_count, parameters_count);
                    // 记录总数
                    totalCount_buyer = Convert.ToInt32(dt_count.Rows[0]["COUNTS"]);
                    // 总页数
                    pageCount_buyer = (int)Math.Ceiling((double)totalCount_buyer / pageSize_buyer);
                    //SQL查询语句
                    string sql = "SELECT Number,Name,BPN AS TPIN,VAT AS VAT_ACC_Name,Address,Tel FROM "
                        + "(SELECT * FROM Buyer_Info WHERE Number=@Number ORDER BY Number ASC LIMIT @pageSize*@pageIndex) LIMIT @pageSize offset @pageSize*@pageIndexbefore;";
                    //配置SQL语句里的参数
                    SQLiteParameter[] parameters = {
                        new SQLiteParameter("@Number",content),
                    new SQLiteParameter("@pageSize",pageSize_buyer),
                    new SQLiteParameter("@pageIndex",pageIndex_buyer),
                    new SQLiteParameter("@pageIndexbefore",pageIndex_buyer - 1),
                        };
                    //执行查询，结果为DataTable类型
                    DataTable dt = sqliteDBHelper_buyerDB.ExecuteDataTable(sql, parameters);
                    //判断查询结果是否为0行
                    if (dt.Rows.Count == 0)
                    {
                        dataGrid4_1.ItemsSource = null;//先清空表格内容
                        DataTable dt_temp = new DataTable();//新建临时表
                        dt_temp.Columns.Add(new DataColumn("No data！"));//添加列
                        //dt_temp.Rows.Add(dt_temp.NewRow());//添加行
                        //dt_temp.Rows[0]["Tip"] = "No result！";//设置行内容
                        dataGrid4_1.ItemsSource = dt_temp.DefaultView;//把表格放到控件中
                        return;
                    }
                    dataGrid4_1.ItemsSource = dt.DefaultView;
                    //count从0开始
                    int count = 0;
                    //以下为设置字段宽度
                    this.dataGrid4_1.Columns[count++].Width = 70;
                    this.dataGrid4_1.Columns[count++].Width = 160;
                    this.dataGrid4_1.Columns[count++].Width = 160;
                    this.dataGrid4_1.Columns[count++].Width = 160;
                    this.dataGrid4_1.Columns[count++].Width = 200;
                    this.dataGrid4_1.Columns[count++].Width = 120;
                }
                else
                {
                    MessageBox.Show("Please fill in the query textbox !", "Information", MessageBoxButton.OK, MessageBoxImage.Warning);

                }
                #endregion
            }
            else if (option.Equals("Name"))
            {
                #region
                if (!content.Equals(""))
                {
                    //SQL语句
                    string sql_count = "SELECT COUNT(*) AS COUNTS FROM Buyer_Info WHERE Name LIKE @Name";
                    //配置SQL语句里的参数
                    SQLiteParameter[] parameters_count = {
                        new SQLiteParameter("@Name","%" +content +"%"),
                     };
                    //执行查询，结果为DataTable类型
                    DataTable dt_count = sqliteDBHelper_buyerDB.ExecuteDataTable(sql_count, parameters_count);
                    // 记录总数
                    totalCount_buyer = Convert.ToInt32(dt_count.Rows[0]["COUNTS"]);
                    // 总页数
                    pageCount_buyer = (int)Math.Ceiling((double)totalCount_buyer / pageSize_buyer);
                    //SQL查询语句
                    string sql = "SELECT Number,Name,BPN AS TPIN,VAT AS VAT_ACC_Name,Address,Tel FROM "
                        + "(SELECT * FROM Buyer_Info WHERE Name LIKE @Name ORDER BY Number ASC LIMIT @pageSize*@pageIndex) LIMIT @pageSize offset @pageSize*@pageIndexbefore;";
                    //配置SQL语句里的参数
                    SQLiteParameter[] parameters = {
                        new SQLiteParameter("@Name","%" +content + "%"),
                        new SQLiteParameter("@pageSize",pageSize_buyer),
                        new SQLiteParameter("@pageIndex",pageIndex_buyer),
                        new SQLiteParameter("@pageIndexbefore",pageIndex_buyer - 1),
                        };
                    //执行查询，结果为DataTable类型
                    DataTable dt = sqliteDBHelper_buyerDB.ExecuteDataTable(sql, parameters);
                    //判断查询结果是否为0行
                    if (dt.Rows.Count == 0)
                    {
                        dataGrid4_1.ItemsSource = null;//先清空表格内容
                        DataTable dt_temp = new DataTable();//新建临时表
                        dt_temp.Columns.Add(new DataColumn("No data！"));//添加列
                        //dt_temp.Rows.Add(dt_temp.NewRow());//添加行
                        //dt_temp.Rows[0]["Tip"] = "No result！";//设置行内容
                        dataGrid4_1.ItemsSource = dt_temp.DefaultView;//把表格放到控件中
                        return;
                    }
                    dataGrid4_1.ItemsSource = dt.DefaultView;
                    //count从0开始
                    int count = 0;
                    //以下为设置字段宽度
                    this.dataGrid4_1.Columns[count++].Width = 70;
                    this.dataGrid4_1.Columns[count++].Width = 160;
                    this.dataGrid4_1.Columns[count++].Width = 160;
                    this.dataGrid4_1.Columns[count++].Width = 160;
                    this.dataGrid4_1.Columns[count++].Width = 200;
                    this.dataGrid4_1.Columns[count++].Width = 120;
                }
                else
                {
                    MessageBox.Show("Please fill in the query textbox !", "Information", MessageBoxButton.OK, MessageBoxImage.Warning);

                }
                #endregion
            }
            else if (option.Equals("TPIN"))
            {
                #region
                if (!content.Equals(""))
                {
                    //SQL语句
                    string sql_count = "SELECT COUNT(*) AS COUNTS FROM Buyer_Info WHERE BPN=@BPN";
                    //配置SQL语句里的参数
                    SQLiteParameter[] parameters_count = {
                        new SQLiteParameter("@BPN",content),
                     };
                    //执行查询，结果为DataTable类型
                    DataTable dt_count = sqliteDBHelper_buyerDB.ExecuteDataTable(sql_count, parameters_count);
                    // 记录总数
                    totalCount_buyer = Convert.ToInt32(dt_count.Rows[0]["COUNTS"]);
                    // 总页数
                    pageCount_buyer = (int)Math.Ceiling((double)totalCount_buyer / pageSize_buyer);
                    //SQL查询语句
                    string sql = "SELECT Number,Name,BPN AS TPIN,VAT AS VAT_ACC_Name,Address,Tel FROM "
                        + "(SELECT * FROM Buyer_Info WHERE BPN=@BPN ORDER BY Number ASC LIMIT @pageSize*@pageIndex) LIMIT @pageSize offset @pageSize*@pageIndexbefore;";
                    //配置SQL语句里的参数
                    SQLiteParameter[] parameters = {
                        new SQLiteParameter("@BPN",content),
                        new SQLiteParameter("@pageSize",pageSize_buyer),
                        new SQLiteParameter("@pageIndex",pageIndex_buyer),
                        new SQLiteParameter("@pageIndexbefore",pageIndex_buyer - 1),
                        };
                    //执行查询，结果为DataTable类型
                    DataTable dt = sqliteDBHelper_buyerDB.ExecuteDataTable(sql, parameters);
                    //判断查询结果是否为0行
                    if (dt.Rows.Count == 0)
                    {
                        dataGrid4_1.ItemsSource = null;//先清空表格内容
                        DataTable dt_temp = new DataTable();//新建临时表
                        dt_temp.Columns.Add(new DataColumn("No data！"));//添加列
                        //dt_temp.Rows.Add(dt_temp.NewRow());//添加行
                        //dt_temp.Rows[0]["Tip"] = "No result！";//设置行内容
                        dataGrid4_1.ItemsSource = dt_temp.DefaultView;//把表格放到控件中
                        return;
                    }
                    dataGrid4_1.ItemsSource = dt.DefaultView;
                    //count从0开始
                    int count = 0;
                    //以下为设置字段宽度
                    this.dataGrid4_1.Columns[count++].Width = 70;
                    this.dataGrid4_1.Columns[count++].Width = 160;
                    this.dataGrid4_1.Columns[count++].Width = 160;
                    this.dataGrid4_1.Columns[count++].Width = 160;
                    this.dataGrid4_1.Columns[count++].Width = 200;
                    this.dataGrid4_1.Columns[count++].Width = 120;
                }
                else
                {
                    MessageBox.Show("Please fill in the query textbox !", "Information", MessageBoxButton.OK, MessageBoxImage.Warning);

                }
                #endregion
            }
        }

        /// <summary>
        /// 设置分页按钮是否可用
        /// </summary>
        private void SetPageButtonEnabled_buyer()
        {
            //确定分页按钮的是否可用
            if (pageCount_buyer <= 1)
            {
                btnPageDown_buyer.IsEnabled = false;
                btnPageUp_buyer.IsEnabled = false;
                btnEndPage_buyer.IsEnabled = false;
            }
            else
            {
                if (pageIndex_buyer == pageCount_buyer)
                {
                    btnPageDown_buyer.IsEnabled = false;
                    btnPageUp_buyer.IsEnabled = true;
                    btnEndPage_buyer.IsEnabled = false;
                }
                else if (pageIndex_buyer <= 1)
                {
                    btnPageDown_buyer.IsEnabled = true;
                    btnPageUp_buyer.IsEnabled = false;
                    btnEndPage_buyer.IsEnabled = true;
                }
                else
                {
                    btnPageDown_buyer.IsEnabled = true;
                    btnPageUp_buyer.IsEnabled = true;
                    btnEndPage_buyer.IsEnabled = true;
                }
            }
        }
        /// <summary>
        /// 设置控件显示信息
        /// </summary>
        /// <param name="pageIndex">当前页码</param>
        /// <param name="pageCount">共有页数</param>
        /// <param name="totalCount">总记录条数</param>
        private void SetPagerInfo_buyer(int pageIndex, int pageSize, int pageCount, int totalCount)
        {
            //txtPagerInfo_buyer.Text = String.Format("The current page is【{0}】, each page shows【{1}】records, a total of【{2}】page, a total of【{3}】records. ", pageIndex_buyer, pageSize_buyer, pageCount_buyer, totalCount_buyer);
            txtPagerInfo_buyer.Text = String.Format("PER PAGE: {0}   ALL: {1}   PAGE: {2} / {3} ", pageSize_buyer, totalCount_buyer, pageIndex_buyer, pageCount_buyer);
        }
        /// <summary>
        /// 首页按钮事件
        /// </summary>
        private void btnFirstPage_buyer_Click(object sender, RoutedEventArgs e)
        {
            pageIndex_buyer = 1;
            button4_1_2_Click();//查询
            SetPageButtonEnabled_buyer();//设置分页按钮是否可用
            SetPagerInfo_buyer(pageIndex_buyer, pageSize_buyer, pageCount_buyer, totalCount_buyer);//设置控件显示信息
        }
        /// <summary>
        /// 下一页按钮事件
        /// </summary>
        private void btnPageDown_buyer_Click(object sender, RoutedEventArgs e)
        {
            pageIndex_buyer++;
            button4_1_2_Click();//查询
            SetPageButtonEnabled_buyer();//设置分页按钮是否可用
            SetPagerInfo_buyer(pageIndex_buyer, pageSize_buyer, pageCount_buyer, totalCount_buyer);//设置控件显示信息
        }
        /// <summary>
        /// 上一页按钮事件
        /// </summary>
        private void btnPageUp_buyer_Click(object sender, RoutedEventArgs e)
        {
            pageIndex_buyer--;
            button4_1_2_Click();//查询
            SetPageButtonEnabled_buyer();//设置分页按钮是否可用
            SetPagerInfo_buyer(pageIndex_buyer, pageSize_buyer, pageCount_buyer, totalCount_buyer);//设置控件显示信息
        }
        /// <summary>
        /// 尾页按钮事件
        /// </summary>
        private void btnEndPage_buyer_Click(object sender, RoutedEventArgs e)
        {
            pageIndex_buyer = pageCount_buyer;
            button4_1_2_Click();//查询
            SetPageButtonEnabled_buyer();//设置分页按钮是否可用
            SetPagerInfo_buyer(pageIndex_buyer, pageSize_buyer, pageCount_buyer, totalCount_buyer);//设置控件显示信息
        }
        ///<summary>
        ///part4_1 客户管理 删除
        ///</summary>
        private void button4_1_3_Click(object sender, RoutedEventArgs e)
        {
            //弹出提示框，未选择要删除的数据
            if (dataGrid4_1.SelectedItem == null)
            {
                MessageBox.Show("Please select one record to delete first!", "Information", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            //选择了某一行数据时
            else
            {
                //获取选中行所有数据
                DataRowView mySelectedElement = (DataRowView)dataGrid4_1.SelectedItem;
                //弹出确认框，当为确定时
                if (MessageBox.Show("Confirm that you want to delete the record ( the Number： " + mySelectedElement.Row["Number"].ToString() + " )？", "Information", MessageBoxButton.OKCancel, MessageBoxImage.Information) == MessageBoxResult.OK)
                {
                    //获取必要信息
                    string Number = mySelectedElement.Row["Number"].ToString();
                    //SQL语句
                    string sql = "DELETE FROM Buyer_Info WHERE Number=@Number";
                    //配置SQL语句里的参数
                    SQLiteParameter[] parameters = {
                    new SQLiteParameter("@Number",Number)
                    };
                    //执行SQL，并做判断
                    if (sqliteDBHelper_buyerDB.ExecuteNonQuery(sql, parameters) == 1)
                    {
                        //刷新查询框内容
                        button4_1_2_Click();
                        //设置分页按钮是否可用
                        SetPageButtonEnabled_buyer();
                        //设置控件显示信息
                        SetPagerInfo_buyer(pageIndex_buyer, pageSize_buyer, pageCount_buyer, totalCount_buyer);
                    }
                    else
                    {
                        MessageBox.Show("Delete failed!", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
        ///<summary>
        ///part4_1 客户管理 修改本地数据库
        ///</summary>
        private void button4_1_4_Click(object sender, RoutedEventArgs e)
        {
            //弹出提示框，未选择要删除的数据
            if (dataGrid4_1.SelectedItem == null)
            {
                MessageBox.Show("Please select one of the data you want to modify!", "Information", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            //选择了某一行数据时
            else
            {
                //获取选中行所有数据
                DataRowView mySelectedElement = (DataRowView)dataGrid4_1.SelectedItem;
                //声明一个变量
                Buyer buyer = new Buyer();
                //变量赋值
                buyer.Number = mySelectedElement.Row["Number"].ToString();
                buyer.Name = mySelectedElement.Row["Name"].ToString();
                buyer.BPN = mySelectedElement.Row["TPIN"].ToString();
                buyer.Address = mySelectedElement.Row["Address"].ToString();
                buyer.Tel = mySelectedElement.Row["Tel"].ToString();
                buyer.VAT = mySelectedElement.Row["VAT_ACC_Name"].ToString();
                //声明一个修改信息的窗体
                Update_Buyer upd_buyer = new Update_Buyer(buyerDB, buyer);
                //弹出窗体
                upd_buyer.ShowDialog();
                //刷新查询框内容
                button4_1_2_Click();
                //设置分页按钮是否可用
                SetPageButtonEnabled_buyer();
                //设置控件显示信息
                SetPagerInfo_buyer(pageIndex_buyer, pageSize_buyer, pageCount_buyer, totalCount_buyer);
            }
        }
        /// <summary>
        /// 动态改变表格大小 客户表
        /// </summary>
        private void gird4_1_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //Grid grid = (Grid)sender;
            dataGrid4_1.MaxHeight = gird4_1.ActualHeight;
            dataGrid4_1.MaxWidth = gird4_1.ActualWidth;
        }
        #endregion
        /////////////////////////////////////////////////////////////////////////////////
    }
}
