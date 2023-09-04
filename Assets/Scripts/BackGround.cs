using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 背景をスクロールするためのコンポーネント。
/// 適当なオブジェクトにアタッチして使う。
/// 背景を指定すると、それをもう一つ複製する。元と複製したものを下にスクロールし、画面下に消えると上から出てくるようになる。
/// 従って、背景画像は上下に繋げてもよいものでなくてはならない。
/// </summary>
public class BackGround : MonoBehaviour
{
    [SerializeField] Transform _parent = null;
    /// <summary>背景</summary>
    [SerializeField] Image _backgroundSprite = null;
    /// <summary>背景のスクロール速度</summary>
    [SerializeField] float _scrollSpeedX = -1f;
    /// <summary>背景をクローンしたものを入れておく変数</summary>
    Image _backgroundSpriteClone;
    /// <summary>背景座標の初期値</summary>
    float _initialPositionX;

    void Start()
    {
        _initialPositionX = _backgroundSprite.transform.localPosition.x;   // 座標の初期値を保存しておく

        // 背景画像を複製して上に並べる
        _backgroundSpriteClone = Instantiate(_backgroundSprite, _parent);
        _backgroundSpriteClone.transform.SetAsFirstSibling();
        _backgroundSpriteClone.transform.Translate(_backgroundSprite.rectTransform.rect.width, 0f, 0f);
    }

    void Update()
    {
        // 背景画像をスクロールする
        _backgroundSprite.transform.Translate(_scrollSpeedX * Time.deltaTime, 0f, 0f);
        _backgroundSpriteClone.transform.Translate(_scrollSpeedX * Time.deltaTime, 0f, 0f);
        // 背景画像がある程度下に降りたら、上に戻す
        if (_backgroundSprite.transform.localPosition.x > _initialPositionX + _backgroundSprite.rectTransform.rect.width)
        {
            _backgroundSprite.transform.Translate(-_backgroundSprite.rectTransform.rect.width * 2, 0f, 0f);
        }

        // 背景画像のクローンがある程度下に降りたら、上に戻す
        if (_backgroundSpriteClone.transform.localPosition.x > _initialPositionX + _backgroundSpriteClone.rectTransform.rect.width)
        {
            _backgroundSpriteClone.transform.Translate(-_backgroundSpriteClone.rectTransform.rect.width * 2, 0f, 0f);
        }
    }
}