using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Multiplay;


public static class Server
{
    private static Socket serverSocket;  //服务器Socket;

    //public static List<Player> Players;

    private static void Await()
    {
        serverSocket client = null;

        while(true)
        {
            try
            {
                //等待客户端连接，连接成功后获得Socket
                client = serverSocket.Accept();

                //获取客户端IP地址和端口号，如192.168.1.100:5000
                string endPoint = client.RemoteEndPoint.ToString();

                Console.WriteLine($"{player.Socket.RemoteEndPoint}连接成功");

                //创建接收线程，每个连接创建一个新线程，支持多客户端连接
                ParameterizedThreadStart receiveMethod = new ParameterizedThreadStart(Receive);
                Thread listener = new Thread(receiveMethod) { IsBackground = true }; //后台线程，程序关闭时自动终止
                
                //开始监听该客户端发送的消息
                listener.Start(player);
            }
            catch (Exception ex) //异常处理
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    //初始化
    public static void Start(string ip)
    {
        //实例化Socket类型 参数1:使用ipv4进行寻址 参数2:使用流进行数据传输 参数3:基于TCP协议
        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        
        //初始化服务器ip地址与端口号，套接字绑定
        IPEndPoint point = new IPEndPoint(IPAddress.Parse(ip), 8848);
        serverSocket.Bind(point); 
        serverSocket.Listen(0); //开启监听

    }

    //数据封装简易版：
    private static byte[] Pack(MessageType type, byte[] data = null)
    {
        List<byte> list = new List<byte>();
        if (data != null)
        {
            list.AddRange(BitConverter.Getbytes((ushort)(4 + data.Length)));//消息长度2字节
            list.AddRange(BitConverter.Getbytes((ushort)type));             //消息类型2字节
            list.AddRange(data);                                            //消息内容n字节
        }
        else
        {
            list.AddRange((ushort)4);                         //消息长度2字节
            list.AddRange((ushort)type);                      //消息类型2字节
        }
        return list.ToArray();
    }


    //数据封装，返回打包后的信息
    //举个例子，加入信息是"Hello"，那么长度为九字节，类型和长度各占2字节，内容占5字节
    // private static byte[] Pack(MessageType type, byte[] data = null)
    // {
    //     MessagePacker packer = new MessagePacker();
    //     //List<byte> packer = new List<byte>();

    //     if (data != null)
    //     {
    //         packer.Add((ushort)(4 + data.Length)); //消息长度
    //         packer.Add((ushort)type);              //消息类型
    //         packer.Add(data);                      //消息内容
    //     }
    //     else
    //     {
    //         packer.Add(4);                         //消息长度
    //         packer.Add((ushort)type);              //消息类型
    //     }
    //     return packer.Package;
    // }

}
