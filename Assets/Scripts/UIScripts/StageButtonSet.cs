using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageButtonSet : MonoBehaviour
{
    [SerializeField] GameObject[] _allButton;
    [SerializeField] GameObject _group;
    [SerializeField] GameObject _parent;
    List<GameObject[]> _buttonGroup = new List<GameObject[]>();
    int _switchGroup = -1;
    GameObject[] _unsetGroup = new GameObject[3];
    private void Start()
    {
        GameObject[] buttons = new GameObject[3];
        int groupCount = 0;
        for (int i = 0; i < _allButton.Length; ++i)
        {
            if (_allButton[i] != null) _allButton[i].GetComponent<Button>().interactable = false;
            buttons[groupCount] = _allButton[i];
            if (groupCount == buttons.Length - 1)
            {
                _buttonGroup.Add(buttons);
                buttons = new GameObject[3];
            }
            ++groupCount;
            groupCount %= buttons.Length;
        }
        SwitchButtonGroup(false);
    }
    public void SwitchButtonGroup(bool decrease)
    {
        foreach(GameObject button in _unsetGroup)
        {
            if(button != null)
            {
                button.transform.SetParent(_parent.transform);
                button.transform.SetAsFirstSibling();
                button.GetComponent<Button>().interactable = false;
            }
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
        for (int i = 0; i < _buttonGroup[_switchGroup].Length; ++i)
        {
            if(_buttonGroup[_switchGroup][i] != null)
            {
                _buttonGroup[_switchGroup][i].transform.SetParent(_group.transform);
                _unsetGroup[i] = _buttonGroup[_switchGroup][i];
                Selectable select = _buttonGroup[_switchGroup][i].GetComponent<Selectable>();
                select.interactable = true;
                GameObject upButton = _buttonGroup[_switchGroup][i == 0 ? _buttonGroup[_switchGroup].Length - 1 : i - 1];
                GameObject downButton = _buttonGroup[_switchGroup][(i + 1) % _buttonGroup[_switchGroup].Length];
                Navigation nav = select.navigation;
                nav.mode = Navigation.Mode.Explicit;
                if(upButton != null) nav.selectOnUp = upButton.GetComponent<Selectable>();
                if(downButton != null) nav.selectOnDown = downButton.GetComponent<Selectable>();
                select.navigation = nav;
            }
        }
        transform.GetComponentInChildren<Text>().text = $"{_switchGroup+1}/{_buttonGroup.Count} ÉyÅ[ÉW";
    }
}
