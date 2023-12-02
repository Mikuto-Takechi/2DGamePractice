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
        GameManager.Instance.PushData += PushUndo;
        GameManager.Instance.PopData += PopUndo;
        GameManager.Instance.ReloadData += Reload;
        GameManager.Instance.MoveEnd += ChangeAnimationState;
    }
    void OnDisable()
    {
        GameManager.Instance.PushData -= PushUndo;
        GameManager.Instance.PopData -= PopUndo;
        GameManager.Instance.ReloadData -= Reload;
        GameManager.Instance.MoveEnd -= ChangeAnimationState;
    }
    private void Start()
    {
        _animator = GetComponent<Animator>();
        _vcam = FindObjectOfType<CinemachineVirtualCamera>();
        if (_vcam != null)
            _vcam.Follow = transform.GetChild(0);//�v���C���[�̎q�I�u�W�F�N�g(�\���p)��Ǐ]�ΏۂɎw�肷��
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
    /// �A�j���[�V�������Đ�
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
        ChangeAnimationState();
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