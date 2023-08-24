using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// ���N���X
/// </summary>
public abstract class ItemBase : MonoBehaviour/*, IReload, IPushUndo, IPopUndo*/
{
    GameObject _player;
    bool _active = true;
    SpriteRenderer _spriteRenderer;
    Stack<bool> _activeStack = new Stack<bool>();
    void OnEnable()
    {
        GameManager.instance.PushData += PushUndo;
        GameManager.instance.PopData += PopUndo;
        GameManager.instance.ReloadData += Reload;
    }
    void OnDisable()
    {
        GameManager.instance.PushData -= PushUndo;
        GameManager.instance.PopData -= PopUndo;
        GameManager.instance.ReloadData -= Reload;
    }
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
        if(transform.position == _player.transform.position && GameManager.instance._gameState == GameManager.GameState.Idle)
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
