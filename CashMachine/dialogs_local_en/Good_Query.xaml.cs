using CashMachine.entity_local;
using CashMachine.SQLiteDB;
using System;
using System.Data;
using System.Data.SQLite;
using System.Windows;

namespace CashMachine.dialogs_local_en
{
    /// <summary>
    /// Good_Selection.xaml 的交互逻辑
    /// </summary>
    public partial class Good_Query : Window
    {
        private string goodsDB;
        private string systemDB;
        private string programmingDB;
        SQLiteDBHelper sqliteDBHelper_goodsDB = null;
        SQLiteDBHelper sqliteDBHelper_systemDB = null;
        SQLiteDBHelper sqliteDBHelper_programmingDB = null;
        /////////////////////////////////////////////////////////////////////////////////
        public Good_Query(string goodsDB, string systemDB, string programmingDB)
        {
            this.goodsDB = goodsDB;
            this.systemDB = systemDB;
            this.programmingDB = programmingDB;
            InitializeComponent();
            sqliteDBHelper_goodsDB = new SQLiteDBHelper(goodsDB);
            sqliteDBHelper_systemDB = new SQLiteDBHelper(systemDB);


            btnPageDown.IsEnabled = false;
            btnPageUp.IsEnabled = false;
            btnEndPage.IsEnabled = false;
            btnFirstPage.IsEnabled = false;
        }
        /////////////////////////////////////////////////////////////////////////////////
        #region part2_8 单品 本地数据库
        ///<summary>
        ///part2_8 单品 查询本地数据库
        ///</summary>
        private void button2_8_2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                pageIndex = 1;
                //查询 设置 pageCount totalCount ；显示数据
                button2_8_2_Click();
                //设置分页按钮是否可用
                SetPageButtonEnabled();
                //设置控件显示信息
                SetPagerInfo(pageIndex, pageSize, pageCount, totalCount);
            }
            catch (Exception ee)
            {
                MessageBox.Show("The operation failed, please try again. Possible causes:" + ee.Message, "Information", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private int pageIndex = 1;// 当前页码
        private int pageSize = 100;// 分页大小
        private int totalCount = 0;// 记录总数
        private int pageCount = 0;// 总页数

        ///<summary>
        ///part2_8 单品 查询本地数据库 带分页
        ///</summary>
        private void button2_8_2_Click()
        {
            btnFirstPage.IsEnabled = true;
            //查询条件
            string option = comboBox1.SelectionBoxItem.ToString();
            //查询内容
            string content = textBox2_8_1.Text;
            //按Number查询
            if (option.Equals("Number"))
            {
                #region
                if (!content.Equals(""))
                {
                    //SQL语句
                    string sql_count = "SELECT COUNT(*) AS COUNTS FROM Goods_Info WHERE Number=@Number";
                    //配置SQL语句里的参数
                    SQLiteParameter[] parameters_count = {
                        new SQLiteParameter("@Number",content),
                     };
                    //执行查询，结果为DataTable类型
                    DataTable dt_count = sqliteDBHelper_goodsDB.ExecuteDataTable(sql_count, parameters_count);
                    // 记录总数
                    totalCount = Convert.ToInt32(dt_count.Rows[0]["COUNTS"]);
                    // 总页数
                    pageCount = (int)Math.Ceiling((double)totalCount / pageSize);
                    //SQL查询语句
                    string sql = "SELECT Number AS value1,Name AS value2,Barcode AS value3,Price AS value4,RRP AS value5,Tax_Index AS value6,Stock_Control AS value7,Stock_Amount AS value8,Currency AS value9 FROM "
                        + "(SELECT * FROM Goods_Info WHERE Number=@Number ORDER BY Number ASC LIMIT @pageSize*@pageIndex) LIMIT @pageSize offset @pageSize*@pageIndexbefore;";
                    //配置SQL语句里的参数
                    SQLiteParameter[] parameters = {
                    new SQLiteParameter("@Number",content),
                    new SQLiteParameter("@pageSize",pageSize),
                    new SQLiteParameter("@pageIndex",pageIndex),
                    new SQLiteParameter("@pageIndexbefore",pageIndex-1),
                     };
                    //执行查询，结果为DataTable类型
                    DataTable dt = sqliteDBHelper_goodsDB.ExecuteDataTable(sql, parameters);
                    //查询税率
                    string sql_tax = "SELECT Number,Tax_Code,Tax_Name,Tax_Rate FROM Tax_Tariff ORDER BY Number ASC";
                    //执行查询
                    DataTable dt_tax = sqliteDBHelper_systemDB.ExecuteDataTable(sql_tax, null);
                    //判断查询结果是否为0行
                    if (dt.Rows.Count == 0)
                    {
                        dataGrid2_8.ItemsSource = null;//先清空表格内容
                        DataTable dt_temp = new DataTable();//新建临时表
                        dt_temp.Columns.Add(new DataColumn("No data！"));//添加列
                        //dt_temp.Rows.Add(dt_temp.NewRow());//添加行
                        //dt_temp.Rows[0]["Tip"] = "No result！";//设置行内容
                        dataGrid2_8.ItemsSource = dt_temp.DefaultView;//把表格放到控件中
                        return;
                    }
                    ////查询出来的表格的字段名为英文，但显示给用户的时候要为中文，所以在此添加若干个中文字段名显示给用户
                    //dt.Columns.Add(new DataColumn("Number"));
                    //dt.Columns.Add(new DataColumn("Name"));
                    //dt.Columns.Add(new DataColumn("Barcode"));
                    //dt.Columns.Add(new DataColumn("Price"));
                    //dt.Columns.Add(new DataColumn("RRP"));
                    //dt.Columns.Add(new DataColumn("Tax Index"));
                    //dt.Columns.Add(new DataColumn("Tax Code"));
                    //dt.Columns.Add(new DataColumn("Tax Name"));
                    //dt.Columns.Add(new DataColumn("Tax Rate"));
                    //dt.Columns.Add(new DataColumn("Stock Control"));
                    //dt.Columns.Add(new DataColumn("Stock Amount"));
                    //dt.Columns.Add(new DataColumn("Currency"));
                    //int j;
                    //DataRow[] dr;
                    ////以下为给新添加的中文字段赋值
                    //for (int i = 0; i < dt.Rows.Count; i++)
                    //{
                    //    dt.Rows[i]["Number"] = dt.Rows[i]["value1"].ToString();
                    //    dt.Rows[i]["Name"] = dt.Rows[i]["value2"].ToString();
                    //    dt.Rows[i]["Barcode"] = dt.Rows[i]["value3"].ToString();
                    //    dt.Rows[i]["Price"] = Convert.ToDouble(dt.Rows[i]["value4"].ToString()) / 100;
                    //    dt.Rows[i]["RRP"] = Convert.ToDouble(dt.Rows[i]["value5"].ToString()) / 100;
                    //    dt.Rows[i]["Tax Index"] = dt.Rows[i]["value6"].ToString();
                    //    //根据Tax Index的值在dt_tax表中查找那一行的数据
                    //    dr = dt_tax.Select("Number='" + dt.Rows[i]["value6"].ToString() + "'");
                    //    j = dt_tax.Rows.IndexOf(dr[0]);
                    //    dt.Rows[i]["Tax Code"] = dt_tax.Rows[j]["Tax_Code"].ToString();
                    //    dt.Rows[i]["Tax Name"] = dt_tax.Rows[j]["Tax_Name"].ToString();
                    //    dt.Rows[i]["Tax Rate"] = (Convert.ToDouble(dt_tax.Rows[j]["Tax_Rate"].ToString()) / 100).ToString("f2") + "%";
                    //    dr = null;
                    //    dt.Rows[i]["Stock Control"] = dt.Rows[i]["value7"].ToString();
                    //    dt.Rows[i]["Stock Amount"] = Convert.ToDouble(dt.Rows[i]["value8"].ToString()) / 10000;
                    //    dt.Rows[i]["Currency"] = dt.Rows[i]["value9"].ToString();
                    //}
                    //添加新的字段
                    dt.Columns.Add(new DataColumn("Number", Type.GetType("System.Decimal")));
                    dt.Columns.Add(new DataColumn("Name"));
                    dt.Columns.Add(new DataColumn("Barcode"));
                    dt.Columns.Add(new DataColumn("Price", Type.GetType("System.Decimal")));
                    dt.Columns.Add(new DataColumn("RRP", Type.GetType("System.Decimal")));
                    dt.Columns.Add(new DataColumn("Tax Index"));//, Type.GetType("System.Decimal")
                    dt.Columns.Add(new DataColumn("Tax Code"));
                    dt.Columns.Add(new DataColumn("Tax Name"));
                    dt.Columns.Add(new DataColumn("Tax Rate"));
                    dt.Columns.Add(new DataColumn("Stock Control"));
                    dt.Columns.Add(new DataColumn("Stock Amount", Type.GetType("System.Decimal")));
                    dt.Columns.Add(new DataColumn("Currency"));
                    int j;
                    DataRow[] dr;
                    //以下为给新添加的中文字段赋值
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        dt.Rows[i]["Number"] = dt.Rows[i]["value1"];
                        dt.Rows[i]["Name"] = dt.Rows[i]["value2"].ToString();
                        dt.Rows[i]["Barcode"] = dt.Rows[i]["value3"];
                        //Console.WriteLine("Barcode=" + dt.Rows[i]["value3"].ToString());
                        dt.Rows[i]["Price"] = Convert.ToDouble(dt.Rows[i]["value4"].ToString()) / 100;
                        dt.Rows[i]["RRP"] = Convert.ToDouble(dt.Rows[i]["value5"].ToString()) / 100;
                        dt.Rows[i]["Tax Index"] = dt.Rows[i]["value6"].ToString();
                        //Console.WriteLine("Tax Index=" + dt.Rows[i]["value6"].ToString());
                        //如果该商品的税率索引为0，则税率索引为空,因为数据库中该字段为NUMERIC类型，所以当该字段数据为空，查出来的时候默认是0
                        if (dt.Rows[i]["value6"].ToString().Equals("0"))
                        {
                            dt.Rows[i]["Tax Index"] = "";
                        }
                        //如果该商品的税率索引不为0，则添加税率信息,因为数据库中该字段为NUMERIC类型，所以当该字段数据为空，查出来的时候默认是0
                        else
                        {
                            //根据Tax Index的值在dt_tax表中查找那一行的数据
                            dr = dt_tax.Select("Number='" + dt.Rows[i]["value6"].ToString() + "'");
                            j = dt_tax.Rows.IndexOf(dr[0]);//Number为dt_tax表中第一个字段
                            dt.Rows[i]["Tax Code"] = dt_tax.Rows[j]["Tax_Code"].ToString();
                            dt.Rows[i]["Tax Name"] = dt_tax.Rows[j]["Tax_Name"].ToString();
                            dt.Rows[i]["Tax Rate"] = (Convert.ToDouble(dt_tax.Rows[j]["Tax_Rate"].ToString()) / 10000).ToString("0.00%");
                            dr = null;
                        }
                        dt.Rows[i]["Stock Control"] = dt.Rows[i]["value7"].ToString();
                        dt.Rows[i]["Stock Amount"] = Convert.ToDouble(dt.Rows[i]["value8"].ToString()) / 10000;
                        dt.Rows[i]["Currency"] = dt.Rows[i]["value9"].ToString();
                    }
                    dataGrid2_8.ItemsSource = dt.DefaultView;
                    //count是dt表格中前若干列标题是英文的字段数，也就是select查询的的字段数
                    int count = 9;
                    //将英文字段隐藏掉
                    for (int i = 0; i < count; i++)
                    {
                        this.dataGrid2_8.Columns[i].Visibility = System.Windows.Visibility.Hidden;
                    }
                    //以下为设置字段宽度
                    this.dataGrid2_8.Columns[count++].Width = 70;
                    this.dataGrid2_8.Columns[count++].Width = 150;
                    this.dataGrid2_8.Columns[count++].Width = 100;
                    this.dataGrid2_8.Columns[count++].Width = 60;
                    this.dataGrid2_8.Columns[count++].Width = 60;
                    this.dataGrid2_8.Columns[count++].Width = 80;
                    this.dataGrid2_8.Columns[count++].Width = 80;
                    this.dataGrid2_8.Columns[count++].Width = 110;
                    this.dataGrid2_8.Columns[count++].Width = 80;
                    this.dataGrid2_8.Columns[count++].Width = 100;
                    this.dataGrid2_8.Columns[count++].Width = 110;
                    this.dataGrid2_8.Columns[count++].Width = 80;
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
                    string sql_count = "SELECT COUNT(*) AS COUNTS FROM Goods_Info WHERE Name LIKE @Name";
                    //配置SQL语句里的参数
                    SQLiteParameter[] parameters_count = {
                        new SQLiteParameter("@Name","%" +content +"%"),
                     };
                    //执行查询，结果为DataTable类型
                    DataTable dt_count = sqliteDBHelper_goodsDB.ExecuteDataTable(sql_count, parameters_count);
                    // 记录总数
                    totalCount = Convert.ToInt32(dt_count.Rows[0]["COUNTS"]);
                    // 总页数
                    pageCount = (int)Math.Ceiling((double)totalCount / pageSize);
                    //SQL查询语句
                    string sql = "SELECT Number AS value1,Name AS value2,Barcode AS value3,Price AS value4,RRP AS value5,Tax_Index AS value6,Stock_Control AS value7,Stock_Amount AS value8,Currency AS value9 FROM "
                        + "(SELECT * FROM Goods_Info WHERE Name LIKE @Name ORDER BY Number ASC LIMIT @pageSize*@pageIndex) LIMIT @pageSize offset @pageSize*@pageIndexbefore;";
                    //配置SQL语句里的参数
                    SQLiteParameter[] parameters = {
                    new SQLiteParameter("@Name","%" +content +"%"),
                    new SQLiteParameter("@pageSize",pageSize),
                    new SQLiteParameter("@pageIndex",pageIndex),
                    new SQLiteParameter("@pageIndexbefore",pageIndex-1),
                     };
                    //执行查询，结果为DataTable类型
                    DataTable dt = sqliteDBHelper_goodsDB.ExecuteDataTable(sql, parameters);
                    //查询税率
                    string sql_tax = "SELECT Number,Tax_Code,Tax_Name,Tax_Rate FROM Tax_Tariff ORDER BY Number ASC";
                    //执行查询
                    DataTable dt_tax = sqliteDBHelper_systemDB.ExecuteDataTable(sql_tax, null);
                    //判断查询结果是否为0行
                    if (dt.Rows.Count == 0)
                    {
                        dataGrid2_8.ItemsSource = null;//先清空表格内容
                        DataTable dt_temp = new DataTable();//新建临时表
                        dt_temp.Columns.Add(new DataColumn("No data！"));//添加列
                        //dt_temp.Rows.Add(dt_temp.NewRow());//添加行
                        //dt_temp.Rows[0]["Tip"] = "No result！";//设置行内容
                        dataGrid2_8.ItemsSource = dt_temp.DefaultView;//把表格放到控件中
                        return;
                    }
                    //添加新的字段
                    dt.Columns.Add(new DataColumn("Number", Type.GetType("System.Decimal")));
                    dt.Columns.Add(new DataColumn("Name"));
                    dt.Columns.Add(new DataColumn("Barcode"));
                    dt.Columns.Add(new DataColumn("Price", Type.GetType("System.Decimal")));
                    dt.Columns.Add(new DataColumn("RRP", Type.GetType("System.Decimal")));
                    dt.Columns.Add(new DataColumn("Tax Index"));//, Type.GetType("System.Decimal")
                    dt.Columns.Add(new DataColumn("Tax Code"));
                    dt.Columns.Add(new DataColumn("Tax Name"));
                    dt.Columns.Add(new DataColumn("Tax Rate"));
                    dt.Columns.Add(new DataColumn("Stock Control"));
                    dt.Columns.Add(new DataColumn("Stock Amount", Type.GetType("System.Decimal")));
                    dt.Columns.Add(new DataColumn("Currency"));
                    int j;
                    DataRow[] dr;
                    //以下为给新添加的中文字段赋值
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        dt.Rows[i]["Number"] = dt.Rows[i]["value1"];
                        dt.Rows[i]["Name"] = dt.Rows[i]["value2"].ToString();
                        dt.Rows[i]["Barcode"] = dt.Rows[i]["value3"];
                        //Console.WriteLine("Barcode=" + dt.Rows[i]["value3"].ToString());
                        dt.Rows[i]["Price"] = Convert.ToDouble(dt.Rows[i]["value4"].ToString()) / 100;
                        dt.Rows[i]["RRP"] = Convert.ToDouble(dt.Rows[i]["value5"].ToString()) / 100;
                        dt.Rows[i]["Tax Index"] = dt.Rows[i]["value6"].ToString();
                        //Console.WriteLine("Tax Index=" + dt.Rows[i]["value6"].ToString());
                        //如果该商品的税率索引为0，则税率索引为空,因为数据库中该字段为NUMERIC类型，所以当该字段数据为空，查出来的时候默认是0
                        if (dt.Rows[i]["value6"].ToString().Equals("0"))
                        {
                            dt.Rows[i]["Tax Index"] = "";
                        }
                        //如果该商品的税率索引不为0，则添加税率信息,因为数据库中该字段为NUMERIC类型，所以当该字段数据为空，查出来的时候默认是0
                        else
                        {
                            //根据Tax Index的值在dt_tax表中查找那一行的数据
                            dr = dt_tax.Select("Number='" + dt.Rows[i]["value6"].ToString() + "'");
                            j = dt_tax.Rows.IndexOf(dr[0]);//Number为dt_tax表中第一个字段
                            dt.Rows[i]["Tax Code"] = dt_tax.Rows[j]["Tax_Code"].ToString();
                            dt.Rows[i]["Tax Name"] = dt_tax.Rows[j]["Tax_Name"].ToString();
                            dt.Rows[i]["Tax Rate"] = (Convert.ToDouble(dt_tax.Rows[j]["Tax_Rate"].ToString()) / 10000).ToString("0.00%");
                            dr = null;
                        }
                        dt.Rows[i]["Stock Control"] = dt.Rows[i]["value7"].ToString();
                        dt.Rows[i]["Stock Amount"] = Convert.ToDouble(dt.Rows[i]["value8"].ToString()) / 10000;
                        dt.Rows[i]["Currency"] = dt.Rows[i]["value9"].ToString();
                    }
                    dataGrid2_8.ItemsSource = dt.DefaultView;
                    //count是dt表格中前若干列标题是英文的字段数，也就是select查询的的字段数
                    int count = 9;
                    //将英文字段隐藏掉
                    for (int i = 0; i < count; i++)
                    {
                        this.dataGrid2_8.Columns[i].Visibility = System.Windows.Visibility.Hidden;
                    }
                    //以下为设置字段宽度
                    this.dataGrid2_8.Columns[count++].Width = 70;
                    this.dataGrid2_8.Columns[count++].Width = 150;
                    this.dataGrid2_8.Columns[count++].Width = 100;
                    this.dataGrid2_8.Columns[count++].Width = 60;
                    this.dataGrid2_8.Columns[count++].Width = 60;
                    this.dataGrid2_8.Columns[count++].Width = 80;
                    this.dataGrid2_8.Columns[count++].Width = 80;
                    this.dataGrid2_8.Columns[count++].Width = 110;
                    this.dataGrid2_8.Columns[count++].Width = 80;
                    this.dataGrid2_8.Columns[count++].Width = 100;
                    this.dataGrid2_8.Columns[count++].Width = 110;
                    this.dataGrid2_8.Columns[count++].Width = 80;
                }
                else
                {
                    MessageBox.Show("Please fill in the query textbox !", "Information", MessageBoxButton.OK, MessageBoxImage.Warning);

                }
                #endregion
            }
            else if (option.Equals("Barcode"))
            {
                #region
                if (!content.Equals(""))
                {
                    //SQL语句
                    string sql_count = "SELECT COUNT(*) AS COUNTS FROM Goods_Info WHERE Barcode=@Barcode";
                    //配置SQL语句里的参数
                    SQLiteParameter[] parameters_count = {
                        new SQLiteParameter("@Barcode",content),
                     };
                    //执行查询，结果为DataTable类型
                    DataTable dt_count = sqliteDBHelper_goodsDB.ExecuteDataTable(sql_count, parameters_count);
                    // 记录总数
                    totalCount = Convert.ToInt32(dt_count.Rows[0]["COUNTS"]);
                    // 总页数
                    pageCount = (int)Math.Ceiling((double)totalCount / pageSize);
                    //SQL查询语句
                    string sql = "SELECT Number AS value1,Name AS value2,Barcode AS value3,Price AS value4,RRP AS value5,Tax_Index AS value6,Stock_Control AS value7,Stock_Amount AS value8,Currency AS value9 FROM "
                        + "(SELECT * FROM Goods_Info WHERE Barcode=@Barcode ORDER BY Number ASC LIMIT @pageSize*@pageIndex) LIMIT @pageSize offset @pageSize*@pageIndexbefore;";
                    //配置SQL语句里的参数
                    SQLiteParameter[] parameters = {
                    new SQLiteParameter("@Barcode",content),
                    new SQLiteParameter("@pageSize",pageSize),
                    new SQLiteParameter("@pageIndex",pageIndex),
                    new SQLiteParameter("@pageIndexbefore",pageIndex-1),
                     };
                    //执行查询，结果为DataTable类型
                    DataTable dt = sqliteDBHelper_goodsDB.ExecuteDataTable(sql, parameters);
                    //查询税率
                    string sql_tax = "SELECT Number,Tax_Code,Tax_Name,Tax_Rate FROM Tax_Tariff ORDER BY Number ASC";
                    //执行查询
                    DataTable dt_tax = sqliteDBHelper_systemDB.ExecuteDataTable(sql_tax, null);
                    //判断查询结果是否为0行
                    if (dt.Rows.Count == 0)
                    {
                        dataGrid2_8.ItemsSource = null;//先清空表格内容
                        DataTable dt_temp = new DataTable();//新建临时表
                        dt_temp.Columns.Add(new DataColumn("No data！"));//添加列
                        //dt_temp.Rows.Add(dt_temp.NewRow());//添加行
                        //dt_temp.Rows[0]["Tip"] = "No result！";//设置行内容
                        dataGrid2_8.ItemsSource = dt_temp.DefaultView;//把表格放到控件中
                        return;
                    }
                    //添加新的字段
                    dt.Columns.Add(new DataColumn("Number", Type.GetType("System.Decimal")));
                    dt.Columns.Add(new DataColumn("Name"));
                    dt.Columns.Add(new DataColumn("Barcode"));
                    dt.Columns.Add(new DataColumn("Price", Type.GetType("System.Decimal")));
                    dt.Columns.Add(new DataColumn("RRP", Type.GetType("System.Decimal")));
                    dt.Columns.Add(new DataColumn("Tax Index"));//, Type.GetType("System.Decimal")
                    dt.Columns.Add(new DataColumn("Tax Code"));
                    dt.Columns.Add(new DataColumn("Tax Name"));
                    dt.Columns.Add(new DataColumn("Tax Rate"));
                    dt.Columns.Add(new DataColumn("Stock Control"));
                    dt.Columns.Add(new DataColumn("Stock Amount", Type.GetType("System.Decimal")));
                    dt.Columns.Add(new DataColumn("Currency"));
                    int j;
                    DataRow[] dr;
                    //以下为给新添加的中文字段赋值
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        dt.Rows[i]["Number"] = dt.Rows[i]["value1"];
                        dt.Rows[i]["Name"] = dt.Rows[i]["value2"].ToString();
                        dt.Rows[i]["Barcode"] = dt.Rows[i]["value3"];
                        //Console.WriteLine("Barcode=" + dt.Rows[i]["value3"].ToString());
                        dt.Rows[i]["Price"] = Convert.ToDouble(dt.Rows[i]["value4"].ToString()) / 100;
                        dt.Rows[i]["RRP"] = Convert.ToDouble(dt.Rows[i]["value5"].ToString()) / 100;
                        dt.Rows[i]["Tax Index"] = dt.Rows[i]["value6"].ToString();
                        //Console.WriteLine("Tax Index=" + dt.Rows[i]["value6"].ToString());
                        //如果该商品的税率索引为0，则税率索引为空,因为数据库中该字段为NUMERIC类型，所以当该字段数据为空，查出来的时候默认是0
                        if (dt.Rows[i]["value6"].ToString().Equals("0"))
                        {
                            dt.Rows[i]["Tax Index"] = "";
                        }
                        //如果该商品的税率索引不为0，则添加税率信息,因为数据库中该字段为NUMERIC类型，所以当该字段数据为空，查出来的时候默认是0
                        else
                        {
                            //根据Tax Index的值在dt_tax表中查找那一行的数据
                            dr = dt_tax.Select("Number='" + dt.Rows[i]["value6"].ToString() + "'");
                            j = dt_tax.Rows.IndexOf(dr[0]);//Number为dt_tax表中第一个字段
                            dt.Rows[i]["Tax Code"] = dt_tax.Rows[j]["Tax_Code"].ToString();
                            dt.Rows[i]["Tax Name"] = dt_tax.Rows[j]["Tax_Name"].ToString();
                            dt.Rows[i]["Tax Rate"] = (Convert.ToDouble(dt_tax.Rows[j]["Tax_Rate"].ToString()) / 10000).ToString("0.00%");
                            dr = null;
                        }
                        dt.Rows[i]["Stock Control"] = dt.Rows[i]["value7"].ToString();
                        dt.Rows[i]["Stock Amount"] = Convert.ToDouble(dt.Rows[i]["value8"].ToString()) / 10000;
                        dt.Rows[i]["Currency"] = dt.Rows[i]["value9"].ToString();
                    }
                    dataGrid2_8.ItemsSource = dt.DefaultView;
                    //count是dt表格中前若干列标题是英文的字段数，也就是select查询的的字段数
                    int count = 9;
                    //将英文字段隐藏掉
                    for (int i = 0; i < count; i++)
                    {
                        this.dataGrid2_8.Columns[i].Visibility = System.Windows.Visibility.Hidden;
                    }
                    //以下为设置字段宽度
                    this.dataGrid2_8.Columns[count++].Width = 70;
                    this.dataGrid2_8.Columns[count++].Width = 150;
                    this.dataGrid2_8.Columns[count++].Width = 100;
                    this.dataGrid2_8.Columns[count++].Width = 60;
                    this.dataGrid2_8.Columns[count++].Width = 60;
                    this.dataGrid2_8.Columns[count++].Width = 80;
                    this.dataGrid2_8.Columns[count++].Width = 80;
                    this.dataGrid2_8.Columns[count++].Width = 110;
                    this.dataGrid2_8.Columns[count++].Width = 80;
                    this.dataGrid2_8.Columns[count++].Width = 100;
                    this.dataGrid2_8.Columns[count++].Width = 110;
                    this.dataGrid2_8.Columns[count++].Width = 80;
                }
                else
                {
                    MessageBox.Show("Please fill in the query textbox !", "Information", MessageBoxButton.OK, MessageBoxImage.Warning);

                }
                #endregion
            }
        }
        ///<summary>
        ///part2_8 单品 删除
        ///</summary>
        private void button2_8_3_Click(object sender, RoutedEventArgs e)
        {
            //弹出提示框，未选择要删除的数据
            if (dataGrid2_8.SelectedItem == null)
            {
                MessageBox.Show("Please select one record to delete first!", "Information", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            //选择了某一行数据时
            else
            {
                //获取选中行所有数据
                DataRowView mySelectedElement = (DataRowView)dataGrid2_8.SelectedItem;
                //获取必要信息
                string Number = mySelectedElement.Row["Number"].ToString();
                //查看部类商品关联表 此商品是否被关联
                string sql_count = "SELECT COUNT(*) AS COUNTS FROM Department_Associate WHERE PLU_No=@PLU_No";
                //配置SQL语句里的参数
                SQLiteParameter[] parameter_count = {
                    new SQLiteParameter("@PLU_No",Number),
                        };
                //执行查询，结果为DataTable类型
                DataTable dt_count = sqliteDBHelper_goodsDB.ExecuteDataTable(sql_count, parameter_count);
                //判断结果
                if (Convert.ToInt32(dt_count.Rows[0]["COUNTS"]) == 0)//若数量为0
                {
                    //弹出确认框，当为确定时
                    if (MessageBox.Show("Confirm that you want to delete the record ( the Number:  " + mySelectedElement.Row["Number"].ToString() + " )？", "Information", MessageBoxButton.OKCancel, MessageBoxImage.Information) == MessageBoxResult.OK)
                    {
                        //SQL语句
                        string sql = "DELETE FROM Goods_Info WHERE Number=@Number";
                        //配置SQL语句里的参数
                        SQLiteParameter[] parameters = {
                            new SQLiteParameter("@Number",Number)
                        };
                        //执行SQL，并做判断
                        if (sqliteDBHelper_goodsDB.ExecuteNonQuery(sql, parameters) == 1)
                        {
                            //刷新查询框内容
                            button2_8_2_Click();
                            //设置分页按钮是否可用
                            SetPageButtonEnabled();
                            //设置控件显示信息
                            SetPagerInfo(pageIndex, pageSize, pageCount, totalCount);
                        }
                        else
                        {
                            MessageBox.Show("Delete unsuccessfully!", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                else//有部类关联
                {
                    //弹出确认框，当为确定时
                    if (MessageBox.Show("Confirm that you want to delete the record ( the Number:  " + mySelectedElement.Row["Number"].ToString() + " ) and the the relationship in Department-Associate that you have set in Department-Setting before？", "Information", MessageBoxButton.OKCancel, MessageBoxImage.Information) == MessageBoxResult.OK)
                    {

                        //SQL语句
                        string sql = "DELETE FROM Goods_Info WHERE Number=@Number";
                        //配置SQL语句里的参数
                        SQLiteParameter[] parameters = {
                            new SQLiteParameter("@Number",Number)
                        };
                        //执行SQL，并做判断
                        if (sqliteDBHelper_goodsDB.ExecuteNonQuery(sql, parameters) == 1)
                        {
                            //刷新查询框内容
                            button2_8_2_Click();
                            //设置分页按钮是否可用
                            SetPageButtonEnabled();
                            //设置控件显示信息
                            SetPagerInfo(pageIndex, pageSize, pageCount, totalCount);
                        }
                        else
                        {
                            MessageBox.Show("Delete this good unsuccessfully!", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        //删除部类管理
                        string sql_update = "UPDATE Department_Associate SET PLU_No='0' WHERE PLU_No=@PLU_No";
                        //配置SQL语句里的参数
                        SQLiteParameter[] parameter_update = {
                                new SQLiteParameter("@PLU_No",Number),
                            };
                        //执行查询，结果为DataTable类型
                        ;
                        if (sqliteDBHelper_goodsDB.ExecuteNonQuery(sql_update, parameter_update) >= 1)
                        {
                            //MessageBox.Show("This good was set in Department Associate before, the relationship has been lifted successfully now!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("The relationship in Department-Associate lifts unsuccessfully!", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
        }
        ///<summary>
        ///part2_8 单品 修改本地数据库
        ///</summary>
        private void button2_8_4_Click(object sender, RoutedEventArgs e)
        {
            //弹出提示框，未选择要删除的数据
            if (dataGrid2_8.SelectedItem == null)
            {
                MessageBox.Show("Please select one of the data you want to modify!", "Information", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            //选择了某一行数据时
            else
            {
                //获取选中行所有数据
                DataRowView mySelectedElement = (DataRowView)dataGrid2_8.SelectedItem;
                //声明一个变量
                Plu plu = new Plu();
                //变量赋值
                plu.Number = mySelectedElement.Row["Number"].ToString();
                plu.Name = mySelectedElement.Row["Name"].ToString();
                plu.Barcode = mySelectedElement.Row["Barcode"].ToString();
                plu.Price = Convert.ToDouble(mySelectedElement.Row["Price"].ToString());
                plu.RRP = Convert.ToDouble(mySelectedElement.Row["RRP"].ToString());
                plu.Tax_Index = mySelectedElement.Row["Tax Index"].ToString();
                plu.Stock_Control = Convert.ToInt32(mySelectedElement.Row["Stock Control"]);
                plu.Stock_Amount = Convert.ToDouble(mySelectedElement.Row["Stock Amount"].ToString());
                plu.Tax_Code = mySelectedElement.Row["Tax Code"].ToString();
                //声明一个修改信息的窗体
                Update_Plu upd_plu = new Update_Plu(plu, goodsDB, systemDB, programmingDB);
                //弹出窗体
                upd_plu.ShowDialog();
                //刷新查询框内容
                button2_8_2_Click();
                //设置分页按钮是否可用
                SetPageButtonEnabled();
                //设置控件显示信息
                SetPagerInfo(pageIndex, pageSize, pageCount, totalCount);
            }
        }
        /// <summary>
        /// 设置分页按钮是否可用
        /// </summary>
        private void SetPageButtonEnabled()
        {
            //确定分页按钮的是否可用
            if (pageCount <= 1)
            {
                btnPageDown.IsEnabled = false;
                btnPageUp.IsEnabled = false;
                btnEndPage.IsEnabled = false;
            }
            else
            {
                if (pageIndex == pageCount)
                {
                    btnPageDown.IsEnabled = false;
                    btnPageUp.IsEnabled = true;
                    btnEndPage.IsEnabled = false;
                }
                else if (pageIndex <= 1)
                {
                    btnPageDown.IsEnabled = true;
                    btnPageUp.IsEnabled = false;
                    btnEndPage.IsEnabled = true;
                }
                else
                {
                    btnPageDown.IsEnabled = true;
                    btnPageUp.IsEnabled = true;
                    btnEndPage.IsEnabled = true;
                }
            }
        }
        /// <summary>
        /// 设置控件显示信息
        /// </summary>
        /// <param name="pageIndex">当前页码</param>
        /// <param name="pageCount">共有页数</param>
        /// <param name="totalCount">总记录条数</param>
        private void SetPagerInfo(int pageIndex, int pageSize, int pageCount, int totalCount)
        {
            //txtPagerInfo.Text = String.Format("The current page is【{0}】, each page shows【{1}】records, a total of【{2}】page, a total of【{3}】records. ", pageIndex, pageSize, pageCount, totalCount);
            txtPagerInfo.Text = String.Format("PER PAGE: {0}   ALL: {1}   PAGE: {2} / {3} ", pageSize, totalCount, pageIndex, pageCount);
        }
        /// <summary>
        /// 首页按钮事件
        /// </summary>
        private void btnFirstPage_Click(object sender, RoutedEventArgs e)
        {
            pageIndex = 1;
            button2_8_2_Click();//查询 设置 pageCount totalCount ；显示数据
            SetPageButtonEnabled();//设置分页按钮是否可用
            SetPagerInfo(pageIndex, pageSize, pageCount, totalCount);//设置控件显示信息
        }
        /// <summary>
        /// 下一页按钮事件
        /// </summary>
        private void btnPageDown_Click(object sender, RoutedEventArgs e)
        {
            pageIndex++;
            button2_8_2_Click();//查询 设置 pageCount totalCount ；显示数据
            SetPageButtonEnabled();//设置分页按钮是否可用
            SetPagerInfo(pageIndex, pageSize, pageCount, totalCount);//设置控件显示信息
        }
        /// <summary>
        /// 上一页按钮事件
        /// </summary>
        private void btnPageUp_Click(object sender, RoutedEventArgs e)
        {
            pageIndex--;
            button2_8_2_Click();//查询 设置 pageCount totalCount ；显示数据
            SetPageButtonEnabled();//设置分页按钮是否可用
            SetPagerInfo(pageIndex, pageSize, pageCount, totalCount);//设置控件显示信息
        }
        /// <summary>
        /// 尾页按钮事件
        /// </summary>
        private void btnEndPage_Click(object sender, RoutedEventArgs e)
        {
            pageIndex = pageCount;
            button2_8_2_Click();//查询 设置 pageCount totalCount ；显示数据
            SetPageButtonEnabled();//设置分页按钮是否可用
            SetPagerInfo(pageIndex, pageSize, pageCount, totalCount);//设置控件显示信息
        }
        /// <summary>
        /// 动态改变表格大小 商品表
        /// </summary>
        private void gird2_8_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            dataGrid2_8.MaxHeight = gird2_8.ActualHeight;
            dataGrid2_8.MaxWidth = gird2_8.ActualWidth;
        }
        #endregion
        /////////////////////////////////////////////////////////////////////////////////
    }
}
