using System;
using System.Collections.Generic;

namespace ModbusCom
{
    class Program
    {
        static void Main(string[] args)
        {
            Dictionary<string, string[]> columns = new Dictionary<string, string[]>()
            {
                { AppConfig.DeviceRegTblName, new string[] { "Reg1", "Reg2", "Reg3" } }
            };
            DeviceInfoDB dataBase = new DeviceInfoDB(columns);
            Dictionary<string, string> colsData = new Dictionary<string, string>()
            {
                { "Reg1", "Data1" },
                { "Reg2", "Data2" },
                { "Reg3", "Data3" }
            };
            dataBase.RecordData(AppConfig.DeviceRegTblName, colsData);

            System.Data.DataTable dataTable = dataBase.ReadData(AppConfig.DeviceRegTblName);
            foreach (System.Data.DataRow row in dataTable.Rows)
                Console.WriteLine(string.Join(",", row.ItemArray));
            
        }
    }
}
