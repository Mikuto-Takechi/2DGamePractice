using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class LockStage : MonoBehaviour
{
    [SerializeField] string _stageName = string.Empty;
    Button _button;
    private void Start()
    {
        _button = GetComponent<Button>();
    }
    void Update()
    {
        if(_button.IsInteractable())//ボタンが干渉可能ならば
        {
            if (GameManager.instance._unlockStages.Contains(_stageName))//ステージが解放されているかを確認する
            {
                _button.interactable = true;
            }
            else
            {
                _button.interactable = false;
            }
        }
    }
}
