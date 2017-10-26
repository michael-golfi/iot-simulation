using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IoTEdgeFridgeSimulator.Utils;
using Microsoft.Azure.Devices.Gateway;
using Newtonsoft.Json;

namespace PrinterModule
{
    public class DotNetPrinterModule : IGatewayModule
    {
        private string configuration;

        private double temperature = Initial.TEMPERATURE;
        private double outsideTemperature = Initial.OUTSIDE_TEMP;
        private double moisture = Initial.MOISTURE;
        private double power = Initial.POWER;
        private bool isDoorOpen = Initial.DOOR_OPEN;


        public void Create(Broker broker, byte[] configuration)
        {
            this.configuration = System.Text.Encoding.UTF8.GetString(configuration);
        }

        public void Destroy()
        {
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

            var line = String.Format("{{isDoorOpen:{0},moisture:{1},power:{2},temperature:{3},outsideTemperature:{4}}}",
                isDoorOpen, moisture, power, temperature, outsideTemperature);
            Console.WriteLine(line);
        }
    }
}