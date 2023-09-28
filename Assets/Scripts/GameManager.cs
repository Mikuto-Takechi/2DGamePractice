using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System;
using static MyNamespace.MessagePackMethods;
using System.Linq;
using UnityEngine.UI;
using DG.Tweening;
using UniRx;
using Takechi;

[RequireComponent(typeof(MapEditor))]
public class GameManager : Singleton<GameManager>
{
    public int _steps { get; set; } = 0;
    public float _stageTime { get; set; } = 0;
    public GameState _gameState = GameState.Title;
    public SaveData _saveData { get; set; } = new SaveData();
    GamePanel _panel;
    Queue<Vector2Int> _inputQueue = new Queue<Vector2Int>();
    /// <summary>入力バッファサイズ</summary>
    [SerializeField] int _maxQueueCount = 2;
    /// <summary>移動速度</summary>
    public float _moveSpeed = 1.0f;
    public float _defaultSpeed { get; set; }//デフォルトの移動速度
    public int _coroutineCount { get; set; } = 0;
    PlayerInput _playerInput;
    bool _singleCrateSound = false;
    bool _pushField = false;
    [SerializeField] GameObject _shadow;
    public MapEditor MapEditor { get; set; }
    List<Func<int>> _inputProcesses;
    int _initInputProcess = 0;
    /// <summary>実績が解除されているかを判定してbool型で返すメソッドを入れるリスト</summary>
    public List<Func<bool>> _isAchieved { get; set; } = null;
    /// <summary>データをスタックにプッシュする際に呼ばれるメソッド</summary>
    public event Action PushData;
    /// <summary>データをスタックからポップする際に呼ばれるメソッド</summary>
    public event Action PopData;
    /// <summary>スタックに溜まっているデータをリセットする際に呼ばれるメソッド</summary>
    public event Action ReloadData;
    /// <summary>移動先を登録元に知らせるメソッド</summary>
    public event Action<Vector2> MoveTo;
    /// <summary>移動終了を知らせるメソッド</summary>
    public event Action MoveEnd;
    /// <summary>記録更新を知らせるメソッド</summary>
    public event Action<TextType> NewRecord;
    /// <summary>ゲームクリアを知らせるメソッド</summary>
    public event Action GameClear;
    public override void AwakeFunction()
    {
        _defaultSpeed = _moveSpeed;
        SceneManager.sceneLoaded += SceneLoaded;
        MapEditor = GetComponent<MapEditor>();
        _playerInput = GetComponent<PlayerInput>();
        if (MessagePackLoad("StepRecords", out Dictionary<string, int> stepData))
            _saveData.StepRecords = stepData;
        if (MessagePackLoad("TimeRecords", out Dictionary<string, float> timeData))
            _saveData.TimeRecords = timeData;
        if (MessagePackLoad("UnlockStages", out HashSet<string> unlockData))
            _saveData.UnlockStages = unlockData;
        if (MessagePackLoad("Achievements", out Dictionary<string, Stars> achievementsData))
            _saveData.Missions = achievementsData;
        // GameObjectが破棄されるまで繰り返す
        Observable.Timer(TimeSpan.FromSeconds(10))
            .DoOnCompleted(Save)
            .RepeatUntilDestroy(gameObject)
            .Subscribe()
            .AddTo(this);
        //引数無し戻り値intの型のリスト
        _inputProcesses = new List<Func<int>>
        {
            () => InputProcess(_gameInputs.Player.Up.IsPressed(), Vector2Int.down, 1),
            () => InputProcess(_gameInputs.Player.Right.IsPressed(), Vector2Int.right, 2),
            () => InputProcess(_gameInputs.Player.Down.IsPressed(), Vector2Int.up, 3),
            () => InputProcess(_gameInputs.Player.Left.IsPressed(), Vector2Int.left, 0)
        };
        _isAchieved = new List<Func<bool>>
        {
            () => MapEditor._stageData.timeLimit - _stageTime <= MapEditor._stageData.timeAchievement,
            () => _steps <= MapEditor._stageData.stepAchievement1,
            () => _steps <= MapEditor._stageData.stepAchievement2
        };
    }
    int InputProcess(bool flag, Vector2Int dir, int next)
    {
        if (flag && _inputQueue.Count < _maxQueueCount)
        {
            _inputQueue.Enqueue(dir);
            if (!(_inputQueue.Count < _maxQueueCount)) return next;
        }
        return _initInputProcess;
    }
    void SceneLoaded(Scene nextScene, LoadSceneMode mode)//シーンが読み込まれた時の処理
    {
        _inputQueue.Clear();//queueの中身を消す
        StopAllCoroutines();//コルーチンを全て止める
        _coroutineCount = 0;//実行中のコルーチンのカウントをリセット
        _moveSpeed = _defaultSpeed;
        _steps = 0;
        _panel = FindObjectOfType<GamePanel>();
        _panel?.ChangePanel(0);
        MapEditor._fieldStack.Clear();//フィールドのスタックを削除する
        MapEditor._gimmickStack.Clear();//ギミックのスタックを削除する
        if (nextScene.name.Contains("Title"))
        {
            _gameState = GameState.Title;
        }
        else
        {
            MapEditor.InitializeGame();
            //記録が追加されていないステージ名ならば新しくDictionaryに追加する
            _saveData.TimeRecords.TryAdd(MapEditor._stageData.name, MaxValue.floatValue);
            _saveData.StepRecords.TryAdd(MapEditor._stageData.name, MaxValue.intValue);
            _saveData.Missions.TryAdd(MapEditor._stageData.name, 0);
            _stageTime = MapEditor._stageData.timeLimit;//制限時間を設定する
            _gameState = GameState.Idle;
        }
    }
    private void OnApplicationQuit()
    {
        Save();
    }
    private void Update()
    {
        if (_gameState == GameState.Title) return;
        if (_gameState == GameState.Clear) return;
        if (MapEditor.IsCleared())
        {
            if (_gameState != GameState.Move)//クリア後処理
            {
                _gameState = GameState.Clear;
                GameClear();
                CheckRecord(MapEditor._stageData.timeLimit - _stageTime, _saveData.TimeRecords);
                CheckRecord(_steps, _saveData.StepRecords);
                CheckAchievements(_saveData.Missions, MapEditor._stageData.name);
                MessagePackSave("Achievements", _saveData.Missions);
                _saveData.UnlockStages.Add(MapEditor._stageData.next);
                MessagePackSave("UnlockStages", _saveData.UnlockStages);
                _panel.ChangePanel(1);
                _panel.Clear();
                AudioManager.Instance.StopBGM();
                AudioManager.Instance.PlaySound(3);
            }
            return;
        }
        if (_gameState == GameState.TimeOver) return;
        if (_gameInputs.Player.Pause.triggered)
        {
            _panel?.SwitchPause();
        }
        if(_stageTime < 0 && _gameState != GameState.Move)
        {
            _panel?.ChangePanel(4);
            AudioManager.Instance.StopBGM();
            AudioManager.Instance.PlaySound(13);
            _gameState = GameState.TimeOver;
        }
        if (_gameState == GameState.Pause) return;
        //Undo処理
        if (_gameInputs.Player.Undo.IsPressed() && _coroutineCount == 0 && _gameState == GameState.Idle && MapEditor._fieldStack.Count != 0)
        {
            _inputQueue.Clear();//queueの中身を消す
            PopData();//登録されているステージ巻き戻しメソッドの呼び出し
        }
        //リセット処理
        if (_gameInputs.Player.Reset.triggered)
        {
            _gameState = GameState.Title;// 操作できなくさせる
            SceneChanger.Instance.ResetGame();// 画面をフェードさせる
        }
        // 入力バッファに追加する
        if (_gameState == GameState.Idle)
        {
            int count = _initInputProcess;
            Enumerable.Range(0, 4).ToList().ForEach(i =>
            {
                count %= 4;
                _initInputProcess = _inputProcesses[count]();
                count++;
            });
        }
        // バッファから入力を処理する
        if (_gameState == GameState.Idle && _inputQueue.Count > 0 && _coroutineCount == 0)
        {
            if (_inputQueue.TryDequeue(out Vector2Int pos))
            {
                var playerIndex = MapEditor.GetPlayerIndex();
                var moveToIndex = playerIndex + pos;
                if(playerIndex != new Vector2Int(-1, -1))// プレイヤーのインデックスが取得出来ている場合の処理
                {
                    var player = MapEditor._layer[playerIndex.y, playerIndex.x].currentField.GetComponent<Player>();
                    player.PlayAnimation(pos.y == 0 ? pos : -pos);// yが反転しているのでy方向の移動アニメーションは反転させる
                    if (IsMovable(playerIndex, moveToIndex))
                    {
                        _singleCrateSound = false;
                        _pushField = false;
                    }
                    else// 移動ができなかった場合の処理
                    {
                        _gameState = GameState.Move;
                        AudioManager.Instance.PlaySound(14);
                        Vector2 moveToIndexHalf = (Vector2)playerIndex + (Vector2)pos / 3;
                        var moveToPos = new Vector2(moveToIndexHalf.x, MapEditor._layer.GetLength(0) - moveToIndexHalf.y);
                        player.transform.DOMove(moveToPos, 0.1f).SetLoops(2, LoopType.Yoyo).OnComplete(() => _gameState = GameState.Idle);
                    }
                }
            }
        }
        //stageTimeに加算
        _stageTime -= Time.deltaTime;
    }
    public void ResetGame()
    {
        _inputQueue.Clear();// queueの中身を消す
        StopAllCoroutines();// コルーチンを全て止める
        _coroutineCount = 0;// 実行中のコルーチンのカウントをリセット
        ReloadData();// 登録されているステージ初期化メソッドの呼び出し
        _steps = 0;// 歩数を初期化
        _stageTime = MapEditor._stageData.timeLimit;// 制限時間を初期化
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
            //処理速度が速すぎるのでUpdateと同じく1f待つ
            yield return null;
        }
    }
    public void MoveFunction(Transform main, Vector2 to, float endTime)
    {
        _gameState = GameState.Move;
        //表示用の子オブジェクトを取得する。
        Transform sprite = main.GetChild(0);
        //表示用のオブジェクトだけ滑らかに移動させる。
        StartCoroutine(Move(sprite, to, endTime, () =>
        {
            //移動処理終了後
            //実行されているコルーチンの数が0の場合はプレイヤーを操作可能な状態へ戻す
            if (_coroutineCount == 0 && _gameState == GameState.Move)
            {
                _gameState = GameState.Idle;
                if (MoveEnd != null) MoveEnd();
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
            yield break;
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
        if (to.y < 0 || to.y >= MapEditor._layer.GetLength(0))
            return false;
        if (to.x < 0 || to.x >= MapEditor._layer.GetLength(1))
            return false;
        if (MapEditor._layer[to.y, to.x].terrain.type == PrefabType.Wall)
            return false;

        Vector2Int direction = to - from;
        var destinationObject = MapEditor._layer[to.y, to.x].currentField;
        if (destinationObject && destinationObject.CompareTag("Movable"))
        {
            bool success = IsMovable(to, to + direction);//再帰呼び出し
            if (!success) return false;
        }
        // GameObjectの座標(position)を移動させてからインデックスの入れ替え
        var gimmickObject = MapEditor._layer[to.y, to.x].currentGimmick;
        var targetObject = MapEditor._layer[from.y, from.x].currentField;
        var targetPosition = new Vector2(to.x, MapEditor._layer.GetLength(0) - to.y);
        var player = targetObject.GetComponent<Player>();

        if (_pushField == false)
        {
            PushData();//登録されているステージ状態一時保存メソッドの呼び出し
            _steps += 1;
            _pushField = true;
        }
        //移動させるオブジェクトがプレイヤーコンポーネントを持っていた場合の処理
        if (player)
        {
            AudioManager.Instance.PlaySound(1);
            // プレイヤーの移動先を登録元へ知らせる
            if(MoveTo != null) MoveTo(targetPosition);
        }
        //Movableタグが付いていた時の処理
        if (targetObject.CompareTag("Movable"))
        {
            StartCoroutine(Vibration(0.0f, 1.0f, 0.07f));
            if (_singleCrateSound == false)
            {
                //箱を元に呼び出された時、最初の1回だけ音を鳴らす。
                AudioManager.Instance.PlaySound(0);
            }
            _singleCrateSound = true;
        }
        MoveFunction(targetObject.transform, targetPosition, _moveSpeed);
        //移動先が川で何もオブジェクトが無いのなら
        if (MapEditor._layer[to.y, to.x].terrain.type == PrefabType.Water && gimmickObject == null && targetObject.TryGetComponent(out IObjectState targetState))
        {
            targetState.ChangeState(ObjectState.UnderWater);//水に沈む
        }
        //オブジェクトが水中に居るのなら_currentGimmickのtoの要素へ移動し、fromの要素はnullで上書きする
        if (targetObject.TryGetComponent(out IObjectState targetState2) && targetState2.objectState == ObjectState.UnderWater)
        {
            AudioManager.Instance.PlaySound(10);
            MapEditor._layer[to.y, to.x].currentGimmick = MapEditor._layer[from.y, from.x].currentField;
            MapEditor._layer[from.y, from.x].currentField = null;
        }
        else// 居なければ通常通りtoの要素へ移動し、fromの要素はnullで上書きする
        {
            MapEditor._layer[to.y, to.x].currentField = MapEditor._layer[from.y, from.x].currentField;
            MapEditor._layer[from.y, from.x].currentField = null;
        }
        return true;
    }

    /// <summary>
    /// 値が記録を更新しているかを確認して文字列を返す
    /// 値が記録を更新していたら記録を上書きする
    /// </summary>
    void CheckRecord<T>(T current, Dictionary<string, T> dic) where T : IComparable<T>
    {
        string stageName = MapEditor._stageData.name;
        string saveLabel = "";
        TextType textType = 0;

        if (typeof(T) == typeof(int))
        {
            saveLabel = "StepRecords";
            textType =  TextType.ClearSteps;
        }
        if (typeof(T) == typeof(float))
        {
            saveLabel = "TimeRecords";
            textType = TextType.ClearTime;
        }
        // 記録更新
        if (dic[stageName].CompareTo(current) > 0)
        {
            NewRecord(textType);
            dic[stageName] = current;
            MessagePackSave(saveLabel, dic);
        }
    }
    /// <summary>
    /// 実績に到達しているかを確認してディクショナリを書き換える
    /// </summary>
    void CheckAchievements(Dictionary<string, Stars> dic , string key)
    {
        if (_isAchieved[0]())
        {
            dic[key] |= Stars.Star1;
            if (_isAchieved[1]())
            {
                dic[key] |= Stars.Star2;
                if (_isAchieved[2]())
                {
                    dic[key] |= Stars.Star3;
                }
            }
        }
    }
    public void Save()
    {
        //Debug.Log("ゲームデータセーブ");
        MessagePackSave("StepRecords", _saveData.StepRecords);
        MessagePackSave("TimeRecords", _saveData.TimeRecords);
        MessagePackSave("UnlockStages", _saveData.UnlockStages);
        MessagePackSave("Achievements", _saveData.Missions);
    }
    public void DeleteSave()
    {
        PlayerPrefs.DeleteKey("StepRecords");
        PlayerPrefs.DeleteKey("TimeRecords");
        PlayerPrefs.DeleteKey("UnlockStages");
        PlayerPrefs.DeleteKey("Achievements");
        _saveData.TimeRecords.Clear();
        _saveData.StepRecords.Clear();
        _saveData.Missions.Clear();
        _saveData.UnlockStages.Clear();
    }
}