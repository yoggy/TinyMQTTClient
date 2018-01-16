//
//  MQTTClient.cs - MQTT Client Class
//    https://github.com/yoggy/TinyMQTTClient/
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

public delegate void OnCloseHandler();
public delegate void OnReceiveHandler(string topic, string message);

public class MQTTEvent
{
    public enum Type { ON_CLOSE, ON_RECEIVE };
    public Type type;

    public string topic;
    public string message;

    public MQTTEvent(Type type)
    {
        this.type = type;
    }

    public MQTTEvent(Type type, string topic, string message)
    {
        this.type = type;
        this.topic = topic;
        this.message = message;
    }
}

public class MQTTQueue
{
    Queue<MQTTEvent> queue = new Queue<MQTTEvent>();

    public void EnqueOnClose()
    {
        Enqueue(new MQTTEvent(MQTTEvent.Type.ON_CLOSE));
    }

    public void EnqueOnRecieve(string topic, string message)
    {
        Enqueue(new MQTTEvent(MQTTEvent.Type.ON_RECEIVE, topic, message));
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Enqueue(MQTTEvent evt)
    {
        queue.Enqueue(evt);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public MQTTEvent Dequeue()
    {
        MQTTEvent evt = null;
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

    MQTTQueue queue = new MQTTQueue();

    private void Update()
    {
        while (true)
        {
            MQTTEvent evt = queue.Dequeue();
            if (evt == null) break;

            switch (evt.type)
            {
                case MQTTEvent.Type.ON_CLOSE:
                    OnMainThreadClose();
                    break;
                case MQTTEvent.Type.ON_RECEIVE:
                    OnMainThreadReceive(evt.topic, evt.message);
                    break;
                default:
                    break;
            }
        }
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public bool Connect(string host, int port)
    {
        if (client != null) return true;

        client = new MqttClient(host, port, false, null, null, MqttSslProtocols.None);

        try
        {
            client.Connect("MQTT-" + Guid.NewGuid().ToString());

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
        string client_id = "MQTT-" + Guid.NewGuid().ToString();

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
