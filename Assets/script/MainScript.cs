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
    Mqtt mqtt;

    [SerializeField]
    Toast toast;

    [SerializeField]
    MessageList message_list;

    void Start()
    {
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

        if (mqtt.Connect() == true)
        {
            config.Intaractable = false;
            transition.Visible = true;
        }
        else
        {
            toast.Show("mqtt.Connect() failed...");
            Disconnect();
        }
    }

    public void Disconnect()
    {
        Debug.Log("Disconnect()");
        mqtt.Disconnect();
        config.Intaractable = true;
        transition.Visible = false;
    }

    public void OnClose()
    {
        Disconnect();
    }

    public void OnReceive(string topic, string message)
    {
        Debug.Log(string.Format("{0}:{1}", topic, message));
        message_list.Upsert(topic, message);
    }
}
