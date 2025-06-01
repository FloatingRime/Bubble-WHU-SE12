using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Runtime.Serialization;
using Multiplay;
using System.Text.Json;



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

    public MoveAction MoveAction { get; set; }
    public long Timestamp { get; set; }    // 时间戳（毫秒）
    public Multiplay.MessageType ClassType { get; set; }  // 类名
    public object Data { get; set; }       // 消息内容
}

public static class Server
{
    private static Socket serverSocket;  //服务器Socket;
    //private static Dictionary<int, Player> players = new(); //在线玩家列表，或许改成队列好一些
    private static int nextPlayerId = 1; //递增的玩家ID

    public static List<Player> players = new List<Player>(); //在线玩家列表
    private static Stack<int> playersOfflineId = new Stack<int>()
        
        ; // 掉线玩家id栈

    //新增用户时返回新的id
    public static int AddPlayer()
    {
        if(playersOfflineId.Count()==0)
        {
            return nextPlayerId++;
        }
        int x = playersOfflineId.Peek();
        playersOfflineId.Pop();
        return x;
    }

    //掉线时移除用户
    public static void RemovePlayer(this Player player)
    {
        playersOfflineId.Push(player.playerId);
        players.Remove(player);
    }

    //初始化，启动服务器
    public static void Start(string ip, int port)
    {
        //实例化Socket类型 参数1:使用ipv4进行寻址 参数2:使用流进行数据传输 参数3:基于TCP协议
        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        //初始化服务器ip地址与端口号，套接字绑定
        IPEndPoint point = new IPEndPoint(IPAddress.Parse(ip), port);
        serverSocket.Bind(point);
        //serverSocket.Listen(0); //开启监听
        serverSocket.Listen(4); //最多监听四人
        Console.WriteLine($"服务器启动，监听{ip}:{port}");

        Await();

    }

    //等待客户端连接
    private static void Await()
    {
        //Socket client = null;

        while(true)
        {
            try
            {
                //等待客户端连接，连接成功后获得Socket，创建用户
                //【to do】如果这个ip地址已经连接，就不应该再次连接
                Socket client = serverSocket.Accept();
                int playerId = AddPlayer();
                Player player = new Player(client,playerId);
                //players[playerId] = player;
                players.Add(player);

                //获取客户端IP地址和端口号，如192.168.1.100:5000
                string endPoint = client.RemoteEndPoint.ToString();

                //Console.WriteLine($"{player.Socket.RemoteEndPoint}连接成功");
                 Console.WriteLine($"玩家 {playerId} 连接成功: {client.RemoteEndPoint}");

                PlayerConnect id_ = new PlayerConnect(player.playerId);
                PlayerOtherConnect playerOtherConnect = new PlayerOtherConnect(player.playerId);

                SendMessageToTypicalPlayer(player,new NetworkMessage(0,Multiplay.MessageType.PlayerConnect,id_));
                if(players.Count > 1)
                {
                    foreach (Player _player in players)
                    {
                        if(player.playerId != _player.playerId)
                        {
                            PlayerOtherConnect otherInfo = new PlayerOtherConnect(_player.playerId);
                            SendMessageToTypicalPlayer(player, new NetworkMessage(0, Multiplay.MessageType.PlayerOtherConnect, otherInfo));
                        }
                        //SendMessageToTypicalPlayer(player, new NetworkMessage(0, Multiplay.MessageType.PlayOtherConnect, playerOtherConnect));
                    }
                }
                /*foreach(Player _player in players)
                {
                    SendMessageToTypicalPlayer(player, new NetworkMessage(0, Multiplay.MessageType.PlayOtherConnect, _player.playerId));
                }*/
                SendMessage(player, new NetworkMessage(0, Multiplay.MessageType.PlayerOtherConnect, playerOtherConnect));

                //创建接收线程，每个连接创建一个新线程，支持多客户端连接
                ParameterizedThreadStart receiveMethod = new ParameterizedThreadStart(Receive);
                Thread listener = new Thread(receiveMethod) { IsBackground = true }; //后台线程，程序关闭时自动终止
                
                //开始监听该客户端发送的消息
                listener.Start(player);
            }
            catch (Exception ex) //异常处理
            {
                Console.WriteLine($"客户端连接异常: {ex.Message}");
            }
        }
    }

