using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(Text))]
public class GameText : MonoBehaviour
{
    Text _text;
    [SerializeField] string[] _texts;
    [SerializeField] TextType _type = TextType.None;
    [Tooltip("ステージ名の指定（_textTypeがStageRecordになっていないと設定しても意味がない）"), SerializeField] string _stageName = "";
    int _indexNumber = 0;
    void Start()
    {
        _text = GetComponent<Text>();
        if(_type == TextType.MoveText) StartCoroutine(MoveText());
    }

    void Update()
    {
        if(_type == TextType.Steps) _text.text = $"歩数 {GameManager.instance._steps}";
        if (_type == TextType.Time) _text.text = $"時間 {GameManager.instance._stageTime.ToString("F2")}";
        if (_type == TextType.ClearSteps) _text.text = GameManager.instance._stepText;
        if (_type == TextType.ClearTime) _text.text = GameManager.instance._timeText;
        if (_type == TextType.TimeRecord)
        {
            var times = GameManager.instance._timeRecords;
            if (times == null) return;
            if(times.ContainsKey(_stageName) && times[_stageName] != default)
            {
                _text.text = $"時間：{times[_stageName].ToString("F2")}";
            }
            else
            {
                _text.text = "- - -";
            }
        }
        if(_type == TextType.StepRecord)
        {
            var steps = GameManager.instance._stepsRecords;
            if (steps == null) return;
            if (steps.ContainsKey(_stageName) && steps[_stageName] != default)
            {
                _text.text = $"歩数：{steps[_stageName]}";
            }
            else
            {
                _text.text = "- - -";
            }
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
    None,
}