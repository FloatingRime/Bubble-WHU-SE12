using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;
using System.Text;
//using UnityEngine;

//网络工具类
//已实现：序列化，反序列化，获取主机地址

public static class NetworkUtils
{



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

    //反序列化
    public static T Deserialize<T>(byte[] data) where T : class
    {
        if (data == null || data.Length == 0)
        {
            Console.WriteLine("数据为空，无法反序列化！");
            return null;
        }

        try
        {
            // 将字节数组转成 UTF-8 字符串
            string json = Encoding.UTF8.GetString(data);
            Console.WriteLine($"反序列化前的 JSON: {json}");

            // 反序列化为对象
            return JsonSerializer.Deserialize<T>(json);
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"反序列化失败 (JSON 格式错误): {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"反序列化失败: {ex.Message}");
        }

        return null;
    }

    //反序列化，重载
    public static T Deserialize<T>(string data) where T : class
    {
        if (data == null || data.Length == 0)
        {
            Console.WriteLine("数据为空，无法反序列化！");
            return null;
        }

        try
        {
            Console.WriteLine($"反序列化前的 JSON: {data}");
            // 反序列化为对象
            return JsonSerializer.Deserialize<T>(data);
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"反序列化失败 (JSON 格式错误): {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"反序列化失败: {ex.Message}");
        }

        return null;
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