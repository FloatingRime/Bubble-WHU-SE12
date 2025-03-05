using System.Net.Sockets;

public class Player
{
    public Socket playerSocket; //网络套接字

    public int playerId;   //玩家id

    public float x,y; //玩家位置

    public int Hp;    //玩家血量

    public Player(Socket socket, int id)
    {
        playerSocket = socket;
        playerId = id;
    }

    public void Offline()
    {
        Console.WriteLine($"玩家 {playerId} 掉线");
        playerSocket.Close();
        //Server.RemovePlayer(Id);  // 从服务器移除
    }
}