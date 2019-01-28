using System;
using System.Collections.Generic;
using System.Threading;
using MySql.Data.MySqlClient;

namespace Teknik.GitService
{
    public class MysqlDatabase
    {
        public event EventHandler<string> MysqlErrorEvent;

        private bool Connected { get; set; }
        private MySqlConnection Connection { get; set; }
        private ReaderWriterLockSlim DatabaseLock { get; set; }

        public MysqlDatabase(string server, string database, string username, string password, int port)
        {
            Connected = false;
            Connection = null;
            DatabaseLock = new ReaderWriterLockSlim();
            Connect(server, database, username, password, port);
        }

        public List<Dictionary<string, object>> Query(string query, params object[] args)
        {
            List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
            if (Connected)
            {
                DatabaseLock.EnterWriteLock();
                MySqlCommand cmd = PrepareQuery(query, args);
                try
                {
                    MySqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        Dictionary<string, object> row = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            row.Add(reader.GetName(i), reader.GetValue(i));
                        }
                        rows.Add(row);
                    }
                    reader.Close();
                }
                catch (MySqlException exception)
                {
                    if (MysqlErrorEvent != null)
                    {
                        MysqlErrorEvent(this, exception.Message);
                    }
                }
                catch (Exception exception)
                {
                    if (MysqlErrorEvent != null)
                    {
                        MysqlErrorEvent(this, exception.Message);
                    }
                }
                DatabaseLock.ExitWriteLock();
            }
            return rows;
        }

        public object ScalarQuery(string query, params object[] args)
        {
            if (Connected)
            {
                DatabaseLock.EnterWriteLock();
                MySqlCommand cmd = PrepareQuery(query, args);
                object result = null;
                try
                {

                    result = cmd.ExecuteScalar();
                }
                catch (MySqlException exception)
                {
                    if (MysqlErrorEvent != null)
                    {
                        MysqlErrorEvent(this, exception.Message);
                    }
                }
                catch (Exception exception)
                {
                    if (MysqlErrorEvent != null)
                    {
                        MysqlErrorEvent(this, exception.Message);
                    }
                }
                DatabaseLock.ExitWriteLock();
                return result;
            }
            return null;
        }

        public void Execute(string query, params object[] args)
        {
            if (Connected)
            {
                DatabaseLock.EnterWriteLock();
                MySqlCommand cmd = PrepareQuery(query, args);
                try
                {
                    int result = cmd.ExecuteNonQuery();
                }
                catch (MySqlException exception)
                {
                    if (MysqlErrorEvent != null)
                    {
                        MysqlErrorEvent(this, exception.Message);
                    }
                }
                catch (Exception exception)
                {
                    if (MysqlErrorEvent != null)
                    {
                        MysqlErrorEvent(this, exception.Message);
                    }
                }
                DatabaseLock.ExitWriteLock();
            }
        }

        private void Connect(string server, string database, string username, string password, int port)
        {
            if (Connection == null)
            {
                if (!string.IsNullOrEmpty(server) && !string.IsNullOrEmpty(database) && !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                {
                    string strCon = string.Format("Server={0}; database={1}; user={2}; password={3}; port={4}; charset=utf8; Allow Zero Datetime=true;", server, database, username, password, port);
                    Connection = new MySqlConnection(strCon);
                    try
                    {
                        Connection.Open();
                        Connected = true;
                    }
                    catch (MySqlException)
                    {
                        Connected = false;
                    }
                }
            }
        }

        private void Disconnect()
        {
            if (Connection != null && Connected)
            {
                Connected = false;
                Connection.Close();
            }
        }

        private MySqlCommand PrepareQuery(string query, object[] args)
        {
            if (Connected)
            {
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = Connection;
                for (int i = 0; i < args.Length; i++)
                {
                    string param = "{" + i + "}";
                    string paramName = "@DBVar_" + i;
                    query = query.Replace(param, paramName);
                    cmd.Parameters.AddWithValue(paramName, args[i]);
                }
                cmd.CommandText = query;
                return cmd;
            }
            return null;
        }
    }
}