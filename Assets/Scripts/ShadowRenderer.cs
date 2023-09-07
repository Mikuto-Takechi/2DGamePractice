using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 残像を表示するコンポーネント
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class ShadowRenderer : MonoBehaviour
{
    [SerializeField] SpriteRenderer _shadow = null;
    [SerializeField] float _shadowInterval = 0.1f;
    [SerializeField] Material[] _materials = null;
    Color _shadowColor = default;
    public Color _externalColor { get; set; }
    public bool _shadowEnabled = false;
    SpriteRenderer _sr = null;
    float timer = 0;
    void OnEnable()
    {
        GameManager.instance.PopData += ShadowEnabled;
        GameManager.instance.MoveEnd += ShadowDisable;
    }
    void OnDisable()
    {
        GameManager.instance.PopData -= ShadowEnabled;
        GameManager.instance.MoveEnd -= ShadowDisable;
    }

    private void Start()
    {
        _sr = GetComponent<SpriteRenderer>();
    }
    void Update()
    {
        timer += Time.deltaTime;
        //一定間隔で残像を生成
        if (timer > _shadowInterval && _shadowEnabled)
        {
            SpriteRenderer shadow = Instantiate(_shadow, transform.position, Quaternion.identity);
            shadow.sprite = _sr.sprite;
            shadow.material = _materials[1];
            shadow.color = _shadowColor;
            shadow.flipX = _sr.flipX;
            timer = 0;
        }
    }
    void ShadowEnabled()
    {
        _shadowEnabled = true;
        _shadowColor = Color.black;
    }
    void ShadowDisable()
    {
        _shadowEnabled = false;
        if(_externalColor != null)_shadowColor = _externalColor;
    }
}
