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
            //EaseOutElastic�Ȃǂ́A1�ȏ�̐��l���o��̂ŁA���̂܂�Lerp�Ŏg���ƈ��̏ꏊ�œ����Ȃ��Ȃ��Ă��܂�
            //Lerp��a�`b�̊Ԃ����l���o���Ȃ��̂�2�{���āAt�̒l��0�`1�����o���Ȃ��̂�0.5�{����B
            transform.position = Vector2.Lerp(_startPos*2, _endPos*2, EaseValue(timer, endTime, _easeType)/2);
            if(timer > endTime)
            {
                yield break;
            }
            //�������x����������̂�Update�Ɠ�����1f�҂�
            yield return null;
        }
    }
}
