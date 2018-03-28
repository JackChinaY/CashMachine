using CashMachine.entity_local;
using CashMachine.SQLiteDB;
using System;
using System.Windows;
using System.Data.SQLite;
using System.Data;

using System.Windows.Input;
using System.Windows.Controls;
using System.Text.RegularExpressions;

namespace CashMachine.dialogs_local_en
{
    /// <summary>
    /// Insert_Plu.xaml 的交互逻辑
    /// </summary>
    public partial class Insert_Plu : Window
    {
        //声明一个变量
        public string dataBase;
        public string dataBase_TAX;
        public string dataBase_PRO;
        //声明一个SQLite数据库
        SQLiteDBHelper sqliteDBHelper = null;
        SQLiteDBHelper sqliteDBHelper_PRO = null;//programmingDB

        //无参构造函数
        public Insert_Plu()
        {
            InitializeComponent();
        }

        //有参构造函数
        public Insert_Plu(string dataBase, string dataBase_TAX, string dataBase_PRO)
        {
            this.dataBase = dataBase;//赋值
            this.dataBase_TAX = dataBase_TAX;//赋值
            this.dataBase_PRO = dataBase_PRO;//赋值
            InitializeComponent();//初始化窗体组件
            this.ResizeMode = ResizeMode.CanMinimize;//禁用“最大化”按钮
            Init();//初始化窗体里的数据
        }
        /// <summary>
        /// 窗体数据初始化
        /// </summary>
        public void Init()
        {
            //设置商品编号 自动生成的
            //SQLite数据库，此处连接的是goodsDB
            sqliteDBHelper = new SQLiteDBHelper(dataBase);
            //SQL语句
            string sql = "SELECT MAX(Number) AS MAXNUM FROM Goods_Info";
            //执行查询，结果为DataTable类型
            DataTable dt = sqliteDBHelper.ExecuteDataTable(sql, null);
            //判断结果
            if (dt.Rows[0]["MAXNUM"].ToString() == "" || dt.Rows[0]["MAXNUM"] == null)//若为空表，则序号从1开始
            {

                textBlock2_8_1.Text = "1";
            }
            else
            {
                textBlock2_8_1.Text = (Convert.ToInt32(dt.Rows[0]["MAXNUM"].ToString()) + 1).ToString();//最大值加1
            }

            ////设置税种税目索引 自动生成的
            ////SQLite数据库，此处连接的是systemDB
            //sqliteDBHelper = new SQLiteDBHelper(dataBase_TAX);
            ////SQL语句
            //string sql_TAX = "SELECT Number FROM Tax_Tariff";
            ////执行查询，结果为DataTable类型
            //DataTable dt_TAX = sqliteDBHelper.ExecuteDataTable(sql_TAX, null);
            ////判断结果
            //if (dt_TAX.Rows.Count == 0)//若为空表，则序号从1开始
            //{
            //    //弹出提示框
            //    MessageBox.Show("税种税目暂时为空，请先前往设置税种税目！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            //    //this.Close();
            //}
            //else
            //{
            //    for (int i = 0; i < dt_TAX.Rows.Count; i++)
            //    {
            //        comboBox2_8_1.Items.Add(dt_TAX.Rows[i]["Number"]);
            //    }
            //}

            ////切换SQLite数据库，此处连接的是goodsDB
            //sqliteDBHelper = new SQLiteDBHelper(dataBase);
        }
        /// <summary>
        /// barcode 只允许输入0-9数字 backspace
        /// </summary>
        private void textBox2_8_2_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textbox = sender as TextBox;
            //正则判断
            Regex regex = new Regex(@"^([0-9]{0,})$");//需要加上@
            if (regex.Matches(textbox.Text).Count > 0)
            {
            }
            //如果不匹配
            else
            {
                //如果长度>=1则删除刚刚输入的一个字符，如果长度为0，按删除键会报错
                if (textbox.Text.Length >= 1)
                {
                    textbox.Text = textbox.Text.Remove(textbox.Text.Length - 1, 1);
                    textbox.SelectionStart = textbox.Text.Length;
                }
            }
        }
        /// <summary>
        /// 当输入完条形码后就开始验证其唯一性
        /// </summary>
        private void textBox2_8_2_LostFocus(object sender, RoutedEventArgs e)
        {
            if (textBox2_8_2.Text.Equals(""))
            {
                textBlock2_8_2.Text = "";
            }
            else
            {
                //SQL语句
                string sql = "SELECT count(*) AS COUNTS FROM Goods_Info WHERE Barcode=@Barcode";
                //配置SQL语句里的参数
                SQLiteParameter[] parameter = {
                    new SQLiteParameter("@Barcode",textBox2_8_2.Text),
                };
                //执行查询，结果为DataTable类型
                DataTable dt = sqliteDBHelper.ExecuteDataTable(sql, parameter);
                //判断结果
                if (Convert.ToInt32(dt.Rows[0]["COUNTS"]) == 0)//若数量为0
                {
                    textBlock2_8_2.Text = "available";
                }
                else
                {
                    textBlock2_8_2.Text = "already exists";
                }
            }
        }
        /// <summary>
        /// 价格 只允许输入0-9数字 .
        /// </summary>
        private void textBox2_8_3_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textbox = sender as TextBox;
            //正则判断
            Regex regex = new Regex(@"^(([0]{1,1}([\.]{0,1})([0-9]{0,9}))|([1-9]{1,1})([0-9]{0,8})([\.]{0,1})([0-9]{0,9}))$");//需要加上@
            if (regex.Matches(textbox.Text).Count > 0)
            {
                //如果误输入00，则换成0
                if (textbox.Text.Equals("00") || textbox.Text.Equals("01") || textbox.Text.Equals("02") || textbox.Text.Equals("03") || textbox.Text.Equals("04") || textbox.Text.Equals("05") || textbox.Text.Equals("06") || textbox.Text.Equals("07") || textbox.Text.Equals("08") || textbox.Text.Equals("09"))
                {
                    textbox.Text = textbox.Text.Remove(0, 1);
                    textbox.SelectionStart = textbox.Text.Length;
                }
            }
            //如果不匹配
            else
            {
                //如果长度>=1则删除刚刚输入的一个字符，如果长度为0，按删除键会报错
                if (textbox.Text.Length >= 1)
                {
                    textbox.Text = textbox.Text.Remove(textbox.Text.Length - 1, 1);
                    textbox.SelectionStart = textbox.Text.Length;
                }
            }
        }
        /// <summary>
        /// 选择税目索引
        /// </summary>
        private void button2_8_2_Click(object sender, RoutedEventArgs e)
        {
            //声明一个添加信息的窗体
            Good_Tax_Choice choice = new Good_Tax_Choice(this.dataBase_TAX);
            //显示对话框
            choice.ShowDialog();
            //商品编号对话框内容赋值
            if (!choice.Number.Equals(string.Empty))
            {
                textBlock2_8_4.Text = choice.Number;
                if (choice.Tax_Code.Equals("B"))
                {
                    textBlock2_8_5.Text = "*";
                    textBlock2_8_7.Text = "*";
                    textBlock2_8_6.Visibility = Visibility.Visible;
                    textBox2_8_5.Visibility = Visibility.Visible;
                }
                else
                {
                    textBlock2_8_5.Text = "";
                    textBox2_8_5.Text = "";
                    textBlock2_8_7.Text = "";
                    textBlock2_8_6.Visibility = Visibility.Hidden;
                    textBox2_8_5.Visibility = Visibility.Hidden;
                }
            }
        }
        /// <summary>
        /// 库存 只允许输入0-9数字 .
        /// </summary>
        private void textBox2_8_4_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textbox = sender as TextBox;
            //正则判断
            Regex regex = new Regex(@"^(([0]{1,1}([\.]{0,1})([0-9]{0,9}))|([1-9]{1,1})([0-9]{0,8})([\.]{0,1})([0-9]{0,9}))$");//需要加上@
            if (regex.Matches(textbox.Text).Count > 0)
            {
                //如果误输入00，则换成0
                if (textbox.Text.Equals("00") || textbox.Text.Equals("01") || textbox.Text.Equals("02") || textbox.Text.Equals("03") || textbox.Text.Equals("04") || textbox.Text.Equals("05") || textbox.Text.Equals("06") || textbox.Text.Equals("07") || textbox.Text.Equals("08") || textbox.Text.Equals("09"))
                {
                    textbox.Text = textbox.Text.Remove(0, 1);
                    textbox.SelectionStart = textbox.Text.Length;
                }
            }
            //如果不匹配
            else
            {
                //如果长度>=1则删除刚刚输入的一个字符，如果长度为0，按删除键会报错
                if (textbox.Text.Length >= 1)
                {
                    textbox.Text = textbox.Text.Remove(textbox.Text.Length - 1, 1);
                    textbox.SelectionStart = textbox.Text.Length;
                }
            }
        }
        /// <summary>
        ///  RRP 只允许输入0-9数字 .
        /// </summary>
        private void textBox2_8_5_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textbox = sender as TextBox;
            //正则判断
            Regex regex = new Regex(@"^(([0]{1,1}([\.]{0,1})([0-9]{0,9}))|([1-9]{1,1})([0-9]{0,8})([\.]{0,1})([0-9]{0,9}))$");//需要加上@
            if (regex.Matches(textbox.Text).Count > 0)
            {
                //如果误输入00，则换成0
                if (textbox.Text.Equals("00") || textbox.Text.Equals("01") || textbox.Text.Equals("02") || textbox.Text.Equals("03") || textbox.Text.Equals("04") || textbox.Text.Equals("05") || textbox.Text.Equals("06") || textbox.Text.Equals("07") || textbox.Text.Equals("08") || textbox.Text.Equals("09"))
                {
                    textbox.Text = textbox.Text.Remove(0, 1);
                    textbox.SelectionStart = textbox.Text.Length;
                }
            }
            //如果不匹配
            else
            {
                //如果长度>=1则删除刚刚输入的一个字符，如果长度为0，按删除键会报错
                if (textbox.Text.Length >= 1)
                {
                    textbox.Text = textbox.Text.Remove(textbox.Text.Length - 1, 1);
                    textbox.SelectionStart = textbox.Text.Length;
                }
            }
        }
        /// <summary>
        /// 若Stock_Control=1，则为Stock_Amount必填项
        /// </summary>
        private void checkBox2_8_1_Checked(object sender, RoutedEventArgs e)
        {
            textBlock2_8_3.Text = "*";
        }
        /// <summary>
        /// 若Stock_Control=0，Stock_Amount可不填，默认为0
        /// </summary>
        private void checkBox2_8_1_Unchecked(object sender, RoutedEventArgs e)
        {
            textBlock2_8_3.Text = "";
        }
        /// <summary>
        /// 提交保存数据
        /// </summary>
        private void button2_8_1_Click(object sender, RoutedEventArgs e)
        {
            //判断输入框内容是否为空
            if (textBox2_8_1.Text.Equals(""))
            {
                MessageBox.Show("The Name is required!", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (textBox2_8_1.Text.Length != 0 && textBox2_8_1.Text.Trim().Equals(""))
            {
                MessageBox.Show("The Name can not be all spaces!", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (textBlock2_8_2.Text.Equals("already exists"))
            {
                MessageBox.Show("The Barcode already existed in the database. Please re-enter it!", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (textBox2_8_3.Text.Equals(""))
            {
                MessageBox.Show("The Price is required!", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (textBox2_8_3.Text.Equals("."))
            {
                MessageBox.Show("Syntax error：the Price is eroor, please check!", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (Convert.ToDouble(textBox2_8_3.Text) > 999999.99)
            {
                MessageBox.Show("The Price exceeds the upper limit, it should be less than or equal to 999999.99!", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (textBox2_8_5.Text.Equals("."))
            {
                MessageBox.Show("Syntax error：the RRP is eroor,please check!", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (textBlock2_8_4.Text.Equals(""))
            {
                MessageBox.Show("The Tax Index is required please click on the 'Choose' button !", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (textBlock2_8_7.Text.Equals("*") && textBox2_8_2.Text.Equals(""))
            {
                MessageBox.Show("The Barcode is required when you choose the Tax Code B!", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (textBox2_8_3.Text.Equals("."))
            {
                MessageBox.Show("Syntax error：the RRP is eroor,please check!", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (textBlock2_8_5.Text.Equals("*") && textBox2_8_5.Text.Equals(""))
            {
                MessageBox.Show("The RRP is required when you choose the Tax Code B!", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (textBlock2_8_5.Text.Equals("*") && Convert.ToDouble(textBox2_8_5.Text) > 999999.99)
            {
                MessageBox.Show("The RRP exceeds the upper limit, it should be less than or equal to 999999.99!", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (textBlock2_8_3.Text.Equals("*") && textBox2_8_4.Text.Equals(""))
            {
                MessageBox.Show("The Stock Amount is required when you check the Stock Control !", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (!textBox2_8_4.Text.Equals("") && Convert.ToDouble(textBox2_8_4.Text) > 9999.9999)
            {
                MessageBox.Show("The Stock Amount exceeds the upper limit, it should be less than or equal to 9999.9999!", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else
            {
                //声明一个变量
                Plu plu = new Plu();
                //变量赋值
                plu.Number = textBlock2_8_1.Text;
                plu.Name = textBox2_8_1.Text;
                plu.Barcode = textBox2_8_2.Text;
                plu.Price = Math.Round(Convert.ToDouble(textBox2_8_3.Text) * 100, 0);
                plu.Tax_Index = textBlock2_8_4.Text.ToString();
                if (checkBox2_8_1.IsChecked == true) { plu.Stock_Control = 1; } else { plu.Stock_Control = 0; };
                if (textBox2_8_4.Text.Equals(""))
                {
                    plu.Stock_Amount = 0;
                }
                else
                {
                    plu.Stock_Amount = Math.Round(Convert.ToDouble(textBox2_8_4.Text) * 10000, 0);
                }
                if (textBox2_8_5.Text.Equals(""))
                {
                    plu.RRP = 0;
                }
                else
                {
                    plu.RRP = Math.Round(Convert.ToDouble(textBox2_8_5.Text) * 100, 0);
                }
                //SQL查询语句
                string sql_current = "SELECT Abbreviation FROM Currency_Table WHERE Current=1";
                //SQLite数据库，此处连接的是programmingDB
                sqliteDBHelper_PRO = new SQLiteDBHelper(dataBase_PRO);
                //执行查询，结果为DataTable类型
                DataTable dt = sqliteDBHelper_PRO.ExecuteDataTable(sql_current, null);
                //判断结果
                if (dt.Rows[0]["Abbreviation"].ToString() == "" || dt.Rows[0]["Abbreviation"] == null)//若为空表
                {

                    plu.Currency = "";
                }
                else
                {
                    plu.Currency = dt.Rows[0]["Abbreviation"].ToString();
                }
                plu.Used = "1";
                //SQL语句
                string sql = "INSERT INTO Goods_Info (Number,Name,Barcode,Price,RRP,Tax_Index,Stock_Control,Stock_Amount,Currency,Used)"
                    + " VALUES(@Number,@Name,@Barcode,@Price,@RRP,@Tax_Index,@Stock_Control,@Stock_Amount,@Currency,@Used)";
                //配置SQL语句里的参数
                SQLiteParameter[] parameters = {
                    new SQLiteParameter("@Number",plu.Number),
                    new SQLiteParameter("@Name",plu.Name),
                    new SQLiteParameter("@Barcode",plu.Barcode),
                    new SQLiteParameter("@Price",plu.Price),
                    new SQLiteParameter("@RRP",plu.RRP),
                    new SQLiteParameter("@Tax_Index",plu.Tax_Index),
                    new SQLiteParameter("@Stock_Control",plu.Stock_Control),
                    new SQLiteParameter("@Stock_Amount",plu.Stock_Amount),
                    new SQLiteParameter("@Currency",plu.Currency),
                    new SQLiteParameter("@Used",plu.Used),
                };
                //执行SQL
                try
                {
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
