using System;
using UnityEngine;
/// <summary>
/// DictionaryをInspector上で表示するためのクラス
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
/// 最大値
/// </summary>
static class MaxValue
{
    public static int intValue = 99999;
    public static float floatValue = 99999f;
    /// <summary>
    /// 与えられた型の最大値を返すメソッド
    /// </summary>
    /// <typeparam name="T">intかfloatのみ</typeparam>
    public static T Max<T>()
    {
        if (typeof(T) == typeof(int))
            return (T)(object)intValue;
        if (typeof(T) == typeof(float))
            return (T)(object)floatValue;
        throw new InvalidOperationException("この型は無効です");
    }
}
/// <summary>
/// ステージのオブジェクトを管理する
/// </summary>
public class Layer
{
    public (int id, GameObject prefab, PrefabType type) field;
    public (int id, GameObject prefab, PrefabType type) terrain;
    public GameObject currentField;
    public GameObject currentGimmick;
}