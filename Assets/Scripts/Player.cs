using Cinemachine;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, IObjectState
{
    Animator _animator;
    CinemachineVirtualCamera _vcam;
    public ObjectState objectState { get; set; } = ObjectState.Default;
    Stack<ObjectState> _stateStack = new Stack<ObjectState>();
    ObjectState _initState;
    void OnEnable()
    {
        GameManager.instance.PushData += PushUndo;
        GameManager.instance.PopData += PopUndo;
        GameManager.instance.ReloadData += Reload;
        GameManager.instance.MoveEnd += ChangeAnimationState;
    }
    void OnDisable()
    {
        GameManager.instance.PushData -= PushUndo;
        GameManager.instance.PopData -= PopUndo;
        GameManager.instance.ReloadData -= Reload;
        GameManager.instance.MoveEnd -= ChangeAnimationState;
    }
    private void Start()
    {
        _animator = GetComponent<Animator>();
        _vcam = FindObjectOfType<CinemachineVirtualCamera>();
        if (_vcam != null)
            _vcam.Follow = transform.GetChild(0);//プレイヤーの子オブジェクト(表示用)を追従対象に指定する
        _initState = objectState;
    }
    void ChangeAnimationState()
    {
        if (objectState == ObjectState.UnderWater)
        {
            _animator.SetBool("UnderWater", true);
        }
        else
        {
            _animator.SetBool("UnderWater", false);
        }
    }
    /// <summary>
    /// アニメーションを再生
    /// </summary>
    public void PlayAnimation(Vector2Int dir)
    {
        if(dir.y < 0)
            _animator.Play("PlayerRun");
        if (dir.y > 0)
            _animator.Play("PlayerBackRun");
        if (dir.x < 0)
            _animator.Play("PlayerLeftRun");
        if (dir.x > 0)
            _animator.Play("PlayerRightRun");
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
            ChangeAnimationState();
        }
    }

    public void ChangeState(ObjectState state)
    {
        objectState = state;
    }
}