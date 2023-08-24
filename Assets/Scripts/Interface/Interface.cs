/// <summary>
/// リセット機能を実装するインターフェイス
/// </summary>
//interface IReload
//{
//    /// <summary>リセットしたときの処理を実装する</summary>
//    void Reload();
//}
/// <summary>
/// 一手一手の行動を記録する機能を実装するインターフェイス
/// </summary>
//interface IPushUndo
//{
//    void PushUndo();
//}
/// <summary>
/// 一手前の行動記録を呼び出す機能を実装するインターフェイス
/// </summary>
//interface IPopUndo
//{
//    void PopUndo();
//}
/// <summary>
/// オブジェクトのステートを管理するインターフェイス
/// </summary>
interface IObjectState
{
    ObjectState objectState { get; set; }
    void ChangeState(ObjectState state);
}
public enum ObjectState
{
    /// <summary>通常</summary>
    Default,
    /// <summary>水中</summary>
    UnderWater,
}