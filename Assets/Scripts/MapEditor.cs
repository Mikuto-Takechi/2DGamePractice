using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameManager;
using System.Linq;
using System.Xml.Linq;
using UnityEngine.InputSystem;
/// <summary>
/// �}�b�v���Ǘ�����N���X
/// </summary>
public class MapEditor : MonoBehaviour
{
    public string[,] _field { get; set; }  // �A�C�e���̏����z�u��S�[���̏ꏊ�̔z��(layer1)
    public string[,] _terrain { get; set; }  // �n�`�f�[�^�p�̔z��(layer0)
    public GameObject[,] _currentField { get; set; } // �Q�[���Ǘ��p�̔z��
    public GameObject[,] _initialField { get; set; } // �Q�[���Ǘ��p�̔z��̏����z�u
    public List<GameObject> _items { get; set; } = new List<GameObject>();// �A�C�e�����i�[���邽�߂̃��X�g
    public string _mapName { get; set; } = "";//�ǂݍ���ł���}�b�v�̖��O
    public string _nextMapName { get; set; } = "";//���̃}�b�v�̖��O
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
        //�S�Ẵ}�b�v���������������
        foreach (TextAsset map in _allMap)
        {
            XElement mapXml = XElement.Parse(map.text);
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
            _field = new string[height, width];
            _terrain = new string[height, width];
            //���C���[��񕪃��[�v
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
        //string debugText = "";
        for (int y = 0; y < _field.GetLength(0); y++)
        {
            for (int x = 0; x < _field.GetLength(1); x++)
            {
                GameObject prefab = null;
                //���̂܂�Vector3(x, y, 0)�ŏ���������Ə㉺�����]����̂�map.Length - y
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
                    Instantiate(prefab, position, Quaternion.identity);   // �n�`��u��
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
            //debugText += "\n";  // ���s
        }

        //Debug.Log(debugText);
        //������Ԃ̕��̈ʒu��ۑ�����
        _initialField = new GameObject[_field.GetLength(0), _field.GetLength(1)];
        Array.Copy(_currentField, _initialField, _field.Length);

        GameManager.instance._gameState = GameState.Idle;
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
        GameObject[,] copyArray = new GameObject[_field.GetLength(0), _field.GetLength(1)];
        Array.Copy(_currentField, copyArray, _field.Length);
        _fieldStack.Push(copyArray);
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
                            GameManager.instance.MoveFunction(obj, to, 0.2f, 0.015f);
                        }
                    }
                }
            }
        }
    }
    /// <summary>
    /// �t�B�[���h��̕��̈ʒu������������
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
                // �i�[�ꏊ���ۂ��𔻒f
                if (_field[y, x].Contains("t"))
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
            if (f == null || f.tag != "Moveable")
            {
                // ��ł���������������������B��
                return false;
            }
        }
        return true;
    }
}
