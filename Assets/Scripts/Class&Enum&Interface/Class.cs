using MessagePack;
using System;
using System.Collections.Generic;
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
/// 最大値を管理するクラス
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
/// ステージのオブジェクトを管理する構造体
/// </summary>
public struct Layer
{
    /// <summary>フィールドの情報。初期化以外は基本読み取り専用</summary>
    public (int id, GameObject prefab, PrefabType type) field;
    /// <summary>地形の情報。初期化以外は基本読み取り専用</summary>
    public (int id, GameObject prefab, PrefabType type) terrain;
    /// <summary>フィールドの情報。物が移動した時などに書き換える</summary>
    public GameObject currentField;
    /// <summary>情報を得たいオブジェクトの位置が重なる場合に格納するための変数。箱が水に落ちた時など</summary>
    public GameObject currentGimmick;
    /// <summary>初期情報</summary>
    public GameObject initialField;
    /// <summary>初期情報</summary>
    public GameObject initialGimmick;
}
static class LayerArray
{
    /// <summary>
    /// Layerの二次元配列のcurrentFieldかcurrentGimmickを初期化する
    /// </summary>
    public static void Initialize(Layer[,] layer, LayerMode mode)
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
    public static void Set(Layer[,] layer, GameObject[,] set, LayerMode mode)
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
    public static void Copy(Layer[,] layer, GameObject[,] copy, LayerMode mode)
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
/// <summary>
/// ステージのデータを管理する構造体
/// </summary>
public struct StageData
{
    public string name;
    public string next;
    public float timeLimit;
    public float timeAchievement;
    public int stepAchievement1;
    public int stepAchievement2;
}
namespace MyNamespace
{
    static class MessagePackMethods
    {
        //以下のライブラリを使用しデータをシリアライズしてPlayerPrefsに保存、読み込みを行う
        //https://github.com/MessagePack-CSharp/MessagePack-CSharp/releases
        /// <summary>
        /// PlayerPrefsにデータを保存する
        /// </summary>
        /// <typeparam name="T">データの型</typeparam>
        /// <param name="label">セーブ名</param>
        /// <param name="data">保存するデータ</param>
        public static void MessagePackSave<T>(string label, T data)
        {
            byte[] bytes = MessagePackSerializer.Serialize(data);
            var json = MessagePackSerializer.ConvertToJson(bytes);
            PlayerPrefs.SetString(label, json);
        }
        /// <summary>
        /// PlayerPrefsからデータを読み込む
        /// </summary>
        /// <typeparam name="T">データの型</typeparam>
        /// <param name="label">セーブ名</param>
        /// <param name="data">読み込んだデータ</param>
        /// <returns>読み込みが成功しているかをbool型で戻す</returns>
        public static bool MessagePackLoad<T>(string label, out T data)
        {
            string json = PlayerPrefs.GetString(label, string.Empty);
            if (json != null && json != string.Empty)
            {
                try
                {
                    byte[] bytes = MessagePackSerializer.ConvertFromJson(json);
                    data = MessagePackSerializer.Deserialize<T>(bytes);
                }
                catch
                {
                    Debug.Log("セーブデータの逆シリアライズに失敗しました");
                    data = default;
                    return false;
                }
                return true;
            }
            else
            {
                Debug.Log("セーブデータを取得できませんでした");
                data = default;
                return false;
            }
        }
    }
}
namespace Takechi
{
    /// <summary>
    /// セーブデータ
    /// </summary>
    [MessagePackObject]
    [Serializable]
    public partial class SaveData
    {
        [Key(0)]
        public Dictionary<string, float> TimeRecords { get; set; } = new Dictionary<string, float>();
        [Key(1)]
        public Dictionary<string, int> StepRecords { get; set; } = new Dictionary<string, int>();
        [Key(2)]
        public Dictionary<string, Stars> Missions { get; set; } = new Dictionary<string, Stars>();
        [Key(3)]
        public HashSet<string> UnlockStages { get; set; } = new HashSet<string>();
        public SaveData()
        {
        }

        [SerializationConstructor]
        public SaveData(Dictionary<string, float> timeRecords, Dictionary<string, int> stepRecords, Dictionary<string, Stars> missions, HashSet<string> unlockStages)
        {
            this.TimeRecords = timeRecords;
            this.StepRecords = stepRecords;
            this.Missions = missions;
            this.UnlockStages = unlockStages;
        }
    }
}