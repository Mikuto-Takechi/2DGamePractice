using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(Text))]
public class GameText : MonoBehaviour
{
    Text _text;
    [SerializeField] string[] _texts;
    [SerializeField] TextType _type = TextType.None;
    ReactiveProperty<int> _time = new IntReactiveProperty();
    int _indexNumber = 0;
    void Start()
    {
        _text = GetComponent<Text>();
        if(_type == TextType.MoveText) 
            StartCoroutine(MoveText());
        if( _type == TextType.Time)
        {
            _time.Value = (int)GameManager.instance._stageTime;
            _time.Where(time => time > 7).Subscribe(_ => _text.color = Color.white);
            _time.Where(time => time > 5 && time <= 7).Subscribe(_ => _text.DOColor(new Color(1, 1, 0), 0.9f));
            _time.Where(time => time <= 5)
            .Subscribe(_ =>
            {
                _text.rectTransform.localScale = Vector3.zero;
                _text.rectTransform.DOScale(1, 0.9f).SetEase(Ease.OutElastic);
                _text.DOColor(new Color(1, 0, 0), 0.9f);
            });
        }

    }

    void Update()
    {
        if(_type == TextType.Time)
        {
            _time.Value = (int)GameManager.instance._stageTime;
            _text.text = TimeDisplay(GameManager.instance._stageTime);
        }
        if (_type == TextType.Steps) 
            _text.text = GameManager.instance._steps.ToString("0000");
        if (_type == TextType.ClearSteps) 
            _text.text = GameManager.instance._stepText;
        if (_type == TextType.ClearTime) 
            _text.text = GameManager.instance._timeText;
        if (_type == TextType.timeAchievement)
        {
            _text.text = $"[1]{GameManager.instance._mapEditor._stageData.timeAchievement}秒以内にクリア";
            if (GameManager.instance._isAchieved[0]())
                _text.color = Color.white;
            else _text.color = Color.red;
        }
        if (_type == TextType.stepAchievement1)
        {
            _text.text = $"[2]{GameManager.instance._mapEditor._stageData.stepAchievement1}歩以内にクリア";
            if (GameManager.instance._isAchieved[0]() && GameManager.instance._isAchieved[1]())
                _text.color = Color.white;
            else _text.color = Color.red;
        }
        if (_type == TextType.stepAchievement2)
        {
            _text.text = $"[3]{GameManager.instance._mapEditor._stageData.stepAchievement2}歩以内にクリア";
            if (GameManager.instance._isAchieved[0]() && GameManager.instance._isAchieved[1]() && GameManager.instance._isAchieved[2]())
                _text.color = Color.white;
            else _text.color = Color.red;
        }
    }
    IEnumerator MoveText()
    {
        while(true)
        {
            _text.text = _texts[_indexNumber];
            ++_indexNumber;
            _indexNumber %= _texts.Length;
            yield return new WaitForSeconds(5f);
        }
    }
    string TimeDisplay(float seconds)
    {
        int minutes = 0;
        minutes += (int)seconds / 60;
        seconds %= 60;
        minutes %= 60;
        return $"{minutes.ToString("00")}:{seconds.ToString("0.00")}";
    }
}
enum TextType
{
    Steps,
    Time,
    ClearSteps,
    ClearTime,
    MoveText,
    TimeRecord,
    StepRecord,
    timeAchievement,
    stepAchievement1,
    stepAchievement2,
    None,
}