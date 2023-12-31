using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class EaseText : MyEase
{
    [SerializeField] float _easeTime = 1f;
    public void EaseStart()
    {
        StopAllCoroutines();
        StartCoroutine(EaseCoroutine(_easeTime));
    }
    IEnumerator EaseCoroutine(float endTime)
    {
        float timer = 0;
        while (true)
        {
            timer += Time.deltaTime;
            float scale = EaseValue(timer, endTime, _easeType);
            //Scale値を置き換える
            transform.localScale = new Vector3(scale, scale, scale);
            if (timer > endTime)
            {
                yield break;
            }
            //処理速度が速すぎるのでUpdateと同じく1f待つ
            yield return null;
        }
    }
}