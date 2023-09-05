using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System;
using System.Globalization;
using MessagePack;
using System.Linq;

[RequireComponent(typeof(MapEditor))]
public class GameManager : Singleton<GameManager>
{
    public int _steps { get; set; } = 0;
    public float _stageTime { get; set; } = 0;
    public GameState _gameState = GameState.Title;
    public Dictionary<string, float> _timeRecords = new Dictionary<string, float>();
    public Dictionary<string, int> _stepsRecords = new Dictionary<string, int>();
    public HashSet<string> _unlockStages = new HashSet<string>();
    GamePanel _panel;
    Queue<Vector2Int> _inputQueue = new Queue<Vector2Int>();
    /// <summary>���̓o�b�t�@�T�C�Y</summary>
    [SerializeField] int _maxQueueCount = 2;
    /// <summary>�ړ����x</summary>
    public float _moveSpeed = 1.0f;
    public float _defaultSpeed { get; set; }//�f�t�H���g�̈ړ����x
    public int _coroutineCount { get; set; } = 0;
    PlayerInput _playerInput;
    public string _timeText { get; set; }
    public string _stepText { get; set; }
    public bool _toggleQuitSave { get; set; } = false;
    bool _singleCrateSound = false;
    bool _pushField = false;
    [SerializeField] GameObject _shadow;
    public MapEditor _mapEditor { get; set; }
    List<Func<int>> _processes;
    int _initProcess = 0;
    /// <summary>�f�[�^���X�^�b�N�Ƀv�b�V������ۂɌĂ΂�郁�\�b�h</summary>
    public event Action PushData;
    /// <summary>�f�[�^���X�^�b�N����|�b�v����ۂɌĂ΂�郁�\�b�h</summary>
    public event Action PopData;
    /// <summary>�X�^�b�N�ɗ��܂��Ă���f�[�^�����Z�b�g����ۂɌĂ΂�郁�\�b�h</summary>
    public event Action ReloadData;
    /// <summary>�ړ����o�^���ɒm�点�郁�\�b�h</summary>
    public event Action<Vector2> MoveTo;
    /// <summary>�ړ��I����m�点�郁�\�b�h</summary>
    public event Action MoveEnd;
    public enum GameState
    {
        Title,
        Pause,
        Clear,
        Idle,
        Move,
    }
    public override void AwakeFunction()
    {
        _defaultSpeed = _moveSpeed;
        SceneManager.sceneLoaded += SceneLoaded;
        _mapEditor = GetComponent<MapEditor>();
        _playerInput = GetComponent<PlayerInput>();
        if (MessagePackLoad("StepRecords", out Dictionary<string, int> stepData))
            _stepsRecords = stepData;
        if (MessagePackLoad("TimeRecords", out Dictionary<string, float> timeData))
            _timeRecords = timeData;
        if (MessagePackLoad("UnlockStages", out HashSet<string> unlockData))
            _unlockStages = unlockData;
        //���������߂�lint�̌^�̃��X�g
        _processes = new List<Func<int>>
        {
            () => InputProcess(_gameInputs.Player.Up.IsPressed(), Vector2Int.down, 1),
            () => InputProcess(_gameInputs.Player.Right.IsPressed(), Vector2Int.right, 2),
            () => InputProcess(_gameInputs.Player.Down.IsPressed(), Vector2Int.up, 3),
            () => InputProcess(_gameInputs.Player.Left.IsPressed(), Vector2Int.left, 0)
        };
    }
    int InputProcess(bool flag, Vector2Int dir, int next)
    {
        if (flag && _inputQueue.Count < _maxQueueCount)
        {
            _inputQueue.Enqueue(dir);
            if (!(_inputQueue.Count < _maxQueueCount)) return next;
        }
        return _initProcess;
    }
    void SceneLoaded(Scene nextScene, LoadSceneMode mode)//�V�[�����ǂݍ��܂ꂽ���̏���
    {
        _inputQueue.Clear();//queue�̒��g������
        StopAllCoroutines();//�R���[�`����S�Ď~�߂�
        _coroutineCount = 0;//���s���̃R���[�`���̃J�E���g�����Z�b�g
        _moveSpeed = _defaultSpeed;
        _steps = 0;
        _stageTime = 0;
        _panel = FindObjectOfType<GamePanel>();
        _panel?.ChangePanel(0);
        _mapEditor._fieldStack.Clear();//�t�B�[���h�̃X�^�b�N���폜����
        _mapEditor._gimmickStack.Clear();//�M�~�b�N�̃X�^�b�N���폜����
        if (nextScene.name.Contains("Title"))
        {
            _gameState = GameState.Title;
        }
        else
        {
            _mapEditor.InitializeGame();
            //�L�^���ǉ�����Ă��Ȃ��X�e�[�W���Ȃ�ΐV����Dictionary�ɒǉ�����
            _timeRecords.TryAdd(_mapEditor._mapName, 99999);
            _stepsRecords.TryAdd(_mapEditor._mapName, 99999);
            _gameState = GameState.Idle;
        }
    }
    private void OnApplicationQuit()
    {
        if (!_toggleQuitSave)
        {
            MessagePackSave("StepRecords", _stepsRecords);
            MessagePackSave("TimeRecords", _timeRecords);
            MessagePackSave("UnlockStages", _unlockStages);
        }
    }
    private void Update()
    {
        if (_gameState == GameState.Title) return;
        if (_gameState == GameState.Clear) return;
        if (_mapEditor.IsCleared())
        {
            if (_gameState != GameState.Clear && _gameState != GameState.Move)//�N���A�㏈��
            {
                _gameState = GameState.Clear;
                _timeText = CheckRecord(_stageTime, _timeRecords);
                _stepText = CheckRecord(_steps, _stepsRecords);
                //�}�b�v�G�f�B�^�[�X�N���v�g��ǂݍ���
                MapEditor mapEditor = FindObjectOfType<MapEditor>();
                _unlockStages.Add(mapEditor._nextMapName);
                MessagePackSave("UnlockStages", _unlockStages);
                _panel.ChangePanel(1);
                _panel.Clear();
                AudioManager.instance.StopBGM();
                AudioManager.instance.PlaySound(3);
            }
            return;
        }
        if (_gameInputs.Player.Pause.triggered)
        {
            _panel?.SwitchPause();
        }
        if (_gameState == GameState.Pause) return;
        //Undo����
        if (_gameInputs.Player.Undo.IsPressed() && _coroutineCount == 0 && _gameState == GameState.Idle && _mapEditor._fieldStack.Count != 0)
        {
            _inputQueue.Clear();//queue�̒��g������
            PopData();//�o�^����Ă���X�e�[�W�����߂����\�b�h�̌Ăяo��
        }
        //���Z�b�g����
        if (_gameInputs.Player.Reset.triggered)
        {
            //����o���Ȃ�����
            _gameState = GameState.Title;
            //��ʂ����X�ɈÂ�����
            AudioManager.instance.PlaySound(7);
            Fade fade = _panel.transform.GetComponentInChildren<Fade>();
            fade?.CallFadeIn(() =>
            {
                _inputQueue.Clear();//queue�̒��g������
                StopAllCoroutines();//�R���[�`����S�Ď~�߂�
                _coroutineCount = 0;//���s���̃R���[�`���̃J�E���g�����Z�b�g
                ReloadData();//�o�^����Ă���X�e�[�W���������\�b�h�̌Ăяo��
                AudioManager.instance.PlaySound(8);
                AudioManager.instance.PlaySound(9);
                fade?.CallFadeOut(() => _gameState = GameState.Idle);
            });
        }
        // ���̓o�b�t�@�ɒǉ�����
        if (_gameState == GameState.Idle)
        {
            int count = _initProcess;
            Enumerable.Range(0, 4).ToList().ForEach(i =>
            {
                count %= 4;
                _initProcess = _processes[count]();
                count++;
            });
        }
        // �o�b�t�@������͂���������
        if (_gameState == GameState.Idle && _inputQueue.Count > 0 && _coroutineCount == 0)
        {
            if (_inputQueue.TryDequeue(out Vector2Int pos))
            {
                var playerIndex = _mapEditor.GetPlayerIndex();
                if (IsMovable(playerIndex, playerIndex + pos))
                {
                    _singleCrateSound = false;
                    _pushField = false;
                }
            }
        }
        //stageTime�ɉ��Z
        _stageTime += Time.deltaTime;
    }
    IEnumerator Move(Transform obj, Vector2 to, float endTime, Action callback)
    {
        ++_coroutineCount;
        float timer = 0;
        Vector2 from = obj.position;
        while (true)
        {
            timer += Time.deltaTime;
            float x = timer / endTime;
            obj.position = Vector2.Lerp(from, to, x);
            if (timer > endTime)
            {
                --_coroutineCount;
                callback();
                yield break;
            }
            //�������x����������̂�Update�Ɠ�����1f�҂�
            yield return null;
        }
    }
    public void MoveFunction(Transform main, Vector2 to, float endTime, float shadowInterval)
    {
        _gameState = GameState.Move;
        //�Ώۂ��v���C���[�̎� && �c����\�����鎞(�߂鎞)
        Player hasPlayerComponent = main.GetComponent<Player>();
        if (hasPlayerComponent && shadowInterval < 100)
        {
            //�v���C���[�̃A�j���[�V�������t�����ōĐ�
            Vector2 dir = to - (Vector2)main.position;
            hasPlayerComponent.PlayAnimation(Vector2Int.RoundToInt(-dir));
        }
        //�\���p�̎q�I�u�W�F�N�g���擾����B
        Transform sprite = main.GetChild(0);
        //�\���p�̃I�u�W�F�N�g�������炩�Ɉړ�������B
        StartCoroutine(Move(sprite, to, endTime, () =>
        {
            //�ړ������I����
            //���s����Ă���R���[�`���̐���0�̏ꍇ�̓v���C���[�𑀍�\�ȏ�Ԃ֖߂�
            if (_coroutineCount == 0 && _gameState == GameState.Move)
            {
                _gameState = GameState.Idle;
                if (MoveEnd != null) MoveEnd();
            }
            //�e�I�u�W�F�N�g�̍��W��ς���Ǝq�I�u�W�F�N�g�����Ă���̂Őe�q�֌W�������Ă���A
            //�e�I�u�W�F�N�g���I�_�ɔ�΂��B
            sprite.SetParent(null);
            main.position = to;
            sprite.SetParent(main);
        }));
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
    bool IsMovable(Vector2Int from, Vector2Int to)
    {
        // �c�������̔z��O�Q�Ƃ����Ă��Ȃ���
        if (to.y < 0 || to.y >= _mapEditor._field.GetLength(0))
            return false;
        if (to.x < 0 || to.x >= _mapEditor._field.GetLength(1))
            return false;   
        var name = _mapEditor._prefabsDictionary.Where(pair => pair.Key == _mapEditor._terrain[to.y, to.x])
                                                .Select(pair => pair.Value.Item2).FirstOrDefault();
        if (name == PrefabType.Wall)
            return false;   // �ړ��悪�ǂȂ瓮�����Ȃ�

        Vector2Int direction = to - from;
        var destinationObject = _mapEditor._currentField[to.y, to.x];
        if (destinationObject && destinationObject.CompareTag("Movable"))
        {
            bool success = IsMovable(to, to + direction);//�ċA�Ăяo��
            if (!success) return false;
        }
        // GameObject�̍��W(position)���ړ������Ă���C���f�b�N�X�̓���ւ�
        var gimmickObject = _mapEditor._currentGimmick[to.y, to.x];
        var targetObject = _mapEditor._currentField[from.y, from.x];
        var targetPosition = new Vector2(to.x, _mapEditor._field.GetLength(0) - to.y);
        var player = targetObject.GetComponent<Player>();

        if (_pushField == false)
        {
            PushData();//�o�^����Ă���X�e�[�W��Ԉꎞ�ۑ����\�b�h�̌Ăяo��
            _steps += 1;
            _pushField = true;
        }
        //�ړ�������I�u�W�F�N�g���v���C���[�R���|�[�l���g�������Ă����ꍇ�̏���
        if (player)
        {
            AudioManager.instance.PlaySound(1);
            // �v���C���[�̈ړ����o�^���֒m�点��
            if(MoveTo != null) MoveTo(targetPosition);
            //y�͔��]���Ă���̂�y�����̈ړ��A�j���[�V�����͔��]������
            player.PlayAnimation(direction.y == 0 ? direction : -direction);
        }
        //Movable�^�O���t���Ă������̏���
        if (targetObject.CompareTag("Movable"))
        {
            StartCoroutine(Vibration(0.0f, 1.0f, 0.07f));
            if (_singleCrateSound == false)
            {
                //�������ɌĂяo���ꂽ���A�ŏ���1�񂾂�����炷�B
                AudioManager.instance.PlaySound(0);
            }
            _singleCrateSound = true;
        }
        MoveFunction(targetObject.transform, targetPosition, _moveSpeed, 999);
        //�ړ��悪��ŉ����I�u�W�F�N�g�������̂Ȃ�
        if (name == PrefabType.Water && gimmickObject == null && targetObject.TryGetComponent(out IObjectState targetState))
        {
            targetState.ChangeState(ObjectState.UnderWater);//���ɒ���
        }
        //�I�u�W�F�N�g�������ɋ���̂Ȃ�_currentGimmick��to�̗v�f�ֈړ����Afrom�̗v�f��null�ŏ㏑������
        if (targetObject.TryGetComponent(out IObjectState targetState2) && targetState2.objectState == ObjectState.UnderWater)
        {
            AudioManager.instance.PlaySound(10);
            _mapEditor._currentGimmick[to.y, to.x] = _mapEditor._currentField[from.y, from.x];
            _mapEditor._currentField[from.y, from.x] = null;
        }
        else// ���Ȃ���Βʏ�ʂ�to�̗v�f�ֈړ����Afrom�̗v�f��null�ŏ㏑������
        {
            _mapEditor._currentField[to.y, to.x] = _mapEditor._currentField[from.y, from.x];
            _mapEditor._currentField[from.y, from.x] = null;
        }
        return true;
    }

    /// <summary>
    /// �l���L�^���X�V���Ă��邩���m�F���ĕ������Ԃ�
    /// �l���L�^���X�V���Ă�����L�^���㏑������
    /// </summary>
    string CheckRecord<T>(T current, Dictionary<string, T> dic) where T : IComparable<T>, IFormattable
    {
        string stageName = _mapEditor._mapName;
        string label = "", saveLabel = "", display = "";

        if (typeof(T) == typeof(int))
        {
            saveLabel = "StepRecords";
            label = "����";
            display = current.ToString();
        }
        if (typeof(T) == typeof(float))
        {
            saveLabel = "TimeRecords";
            label = "����";
            display = current.ToString("F2", new CultureInfo("en-US"));
        }

        string text = $"{label}�F{display}";

        if (dic[stageName].CompareTo(current) > 0)
        {
            dic[stageName] = current;
            MessagePackSave(saveLabel, dic);
            text = $"{label}�F{display} �L�^�X�V�I�I";
        }
        return text;
    }
    //�ȉ��̃��C�u�������g�p���f�[�^���V���A���C�Y����PlayerPrefs�ɕۑ��A�ǂݍ��݂��s��
    //https://github.com/MessagePack-CSharp/MessagePack-CSharp/releases
    /// <summary>
    /// PlayerPrefs�Ƀf�[�^��ۑ�����
    /// </summary>
    /// <typeparam name="T">�f�[�^�̌^</typeparam>
    /// <param name="label">�Z�[�u��</param>
    /// <param name="data">�ۑ�����f�[�^</param>
    void MessagePackSave<T>(string label, T data)
    {
        byte[] bytes = MessagePackSerializer.Serialize(data);
        var json = MessagePackSerializer.ConvertToJson(bytes);
        PlayerPrefs.SetString(label, json);
    }
    /// <summary>
    /// PlayerPrefs����f�[�^��ǂݍ���
    /// </summary>
    /// <typeparam name="T">�f�[�^�̌^</typeparam>
    /// <param name="label">�Z�[�u��</param>
    /// <param name="data">�ǂݍ��񂾃f�[�^</param>
    /// <returns>�ǂݍ��݂��������Ă��邩��bool�^�Ŗ߂�</returns>
    bool MessagePackLoad<T>(string label, out T data)
    {
        string json = PlayerPrefs.GetString(label, "");
        if (json != null && json != "")
        {
            byte[] bytes = MessagePackSerializer.ConvertFromJson(json);
            data = MessagePackSerializer.Deserialize<T>(bytes);
            return true;
        }
        else
        {
            data = default;
            return false;
        }
    }
}