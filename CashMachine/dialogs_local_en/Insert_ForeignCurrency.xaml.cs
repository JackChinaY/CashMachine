using CashMachine.entity_local;
using CashMachine.SQLiteDB;
using System;
using System.Windows;
using System.Data.SQLite;

namespace CashMachine.dialogs_local_en
{
    /// <summary>
    /// Insert_ForeignCurrency.xaml 的交互逻辑
    /// </summary>
    public partial class Insert_ForeignCurrency : Window
    {
        //声明一个变量
        public string dataBase;
        //声明一个SQLite数据库
        SQLiteDBHelper sqliteDBHelper = null;

        //无参构造函数
        public Insert_ForeignCurrency()
        {
            InitializeComponent();
        }

        //有参构造函数
        public Insert_ForeignCurrency(string dataBase)
        {
            this.dataBase = dataBase;//赋值
            InitializeComponent();//初始化窗体组件
            this.ResizeMode = ResizeMode.CanMinimize;//禁用“最大化”按钮
        }

        /// <summary>
        /// 提交保存数据
        /// </summary>
        private void button2_7_1_Click(object sender, RoutedEventArgs e)
        {
            //判断输入框内容是否为空
            if (comboBox2_7_1.Text.Equals("请选择外币编号"))
            {
                MessageBox.Show("请选择外币编号!", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            else if (textBox2_7_2.Text.Equals(""))
            {
                MessageBox.Show("外币缩写为必填项!", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            //不为空时
            else
            {
                //声明一个变量
                ForeignCurrency foreignCurrency = new ForeignCurrency();
                //变量赋值
                foreignCurrency.Number = comboBox2_7_1.Text;
                foreignCurrency.Code = textBox2_7_1.Text;
                foreignCurrency.Abbreviation = textBox2_7_2.Text;
                foreignCurrency.Symbol = textBox2_7_3.Text;
                foreignCurrency.Symbol_Direction = textBox2_7_4.Text;
                foreignCurrency.Thousand_Separator = textBox2_7_5.Text;
                foreignCurrency.Cent_Separator = textBox2_7_6.Text;
                foreignCurrency.Decimal_Places = textBox2_7_7.Text;
                foreignCurrency.Exchange_Rate = Convert.ToDouble(textBox2_7_8.Text) * 10000;
                if (checkBox2_7_1.IsChecked == true) { foreignCurrency.Flag = 1; } else { foreignCurrency.Flag = 0; };

                //Console.WriteLine(discount.ToString());
                //SQL语句
                string sql = "INSERT INTO Currency_Table (Number,Code,Abbreviation,Symbol,Symbol_Direction,Thousand_Separator,Cent_Separator,Decimal_Places,Exchange_Rate,Flag)"
                    + " VALUES(@Number,@Code,@Abbreviation,@Symbol,@Symbol_Direction,@Thousand_Separator,@Cent_Separator,@Decimal_Places,@Exchange_Rate,@Flag)";
                //配置SQL语句里的参数
                SQLiteParameter[] parameters = {
                    new SQLiteParameter("@Number", foreignCurrency.Number),
                    new SQLiteParameter("@Code",foreignCurrency.Code),
                    new SQLiteParameter("@Abbreviation",foreignCurrency.Abbreviation),
                    new SQLiteParameter("@Symbol",foreignCurrency.Symbol),
                    new SQLiteParameter("@Symbol_Direction",foreignCurrency.Symbol_Direction),
                    new SQLiteParameter("@Thousand_Separator",foreignCurrency.Thousand_Separator),
                    new SQLiteParameter("@Cent_Separator",foreignCurrency.Cent_Separator),
                    new SQLiteParameter("@Decimal_Places",foreignCurrency.Decimal_Places),
                    new SQLiteParameter("@Exchange_Rate",foreignCurrency.Exchange_Rate),
                    new SQLiteParameter("@Flag",foreignCurrency.Flag),
                };
                //声明一个sqlite数据库
                sqliteDBHelper = new SQLiteDBHelper(dataBase);
                //执行SQL
                try
                {
                    //执行成功时
                    if (sqliteDBHelper.ExecuteNonQuery(sql, parameters) == 1)
                    {
                        //弹出提示框
                        MessageBox.Show("数据提交至本地数据库成功!", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                        //关闭弹出框
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("数据提交失败!", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                //执行失败时
                catch (Exception ee)
                {
                    //弹出提示框
                    MessageBox.Show("数据提交失败!可能原因：" + ee.Message, "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
    }
}
