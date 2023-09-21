using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Animator))]
public class Crate : MonoBehaviour, IObjectState
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
        var layer = GameManager.instance.mapEditor._layer;
        int x = (int)transform.position.x;
        int y = layer.GetLength(0) - (int)transform.position.y;
        if (layer[y,x].field.type == PrefabType.Target)
            _sr.color = new Color(0, 1, 1, 1);
        else
            _sr.color = new Color(0, 0, 0, 1);
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
