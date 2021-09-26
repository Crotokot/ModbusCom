using System;
using System.Collections.Generic;
using System.Linq;
using Modbus.Device;
using System.IO.Ports;
using System.Threading;


namespace ModbusCom
{
    public struct SlaveInfo
    {
        public ushort StartAddr;
        public ushort RegistersCount;
        public string[] RegNames;
    }

    class MasterDevice
    {
        private ModbusSerialMaster master = null;
        private SlaveInfo slaveInfo;

        public MasterDevice(SlaveInfo slvInf, string port = "COM2")
        {
            slaveInfo = slvInf;

            SerialPort serialPort = new SerialPort(port);
            serialPort.BaudRate = 115200;
            serialPort.DataBits = 8;
            serialPort.Parity = Parity.Even;
            serialPort.StopBits = StopBits.One;
            serialPort.Open();

            master = ModbusSerialMaster.CreateRtu(serialPort);
        }

        public IEnumerable<Dictionary<string, string>> AnswerSlave(byte slaveID, int timeout)
        {
            while (true)
            {
                yield return AnswerSlave(slaveID);
                Thread.Sleep(timeout);
            }
        }

        public Dictionary<string, string> AnswerSlave(byte slaveID)
        {
            ushort startAddr = slaveInfo.StartAddr;
            ushort registersCount = slaveInfo.RegistersCount;
            string[] regNames = slaveInfo.RegNames;
            ushort[] holdingRegisters = master.ReadHoldingRegisters(slaveID, startAddr, registersCount);
            var answer = regNames.Zip(holdingRegisters.Select(reg => $"{reg}"), (k, v) => new { k, v });

            return answer.ToDictionary(el => el.k, el => el.v);
        }

        public void ImitateSlaveDevice(byte slaveID, int timeout)
        {
            while (true)
            {
                ImitateSlaveDevice(slaveID);
                Thread.Sleep(timeout);
            }
        }

        public void ImitateSlaveDevice(byte slaveID)
        {
            Random random = new Random();
            ushort regsCount = slaveInfo.RegistersCount,
                startAddr = slaveInfo.StartAddr;
            ushort[] regsValues = new ushort[regsCount];
            for (ushort i = 0; i < regsCount; i++)
                regsValues[i] = Convert.ToUInt16(random.Next() % 100);

            master.WriteMultipleRegisters(slaveID, startAddr, regsValues);
        }

        private void CorrectRegisterNames()
        {
            List<string> correctedRegNames = new List<string>();
            foreach (var regName in slaveInfo.RegNames)
            {
                string correctedName = "Reg4";
                for (int i = regName.Length; i < 4; i++)
                    correctedName += "0";
                correctedName += regName;
                correctedRegNames.Add(correctedName);
            }
            slaveInfo.RegNames = correctedRegNames.ToArray();
        }

        ~MasterDevice()
        {
            master.Dispose();
        }

    }
}
