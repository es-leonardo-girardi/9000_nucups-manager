// dotnet publish 9000_nucups-manager.csproj --runtime linux-x64 -o publish -p:PublishSingleFile=true --self-contained true -p:IncludeNativeLibrariesForSelfExtract=true

using Newtonsoft.Json;
using System.Diagnostics;

namespace NucUpsManager
{
    public class Cell
    {
        [JsonProperty("detected")]
        public bool Detected { get; set; }
        [JsonProperty("balanceOn")]
        public bool BalanceOn { get; set; }
        [JsonProperty("voltage")]
        public float Voltage { get; set; }
        [JsonProperty("temperature")]
        public float Temperature { get; set; }
    }
    public class TimersStruct
    {
        [JsonProperty("onBattery")]
        public uint OnBattery { get; set; }
        [JsonProperty("hardOff")]
        public uint HardOff { get; set; }
        [JsonProperty("chargeGlobal")]
        public uint ChargeGlobal { get; set; }
        [JsonProperty("topping")]
        public uint Topping { get; set; }
    }
    public class StatusStruct
    {
        [JsonProperty("inBattery")]
        public bool InBattery { get; set; }
        [JsonProperty("inAlert")]
        public bool InAlert { get; set; }
        [JsonProperty("inShutdown")]
        public bool InShutdown { get; set; }
        [JsonProperty("ups")]
        public StateInfo Ups { get; set; }
        [JsonProperty("charging")]
        public StateInfo Charging { get; set; }
    }
    public class StateInfo
    {
        [JsonProperty("value")]
        public uint Value { get; set; }
        [JsonProperty("text")]
        public string Text { get; set; }
    }
    public class UpsData
    {
        [JsonProperty("level")]
        public int Level { get; set; }
        [JsonProperty("inVoltage")]
        public float InVoltage { get; set; }
        [JsonProperty("outVoltage")]
        public float OutVoltage { get; set; }
        [JsonProperty("outCurrent")]
        public float OutCurrent { get; set; }
        [JsonProperty("outPower")]
        public float OutPower { get; set; }
        [JsonProperty("batteryVoltage")]
        public float BatteryVoltage { get; set; }
        [JsonProperty("batteryCurrent")]
        public float BatteryCurrent { get; set; }
        [JsonProperty("batteryPack")]
        public float BatteryPack { get; set; }
        [JsonProperty("batteryCells")]
        public List<Cell> BatteryCells { get; set; } = new ();
        [JsonProperty("timers")]
        public TimersStruct Timers { get; set; } = new();
        [JsonProperty("status")]
        public StatusStruct Status { get; set; } = new();

        public UpsData() { }
    }

    public class Manager
    {
        static void Main(string[] args)
        {            
            var strOutput = "";
            var NucUps = new UpsData();
            var readDataInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                Arguments = string.Format("-c \"sudo ./openups -t nucups -s\"")
            };
            var exeScriptInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                UseShellExecute = false,
                Arguments = string.Format($"-c \"sudo {args[0]}\"")
            };

            while (true)
            {
                using (var p = Process.Start(readDataInfo))
                {
                    if (p != null)
                    {
                        strOutput = p.StandardOutput.ReadToEnd();
                        p.WaitForExit();
                    }
                    if (!string.IsNullOrEmpty(strOutput))
                    {
                        NucUps = JsonConvert.DeserializeObject<UpsData>(strOutput); ;
                    }
                }

                Console.WriteLine("OnBattery: {0}, Level: {1}%", NucUps.Status.InBattery, NucUps.Level);

                if (NucUps.Status.InShutdown == true)
                {
                    Console.WriteLine($"Shutdown flag: {NucUps.Status.InShutdown}");
                    break;
                }

                Thread.Sleep(500);
            }

            Console.WriteLine("in shutdown...");
            Console.WriteLine(exeScriptInfo.Arguments);
            Process.Start(exeScriptInfo);
        }
    } 
}