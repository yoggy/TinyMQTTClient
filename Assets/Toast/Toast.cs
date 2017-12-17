using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Toast : MonoBehaviour {

    [SerializeField]
    RectTransform canvas_rect_transform;

    [SerializeField]
    GameObject toast_panel;

    [SerializeField]
    GameObject toast_message;

    Text text_toast_message;
    Animator animator_toast_panel;
    Animator animator_toast_message;

	void Start () {
        text_toast_message = toast_message.GetComponent<Text>();

        animator_toast_panel = toast_panel.GetComponent<Animator>();
        animator_toast_message = toast_message.GetComponent<Animator>();

        canvas_rect_transform.anchorMin = new Vector2(0, 0);
        canvas_rect_transform.anchorMax = new Vector2(1, 1);
    }

    public void Show(string message)
    {
        text_toast_message.text = message;

        animator_toast_panel.SetTrigger("show");
        animator_toast_message.SetTrigger("show");
    }

    public void ToastTest()
    {
        Show("test message...");
    }
}
