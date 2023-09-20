using System;

public enum ObjectState
{
    /// <summary>’Êí</summary>
    Default,
    /// <summary>…’†</summary>
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
[Flags]
public enum Stars
{
    Star1 = 1 << 0,
    Star2 = 1 << 1,
    Star3 = 1 << 2,
}
public enum TextType
{
    Steps,
    Time,
    ClearSteps,
    ClearTime,
    MoveText,
    TimeRecord,
    StepRecord,
    timeAchievement,
    stepAchievement1,
    stepAchievement2,
    None,
}