using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Šî’êƒNƒ‰ƒX
/// </summary>
public abstract class ItemBase : MonoBehaviour, IReload, IPushUndo, IPopUndo
{
    GameObject _player;
    bool _active = true;
    SpriteRenderer _spriteRenderer;
    Stack<bool> _activeStack = new Stack<bool>();
    protected void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player");
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }
    public abstract void ItemEffect();
    private void Update()
    {
        if (_player == null) return;
        if (!_active) return;
        if(transform.position == _player.transform.position && GManager.instance._gameState == GManager.GameState.Idle)
        {
            ItemEffect();
            _active = false;
            if(_spriteRenderer != null) _spriteRenderer.enabled = _active;
        }
    }

    public void Reload()
    {
        _active = true;
        if (_spriteRenderer != null) _spriteRenderer.enabled = _active;
        _activeStack.Clear();
    }
    public void PushUndo()
    {
        _activeStack.Push(_active);
    }

    public void PopUndo()
    {
        if (_activeStack.TryPop(out bool flag))
        {
            _active = flag;
            if (_spriteRenderer != null) _spriteRenderer.enabled = _active;
        }
    }
}
