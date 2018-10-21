//
//  MQTTClient.cs - MQTT Client Class
//    https://github.com/yoggy/TinyMQTTClient/blob/master/Assets/script/MQTTClient.cs
//
//  license:
//    Copyright(c) 2018 yoggy<yoggy0@gmail.com>
//    Released under the MIT license
//    http://opensource.org/licenses/mit-license.php;
//

#pragma warning disable 168

using System;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.CompilerServices;

// https://github.com/eclipse/paho.mqtt.m2mqtt
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Exceptions;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Threading.Tasks;

namespace MQTT {

    public delegate void OnConnectedHandler(bool result, string message);
    public delegate void OnCloseHandler();
    public delegate void OnReceiveHandler(string topic, string message);

    public class Event
    {
        public enum Type { ON_CONNECTED, ON_CLOSE, ON_RECEIVE };
        public Type type;

        public bool result;
        public string topic;
        public string message;

        public Event(Type type)
        {
            this.type = type;
        }

        public Event(Type type, bool result, string message)
        {
            this.type = type;
            this.result = result;
            this.message = message;
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

        public void EnqueOnConnected(bool result, string message)
        {
            Enqueue(new Event(Event.Type.ON_CONNECTED, result, message));
        }

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

    [DisallowMultipleComponent]
    public class MQTTClient : MonoBehaviour
    {
        MqttClient client = null;

        public event OnConnectedHandler OnConnected;
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
                    case Event.Type.ON_CONNECTED:
                        OnMainThreadConnected(evt.result, evt.message);
                        break;
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
        public void Connect(string host)
        {
            Connect(host, 1883);           
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Connect(string host, int port)
        {
            Connect(host, port, null, null);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Connect(string host, int port, string username, string password)
        {
            if (client != null) return;

            Task.Run(() => {
                try
                {
                    client = new MqttClient(host, port, false, null, null, MqttSslProtocols.None); // raise SocketException...
                    string client_id = "MQTTClient-" + Guid.NewGuid().ToString();

                    if (username != null || username.Length == 0)
                    {
                        client.Connect(client_id, username, password);
                    }
                    else
                    {
                        client.Connect(client_id);
                    }

                    if (client.IsConnected == false)
                    {
                        client = null;
                        OnConnectionConnected(false, "connection failed...");
                    }

                    client.MqttMsgPublishReceived += OnMqttMsgPublishReceived;
                    client.ConnectionClosed += OnConnectionClosed;
                    OnConnectionConnected(true, "");
                }
                catch (Exception e)
                {
                    client = null;
                    string err_msg;
                    Debug.LogError(e);
                    if (username != null && e.GetType() == typeof(System.NullReferenceException)) {
                        err_msg = "authentication failed..";
                    }
                    else {
                        err_msg = e.Message;
                    }
                    OnConnectionConnected(false, err_msg);
                }
            });
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

        public void OnConnectionConnected(bool result, string message)
        {
            queue.EnqueOnConnected(result, message);
        }

        public void OnMainThreadConnected(bool result, string message)
        {
            OnConnected?.Invoke(result, message);
        }

        void OnMqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            try
            {
                string topic = e.Topic;
                string message = System.Text.Encoding.UTF8.GetString(e.Message);
                queue.EnqueOnRecieve(topic, message);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
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
