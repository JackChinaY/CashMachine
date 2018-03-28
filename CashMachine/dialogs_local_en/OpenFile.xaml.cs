﻿using CashMachine.SQLiteDB;
using CheckUtils;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Windows;

namespace CashMachine
{
    /// <summary>
    /// OpenFile.xaml 的交互逻辑
    /// </summary>
    public partial class OpenFile : Window
    {
        //设置的时间和Z号码参数
        public string fileName { get; set; }
        public bool headFlag { get; set; }
        public bool flag { get; set; }//确认是否点击了提交按钮
        public string dataBase { get; set; }
        //声明一个SQLite数据库
        SQLiteDBHelper sqliteDBHelper = null;
        //无参构造函数
        public OpenFile()
        {
            InitializeComponent();
            this.fileName = "";
            this.headFlag = true;
            this.flag = false;
            this.ResizeMode = ResizeMode.CanMinimize;//禁用最大化按钮
        }
        //有参构造函数
        public OpenFile(string goodsDB)
        {
            InitializeComponent();
            this.fileName = "";
            this.dataBase = goodsDB;
            this.headFlag = true;
            this.flag = false;
            this.ResizeMode = ResizeMode.CanMinimize;//禁用最大化按钮
        }
        /// <summary>
        /// 选择文件
        /// </summary>
        private void button2_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;//该值确定是否可以选择多个文件
            dialog.Title = "Please select the file";
            dialog.Filter = "excel(*.xls,*.xlsx*)|*.xls;*.xlsx";
            if (dialog.ShowDialog() == true)
            {
                textBox1.Text = dialog.FileName;
            }
        }
        /// <summary>
        /// 提交保存数据
        /// </summary>
        private void button_Click(object sender, RoutedEventArgs e)
        {
            //判断输入框内容是否为空
            if (textBox1.Text.Equals(""))
            {
                MessageBox.Show("Please select the file!", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            //不为空时
            else
            {
                DataTable dt = OperationExcel.ExcelToDataTable(textBox1.Text, true);
                //SQLite数据库，此处连接的是goodsDB
                sqliteDBHelper = new SQLiteDBHelper(dataBase);
                int number_max = 1;
                //查询商品编号
                //SQL语句
                string sql_max = "SELECT MAX(Number) AS MAXNUM FROM Goods_Info";
                //执行查询，结果为DataTable类型
                DataTable dt_max = sqliteDBHelper.ExecuteDataTable(sql_max, null);
                //判断结果
                if (dt_max.Rows[0]["MAXNUM"].ToString() == "" || dt_max.Rows[0]["MAXNUM"] == null)//若为空表，则序号从1开始
                {

                    number_max = 1;
                }
                else
                {
                    number_max = (Convert.ToInt32(dt_max.Rows[0]["MAXNUM"].ToString()) + 1);//最大值加1
                }
                //插入数据
                //SQL语句
                string sql = "INSERT INTO Goods_Info (Number,Name,Barcode,Price,RRP,Tax_Index,Stock_Control,Stock_Amount,Currency,Used)"
                    + " VALUES(@Number,@Name,@Barcode,@Price,@RRP,@Tax_Index,@Stock_Control,@Stock_Amount,@Currency,@Used)";
                //声明一个集合
                List<SQLiteParameter[]> parametersList = new List<SQLiteParameter[]>();
                //受影响行数
                int p = 0;
                //循环插入多条数据到本地数据库
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    try
                    {
                        //配置SQL语句里的参数
                        SQLiteParameter[] parameters = {
                        new SQLiteParameter("@Number",number_max + i),
                        new SQLiteParameter("@Name",dt.Rows[i]["Name"].ToString()),
                        new SQLiteParameter("@Barcode",dt.Rows[i]["Barcode"].ToString()),
                        new SQLiteParameter("@Price",dt.Rows[i]["Price"].ToString()),
                        new SQLiteParameter("@RRP",dt.Rows[i]["RRP"].ToString()),
                        new SQLiteParameter("@Tax_Index",dt.Rows[i]["Tax_Index"].ToString()),
                        new SQLiteParameter("@Stock_Control",dt.Rows[i]["Stock_Control"].ToString()),
                        new SQLiteParameter("@Stock_Amount",dt.Rows[i]["Stock_Amount"].ToString()),
                        new SQLiteParameter("@Currency",dt.Rows[i]["Currency"].ToString()),
                        new SQLiteParameter("@Used",dt.Rows[i]["Used"].ToString()),
                        };
                        parametersList.Add(parameters);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Import failure, the data format is wrong!", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                    }
                }
                //执行SQL，并判断成功与否
                p = sqliteDBHelper.ExecuteNonQueryList(sql, parametersList);

                if (dt.Rows.Count == p)
                {
                    MessageBox.Show("Import successfully, total：" + p + "  records！", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
                //else
                //{
                //    MessageBox.Show("Import failure!", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                //}
            }
        }
    }
}
