using DG.Tweening;
using UnityEngine;

public class SpeedUp : ItemBase
{
    //変更後のスピード
    [SerializeField] float _changeSpeed = 0.1f;
    //効果時間
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
        // 指定した時間が経過したら効果を解除する
        _removeEffect = DOVirtual.DelayedCall(_effectTime ,() => 
        {
            GameManager.Instance._moveSpeed = GameManager.Instance._defaultSpeed;
            _shadowRenderer._effectEnabled = false;
            _shadowRenderer._externalColor = default;
        }).SetLink(gameObject);
    }
}
