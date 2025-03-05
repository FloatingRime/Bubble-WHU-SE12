using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class NetWork : MonoBehaviour
{
    private Socket socket;
    private byte[] buffer = new byte[4096]; // 4KB ������

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
            // �ֶ���������������
            NetworkMessage message = DeserializeNetworkMessage(buffer, len);

            Debug.Log($"[�յ���Ϣ] ����: {message.ClassName}, ʱ���: {message.Timestamp}, ����: {Encoding.UTF8.GetString(message.Data)}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[��Ϣ����ʧ��] {ex.Message}");
        }

        StartReceive();
    }

    public void Send<T>(T data)
    {
        try
        {
            // ���л�����
            byte[] serializedData = SerializeData(data);

            // ����������Ϣ
            NetworkMessage message = new NetworkMessage
            {
                ClassName = typeof(T).Name,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                Data = serializedData
            };

            // ���л�������Ϣ
            byte[] finalData = SerializeNetworkMessage(message);
            socket.Send(finalData);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[����ʧ��] {ex.Message}");
        }
    }

    // �ֶ����л�����
    private byte[] SerializeData<T>(T data)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                // д���������ݣ������������ַ�����
                writer.Write(data.ToString());
            }
            return ms.ToArray();
        }
    }

    // �ֶ����л�������Ϣ
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

    // �ֶ������л�����
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

// ����ͨ�õĶ�������Ϣ�ṹ
public class NetworkMessage
{
    public long Timestamp { get; set; }    // ʱ��������룩
    public string ClassName { get; set; }  // ����
    public byte[] Data { get; set; }       // ����������
}