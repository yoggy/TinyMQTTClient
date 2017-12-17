using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageList : MonoBehaviour {

    [SerializeField]
    GameObject list_item_prefab;

    Dictionary<string, GameObject> map = new Dictionary<string, GameObject>();

    void Start () {
        Clear();
    }

    public void Clear()
    {
        map.Clear();

        foreach (Transform t in gameObject.transform)
        {
            GameObject.Destroy(t.gameObject);
        }
    }

    public void Upsert(string topic, string message)
    {
        GameObject obj = null;
        if (map.ContainsKey(topic) == true)
        {
            obj = map[topic];
        }
        else
        {
            obj = GameObject.Instantiate(list_item_prefab);
            obj.transform.SetParent(gameObject.transform);
            map[topic] = obj;
        }

        ListItem item = obj.GetComponent<ListItem>();
        item.Key = topic;
        item.Value = message;

    }
}

    