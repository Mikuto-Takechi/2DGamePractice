using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// ステージ選択ボタンに付けるコンポーネント
/// </summary>
[RequireComponent(typeof(Button))]
public class StageButton : MonoBehaviour
{
    /// <summary>そのボタンに割り当てるステージ名</summary>
    [SerializeField] string _stageName = "";
    /// <summary>ステージ解放関連の処理を行うかどうか</summary>
    [SerializeField] bool _enableLockProcess = true;
    /// <summary>クリア時間記録を表示させるテキスト</summary>
    [SerializeField] Text _timeRecord = null;
    /// <summary>歩数記録を表示させるテキスト</summary>
    [SerializeField] Text _stepRecord = null;
    /// <summary>ロックされた時に表示するオブジェクト</summary>
    [SerializeField] GameObject _lockImage = null;
    /// <summary>ロックされた時に非表示するオブジェクト</summary>
    [SerializeField] GameObject[] _hiddenObjects = null;
    Button _button;
    private void Start()
    {
        _button = GetComponent<Button>();
        if (_lockImage) _lockImage.SetActive(false);
    }
    private void Update()
    {
        StageRecord(GameManager.instance._timeRecords, _timeRecord);
        StageRecord(GameManager.instance._stepsRecords, _stepRecord);
        if (_enableLockProcess == false) return;
        StageLock();
    }
    /// <summary>
    /// ステージ記録をテキストに表示させる
    /// </summary>
    /// <typeparam name="T">記録の型</typeparam>
    /// <param name="dic">ステージ記録の入ったDictionary</param>
    /// <param name="text">表示用テキスト</param>
    void StageRecord<T>(Dictionary<string, T> dic, Text text) where T : IComparable<T>, IFormattable
    {
        if (dic == null) return;
        if (text == null) return;
        if (dic.ContainsKey(_stageName) && !dic[_stageName].Equals(default(T)))
        {
            //float型なら書式指定して表示
            if (typeof(T) == typeof(float))
                text.text = dic[_stageName].ToString("F2", new CultureInfo("en-US"));
            //int型ならそのまま表示
            if (typeof(T) == typeof(int))
                text.text = dic[_stageName].ToString();
        }
        else
        {
            text.text = "- - -";
        }
    }
    /// <summary>
    /// ステージが解放されているかを判断してオンオフを切り替える
    /// </summary>
    void StageLock()
    {
        if (_button.IsInteractable())//ボタンが干渉可能ならば
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
            if (_lockImage) _lockImage.SetActive(!_button.IsInteractable());
        }
    }
}
