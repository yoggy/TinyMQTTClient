using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainScript : MonoBehaviour {

    [SerializeField]
    Transition transition;

    [SerializeField]
    Config config;

    [SerializeField]
    MQTT.MQTTClient mqtt;

    [SerializeField]
    Toast toast;

    [SerializeField]
    MessageList message_list;

    [SerializeField]
    Button connect_button;

    void Start()
    {
        mqtt.OnConnected += OnConnected;
        mqtt.OnClose += OnClose;
        mqtt.OnReceive += OnReceive;
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            Disconnect();
        }
    }

    void OnApplicationQuit()
    {
        Disconnect();
    }

    public void Connect()
    {
        Debug.Log("Connect()");
        config.SaveConfig();
        message_list.Clear();

        if (config.UseAuth)
        {
            mqtt.Connect(config.Host, config.Port, config.Username, config.Password);
        }
        else
        {
            mqtt.Connect(config.Host, config.Port);
        }
        config.Intaractable = false;
        connect_button.interactable = false;
    }

    public void Disconnect()
    {
        Debug.Log("Disconnect()");
        mqtt.Disconnect();
        config.Intaractable = true;
        connect_button.interactable = true;
        transition.Visible = false;
    }

    public void OnConnected(bool result, string message)
    {
        if (result == true)
        {
            mqtt.Subscribe(config.SubscribeTopic);
            config.Intaractable = false;
            connect_button.interactable = false;
            transition.Visible = true;
        }
        else
        {
            toast.Show("mqtt.Connect() failed...e=" + message);
            Disconnect();
        }
    }

    public void OnClose()
    {
        Disconnect();
    }

    public void OnReceive(string topic, string message)
    {
        if (message.Length > 256)
        {
            message = message.Substring(0, 256) + "...";
        }

        Debug.Log(string.Format("{0}:{1}", topic, message));
        message_list.Upsert(topic, message);
    }
}
