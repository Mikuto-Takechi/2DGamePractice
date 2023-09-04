using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// ボタンを一定の数で分割して切り替えれるようにするスクリプト
/// </summary>
public class StageButtonSet : MonoBehaviour
{
    [SerializeField] GameObject[] _allButton;
    /// <summary>アクティブ時の親オブジェクト</summary>
    [SerializeField] GameObject _activeParent;
    /// <summary>非アクティブ時の親オブジェクト</summary>
    [SerializeField] GameObject _inactiveParent;
    /// <summary>1グループに含まれるボタンの数</summary>
    [SerializeField] int _buttonCount = 3;
    List<List<GameObject>> _buttonGroup = new List<List<GameObject>>();
    int _switchGroup = -1;
    GameObject[] _unsetGroup = new GameObject[0];
    private void Start()
    {
        for(int i = 0; i < _allButton.Length; i += _buttonCount)
        {
            var buttons = _allButton.Skip(i).Take(_buttonCount).ToList();//i番目の要素から_buttonCountだけ取り出してリスト化する
            buttons.Where(obj => obj).Select(obj => obj.GetComponent<Button>().interactable = false);//ボタンを接触不可にする
            _buttonGroup.Add(buttons);//取り出したボタンのリストをまとめる
        }
        SwitchButtonGroup(false);//初期化のため1回だけこのメソッドを呼ぶ
    }
    /// <summary>
    /// アクティブなボタンのグループを切り替える
    /// </summary>
    /// <param name="decrease">falseでプラス、trueでマイナス</param>
    public void SwitchButtonGroup(bool decrease)
    {
        //_unsetGroupにnullチェックとGetComponentを行う
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
        }
        transform.Find("PagePanel").Find("Number").GetComponent<Text>().text = $"{_switchGroup+1}/{_buttonGroup.Count} ページ";
    }
}
