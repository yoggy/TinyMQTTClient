using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

// https://github.com/eclipse/paho.mqtt.m2mqtt
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Exceptions;
using uPLibrary.Networking.M2Mqtt.Messages;

public delegate void OnCloseHandler();
public delegate void OnReceiveHandler(string topic, string message);

class Message
{
    public string topic;
    public string message;
}

public class Mqtt : MonoBehaviour {

    [SerializeField]
    Config config;

    MqttClient client = null;

    public event OnCloseHandler OnClose;
    public event OnReceiveHandler OnReceive;

    Queue message_queue;
    bool request_onclose = false;

    void Awake () {
        message_queue = Queue.Synchronized(new Queue());
    }

    private void Update()
    {
        while (true)
        {
            object obj = message_queue.Dequeue();
            if (obj == null) break;

            Message msg = (Message)obj;
            if (OnReceive != null)
            {
                OnReceive(msg.topic, msg.message);
            }
        }

        if (request_onclose)
        {
            if (OnClose != null)
            {
                OnClose();
            }
            request_onclose = false;
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
        string msg = Encoding.UTF8.GetString(e.Message);

        if (OnReceive != null)
        {
            OnReceive(topic, msg);
        }
    }

    void OnConnectionClosed(object sender, EventArgs e)
    {
        Debug.Log("OnConnectionClosed()");
        request_onclose = true;
    }
}
