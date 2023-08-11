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
    /// <summary>入力バッファサイズ</summary>
    [SerializeField] int _maxQueueCount = 2;
    /// <summary>移動速度</summary>
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
    void SceneLoaded(Scene nextScene, LoadSceneMode mode)//シーンが読み込まれた時の処理
    {
        _inputQueue.Clear();//queueの中身を消す
        StopAllCoroutines();//コルーチンを全て止める
        _coroutineCount = 0;//実行中のコルーチンのカウントをリセット
        _steps = 0;
        _stageTime = 0;
        _panel = FindObjectOfType<GamePanel>();
        _panel?.ChangePanel(0);
        _mapEditor._fieldStack.Clear();//フィールドのスタックを削除
        if (TryGetComponent(out IReload timeAndStep))//時間と歩数をリセットする
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
        //Debug.Log("アプリ終了");
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
            if (_gameState != GameState.Clear && _gameState != GameState.Move)//クリア後処理
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
        //Undo処理
        if (_gameInputs.Player.Undo.triggered && _coroutineCount == 0 && _gameState == GameState.Idle)
        {
            _inputQueue.Clear();//queueの中身を消す
            _mapEditor.PopField();
            if(TryGetComponent(out IPopUndo timeAndStep))//時間と歩数を取り出す
                timeAndStep.PopUndo();
            //Undo用インターフェイスの呼び出し
            foreach (GameObject obj in _mapEditor._items)
            {
                if (obj != null)
                {
                    IPopUndo i = obj.GetComponent<IPopUndo>();
                    if (i != null) i.PopUndo();
                }
            }
        }
        //リセット処理
        if (_gameInputs.Player.Reset.triggered)
        {
            //操作出来なくする
            _gameState = GameState.Title;
            //画面を徐々に暗くする
            AudioManager.instance.PlaySound(7);
            Fade fade = _panel.transform.GetComponentInChildren<Fade>();
            fade?.CallFadeIn(() =>
            {
                _inputQueue.Clear();//queueの中身を消す
                StopAllCoroutines();//コルーチンを全て止める
                _coroutineCount = 0;//実行中のコルーチンのカウントをリセット
                //フィールドを初期化してスタックのデータを削除
                _mapEditor.InitializeField();
                _mapEditor._fieldStack.Clear();
                if (TryGetComponent(out IReload timeAndStep))//時間と歩数をリセットする
                    timeAndStep.Reload();
                //リセット用インターフェイスの呼び出し
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
        // 入力バッファに追加する
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

        //1f待ってバッファから入力を処理する
        StartCoroutine(InputProcessing());

        //stageTimeに加算
        _stageTime += Time.deltaTime;
    }
    /// <summary>
    /// 1f待ってから入力を受け付ける
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
            //一定間隔で残像を生成
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
            //処理速度が速すぎるのでUpdateと同じく1f待つ
            yield return null;
        }
    }
    public void MoveFunction(Transform main, Vector2 to, float endTime, float shadowInterval)
    {
        _gameState = GameState.Move;
        //対象がプレイヤーの時 && 残像を表示する時(戻る時)
        Player hasPlayerComponent = main.GetComponent<Player>();
        if (hasPlayerComponent && shadowInterval < 100)
        {
            //プレイヤーのアニメーションを逆向きで再生
            Vector2 dir = to - (Vector2)main.position;
            hasPlayerComponent.PlayAnimation(Vector2Int.RoundToInt(-dir));
        }
        //表示用の子オブジェクトを取得する。
        Transform sprite = main.GetChild(0);
        //表示用のオブジェクトだけ滑らかに移動させる。
        StartCoroutine(Move(sprite, to, endTime, shadowInterval, () =>
        {
            //移動処理終了後
            //実行されているコルーチンの数が0の場合はプレイヤーを操作可能な状態へ戻す
            if (_coroutineCount == 0 && _gameState == GameState.Move)
            {
                _gameState = GameState.Idle;
            }
            //親オブジェクトの座標を変えると子オブジェクトもついてくるので親子関係を解いてから、
            //親オブジェクトを終点に飛ばす。
            sprite.SetParent(null);
            main.position = to;
            sprite.SetParent(main);
        }));
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
    bool IsMovable(Vector2Int from, Vector2Int to)
    {
        // 縦軸横軸の配列外参照をしていないか
        if (to.y < 0 || to.y >= _mapEditor._field.GetLength(0))
            return false;
        if (to.x < 0 || to.x >= _mapEditor._field.GetLength(1))
            return false;
        if (_mapEditor._terrain[to.y, to.x].Contains("w"))
            return false;   // 移動先が壁なら動かせない
        if (_mapEditor._terrain[to.y, to.x].Contains("r"))
            return false;   // 移動先が川なら動かせない
        Vector2Int direction = to - from;
        if (_mapEditor._currentField[to.y, to.x] != null && _mapEditor._currentField[to.y, to.x].tag == "Moveable")
        {
            bool success = IsMovable(to, to + direction);

            if (!success)
                return false;
        }

        // GameObjectの座標(position)を移動させてからインデックスの入れ替え
        var targetObject = _mapEditor._currentField[from.y, from.x];
        var targetPosition = new Vector2(to.x, _mapEditor._field.GetLength(0) - to.y);
        var player = targetObject.GetComponent<Player>();

        if (player)
        {
            AudioManager.instance.PlaySound(1);
            //yは反転しているのでy方向の移動アニメーションは反転させる
            player.PlayAnimation(direction.y == 0 ? direction : -direction);
        }
        if (targetObject.CompareTag("Moveable"))
        {
            StartCoroutine(Vibration(0.0f, 1.0f, 0.07f));
            if (_singleCrateSound == false)
            {
                //箱を元に呼び出された時、最初の1回だけ音を鳴らす。
                AudioManager.instance.PlaySound(0);
            }
            _singleCrateSound = true;
        }
        if (_pushField == false)
        {
            _mapEditor.PushField();
            if (TryGetComponent(out IPushUndo timeAndStep))//時間と歩数をスタックする
                timeAndStep.PushUndo();
            //インターフェイスの呼び出し
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
    /// 値が記録を更新しているかを確認して文字列を返す
    /// 値が記録を更新していたら記録を上書きする
    /// </summary>
    string CheckRecord<T>(T current, Dictionary<string, T> dic) where T : IComparable<T>, IFormattable
    {
        string stageName = _mapEditor._mapName;
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
    void Save<T>(string name, Dictionary<string, T> dic)
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