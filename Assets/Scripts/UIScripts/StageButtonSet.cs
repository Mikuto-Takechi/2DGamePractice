using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// �{�^�������̐��ŕ������Đ؂�ւ����悤�ɂ���X�N���v�g
/// </summary>
public class StageButtonSet : MonoBehaviour
{
    [SerializeField] GameObject[] _allButton;
    /// <summary>�A�N�e�B�u���̐e�I�u�W�F�N�g</summary>
    [SerializeField] GameObject _activeParent;
    /// <summary>��A�N�e�B�u���̐e�I�u�W�F�N�g</summary>
    [SerializeField] GameObject _inactiveParent;
    /// <summary>1�O���[�v�Ɋ܂܂��{�^���̐�</summary>
    [SerializeField] int _buttonCount = 3;
    List<List<GameObject>> _buttonGroup = new List<List<GameObject>>();
    int _switchGroup = -1;
    GameObject[] _unsetGroup = new GameObject[0];
    private void Start()
    {
        for(int i = 0; i < _allButton.Length; i += _buttonCount)
        {
            var buttons = _allButton.Skip(i).Take(_buttonCount).ToList();//i�Ԗڂ̗v�f����_buttonCount�������o���ă��X�g������
            buttons.Where(obj => obj).Select(obj => obj.GetComponent<Button>().interactable = false);//�{�^����ڐG�s�ɂ���
            _buttonGroup.Add(buttons);//���o�����{�^���̃��X�g���܂Ƃ߂�
        }
        SwitchButtonGroup(false);//�������̂���1�񂾂����̃��\�b�h���Ă�
    }
    /// <summary>
    /// �A�N�e�B�u�ȃ{�^���̃O���[�v��؂�ւ���
    /// </summary>
    /// <param name="decrease">false�Ńv���X�Atrue�Ń}�C�i�X</param>
    public void SwitchButtonGroup(bool decrease)
    {
        //_unsetGroup��null�`�F�b�N��GetComponent���s��
        foreach(Button button in _unsetGroup.Where(btn => btn).Select(btn => btn.GetComponent<Button>()))
        {
            button.transform.SetParent(_inactiveParent.transform);
            button.transform.SetAsFirstSibling();
            button.interactable = false;
        }
        if(!decrease)
        {
            ++_switchGroup;
            _switchGroup %= _buttonGroup.Count;
        }
        else
        {
            --_switchGroup;
            if (_switchGroup < 0) _switchGroup = _buttonGroup.Count - 1;
        }
        _unsetGroup = _buttonGroup[_switchGroup].ToArray();
        foreach (Selectable sel in _buttonGroup[_switchGroup].Where(obj => obj).Select(sel => sel.GetComponent<Selectable>()))
        {
            sel.transform.SetParent(_activeParent.transform);
            sel.interactable = true;
            //���ݏ������Ă���I�u�W�F�N�g�̗v�f�ԍ����擾����
            int index = _buttonGroup[_switchGroup].FindIndex(v => v == sel.gameObject);
            GameObject upButton = _buttonGroup[_switchGroup][index == 0 ? _buttonGroup[_switchGroup].Count - 1 : index - 1];//��̃{�^��
            GameObject downButton = _buttonGroup[_switchGroup][(index + 1) % _buttonGroup[_switchGroup].Count];//���̃{�^��
            //�i�r�Q�[�V������ݒ肷��
            Navigation nav = sel.navigation;
            nav.mode = Navigation.Mode.Explicit;
            if (upButton != null) nav.selectOnUp = upButton.GetComponent<Selectable>();
            if (downButton != null) nav.selectOnDown = downButton.GetComponent<Selectable>();
            sel.navigation = nav;
        }
        transform.Find("PagePanel").Find("Number").GetComponent<Text>().text = $"{_switchGroup+1}/{_buttonGroup.Count} �y�[�W";
    }
}
