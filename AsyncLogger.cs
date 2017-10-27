using System.Collections.Generic;
using System.IO;
using CsvHelper;
using FridgeSimulator.Utils;
using Microsoft.Azure.Devices.Gateway;
using Newtonsoft.Json;

namespace FridgeSimulator
{
    class CsvData
    {
        public double Temperature { get; set; }
        public double OutsideTemperature { get; set; }
        public double Moisture { get; set; }
        public double Power { get; set; }
        public bool IsDoorOpen { get; set; }
    }

    public class AsyncLogger : IGatewayModule
    {
        private string configuration;
        private static string filePath = "output.csv";
        private double temperature = Initial.TEMPERATURE;
        private double outsideTemperature = Initial.OUTSIDE_TEMP;
        private double moisture = Initial.MOISTURE;
        private double power = Initial.POWER_LOW;
        private bool isDoorOpen = Initial.DOOR_OPEN;

        private CsvWriter csvWriter = new CsvWriter(new StreamWriter(filePath));

        public void Create(Broker broker, byte[] configuration)
        {
            this.configuration = System.Text.Encoding.UTF8.GetString(configuration);
        }

        public void Destroy()
        {
            csvWriter.Flush();
            csvWriter.Context.Dispose();
        }

        public void Start()
        {
            var record = new CsvData
            {
                Temperature = temperature,
                OutsideTemperature = outsideTemperature,
                Power = power,
                Moisture = moisture,
                IsDoorOpen = isDoorOpen
            };

            csvWriter.WriteHeader<CsvData>();
            csvWriter.NextRecord();
            csvWriter.WriteRecord(record);
            csvWriter.NextRecord();
        }

        public void Receive(Message msg)
        {
            string recMsg = System.Text.Encoding.UTF8.GetString(msg.Content, 0, msg.Content.Length);
            Dictionary<string, string> receivedProperties = msg.Properties;

            Payload receivedData = JsonConvert.DeserializeObject<Payload>(recMsg);

            switch (msg.Properties["source"])
            {
                case SensorTypes.DOOR:
                    isDoorOpen = receivedData.BoolValue;
                    break;
                case SensorTypes.MOISTURE:
                    moisture = receivedData.Value;
                    break;
                case SensorTypes.POWER:
                    power = receivedData.Value;
                    break;
                case SensorTypes.TEMPERATURE:
                    temperature = receivedData.Value;
                    break;
                case SensorTypes.THERMOSTAT:
                    outsideTemperature = receivedData.Value;
                    break;
            }

            var record = new CsvData
            {
                Temperature = temperature,
                OutsideTemperature = outsideTemperature,
                Power = power,
                Moisture = moisture,
                IsDoorOpen = isDoorOpen
            };

            csvWriter.WriteRecord(record);
            csvWriter.NextRecord();
        }
    }
}