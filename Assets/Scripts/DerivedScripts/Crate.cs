using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crate : MonoBehaviour, IReload, IPushUndo, IPopUndo
{
    Vector2 _initPos = Vector2.zero;
    Stack<Vector2> _moveStack = new Stack<Vector2>();
    void Start()
    {
        _initPos = transform.position;
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
            transform.position = pos;
        }
    }
}
