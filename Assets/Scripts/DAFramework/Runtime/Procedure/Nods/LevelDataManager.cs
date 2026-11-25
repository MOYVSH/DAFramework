using System.Collections.Generic;

public class LevelDataManager : BaseDataManager
{
    public int LevelID { get; private set; }
    
    private Dictionary<int, BaseData> _dataDic;

    public LevelDataManager(int levelID)
    {
        LevelID = levelID;
        _dataDic = new Dictionary<int, BaseData>();
    }


    public T GetData<T>(int entityID) where T : BaseData
    {
        bool canGet = _dataDic.TryGetValue(entityID, out var data);
        if (canGet)
        {
            return (T) data;
        }
        else
        {
            return null;
        }
    }
    
    public void AddData(int entityID, BaseData data)
    {
        _dataDic.Add(entityID, data);
    }
    
    public void RemoveData(int entityID)
    {
        _dataDic.Remove(entityID);
    }
}
