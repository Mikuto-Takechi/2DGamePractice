using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 派生クラス2
/// </summary>
public class ReduceStepCount : ItemBase
{
    [SerializeField] int reduceCount = 0;
    GameObject _easeText;
    private new void Start()
    {
        base.Start();
        _easeText = GameObject.Find("DisplayCanvas/MainPanel/Steps");
    }
    public override void ItemEffect()//多態性を使った呼び出しをしている場所
    {
        if (_easeText.TryGetComponent(out EaseText text))
        {
            text.EaseStart();
        }
        if (GameManager.Instance._steps > 0)
        {
            int count = GameManager.Instance._steps;
            count += reduceCount;
            if(count <= 0)
            {
                GameManager.Instance._steps = 0;
            }
            if(count > 0)
            {
                GameManager.Instance._steps = count;
            }
        }
    }
}