    //客户端接收、解析消息数据
    public static void Receive(object obj)
    {
        Player player = (Player)obj;
        Socket client = player.playerSocket;

        while(true)
        {

            //读取长度头（4字节）
            byte[] lengthBuffer = new byte[4];
            int readBytes = 0;
            try
            {
                readBytes = client.Receive(lengthBuffer, 0, 4, SocketFlags.None);
            }
            catch (Exception)
            {
                Console.WriteLine($"来自玩家{player.playerId}的信息接收不到");
                player.Offline();
                return;
            }
            //int readBytes = client.Receive(lengthBuffer, 0, 4, SocketFlags.None);
            if (readBytes < 4)
            {
                Console.WriteLine("接收长度失败");
                player.Offline();
                return;
            }
            int messageLength = BitConverter.ToInt32(lengthBuffer, 0);
            Console.WriteLine("接收到来自"+ player.playerId +"的消息，消息长度" + messageLength);
            if (messageLength <= 0 || messageLength > 4096)
            {
                Console.WriteLine("无效的消息长度");
                player.Offline();
                return;
            }

            //int length = 4096; //消息长度
            byte[] messageData = new byte[messageLength]; //存储报文
            MessageType type = MessageType.None; //消息类型
            int receive = 0; //实际接收的信息长度
            NetworkMessage messageReceive; //收到的信息类
            long timeStamp = 0; //时间戳
            object data; //消息内容
            JsonElement dataElement; //辅助用，转化data用的

            //接收数据包头
            try
            {
                receive = client.Receive(messageData);
                //Console.WriteLine(BitConverter.ToString(messageData));
            }
            catch(Exception)
            {
                Console.WriteLine($"来自玩家{player.playerId}的数据包接收不到");
                player.Offline();
                return;
            }

            //解析消息
            using (MemoryStream stream = new MemoryStream(messageData))
            {
                try
                {
                    //依次获取信息类，时间戳，类型，消息内容
                    messageReceive = NetworkUtils.Deserialize<NetworkMessage>(messageData);
                    timeStamp = messageReceive.Timestamp;
                    type = (MessageType)messageReceive.ClassType;
                    dataElement = (JsonElement)messageReceive.Data;

                }
                catch(Exception)
                {
                    Console.WriteLine($"来自玩家{player.playerId}的消息解析失败");
                    player.Offline();
                    return ;
                }
            }

            //接收消息
            if(messageLength - 8 > 0) //如果有消息
            {
                //Console.WriteLine($"接受到消息");
                HandleMessage(player,type,dataElement.GetRawText()); //处理消息
                SendMessage(player, messageReceive); //发送消息
            }
            else
            {
                receive = 0;
            }
        }
    }


    // 【to do】处理消息
    private static void HandleMessage(Player player, MessageType type, string data)
    {
        Console.WriteLine($"消息种类为{type}");
        switch (type)
        {
            case MessageType.HeartBeat:
                Console.WriteLine($"收到心跳包 - 玩家 {player.playerId}");
                break;

            case MessageType.Move:
                Move move = NetworkUtils.Deserialize<Move>(data);
                //player.x = move.x;
                //player.y = move.y;
                Console.WriteLine($"玩家 {player.playerId} 移动到({move.x},{move.y})");
                break;

            case MessageType.MoveAction:
                MoveAction moveAction = NetworkUtils.Deserialize<MoveAction>(data);
                Console.WriteLine($"玩家{moveAction.id}移动，操作为{moveAction.key}");
                break;

            case MessageType.HpChange:
                //int hpChange = BitConverter.ToInt32(data, 0);
                //player.Hp += hpChange;
                Console.WriteLine($"玩家 {player.playerId} 血量变化");
                break;

            case MessageType.Boom:
                Console.WriteLine($"玩家 {player.playerId} 放置炸弹！");
                break;

            default:
                Console.WriteLine($"未知消息类型: {type}");
                break;
        }
    }

    //发送消息给除了player以外的玩家
    public static void SendMessage(Player player, NetworkMessage message)
    {
        //收到的数据即为需要发送的数据
        NetworkMessage messageSend = message;

        //数据序列化
        byte[] data = NetworkUtils.Serialize(messageSend);

        //长度
        byte[] length = BitConverter.GetBytes(data.Length);


        //拼接
        byte[] dataSend = new byte[length.Length + data.Length];
        Buffer.BlockCopy(length, 0, dataSend, 0, length.Length);
        Buffer.BlockCopy(data, 0, dataSend, length.Length, data.Length);

        //要求向players里面所有的player转发
        //信息需要记录是哪个player发生了移动，比如二号玩家发生移动
        if (dataSend != null && dataSend.Length > 0)
        {
            foreach (Player _player in players)
            {
                if (_player != player)
                {
                    _player.playerSocket.Send(dataSend);
                }
                //_player.playerSocket.Send(dataSend);
            }
            Console.WriteLine("消息已广播给所有玩家！");
            //Console.WriteLine("广播消息为：" + BitConverter.ToString(dataSend));
        }
    }

    //发送消息给某个玩家
    public static void SendMessageToTypicalPlayer(Player player, NetworkMessage message)
    {
        //收到的数据即为需要发送的数据
        NetworkMessage messageSend = message;

        //数据序列化
        byte[] data = NetworkUtils.Serialize(messageSend);

        //长度
        byte[] length = BitConverter.GetBytes(data.Length);


        //拼接
        byte[] dataSend = new byte[length.Length + data.Length];
        Buffer.BlockCopy(length, 0, dataSend, 0, length.Length);
        Buffer.BlockCopy(data, 0, dataSend, length.Length, data.Length);

        //要求向players里面所有的player转发
        //信息需要记录是哪个player发生了移动，比如二号玩家发生移动
        if (dataSend != null && dataSend.Length > 0)
        {
            player.playerSocket.Send(dataSend);
            //Console.WriteLine("消息已广播给某玩家！");
            //Console.WriteLine("广播消息为：" + BitConverter.ToString(dataSend));
        }
    }





}
