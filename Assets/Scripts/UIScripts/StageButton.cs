using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// �X�e�[�W�I���{�^���ɕt����R���|�[�l���g
/// </summary>
[RequireComponent(typeof(Button))]
public class StageButton : MonoBehaviour
{
    /// <summary>���̃{�^���Ɋ��蓖�Ă�X�e�[�W��</summary>
    [SerializeField] string _stageName = "";
    /// <summary>�X�e�[�W����֘A�̏������s�����ǂ���</summary>
    [SerializeField] bool _enableLockProcess = true;
    /// <summary>�N���A���ԋL�^��\��������e�L�X�g</summary>
    [SerializeField] Text _timeRecord = null;
    /// <summary>�����L�^��\��������e�L�X�g</summary>
    [SerializeField] Text _stepRecord = null;
    /// <summary>���щ���x�\��</summary>
    [SerializeField] Image _achievementStar1 = null;
    [SerializeField] Image _achievementStar2 = null;
    [SerializeField] Image _achievementStar3 = null;
    /// <summary>���щ���}�[�N</summary>
    [SerializeField] Sprite _achievedImage = null;
    /// <summary>���і�����}�[�N</summary>
    [SerializeField] Sprite _notAchievedImage = null;
    /// <summary>���b�N���ꂽ���ɕ\������I�u�W�F�N�g</summary>
    [SerializeField] GameObject _lockImage = null;
    /// <summary>���b�N���ꂽ���ɔ�\������I�u�W�F�N�g</summary>
    [SerializeField] GameObject[] _hiddenObjects = null;
    Button _button;
    private void Start()
    {
        _button = GetComponent<Button>();
        if (_lockImage) _lockImage.SetActive(false);
    }
    private void Update()
    {
        StageRecord(GameManager.instance._timeRecords, _timeRecord);
        StageRecord(GameManager.instance._stepsRecords, _stepRecord);
        StageAchievements();
        if (_enableLockProcess == false) return;
        StageLock();
    }
    /// <summary>
    /// �X�e�[�W�L�^���e�L�X�g�ɕ\��������
    /// </summary>
    /// <typeparam name="T">�L�^�̌^</typeparam>
    /// <param name="dic">�X�e�[�W�L�^�̓�����Dictionary</param>
    /// <param name="text">�\���p�e�L�X�g</param>
    void StageRecord<T>(Dictionary<string, T> dic, Text text) where T : IFormattable
    {
        if (dic == null) return;
        if (text == null) return;
        //  �X�e�[�W�L�^�����݂��� && �L�^�̒l���^�̍ő�l�ł͂Ȃ���
        if (dic.ContainsKey(_stageName) && !dic[_stageName].Equals(MaxValue.Max<T>()))
        {
            //float�^�Ȃ珑���w�肵�ĕ\��
            if (typeof(T) == typeof(float))
                text.text = dic[_stageName].ToString("F2", new CultureInfo("en-US"));
            //int�^�Ȃ炻�̂܂ܕ\��
            if (typeof(T) == typeof(int))
                text.text = dic[_stageName].ToString();
        }
        else
        {
            text.text = "- - -";
        }
    }
    /// <summary>
    /// �X�e�[�W���������Ă��邩�𔻒f���ăI���I�t��؂�ւ���
    /// </summary>
    void StageLock()
    {
        if (_button.IsInteractable())//�{�^�������\�Ȃ��
        {
            if (GameManager.instance._unlockStages.Contains(_stageName))//�X�e�[�W���������Ă��邩���m�F����
            {
                _button.interactable = true;
            }
            else
            {
                _button.interactable = false;
            }
            _hiddenObjects.Where(o => o).ToList().ForEach(o => o.SetActive(_button.IsInteractable()));
            if (_lockImage) _lockImage.SetActive(!_button.IsInteractable());
        }
    }
    /// <summary>
    /// �X�e�[�W�̎��т��������Ă��邩�𔻒f���ĕ\����؂�ւ���
    /// </summary>
    void StageAchievements()
    {
        if(!GameManager.instance._achievements.ContainsKey(_stageName))
        {
            _achievementStar1.sprite = _notAchievedImage;
            _achievementStar2.sprite = _notAchievedImage;
            _achievementStar3.sprite = _notAchievedImage;
            return;
        }
        if ((GameManager.instance._achievements[_stageName] & Stars.Star1) == Stars.Star1)
            _achievementStar1.sprite = _achievedImage;
        else
            _achievementStar1.sprite = _notAchievedImage;
        if ((GameManager.instance._achievements[_stageName] & Stars.Star2) == Stars.Star2)
            _achievementStar2.sprite = _achievedImage;
        else
            _achievementStar2.sprite = _notAchievedImage;
        if ((GameManager.instance._achievements[_stageName] & Stars.Star3) == Stars.Star3)
            _achievementStar3.sprite = _achievedImage;
        else
            _achievementStar3.sprite = _notAchievedImage;
    }
}
