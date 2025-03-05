using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;

//网络工具类
//将客户端的信息序列化，传输给服务器，服务器反序列化

public static class NetworkUtils
{
    /*//序列化，将object转化为字节数组
    public static byte[] Serialize(object obj)
    {
        //检测输入有效性
        if(obj == null || !obj.GetType().IsSerializable)
        {
            return null;
        }

        //BinaryFormatter 是 .NET 的二进制序列化工具，用于将对象转换为字节流。
        BinaryFormatter formatter = new BinaryFormatter();

        using (MemoryStream stream = new MemoryStream())
        {
            formatter.Serialize(stream, obj); //obj序列化为二进制，写入stream
            byte[] data = stream.ToArray();
            return data;
        }
    }

    //反序列化，将字节数组转化为object
    public static T Deserialize<T>(byte[] data) where T : class
    {
        if (data == null || !typeof(T).IsSerializable)
        {
            return null;
        }

        BinaryFormatter formatter = new BinaryFormatter();

        using (MemoryStream stream = new MemoryStream(data))
        {
            object obj = formatter.Deserialize(stream);
            return obj as T;
        }
    }*/

    // 序列化
    public static byte[] Serialize(object obj)
    {
        try
        {
            /*if (obj == null || !obj.GetType().IsSerializable)
            {
                return null;
            }*/
            return JsonSerializer.SerializeToUtf8Bytes(obj);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"序列化失败: {ex.Message}");
            return null;
        }
    }

    // 反序列化
    public static T Deserialize<T>(byte[] data) where T : class
    {
        try
        {
            /*if (data == null || !typeof(T).IsSerializable)
            {
                return null;
            }*/
            return JsonSerializer.Deserialize<T>(data);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"反序列化失败: {ex.Message}");
            return null;
        }
    }

    //获取主机IPv4地址
    public static string GetLocalIPv4()
    {
        string hostName = Dns.GetHostName(); //获取主机名
        //IPHostEntry类存储与主机关联的 IP 地址和别名信息
        IPHostEntry iPEntry = Dns.GetHostEntry(hostName); //解析主机IP信息

        for (int i = 0; i < iPEntry.AddressList.Length; i++)
        {
            //从IP地址列表中筛选出IPv4类型的IP地址
            //AddressFamily：返回地址类型(IPv4 or IPv6)
            if (iPEntry.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
            {
                return iPEntry.AddressList[i].ToString();
            }
        }

        return null;
    }

}