using CashMachine.entity_local;
using CashMachine.SQLiteDB;
using System;
using System.Windows;
using System.Data.SQLite;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using System.Data;

namespace CashMachine.dialogs_local_en
{
    /// <summary>
    /// Update_ForeignCurrency.xaml 的交互逻辑
    /// </summary>
    public partial class Update_ForeignCurrency : Window
    {
        //声明一个变量
        public string dataBase;
        //声明一个变量
        public string dataBase_list;
        //声明一个变量
        public string dataBase_goods;
        //声明一个SQLite数据库
        SQLiteDBHelper sqliteDBHelper = null;
        //声明一个变量
        ForeignCurrency foreignCurrency = new ForeignCurrency();

        //无参构造函数
        public Update_ForeignCurrency()
        {
            InitializeComponent();
        }

        //有参构造函数
        public Update_ForeignCurrency(ForeignCurrency foreignCurrency, string dataBase, string dataBase_list, string dataBase_goods)
        {
            this.foreignCurrency = foreignCurrency;
            this.dataBase = dataBase;
            this.dataBase_list = dataBase_list;
            this.dataBase_goods = dataBase_goods;
            InitializeComponent();//初始化窗体组件
            this.ResizeMode = ResizeMode.CanMinimize;//禁用“最大化”按钮
            Init();
            sqliteDBHelper = new SQLiteDBHelper(dataBase);
        }
        /// <summary>
        /// 窗体数据初始化
        /// </summary>
        public void Init()
        {
            textBlock2_7_1.Text = this.foreignCurrency.Number;
            //comboBox2_7_1.SelectedValue = this.foreignCurrency.Number;
            //textBox2_7_1.Text = this.foreignCurrency.Code;
            textBox2_7_1.Text = this.foreignCurrency.Abbreviation;
            //textBox2_7_3.Text = this.foreignCurrency.Symbol;
            //textBox2_7_4.Text = this.foreignCurrency.Symbol_Direction;
            //textBox2_7_5.Text = this.foreignCurrency.Thousand_Separator;
            //textBox2_7_6.Text = this.foreignCurrency.Cent_Separator;
            //textBox2_7_7.Text = this.foreignCurrency.Decimal_Places;
            textBox2_7_8.Text = this.foreignCurrency.Exchange_Rate.ToString();
            //textBox2_7_8.Text = (Convert.ToDouble(this.foreignCurrency.Exchange_Rate) / 10000).ToString();
            if (this.foreignCurrency.Current == 1)
            {
                checkBox2_7_1.IsChecked = true;
            }
        }
        /// <summary>
        /// 外币浏览按钮
        /// </summary>
        private void button2_7_2_Click(object sender, RoutedEventArgs e)
        {
            //声明一个添加信息的窗体
            Currency_Choice choice = new Currency_Choice(this.dataBase_list);
            //显示对话框
            choice.ShowDialog();
            //商品编号对话框内容赋值,说明已经选择了一个外币
            if (!choice.Number.Equals(string.Empty))
            {
                //如果和当前外币一样
                if (choice.Number == this.foreignCurrency.Abbreviation)
                {
                    //弹出提示框
                    MessageBox.Show("The Abbreviation is the same as the one that you are modifying, but you can submit it again!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                //不一样时判断是否和其他已设外币重复
                else
                {
                    //SQL语句
                    string sql = "SELECT count(*) AS COUNTS FROM Currency_Table WHERE Abbreviation=@Abbreviation";
                    //配置SQL语句里的参数
                    SQLiteParameter[] parameter = {
                    new SQLiteParameter("@Abbreviation",choice.Number),
                };
                    //执行查询，结果为DataTable类型
                    DataTable dt = sqliteDBHelper.ExecuteDataTable(sql, parameter);
                    //判断结果
                    if (Convert.ToInt32(dt.Rows[0]["COUNTS"]) == 0)//若数量为0
                    {
                        textBox2_7_1.Text = choice.Number;
                        this.foreignCurrency.Name = choice.foreignCurrency.Name;
                        this.foreignCurrency.Abbreviation = choice.foreignCurrency.Abbreviation;
                        this.foreignCurrency.Symbol = choice.foreignCurrency.Symbol;
                    }
                    else
                    {
                        //弹出提示框
                        MessageBox.Show("The Abbreviation already existed in the table. Please re-enter it !", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
        }
        /// <summary>
        /// 提交保存数据
        /// </summary>
        private void button2_7_1_Click(object sender, RoutedEventArgs e)
        {
            //判断输入框内容是否为空
            if (textBox2_7_1.Text.Equals(""))
            {
                MessageBox.Show("The Abbreviation is required !", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (textBox2_7_1.Text.Equals("ZMW") && !textBox2_7_8.Text.Equals("1"))
            {
                MessageBox.Show("When you choose the Abbreviation 'ZMW', the Exchange Rate must be '1' !", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (textBox2_7_8.Text.Equals(""))
            {
                MessageBox.Show("The Exchange Rate is required !", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (textBox2_7_8.Text.Equals("0"))
            {
                MessageBox.Show("The Exchange Rate can not be the number '0' !", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (Convert.ToDouble(textBox2_7_8.Text) < 0.0001 || Convert.ToDouble(textBox2_7_8.Text) > 99999.9999)
            {
                MessageBox.Show("The Exchange Rate must be between 0.0001 and 99999.9999!", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            //不为空时
            else
            {
                //Regex regex = new Regex(@"^([A-Z]{3,3})$");//需要加上@
                //if (regex.Matches(textBox2_7_1.Text).Count > 0)
                //{
                //}
                ////如果不匹配
                //else
                //{
                //    MessageBox.Show("The characters of the Abbreviation must be capitalized in A-Z, and the number of it is only 3 characters!", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                //    return;
                //}

                //声明一个sqlite数据库
                //sqliteDBHelper = new SQLiteDBHelper(dataBase);

                //将其它外币Current设置为0
                if (checkBox2_7_1.IsChecked == true)
                {
                    string sql_set = "UPDATE Currency_Table SET Current=0";
                    sqliteDBHelper.ExecuteNonQuery(sql_set, null);
                }

                //提交数据
                //声明一个变量
                //ForeignCurrency foreignCurrency = new ForeignCurrency();
                //变量赋值
                //foreignCurrency.Number = this.foreignCurrency.Number;
                //foreignCurrency.Code = textBox2_7_1.Text;
                //foreignCurrency.Abbreviation = textBox2_7_1.Text;
                //foreignCurrency.Symbol = textBox2_7_3.Text;
                //foreignCurrency.Symbol_Direction = textBox2_7_4.Text;
                //foreignCurrency.Thousand_Separator = textBox2_7_5.Text;
                //foreignCurrency.Cent_Separator = textBox2_7_6.Text;
                //foreignCurrency.Decimal_Places = textBox2_7_7.Text;
                foreignCurrency.Exchange_Rate = Convert.ToDouble(textBox2_7_8.Text) * 10000;
                foreignCurrency.Flag = 1;
                if (checkBox2_7_1.IsChecked == true) { foreignCurrency.Current = 1; } else { foreignCurrency.Current = 0; }

                //Console.WriteLine(discount.ToString());
                //SQL语句
                string sql = "UPDATE Currency_Table SET Name=@Name,Abbreviation=@Abbreviation,Symbol=@Symbol,Exchange_Rate=@Exchange_Rate,Flag=@Flag,Current=@Current WHERE Number=@Number;";
                //配置SQL语句里的参数
                SQLiteParameter[] parameters = {
                    new SQLiteParameter("@Number", foreignCurrency.Number),
                    new SQLiteParameter("@Name",foreignCurrency.Name),
                    new SQLiteParameter("@Abbreviation",foreignCurrency.Abbreviation),
                    new SQLiteParameter("@Symbol",foreignCurrency.Symbol),
                    //new SQLiteParameter("@Symbol_Direction",foreignCurrency.Symbol_Direction),
                    //new SQLiteParameter("@Thousand_Separator",foreignCurrency.Thousand_Separator),
                    //new SQLiteParameter("@Cent_Separator",foreignCurrency.Cent_Separator),
                    new SQLiteParameter("@Exchange_Rate",foreignCurrency.Exchange_Rate),
                    new SQLiteParameter("@Flag",foreignCurrency.Flag),
                    new SQLiteParameter("@Current",foreignCurrency.Current),
                    //new SQLiteParameter("@id",this.foreignCurrency.Id),
                };

                //执行SQL
                try
                {
                    //执行成功时
                    if (sqliteDBHelper.ExecuteNonQuery(sql, parameters) == 1)
                    {
                        //如果当前外币被设置为当前使用的外币，则需要将商品库中的所有商品的Currency设置为当前的外币的缩写
                        if (checkBox2_7_1.IsChecked == true)
                        {
                            SQLiteDBHelper sqliteDBHelper_goods = new SQLiteDBHelper(dataBase_goods);
                            string sql_goods = "UPDATE Goods_Info SET Currency=@Current WHERE Used=1";
                            SQLiteParameter[] parameter = {
                                new SQLiteParameter("@Current", foreignCurrency.Abbreviation)
                            };
                            sqliteDBHelper_goods.ExecuteNonQuery(sql_goods, parameter);
                        }
                        //弹出提示框
                        MessageBox.Show("Submit successfully!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                        //关闭添加框
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
        /// <summary>
        ///  汇率 只允许输入0-9数字 .
        /// </summary>
        private void textBox2_7_8_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
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
        /// 外币缩写
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox2_7_2_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textbox = sender as TextBox;
            //正则判断
            Regex regex = new Regex(@"^([a-zA-Z]{0,3})$");//需要加上@
            if (regex.Matches(textbox.Text).Count > 0)
            {
                //如果误输入00，则换成0
                //if (textbox.Text.Equals("00") || textbox.Text.Equals("01") || textbox.Text.Equals("02") || textbox.Text.Equals("03") || textbox.Text.Equals("04") || textbox.Text.Equals("05") || textbox.Text.Equals("06") || textbox.Text.Equals("07") || textbox.Text.Equals("08") || textbox.Text.Equals("09"))
                //{
                textbox.Text = textbox.Text.ToUpper();
                textbox.SelectionStart = textbox.Text.Length;
                //}
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
    }
}


