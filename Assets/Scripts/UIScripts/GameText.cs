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
        if(_type == TextType.Steps) _text.text = $"歩数：{GManager.instance.Steps}";
        if (_type == TextType.Time) _text.text = $"時間：{GManager.instance.StageTime.ToString("F2")}";
        if (_type == TextType.Records)
        {
            if(GManager.instance._stepsRecords.ContainsKey(_stageName) && GManager.instance._timeRecords.ContainsKey(_stageName))
            {
                _text.text = $"歩数：{GManager.instance._stepsRecords[_stageName]}\n時間：{GManager.instance._timeRecords[_stageName]}";
            }
            else
            {
                _text.text = "記録なし\n記録なし";
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
    MoveText,
    Records,
    None,
}