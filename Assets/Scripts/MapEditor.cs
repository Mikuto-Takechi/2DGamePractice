using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Xml.Linq;
/// <summary>
/// マップを管理するクラス
/// </summary>
public class MapEditor : MonoBehaviour
{
    /// <summary>マップを構成するプレファブを設定するインスペクター用のクラス</summary>
    [SerializeField,Tooltip("Keyに番号、ValueのItem1にプレファブ、Item2には名前を入れる")]
    SerializableKeyPair<int, SerializableTuple<GameObject, PrefabType>>[] _prefabs = default;
    /// <summary>実際にゲームの処理で使うマップを構成するプレファブを設定したDictionary</summary>
    public Dictionary<int, (GameObject, PrefabType)> _prefabsDictionary { get; set; }
    public int[,] _field { get; set; }  // アイテムの初期配置やゴールの場所の配列(layer1)
    public int[,] _terrain { get; set; }  // 地形データ用の配列(layer0)
    public GameObject[,] _currentField { get; set; } // ゲーム管理用の配列
    public GameObject[,] _currentGimmick { get; set; }// ステージギミック用の配列
    public GameObject[,] _initialField { get; set; } // ゲーム管理用の配列の初期配置
    public GameObject[,] _initialGimmick { get; set; }// ステージギミック用の配列の初期配置
    public string _mapName { get; set; } = "";//読み込んでいるマップの名前
    public string _nextMapName { get; set; } = "";//次のマップの名前
    TextAsset[] _allMap;
    public Stack<GameObject[,]> _fieldStack { get; set; } = new Stack<GameObject[,]>();
    public Stack<GameObject[,]> _gimmickStack { get; set; } = new Stack<GameObject[,]>();
    private void Awake()
    {
        //マップデータを全て読み込む
        _allMap = Resources.LoadAll<TextAsset>("Levels");
        //Inspectorで設定するためのクラスからDictionaryに変換する
        _prefabsDictionary ??= _prefabs.ToDictionary(p => p.Key, p => (p.Value.Item1, p.Value.Item2));
    }
    void OnEnable()
    {
        GameManager.instance.PushData += PushField;
        GameManager.instance.PopData += PopField;
        GameManager.instance.ReloadData += InitializeField;
    }
    void OnDisable()
    {
        GameManager.instance.PushData -= PushField;
        GameManager.instance.PopData -= PopField;
        GameManager.instance.ReloadData -= InitializeField;
    }
    public bool BuildMapData(string stageName)
    {
        //全てのマップを一つずつ処理をする
        foreach (TextAsset map in _allMap)
        {
            XElement mapXml = XElement.Parse(map.text);
            var stageData = mapXml.Element("stageData");
            Debug.Log(stageData.Element("name").Value);
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
            _field = new int[height, width];
            _terrain = new int[height, width];
            //レイヤー情報分ループ
            foreach (XElement layer in layers)
            {
                string data = layer.Element("data").Value.Trim();
                string[] lines = data.Split('\n');
                for (int col = 0; col < height; col++)
                {
                    for (int row = 0; row < width; row++)
                    {
                        //int[] nums = Array.ConvertAll(lines[col].Split(','), int.Parse);
                        var nums = lines[col].Split(',');
                        if (layer.Attribute("name").Value == "Terrain")
                            _terrain[col, row] = int.Parse(nums[row]);
                        if (layer.Attribute("name").Value == "Field")
                            _field[col, row] = int.Parse(nums[row]);
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
        _currentGimmick = new GameObject[_field.GetLength(0), _field.GetLength(1)];
        for (int y = 0; y < _field.GetLength(0); y++)
        {
            for (int x = 0; x < _field.GetLength(1); x++)
            {
                GameObject prefab = null;
                //そのままVector3(x, y, 0)で初期化すると上下が反転するのでmap.Length - y
                Vector3 position = new Vector3(x, _field.GetLength(0) - y, 0);
                // キーと一致する要素が配列内にあるならValueを取り出して生成する
                foreach (var keyValue in _prefabsDictionary)
                {
                    if(keyValue.Key == _terrain[y, x])
                    {
                        prefab = keyValue.Value.Item1;
                        Instantiate(prefab, position, Quaternion.identity); // 地形を置く
                    }
                    if(keyValue.Key == _field[y, x])
                    {
                        prefab = keyValue.Value.Item1;
                        _currentField[y, x] = Instantiate(prefab, position, Quaternion.identity);
                    }
                }
                //debugText += _field[y, x] + ", ";
            }
            //debugText += "\n";  // 改行
        }

        //Debug.Log(debugText);
        //初期状態の物の位置を保存する
        _initialField = new GameObject[_field.GetLength(0), _field.GetLength(1)];
        Array.Copy(_currentField, _initialField, _field.Length);
        _initialGimmick = new GameObject[_field.GetLength(0), _field.GetLength(1)];
        Array.Copy(_currentGimmick, _initialGimmick, _field.Length);
        //ゲームオブジェクトをすべて取得してUIlayerのオブジェクトを除外した物をカウントする
        //int count = 0;
        //FindObjectsOfType<GameObject>().Where(obj => obj.layer != 5)
        //                               .ToList().ForEach(obj => count++);
        //Debug.Log("オブジェクトの数： " + count);
        GameManager.instance._gameState = GameManager.GameState.Idle;
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
        GameObject[,] copyField = new GameObject[_field.GetLength(0), _field.GetLength(1)];
        Array.Copy(_currentField, copyField, _field.Length);
        _fieldStack.Push(copyField);
        GameObject[,] copyGimmick = new GameObject[_field.GetLength(0), _field.GetLength(1)];
        Array.Copy(_currentGimmick, copyGimmick, _field.Length);
        _gimmickStack.Push(copyGimmick);
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
                            GameManager.instance.MoveFunction(obj, to, 0.2f);
                            Player player = obj.GetComponent<Player>();
                            if (player)//プレイヤーのアニメーションを逆向きで再生
                            {
                                Vector2 dir = (Vector2)to - (Vector2)obj.position;
                                player.PlayAnimation(Vector2Int.RoundToInt(-dir));
                            }
                        }
                    }
                }
            }
        }
        if(_gimmickStack.TryPop(out var undoGimmick))
        {
            _currentGimmick = undoGimmick;
        }
    }
    /// <summary>
    /// フィールド上の物の位置を初期化する
    /// </summary>
    public void InitializeField()
    {
        //初期のフィールドをコピーして現在のフィールドを上書きする
        GameObject[,] copyField = new GameObject[_field.GetLength(0), _field.GetLength(1)];
        Array.Copy(_initialField, copyField, _initialField.Length);
        _currentField = copyField;
        GameObject[,] copyGimmick = new GameObject[_field.GetLength(0), _field.GetLength(1)];
        Array.Copy(_initialGimmick, copyGimmick, _initialGimmick.Length);
        _currentGimmick = copyGimmick;
        //スタックに溜まっているデータを削除する
        _fieldStack.Clear();
        _gimmickStack.Clear();
        //物の位置を戻す
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
                var name = _prefabsDictionary.Where(pair => pair.Key == _field[y, x])
                                             .Select(pair => pair.Value.Item2).FirstOrDefault();
                // 格納場所か否かを判断
                if (name == PrefabType.Target)
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
            if (f == null || f.tag != "Movable")
            {
                // 一つでも箱が無かったら条件未達成
                return false;
            }
        }
        return true;
    }
}
