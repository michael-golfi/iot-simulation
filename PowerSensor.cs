using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FridgeSimulator.Utils;
using Microsoft.Azure.Devices.Gateway;
using Newtonsoft.Json;

namespace FridgeSimulator
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
        private double power = Initial.POWER_LOW;

        Dictionary<string, string> properties = new Dictionary<string, string>{
            {"source", SensorTypes.POWER},
            {"macAddress", "03:03:03:03:03:03"},
        };

        public void Create(Broker broker, byte[] configuration)
        {
            this.broker = broker;
            this.configuration = System.Text.Encoding.UTF8.GetString(configuration);
        }

        public void Start()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    var payload = new Payload
                    {
                        Id = System.Guid.NewGuid().ToString(),
                        MessageId = messageId++,
                        Value = RandomNoise.NoisyReading(power)
                    };
                    var json = JsonConvert.SerializeObject(payload);
                    this.broker.Publish(new Message(json, properties));

                    Thread.Sleep(Initial.REFRESH_INTERVAL);
                }
            });
        }

        public void Destroy()
        {
        }

        public void Receive(Message msg)
        {
            string recMsg = Encoding.UTF8.GetString(msg.Content, 0, msg.Content.Length);
            Payload receivedData = JsonConvert.DeserializeObject<Payload>(recMsg);
            power = (receivedData.Value > 6.0) ? Initial.POWER_HIGH : Initial.POWER_LOW;
            if (receivedData.Value > 6.0)
                power = Initial.POWER_HIGH;
            if (receivedData.Value < 4)
                power = Initial.POWER_LOW;
            // Should stop once it hits 5 with a bit of momentum too
        }
    }
}