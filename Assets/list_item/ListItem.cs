using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ListItem : MonoBehaviour {

    [SerializeField]
    Text text_key;

    [SerializeField]
    Text text_value;

    [SerializeField]
    Animator animator;

    public string Key
    {
        get
        {
            return text_key.text;
        }

        set
        {
            text_key.text = value;
        }
    }

    public string Value
    {
        get
        {
            return text_value.text;
        }

        set
        {
            text_value.text = value;
            animator.SetTrigger("blink");
        }
    }
}
