using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
/// <summary>
/// Unity��Button�R���|�[�l���g��Onclick���L�[���͂œ��������߂̃R���|�[�l���g
/// </summary>
[RequireComponent(typeof(Button))]
public class ButtonKeyBind : MonoBehaviour
{
    /// <summary>�󂯕t�������</summary>
    [SerializeField] InputAction _action;
    /// <summary>���\�t���O�B������2�ȏ㓮���Ăق����Ȃ��̂�static�ɂ����B</summary>
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
        //�{�^�������\�ȏ�� && �w�肵�����͂������ꂽ�� && ���\�t���O
        if (_button.IsInteractable() && _action.triggered && _interactable)
        {
            _interactable = false;
            _button.onClick.Invoke();//�{�^���ɓo�^����Ă��郁�\�b�h���Ăяo��
            StartCoroutine(Delay());
        }
    }
    /// <summary>
    /// 0.1�b�҂��Ċ��\�ȏ�Ԃ֖߂�
    /// </summary>
    IEnumerator Delay()
    {
        yield return null;
        _interactable = true;
    }
}
