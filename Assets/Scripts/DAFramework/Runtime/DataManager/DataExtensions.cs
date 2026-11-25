using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class DataExtensions
{
    public static T DeepClone<T>(this T obj) where T : IData
    {
        using (var ms = new MemoryStream())
        {
            // 序列化
            var formatter = new BinaryFormatter();
            formatter.Serialize(ms, obj);

            // 反序列化
            ms.Position = 0;
            return (T)formatter.Deserialize(ms);
        }
    }
}
