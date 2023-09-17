using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
/// <summary>
/// UnityのButtonコンポーネントのOnclickをキー入力で動かすためのコンポーネント
/// </summary>
[RequireComponent(typeof(Button))]
public class ButtonKeyBind : MonoBehaviour
{
    /// <summary>受け付ける入力</summary>
    [SerializeField] InputAction _action;
    /// <summary>干渉可能フラグ。同時に2つ以上動いてほしくないのでstaticにした。</summary>
    static bool _interactable = true;
    Button _button;
    void OnEnable()
    {
        _action?.Enable();
    }
    void OnDisable()
    {
        _action?.Disable();
    }
    void Start()
    {
        _button = GetComponent<Button>();
    }

    void Update()
    {
        if (_action == null) return;
        //ボタンが干渉可能な状態 && 指定した入力が押された時 && 干渉可能フラグ
        if (_button.IsInteractable() && _action.triggered && _interactable)
        {
            _interactable = false;
            _button.onClick.Invoke();//ボタンに登録されているメソッドを呼び出す
            StartCoroutine(Delay());
        }
    }
    /// <summary>
    /// 0.1秒待って干渉可能な状態へ戻す
    /// </summary>
    IEnumerator Delay()
    {
        yield return null;
        _interactable = true;
    }
}
