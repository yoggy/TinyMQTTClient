//
//  MQTTClient.cs - MQTT Client Class
//    https://github.com/yoggy/TinyMQTTClient/blob/master/Assets/script/MQTTClient.cs
//
//  license:
//    Copyright(c) 2018 yoggy<yoggy0@gmail.com>
//    Released under the MIT license
//    http://opensource.org/licenses/mit-license.php;
//
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.CompilerServices;

// https://github.com/eclipse/paho.mqtt.m2mqtt
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Exceptions;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace MQTT {

    public delegate void OnCloseHandler();
    public delegate void OnReceiveHandler(string topic, string message);

    public class Event
    {
        public enum Type { ON_CLOSE, ON_RECEIVE };
        public Type type;

        public string topic;
        public string message;

        public Event(Type type)
        {
            this.type = type;
        }

        public Event(Type type, string topic, string message)
        {
            this.type = type;
            this.topic = topic;
            this.message = message;
        }
    }

    public class Queue
    {
        Queue<Event> queue = new Queue<Event>();

        public void EnqueOnClose()
        {
            Enqueue(new Event(Event.Type.ON_CLOSE));
        }

        public void EnqueOnRecieve(string topic, string message)
        {
            Enqueue(new Event(Event.Type.ON_RECEIVE, topic, message));
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Enqueue(Event evt)
        {
            queue.Enqueue(evt);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public Event Dequeue()
        {
            Event evt = null;
            try
            {
                evt = queue.Dequeue();
            }
            catch (Exception e)
            {
            }
            return evt;
        }
    }

    public class MQTTClient : MonoBehaviour
    {
        MqttClient client = null;

        public event OnCloseHandler OnClose;
        public event OnReceiveHandler OnReceive;

        Queue queue = new Queue();

        public bool IsConnected
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return client == null ? false : true;
            }
        }

        private void Update()
        {
            while (true)
            {
                var evt = queue.Dequeue();
                if (evt == null) break;

                switch (evt.type)
                {
                    case Event.Type.ON_CLOSE:
                        OnMainThreadClose();
                        break;
                    case Event.Type.ON_RECEIVE:
                        OnMainThreadReceive(evt.topic, evt.message);
                        break;
                    default:
                        break;
                }
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool Connect(string host)
        {
            return Connect(host, 1883);           
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool Connect(string host, int port)
        {
            if (client != null) return true;

            client = new MqttClient(host, port, false, null, null, MqttSslProtocols.None);
            string client_id = "MQTTClient-" + Guid.NewGuid().ToString();

            try
            {
                client.Connect(client_id);

                if (client.IsConnected == false)
                {
                    client = null;
                    return false;
                }

                client.MqttMsgPublishReceived += OnMqttMsgPublishReceived;
                client.ConnectionClosed += OnConnectionClosed;
            }
            catch (MqttConnectionException e)
            {
                client = null;
                return false;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool Connect(string host, int port, string username, string password)
        {
            if (client != null) return true;

            client = new MqttClient(host, port, false, null, null, MqttSslProtocols.None);
            string client_id = "MQTTClient-" + Guid.NewGuid().ToString();

            try
            {
                client.Connect(client_id, username, password);

                if (client.IsConnected == false)
                {
                    client = null;
                    return false;
                }

                client.MqttMsgPublishReceived += OnMqttMsgPublishReceived;
                client.ConnectionClosed += OnConnectionClosed;
            }
            catch (MqttConnectionException e)
            {
                client = null;
                return false;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Disconnect()
        {
            if (client != null)
            {
                client.Disconnect();
                client = null;
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Publish(string topic, string message)
        {
            if (client == null) return;

            client.Publish(topic, System.Text.Encoding.UTF8.GetBytes(message));
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Subscribe(string topic)
        {
            if (client == null) return;
            client.Subscribe(new string[] { topic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE }); // QoS0
        }

        void OnMqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            string topic = e.Topic;
            string message = System.Text.Encoding.UTF8.GetString(e.Message);

            queue.EnqueOnRecieve(topic, message);
        }

        public void OnMainThreadReceive(string topic, string message)
        {
            OnReceive?.Invoke(topic, message);
        }

        void OnConnectionClosed(object sender, EventArgs e)
        {
            Debug.Log("OnConnectionClosed()");
            queue.EnqueOnClose();
        }

        public void OnMainThreadClose()
        {
            OnClose?.Invoke();
        }
    }
}
