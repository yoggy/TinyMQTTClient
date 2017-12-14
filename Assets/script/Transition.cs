using UnityEngine;

public class Transition : MonoBehaviour {

    [SerializeField]
    protected Canvas canvas_login;

    [SerializeField]
    protected Canvas canvas_messages;

    [SerializeField]
    Animator slide_animator;

    public bool Visible
    {
        get
        {
            return slide_animator.GetBool("visible");
        }
        
        set
        {
            slide_animator.SetBool("visible", value);
        }
    }
}
