using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Xml.Linq;
/// <summary>
/// マップを管理するクラス
/// </summary>
[RequireComponent(typeof(GameManager))]
public class MapEditor : MonoBehaviour
{
    /// <summary>マップを構成するプレファブを設定するインスペクター用のクラス</summary>
    [SerializeField,Tooltip("Keyに番号、ValueのItem1にプレファブ、Item2には名前を入れる")]
    SerializableKeyPair<int, SerializableTuple<GameObject, PrefabType>>[] _prefabs = default;
    /// <summary>実際にゲームの処理で使うマップを構成するプレファブを設定したDictionary</summary>
    public Dictionary<int, (GameObject, PrefabType)> _prefabsDictionary { get; set; }
    public Layer[,] _layer { get; set; }
    public StageData _stageData { get; set; }
    TextAsset[] _allMap;
    public Stack<GameObject[,]> _fieldStack { get; set; } = new Stack<GameObject[,]>();
    public Stack<GameObject[,]> _gimmickStack { get; set; } = new Stack<GameObject[,]>();
    GameManager gameManager;
    private void Awake()
    {
        gameManager = GetComponent<GameManager>();
        //マップデータを全て読み込む
        _allMap = Resources.LoadAll<TextAsset>("Levels");
        //Inspectorで設定するためのクラスからDictionaryに変換する
        _prefabsDictionary ??= _prefabs.ToDictionary(p => p.Key, p => (p.Value.Item1, p.Value.Item2));
    }
    void OnEnable()
    {
        gameManager.PushData += PushField;
        gameManager.PopData += PopField;
        gameManager.ReloadData += InitializeField;
    }
    void OnDisable()
    {
        gameManager.PushData -= PushField;
        gameManager.PopData -= PopField;
        gameManager.ReloadData -= InitializeField;
    }
    public bool BuildMapData(string stageName)
    {
        //全てのマップを一つずつ処理をする
        foreach (TextAsset map in _allMap)
        {
            XElement mapXml = XElement.Parse(map.text);
            string stageDataString = mapXml.Element("stageData")?.Value;
            if (stageDataString == null)
                continue;
            _stageData = JsonUtility.FromJson<StageData>(stageDataString);
            //一番最初のタグ<map>の要素nameに指定した値が無いなら次のループへ移動する
            if (_stageData.name != stageName)
                continue;
            //読み込み始めるマップの名前と次のステージの名前と制限時間を登録
            //マップの幅と縦の大きさを取り出す
            int width = int.Parse(mapXml.Attribute("width").Value);
            int height = int.Parse(mapXml.Attribute("height").Value);
            //layerのデータを取り出す
            IEnumerable<XElement> layers = from item in mapXml.Elements("layer")
                                           select item;
            //配列の領域を確保
            _layer = new Layer[height, width];
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
                            _layer[col, row].terrain.id = int.Parse(nums[row]);
                        if (layer.Attribute("name").Value == "Field")
                            _layer[col, row].field.id = int.Parse(nums[row]);
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
                            debugText += $"{_layer[i, j].terrain.id},";
                        if (layer.Attribute("name").Value == "Field")
                            debugText += $"{_layer[i, j].field.id},";
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
        for (int y = 0; y < _layer.GetLength(0); y++)
        {
            for (int x = 0; x < _layer.GetLength(1); x++)
            {
                GameObject prefab = null;
                //そのままVector3(x, y, 0)で初期化すると上下が反転するのでmap.Length - y
                Vector3 position = new Vector3(x, _layer.GetLength(0) - y, 0);
                // キーと一致する要素が配列内にあるならValueを取り出して生成する
                foreach (var keyValue in _prefabsDictionary)
                {
                    if(keyValue.Key == _layer[y, x].terrain.id)
                    {
                        prefab = keyValue.Value.Item1;
                        _layer[y,x].terrain.prefab = Instantiate(prefab, position, Quaternion.identity); // 地形を置く
                        _layer[y, x].terrain.type = keyValue.Value.Item2;
                    }
                    if(keyValue.Key == _layer[y, x].field.id)
                    {
                        prefab = keyValue.Value.Item1;
                        _layer[y, x].field.prefab = Instantiate(prefab, position, Quaternion.identity);
                        _layer[y, x].field.type = keyValue.Value.Item2;
                        _layer[y, x].currentField = _layer[y, x].field.prefab;
                        _layer[y, x].initialField = _layer[y, x].field.prefab;
                    }
                }
            }
        }
        gameManager._gameState = GameState.Idle;
    }
    /// <summary>
    /// プレイヤーの座標を取得する
    /// </summary>
    /// <returns></returns>
    public Vector2Int GetPlayerIndex()
    {
        for (int y = 0; y < _layer.GetLength(0); y++)
        {
            for (int x = 0; x < _layer.GetLength(1); x++)
            {
                if (_layer[y, x].currentField == null) { continue; }
                if (_layer[y, x].currentField.tag == "Player")
                    return new Vector2Int(x, y);
            }
        }
        return new Vector2Int(-1, -1);
    }

    public void PushField()
    {
        //この関数は再帰呼び出しで何回か繰り返されるので、最初の1回だけ盤面を保存させる。
        GameObject[,] copyField = new GameObject[_layer.GetLength(0), _layer.GetLength(1)];
        LayerArray.Copy(_layer, copyField, LayerMode.CurrentField);
        _fieldStack.Push(copyField);
        GameObject[,] copyGimmick = new GameObject[_layer.GetLength(0), _layer.GetLength(1)];
        LayerArray.Copy(_layer, copyGimmick, LayerMode.CurrentGimmick);
        _gimmickStack.Push(copyGimmick);
    }
    public void PopField()
    {
        //スタックにフィールドの情報が入っているかを判定する。
        if (_fieldStack.TryPop(out var undoField))
        {
            LayerArray.Set(_layer, undoField, LayerMode.CurrentField);
            AudioManager.instance.PlaySound(6);
            for (int y = 0; y < _layer.GetLength(0); y++)
            {
                for (int x = 0; x < _layer.GetLength(1); x++)
                {
                    if (_layer[y, x].currentField != null)
                    {
                        Transform obj = _layer[y, x].currentField.transform;
                        Vector3 to = new Vector3(x, _layer.GetLength(0) - y, 0);
                        if (obj.position != to)
                        {
                            //y, x番目にあるGameobjectをx, 配列の縦の長さ-yへ移動させる
                            gameManager.MoveFunction(obj, to, 0.2f);
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
            LayerArray.Set(_layer, undoGimmick, LayerMode.CurrentGimmick);
        }
    }
    /// <summary>
    /// フィールド上の物の位置を初期化する
    /// </summary>
    public void InitializeField()
    {
        //Layerクラスの初期化メソッドで初期化する
        LayerArray.Initialize(_layer, LayerMode.CurrentField);
        LayerArray.Initialize(_layer, LayerMode.CurrentGimmick);
        //スタックに溜まっているデータを削除する
        _fieldStack.Clear();
        _gimmickStack.Clear();
        //物の位置を戻す
        for (int y = 0; y < _layer.GetLength(0); y++)
        {
            for (int x = 0; x < _layer.GetLength(1); x++)
            {
                if (_layer[y, x].currentField != null)
                {
                    _layer[y, x].currentField.transform.position = new Vector3(x, _layer.GetLength(0) - y, 0);
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
        for (int y = 0; y < _layer.GetLength(0); y++)
        {
            for (int x = 0; x < _layer.GetLength(1); x++)
            {
                // 格納場所か否かを判断
                if (_layer[y, x].field.type == PrefabType.Target)
                {
                    // 格納場所のインデックスを控えておく
                    goals.Add(new Vector2Int(x, y));
                }
            }
        }

        // 要素数はgoals.Countで取得
        for (int i = 0; i < goals.Count; i++)
        {
            GameObject f = _layer[goals[i].y, goals[i].x].currentField;
            if (f == null || f.tag != "Movable")
            {
                // 一つでも箱が無かったら条件未達成
                return false;
            }
        }
        return true;
    }
}
