using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Runtime.CompilerServices;

// https://github.com/eclipse/paho.mqtt.m2mqtt
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Exceptions;
using uPLibrary.Networking.M2Mqtt.Messages;

public delegate void OnCloseHandler();
public delegate void OnReceiveHandler(string topic, string message);

public class MqttEvent
{
    public enum EventType {ON_CLOSE, ON_RECEIVE };
    public EventType event_type;

    // for ON_RECEIVE
    public string topic;
    public string message;

    public MqttEvent(EventType type)
    {
        this.event_type = type;        
    }

    public MqttEvent(EventType type, string topic, string message)
    {
        this.event_type = type;
        this.topic = topic;
        this.message = message;
    }
}

public class MqttEventQueue
{
    Queue<MqttEvent> queue = new Queue<MqttEvent>();

    public void EnqueOnClose()
    {
        Enqueue(new MqttEvent(MqttEvent.EventType.ON_CLOSE));
    }

    public void EnqueOnRecieve(string topic, string message)
    {
        Enqueue(new MqttEvent(MqttEvent.EventType.ON_RECEIVE, topic, message));
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Enqueue(MqttEvent evt)
    {
        queue.Enqueue(evt);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public MqttEvent Dequeue()
    {
        try
        {
            MqttEvent evt = queue.Dequeue();
            return evt;
        }
        catch (Exception e)
        {    
        }
        return null;
    }
}

public class Mqtt : MonoBehaviour
{
    [SerializeField]
    Config config;

    MqttClient client = null;

    public event OnCloseHandler OnClose;
    public event OnReceiveHandler OnReceive;

    MqttEventQueue queue = new MqttEventQueue();

    void Awake ()
    {
    }

    private void Update()
    {
        // message pump
        while (true)
        {
            MqttEvent evt = queue.Dequeue();
            if (evt == null) break;

            switch (evt.event_type) {
                case MqttEvent.EventType.ON_CLOSE:
                    OnMainThreadClose();
                    break;
                case MqttEvent.EventType.ON_RECEIVE:
                    OnMainThreadReceive(evt.topic, evt.message);
                    break;
                default:
                    break;
            }
        }
    }

    public bool Connect()
    {
        if (client != null) return true;

        client = new MqttClient(config.Host, config.Port, false, null, null, MqttSslProtocols.None);
        string client_id = "TinyMQTTClient-" + Guid.NewGuid().ToString();

        try
        {
            if (config.UseAuth)
            {
                client.Connect(client_id, config.Username, config.Password);
            }
            else
            {
                client.Connect(client_id);
            }

            if (client.IsConnected == false)
            {
                client = null;
                return false;
            }

            client.MqttMsgPublishReceived += OnMqttMsgPublishReceived;
            client.ConnectionClosed += OnConnectionClosed;

            client.Subscribe(new string[] { config.SubscribeTopic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE }); // QoS0
        }
        catch (MqttConnectionException e) {
            client = null;
            return false;
        }

        return true;
    }

    public void Disconnect()
    {
        if (client != null)
        {
            client.Disconnect();
            client = null;
        }
    }

    void OnMqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
    {
        string topic = e.Topic;
        string message = Encoding.UTF8.GetString(e.Message);

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
