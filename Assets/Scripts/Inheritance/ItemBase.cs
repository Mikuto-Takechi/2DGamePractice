using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 基底クラス
/// </summary>
public abstract class ItemBase : MonoBehaviour
{
    bool _active = true;
    SpriteRenderer _spriteRenderer;
    Stack<bool> _activeStack = new Stack<bool>();
    void OnEnable()
    {
        GameManager.instance.MoveTo += PlayerMove;
        GameManager.instance.PushData += PushUndo;
        GameManager.instance.PopData += PopUndo;
        GameManager.instance.ReloadData += Reload;
    }
    void OnDisable()
    {
        GameManager.instance.MoveTo -= PlayerMove;
        GameManager.instance.PushData -= PushUndo;
        GameManager.instance.PopData -= PopUndo;
        GameManager.instance.ReloadData -= Reload;
    }
    protected void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }
    public abstract void ItemEffect();
    /// <summary>
    /// プレイヤーが移動したときに呼ばれるメソッド
    /// </summary>
    /// <param name="pos">プレイヤーが移動した座標</param>
    public void PlayerMove(Vector2 pos)
    {
        if (!_active) return;
        if ((Vector2)transform.position == pos)
        {
            ItemEffect();
            _active = false;
            if (_spriteRenderer != null) _spriteRenderer.enabled = _active;
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
