using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// �h���N���X2
/// </summary>
public class ReduceStepCount : ItemBase
{
    [SerializeField] int reduceCount = 0;
    public override void ItemEffect()//���Ԑ����g�����Ăяo�������Ă���ꏊ
    {
        if(GManager.instance._steps > 0)
        {
            int count = GManager.instance._steps;
            count += reduceCount;
            if(count <= 0)
            {
                GManager.instance._steps = 0;
            }
            if(count > 0)
            {
                GManager.instance._steps = count;
            }
        }
    }
}
