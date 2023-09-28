using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteAllPlayerPrefs : MonoBehaviour
{
    public void Delete()
    {
        //PlayerPrefs.DeleteAll();
        GameManager.Instance.DeleteSave();
        AudioManager.Instance.DeleteSave();
        Debug.Log("PlayerPrefs‚Ìƒf[ƒ^‚ğ‚·‚×‚Äíœ‚µ‚½");
    }
}
