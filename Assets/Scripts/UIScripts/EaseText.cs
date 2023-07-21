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
            //Scale’l‚ğ’u‚«Š·‚¦‚é
            transform.localScale = new Vector3(scale, scale, scale);
            if (timer > endTime)
            {
                yield break;
            }
            //ˆ—‘¬“x‚ª‘¬‚·‚¬‚é‚Ì‚ÅUpdate‚Æ“¯‚¶‚­1f‘Ò‚Â
            yield return null;
        }
    }
}