using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System;
using Kogane;
using System.Globalization;

public class GManager : Singleton<GManager>
{
    public int _steps { get; set; } = 0;
    public float _stageTime { get; set; } = 0;
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
    ///// <summary>�ړ������̊Ԋu</summary>
    //[SerializeField] float _moveInterval = 0.05f;
    [SerializeField] float _moveSpeed = 1.0f;
    int _coroutineCount = 0;
    PlayerInput _playerInput;
    public string _timeText, _stepText;
    public bool _toggleQuitSave { get; set; } = false;
    bool _singleCrateSound = false;

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
        _playerInput = GetComponent<PlayerInput>();
        var buffer = Load<int>("StepRecords");
        if(buffer != null) _stepsRecords = buffer;
        var buffer2 = Load<float>("TimeRecords");
        if(buffer2 != null) _timeRecords = buffer2;
    }
    void SceneLoaded(Scene nextScene, LoadSceneMode mode)//�V�[�����ǂݍ��܂ꂽ���̏���
    {
        if (!_timeRecords.ContainsKey(nextScene.name) && nextScene.name.Contains("Stage")) _timeRecords.Add(nextScene.name, 99999.99f);
        if (!_stepsRecords.ContainsKey(nextScene.name) && nextScene.name.Contains("Stage")) _stepsRecords.Add(nextScene.name, 99999);
        _inputQueue.Clear();//queue�̒��g������
        StopAllCoroutines();//�R���[�`����S�Ď~�߂�
        _coroutineCount = 0;//���s���̃R���[�`���̃J�E���g�����Z�b�g
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
    private void OnApplicationQuit()
    {
        //Debug.Log("�A�v���I��");
        if(!_toggleQuitSave)
        {
            Save("StepRecords", _stepsRecords);
            Save("TimeRecords", _timeRecords);
        }
    }
    private void Update()
    {
        if (_gameState == GameState.Title) return;
        IsCleard();
        if (_gameState == GameState.Clear) return;
        if (_gameState == GameState.Pause) return;
        //Undo����
        if (_gameInputs.Player.Undo.triggered)
        {
            _inputQueue.Clear();//queue�̒��g������
            StopAllCoroutines();//�R���[�`����S�Ď~�߂�
            _coroutineCount = 0;//���s���̃R���[�`���̃J�E���g�����Z�b�g
            //Undo�p�C���^�[�t�F�C�X�̌Ăяo��
            foreach (GameObject obj in _allObjects)
            {
                //�V���O���g���p�^�[����_allObjects�ɕK��1��null���܂܂�Ă���̂�null�`�F�b�N
                if (obj != null)
                {
                    IPopUndo i = obj.GetComponent<IPopUndo>();
                    if (i != null) i.PopUndo();
                }
            }
            _gameState = GameState.Idle;
        }
        //���Z�b�g����
        if (_gameInputs.Player.Reset.triggered)
        {
            _inputQueue.Clear();//queue�̒��g������
            StopAllCoroutines();//�R���[�`����S�Ď~�߂�
            _coroutineCount = 0;//���s���̃R���[�`���̃J�E���g�����Z�b�g
            //���Z�b�g�p�C���^�[�t�F�C�X�̌Ăяo��
            foreach (GameObject obj in _allObjects)
            {
                //�V���O���g���p�^�[����_allObjects�ɕK��1��null���܂܂�Ă���̂�null�`�F�b�N
                if (obj != null)
                {
                    IReload i = obj.GetComponent<IReload>();
                    if (i != null) i.Reload();
                }
            }
            _gameState = GameState.Idle;
        }
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
            if(_inputQueue.TryDequeue(out Vector2 pos))
            {
                if (PushBlock((Vector2)_player.transform.position, (Vector2)_player.transform.position + pos))
                {
                    _singleCrateSound = false;
                }
            }
        }
        //stageTime�ɉ��Z
        _stageTime += Time.deltaTime;
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
            _timeText = CheckRecord(_stageTime, _timeRecords);
            _stepText = CheckRecord(_steps, _stepsRecords);
            _panel.ChangePanel(1);
            _panel.Clear();
            AudioManager.instance.StopBGM();
            AudioManager.instance.PlaySound(3);
        }
    }
    IEnumerator MoveSecond(Transform obj, Vector2 to, float endTime, Action callback)
    {
        ++_coroutineCount;
        float timer = 0;
        Vector2 from = obj.position;
        //�s���L�^��ۑ����邽�߂�1�t���[���҂�
        yield return null;
        while (true)
        {
            timer += Time.deltaTime;
            float x = timer / endTime;
            obj.transform.position = Vector2.Lerp(from, to, x);
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
    //IEnumerator Move(Transform obj, Vector2 to, Action callback)
    //{
    //    ++_coroutineCount;
    //    var wait = new WaitForSeconds(_moveInterval);
    //    //�s���L�^��ۑ����邽�߂�1�t���[���҂�
    //    yield return null;
    //    while (true)
    //    {
    //        Vector2 direction = (to - (Vector2)obj.position).normalized;
    //        float distance = (to - (Vector2)obj.position).sqrMagnitude;
    //        if (distance <= 0.1f * 0.1f)
    //        {
    //            obj.position = to;
    //            --_coroutineCount;
    //            callback();
    //            yield break;
    //        }
    //        else
    //        {
    //            obj.position += (Vector3)direction / 10;
    //            yield return wait;
    //        }
    //    }
    //}
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
                //�v���C���[��1�̂������Ȃ��̂�1�񂾂����s�����
                if (ob.CompareTag("Player"))
                {
                    //�S�Ă̍s���L�^�C���^�[�t�F�C�X�Ăяo��
                    foreach (GameObject obj in _allObjects)
                    {
                        //�V���O���g���p�^�[����_allObjects�ɕK��1��null���܂܂�Ă���̂�null�`�F�b�N
                        if(obj != null)
                        {
                            IPushUndo i = obj.GetComponent<IPushUndo>();
                            if (i != null) i.PushUndo();
                        }
                    }
                    _steps += 1;
                    AudioManager.instance.PlaySound(1);
                    _player.GetComponent<Player>().PlayAnimation();
                }
                //�u���b�N
                if (ob.CompareTag("Moveable"))
                {
                    StartCoroutine(Vibration(0.0f, 1.0f, 0.07f));
                    if (_singleCrateSound == false) AudioManager.instance.PlaySound(0);
                    _singleCrateSound = true;
                }
                _gameState = GameState.Move;
                //�I�u�W�F�N�g��ڕW�̒n�_�܂ŃX���[�Y�ɓ�������
                Transform objTransform = ob.transform;
                StartCoroutine(MoveSecond(objTransform, to, _moveSpeed,() =>
                {
                    if (_coroutineCount == 0 && _gameState == GameState.Move)//���s����Ă���R���[�`���̐���0�̏ꍇ�̓v���C���[�𑀍�\�ȏ�Ԃ֖߂�
                    {
                        _gameState = GameState.Idle;
                    }
                }));
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
    /// <summary>
    /// �l���L�^���X�V���Ă��邩���m�F���ĕ������Ԃ�
    /// �l���L�^���X�V���Ă�����L�^���㏑������
    /// </summary>
    string CheckRecord<T>(T current, Dictionary<string, T> dic) where T : IComparable<T>, IFormattable
    {
        string stageName = SceneManager.GetActiveScene().name;
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
    void Save<T>(string name, Dictionary<string,T> dic)
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