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


public static class Server
{
    private static Socket serverSocket;  //服务器Socket;
    private static Dictionary<int, Player> players = new(); //在线玩家列表
    private static int nextPlayerId = 1; //递增的玩家ID

    //public static List<Player> Players;

    //等待客户端连接
    private static void Await()
    {
        //Socket client = null;

        while(true)
        {
            try
            {
                //等待客户端连接，连接成功后获得Socket，创建用户
                Socket client = serverSocket.Accept();
                int playerId = nextPlayerId++;
                Player player = new Player(client,playerId);
                players[playerId] = player;

                //获取客户端IP地址和端口号，如192.168.1.100:5000
                string endPoint = client.RemoteEndPoint.ToString();

                //Console.WriteLine($"{player.Socket.RemoteEndPoint}连接成功");
                 Console.WriteLine($"玩家 {playerId} 连接成功: {client.RemoteEndPoint}");

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
            byte[] header = new byte[8]; //存储报文头部
            int length = 4096; //消息长度
            MessageType type = MessageType.None; //消息类型
            int receive = 0; //实际接收的信息长度
            long timeStamp = 0;

            //接收数据包头
            try
            {
                receive = client.Receive(header);
            }
            catch(Exception)
            {
                Console.WriteLine($"来自玩家{player.playerId}的数据包接收不到");
                player.Offline();
                return;
            }

            //包头不完整
            if(receive < header.Length)
            {
                Console.WriteLine($"来自玩家{player.playerId}的数据包头不完整");
                player.Offline();
                return;
            }

            //解析消息
            using (MemoryStream stream = new MemoryStream(header))
            {
                BinaryReader binary = new BinaryReader(stream, Encoding.UTF8);
                try
                {
                    //length = binary.ReadUInt16();//从数据流中读取前2字节作为消息长度（length）
                    //type = (MessageType)binary.ReadUInt16();
                    //type = (MessageType)binary.ReadInt32();
                    timeStamp = (long)BitConverter.ToInt32(header, 0);
                    type = (MessageType)BitConverter.ToInt32(header, 4);
                }
                catch(Exception)
                {
                    Console.WriteLine($"来自玩家{player.playerId}的消息解析失败");
                    player.Offline();
                    return ;
                }
            }

            //接收消息
            if(length - 8 > 0) //如果有消息
            {
                byte[] data = new byte[length];
                receive = client.Receive(data);
                /*if(receive < data.Length)
                {
                    Console.WriteLine($"来自玩家{player.playerId}的消息内容不完整");
                    player.Offline();
                    return;
                }*/
                Console.WriteLine($"接受到消息");
                HandleMessage(player,type,data);
            }
            else
            {
                //data = new byte[0];
                receive = 0;
            }
        }
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

    // 处理消息
    private static void HandleMessage(Player player, MessageType type, byte[] data)
    {
        switch (type)
        {
            case MessageType.HeartBeat:
                Console.WriteLine($"收到心跳包 - 玩家 {player.playerId}");
                break;

            case MessageType.Move:
                //Move move = Deserialize<Move>(data);
                //player.x = move.x;
                //player.y = move.y;
                Console.WriteLine($"玩家 {player.playerId} 移动");
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



    //数据封装，返回打包后的信息，需要做出修改
    //举个例子，加入信息是"Hello"，那么长度为九字节，类型和长度各占2字节，内容占5字节
    private static byte[] Pack(MessageType type, byte[] data = null)
    {
        MessagePacker packer = new MessagePacker();
        //List<byte> packer = new List<byte>();

        if (data != null)
        {
            packer.Add((ushort)(4 + data.Length)); //消息长度
            packer.Add((ushort)type);              //消息类型
            packer.Add(data);                      //消息内容
        }
        else
        {
            packer.Add(4);                         //消息长度
            packer.Add((ushort)type);              //消息类型
        }
        return packer.Package;
    }

}
