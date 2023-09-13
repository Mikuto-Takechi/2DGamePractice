using System;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEditor.Experimental.GraphView.GraphView;
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
/// ステージのオブジェクトを管理するクラス
/// </summary>
public class Layer
{
    /// <summary>フィールドの情報。初期化以外は基本読み取り専用</summary>
    public (int id, GameObject prefab, PrefabType type) field = (0, default, PrefabType.Default);
    /// <summary>地形の情報。初期化以外は基本読み取り専用</summary>
    public (int id, GameObject prefab, PrefabType type) terrain = (0, default, PrefabType.Default);
    /// <summary>フィールドの情報。物が移動した時などに書き換える</summary>
    public GameObject currentField;
    /// <summary>情報を得たいオブジェクトの位置が重なる場合に格納するための変数。箱が水に落ちた時など</summary>
    public GameObject currentGimmick;
    /// <summary>初期情報</summary>
    public GameObject initialField;
    /// <summary>初期情報</summary>
    public GameObject initialGimmick;
    /// <summary>
    /// Layerの二次元配列のcurrentFieldかcurrentGimmickを初期化する
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
    /// Layerの二次元配列のcurrentFieldかcurrentGimmickを引数で受け取った二次元配列で上書きする
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
    /// Layerの二次元配列のcurrentFieldかcurrentGimmickのデータを引数で受け取った二次元配列にコピーする
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