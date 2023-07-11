using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System;

public class GManager : Singleton<GManager>
{
    int _steps = 0;
    float _stageTime = 0;
    public GameState _gameState = GameState.Title;
    public Dictionary<string, float> _timeRecords = new Dictionary<string, float>();
    public Dictionary<string, int> _stepsRecords = new Dictionary<string, int>();
    GamePanel _panel;

    GameObject[] _allObjects;
    GameObject _player;
    List<GameObject> _blockPoints = new List<GameObject>();
    List<GameObject> _blocks = new List<GameObject>();
    Queue<Vector2> _inputQueue = new Queue<Vector2>();
    /// <summary>���̓o�b�t�@�T�C�Y</summary>
    [SerializeField] int _maxQueueCount = 2;
    /// <summary>�ړ������̊Ԋu</summary>
    [SerializeField] float _moveInterval = 0.05f;
    int _coroutineCount = 0;
    PlayerInput _playerInput;
    public enum GameState
    {
        Title,
        Pause,
        Clear,
        Idle,
        Move,
    }
    public int Steps
    {
        get { return _steps; }
        set { _steps = value; }
    }
    public float StageTime
    {
        get { return _stageTime; }
        set { _stageTime = value; } 
    }
    public override void AwakeFunction()
    {
        SceneManager.sceneLoaded += SceneLoaded;
    }
    void SceneLoaded(Scene nextScene, LoadSceneMode mode)//�V�[�����ǂݍ��܂ꂽ���̏���
    {
        if (!_timeRecords.ContainsKey(nextScene.name) && nextScene.name.Contains("Stage")) _timeRecords.Add(nextScene.name, 99999.99f);
        if (!_stepsRecords.ContainsKey(nextScene.name) && nextScene.name.Contains("Stage")) _stepsRecords.Add(nextScene.name, 99999);
        _steps = 0;
        _stageTime = 0;
        _panel = FindObjectOfType<GamePanel>();
        _panel?.ChangePanel(0);
        if(nextScene.name.Contains("Title"))
        {
            _gameState = GameState.Title;
        }
        if (nextScene.name.Contains("Stage"))
        {
            _gameState = GameState.Idle;
            _allObjects = FindObjectsOfType<GameObject>();
            _player = GameObject.Find("Player");
            _blockPoints = new List<GameObject>();
            _blocks = new List<GameObject>();
            foreach (GameObject obj in _allObjects)
            {
                if (obj.CompareTag("BlockPoint"))
                {
                    _blockPoints.Add(obj);
                }
                if (obj.CompareTag("Moveable"))
                {
                    _blocks.Add(obj);
                }
            }
        }
    }
    private void Update()
    {
        if (_gameState == GameState.Title) return;
        IsCleard();
        if (_gameState == GameState.Clear) return;
        if (_gameState == GameState.Pause) return;

        if (_gameInputs.Player.Reset.triggered)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        _stageTime += Time.deltaTime;
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
        if (_gameState == GameState.Idle && _inputQueue.Count > 0)
        {
            PushBlock((Vector2)_player.transform.position, (Vector2)_player.transform.position + _inputQueue.Dequeue());
        }
    }
    /// <summary>
    /// �N���A����
    /// </summary>
    void IsCleard()
    {
        int pointCount = _blockPoints.Count;
        int blockOnPoints = 0;
        foreach (GameObject obj in _blockPoints)
        {
            Transform pointTransform = obj.transform;
            foreach (GameObject block in _blocks)
            {
                if (pointTransform.position == block.transform.position)
                {
                    ++blockOnPoints;
                }
            }
        }
        if (blockOnPoints == pointCount && _gameState != GameState.Clear)//�N���A�㏈��
        {
            _gameState = GameState.Clear;
            _timeRecords[SceneManager.GetActiveScene().name] = _stageTime;
            _stepsRecords[SceneManager.GetActiveScene().name] = _steps;
            _panel.ChangePanel(1);
            _panel.Clear();
            AudioManager.instance.StopBGM();
            AudioManager.instance.PlaySound(3);
        }
    }
    /// <summary>
    /// �w�肵�����W�܂Œi�K�I�Ɉړ�������
    /// </summary>
    IEnumerator Move(Transform obj, Vector2 to, Action callback)
    {
        ++_coroutineCount;
        var wait = new WaitForSeconds(_moveInterval);
        while (true)
        {
            Vector2 direction = (to - (Vector2)obj.position).normalized;
            float distance = (to - (Vector2)obj.position).sqrMagnitude;
            if (distance <= 0.1f * 0.1f)
            {
                obj.position = to;
                --_coroutineCount;
                callback();
                yield break;
            }
            else
            {
                obj.position += (Vector3)direction / 10;
                yield return wait;
            }
        }
    }
    IEnumerator Vibration(float left, float right, float waitTime)
    {
        Gamepad gamepad = Gamepad.current;//�Q�[���p�b�h�ڑ��m�F
        if (gamepad == null)
        {
            yield break;
        }
        if (_playerInput.currentControlScheme == "Gamepad")//�Q�[���p�b�h����m�F
        {
            gamepad.SetMotorSpeeds(left, right);
            yield return new WaitForSeconds(waitTime);
            gamepad.SetMotorSpeeds(0, 0);
        }
    }
    /// <summary>
    /// �ړ��̔��������ď����ɂȂ���
    /// </summary>
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
            if (ob != null && (Vector2)ob.transform.position == from && (ob.CompareTag("Player") || ob.CompareTag("Moveable")))
            {
                Transform objTransform = ob.transform;
                _gameState = GameState.Move;
                //�I�u�W�F�N�g��ڕW�̒n�_�܂ŃX���[�Y�ɓ�������
                StartCoroutine(Move(objTransform, to, () =>
                {
                    if (_coroutineCount == 0 && _gameState == GameState.Move)//���s����Ă���R���[�`���̐���0�̏ꍇ�̓v���C���[�𑀍�\�ȏ�Ԃ֖߂�
                    {
                        _gameState = GameState.Idle;
                    }
                }));
                if (ob.CompareTag("Player"))
                {
                    _steps += 1;
                    AudioManager.instance.PlaySound(1);
                    _player.GetComponent<Player>().PlayAnimation();
                }
                if (ob.CompareTag("Moveable"))
                {
                    StartCoroutine(Vibration(0.0f, 1.0f, 0.07f));
                    AudioManager.instance.PlaySound(0);
                }
                break;
            }
        }
        return true;
    }
    /// <summary>
    /// Linecast��1�_�ɍi��
    /// </summary>
    RaycastHit2D PointCast(Vector2 pos)
    {
        return Physics2D.Linecast(pos, pos);
    }
}