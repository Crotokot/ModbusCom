using System;
using System.Collections.Generic;
using System.Threading;


namespace ModbusCom
{
    class Program
    {
        static void Main(string[] args)
        {
            ushort startAddr = 0, regsCount = 10;
            if (args.Length == 2)
            {
                try
                {
                    startAddr = Convert.ToUInt16(args[0]);
                    regsCount = Convert.ToUInt16(args[1]);
                }
                catch (FormatException)
                {
                    Console.WriteLine($"Invalid format.");
                }
                catch (OverflowException)
                {
                    Console.WriteLine($"Value is too big. Values must be ushort types.");
                }
            }
            Console.WriteLine($"startAddr was set to {startAddr} and regsCount was set to {regsCount}.");            

            RunApplication(startAddr, regsCount);
        }

        private static string[] CorrectColNames(string[] regNames)
        {
            List<string> correctedRegNames = new List<string>();
            foreach (var regName in regNames)
            {
                string correctedName = "Reg4";
                for (int i = regName.Length; i < 4; i++)
                    correctedName += "0";
                correctedName += regName;
                correctedRegNames.Add(correctedName);
            }
            return correctedRegNames.ToArray();
        }

        public static void RunApplication(ushort startAddr, ushort registersCount)
        {
            void StartServer(object server) => (server as HttpServer).Start();

            List<string> regNames = new List<string>();
            for (ushort regNum = (ushort)(startAddr + 1); regNum <= startAddr + registersCount; regNum++)
                regNames.Add($"{regNum}");
            Dictionary<string, string[]> colsData = new Dictionary<string, string[]>()
            {
                {   AppConfig.DeviceRegTblName, CorrectColNames(regNames.ToArray()) },
            };
            HttpServer server = new HttpServer(colsData);
            Thread serverThread = new Thread(new ParameterizedThreadStart(StartServer));
            serverThread.Start(server);

            MasterSlaveCommunication connection = 
                new MasterSlaveCommunication(startAddr, registersCount, CorrectColNames(regNames.ToArray()), "COM6");
            connection.Establish(1);
        }
    }
}
