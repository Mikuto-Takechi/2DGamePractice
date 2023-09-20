using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//https://hirukotime.hatenablog.jp/entry/2022/11/15/205224
//https://easings.net/ja#easeOutBounce
public class MyEase : MonoBehaviour
{
    [SerializeField] protected EaseType _easeType;
    protected enum EaseType
    {
        EaseOutBounce,
        EaseOutElastic,
    }
    protected float EaseValue(float currentTimer, float maxTime, EaseType type)
    {
        //���݂̃^�C�}�[��0�ȉ��ɂȂ�����0��Ԃ��B
        if (currentTimer <= 0) return 0;
        //���݂̃^�C�}�[���ő�l�ȏ�ɂȂ�����1��Ԃ�
        if (maxTime <= currentTimer) return 1;

        //���݂̃^�C�}�[/�ő�l
        var x = currentTimer / maxTime;
        //�C�[�W���O�֐��ɒl��n���ċA���Ă������ʂ�Ԃ�
        switch (type)
        {
            case EaseType.EaseOutBounce:
                return EaseOutBounce(x);
            case EaseType.EaseOutElastic:
                return EaseOutElastic(x);
        }
        return 0;
    }
    protected float EaseOutBounce(float x)
    {
        float n1 = 7.5625f;
        float d1 = 2.75f;
        if (x < 1 / d1)
        {
            return n1 * x * x;
        }
        else if (x < 2 / d1)
        {
            return n1 * (x -= 1.5f / d1) * x + 0.75f;
        }
        else if (x < 2.5 / d1)
        {
            return n1 * (x -= 2.25f / d1) * x + 0.9375f;
        }
        else
        {
            return n1 * (x -= 2.625f / d1) * x + 0.984375f;
        }
    }
    protected float EaseOutElastic(float x)
    {
        float c4 = (2 * Mathf.PI) / 3;
        if (x == 0)
        {
            return 0;
        }
        else if (x == 1)
        {
            return 1;
        }
        else
        {
            return Mathf.Pow(2, -10 * x) * Mathf.Sin((x * 10 - 0.75f) * c4) + 1;
        }
    }
}
