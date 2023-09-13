using System;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEditor.Experimental.GraphView.GraphView;
/// <summary>
/// Dictionary��Inspector��ŕ\�����邽�߂̃N���X
/// https://rimever.hatenablog.com/entry/2022/05/27/000000
/// </summary>
[Serializable]
public class SerializableKeyPair<TKey, TValue>
{
    [SerializeField] private TKey key;
    [SerializeField] private TValue value;

    public TKey Key => key;
    public TValue Value => value;
}

[Serializable]
public class SerializableTuple<T1, T2>
{
    [SerializeField] private T1 item1;
    [SerializeField] private T2 item2;
    public T1 Item1 => item1;
    public T2 Item2 => item2;
}
/// <summary>
/// �ő�l
/// </summary>
static class MaxValue
{
    public static int intValue = 99999;
    public static float floatValue = 99999f;
    /// <summary>
    /// �^����ꂽ�^�̍ő�l��Ԃ����\�b�h
    /// </summary>
    /// <typeparam name="T">int��float�̂�</typeparam>
    public static T Max<T>()
    {
        if (typeof(T) == typeof(int))
            return (T)(object)intValue;
        if (typeof(T) == typeof(float))
            return (T)(object)floatValue;
        throw new InvalidOperationException("���̌^�͖����ł�");
    }
}
/// <summary>
/// �X�e�[�W�̃I�u�W�F�N�g���Ǘ�����N���X
/// </summary>
public class Layer
{
    /// <summary>�t�B�[���h�̏��B�������ȊO�͊�{�ǂݎ���p</summary>
    public (int id, GameObject prefab, PrefabType type) field = (0, default, PrefabType.Default);
    /// <summary>�n�`�̏��B�������ȊO�͊�{�ǂݎ���p</summary>
    public (int id, GameObject prefab, PrefabType type) terrain = (0, default, PrefabType.Default);
    /// <summary>�t�B�[���h�̏��B�����ړ��������Ȃǂɏ���������</summary>
    public GameObject currentField;
    /// <summary>���𓾂����I�u�W�F�N�g�̈ʒu���d�Ȃ�ꍇ�Ɋi�[���邽�߂̕ϐ��B�������ɗ��������Ȃ�</summary>
    public GameObject currentGimmick;
    /// <summary>�������</summary>
    public GameObject initialField;
    /// <summary>�������</summary>
    public GameObject initialGimmick;
    /// <summary>
    /// Layer�̓񎟌��z���currentField��currentGimmick������������
    /// </summary>
    public static void Initialize(in Layer[,] layer, LayerMode mode)
    {
        for (int i = 0; i < layer.GetLength(0); i++)
        for (int j = 0; j < layer.GetLength(1); j++)
        {
            if (mode == LayerMode.CurrentField)
                layer[i, j].currentField = layer[i, j].initialField;
            else if (mode == LayerMode.CurrentGimmick)
                layer[i, j].currentGimmick = layer[i, j].initialGimmick;
        }
    }
    /// <summary>
    /// Layer�̓񎟌��z���currentField��currentGimmick�������Ŏ󂯎�����񎟌��z��ŏ㏑������
    /// </summary>
    public static void Set(in Layer[,] layer, GameObject[,] set, LayerMode mode)
    {
        for (int i = 0; i < layer.GetLength(0); i++)
        for (int j = 0; j < layer.GetLength(1); j++)
        {
            if (mode == LayerMode.CurrentField)
                layer[i, j].currentField = set[i, j];
            else if (mode == LayerMode.CurrentGimmick)
                layer[i, j].currentGimmick = set[i, j];
        }
    }
    /// <summary>
    /// Layer�̓񎟌��z���currentField��currentGimmick�̃f�[�^�������Ŏ󂯎�����񎟌��z��ɃR�s�[����
    /// </summary>
    public static void Copy(Layer[,] layer, in GameObject[,] copy, LayerMode mode)
    {
        for (int i = 0; i < layer.GetLength(0); i++)
        for (int j = 0; j < layer.GetLength(1); j++)
        {
            if (mode == LayerMode.CurrentField)
                copy[i, j] = layer[i, j].currentField;
            else if (mode == LayerMode.CurrentGimmick)
                copy[i, j] = layer[i, j].currentGimmick;
        }
    }
}