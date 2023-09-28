using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MissionStar : MonoBehaviour
{
    /// <summary>ミッション成功度表示</summary>
    [SerializeField] Image _missionStar1 = null;
    /// <summary>ミッション成功度表示</summary>
    [SerializeField] Image _missionStar2 = null;
    /// <summary>ミッション成功度表示</summary>
    [SerializeField] Image _missionStar3 = null;
    /// <summaryミッション成功マーク</summary>
    [SerializeField] Sprite _missionSuccessful = null;
    /// <summary>ミッション失敗マーク</summary>
    [SerializeField] Sprite _missionFailure = null;
    void OnEnable()
    {
        GameManager.Instance.GameClear += CheckMissions;
    }
    void OnDisable()
    {
        GameManager.Instance.GameClear -= CheckMissions;
    }
    void Start()
    {
        _missionStar1.sprite = _missionFailure;
        _missionStar2.sprite = _missionFailure;
        _missionStar3.sprite = _missionFailure;
    }
    /// <summary>ミッションが成功しているかを判定して表示を変える</summary>
    void CheckMissions()
    {
        if (GameManager.Instance._isAchieved[0]())
        {
            _missionStar1.sprite = _missionSuccessful;
            if (GameManager.Instance._isAchieved[1]())
            {
                _missionStar2.sprite = _missionSuccessful;
                if (GameManager.Instance._isAchieved[2]())
                    _missionStar3.sprite = _missionSuccessful;
            }
        }
    }
}
