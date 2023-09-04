using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

[RequireComponent(typeof(Button))]
public class LockStage : MonoBehaviour
{
    /// <summary>判定するステージの名前</summary>
    [SerializeField] string _stageName = string.Empty;
    /// <summary>ロックされた時に表示するオブジェクト</summary>
    [SerializeField] GameObject _visibleObject = null;
    /// <summary>ロックされた時に非表示するオブジェクト</summary>
    [SerializeField] GameObject[] _hiddenObjects = null;
    Button _button;
    private void Start()
    {
        _button = GetComponent<Button>();
        if(_visibleObject) _visibleObject.SetActive(false);
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
            _hiddenObjects.Where(o => o).ToList().ForEach(o => o.SetActive(_button.IsInteractable()));
            if (_visibleObject) _visibleObject.SetActive(!_button.IsInteractable());
        }
    }
}
