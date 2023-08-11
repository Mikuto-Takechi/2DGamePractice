using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System;
using Kogane;
using System.Globalization;
[RequireComponent(typeof(MapEditor))]
[RequireComponent(typeof(TimeAndStepStack))]
public class GManager : Singleton<GManager>
{
    public int _steps { get; set; } = 0;
    public float _stageTime { get; set; } = 0;
    public GameState _gameState = GameState.Title;
    public Dictionary<string, float> _timeRecords = new Dictionary<string, float>();
    public Dictionary<string, int> _stepsRecords = new Dictionary<string, int>();
    GamePanel _panel;
    Queue<Vector2Int> _inputQueue = new Queue<Vector2Int>();
    /// <summary>���̓o�b�t�@�T�C�Y</summary>
    [SerializeField] int _maxQueueCount = 2;
    /// <summary>�ړ����x</summary>
    [SerializeField] float _moveSpeed = 1.0f;
    public int _coroutineCount { get; set; } = 0;
    PlayerInput _playerInput;
    public string _timeText, _stepText;
    public bool _toggleQuitSave { get; set; } = false;
    bool _singleCrateSound = false;
    bool _pushField = false;
    [SerializeField] GameObject _shadow;
    MapEditor _mapEditor;
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
        SceneManager.sceneLoaded += SceneLoaded;
        _mapEditor = GetComponent<MapEditor>();
        _playerInput = GetComponent<PlayerInput>();
        var buffer = Load<int>("StepRecords");
        if (buffer != null) _stepsRecords = buffer;
        var buffer2 = Load<float>("TimeRecords");
        if (buffer2 != null) _timeRecords = buffer2;
    }
    void SceneLoaded(Scene nextScene, LoadSceneMode mode)//�V�[�����ǂݍ��܂ꂽ���̏���
    {
        _inputQueue.Clear();//queue�̒��g������
        StopAllCoroutines();//�R���[�`����S�Ď~�߂�
        _coroutineCount = 0;//���s���̃R���[�`���̃J�E���g�����Z�b�g
        _steps = 0;
        _stageTime = 0;
        _panel = FindObjectOfType<GamePanel>();
        _panel?.ChangePanel(0);
        _mapEditor._fieldStack.Clear();//�t�B�[���h�̃X�^�b�N���폜
        if (TryGetComponent(out IReload timeAndStep))//���Ԃƕ��������Z�b�g����
            timeAndStep.Reload();
        if (nextScene.name.Contains("Title"))
        {
            _gameState = GameState.Title;
        }
        else
        {
            _mapEditor.InitializeGame();
            if (!_timeRecords.ContainsKey(_mapEditor._mapName)) _timeRecords.Add(_mapEditor._mapName, 99999.99f);
            if (!_stepsRecords.ContainsKey(_mapEditor._mapName)) _stepsRecords.Add(_mapEditor._mapName, 99999);
            _gameState = GameState.Idle;
        }
    }
    private void OnApplicationQuit()
    {
        //Debug.Log("�A�v���I��");
        if (!_toggleQuitSave)
        {
            Save("StepRecords", _stepsRecords);
            Save("TimeRecords", _timeRecords);
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
        if (_gameInputs.Player.Undo.triggered && _coroutineCount == 0 && _gameState == GameState.Idle)
        {
            _inputQueue.Clear();//queue�̒��g������
            _mapEditor.PopField();
            if(TryGetComponent(out IPopUndo timeAndStep))//���Ԃƕ��������o��
                timeAndStep.PopUndo();
            //Undo�p�C���^�[�t�F�C�X�̌Ăяo��
            foreach (GameObject obj in _mapEditor._items)
            {
                if (obj != null)
                {
                    IPopUndo i = obj.GetComponent<IPopUndo>();
                    if (i != null) i.PopUndo();
                }
            }
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
                //�t�B�[���h�����������ăX�^�b�N�̃f�[�^���폜
                _mapEditor.InitializeField();
                _mapEditor._fieldStack.Clear();
                if (TryGetComponent(out IReload timeAndStep))//���Ԃƕ��������Z�b�g����
                    timeAndStep.Reload();
                //���Z�b�g�p�C���^�[�t�F�C�X�̌Ăяo��
                foreach (GameObject obj in _mapEditor._items)
                {
                    if (obj != null)
                    {
                        IReload i = obj.GetComponent<IReload>();
                        if (i != null) i.Reload();
                    }
                }
                AudioManager.instance.PlaySound(8);
                AudioManager.instance.PlaySound(9);
                fade?.CallFadeOut(() => _gameState = GameState.Idle);
            });
        }
        // ���̓o�b�t�@�ɒǉ�����
        if (_gameInputs.Player.Up.triggered && _inputQueue.Count < _maxQueueCount)
        {
            _inputQueue.Enqueue(Vector2Int.down);
        }
        if (_gameInputs.Player.Down.triggered && _inputQueue.Count < _maxQueueCount)
        {
            _inputQueue.Enqueue(Vector2Int.up);
        }
        if (_gameInputs.Player.Right.triggered && _inputQueue.Count < _maxQueueCount)
        {
            _inputQueue.Enqueue(Vector2Int.right);
        }
        if (_gameInputs.Player.Left.triggered && _inputQueue.Count < _maxQueueCount)
        {
            _inputQueue.Enqueue(Vector2Int.left);
        }

        //1f�҂��ăo�b�t�@������͂���������
        StartCoroutine(InputProcessing());

        //stageTime�ɉ��Z
        _stageTime += Time.deltaTime;
    }
    /// <summary>
    /// 1f�҂��Ă�����͂��󂯕t����
    /// </summary>
    /// <returns></returns>
    IEnumerator InputProcessing()
    {
        yield return null;
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
    }
    IEnumerator Move(Transform obj, Vector2 to, float endTime, float shadowInterval, Action callback)
    {
        ++_coroutineCount;
        float timer = 0, shadowTimer = 0;
        Vector2 from = obj.position;
        SpriteRenderer objSR = obj.GetComponent<SpriteRenderer>();
        while (true)
        {
            timer += Time.deltaTime;
            shadowTimer += Time.deltaTime;
            float x = timer / endTime;
            //���Ԋu�Ŏc���𐶐�
            if (shadowTimer > shadowInterval)
            {
                GameObject shadow = Instantiate(_shadow, obj.position, Quaternion.identity);
                SpriteRenderer shadowSR = shadow.GetComponent<SpriteRenderer>();
                shadowSR.sprite = objSR.sprite;
                shadowSR.flipX = objSR.flipX;
                shadowTimer = 0;
            }
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
        StartCoroutine(Move(sprite, to, endTime, shadowInterval, () =>
        {
            //�ړ������I����
            //���s����Ă���R���[�`���̐���0�̏ꍇ�̓v���C���[�𑀍�\�ȏ�Ԃ֖߂�
            if (_coroutineCount == 0 && _gameState == GameState.Move)
            {
                _gameState = GameState.Idle;
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
        if (_mapEditor._terrain[to.y, to.x].Contains("w"))
            return false;   // �ړ��悪�ǂȂ瓮�����Ȃ�
        if (_mapEditor._terrain[to.y, to.x].Contains("r"))
            return false;   // �ړ��悪��Ȃ瓮�����Ȃ�
        Vector2Int direction = to - from;
        if (_mapEditor._currentField[to.y, to.x] != null && _mapEditor._currentField[to.y, to.x].tag == "Moveable")
        {
            bool success = IsMovable(to, to + direction);

            if (!success)
                return false;
        }

        // GameObject�̍��W(position)���ړ������Ă���C���f�b�N�X�̓���ւ�
        var targetObject = _mapEditor._currentField[from.y, from.x];
        var targetPosition = new Vector2(to.x, _mapEditor._field.GetLength(0) - to.y);
        var player = targetObject.GetComponent<Player>();

        if (player)
        {
            AudioManager.instance.PlaySound(1);
            //y�͔��]���Ă���̂�y�����̈ړ��A�j���[�V�����͔��]������
            player.PlayAnimation(direction.y == 0 ? direction : -direction);
        }
        if (targetObject.CompareTag("Moveable"))
        {
            StartCoroutine(Vibration(0.0f, 1.0f, 0.07f));
            if (_singleCrateSound == false)
            {
                //�������ɌĂяo���ꂽ���A�ŏ���1�񂾂�����炷�B
                AudioManager.instance.PlaySound(0);
            }
            _singleCrateSound = true;
        }
        if (_pushField == false)
        {
            _mapEditor.PushField();
            if (TryGetComponent(out IPushUndo timeAndStep))//���Ԃƕ������X�^�b�N����
                timeAndStep.PushUndo();
            //�C���^�[�t�F�C�X�̌Ăяo��
            foreach (GameObject obj in _mapEditor._items)
            {
                if (obj != null)
                {
                    IPushUndo i = obj.GetComponent<IPushUndo>();
                    if (i != null) i.PushUndo();
                }
            }
            _steps += 1;
            _pushField = true;
        }
        MoveFunction(targetObject.transform, targetPosition, _moveSpeed, 999);
        _mapEditor._currentField[to.y, to.x] = _mapEditor._currentField[from.y, from.x];
        _mapEditor._currentField[from.y, from.x] = null;
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

        //if (dic[stageName] > current)
        if (dic[stageName].CompareTo(current) > 0)
        {
            dic[stageName] = current;
            Save(saveLabel, dic);
            text = $"{label}�F{display} �L�^�X�V�I�I";
        }
        return text;
    }
    /// <summary>
    /// Dictionary�ɕێ�����Ă���f�[�^��PlayerPrefs�ɕۑ�����
    /// </summary>
    void Save<T>(string name, Dictionary<string, T> dic)
    {
        //Dictionary���V���A�����\�Ȍ^�ɕϊ�
        var jsonDictionary = new JsonDictionary<string, T>(dic);
        // �C���X�^���X�ϐ��� JSON �ɃV���A��������
        var json = JsonUtility.ToJson(jsonDictionary, true);
        // PlayerPrefs �ɕۑ�����
        PlayerPrefs.SetString(name, json);
    }
    /// <summary>
    /// PlayerPrefs�ɕۑ�����Ă���f�[�^��Dictionary�ɖ߂�
    /// </summary>
    Dictionary<string, T> Load<T>(string name)
    {
        // PlayerPrefs ���當��������o��
        string json = PlayerPrefs.GetString(name);
        // �f�V���A���C�Y����
        var jsonDictionary = JsonUtility.FromJson<JsonDictionary<string, T>>(json);
        if (jsonDictionary == null)
        {
            Debug.Log("�f�[�^�������Ă��܂���");
            return null;
        }
        //Dictionary�^�֖߂�
        Dictionary<string, T> dictionary = jsonDictionary.Dictionary;
        return dictionary;
    }
}