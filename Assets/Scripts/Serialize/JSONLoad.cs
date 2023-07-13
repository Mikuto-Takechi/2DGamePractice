using Kogane;
using UnityEngine;

/// <summary>
/// JSON として PlayerPrefs に保存したデータを読み込み、
/// インスタンスに復元（逆シリアル化、デシリアライズ）する。
/// </summary>
public class JSONLoad : MonoBehaviour
{
    public void ResetPrefs()
    {
        PlayerPrefs.DeleteKey("SaveData");
    }
    public void Load()
    {
        // PlayerPrefs から文字列を取り出す
        string json = PlayerPrefs.GetString("SaveData");
        // デシリアライズする
        var jsonDictionary2 = JsonUtility.FromJson<JsonDictionary<int, string>>(json);
        if(jsonDictionary2 == null)
        {
            Debug.Log("データが入っていません");
            return;
        }
        var dictionary2 = jsonDictionary2.Dictionary;
        // 画面に表示する
        foreach (var x in dictionary2)
        {
            Debug.Log(x.Key + ": " + x.Value);
        }
    }
}
