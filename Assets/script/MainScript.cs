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
        mqtt.Disconnect();
        config.Intaractable = true;
        transition.Visible = false;
    }
}
