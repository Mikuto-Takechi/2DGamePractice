using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LerpTest : MyEase
{
    [SerializeField] Transform _startTrans;
    [SerializeField] Transform _endTrans;
    [SerializeField] float _easeTime = 1f;

    Vector2 _startPos;
    Vector2 _endPos;

    void Start()
    {
        _startPos = _startTrans.position;
        _endPos = _endTrans.position;
        StartCoroutine(EaseCoroutine(_easeTime));
    }
    public void EaseButton()
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
            //EaseOutElasticなどは、1以上の数値が出るので、そのままLerpで使うと一定の場所で動かなくなってしまう
            //Lerpはa～bの間しか値を出せないので2倍して、tの値も0～1しか出せないので0.5倍する。
            transform.position = Vector2.Lerp(_startPos*2, _endPos*2, EaseValue(timer, endTime, _easeType)/2);
            if(timer > endTime)
            {
                yield break;
            }
            //処理速度が速すぎるのでUpdateと同じく1f待つ
            yield return null;
        }
    }
}
