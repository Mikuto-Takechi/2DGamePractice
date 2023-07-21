using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class EaseText : Ease
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
            //Scale�l��u��������
            transform.localScale = new Vector3(scale, scale, scale);
            if (timer > endTime)
            {
                yield break;
            }
            //�������x����������̂�Update�Ɠ�����1f�҂�
            yield return null;
        }
    }
}