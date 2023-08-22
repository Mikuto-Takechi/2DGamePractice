using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameManager;
using System.Linq;
using System.Xml.Linq;
using UnityEngine.InputSystem;
/// <summary>
/// マップを管理するクラス
/// </summary>
public class MapEditor : MonoBehaviour
{
    public string[,] _field { get; set; }  // アイテムの初期配置やゴールの場所の配列(layer1)
    public string[,] _terrain { get; set; }  // 地形データ用の配列(layer0)
    public GameObject[,] _currentField { get; set; } // ゲーム管理用の配列
    public GameObject[,] _initialField { get; set; } // ゲーム管理用の配列の初期配置
    public List<GameObject> _items { get; set; } = new List<GameObject>();// アイテムを格納するためのリスト
    public string _mapName { get; set; } = "";//読み込んでいるマップの名前
    public string _nextMapName { get; set; } = "";//次のマップの名前
    TextAsset[] _allMap;
    public Stack<GameObject[,]> _fieldStack { get; set; } = new Stack<GameObject[,]>();
    [SerializeField] GameObject[] _wallPrefabs = default;
    [SerializeField] GameObject[] _groundPrefabs = default;
    [SerializeField] GameObject[] _itemPrefabs = default;
    [SerializeField] GameObject _playerPrefab = default;
    [SerializeField] GameObject _boxPrefab = default;
    [SerializeField] GameObject _targetPrefab = default;
    private void Awake()
    {
        _allMap = Resources.LoadAll<TextAsset>("Levels");
    }
    public bool BuildMapData(string stageName)
    {
        //全てのマップを一つずつ処理をする
        foreach (TextAsset map in _allMap)
        {
            XElement mapXml = XElement.Parse(map.text);
            //一番最初のタグ<map>の属性nameに指定した値が無いなら次のループへ移動する
            if (mapXml.Attribute("name").Value != stageName)
                continue;
            //読み込み始めるマップの名前と次のステージの名前を登録
            _mapName = mapXml.Attribute("name").Value;
            _nextMapName = mapXml.Attribute("next").Value;
            //マップの幅と縦の大きさを取り出す
            int width = int.Parse(mapXml.Attribute("width").Value);
            int height = int.Parse(mapXml.Attribute("height").Value);
            //layerのデータを取り出す
            IEnumerable<XElement> layers = from item in mapXml.Elements("layer")
                                           select item;
            //配列の領域を確保
            _field = new string[height, width];
            _terrain = new string[height, width];
            //レイヤー情報分ループ
            foreach (XElement layer in layers)
            {
                string data = layer.Element("data").Value.Trim();
                string[] lines = data.Split('\n');
                for (int col = 0; col < height; col++)
                {
                    for (int row = 0; row < width; row++)
                    {
                        string[] text = lines[col].Split(',');
                        if (layer.Attribute("name").Value == "Terrain")
                            _terrain[col, row] = text[row];
                        if (layer.Attribute("name").Value == "Field")
                            _field[col, row] = text[row];
                    }
                }
                //デバッグログ
                string debugText = "";
                if (layer.Attribute("name").Value == "Terrain")
                    debugText += "Terrain\n";
                if (layer.Attribute("name").Value == "Field")
                    debugText += "Field\n";
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        if (layer.Attribute("name").Value == "Terrain")
                            debugText += $"{_terrain[i, j]},";
                        if (layer.Attribute("name").Value == "Field")
                            debugText += $"{_field[i, j]},";
                    }
                    debugText += "\n";
                }
                Debug.Log(debugText);
            }
            return true;
        }
        return false;
    }
    public void InitializeGame()
    {
        //フィールド用配列の領域を確保
        _currentField = new GameObject[_field.GetLength(0), _field.GetLength(1)];
        //string debugText = "";
        for (int y = 0; y < _field.GetLength(0); y++)
        {
            for (int x = 0; x < _field.GetLength(1); x++)
            {
                GameObject prefab = null;
                //そのままVector3(x, y, 0)で初期化すると上下が反転するのでmap.Length - y
                Vector3 position = new Vector3(x, _field.GetLength(0) - y, 0);
                string terrainId = _terrain[y, x];

                if (terrainId.Contains("w"))//wall
                    prefab = _wallPrefabs[0];
                else if (terrainId.Contains("g"))//ground
                    prefab = _groundPrefabs[0];
                else if (terrainId.Contains("r"))//river
                    prefab = _groundPrefabs[1];

                if (prefab)
                {
                    Instantiate(prefab, position, Quaternion.identity);   // 地形を置く
                }
                prefab = null;
                string fieldId = _field[y, x];

                if (fieldId.Contains("i0"))//item0
                    prefab = _itemPrefabs[0];
                else if (fieldId.Contains("i1"))//item1
                    prefab = _itemPrefabs[1];
                else if (fieldId.Contains("p"))//player
                    prefab = _playerPrefab;
                else if (fieldId.Contains("b"))//box
                    prefab = _boxPrefab;
                else if (fieldId.Contains("t"))//target
                    prefab = _targetPrefab;

                if (prefab)
                {
                    _currentField[y, x] = Instantiate(prefab, position, Quaternion.identity);
                    if (fieldId.Contains("i"))
                        _items.Add(_currentField[y, x]);
                }
                //debugText += _field[y, x] + ", ";
            }
            //debugText += "\n";  // 改行
        }

        //Debug.Log(debugText);
        //初期状態の物の位置を保存する
        _initialField = new GameObject[_field.GetLength(0), _field.GetLength(1)];
        Array.Copy(_currentField, _initialField, _field.Length);

        GameManager.instance._gameState = GameState.Idle;
    }
    /// <summary>
    /// プレイヤーの座標を取得する
    /// </summary>
    /// <returns></returns>
    public Vector2Int GetPlayerIndex()
    {
        for (int y = 0; y < _field.GetLength(0); y++)
        {
            for (int x = 0; x < _field.GetLength(1); x++)
            {
                if (_currentField[y, x] == null) { continue; }
                if (_currentField[y, x].tag == "Player")
                    return new Vector2Int(x, y);
            }
        }
        return new Vector2Int(-1, -1);
    }

    public void PushField()
    {
        //この関数は再帰呼び出しで何回か繰り返されるので、最初の1回だけ盤面を保存させる。
        GameObject[,] copyArray = new GameObject[_field.GetLength(0), _field.GetLength(1)];
        Array.Copy(_currentField, copyArray, _field.Length);
        _fieldStack.Push(copyArray);
    }
    public void PopField()
    {
        //スタックにフィールドの情報が入っているかを判定する。
        if (_fieldStack.TryPop(out var undoField))
        {
            _currentField = undoField;
            AudioManager.instance.PlaySound(6);
            for (int y = 0; y < _field.GetLength(0); y++)
            {
                for (int x = 0; x < _field.GetLength(1); x++)
                {
                    if (_currentField[y, x] != null)
                    {
                        Transform obj = _currentField[y, x].transform;
                        Vector3 to = new Vector3(x, _field.GetLength(0) - y, 0);
                        if (obj.position != to)
                        {
                            //y, x番目にあるGameobjectをx, 配列の縦の長さ-yへ移動させる
                            GameManager.instance.MoveFunction(obj, to, 0.2f, 0.015f);
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
        GameObject[,] copyArray = new GameObject[_field.GetLength(0), _field.GetLength(1)];
        Array.Copy(_initialField, copyArray, _initialField.Length);
        _currentField = copyArray;
        for (int y = 0; y < _field.GetLength(0); y++)
        {
            for (int x = 0; x < _field.GetLength(1); x++)
            {
                if (_currentField[y, x] != null)
                {
                    _currentField[y, x].transform.position = new Vector3(x, _field.GetLength(0) - y, 0);
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
        for (int y = 0; y < _field.GetLength(0); y++)
        {
            for (int x = 0; x < _field.GetLength(1); x++)
            {
                // 格納場所か否かを判断
                if (_field[y, x].Contains("t"))
                {
                    // 格納場所のインデックスを控えておく
                    goals.Add(new Vector2Int(x, y));
                }
            }
        }

        // 要素数はgoals.Countで取得
        for (int i = 0; i < goals.Count; i++)
        {
            GameObject f = _currentField[goals[i].y, goals[i].x];
            if (f == null || f.tag != "Moveable")
            {
                // 一つでも箱が無かったら条件未達成
                return false;
            }
        }
        return true;
    }
}
