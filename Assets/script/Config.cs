using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Config : MonoBehaviour {
    [SerializeField]
    InputField input_mqtt_host;

    [SerializeField]
    InputField input_mqtt_port;

    [SerializeField]
    Toggle toggle_use_auth;

    [SerializeField]
    InputField input_mqtt_username;

    [SerializeField]
    InputField input_mqtt_password;

    [SerializeField]
    InputField input_mqtt_subscribe_topic;

    public string Host
    {
        get
        {
            return input_mqtt_host.text;
        }

        set
        {
            input_mqtt_host.text = value;
        }
    }

    public int Port
    {
        get
        {
            return int.Parse(input_mqtt_port.text);
        }

        set
        {
            input_mqtt_port.text = "" + value;
        }
    }

    public bool UseAuth
    {
        get
        {
            return toggle_use_auth.isOn;
        }

        set
        {
            toggle_use_auth.isOn = value;
        }
    }

    public string Username
    {
        get
        {
            return input_mqtt_username.text;
        }

        set
        {
            input_mqtt_username.text = value;
        }
    }

    public string Password
    {
        get
        {
            return input_mqtt_password.text;
        }

        set
        {
            input_mqtt_password.text = value;
        }
    }

    public string SubscribeTopic
    {
        get
        {
            return input_mqtt_subscribe_topic.text;
        }

        set
        {
            input_mqtt_subscribe_topic.text = value;
        }
    }

    bool intaractable = true;
    public bool Intaractable
    {
        get
        {
            return intaractable;
        }

        set
        {
            intaractable = value;
        }
    }

    void Awake () {
        LoadConfig();		
	}

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveConfig();
        }
    }

    void OnApplicationQuit () {
        SaveConfig();
    }

    void LoadConfig()
    {
        Host = PlayerPrefs.GetString("host", "iot.eclipse.org");
        Port = PlayerPrefs.GetInt("port", 1883);
        UseAuth = PlayerPrefs.GetInt("use_auth", 0) != 0 ? true : false;
        Username = PlayerPrefs.GetString("username", "");
        Password = PlayerPrefs.GetString("password", "");
        SubscribeTopic = PlayerPrefs.GetString("subscribe_topic", "tiny_mqtt_client/test/#");

        UpdateUI();
    }

    public void SaveConfig()
    {
        Debug.Log("SaveConfig()");
        PlayerPrefs.SetString("host", Host);
        PlayerPrefs.SetInt("port", Port);
        PlayerPrefs.SetInt("use_auth", UseAuth ? 1 : 0);
        PlayerPrefs.SetString("username", Username);
        PlayerPrefs.SetString("password", Password);
        PlayerPrefs.SetString("subscribe_topic", SubscribeTopic);
    }

    public void OnUpdateToggle(bool flag)
    {
        UpdateUI();
    }

    void UpdateUI()
    {
        if (Intaractable == true)
        {
            if (UseAuth == true)
            {
                input_mqtt_username.interactable = true;
                input_mqtt_password.interactable = true;
            }
            else
            {
                input_mqtt_username.interactable = false;
                input_mqtt_password.interactable = false;
            }
            input_mqtt_host.interactable = true;
            input_mqtt_port.interactable = true;
            toggle_use_auth.interactable = true;
            input_mqtt_subscribe_topic.interactable = true;
        }
        else 
        {
            input_mqtt_host.interactable = false;
            input_mqtt_port.interactable = false;
            toggle_use_auth.interactable = false;
            input_mqtt_username.interactable = false;
            input_mqtt_password.interactable = false;
            input_mqtt_subscribe_topic.interactable = false;
        }
    }
}
