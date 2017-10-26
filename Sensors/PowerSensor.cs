using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IoTEdgeFridgeSimulator.Utils;
using Microsoft.Azure.Devices.Gateway;
using Newtonsoft.Json;

namespace IoTEdgeFridgeSimulator
{
    /**
        Rules:

        Power On if Temp > Threshold
    */
    public class PowerSensor : IGatewayModule, IGatewayModuleStart
    {
        private Broker broker;
        private string configuration;
        private int messageId;
        private const double idleWattage = 0.001;
        private const double voltage = 120;
        private const double current = 6;

        Dictionary<string, string> properties = new Dictionary<string, string>{
            {"source", SensorTypes.POWER},
            {"macAddress", "02:02:02:02:02:02"},
        };

        public void Create(Broker broker, byte[] configuration)
        {
            this.broker = broker;
            this.configuration = System.Text.Encoding.UTF8.GetString(configuration);
        }

        public void Start()
        {
        }

        public void Destroy()
        {
        }

        public void Receive(Message msg)
        {
            string recMsg = Encoding.UTF8.GetString(msg.Content, 0, msg.Content.Length);
            Payload receivedData = JsonConvert.DeserializeObject<Payload>(recMsg);
            double wattage = idleWattage;

            if (msg.Properties["source"] == SensorTypes.TEMPERATURE)
            {
                wattage = (receivedData.Value > 6.0) ? voltage * current : idleWattage;
            }

            var payload = new Payload
            {
                Id = System.Guid.NewGuid().ToString(),
                MessageId = messageId++,
                Value = RandomNoise.NoisyReading(wattage)
            };
            var json = JsonConvert.SerializeObject(payload);
            this.broker.Publish(new Message(json, properties));
        }
    }
}