using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
/// <summary>
/// 连接服务器端的mysql数据库
/// </summary>
namespace CheckProject.MysqlDB
{
    class MysqlDBHelper
    {
        /////////////////////////////////////////////////////////////////////////////////
        #region  建立MySql数据库连接
        /// <summary>
        /// 建立数据库连接
        /// </summary>
        public MySqlConnection getmysqlcon()
        {
            string sqlcon = "server=localhost;user id=root;password=;database=taxmastercomputer;CharSet=utf8;Allow User Variables=True;"; //根据自己的设置  本地电脑测试
            //string M_str_sqlcon = "server=47.93.232.64;Port=3306;user id=root;password=1236547;database=taxmastercomputer;CharSet=utf8;Allow User Variables=True;"; //根据自己的设置 阿里云端
            MySqlConnection myCon = new MySqlConnection(sqlcon);
            return myCon;
        }
        #endregion
        /////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 查询 返回DataTable类型的数据
        /// 参数说明 sql：要执行的查询语句，parameters：执行SQL查询语句所需要的参数，参数必须以它们在SQL语句中的顺序为准
        /// using代码块在尾行的时候自动调用自身的dispose()函数来释放自身资源，无需手动释放，相当于手写的try catch函数
        /// </summary>
        public DataTable ExecuteDataTable(string sql, MySqlParameter[] parameters)
        {
            using (MySqlConnection connection = this.getmysqlcon())//创建连接对象 //使用using语句块，可确保关闭数据库连接
            {
                connection.Open();//打开连接
                using (MySqlCommand command = new MySqlCommand(sql, connection))//创建MySqlCommand对象，参数有两个，一个是需要执行的SQL语句，另一个是数据库连接对象
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);//将参数插入到SQL语句中
                    }
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))//创建DataAdapter数据适配器实例，用于MySqlConnection和MySqlCommand的连接，用以提升连接至MySQL数据库时的性能
                    {
                        DataTable data = new DataTable();
                        adapter.Fill(data);//Fill方法(填充)
                        return data;//返回DataTable类型的数据
                    }
                }
            }
        }
        /////////////////////////////////////////////////////////////////////////////////
        /// <summary> 
        /// 对MySQL数据库执行增删改操作，返回受影响的行数
        /// 参数说明 sql：要执行的查询语句，parameters：执行SQL查询语句所需要的参数，参数必须以它们在SQL语句中的顺序为准
        /// </summary> 
        public int ExecuteNonQuery(string sql, MySqlParameter[] parameters)
        {
            int affectedRows = 0;

            using (MySqlConnection connection = this.getmysqlcon())//创建连接对象
            {
                connection.Open();//打开连接
                using (MySqlTransaction transaction = connection.BeginTransaction())//开启事务
                {
                    using (MySqlCommand command = new MySqlCommand(sql, connection, transaction))//创建MySqlCommand对象
                    {
                        //command.CommandText = sql;
                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);//将参数插入到SQL语句中
                        }
                        affectedRows = command.ExecuteNonQuery();//执行MySqlCommand
                    }
                    transaction.Commit();//提交事务
                }
                //connection.Close();//关闭连接
            }
            return affectedRows;//返回受影响的行数
        }
        /////////////////////////////////////////////////////////////////////////////////
        /// <summary> 
        /// 对MySQL数据库执行增删改操作，返回受影响的行数 参数为集合  一个事务中包含多条sql语句
        /// 参数说明 sql：要执行的查询语句，parameters：执行SQL查询语句所需要的参数，参数必须以它们在SQL语句中的顺序为准
        /// </summary> 
        public int ExecuteNonQueryList(string sql, List<MySqlParameter[]> parametersList)
        {
            int affectedRows = 0;

            using (MySqlConnection connection = this.getmysqlcon())//创建连接对象
            {
                connection.Open();//打开连接
                using (MySqlTransaction transaction = connection.BeginTransaction())//开启事务
                {
                    using (MySqlCommand command = transaction.Connection.CreateCommand())//创建MySqlCommand对象
                    {
                        //command.Transaction = transaction;transaction.Connection.CreateCommand
                        command.CommandText = sql;
                        for (int i = 0; i < parametersList.Count; i++)
                        {
                            if (parametersList[i] != null)
                            {
                                command.Parameters.AddRange(parametersList[i]);//将参数插入到SQL语句中
                            }
                            affectedRows += command.ExecuteNonQuery();//执行MySqlCommand
                            command.Parameters.Clear();//对于mysql数据库，此条为特殊要加上的，每次提交完一条sql语句后，需要及时清除上次的参数
                        }
                    }
                    transaction.Commit();//提交事务
                }
                //connection.Close();//关闭连接
            }
            return affectedRows;//返回受影响的行数
        }
        
    }
}
