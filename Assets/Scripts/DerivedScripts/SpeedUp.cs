using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedUp : ItemBase
{
    //�ύX��̃X�s�[�h
    [SerializeField] float _changeSpeed = 0.1f;
    public override void ItemEffect()
    {
        GameManager.instance._moveSpeed = _changeSpeed; 
    }
}
