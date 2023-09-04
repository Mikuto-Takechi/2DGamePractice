using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

[RequireComponent(typeof(Button))]
public class LockStage : MonoBehaviour
{
    /// <summary>���肷��X�e�[�W�̖��O</summary>
    [SerializeField] string _stageName = string.Empty;
    /// <summary>���b�N���ꂽ���ɕ\������I�u�W�F�N�g</summary>
    [SerializeField] GameObject _visibleObject = null;
    /// <summary>���b�N���ꂽ���ɔ�\������I�u�W�F�N�g</summary>
    [SerializeField] GameObject[] _hiddenObjects = null;
    Button _button;
    private void Start()
    {
        _button = GetComponent<Button>();
        if(_visibleObject) _visibleObject.SetActive(false);
    }
    void Update()
    {
        if(_button.IsInteractable())//�{�^�������\�Ȃ��
        {
            if (GameManager.instance._unlockStages.Contains(_stageName))//�X�e�[�W���������Ă��邩���m�F����
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
