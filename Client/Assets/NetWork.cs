using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class NetWork : MonoBehaviour
{
    private Socket socket;
    private byte[] buffer = new byte[4096]; // 4KB 缓冲区

    void Start()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Connect("127.0.0.1", 6666);
        StartReceive();
    }

    void StartReceive()
    {
        socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReceiveCallback, null);
    }

    void ReceiveCallback(IAsyncResult iar)
    {
        int len = socket.EndReceive(iar);
        if (len == 0) return;

        try
        {
            // 手动解析二进制数据
            NetworkMessage message = DeserializeNetworkMessage(buffer, len);

            Debug.Log($"[收到消息] 类名: {message.ClassName}, 时间戳: {message.Timestamp}, 内容: {Encoding.UTF8.GetString(message.Data)}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[消息解析失败] {ex.Message}");
        }

        StartReceive();
    }

    public void Send<T>(T data)
    {
        try
        {
            // 序列化对象
            byte[] serializedData = SerializeData(data);

            // 创建网络消息
            NetworkMessage message = new NetworkMessage
            {
                ClassName = typeof(T).Name,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                Data = serializedData
            };

            // 序列化网络消息
            byte[] finalData = SerializeNetworkMessage(message);
            socket.Send(finalData);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[发送失败] {ex.Message}");
        }
    }

    // 手动序列化数据
    private byte[] SerializeData<T>(T data)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                // 写入对象的数据（假设数据是字符串）
                writer.Write(data.ToString());
            }
            return ms.ToArray();
        }
    }

    // 手动序列化网络消息
    private byte[] SerializeNetworkMessage(NetworkMessage message)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                writer.Write(message.Timestamp);
                writer.Write(message.ClassName);
                writer.Write(message.Data.Length);
                writer.Write(message.Data);
            }
            return ms.ToArray();
        }
    }

    // 手动反序列化数据
    private NetworkMessage DeserializeNetworkMessage(byte[] data, int length)
    {
        using (MemoryStream ms = new MemoryStream(data, 0, length))
        {
            using (BinaryReader reader = new BinaryReader(ms))
            {
                NetworkMessage message = new NetworkMessage
                {
                    Timestamp = reader.ReadInt64(),
                    ClassName = reader.ReadString(),
                    Data = reader.ReadBytes(reader.ReadInt32())
                };
                return message;
            }
        }
    }
}

// 定义通用的二进制消息结构
public class NetworkMessage
{
    public long Timestamp { get; set; }    // 时间戳（毫秒）
    public string ClassName { get; set; }  // 类名
    public byte[] Data { get; set; }       // 二进制数据
}