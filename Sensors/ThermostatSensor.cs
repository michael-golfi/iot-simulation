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

        Thermostat may fluctuate
    */
    public class ThermostatSensor : IGatewayModule, IGatewayModuleStart
    {
        private Broker broker;
        private string configuration;
        private int messageId = 0;
        private double outsideTemperature = 20;
        Dictionary<string, string> properties = new Dictionary<string, string>{
            {"source", SensorTypes.THERMOSTAT},
            {"macAddress", "05:05:05:05:05:05"},
        };

        public void Create(Broker broker, byte[] configuration)
        {
            this.broker = broker;
            this.configuration = System.Text.Encoding.UTF8.GetString(configuration);
        }

        public void Start()
        {
            var payload = new Payload
            {
                Id = System.Guid.NewGuid().ToString(),
                MessageId = messageId++,
                Value = RandomNoise.NoisyReading(this.outsideTemperature)
            };
            var json = JsonConvert.SerializeObject(payload);
            this.broker.Publish(new Message(json, properties));
        }

        public void Destroy()
        {
        }

        public void Receive(Message msg)
        {
            string recMsg = Encoding.UTF8.GetString(msg.Content, 0, msg.Content.Length);
            Payload receivedData = JsonConvert.DeserializeObject<Payload>(recMsg);

            // TODO - Inject arbitary temperature changes too

            if (msg.Properties["source"] == SensorTypes.DOOR)
            {
                // When the door is open, outside temp should equalize with fridge temp
                // Outside temp should slightly drop
                this.outsideTemperature *= 0.999;
            }
            else if (msg.Properties["source"] == SensorTypes.POWER)
            {
                // Compressor is on, outside temp rises
                this.outsideTemperature *= 1.001;
            }

            var payload = new Payload
            {
                Id = System.Guid.NewGuid().ToString(),
                MessageId = messageId++,
                Value = RandomNoise.NoisyReading(this.outsideTemperature)
            };
            var json = JsonConvert.SerializeObject(payload);
            this.broker.Publish(new Message(json, properties));
        }

    }
}