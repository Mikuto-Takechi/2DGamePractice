using UnityEngine;

public class Player : MonoBehaviour
{
    Animator _animator;
    private void Start()
    {
        _animator = GetComponent<Animator>();
    }
    /// <summary>
    /// �A�j���[�V�������Đ�
    /// </summary>
    public void PlayAnimation()
    {
        _animator.Play("PlayerRun");
    }
}