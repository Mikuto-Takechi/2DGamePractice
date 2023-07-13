using Kogane;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// JSON 形式に変換（シリアライズ、シリアル化）して PlayerPrefs に保存する
/// </summary>
public class JSONSave : MonoBehaviour
{
    public void Save()
    {
        // インスタンスを作る
        var dictionary1 = new Dictionary<int, string>()
        {
            { 1, "フシギダネ" },
            { 2, "フシギソウ" },
            { 3, "フシギバナ" },
        };

        var jsonDictionary1 = new JsonDictionary<int, string>(dictionary1);
        // インスタンス変数を JSON にシリアル化する
        var json = JsonUtility.ToJson(jsonDictionary1, true);
        Debug.Log($"JSON: {json}");

        // PlayerPrefs に保存する
        PlayerPrefs.SetString("SaveData", json);
    }
}
