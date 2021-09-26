using System.Collections.Generic;
using System.IO;
using System.Net.Http;

namespace ModbusCom
{
    class AppConfig
    {
        private static string projectPath = GetProjectPath();
        private static string DataBasePath = projectPath + @"ModbusCom\DeviceInfo.mdf";
        public static string TablePagePath = projectPath + @"Data\TablePage.html";
        //if database coudn't be find insert here your connection string
        public static string ConnectionString = 
         $@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename={DataBasePath};Integrated Security=True";


        public static string DropTableQuery = "DROP TABLE IF EXISTS {0}";
        public static string CreateTableQuery = "CREATE TABLE {0} ({1})";
        public static string ReadFromTableQuery = "SELECT {0} FROM {1}";
        public static string RecordToTableQuery = "INSERT INTO {0} ({1}) VALUES ({2})";

        public static string DeviceRegTblName = "DeviceRegisters";

        public static string ServiceURL = "http://localhost:8080/";
        public static Dictionary<string, int> MethodsTypes = new Dictionary<string, int>()
        {
            { HttpMethod.Get.ToString(), 0 },
            { HttpMethod.Post.ToString(), 1 }
        };

        private static string GetProjectPath()
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            int length = currentDirectory.IndexOf("ModbusCom");
            string projectPath = currentDirectory.Substring(0, length) + @"ModbusCom\ModbusCom\";
            return projectPath;
        }
    }
}