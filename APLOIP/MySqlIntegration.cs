using System;
using System.Data;
using System.Configuration;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics;

/// <summary>
/// MySqlIntegration 是对mysql数据库进行操作的工具类
/// </summary>
namespace MySql.Data.MySqlClient
{
    public class MySqlIntegration
    {
        private static string connStr = ConfigurationManager.ConnectionStrings["mysqlConnectionString"].ConnectionString;
        private MySqlConnection connection;
        private string queryString;
        public string QueryString
        {
            get { return queryString; }
        }

        public MySqlIntegration()
        {
            connection = mySqlconnect();
        }

        /**
         * 返回一个mysql连接
         * */
         private MySqlConnection mySqlconnect()
        {
            MySqlConnection conn = null;
            try
            {
                conn = new MySqlConnection(ConfigurationManager.ConnectionStrings["mysqlConnectionString"].ConnectionString);
            }
            catch (MySqlException e)
            {
                throw e;
            }
            return conn;
        }

        /**
         * 关闭当前integration的连接（需手动操作）
         * */
        private void alterConnection(bool state)
        {
            try
            {
                if (state)
                    connection.Open();
                else
                    connection.Close();
            }
            catch(MySqlException e)
            {
                throw e;
            }
        }

        /**
         * 进行SELECT操作
         * */
        public List<Dictionary<string, string>>/*MySqlDataReader */ mySqlSelect(string table, string[] keys, string specifier = null)
        {
            if (connection.State == ConnectionState.Open) return null;
            alterConnection(true);
            string queryStr = "SELECT {0} FROM {1} {2}";
            queryStr = string.Format(queryStr, makeStr(keys), table, (specifier != null && specifier.Trim() != "") ? "WHERE " + specifier : "");
            MySqlDataReader mySqlDataReader = null;
            queryString = queryStr;
            try
            { 
                MySqlCommand mySqlCommand = new MySqlCommand(queryStr, connection);
                mySqlDataReader = mySqlCommand.ExecuteReader();
            }
            catch(MySqlException e)
            {
                throw e;
            }

            List<Dictionary<string, string>> result = getResult(mySqlDataReader);

            alterConnection(false);
            return result;
            //return mySqlDataReader;
        }

        /**
         * 进行INSERT INTO操作
         * */
        public int /*MySqlDataReader*/ mysqlInsert(string table, string[] keys, params string[] vals)
        {
            if (connection.State == ConnectionState.Open) return 0;
            alterConnection(true);
            string queryStr = "INSERT INTO {0} ({1}) VALUES ({2})";
            queryStr = string.Format(queryStr, table, makeStr(keys), makeStr(vals));
            MySqlDataReader mySqlDataReader = null;
            queryString = queryStr;
            try
            {
                MySqlCommand mySqlCommand = new MySqlCommand(queryStr, connection);
                mySqlDataReader = mySqlCommand.ExecuteReader();
            }
            catch(MySqlException e)
            {
                throw e;
            }

            int result = mySqlDataReader.RecordsAffected;
            mySqlDataReader.Close();

            alterConnection(false);
            return result;
            //return mySqlDataReader;
        }

        /**
         * 进行UPDATE操作
         * */
        public int /*MySqlDataReader */ mysqlUpdate(string table, string[] keys, string specifier, params string[] vals)
        {
            if (connection.State == ConnectionState.Open) return 0;
            alterConnection(true);

            string queryStr = "UPDATE {0} SET {1} {2}";
            string kvpsStr = "";
            foreach(var item in keys.Select((value, i) =>  new { i, value } ))
            {
                kvpsStr += item.value + "=" + vals[item.i] + ",";
            }
            kvpsStr = kvpsStr.Substring(0, kvpsStr.Length - 1);
            queryStr = string.Format(queryStr, table, kvpsStr, (specifier != null && specifier.Trim() != "") ? "WHERE " + specifier : "");
            MySqlDataReader mySqlDataReader = null;
            queryString = queryStr;
            System.Diagnostics.Debug.WriteLine(queryStr);
            try
            {
                MySqlCommand mySqlCommand = new MySqlCommand(queryStr, connection);
                mySqlDataReader = mySqlCommand.ExecuteReader();
            }
            catch(MySqlException e)
            {
                throw e;
            }

            int result = mySqlDataReader.RecordsAffected;
            mySqlDataReader.Close();

            alterConnection(false);
            return result;
            //return mySqlDataReader;
        }

        /**
         * 将指定数据删除
         * */
        public int /*MySqlDataReader*/ mySqlDelete(string table, string specifier = null)
        {
            if (connection.State == ConnectionState.Open) return 0;
            alterConnection(true);
            string queryStr = "DELETE FROM {0} {1}";
            queryStr = string.Format(queryStr, table, (specifier != null && specifier.Trim() != "") ? "WHERE " + specifier : "");
            MySqlDataReader mySqlDataReader = null;
            queryString = queryStr;
            try
            {
                MySqlCommand mySqlCommand = new MySqlCommand(queryStr, connection);
                mySqlDataReader = mySqlCommand.ExecuteReader();
            }
            catch (MySqlException e)
            {
                throw e;
            }

            int result = mySqlDataReader.RecordsAffected;
            mySqlDataReader.Close();

            alterConnection(false);
            return result;
            //return mySqlDataReader;
        }

        /**
         * 将MySqlDataReader对象读取为二维数组
         * */
        public List<Dictionary<string, string>> getResult(MySqlDataReader mySqlDataReader)
        {
            if (connection.State == ConnectionState.Closed) return null;
            if (mySqlDataReader != null)
            {
                //判断列数
                List<Dictionary<string, string>> result = new List<Dictionary<string, string>>();
                try
                { 
                    while (mySqlDataReader.Read())
                    {
                        Dictionary<string, string> tempRow = new Dictionary<string, string>();
                        for (int i = 0; i < mySqlDataReader.FieldCount; i++)
                        {
                            var val = mySqlDataReader.GetValue(i);
                            string str;
                            if (val.GetType() == typeof(DateTime))
                                str = ((DateTime)val).ToString("yyyy-MM-dd HH:mm:ss");
                            else
                                str = val.ToString();
                            tempRow.Add(mySqlDataReader.GetName(i), str);
                        }
                        result.Add(tempRow);
                    }
                }
                catch(MySqlException e)
                {
                    throw e;
                }
                mySqlDataReader.Close();
                return result;
            }
            return null;
        }

        /**
         * 将数组构建为字符串
         * */
        private static string makeStr(string[] keys)
        {
            string result = "";
            foreach(string str in keys)
                result += str + ",";
            return result.Substring(0, result.Length - 1);
        }

        public static string quoteStr(string str)
        {
            return "'" + str + "'";
        }
    }
}