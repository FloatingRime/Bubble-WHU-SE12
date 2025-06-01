using System;
using System.Net.Sockets;
using System.Text;
using UnityEditor;
using UnityEngine;
using Multiplay;
using System.Text.Json;
using System.Threading;
using System.Collections.Generic;
using System.Collections;
using Unity.VisualScripting;
//using System.Text;

//��Ϣ�࣬�洢Ҫ���л�������
[Serializable]
public class NetworkMessage
{
    public NetworkMessage(long timestamp, Multiplay.MessageType classType, object data)
    {
        Timestamp = timestamp;
        ClassType = classType;
        Data = data;
    }

    public NetworkMessage() { }

    public long Timestamp { get; set; }    // ʱ��������룩
    public Multiplay.MessageType ClassType { get; set; }  // ����
    public object Data { get; set; }       // ��Ϣ����
}

public class SimpleClient : MonoBehaviour
{

    public static SimpleClient instance { get; private set; }


    private TcpClient client;
    private NetworkStream stream;

    public static int id;
    public Player _player;

    //public static Dictionary<int,Enemy> Enemies = new Dictionary<int,Enemy>();
    public static Dictionary<int, GameObject> enemies = new Dictionary<int, GameObject>();
    public GameObject enemyPrefab;

    //public GameObject enemy1;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }


    void Start()
    {
        try
        {
            // ���ӱ���������
            client = new TcpClient("127.0.0.1", 6666); // �ĳ���������˿�
            stream = client.GetStream();
            Debug.Log("�����ӷ�����");

            //SendTestMessage(); // �������������Ͳ�����Ϣ

            //NetworkStream stream = client.GetStream();

            // ���������߳�
            Thread receiveThread = new Thread(() => SimpleClient.Receive(stream));
            receiveThread.IsBackground = true;
            receiveThread.Start();

            //stream.ReadTimeout = 100;
            //StartCoroutine(Receive(stream));
        }
        catch (Exception ex)
        {
            Debug.LogError("����ʧ��: " + ex.Message);
        }
    }

    //���Ͳ�����Ϣ
    public void SendTestMessage()
    {
        if (stream == null) return;
        Move move = new Move
        {
            x = 3.0f,
            y = 5.0f
        };
        // �������Ϣ
        var message = new NetworkMessage(
            DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            Multiplay.MessageType.Move,
            move
        );

        // ���л����ֽ�
        byte[] data = Serialize(message);

        //������Ϣ����
        byte[] lengthBytes = BitConverter.GetBytes(data.Length);

        // ����
        stream.Write(lengthBytes, 0, lengthBytes.Length);
        stream.Write(data, 0, data.Length);
        Debug.Log("��Ϣ�ѷ���");
    }


    //������Ϣ
    public static void Receive(NetworkStream stream)
    {
        Debug.Log("������Ϣ��...");
        while (true)
        {

            // ��ȡ����ͷ��4 �ֽڣ�
            byte[] lengthBuffer = new byte[4];
            int readLength = 0;
            try
            {
                readLength = stream.Read(lengthBuffer, 0, 4);
            }
            catch (Exception)
            {
                return;
            }
            //int readLength = stream.Read(lengthBuffer, 0, 4);
            if (readLength < 4)
            {
                Debug.LogError("���ճ���ͷʧ��");
                return;
            }

            int messageLength = BitConverter.ToInt32(lengthBuffer, 0);
            if (messageLength <= 0 || messageLength > 4096)
            {
                Debug.LogError("��Ч����Ϣ����: " + messageLength);
                return;
            }

            // ������������Ϣ��
            byte[] messageData = new byte[messageLength];
            int receive = 0;
            /*while (totalReceived < messageLength)
            {
                int received = stream.Read(messageData, totalReceived, messageLength - totalReceived);
                if (received <= 0)
                {
                    Debug.LogError("��Ϣ����ʧ��");
                    return;
                }
                totalReceived += received;
            }*/
            try
            {
                receive = stream.Read(messageData);
            }
            catch (Exception)
            {
                Debug.Log("������Ϣʧ��");
            }

            string json = Encoding.UTF8.GetString(messageData);
            //Debug.Log("�յ�������Ϣ: " + json);

            // �����л�Ϊ NetworkMessage
            NetworkMessage message;
            long timeStamp = 0;
            Multiplay.MessageType type;
            //JsonElement dataElement;
            string data;

            try
            {
                //���λ�ȡ��Ϣ�࣬ʱ��������ͣ���Ϣ����
                using JsonDocument doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                message = Deserialize<NetworkMessage>(json);
                timeStamp = message.Timestamp;
                type = (Multiplay.MessageType)message.ClassType;
                //Debug.Log(1);
                //dataElement = root.GetProperty("Data").Clone();
                data = root.GetProperty("Data").GetRawText();
                //Debug.Log(2+"   "+data);
                //HandleMessage(type, dataElement.GetRawText()); //������Ϣ

            }
            catch (Exception)
            {
                Debug.Log("��Ϣ����ʧ��");
                return;
            }

            //Debug.Log(3);


            //������Ϣ
            if (messageLength - 8 > 0) //�������Ϣ
            {
                //Debug.Log($"���ܵ���Ϣ");
                //Debug.Log("���յ���Ϣ");
                //HandleMessage(type, data); //������Ϣ
                MainThreadDispatcher.Enqueue(() =>
                {
                    // ����Ͱ�ȫ�ˣ����Ե��� GetComponent��Instantiate �� Unity API
                    HandleMessage(type, data);
                });

            }
            else
            {
                receive = 0;
            }

            //yield return null;

        }

        //yield return null;
    }

