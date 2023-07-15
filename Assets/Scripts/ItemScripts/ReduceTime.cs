using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 派生クラス1
/// </summary>
public class ReduceTime : ItemBase
{
    [SerializeField] float reduceCount = 0;
    public override void ItemEffect()//多態性を使った呼び出しをしている場所
    {
        if (GManager.instance._stageTime > 0)
        {
            float count = GManager.instance._stageTime;
            count += reduceCount;
            if (count <= 0)
            {
                GManager.instance._stageTime = 0f;
            }
            if (count > 0)
            {
                GManager.instance._stageTime = count;
            }
        }
    }
}
