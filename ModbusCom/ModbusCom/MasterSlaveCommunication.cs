using System.Collections.Generic;
using System.Threading;


namespace ModbusCom
{
    class MasterSlaveCommunication
    {
        MasterDevice master = null;
        public MasterSlaveCommunication(ushort startAddr, ushort regsCount, 
            string[] regNames, string port = "COM2")
        {
            SlaveInfo slaveInfo;
            slaveInfo.StartAddr = startAddr;
            slaveInfo.RegistersCount = regsCount;
            slaveInfo.RegNames = regNames;

            master = new MasterDevice(slaveInfo, port);
        }

        public void Establish(byte slaveID, int slaveTimeout = 7000, int masterTimeout = 5000)
        {          
            void StartMaster(object master) 
            { 
                IEnumerable<Dictionary<string, string>> answersEnum = 
                    (master as MasterDevice).AnswerSlave(slaveID, masterTimeout);
                foreach (var answer in answersEnum)
                    PostQueryMaker.PostQuery(AppConfig.ServiceURL, answer);
            }

            void StartSlave(object master)
                => (master as MasterDevice).ImitateSlaveDevice(slaveID, slaveTimeout);

            Thread masterThread = new Thread(new ParameterizedThreadStart(StartMaster));
            Thread slaveThread = new Thread(new ParameterizedThreadStart(StartSlave));

            masterThread.Start(master);
            slaveThread.Start(master);

            masterThread.Join();
            slaveThread.Join();
        }
    }
}