using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 派生クラス2
/// </summary>
public class ReduceStepCount : ItemBase
{
    [SerializeField] int reduceCount = 0;
    public override void ItemEffect()//多態性を使った呼び出しをしている場所
    {
        if(GManager.instance.Steps > 0)
        {
            int count = GManager.instance.Steps;
            count += reduceCount;
            if(count <= 0)
            {
                GManager.instance.Steps = 0;
            }
            if(count > 0)
            {
                GManager.instance.Steps = count;
            }
        }
    }
}
