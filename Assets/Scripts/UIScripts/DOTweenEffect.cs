using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class DOTweenEffect : MonoBehaviour
{
    [SerializeField] EffectType _effectType;
    void Start()
    {
        if(TryGetComponent(out Text text))
        {
            if (_effectType == EffectType.Blink)
                text.DOFade(0.0f, 1.0f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
        }
    }
    enum EffectType
    {
        Blink,
    }
}
