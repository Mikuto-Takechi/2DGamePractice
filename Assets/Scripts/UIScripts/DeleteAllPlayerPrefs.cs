using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteAllPlayerPrefs : MonoBehaviour
{
    public void Delete()
    {
        GameManager.instance._toggleQuitSave = true;
        AudioManager.instance._toggleQuitSave = true;
        PlayerPrefs.DeleteAll();
        Debug.Log("PlayerPrefsのデータをすべて削除した");
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;//Unityエディター上ならこっち
    #else
        Application.Quit();//アプリ上ならこっち
    #endif
    }
}
