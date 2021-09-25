using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Linq;


namespace ModbusCom
{
    class DeviceInfoDB
    {
        private SqlConnection sqlConnection = null;
        public Dictionary<string, string[]> Columns { get; }

        public DeviceInfoDB(Dictionary<string, string[]> columns)
        {
            Columns = columns;
            sqlConnection = new SqlConnection(AppConfig.ConnectionString);
            ClearDataBase();
            CreateDataBaseTbls();
        }

        private void ClearDataBase()
        {
            try
            {
                List<string> tblNames = GetTblNames();
                SqlCommand sqlCommand = null;
                sqlConnection.Open();
                foreach (var tblName in tblNames)
                {
                    string dropQuery = string.Format(AppConfig.DropTableQuery, tblName);
                    sqlCommand = new SqlCommand(dropQuery, sqlConnection);
                    sqlCommand.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("DataBase couldn't be cleared.");
                throw ex;
            }
            finally
            {
                if (sqlConnection != null && sqlConnection.State == ConnectionState.Open)
                    sqlConnection.Close();
            }
        }

        private void CreateDataBaseTbls()
        {
            try
            {
                sqlConnection.Open();
                SqlCommand sqlCommand = null;
                foreach (var tblCols in Columns)
                {
                    string tblName = tblCols.Key;
                    List<string> colsWithTypes = new List<string>();
                    foreach (var col in tblCols.Value) colsWithTypes.Add(col + " VARCHAR(20)");
                    string colsFormat = string.Join(",", colsWithTypes);
                    string createQuery = string.Format(AppConfig.CreateTableQuery, tblName, colsFormat);
                    sqlCommand = new SqlCommand(createQuery, sqlConnection);
                    sqlCommand.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Couldn't create tables.");
                throw ex;
            }
            finally
            {
                if (sqlConnection != null && sqlConnection.State == ConnectionState.Open)
                    sqlConnection.Close();
            }
        }

        public DataTable ReadData(string tblName, List<string> columns = null)
        {
            DataTable dataTable = null;
            try
            {
                sqlConnection.Open();
                string colsFormat = "*";
                if (columns != null)
                    colsFormat = string.Join(',', columns);
                string selectQuery = string.Format(AppConfig.ReadFromTableQuery,colsFormat,  tblName);
                SqlDataAdapter dataAdapter = new SqlDataAdapter(selectQuery, sqlConnection);
                DataSet dataset = new DataSet();
                dataAdapter.Fill(dataset);
                dataTable = dataset.Tables[0];
            }
            catch (Exception ex)
            {
                Console.WriteLine("Couldn't read from DataBase.");
                throw ex;
            }
            finally
            {
                if (sqlConnection != null && sqlConnection.State == ConnectionState.Open)
                    sqlConnection.Close();
            }
            return dataTable;
        }

        public void RecordData(string tblName, Dictionary<string, string> columnsData)
        {
            try
            {
                sqlConnection.Open();
                string colsFormat = string.Join(',', columnsData.Keys);
                List<string> correctedValues = new List<string>();
                foreach (var val in columnsData.Values) correctedValues.Add("'" + val + "'");
                string valsFormat = string.Join(',', correctedValues);
                string insertQuery = string.Format(AppConfig.RecordToTableQuery, 
                    tblName, colsFormat, valsFormat);
                SqlCommand sqlCommand = new SqlCommand(insertQuery, sqlConnection);
                sqlCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Couldn't record data to DataBase.");
                throw ex;
            }
            finally
            {
                if (sqlConnection != null && sqlConnection.State == ConnectionState.Open)
                    sqlConnection.Close();
            }
        }

        private List<string> GetTblNames()
        {
            List<string> tblNames = null;
            try
            {
                tblNames = new List<string>();
                string selectTblNames = "SELECT name FROM sys.tables";
                sqlConnection.Open();

                SqlCommand sqlCommand = new SqlCommand(selectTblNames, sqlConnection);
                var dataReader = sqlCommand.ExecuteReader();
                while (dataReader.Read())
                    tblNames.Add(dataReader.GetString(0));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw ex;
            }
            finally
            {
                if (sqlConnection != null && sqlConnection.State == ConnectionState.Open)
                    sqlConnection.Close();
            }
            return tblNames;
        }
    }
}