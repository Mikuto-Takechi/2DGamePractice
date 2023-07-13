using Kogane;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// JSON �`���ɕϊ��i�V���A���C�Y�A�V���A�����j���� PlayerPrefs �ɕۑ�����
/// </summary>
public class JSONSave : MonoBehaviour
{
    public void Save()
    {
        // �C���X�^���X�����
        var dictionary1 = new Dictionary<int, string>()
        {
            { 1, "�t�V�M�_�l" },
            { 2, "�t�V�M�\�E" },
            { 3, "�t�V�M�o�i" },
        };

        var jsonDictionary1 = new JsonDictionary<int, string>(dictionary1);
        // �C���X�^���X�ϐ��� JSON �ɃV���A��������
        var json = JsonUtility.ToJson(jsonDictionary1, true);
        Debug.Log($"JSON: {json}");

        // PlayerPrefs �ɕۑ�����
        PlayerPrefs.SetString("SaveData", json);
    }
}
