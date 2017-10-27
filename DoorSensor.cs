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

        private const int SECONDS_IN_DAY = 86400;
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
            Task.Run(() =>
            {
                //while (true)
                for (int i = 0; ; i++)
                {
                    // If time between 9 am and 11 pm
                    // Door can open at arbitrary intervals
                    if (i > 32400 && i < 82800)
                    {
                        // Between 9am and 11pm store hours
                        if (isDoorOpen)
                        {
                            // Higher chance it'll stay open
                            int isOpen = Math.Min(3 * new Random().Next(0, 1), 1);
                            isDoorOpen = isOpen >= 0.5;
                        }
                    }

                    var payload = new Payload
                    {
                        Id = System.Guid.NewGuid().ToString(),
                        MessageId = messageId++,
                        BoolValue = RandomNoise.NoisyReading(isDoorOpen)
                        //BoolValue = isDoorOpen
                    };
                    var json = JsonConvert.SerializeObject(payload);
                    this.broker.Publish(new Message(json, properties));

                    if (i >= SECONDS_IN_DAY)
                        i = 0;

                    Thread.Sleep(Initial.REFRESH_INTERVAL);
                }
            });
        }

        public void Destroy()
        {
        }

        public void Receive(Message msg)
        {
        }
    }
}