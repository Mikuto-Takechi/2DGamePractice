using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GManager;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;
/// <summary>
/// マップを管理するクラス
/// </summary>
public class MapEditor : MonoBehaviour
{
    /// <summary>マップが CSV 形式で格納されているテキスト</summary>
    [SerializeField] TextAsset _mapData = default;
    public string[,] map { get; set; }  // レベルデザイン用の配列
    public GameObject[,] _field { get; set; } // ゲーム管理用の配列
    public GameObject[,] _initialField { get; set; } // ゲーム管理用の配列の初期配置
    public List<GameObject> _items { get; set; } = new List<GameObject>();// アイテムを格納するためのリスト
    public string _mapName { get; set; } = "";//読み込んでいるマップの名前
    Stack<GameObject[,]> _fieldStack = new Stack<GameObject[,]>();
    [SerializeField] GameObject[] _wallPrefabs = default;
    [SerializeField] GameObject[] _groundPrefabs = default;
    [SerializeField] GameObject[] _itemPrefabs = default;
    [SerializeField] GameObject _playerPrefab = default;
    [SerializeField] GameObject _boxPrefab = default;
    [SerializeField] GameObject _goalPrefab = default;
    public void BuildMapData()
    {
        // C# の標準ライブラリを使って「一行ずつ読む」という処理をする
        System.IO.StringReader sr = new System.IO.StringReader(_mapData.text);
        List<string> lines = new List<string>();
        //最初の一行にステージ名が入っているので読み取る
        string _mapName = sr.ReadLine();
        while (true)
        {
            // 一行ずつ読みこんで処理する
            string line = sr.ReadLine();

            // line に何も入っていなかったら終わったとみなして処理を終わる
            if (string.IsNullOrEmpty(line))
            {
                break;
            }
            lines.Add(line);
        }
        int rows = lines.Count;
        int cols = lines[0].Split(',').Length;
        string[,] result = new string[rows, cols];
        for (int y = 0; y < rows; y++)
        {
            string[] row = lines[y].Split(',');
            for (int x = 0; x < row.Length; x++)
            {
                result[y, x] = row[x];
            }
        }
        map = result;
        Debug.Log("マップデータの読み取りが完了しました。");
    }
    public void InitializeGame()
    {
        BuildMapData();
        //フィールド用配列の領域を確保
        _field = new GameObject[map.GetLength(0), map.GetLength(1)];
        string debugText = "";
        //ジャグ配列の情報を出力
        for (int y = 0; y < map.GetLength(0); y++)
        {
            for (int x = 0; x < map.GetLength(1); x++)
            {
                string itemId = map[y, x];
                GameObject prefab = null;

                if (itemId.Contains("w"))
                    prefab = _wallPrefabs[0];
                else if (itemId.Contains("i0"))
                    prefab = _itemPrefabs[0];
                else if (itemId.Contains("i1"))
                    prefab = _itemPrefabs[1];
                else if (itemId.Contains("0"))
                    prefab = _groundPrefabs[0];
                else if (itemId.Contains("p"))
                    prefab = _playerPrefab;
                else if (itemId.Contains("b"))
                    prefab = _boxPrefab;
                else if (itemId.Contains("g"))
                    prefab = _goalPrefab;

                if (prefab)
                {
                    //そのままVector3(x, y, 0)で初期化すると上下が反転するのでmap.Length - y
                    Vector3 position = new Vector3(x, map.GetLength(0) - y, 0);
                    _field[y, x] = Instantiate(prefab, position, Quaternion.identity);
                    if(itemId.Contains("i"))
                        _items.Add(_field[y, x]);
                    Instantiate(_groundPrefabs[0], position, Quaternion.identity);   // 床を置く
                }

                debugText += map[y, x] + ", ";
            }
            debugText += "\n";  // 改行
        }

        Debug.Log(debugText);
        //初期状態の物の位置を保存する
        _initialField = new GameObject[map.GetLength(0), map.GetLength(1)];
        Array.Copy(_field, _initialField, _field.Length);

        GManager.instance._gameState = GameState.Idle;
    }
    /// <summary>
    /// プレイヤーの座標を取得する
    /// </summary>
    /// <returns></returns>
    public Vector2Int GetPlayerIndex()
    {
        for (int y = 0; y < map.GetLength(0); y++)
        {
            for (int x = 0; x < map.GetLength(1); x++)
            {
                if (_field[y, x] == null) { continue; }
                if (_field[y, x].tag == "Player")
                    return new Vector2Int(x, y);
            }
        }
        return new Vector2Int(-1, -1);
    }

    public void PushField()
    {
        //この関数は再帰呼び出しで何回か繰り返されるので、最初の1回だけ盤面を保存させる。
        GameObject[,] copyArray = new GameObject[map.GetLength(0), map.GetLength(1)];
        Array.Copy(_field, copyArray, _field.Length);
        _fieldStack.Push(copyArray);
    }
    public void PopField()
    {
        if (_fieldStack.TryPop(out var undoField))
        {
            _field = undoField;
            for (int y = 0; y < map.GetLength(0); y++)
            {
                for (int x = 0; x < map.GetLength(1); x++)
                {
                    if (_field[y, x] != null)
                    {
                        Transform obj = _field[y, x].transform;
                        Vector3 to = new Vector3(x, map.GetLength(0) - y, 0);
                        if (obj.position != to)
                        {
                            //y, x番目にあるGameobjectをx, 配列の縦の長さ-yへ移動させる
                            GManager.instance.MoveFunction(obj, to, 0.1f, 0.015f);
                        }
                    }
                }
            }
        }
    }
    /// <summary>
    /// フィールド上の物の位置を初期化する
    /// </summary>
    public void InitializeField()
    {
        GameObject[,] copyArray = new GameObject[map.GetLength(0), map.GetLength(1)];
        Array.Copy(_initialField, copyArray, _initialField.Length);
        _field = copyArray;
        for (int y = 0; y < map.GetLength(0); y++)
        {
            for (int x = 0; x < map.GetLength(1); x++)
            {
                if (_field[y, x] != null)
                {
                    _field[y, x].transform.position = new Vector3(x, map.GetLength(0) - y, 0);
                }
            }
        }
    }
    /// <summary>
    /// クリア判定
    /// </summary>
    /// <returns></returns>
    public bool IsCleared()
    {
        List<Vector2Int> goals = new List<Vector2Int>();

        for (int y = 0; y < map.GetLength(0); y++)
        {
            for (int x = 0; x < map.GetLength(1); x++)
            {
                // 格納場所か否かを判断
                if (map[y, x].Contains("g"))
                {
                    // 格納場所のインデックスを控えておく
                    goals.Add(new Vector2Int(x, y));
                }
            }
        }

        // 要素数はgoals.Countで取得
        for (int i = 0; i < goals.Count; i++)
        {
            GameObject f = _field[goals[i].y, goals[i].x];
            if (f == null || f.tag != "Moveable")
            {
                // 一つでも箱が無かったら条件未達成
                return false;
            }
        }
        return true;
    }
}
