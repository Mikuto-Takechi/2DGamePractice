using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// �h���N���X1
/// </summary>
public class ReduceTime : ItemBase
{
    [SerializeField] float reduceCount = 0;
    public override void ItemEffect()//���Ԑ����g�����Ăяo�������Ă���ꏊ
    {
        if (GManager.instance.StageTime > 0)
        {
            float count = GManager.instance.StageTime;
            count += reduceCount;
            if (count <= 0)
            {
                GManager.instance.StageTime = 0f;
            }
            if (count > 0)
            {
                GManager.instance.StageTime = count;
            }
        }
    }
}
