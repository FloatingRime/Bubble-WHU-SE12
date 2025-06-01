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

//信息类，存储要序列化的数据
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

    public long Timestamp { get; set; }    // 时间戳（毫秒）
    public Multiplay.MessageType ClassType { get; set; }  // 类名
    public object Data { get; set; }       // 消息内容
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
            // 连接本机服务器
            client = new TcpClient("127.0.0.1", 6666); // 改成你服务器端口
            stream = client.GetStream();
            Debug.Log("已连接服务器");

            //SendTestMessage(); // 启动后立即发送测试消息

            //NetworkStream stream = client.GetStream();

            // 启动接收线程
            Thread receiveThread = new Thread(() => SimpleClient.Receive(stream));
            receiveThread.IsBackground = true;
            receiveThread.Start();

            //stream.ReadTimeout = 100;
            //StartCoroutine(Receive(stream));
        }
        catch (Exception ex)
        {
            Debug.LogError("连接失败: " + ex.Message);
        }
    }

    //发送测试信息
    public void SendTestMessage()
    {
        if (stream == null) return;
        Move move = new Move
        {
            x = 3.0f,
            y = 5.0f
        };
        // 构造简单消息
        var message = new NetworkMessage(
            DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            Multiplay.MessageType.Move,
            move
        );

        // 序列化成字节
        byte[] data = Serialize(message);

        //计算消息长度
        byte[] lengthBytes = BitConverter.GetBytes(data.Length);

        // 发送
        stream.Write(lengthBytes, 0, lengthBytes.Length);
        stream.Write(data, 0, data.Length);
        Debug.Log("消息已发送");
    }


    //接收信息
    public static void Receive(NetworkStream stream)
    {
        Debug.Log("接收信息中...");
        while (true)
        {

            // 读取长度头（4 字节）
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
                Debug.LogError("接收长度头失败");
                return;
            }

            int messageLength = BitConverter.ToInt32(lengthBuffer, 0);
            if (messageLength <= 0 || messageLength > 4096)
            {
                Debug.LogError("无效的消息长度: " + messageLength);
                return;
            }

            // 接收完整的消息体
            byte[] messageData = new byte[messageLength];
            int receive = 0;
            /*while (totalReceived < messageLength)
            {
                int received = stream.Read(messageData, totalReceived, messageLength - totalReceived);
                if (received <= 0)
                {
                    Debug.LogError("消息接收失败");
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
                Debug.Log("接收消息失败");
            }

            string json = Encoding.UTF8.GetString(messageData);
            //Debug.Log("收到完整消息: " + json);

            // 反序列化为 NetworkMessage
            NetworkMessage message;
            long timeStamp = 0;
            Multiplay.MessageType type;
            //JsonElement dataElement;
            string data;

            try
            {
                //依次获取信息类，时间戳，类型，消息内容
                using JsonDocument doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                message = Deserialize<NetworkMessage>(json);
                timeStamp = message.Timestamp;
                type = (Multiplay.MessageType)message.ClassType;
                //Debug.Log(1);
                //dataElement = root.GetProperty("Data").Clone();
                data = root.GetProperty("Data").GetRawText();
                //Debug.Log(2+"   "+data);
                //HandleMessage(type, dataElement.GetRawText()); //处理消息

            }
            catch (Exception)
            {
                Debug.Log("消息解析失败");
                return;
            }

            //Debug.Log(3);


            //接收消息
            if (messageLength - 8 > 0) //如果有消息
            {
                //Debug.Log($"接受到消息");
                //Debug.Log("接收到消息");
                //HandleMessage(type, data); //处理消息
                MainThreadDispatcher.Enqueue(() =>
                {
                    // 这里就安全了，可以调用 GetComponent、Instantiate 等 Unity API
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

    //处理信息
    private static void HandleMessage(Multiplay.MessageType type, string data)
    {
        Debug.Log($"接收到消息,消息种类为{type}");
        switch (type)
        {
            case Multiplay.MessageType.HeartBeat:
                //Debug.Log($"收到心跳包 - 玩家 {player.playerId}");
                break;

            case Multiplay.MessageType.Move:
                Move move = Deserialize<Move>(data);
                //player.x = move.x;
                //player.y = move.y;
                Debug.Log($"玩家移动到({move.x},{move.y})");
                break;

            case Multiplay.MessageType.HpChange:
                //int hpChange = BitConverter.ToInt32(data, 0);
                //player.Hp += hpChange;
                //Debug.Log($"玩家 {player.playerId} 血量变化");
                break;

            case Multiplay.MessageType.Boom:
                //Debug.Log($"玩家 {player.playerId} 放置炸弹！");
                break;

            case Multiplay.MessageType.MoveAction:
                MoveAction moveAction = Deserialize<MoveAction>(data);
                GameObject enemyToMoveObj = enemies[moveAction.id]; 
                Enemy enemyToMove = enemyToMoveObj.GetComponent<Enemy>();
                enemyToMove.OnReceiveMoveMessage(moveAction);
                break;

            case Multiplay.MessageType.PlayerConnect:
                Debug.Log($"玩家获得id！");
                PlayerConnect playerId = Deserialize<PlayerConnect>(data);
                id = playerId.id;
                instance._player.id = id;
                break;

            case Multiplay.MessageType.PlayerOtherConnect:
                Debug.Log("其他玩家连接！");
                PlayerOtherConnect newPlayer = Deserialize<PlayerOtherConnect>(data);
                if (!enemies.ContainsKey(newPlayer.id) && newPlayer.id != id)
                {
                    // 创建敌人 GameObject
                    Vector3 spawnPos = new Vector3(0, 0, 0); // 也可以用默认值
                    GameObject enemy = Instantiate(instance.enemyPrefab, spawnPos, Quaternion.identity);
                    Enemy enemy1 = enemy.GetComponent<Enemy>();
                    enemy1.id = newPlayer.id;

                    // 存入字典中
                    enemies.Add(newPlayer.id, enemy);

                    Debug.Log($"创建一个敌人，ID = {newPlayer.id}");
                }
                break;

            default:
                //Debug.Log($"未知消息类型: {type}");
                break;
        }
    }



    void OnApplicationQuit()
    {
        stream?.Close();
        client?.Close();
    }

    // 序列化
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
            Debug.Log($"序列化失败: {ex.Message}");
            return null;
        }
    }

    //反序列化
    public static T Deserialize<T>(byte[] data) where T : class
    {
        if (data == null || data.Length == 0)
        {
            Debug.Log("数据为空，无法反序列化！");
            return null;
        }

        try
        {
            // 将字节数组转成 UTF-8 字符串
            string json = Encoding.UTF8.GetString(data);
            //Debug.Log($"反序列化前的 JSON: {json}");

            // 反序列化为对象
            return JsonSerializer.Deserialize<T>(json);
        }
        /*catch (JsonException ex)
        {
            Debug.Log($"反序列化失败 (JSON 格式错误): {ex.Message}");
        }*/
        catch (Exception ex)
        {
            Debug.Log($"反序列化失败: {ex.Message}");
        }

        return null;
    }

    //反序列化，重载
    public static T Deserialize<T>(string data) where T : class
    {
        if (data == null || data.Length == 0)
        {
            Debug.Log("数据为空，无法反序列化！");
            return null;
        }

        try
        {
            //Debug.Log($"反序列化前的 JSON: {data}");
            // 反序列化为对象
            return JsonSerializer.Deserialize<T>(data);
        }
        catch (JsonException ex)
        {
            Debug.Log($"反序列化失败 (JSON 格式错误): {ex.Message}");
        }
        catch (Exception ex)
        {
            Debug.Log($"反序列化失败: {ex.Message}");
        }

        return null;
    }

    public static void SendMessage<T>(T message)
    {
        switch(message)
        {
            case MoveAction moveAction:
                // 封装为 NetworkMessage（你自己定义的类）
                NetworkMessage messageData = new NetworkMessage
                {
                    Timestamp = 0,
                    ClassType = Multiplay.MessageType.MoveAction, // 自定义枚举
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

        // 序列化为 JSON
        string json = JsonSerializer.Serialize(message);
        byte[] data = Encoding.UTF8.GetBytes(json);

        // 添加长度前缀
        byte[] length = BitConverter.GetBytes(data.Length);
        byte[] finalData = new byte[length.Length + data.Length];
        Buffer.BlockCopy(length, 0, finalData, 0, length.Length);
        Buffer.BlockCopy(data, 0, finalData, length.Length, data.Length);

        // 发送
        try
        {
            instance.stream.Write(finalData, 0, finalData.Length);
            Debug.Log("已发送移动消息：" + json);
        }
        catch (Exception ex)
        {
            Debug.LogError("发送失败：" + ex.Message);
        }
    }
}

