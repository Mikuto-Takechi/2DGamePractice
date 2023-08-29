using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class LockStage : MonoBehaviour
{
    [SerializeField] string _stageName = string.Empty;
    Button _button;
    Image _lockImage;
    Text _stageText;
    private void Start()
    {
        _button = GetComponent<Button>();
        _lockImage = transform.Find("StageLock")?.GetComponent<Image>();
        _stageText = transform.Find("StageText")?.GetComponent<Text>();
        if(_lockImage) _lockImage.enabled = false;
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
            if(_stageText && _lockImage)
            {
                _stageText.enabled = _button.IsInteractable();
                _lockImage.enabled = !_button.IsInteractable();
            }
        }
    }
}
