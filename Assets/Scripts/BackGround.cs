using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// �w�i���X�N���[�����邽�߂̃R���|�[�l���g�B
/// �K���ȃI�u�W�F�N�g�ɃA�^�b�`���Ďg���B
/// �w�i���w�肷��ƁA������������������B���ƕ����������̂����ɃX�N���[�����A��ʉ��ɏ�����Əォ��o�Ă���悤�ɂȂ�B
/// �]���āA�w�i�摜�͏㉺�Ɍq���Ă��悢���̂łȂ��Ă͂Ȃ�Ȃ��B
/// </summary>
public class BackGround : MonoBehaviour
{
    [SerializeField] Transform _parent = null;
    /// <summary>�w�i</summary>
    [SerializeField] Image _backgroundSprite = null;
    /// <summary>�w�i�̃X�N���[�����x</summary>
    [SerializeField] float _scrollSpeedX = -1f;
    /// <summary>�w�i���N���[���������̂����Ă����ϐ�</summary>
    Image _backgroundSpriteClone;
    /// <summary>�w�i���W�̏����l</summary>
    float _initialPositionX;

    void Start()
    {
        _initialPositionX = _backgroundSprite.transform.localPosition.x;   // ���W�̏����l��ۑ����Ă���

        // �w�i�摜�𕡐����ď�ɕ��ׂ�
        _backgroundSpriteClone = Instantiate(_backgroundSprite, _parent);
        _backgroundSpriteClone.transform.SetAsFirstSibling();
        _backgroundSpriteClone.transform.Translate(_backgroundSprite.rectTransform.rect.width, 0f, 0f);
    }

    void Update()
    {
        // �w�i�摜���X�N���[������
        _backgroundSprite.transform.Translate(_scrollSpeedX * Time.deltaTime, 0f, 0f);
        _backgroundSpriteClone.transform.Translate(_scrollSpeedX * Time.deltaTime, 0f, 0f);
        // �w�i�摜��������x���ɍ~�肽��A��ɖ߂�
        if (_backgroundSprite.transform.localPosition.x > _initialPositionX + _backgroundSprite.rectTransform.rect.width)
        {
            _backgroundSprite.transform.Translate(-_backgroundSprite.rectTransform.rect.width * 2, 0f, 0f);
        }

        // �w�i�摜�̃N���[����������x���ɍ~�肽��A��ɖ߂�
        if (_backgroundSpriteClone.transform.localPosition.x > _initialPositionX + _backgroundSpriteClone.rectTransform.rect.width)
        {
            _backgroundSpriteClone.transform.Translate(-_backgroundSpriteClone.rectTransform.rect.width * 2, 0f, 0f);
        }
    }
}