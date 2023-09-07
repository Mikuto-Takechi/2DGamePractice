using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Xml.Linq;
/// <summary>
/// �}�b�v���Ǘ�����N���X
/// </summary>
public class MapEditor : MonoBehaviour
{
    /// <summary>�}�b�v���\������v���t�@�u��ݒ肷��C���X�y�N�^�[�p�̃N���X</summary>
    [SerializeField,Tooltip("Key�ɔԍ��AValue��Item1�Ƀv���t�@�u�AItem2�ɂ͖��O������")]
    SerializableKeyPair<int, SerializableTuple<GameObject, PrefabType>>[] _prefabs = default;
    /// <summary>���ۂɃQ�[���̏����Ŏg���}�b�v���\������v���t�@�u��ݒ肵��Dictionary</summary>
    public Dictionary<int, (GameObject, PrefabType)> _prefabsDictionary { get; set; }
    public int[,] _field { get; set; }  // �A�C�e���̏����z�u��S�[���̏ꏊ�̔z��(layer1)
    public int[,] _terrain { get; set; }  // �n�`�f�[�^�p�̔z��(layer0)
    public GameObject[,] _currentField { get; set; } // �Q�[���Ǘ��p�̔z��
    public GameObject[,] _currentGimmick { get; set; }// �X�e�[�W�M�~�b�N�p�̔z��
    public GameObject[,] _initialField { get; set; } // �Q�[���Ǘ��p�̔z��̏����z�u
    public GameObject[,] _initialGimmick { get; set; }// �X�e�[�W�M�~�b�N�p�̔z��̏����z�u
    public string _mapName { get; set; } = "";//�ǂݍ���ł���}�b�v�̖��O
    public string _nextMapName { get; set; } = "";//���̃}�b�v�̖��O
    TextAsset[] _allMap;
    public Stack<GameObject[,]> _fieldStack { get; set; } = new Stack<GameObject[,]>();
    public Stack<GameObject[,]> _gimmickStack { get; set; } = new Stack<GameObject[,]>();
    private void Awake()
    {
        //�}�b�v�f�[�^��S�ēǂݍ���
        _allMap = Resources.LoadAll<TextAsset>("Levels");
        //Inspector�Őݒ肷�邽�߂̃N���X����Dictionary�ɕϊ�����
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
        //�S�Ẵ}�b�v���������������
        foreach (TextAsset map in _allMap)
        {
            XElement mapXml = XElement.Parse(map.text);
            var stageData = mapXml.Element("stageData");
            Debug.Log(stageData.Element("name").Value);
            //��ԍŏ��̃^�O<map>�̑���name�Ɏw�肵���l�������Ȃ玟�̃��[�v�ֈړ�����
            if (mapXml.Attribute("name").Value != stageName)
                continue;
            //�ǂݍ��ݎn�߂�}�b�v�̖��O�Ǝ��̃X�e�[�W�̖��O��o�^
            _mapName = mapXml.Attribute("name").Value;
            _nextMapName = mapXml.Attribute("next").Value;
            //�}�b�v�̕��Əc�̑傫�������o��
            int width = int.Parse(mapXml.Attribute("width").Value);
            int height = int.Parse(mapXml.Attribute("height").Value);
            //layer�̃f�[�^�����o��
            IEnumerable<XElement> layers = from item in mapXml.Elements("layer")
                                           select item;
            //�z��̗̈���m��
            _field = new int[height, width];
            _terrain = new int[height, width];
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
                            _terrain[col, row] = int.Parse(nums[row]);
                        if (layer.Attribute("name").Value == "Field")
                            _field[col, row] = int.Parse(nums[row]);
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
        //�t�B�[���h�p�z��̗̈���m��
        _currentField = new GameObject[_field.GetLength(0), _field.GetLength(1)];
        _currentGimmick = new GameObject[_field.GetLength(0), _field.GetLength(1)];
        for (int y = 0; y < _field.GetLength(0); y++)
        {
            for (int x = 0; x < _field.GetLength(1); x++)
            {
                GameObject prefab = null;
                //���̂܂�Vector3(x, y, 0)�ŏ���������Ə㉺�����]����̂�map.Length - y
                Vector3 position = new Vector3(x, _field.GetLength(0) - y, 0);
                // �L�[�ƈ�v����v�f���z����ɂ���Ȃ�Value�����o���Đ�������
                foreach (var keyValue in _prefabsDictionary)
                {
                    if(keyValue.Key == _terrain[y, x])
                    {
                        prefab = keyValue.Value.Item1;
                        Instantiate(prefab, position, Quaternion.identity); // �n�`��u��
                    }
                    if(keyValue.Key == _field[y, x])
                    {
                        prefab = keyValue.Value.Item1;
                        _currentField[y, x] = Instantiate(prefab, position, Quaternion.identity);
                    }
                }
                //debugText += _field[y, x] + ", ";
            }
            //debugText += "\n";  // ���s
        }

        //Debug.Log(debugText);
        //������Ԃ̕��̈ʒu��ۑ�����
        _initialField = new GameObject[_field.GetLength(0), _field.GetLength(1)];
        Array.Copy(_currentField, _initialField, _field.Length);
        _initialGimmick = new GameObject[_field.GetLength(0), _field.GetLength(1)];
        Array.Copy(_currentGimmick, _initialGimmick, _field.Length);
        //�Q�[���I�u�W�F�N�g�����ׂĎ擾����UIlayer�̃I�u�W�F�N�g�����O���������J�E���g����
        //int count = 0;
        //FindObjectsOfType<GameObject>().Where(obj => obj.layer != 5)
        //                               .ToList().ForEach(obj => count++);
        //Debug.Log("�I�u�W�F�N�g�̐��F " + count);
        GameManager.instance._gameState = GameManager.GameState.Idle;
    }
    /// <summary>
    /// �v���C���[�̍��W���擾����
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
        //���̊֐��͍ċA�Ăяo���ŉ��񂩌J��Ԃ����̂ŁA�ŏ���1�񂾂��Ֆʂ�ۑ�������B
        GameObject[,] copyField = new GameObject[_field.GetLength(0), _field.GetLength(1)];
        Array.Copy(_currentField, copyField, _field.Length);
        _fieldStack.Push(copyField);
        GameObject[,] copyGimmick = new GameObject[_field.GetLength(0), _field.GetLength(1)];
        Array.Copy(_currentGimmick, copyGimmick, _field.Length);
        _gimmickStack.Push(copyGimmick);
    }
    public void PopField()
    {
        //�X�^�b�N�Ƀt�B�[���h�̏�񂪓����Ă��邩�𔻒肷��B
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
                            //y, x�Ԗڂɂ���Gameobject��x, �z��̏c�̒���-y�ֈړ�������
                            GameManager.instance.MoveFunction(obj, to, 0.2f);
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
            _currentGimmick = undoGimmick;
        }
    }
    /// <summary>
    /// �t�B�[���h��̕��̈ʒu������������
    /// </summary>
    public void InitializeField()
    {
        //�����̃t�B�[���h���R�s�[���Č��݂̃t�B�[���h���㏑������
        GameObject[,] copyField = new GameObject[_field.GetLength(0), _field.GetLength(1)];
        Array.Copy(_initialField, copyField, _initialField.Length);
        _currentField = copyField;
        GameObject[,] copyGimmick = new GameObject[_field.GetLength(0), _field.GetLength(1)];
        Array.Copy(_initialGimmick, copyGimmick, _initialGimmick.Length);
        _currentGimmick = copyGimmick;
        //�X�^�b�N�ɗ��܂��Ă���f�[�^���폜����
        _fieldStack.Clear();
        _gimmickStack.Clear();
        //���̈ʒu��߂�
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
    /// �N���A����
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
                // �i�[�ꏊ���ۂ��𔻒f
                if (name == PrefabType.Target)
                {
                    // �i�[�ꏊ�̃C���f�b�N�X���T���Ă���
                    goals.Add(new Vector2Int(x, y));
                }
            }
        }

        // �v�f����goals.Count�Ŏ擾
        for (int i = 0; i < goals.Count; i++)
        {
            GameObject f = _currentField[goals[i].y, goals[i].x];
            if (f == null || f.tag != "Movable")
            {
                // ��ł���������������������B��
                return false;
            }
        }
        return true;
    }
}
