using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MissionStar : MonoBehaviour
{
    /// <summary>�~�b�V���������x�\��</summary>
    [SerializeField] Image _missionStar1 = null;
    /// <summary>�~�b�V���������x�\��</summary>
    [SerializeField] Image _missionStar2 = null;
    /// <summary>�~�b�V���������x�\��</summary>
    [SerializeField] Image _missionStar3 = null;
    /// <summary�~�b�V���������}�[�N</summary>
    [SerializeField] Sprite _missionSuccessful = null;
    /// <summary>�~�b�V�������s�}�[�N</summary>
    [SerializeField] Sprite _missionFailure = null;
    void OnEnable()
    {
        GameManager.instance.GameClear += CheckMissions;
    }
    void OnDisable()
    {
        GameManager.instance.GameClear -= CheckMissions;
    }
    void Start()
    {
        _missionStar1.sprite = _missionFailure;
        _missionStar2.sprite = _missionFailure;
        _missionStar3.sprite = _missionFailure;
    }
    /// <summary>�~�b�V�������������Ă��邩�𔻒肵�ĕ\����ς���</summary>
    void CheckMissions()
    {
        if (GameManager.instance._isAchieved[0]())
        {
            _missionStar1.sprite = _missionSuccessful;
            if (GameManager.instance._isAchieved[1]())
            {
                _missionStar2.sprite = _missionSuccessful;
                if (GameManager.instance._isAchieved[2]())
                    _missionStar3.sprite = _missionSuccessful;
            }
        }
    }
}
