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
    int _indexNumber = 0;
    void Start()
    {
        _text = GetComponent<Text>();
        if(_type == TextType.MoveText) StartCoroutine(MoveText());
    }

    void Update()
    {
        if(_type == TextType.Steps) _text.text = $"ï‡êî {GameManager.instance._steps}";
        if (_type == TextType.Time) _text.text = $"éûä‘ {GameManager.instance._stageTime.ToString("F2")}";
        if (_type == TextType.ClearSteps) _text.text = GameManager.instance._stepText;
        if (_type == TextType.ClearTime) _text.text = GameManager.instance._timeText;
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