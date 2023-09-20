using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
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
    /// <summary>実績解放度表示</summary>
    [SerializeField] Image _achievementStar1 = null;
    [SerializeField] Image _achievementStar2 = null;
    [SerializeField] Image _achievementStar3 = null;
    /// <summary>実績解放マーク</summary>
    [SerializeField] Sprite _achievedImage = null;
    /// <summary>実績未解放マーク</summary>
    [SerializeField] Sprite _notAchievedImage = null;
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
        StageAchievements();
        if (_enableLockProcess == false) return;
        StageLock();
    }
    /// <summary>
    /// ステージ記録をテキストに表示させる
    /// </summary>
    /// <typeparam name="T">記録の型</typeparam>
    /// <param name="dic">ステージ記録の入ったDictionary</param>
    /// <param name="text">表示用テキスト</param>
    void StageRecord<T>(Dictionary<string, T> dic, Text text) where T : IFormattable
    {
        if (dic == null) return;
        if (text == null) return;
        //  ステージ記録が存在する && 記録の値が型の最大値ではない時
        if (dic.ContainsKey(_stageName) && !dic[_stageName].Equals(MaxValue.Max<T>()))
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
    /// <summary>
    /// ステージの実績が解放されているかを判断して表示を切り替える
    /// </summary>
    void StageAchievements()
    {
        if(!GameManager.instance._achievements.ContainsKey(_stageName))
        {
            _achievementStar1.sprite = _notAchievedImage;
            _achievementStar2.sprite = _notAchievedImage;
            _achievementStar3.sprite = _notAchievedImage;
            return;
        }
        if ((GameManager.instance._achievements[_stageName] & Stars.Star1) == Stars.Star1)
            _achievementStar1.sprite = _achievedImage;
        else
            _achievementStar1.sprite = _notAchievedImage;
        if ((GameManager.instance._achievements[_stageName] & Stars.Star2) == Stars.Star2)
            _achievementStar2.sprite = _achievedImage;
        else
            _achievementStar2.sprite = _notAchievedImage;
        if ((GameManager.instance._achievements[_stageName] & Stars.Star3) == Stars.Star3)
            _achievementStar3.sprite = _achievedImage;
        else
            _achievementStar3.sprite = _notAchievedImage;
    }
}
