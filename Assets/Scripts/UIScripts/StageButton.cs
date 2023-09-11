using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class StageButton : MonoBehaviour
{
    /// <summary>クリア時間記録を表示させるテキスト</summary>
    [SerializeField] Text _timeRecord = null;
    /// <summary>歩数記録を表示させるテキスト</summary>
    [SerializeField] Text _stepRecord = null;
    /// <summary>そのボタンに割り当てるステージ名</summary>
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
}
