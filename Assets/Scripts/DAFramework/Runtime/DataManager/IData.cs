using DAFramework.Entity;
public interface IData
{
    public void InitData(Entity entity);
    public void UnInitData();

    public void RegisterEventListeners();
    public void UnRegisterEventListeners();
}
