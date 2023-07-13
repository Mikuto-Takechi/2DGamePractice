using Kogane;
using UnityEngine;

/// <summary>
/// JSON �Ƃ��� PlayerPrefs �ɕۑ������f�[�^��ǂݍ��݁A
/// �C���X�^���X�ɕ����i�t�V���A�����A�f�V���A���C�Y�j����B
/// </summary>
public class JSONLoad : MonoBehaviour
{
    public void ResetPrefs()
    {
        PlayerPrefs.DeleteKey("SaveData");
    }
    public void Load()
    {
        // PlayerPrefs ���當��������o��
        string json = PlayerPrefs.GetString("SaveData");
        // �f�V���A���C�Y����
        var jsonDictionary2 = JsonUtility.FromJson<JsonDictionary<int, string>>(json);
        if(jsonDictionary2 == null)
        {
            Debug.Log("�f�[�^�������Ă��܂���");
            return;
        }
        var dictionary2 = jsonDictionary2.Dictionary;
        // ��ʂɕ\������
        foreach (var x in dictionary2)
        {
            Debug.Log(x.Key + ": " + x.Value);
        }
    }
}
