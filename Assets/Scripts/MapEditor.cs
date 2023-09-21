using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Xml.Linq;
/// <summary>
/// �}�b�v���Ǘ�����N���X
/// </summary>
[RequireComponent(typeof(GameManager))]
public class MapEditor : MonoBehaviour
{
    /// <summary>�}�b�v���\������v���t�@�u��ݒ肷��C���X�y�N�^�[�p�̃N���X</summary>
    [SerializeField,Tooltip("Key�ɔԍ��AValue��Item1�Ƀv���t�@�u�AItem2�ɂ͖��O������")]
    SerializableKeyPair<int, SerializableTuple<GameObject, PrefabType>>[] _prefabs = default;
    /// <summary>���ۂɃQ�[���̏����Ŏg���}�b�v���\������v���t�@�u��ݒ肵��Dictionary</summary>
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
        //�}�b�v�f�[�^��S�ēǂݍ���
        _allMap = Resources.LoadAll<TextAsset>("Levels");
        //Inspector�Őݒ肷�邽�߂̃N���X����Dictionary�ɕϊ�����
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
        //�S�Ẵ}�b�v���������������
        foreach (TextAsset map in _allMap)
        {
            XElement mapXml = XElement.Parse(map.text);
            string stageDataString = mapXml.Element("stageData")?.Value;
            if (stageDataString == null)
                continue;
            _stageData = JsonUtility.FromJson<StageData>(stageDataString);
            //��ԍŏ��̃^�O<map>�̗v�fname�Ɏw�肵���l�������Ȃ玟�̃��[�v�ֈړ�����
            if (_stageData.name != stageName)
                continue;
            //�ǂݍ��ݎn�߂�}�b�v�̖��O�Ǝ��̃X�e�[�W�̖��O�Ɛ������Ԃ�o�^
            //�}�b�v�̕��Əc�̑傫�������o��
            int width = int.Parse(mapXml.Attribute("width").Value);
            int height = int.Parse(mapXml.Attribute("height").Value);
            //layer�̃f�[�^�����o��
            IEnumerable<XElement> layers = from item in mapXml.Elements("layer")
                                           select item;
            //�z��̗̈���m��
            _layer = new Layer[height, width];
            //���C���[��񕪃��[�v
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
                //�f�o�b�O���O
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
        //�t�B�[���h�p�z��̗̈���m��
        for (int y = 0; y < _layer.GetLength(0); y++)
        {
            for (int x = 0; x < _layer.GetLength(1); x++)
            {
                GameObject prefab = null;
                //���̂܂�Vector3(x, y, 0)�ŏ���������Ə㉺�����]����̂�map.Length - y
                Vector3 position = new Vector3(x, _layer.GetLength(0) - y, 0);
                // �L�[�ƈ�v����v�f���z����ɂ���Ȃ�Value�����o���Đ�������
                foreach (var keyValue in _prefabsDictionary)
                {
                    if(keyValue.Key == _layer[y, x].terrain.id)
                    {
                        prefab = keyValue.Value.Item1;
                        _layer[y,x].terrain.prefab = Instantiate(prefab, position, Quaternion.identity); // �n�`��u��
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
    /// �v���C���[�̍��W���擾����
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
        //���̊֐��͍ċA�Ăяo���ŉ��񂩌J��Ԃ����̂ŁA�ŏ���1�񂾂��Ֆʂ�ۑ�������B
        GameObject[,] copyField = new GameObject[_layer.GetLength(0), _layer.GetLength(1)];
        LayerArray.Copy(_layer, copyField, LayerMode.CurrentField);
        _fieldStack.Push(copyField);
        GameObject[,] copyGimmick = new GameObject[_layer.GetLength(0), _layer.GetLength(1)];
        LayerArray.Copy(_layer, copyGimmick, LayerMode.CurrentGimmick);
        _gimmickStack.Push(copyGimmick);
    }
    public void PopField()
    {
        //�X�^�b�N�Ƀt�B�[���h�̏�񂪓����Ă��邩�𔻒肷��B
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
                            //y, x�Ԗڂɂ���Gameobject��x, �z��̏c�̒���-y�ֈړ�������
                            gameManager.MoveFunction(obj, to, 0.2f);
                            Player player = obj.GetComponent<Player>();
                            if (player)//�v���C���[�̃A�j���[�V�������t�����ōĐ�
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
    /// �t�B�[���h��̕��̈ʒu������������
    /// </summary>
    public void InitializeField()
    {
        //Layer�N���X�̏��������\�b�h�ŏ���������
        LayerArray.Initialize(_layer, LayerMode.CurrentField);
        LayerArray.Initialize(_layer, LayerMode.CurrentGimmick);
        //�X�^�b�N�ɗ��܂��Ă���f�[�^���폜����
        _fieldStack.Clear();
        _gimmickStack.Clear();
        //���̈ʒu��߂�
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
    /// �N���A����
    /// </summary>
    /// <returns></returns>
    public bool IsCleared()
    {
        List<Vector2Int> goals = new List<Vector2Int>();
        for (int y = 0; y < _layer.GetLength(0); y++)
        {
            for (int x = 0; x < _layer.GetLength(1); x++)
            {
                // �i�[�ꏊ���ۂ��𔻒f
                if (_layer[y, x].field.type == PrefabType.Target)
                {
                    // �i�[�ꏊ�̃C���f�b�N�X���T���Ă���
                    goals.Add(new Vector2Int(x, y));
                }
            }
        }

        // �v�f����goals.Count�Ŏ擾
        for (int i = 0; i < goals.Count; i++)
        {
            GameObject f = _layer[goals[i].y, goals[i].x].currentField;
            if (f == null || f.tag != "Movable")
            {
                // ��ł���������������������B��
                return false;
            }
        }
        return true;
    }
}
