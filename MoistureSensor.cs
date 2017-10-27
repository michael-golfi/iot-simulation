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

        Inside Moisture ~ Temp
    */
    public class MoistureSensor : IGatewayModule, IGatewayModuleStart
    {
        private Broker broker;
        private string configuration;
        private int messageId;
        private double moisture = Initial.MOISTURE;
        private bool sealBroken = false;

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
            Task.Run(() =>
            {
                var rand = new Random();
                for (int i = 0; ; i++)
                {
                    if (i > 6048 && !sealBroken)
                    { // Inject this fault only after 7 days of data.
                      // Three times likely chance the seal will break
                        Console.WriteLine("Past 21 days");
                        if (rand.Next(100) < 40)
                        {
                            sealBroken = true;
                            Console.WriteLine("Seal Broken");
                        }
                    }

                    if (sealBroken)
                    {
                        // Make moisture at least 71, 15 above norm.
                        moisture = Math.Max(71, moisture);
                    }

                    // TODO inject broken seal fault
                    var payload = new Payload
                    {
                        Id = System.Guid.NewGuid().ToString(),
                        MessageId = messageId++,
                        Value = RandomNoise.NoisyReading(this.moisture)
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

            if (msg.Properties["source"] == SensorTypes.DOOR)
            {
                // Moisture should increase
                // TODO - Figure out how moisture increases when door opens
                this.moisture = Math.Min(this.moisture + 0.35, 70);
            }
            else if (msg.Properties["source"] == SensorTypes.POWER)
            {
                // Compressor is on, moisture drops
                this.moisture = Math.Max(this.moisture - 0.75, 40);
            }
            else if (msg.Properties["source"] == SensorTypes.TEMPERATURE)
            {
                // TODO - How does moisture change when Temp rises
                this.moisture = Math.Min(this.moisture + 0.4, 70);
            }
        }
    }
}