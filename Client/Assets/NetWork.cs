using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public enum MessageType
{
    None, //������
    HeartBeat, //��������֤
    Move, //�ƶ�
    HpChange, //Ѫ���ı�
    Boom, //ը������
}

public class NetWork : MonoBehaviour
{
    private Socket socket;
    private byte[] buffer = new byte[4096]; // 4KB ������

    public void StartNetWork(string s,int port)
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Connect(s, port);
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
        if (socket == null) { print("��δ���ӷ�����"); return; }
        try
        {
            // ���л�����
            byte[] serializedData = SerializeData(data);

            // ȷ�� MessageType ö�����ж�Ӧ������
            if (!Enum.IsDefined(typeof(MessageType), typeof(T).Name))
            {
                Debug.LogError($"[����ʧ��] δ֪����Ϣ����: {typeof(T).Name}");
                return;
            }

            // ����������Ϣ
            MessageType messageType = (MessageType)Enum.Parse(typeof(MessageType), typeof(T).Name);
            NetworkMessage message = new NetworkMessage
            {
                ClassName = (long)messageType,  // ��������ת������
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
                    ClassName = reader.ReadInt64(),
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
    public long ClassName { get; set; }  // ����lao1
    public byte[] Data { get; set; }       // ����������
}