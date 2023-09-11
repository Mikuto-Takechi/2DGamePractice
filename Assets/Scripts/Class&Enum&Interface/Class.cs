using System;
using UnityEngine;
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
/// �X�e�[�W�̃I�u�W�F�N�g���Ǘ�����
/// </summary>
public class Layer
{
    public (int id, GameObject prefab, PrefabType type) field;
    public (int id, GameObject prefab, PrefabType type) terrain;
    public GameObject currentField;
    public GameObject currentGimmick;
}