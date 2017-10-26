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

        Inside Moisture ~ Temp
    */
    public class MoistureSensor : IGatewayModule, IGatewayModuleStart
    {
        private Broker broker;
        private string configuration;
        private int messageId;
        private double moisture = Initial.MOISTURE;

        Dictionary<string, string> properties = new Dictionary<string, string>{
            {"source", SensorTypes.MOISTURE},
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

            if (msg.Properties["source"] == SensorTypes.DOOR)
            {
                // Moisture should increase
                // TODO - Figure out how moisture increases when door opens
                // this.moisture *= ; 
            }
            else if (msg.Properties["source"] == SensorTypes.POWER)
            {
                // Compressor is on, moisture drops
                // this.moisture *= ;
            }
            else if (msg.Properties["source"] == SensorTypes.TEMPERATURE)
            {
                // Inject failure for broken seal?
                // Temp increases, moisture increases
                // this.moisture *= ;
            }
            else if (msg.Properties["source"] == SensorTypes.THERMOSTAT)
            {
                // Inject failure for broken seal?
            }

            var payload = new Payload
            {
                Id = System.Guid.NewGuid().ToString(),
                MessageId = messageId++,
                Value = RandomNoise.NoisyReading(this.moisture)
            };
            var json = JsonConvert.SerializeObject(payload);
            this.broker.Publish(new Message(json, properties));
        }
    }
}