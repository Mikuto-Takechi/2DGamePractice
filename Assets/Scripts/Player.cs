using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, IReload, IPushUndo, IPopUndo
{
    Animator _animator;
    Vector2 _initPos = Vector2.zero;
    Stack<Vector2> _moveStack = new Stack<Vector2>();
    private void Start()
    {
        _animator = GetComponent<Animator>();
        _initPos = transform.position;
    }
    /// <summary>
    /// アニメーションを再生
    /// </summary>
    public void PlayAnimation()
    {
        _animator.Play("PlayerRun");
    }

    public void Reload()
    {
        transform.position = _initPos;
        _moveStack.Clear();
    }

    public void PushUndo()
    {
        _moveStack.Push(transform.position);
    }

    public void PopUndo()
    {
        if (_moveStack.TryPop(out Vector2 pos))
        {
            if ((Vector2)transform.position == pos) return;
            AudioManager.instance.PlaySound(6);
            GManager.instance.MoveFunction(transform, pos, 0.1f, 0.015f);
        }
    }
}