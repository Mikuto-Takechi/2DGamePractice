using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Šî’êƒNƒ‰ƒX
/// </summary>
public abstract class ItemBase : MonoBehaviour
{
    GameObject _player;
    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player");
    }
    public abstract void ItemEffect();
    private void Update()
    {
        if (_player == null) return;
        if(transform.position == _player.transform.position)
        {
            ItemEffect();
            Destroy(gameObject);
        }
    }
}
