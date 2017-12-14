using UnityEditor;
using UnityEngine;

public class CleanPlayerPrefs
{
    [MenuItem("Utils/PlayerPrefs.DeleteAll()")]
    static void DeleteAll()
    {
        Debug.Log("PlayerPrefs.DeleteAll()");
        PlayerPrefs.DeleteAll();
    }
}