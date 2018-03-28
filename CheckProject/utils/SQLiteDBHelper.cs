using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Text;
/// <summary>
/// 连接本地的sqlite数据库
/// </summary>
namespace CashMachine.SQLiteDB
{
    /// <summary>
    /// 说明：这是一个针对System.Data.SQLite的数据库常规操作封装的通用类
    /// </summary>
    public class SQLiteDBHelper
    {
        /////////////////////////////////////////////////////////////////////////////////
        private string connectionString = string.Empty;
        /// <summary>
        /// 无参构造函数
        /// </summary>
        /// <param name="dbPath">SQLite数据库文件路径</param>
        public SQLiteDBHelper()
        {
            //this.connectionString = "Data Source =" + Environment.CurrentDirectory + "/programmingDB.db";//和项目放在一个文件夹中的
            //this.connectionString = "Data Source=" + "E:/DB/dmDB_factory.db";
        }
        /// <summary> 
        /// 有参构造函数 
        /// </summary> 
        /// <param name="dbPath">SQLite数据库文件路径</param>
        public SQLiteDBHelper(string dbPath)
        {
            this.connectionString = "Data Source=" + dbPath;
        }

        /////////////////////////////////////////////////////////////////////////////////
        /// <summary> 
        /// 对SQLite数据库执行增、删、改操作，返回受影响的行数
        /// 参数说明 sql：要执行的查询语句，parameters：执行SQL查询语句所需要的参数，参数必须以它们在SQL语句中的顺序为准
        /// </summary> 
        public int ExecuteNonQuery(string sql, SQLiteParameter[] parameters)
        {
            int affectedRows = 0;
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))//创建连接对象
            {
                connection.Open();//打开连接
                using (SQLiteTransaction transaction = connection.BeginTransaction())//开启事务
                {
                    using (SQLiteCommand command = new SQLiteCommand(connection))//创建SQLiteCommand对象
                    {
                        command.CommandText = sql;
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
        /// 对SQLite数据库执行增、删、改操作，返回受影响的行数  参数为集合  一个事务中包含多条sql语句
        /// 参数说明 sql：要执行的查询语句，parameters：执行SQL查询语句所需要的参数，参数必须以它们在SQL语句中的顺序为准
        /// </summary> 
        public int ExecuteNonQueryList(string sql, List<SQLiteParameter[]> parametersList)
        {
            int affectedRows = 0;
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))//创建连接对象
            {
                connection.Open();//打开连接
                using (SQLiteTransaction transaction = connection.BeginTransaction())//开启事务
                {
                    using (SQLiteCommand command = new SQLiteCommand(connection))//创建SQLiteCommand对象
                    {
                        command.CommandText = sql;
                        for (int i = 0; i < parametersList.Count; i++)
                        {
                            if (parametersList[i] != null)
                            {
                                command.Parameters.AddRange(parametersList[i]);//将参数插入到SQL语句中
                            }
                            affectedRows += command.ExecuteNonQuery();//执行MySqlCommand
                        }
                        
                    }
                    transaction.Commit();//提交事务
                }
                //connection.Close();//关闭连接
            }
            return affectedRows;//返回受影响的行数
        }

        /////////////////////////////////////////////////////////////////////////////////
        /// <summary> 
        /// 执行一个查询语句，返回一个包含查询结果的DataTable
        /// 参数说明 sql：要执行的查询语句，parameters：执行SQL查询语句所需要的参数，参数必须以它们在SQL语句中的顺序为准
        /// using代码块在尾行的时候自动调用自身的dispose()函数(相当于close()函数)来释放自身资源，无需手动释放，相当于手写的try catch函数
        /// </summary> 
        public DataTable ExecuteDataTable(string sql, SQLiteParameter[] parameters)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))//创建连接对象 //使用using语句块，可确保关闭数据库连接
            {
                using (SQLiteCommand command = new SQLiteCommand(sql, connection))//创建MySqlCommand对象，参数有两个，一个是需要执行的SQL语句，另一个是数据库连接对象
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);//将参数插入到SQL语句中
                    }
                    using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(command))//创建DataAdapter数据适配器实例，用于MySqlConnection和MySqlCommand的连接，用以提升连接至MySQL数据库时的性能
                    {
                        DataTable data = new DataTable();
                        adapter.Fill(data);//Fill方法(填充)
                        return data;//返回DataTable类型的数据
                    }
                }
            }
        }
        























































































        /// <summary> 
        /// 创建SQLite数据库文件
        /// </summary> 
        /// <param name="dbPath">要创建的SQLite数据库文件路径</param> 
        public static void CreateDB(string dbPath)
        {
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + dbPath))
            {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    command.CommandText = "CREATE TABLE Demo(id integer NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE)";
                    command.ExecuteNonQuery();
                    command.CommandText = "DROP TABLE Demo";
                    command.ExecuteNonQuery();
                }
            }
        }
        
        
        
        /// <summary> 
        /// 执行一个查询语句，返回查询结果的第一行第一列 
        /// </summary> 
        /// <param name="sql">要执行的查询语句</param> 
        /// <param name="parameters">执行SQL查询语句所需要的参数，参数必须以它们在SQL语句中的顺序为准</param> 
        /// <returns></returns> 
        public Object ExecuteScalar(string sql, SQLiteParameter[] parameters)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }
                    SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
                    DataTable data = new DataTable();
                    adapter.Fill(data);
                    return data;
                }
            }
        }
        /// <summary> 
        /// 查询数据库中的所有数据类型信息
        /// </summary> 
        /// <returns></returns> 
        public DataTable GetSchema()
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                DataTable data = connection.GetSchema("TABLES");
                connection.Close();
                //foreach (DataColumn column in data.Columns) 
                //{ 
                //  Console.WriteLine(column.ColumnName); 
                //} 
                return data;
            }
        }
    }
}