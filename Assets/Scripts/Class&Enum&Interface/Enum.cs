using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ObjectState
{
    /// <summary>�ʏ�</summary>
    Default,
    /// <summary>����</summary>
    UnderWater,
}
public enum PrefabType
{
    Default,
    Wall,
    Water,
    Ground,
    Target,
    Player,
}
public enum LayerMode
{
    CurrentField,
    CurrentGimmick,
}