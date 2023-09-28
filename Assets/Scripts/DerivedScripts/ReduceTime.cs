using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// �h���N���X1
/// </summary>
public class ReduceTime : ItemBase
{
    [SerializeField] float reduceCount = 0;
    GameObject _easeText;
    private new void Start()
    {
        base.Start();
        _easeText = GameObject.Find("DisplayCanvas/MainPanel/Time");
    }
    public override void ItemEffect()//���Ԑ����g�����Ăяo�������Ă���ꏊ
    {
        if(_easeText.TryGetComponent(out EaseText text))
        {
            text.EaseStart();
        }
        if (GameManager.Instance._stageTime > 0)
        {
            float count = GameManager.Instance._stageTime;
            count += reduceCount;
            if (count <= 0)
            {
                GameManager.Instance._stageTime = 0f;
            }
            if (count > 0)
            {
                GameManager.Instance._stageTime = count;
            }
        }
    }
}
