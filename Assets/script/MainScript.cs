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

        if (mqtt.Connect() == true)
        {
            config.Intaractable = false;
            transition.Visible = true;
        }
        else
        {
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
    }
}
