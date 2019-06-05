using System.Collections.Generic;
using System.Data;
using System.Linq;
/// <summary>
/// MySqlIntegration 是对mysql数据库进行操作的工具类
/// </summary>
namespace MySql.Data.MySqlClient
{
    public class MySqlIntegration
    {
        //private static readonly string connStr = ;
        private readonly MySqlConnection connection;
        private string queryString;
        public string QueryString
        {
            get { return queryString; }
        }

        public List<Dictionary<string, object>> IntegratedResult { get; private set; }

        public MySqlIntegration(string connString)
        {
            connection = MySqlconnect(connString);
        }

        /**
         * 返回一个mysql连接
         * */
        private MySqlConnection MySqlconnect(string connString)
        {
            MySqlConnection conn = null;
            try
            {
                conn = new MySqlConnection(connString);
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
        private void AlterConnection(bool state)
        {
            try
            {
                if (state)
                    connection.Open();
                else
                    connection.Close();
            }
            catch (MySqlException e)
            {
                throw e;
            }
        }

        /**
         * 进行SELECT操作
         * */
        public List<Dictionary<string, object>>/*MySqlDataReader */ MySqlSelect(string table, string[] keys, string specifier = null)
        {
            if (connection.State == ConnectionState.Open) return null;
            AlterConnection(true);
            string queryStr = "SELECT {0} FROM {1} {2}";
            queryStr = string.Format(queryStr, MakeStr(keys, ignore: true), table, (specifier != null && specifier.Trim() != "") ? "WHERE " + specifier : "");
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

            List<Dictionary<string, object>> result = GetResult(mySqlDataReader);

            AlterConnection(false);

            IntegratedResult = result;
            return result;
            //return mySqlDataReader;
        }

        /**
         * 进行INSERT INTO操作
         * */
        public int /*MySqlDataReader*/ MySqlInsert(string table, string[] keys, params object[] vals)
        {
            if (connection.State == ConnectionState.Open) return 0;
            AlterConnection(true);
            string queryStr = "INSERT INTO {0} ({1}) VALUES ({2})";
            queryStr = string.Format(queryStr, table, MakeStr(keys, ignore: true), MakeStr(vals));
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

            AlterConnection(false);
            return result;
            //return mySqlDataReader;
        }

        /**
         * 进行UPDATE操作
         * */
        public int /*MySqlDataReader */ MySqlUpdate(string table, string[] keys, string specifier, params object[] vals)
        {
            if (connection.State == ConnectionState.Open) return 0;
            AlterConnection(true);

            string queryStr = "UPDATE {0} SET {1} {2}";
            string kvpsStr = "";
            foreach (var item in keys.Select((value, i) => new { i, value }))
            {
                object[] temp = { vals[item.i] };
                kvpsStr += item.value + "=" + MakeStr(temp) + ",";
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
            catch (MySqlException e)
            {
                throw e;
            }

            int result = mySqlDataReader.RecordsAffected;
            mySqlDataReader.Close();

            AlterConnection(false);
            return result;
            //return mySqlDataReader;
        }

        /**
         * 将指定数据删除
         * */
        public int /*MySqlDataReader*/ MySqlDelete(string table, string specifier = null)
        {
            if (connection.State == ConnectionState.Open) return 0;
            AlterConnection(true);
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

            AlterConnection(false);
            return result;
            //return mySqlDataReader;
        }

        /**
         * 将MySqlDataReader对象读取为二维数组
         * */
        public List<Dictionary<string, object>> GetResult(MySqlDataReader mySqlDataReader)
        {
            if (connection.State == ConnectionState.Closed) return null;
            if (mySqlDataReader != null)
            {
                //判断列数
                List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
                try
                {
                    while (mySqlDataReader.Read())
                    {
                        Dictionary<string, object> tempRow = new Dictionary<string, object>();
                        for (int i = 0; i < mySqlDataReader.FieldCount; i++)
                        {
                            var val = mySqlDataReader.GetValue(i);
                            /*string str;
                            if (val.GetType() == typeof(DateTime))
                                str = ((DateTime)val).ToString("yyyy-MM-dd HH:mm:ss");
                            else
                                str = val.ToString();*/
                            tempRow.Add(mySqlDataReader.GetName(i), val);
                        }
                        result.Add(tempRow);
                    }
                }
                catch (MySqlException e)
                {
                    throw e;
                }
                mySqlDataReader.Close();
                IntegratedResult = result;
                return result;
            }
            IntegratedResult = null;
            return null;
        }

        /**
         * 将数组构建为字符串
         * @ignore 为true时无论如何都不会添加引号，为false时将为字符串类型的变量添加引号
         * */
        private static string MakeStr(object[] keys, bool ignore = false)
        {
            string result = "";
            foreach (object obj in keys)
                result += ((!ignore && obj.GetType() == typeof(string)) ? QuoteStr((string)obj) : obj.ToString()) + ",";
            return result.Substring(0, result.Length - 1);
        }

        public static string QuoteStr(string str)
        {
            return "'" + str + "'";
        }
    }
}