using UnityEngine;

public class Player : MonoBehaviour
{
    Animator _animator;
    private void Start()
    {
        _animator = GetComponent<Animator>();
    }
    /// <summary>
    /// アニメーションを再生
    /// </summary>
    public void PlayAnimation()
    {
        _animator.Play("PlayerRun");
    }
}