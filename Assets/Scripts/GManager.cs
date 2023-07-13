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
    /// <summary>入力バッファサイズ</summary>
    [SerializeField] int _maxQueueCount = 2;
    /// <summary>移動処理の間隔</summary>
    [SerializeField] float _moveInterval = 0.05f;
    int _coroutineCount = 0;
    PlayerInput _playerInput;
    public string _timeText, _stepText;

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
        _playerInput = GetComponent<PlayerInput>();
        var buffer = Load<int>("StepRecords");
        if(buffer != null) _stepsRecords = buffer;
        var buffer2 = Load<float>("TimeRecords");
        if(buffer2 != null) _timeRecords = buffer2;
    }
    void SceneLoaded(Scene nextScene, LoadSceneMode mode)//シーンが読み込まれた時の処理
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
            _inputQueue.Clear();//queueの中身を消す
            StopAllCoroutines();//コルーチンを全て止める
            _coroutineCount = 0;//実行中のコルーチンのカウントをリセット
            _gameState = GameState.Idle;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        _stageTime += Time.deltaTime;
        // 入力バッファに追加する
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

        // バッファから入力を処理する
        if (_gameState == GameState.Idle && _inputQueue.Count > 0)
        {
            PushBlock((Vector2)_player.transform.position, (Vector2)_player.transform.position + _inputQueue.Dequeue());
        }
    }
    /// <summary>
    /// クリア判定
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
        if (blockOnPoints == pointCount && _gameState != GameState.Clear)//クリア後処理
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
    /// <summary>
    /// 指定した座標まで段階的に移動させる
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
        Gamepad gamepad = Gamepad.current;//ゲームパッド接続確認
        if (gamepad == null)
        {
            yield break;
        }
        if (_playerInput.currentControlScheme == "Gamepad")//ゲームパッド操作確認
        {
            gamepad.SetMotorSpeeds(left, right);
            yield return new WaitForSeconds(waitTime);
            gamepad.SetMotorSpeeds(0, 0);
        }
    }
    /// <summary>
    /// 移動の判定を取って処理につなげる
    /// </summary>
    bool PushBlock(Vector2 from, Vector2 to)
    {
        RaycastHit2D hit = PointCast(to);
        if (hit.collider && !hit.collider.CompareTag("Moveable")) return false;//移動先が壁なら処理を抜ける
        Vector2 direction = to - from;
        if (hit.collider && hit.collider.CompareTag("Moveable"))//判定が取れたオブジェクトがブロックなら再帰処理
        {
            bool success = PushBlock(to, to + direction);
            if (!success) return false;
        }

        foreach (GameObject ob in _allObjects)//座標から移動させるオブジェクトを探し出す
        {
            if (ob != null && (Vector2)ob.transform.position == from && (ob.CompareTag("Player") || ob.CompareTag("Moveable")))
            {
                Transform objTransform = ob.transform;
                _gameState = GameState.Move;
                //オブジェクトを目標の地点までスムーズに動かす↓
                StartCoroutine(Move(objTransform, to, () =>
                {
                    if (_coroutineCount == 0 && _gameState == GameState.Move)//実行されているコルーチンの数が0の場合はプレイヤーを操作可能な状態へ戻す
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
    /// Linecastを1点に絞る
    /// </summary>
    RaycastHit2D PointCast(Vector2 pos)
    {
        return Physics2D.Linecast(pos, pos);
    }
    /// <summary>
    /// 値が記録を更新しているかを確認して文字列を返す
    /// 値が記録を更新していたら記録を上書きする
    /// </summary>
    string CheckRecord<T>(T current, Dictionary<string, T> dic) where T : IComparable<T>, IFormattable
    {
        string stageName = SceneManager.GetActiveScene().name;
        string label = "", saveLabel = "", display = "";

        if (typeof(T) == typeof(int))
        {
            saveLabel = "StepRecords";
            label = "歩数";
            display = current.ToString();
        }
        if (typeof(T) == typeof(float))
        {
            saveLabel = "TimeRecords";
            label = "時間";
            display = current.ToString("F2", new CultureInfo("en-US"));
        }

        string text = $"{label}：{display}";

        //if (dic[stageName] > current)
        if (dic[stageName].CompareTo(current) > 0)
        {
            dic[stageName] = current;
            Save(saveLabel, dic);
            text = $"{label}：{display} 記録更新！！";
        }
        return text;
    }
    /// <summary>
    /// Dictionaryに保持されているデータをPlayerPrefsに保存する
    /// </summary>
    void Save<T>(string name, Dictionary<string,T> dic)
    {
        //Dictionaryをシリアル化可能な型に変換
        var jsonDictionary = new JsonDictionary<string, T>(dic);
        // インスタンス変数を JSON にシリアル化する
        var json = JsonUtility.ToJson(jsonDictionary, true);
        // PlayerPrefs に保存する
        PlayerPrefs.SetString(name, json);
    }
    /// <summary>
    /// PlayerPrefsに保存されているデータをDictionaryに戻す
    /// </summary>
    Dictionary<string, T> Load<T>(string name)
    {
        // PlayerPrefs から文字列を取り出す
        string json = PlayerPrefs.GetString(name);
        // デシリアライズする
        var jsonDictionary = JsonUtility.FromJson<JsonDictionary<string, T>>(json);
        if (jsonDictionary == null)
        {
            Debug.Log("データが入っていません");
            return null;
        }
        //Dictionary型へ戻す
        Dictionary<string, T> dictionary = jsonDictionary.Dictionary;
        return dictionary;
    }
}