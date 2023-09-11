using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class StageButton : MonoBehaviour
{
    /// <summary>�N���A���ԋL�^��\��������e�L�X�g</summary>
    [SerializeField] Text _timeRecord = null;
    /// <summary>�����L�^��\��������e�L�X�g</summary>
    [SerializeField] Text _stepRecord = null;
    /// <summary>���̃{�^���Ɋ��蓖�Ă�X�e�[�W��</summary>
    [SerializeField] string _stageName = "";
    private void Update()
    {
        Record(GameManager.instance._timeRecords, _timeRecord);
        Record(GameManager.instance._stepsRecords, _stepRecord);
    }
    void Record<T>(Dictionary<string, T> dic, Text text) where T : IComparable<T>, IFormattable
    {
        if (dic == null) return;
        if (text == null) return;
        if (dic.ContainsKey(_stageName) && !dic[_stageName].Equals(default(T)))
        {
            //float�^�Ȃ珑���w�肵�ĕ\��
            if (typeof(T) == typeof(float))
                text.text = dic[_stageName].ToString("F2", new CultureInfo("en-US"));
            //int�^�Ȃ炻�̂܂ܕ\��
            if (typeof(T) == typeof(int))
                text.text = dic[_stageName].ToString();
        }
        else
        {
            text.text = "- - -";
        }
    }
}
