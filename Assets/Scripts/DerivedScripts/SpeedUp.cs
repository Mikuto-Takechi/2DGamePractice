using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedUp : ItemBase
{
    //変更後のスピード
    [SerializeField] float _changeSpeed = 0.1f;
    //効果時間
    [SerializeField] float _effectTime = 1f;
    Coroutine _coroutine;
    public override void ItemEffect()
    {
        GameManager.instance._moveSpeed = _changeSpeed;
        if(_coroutine != null) StopCoroutine(_coroutine);
        _coroutine = StartCoroutine(EffectTime());
    }
    IEnumerator EffectTime()
    {
        float timer = 0f;
        while (true)
        {
            timer += Time.deltaTime;
            if (timer > _effectTime)
            {
                GameManager.instance._moveSpeed = GameManager.instance._defaultSpeed;
                yield break;
            }
            yield return null;
        }
    }
}
