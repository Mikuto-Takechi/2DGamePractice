using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteAllPlayerPrefs : MonoBehaviour
{
    public void Delete()
    {
        //PlayerPrefs.DeleteAll();
        GameManager.instance.DeleteSave();
        AudioManager.instance.DeleteSave();
        Debug.Log("PlayerPrefs�̃f�[�^�����ׂč폜����");
    }
}
