/// <summary>
/// ���Z�b�g�@�\����������C���^�[�t�F�C�X
/// </summary>
interface IReload
{
    /// <summary>���Z�b�g�����Ƃ��̏�������������</summary>
    void Reload();
}
/// <summary>
/// �����̍s�����L�^����@�\����������C���^�[�t�F�C�X
/// </summary>
interface IPushUndo
{
    void PushUndo();
}
/// <summary>
/// ���O�̍s���L�^���Ăяo���@�\����������C���^�[�t�F�C�X
/// </summary>
interface IPopUndo
{
    void PopUndo();
}
public enum ObjectState
{
    /// <summary>�ʏ�</summary>
    Default,
    /// <summary>����</summary>
    UnderWater,
}