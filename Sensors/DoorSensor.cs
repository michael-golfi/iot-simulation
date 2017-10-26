using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        private int messageCount;

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

        public void Receive(Message received_message)
        {
        }
    }
}