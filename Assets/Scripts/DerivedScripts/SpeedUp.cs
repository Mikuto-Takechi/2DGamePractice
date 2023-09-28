using DG.Tweening;
using UnityEngine;

public class SpeedUp : ItemBase
{
    //�ύX��̃X�s�[�h
    [SerializeField] float _changeSpeed = 0.1f;
    //���ʎ���
    [SerializeField] float _effectTime = 1f;
    ShadowRenderer _shadowRenderer;
    static Tween _removeEffect;
    
    private new void Start()
    {
        base.Start();
        _shadowRenderer = FindObjectOfType<Player>().transform.GetComponentInChildren<ShadowRenderer>();
    }
    public override void ItemEffect()
    {
        AudioManager.Instance.PlaySound(12);
        GameManager.Instance._moveSpeed = _changeSpeed;
        _shadowRenderer._effectEnabled = true;
        _shadowRenderer._externalColor = new Color(0, 1, 1, 1);
        if( _removeEffect != null ) 
        {
            _removeEffect.Kill();
            _removeEffect = null;
        }
        // �w�肵�����Ԃ��o�߂�������ʂ���������
        _removeEffect = DOVirtual.DelayedCall(_effectTime ,() => 
        {
            GameManager.Instance._moveSpeed = GameManager.Instance._defaultSpeed;
            _shadowRenderer._effectEnabled = false;
            _shadowRenderer._externalColor = default;
        }).SetLink(gameObject);
    }
}
