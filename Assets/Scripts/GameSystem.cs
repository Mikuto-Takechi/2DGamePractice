//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using System;
//using UnityEngine.InputSystem;
//using UnityEngine.SceneManagement;

//public class GameSystem : InputBase
//{
//    GameObject[] _allObjects;
//    GameObject _player;
//    List<GameObject> _blockPoints = new List<GameObject>();
//    List<GameObject> _blocks = new List<GameObject>();
//    Queue<Vector2> _inputQueue = new Queue<Vector2>();
//    /// <summary>入力バッファサイズ</summary>
//    [SerializeField] int _maxQueueCount = 2;
//    /// <summary>移動処理の間隔</summary>
//    [SerializeField] float _moveInterval = 0.05f;
//    int _coroutineCount = 0;
//    PlayerInput _playerInput;
//    void Start()
//    {
//        _allObjects = FindObjectsOfType<GameObject>();
//        _player = GameObject.Find("Player");
//        _playerInput = GetComponent<PlayerInput>();
//        foreach (GameObject obj in _allObjects)
//        {
//            if (obj.CompareTag("BlockPoint"))
//            {
//                _blockPoints.Add(obj);
//            }
//            if (obj.CompareTag("Moveable"))
//            {
//                _blocks.Add(obj);
//            }
//        }
//    }

//    void Update()
//    {
//        IsCleard();
//        if (GManager.instance._gameState == GManager.GameState.Clear) return;
//        if (GManager.instance._gameState == GManager.GameState.Pause) return;

//        if (_gameInputs.Player.Reset.triggered)
//        {
//            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
//        }

//        // 入力バッファに追加する
//        if (_gameInputs.Player.Up.triggered && _inputQueue.Count < _maxQueueCount)
//        {
//            _inputQueue.Enqueue(Vector2.up);
//        }
//        if (_gameInputs.Player.Down.triggered && _inputQueue.Count < _maxQueueCount)
//        {
//            _inputQueue.Enqueue(Vector2.down);
//        }
//        if (_gameInputs.Player.Right.triggered && _inputQueue.Count < _maxQueueCount)
//        {
//            _inputQueue.Enqueue(Vector2.right);
//        }
//        if (_gameInputs.Player.Left.triggered && _inputQueue.Count < _maxQueueCount)
//        {
//            _inputQueue.Enqueue(Vector2.left);
//        }

//        // バッファから入力を処理する
//        if (GManager.instance._gameState == GManager.GameState.Idle && _inputQueue.Count > 0)
//        {
//            PushBlock((Vector2)_player.transform.position, (Vector2)_player.transform.position + _inputQueue.Dequeue());
//        }
//    }
//    /// <summary>
//    /// クリア判定
//    /// </summary>
//    void IsCleard()
//    {
//        int pointCount = _blockPoints.Count;
//        int blockOnPoints = 0;
//        foreach (GameObject obj in _blockPoints)
//        {
//            Transform pointTransform = obj.transform;
//            foreach (GameObject block in _blocks)
//            {
//                if (pointTransform.position == block.transform.position)
//                {
//                    ++blockOnPoints;
//                }
//            }
//        }
//        if (blockOnPoints == pointCount && GManager.instance._gameState != GManager.GameState.Clear)
//        {
//            GManager.instance._gameState = GManager.GameState.Clear;
//            AudioManager.instance.StopBGM();
//            AudioManager.instance.PlaySound(3);
//        }
//    }
//    /// <summary>
//    /// 指定した座標まで段階的に移動させる
//    /// </summary>
//    IEnumerator Move(Transform obj, Vector2 to, Action callback)
//    {
//        ++_coroutineCount;
//        var wait = new WaitForSeconds(_moveInterval);
//        while (true)
//        {
//            Vector2 direction = (to - (Vector2)obj.position).normalized;
//            float distance = (to - (Vector2)obj.position).sqrMagnitude;
//            if (distance <= 0.1f * 0.1f)
//            {
//                obj.position = to;
//                --_coroutineCount;
//                callback();
//                yield break;
//            }
//            else
//            {
//                obj.position += (Vector3)direction / 10;
//                yield return wait;
//            }
//        }
//    }
//    IEnumerator Vibration(float left, float right, float waitTime)
//    {
//        Gamepad gamepad = Gamepad.current;//ゲームパッド接続確認
//        if (gamepad == null)
//        {
//            yield break;
//        }
//        if(_playerInput.currentControlScheme == "Gamepad")//ゲームパッド操作確認
//        {
//            gamepad.SetMotorSpeeds(left, right);
//            yield return new WaitForSeconds(waitTime);
//            gamepad.SetMotorSpeeds(0, 0);
//        }
//    }
//    /// <summary>
//    /// 移動の判定を取って処理につなげる
//    /// </summary>
//    bool PushBlock(Vector2 from, Vector2 to)
//    {
//        RaycastHit2D hit = PointCast(to);
//        if (hit.collider && !hit.collider.CompareTag("Moveable")) return false;//移動先が壁なら処理を抜ける
//        Vector2 direction = to - from;
//        if (hit.collider && hit.collider.CompareTag("Moveable"))//判定が取れたオブジェクトがブロックなら再帰処理
//        {
//            bool success = PushBlock(to, to + direction);
//            if (!success) return false;
//        }

//        foreach (GameObject ob in _allObjects)//座標から移動させるオブジェクトを探し出す
//        {
//            if (ob != null && (Vector2)ob.transform.position == from && (ob.CompareTag("Player") || ob.CompareTag("Moveable")))
//            {
//                Transform objTransform = ob.transform;
//                GManager.instance._gameState = GManager.GameState.Move;
//                //オブジェクトを目標の地点までスムーズに動かす↓
//                StartCoroutine(Move(objTransform, to, () =>
//                {
//                    if (_coroutineCount == 0 && GManager.instance._gameState == GManager.GameState.Move)//実行されているコルーチンの数が0の場合はプレイヤーを操作可能な状態へ戻す
//                    {
//                        GManager.instance._gameState = GManager.GameState.Idle;
//                    }
//                }));
//                if (ob.CompareTag("Player"))
//                {
//                    GManager.instance.Steps += 1;
//                    AudioManager.instance.PlaySound(1);
//                    _player.GetComponent<Player>().PlayAnimation();
//                }
//                if (ob.CompareTag("Moveable"))
//                {
//                    StartCoroutine(Vibration(0.0f, 1.0f, 0.07f));
//                    AudioManager.instance.PlaySound(0);
//                }
//                break;
//            }
//        }
//        return true;
//    }
//    /// <summary>
//    /// Linecastを1点に絞る
//    /// </summary>
//    RaycastHit2D PointCast(Vector2 pos)
//    {
//        return Physics2D.Linecast(pos, pos);
//    }
//}
