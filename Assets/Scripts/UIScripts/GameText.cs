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
    [Tooltip("�X�e�[�W���̎w��i_textType��StageRecord�ɂȂ��Ă��Ȃ��Ɛݒ肵�Ă��Ӗ����Ȃ��j"), SerializeField] string _stageName = "";
    int _indexNumber = 0;
    void Start()
    {
        _text = GetComponent<Text>();
        if(_type == TextType.MoveText) StartCoroutine(MoveText());
    }

    void Update()
    {
        if(_type == TextType.Steps) _text.text = $"�����F{GManager.instance._steps}";
        if (_type == TextType.Time) _text.text = $"���ԁF{GManager.instance._stageTime.ToString("F2")}";
        if (_type == TextType.ClearSteps) _text.text = GManager.instance._stepText;
        if (_type == TextType.ClearTime) _text.text = GManager.instance._timeText;
        if (_type == TextType.Records)
        {
            if(GManager.instance._stepsRecords.ContainsKey(_stageName) && GManager.instance._timeRecords.ContainsKey(_stageName))
            {
                _text.text = $"�����F{GManager.instance._stepsRecords[_stageName]}\n���ԁF{GManager.instance._timeRecords[_stageName].ToString("F2")}";
            }
            else
            {
                _text.text = "�L�^�Ȃ�\n�L�^�Ȃ�";
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
    Records,
    None,
}