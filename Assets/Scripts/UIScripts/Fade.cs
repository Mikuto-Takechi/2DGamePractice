using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class Fade : MonoBehaviour
{
    Image _image;
    [SerializeField] float _fadeSpeed = 1.0f;
    void Start()
    {
        _image = GetComponent<Image>();
        Color defaultColor = _image.color;
        defaultColor.a = 0;
        _image.color = defaultColor;
    }
    public void CallFadeIn(Action callback)
    {
        StartCoroutine(FadeStart(true, () => callback()));
    }
    public void CallFadeOut(Action callback)
    {
        StartCoroutine(FadeStart(false, () => callback()));
    }
    IEnumerator FadeStart(bool fadeIn, Action callback)
    {
        if(fadeIn == true)
        {
            //フェードイン
            while (true)
            {
                Color panelColor = _image.color;
                panelColor.a += _fadeSpeed * Time.deltaTime;
                _image.color = panelColor;
                if (panelColor.a > 1)
                {
                    panelColor.a = 1;
                    _image.color = panelColor;
                    callback();
                    yield break;
                }
                yield return null;
            }
        }
        else
        {
            //フェードアウト
            while (true)
            {
                Color panelColor = _image.color;
                panelColor.a -= _fadeSpeed * Time.deltaTime;
                _image.color = panelColor;
                if (panelColor.a < 0)
                {
                    panelColor.a = 0;
                    _image.color = panelColor;
                    callback();
                    yield break;
                }
                yield return null;
            }
        }
    }
}
