using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
        if (_enableLockProcess == false) return;
        StageLock();
    }
    /// <summary>
    /// �X�e�[�W�L�^���e�L�X�g�ɕ\��������
    /// </summary>
    /// <typeparam name="T">�L�^�̌^</typeparam>
    /// <param name="dic">�X�e�[�W�L�^�̓�����Dictionary</param>
    /// <param name="text">�\���p�e�L�X�g</param>
    void StageRecord<T>(Dictionary<string, T> dic, Text text) where T : IComparable<T>, IFormattable
    {
        if (dic == null) return;
        if (text == null) return;
        if (dic.ContainsKey(_stageName) && !dic[_stageName].Equals(default(T)))
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
}
