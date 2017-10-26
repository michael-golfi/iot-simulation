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

        Temp inside equalizes to Temp outside.
        Door usually only opens between 9am and 11pm
        May momentarily or permanently break
    */
    public class DoorSensor : IGatewayModule, IGatewayModuleStart
    {
        private Broker broker;
        private string configuration;
        private int messageId;
        private bool isDoorOpen = Initial.DOOR_OPEN;

        Dictionary<string, string> properties = new Dictionary<string, string>{
            {"source", SensorTypes.DOOR},
            {"macAddress", "01:01:01:01:01:01"},
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
                BoolValue = RandomNoise.NoisyReading(isDoorOpen)
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

            // If time between 9 am and 11 pm
            // Door can open at arbitrary intervals
            //isDoorOpen = !isDoorOpen;

            var payload = new Payload
            {
                Id = System.Guid.NewGuid().ToString(),
                MessageId = messageId++,
                BoolValue = RandomNoise.NoisyReading(isDoorOpen)
            };
            var json = JsonConvert.SerializeObject(payload);
            this.broker.Publish(new Message(json, properties));
        }
    }
}