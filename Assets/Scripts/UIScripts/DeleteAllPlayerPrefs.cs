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
        Debug.Log("PlayerPrefs�̃f�[�^�����ׂč폜����");
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;//Unity�G�f�B�^�[��Ȃ炱����
    #else
        Application.Quit();//�A�v����Ȃ炱����
    #endif
    }
}