    //������Ϣ
    private static void HandleMessage(Multiplay.MessageType type, string data)
    {
        Debug.Log($"���յ���Ϣ,��Ϣ����Ϊ{type}");
        switch (type)
        {
            case Multiplay.MessageType.HeartBeat:
                //Debug.Log($"�յ������� - ��� {player.playerId}");
                break;

            case Multiplay.MessageType.Move:
                Move move = Deserialize<Move>(data);
                //player.x = move.x;
                //player.y = move.y;
                Debug.Log($"����ƶ���({move.x},{move.y})");
                break;

            case Multiplay.MessageType.HpChange:
                //int hpChange = BitConverter.ToInt32(data, 0);
                //player.Hp += hpChange;
                //Debug.Log($"��� {player.playerId} Ѫ���仯");
                break;

            case Multiplay.MessageType.Boom:
                //Debug.Log($"��� {player.playerId} ����ը����");
                break;

            case Multiplay.MessageType.MoveAction:
                MoveAction moveAction = Deserialize<MoveAction>(data);
                GameObject enemyToMoveObj = enemies[moveAction.id]; 
                Enemy enemyToMove = enemyToMoveObj.GetComponent<Enemy>();
                enemyToMove.OnReceiveMoveMessage(moveAction);
                break;

            case Multiplay.MessageType.PlayerConnect:
                Debug.Log($"��һ��id��");
                PlayerConnect playerId = Deserialize<PlayerConnect>(data);
                id = playerId.id;
                instance._player.id = id;
                break;

            case Multiplay.MessageType.PlayerOtherConnect:
                Debug.Log("����������ӣ�");
                PlayerOtherConnect newPlayer = Deserialize<PlayerOtherConnect>(data);
                if (!enemies.ContainsKey(newPlayer.id) && newPlayer.id != id)
                {
                    // �������� GameObject
                    Vector3 spawnPos = new Vector3(0, 0, 0); // Ҳ������Ĭ��ֵ
                    GameObject enemy = Instantiate(instance.enemyPrefab, spawnPos, Quaternion.identity);
                    Enemy enemy1 = enemy.GetComponent<Enemy>();
                    enemy1.id = newPlayer.id;

                    // �����ֵ���
                    enemies.Add(newPlayer.id, enemy);

                    Debug.Log($"����һ�����ˣ�ID = {newPlayer.id}");
                }
                break;

            default:
                //Debug.Log($"δ֪��Ϣ����: {type}");
                break;
        }
    }



    void OnApplicationQuit()
    {
        stream?.Close();
        client?.Close();
    }

    // ���л�
    public static byte[] Serialize(object obj)
    {
        try
        {
            if (obj == null || !obj.GetType().IsSerializable)
            {
                return null;
            }
            string json = JsonUtility.ToJson(obj);
            return JsonSerializer.SerializeToUtf8Bytes(obj);
        }
        catch (Exception ex)
        {
            Debug.Log($"���л�ʧ��: {ex.Message}");
            return null;
        }
    }

    //�����л�
    public static T Deserialize<T>(byte[] data) where T : class
    {
        if (data == null || data.Length == 0)
        {
            Debug.Log("����Ϊ�գ��޷������л���");
            return null;
        }

        try
        {
            // ���ֽ�����ת�� UTF-8 �ַ���
            string json = Encoding.UTF8.GetString(data);
            //Debug.Log($"�����л�ǰ�� JSON: {json}");

            // �����л�Ϊ����
            return JsonSerializer.Deserialize<T>(json);
        }
        /*catch (JsonException ex)
        {
            Debug.Log($"�����л�ʧ�� (JSON ��ʽ����): {ex.Message}");
        }*/
        catch (Exception ex)
        {
            Debug.Log($"�����л�ʧ��: {ex.Message}");
        }

        return null;
    }

    //�����л�������
    public static T Deserialize<T>(string data) where T : class
    {
        if (data == null || data.Length == 0)
        {
            Debug.Log("����Ϊ�գ��޷������л���");
            return null;
        }

        try
        {
            //Debug.Log($"�����л�ǰ�� JSON: {data}");
            // �����л�Ϊ����
            return JsonSerializer.Deserialize<T>(data);
        }
        catch (JsonException ex)
        {
            Debug.Log($"�����л�ʧ�� (JSON ��ʽ����): {ex.Message}");
        }
        catch (Exception ex)
        {
            Debug.Log($"�����л�ʧ��: {ex.Message}");
        }

        return null;
    }

    public static void SendMessage<T>(T message)
    {
        switch(message)
        {
            case MoveAction moveAction:
                // ��װΪ NetworkMessage�����Լ�������ࣩ
                NetworkMessage messageData = new NetworkMessage
                {
                    Timestamp = 0,
                    ClassType = Multiplay.MessageType.MoveAction, // �Զ���ö��
                    Data = moveAction,
                };
                PackAndSend(messageData);
                break;

            default:
                break;
        }
    }

    public static void PackAndSend(NetworkMessage message)
    {

        // ���л�Ϊ JSON
        string json = JsonSerializer.Serialize(message);
        byte[] data = Encoding.UTF8.GetBytes(json);

        // ��ӳ���ǰ׺
        byte[] length = BitConverter.GetBytes(data.Length);
        byte[] finalData = new byte[length.Length + data.Length];
        Buffer.BlockCopy(length, 0, finalData, 0, length.Length);
        Buffer.BlockCopy(data, 0, finalData, length.Length, data.Length);

        // ����
        try
        {
            instance.stream.Write(finalData, 0, finalData.Length);
            Debug.Log("�ѷ����ƶ���Ϣ��" + json);
        }
        catch (Exception ex)
        {
            Debug.LogError("����ʧ�ܣ�" + ex.Message);
        }
    }
}

