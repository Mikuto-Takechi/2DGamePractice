using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
//https://hirukotime.hatenablog.jp/entry/2022/11/15/205224
//https://easings.net/ja#easeOutBounce
//function easeOutElastic(x: number): number {
//const c4 = (2 * Math.PI) / 3;

//return x === 0
//  ? 0
//  : x === 1
//  ? 1
//  : Math.pow(2, -10 * x) * Math.sin((x * 10 - 0.75) * c4) + 1;
//}
public class Ease : MonoBehaviour
{
    public float _timer { get; set; } = 0f;
    [SerializeField] float _size = 1f;
    [SerializeField] float _maxTime = 1f;
    void Update()
    {
        _timer += Time.deltaTime;
        transform.localScale = new Vector3(Value(_timer, 0, _maxTime) * _size, Value(_timer, 0, _maxTime) * _size, 0);
        //transform.position = new Vector3(0, Value(_timer, 0, 1.5f) * 2, 0f);
    }
    public void Reset()
    {
        _timer = 0f;
    }
    float Value(float current, float min, float max)
    {
        if (current <= min) return 0;
        if (max <= current) return 1;

        var diff = max - min;
        var time = current - min;

        var x = time / diff;
        return EaseOutBounce(x);
    }
    float EaseOutBounce(float x)
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
    float EaseOutElastic(float x)
    {
        float c4 = (2 * Mathf.PI) / 3;
        if (x == 0)
        {
            return 0;
        }
        else if(x == 1)
        {
            return 1;
        }
        else
        {
            return Mathf.Pow(2, -10 * x) * Mathf.Sin((x * 10 - 0.75f) * c4) + 1;
        }
    }
}
