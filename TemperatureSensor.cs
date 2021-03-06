﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Gateway;
using Newtonsoft.Json;
using FridgeSimulator.Utils;

namespace FridgeSimulator
{
    /**
        Rules:
        Temperature should stay within certain range 5 +- 1 degree C.
    */
    public class TemperatureSensor : IGatewayModule, IGatewayModuleStart
    {
        private Broker broker;
        private string configuration;
        private int messageId = 0;
        private double insideTemperature = 5;
        private double outsideTemperature = 20;

        Dictionary<string, string> properties = new Dictionary<string, string>{
            {"source", SensorTypes.TEMPERATURE},
            {"macAddress", "04:04:04:04:04:04"},
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
                        Value = RandomNoise.NoisyReading(this.insideTemperature)
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

            if (msg.Properties["source"] == SensorTypes.DOOR && receivedData.BoolValue)
            {
                // Temp should equalize to Thermostat temp
                this.insideTemperature = Math.Min(this.insideTemperature + 0.1 * this.outsideTemperature, this.outsideTemperature);
            }
            else if (msg.Properties["source"] == SensorTypes.POWER && receivedData.Value > 500)
            {
                // Compressor is on, temp drops, not below 0
                this.insideTemperature = Math.Max(this.insideTemperature - 0.1 * this.insideTemperature, 0);
            }
            else if (msg.Properties["source"] == SensorTypes.THERMOSTAT)
            {
                // Temp should go up minimally
                this.outsideTemperature = receivedData.Value;
                this.insideTemperature = Math.Min(this.insideTemperature + 0.001 * receivedData.Value, receivedData.Value);
            }
        }
    }
}