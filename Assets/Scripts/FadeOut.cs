using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(SpriteRenderer))]
public class FadeOut : MonoBehaviour
{
    SpriteRenderer _sprite;
    float _timer = 0;
    [SerializeField] float _destroyTime = 1f;
    void Start()
    {
        _sprite = GetComponent<SpriteRenderer>();
        Color color = _sprite.color;
        color.a = 0.5f;
        _sprite.color = color;
    }
    void Update()
    {
        _timer += Time.deltaTime;
        Color color = _sprite.color;
        color.a = 1 - _timer / _destroyTime;
        _sprite.color = color;
        if (_timer > _destroyTime) Destroy(gameObject);
    }
}
