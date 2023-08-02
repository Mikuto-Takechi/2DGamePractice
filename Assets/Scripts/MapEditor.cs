using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GManager;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;
/// <summary>
/// �}�b�v���Ǘ�����N���X
/// </summary>
public class MapEditor : MonoBehaviour
{
    /// <summary>�}�b�v�� CSV �`���Ŋi�[����Ă���e�L�X�g</summary>
    [SerializeField] TextAsset _mapData = default;
    public string[,] map { get; set; }  // ���x���f�U�C���p�̔z��
    public GameObject[,] _field { get; set; } // �Q�[���Ǘ��p�̔z��
    public GameObject[,] _initialField { get; set; } // �Q�[���Ǘ��p�̔z��̏����z�u
    public List<GameObject> _items { get; set; } = new List<GameObject>();// �A�C�e�����i�[���邽�߂̃��X�g
    public string _mapName { get; set; } = "";//�ǂݍ���ł���}�b�v�̖��O
    Stack<GameObject[,]> _fieldStack = new Stack<GameObject[,]>();
    [SerializeField] GameObject[] _wallPrefabs = default;
    [SerializeField] GameObject[] _groundPrefabs = default;
    [SerializeField] GameObject[] _itemPrefabs = default;
    [SerializeField] GameObject _playerPrefab = default;
    [SerializeField] GameObject _boxPrefab = default;
    [SerializeField] GameObject _goalPrefab = default;
    public void BuildMapData()
    {
        // C# �̕W�����C�u�������g���āu��s���ǂށv�Ƃ�������������
        System.IO.StringReader sr = new System.IO.StringReader(_mapData.text);
        List<string> lines = new List<string>();
        //�ŏ��̈�s�ɃX�e�[�W���������Ă���̂œǂݎ��
        string _mapName = sr.ReadLine();
        while (true)
        {
            // ��s���ǂ݂���ŏ�������
            string line = sr.ReadLine();

            // line �ɉ��������Ă��Ȃ�������I������Ƃ݂Ȃ��ď������I���
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
        Debug.Log("�}�b�v�f�[�^�̓ǂݎ�肪�������܂����B");
    }
    public void InitializeGame()
    {
        BuildMapData();
        //�t�B�[���h�p�z��̗̈���m��
        _field = new GameObject[map.GetLength(0), map.GetLength(1)];
        string debugText = "";
        //�W���O�z��̏����o��
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
                    //���̂܂�Vector3(x, y, 0)�ŏ���������Ə㉺�����]����̂�map.Length - y
                    Vector3 position = new Vector3(x, map.GetLength(0) - y, 0);
                    _field[y, x] = Instantiate(prefab, position, Quaternion.identity);
                    if(itemId.Contains("i"))
                        _items.Add(_field[y, x]);
                    Instantiate(_groundPrefabs[0], position, Quaternion.identity);   // ����u��
                }

                debugText += map[y, x] + ", ";
            }
            debugText += "\n";  // ���s
        }

        Debug.Log(debugText);
        //������Ԃ̕��̈ʒu��ۑ�����
        _initialField = new GameObject[map.GetLength(0), map.GetLength(1)];
        Array.Copy(_field, _initialField, _field.Length);

        GManager.instance._gameState = GameState.Idle;
    }
    /// <summary>
    /// �v���C���[�̍��W���擾����
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
        //���̊֐��͍ċA�Ăяo���ŉ��񂩌J��Ԃ����̂ŁA�ŏ���1�񂾂��Ֆʂ�ۑ�������B
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
                            //y, x�Ԗڂɂ���Gameobject��x, �z��̏c�̒���-y�ֈړ�������
                            GManager.instance.MoveFunction(obj, to, 0.1f, 0.015f);
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
    /// �N���A����
    /// </summary>
    /// <returns></returns>
    public bool IsCleared()
    {
        List<Vector2Int> goals = new List<Vector2Int>();

        for (int y = 0; y < map.GetLength(0); y++)
        {
            for (int x = 0; x < map.GetLength(1); x++)
            {
                // �i�[�ꏊ���ۂ��𔻒f
                if (map[y, x].Contains("g"))
                {
                    // �i�[�ꏊ�̃C���f�b�N�X���T���Ă���
                    goals.Add(new Vector2Int(x, y));
                }
            }
        }

        // �v�f����goals.Count�Ŏ擾
        for (int i = 0; i < goals.Count; i++)
        {
            GameObject f = _field[goals[i].y, goals[i].x];
            if (f == null || f.tag != "Moveable")
            {
                // ��ł���������������������B��
                return false;
            }
        }
        return true;
    }
}
