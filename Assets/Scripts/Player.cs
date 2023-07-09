using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Animations;
using System;

public class Player : InputBase
{
    Animator _animator;
    GameObject[] _allObjects;
    Transform _transform;
    Queue<Vector2> _inputQueue = new Queue<Vector2>();
    /// <summary>���̓o�b�t�@�T�C�Y</summary>
    [SerializeField] int _maxQueueCount = 2;
    PlayerState _playerState = PlayerState.PlayerIdle;
    int _coroutineCount = 0;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _allObjects = GameObject.FindObjectsOfType<GameObject>();
        _transform = GetComponent<Transform>();
    }
    void Update()
    {
        if (GManager.instance._gameState == GManager.GameState.Clear) return;

        // ���̓o�b�t�@�ɒǉ�����
        if (_gameInputs.Player.Up.triggered && _inputQueue.Count < _maxQueueCount)
        {
            _inputQueue.Enqueue(Vector2.up);
        }
        if (_gameInputs.Player.Down.triggered && _inputQueue.Count < _maxQueueCount)
        {
            _inputQueue.Enqueue(Vector2.down);
        }
        if (_gameInputs.Player.Right.triggered && _inputQueue.Count < _maxQueueCount)
        {
            _inputQueue.Enqueue(Vector2.right);
        }
        if (_gameInputs.Player.Left.triggered && _inputQueue.Count < _maxQueueCount)
        {
            _inputQueue.Enqueue(Vector2.left);
        }

        // �o�b�t�@������͂���������
        if (_playerState == PlayerState.PlayerIdle && _inputQueue.Count > 0)
        {
            PushBlock((Vector2)_transform.position, (Vector2)_transform.position + _inputQueue.Dequeue());
        }
    }
    IEnumerator Move(Transform obj, Vector2 to, Action callback)
    {
        ++_coroutineCount;
        while (true)
        {
            Vector2 direction = (to - (Vector2)obj.position).normalized;
            float distance = (to - (Vector2)obj.position).magnitude;
            if (distance <= 0.1f)
            {
                obj.position = to;
                --_coroutineCount;
                callback();
                yield break;
            }
            else
            {
                obj.position += (Vector3)direction / 10;
                yield return new WaitForSeconds(0.1f);
            }
        }
    }
    bool PushBlock(Vector2 from, Vector2 to)
    {
        RaycastHit2D hit = PointCast(to);
        if (hit.collider && !hit.collider.CompareTag("Moveable")) return false;//�ړ��悪�ǂȂ珈���𔲂���
        Vector2 direction = to - from;
        if (hit.collider && hit.collider.CompareTag("Moveable"))//���肪��ꂽ�I�u�W�F�N�g���u���b�N�Ȃ�ċA����
        {
            bool success = PushBlock(to, to + direction);
            if (!success) return false;
        }

        foreach (GameObject ob in _allObjects)//���W����ړ�������I�u�W�F�N�g��T���o��
        {
            if (ob !=null && (Vector2)ob.transform.position == from && (ob.CompareTag("Player") || ob.CompareTag("Moveable")))
            {
                Transform objTransform = ob.transform;
                _playerState = PlayerState.PlayerMoving;
                //�I�u�W�F�N�g��ڕW�̒n�_�܂ŃX���[�Y�ɓ�������
                StartCoroutine(Move(objTransform, to, () => 
                {
                    if (_coroutineCount == 0)//���s����Ă���R���[�`���̐���0�̏ꍇ�̓v���C���[�𑀍�\�ȏ�Ԃ֖߂�
                    {
                        _playerState = PlayerState.PlayerIdle;
                    }
                }));
                if (ob.CompareTag("Player"))
                {
                    GManager.instance.Steps += 1;
                    AudioManager.instance.PlaySound(1);
                    _animator.Play("PlayerRun");
                }
                if (ob.CompareTag("Moveable"))
                {
                    AudioManager.instance.PlaySound(0);
                }
                break;
            }
        }
        return true;
    }
    RaycastHit2D PointCast(Vector2 pos)
    {
        return Physics2D.Linecast(pos, pos);
    }
}

public enum PlayerState
{
    NotReady,
    PlayerIdle,
    PlayerMoving,
    Finished,
}