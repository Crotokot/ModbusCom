using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;


namespace ModbusCom
{
    class Program
    {
        static void Main(string[] args)
        {
            TestModbusDevice();
        }

        public static void TestModbusDevice()
        {
            void StartServer(object obj) => (obj as HttpServer).Start();

            ushort startAddr = 100;
            ushort registersCount = 3;
            string[] regNames = new string[] { "Reg1", "Reg2", "Reg3" };
            Dictionary<string, string[]> colsData = new Dictionary<string, string[]>()
            {
                {   AppConfig.DeviceRegTblName, regNames },
            };
            HttpServer server = new HttpServer(colsData);
            Thread serverThread = new Thread(new ParameterizedThreadStart(StartServer));
            serverThread.Start(server);

            MasterSlaveCommunication connection = 
                new MasterSlaveCommunication(startAddr, registersCount, regNames, "COM6");
            connection.Establish(1);
        }

        public static void TestHttpServer()
        {
            void StartServer(object obj) => (obj as HttpServer).Start();
            async Task<HttpResponseMessage> GetRequest(string address, Dictionary<string, string> queryParams)
            {
                HttpClient client = new HttpClient();

                try
                {
                    FormUrlEncodedContent content = new FormUrlEncodedContent(queryParams);
                    return await client.PostAsync(address, content);
                }
                catch (Exception)
                {
                    Console.WriteLine("Anything was wrong!");
                }
                finally
                {
                    client.Dispose();
                }
                return null;
            }

            Dictionary<string, string[]> columns = new Dictionary<string, string[]>()
            {
                { AppConfig.DeviceRegTblName, new string[] { "Reg1", "Reg2", "Reg3" } }
            };
            HttpServer server = new HttpServer(columns);
            Thread serverThread = new Thread(new ParameterizedThreadStart(StartServer));
            serverThread.Start(server);

            Dictionary<string, string> queryString = new Dictionary<string, string>()
            {
                { "Reg1", "109" },
                { "Reg2", "78" },
                { "Reg3", "75" }
            };

            var answer = GetRequest(AppConfig.ServiceURL, queryString)?.Result;
            if (answer != null)
                Console.WriteLine(answer.Content);
        }
        public static void TestDataBase()
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
