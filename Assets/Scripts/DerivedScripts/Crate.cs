using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Animator))]
public class Crate : MonoBehaviour, /*IReload, IPushUndo, IPopUndo,*/ IObjectState
{
    public ObjectState objectState { get; set; } = ObjectState.Default;
    Stack<ObjectState> _stateStack = new Stack<ObjectState>();
    ObjectState _initState;
    Animator _animator;
    SpriteRenderer _sr;

    void Awake()
    {
        _initState = objectState;
        _animator = GetComponent<Animator>();
        _sr = transform.GetComponentInChildren<SpriteRenderer>();
        _sr.color = new Color(0,1,1,1);
    }
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
    private void Update()
    {
        if(objectState == ObjectState.UnderWater)
        {
            _animator.SetBool("UnderWater", true);
        }
        else
        {
            _animator.SetBool("UnderWater", false);
        }
    }
    public void Reload()
    {
        objectState = _initState;
        _stateStack.Clear();
    }
    public void PushUndo()
    {
        _stateStack.Push(objectState);
    }

    public void PopUndo()
    {
        if (_stateStack.TryPop(out ObjectState state))
        {
            objectState = state;
        }
    }

    public void ChangeState(ObjectState state)
    {
        objectState = state;
    }
}
