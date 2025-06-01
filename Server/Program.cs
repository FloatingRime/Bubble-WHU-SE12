//string ip = "192.168.148.239";
using Multiplay;
using System.Text;
using System.Text.Json;

//string ip = "10.131.135.34";
string ip = "127.0.0.1";
Server.Start(ip,6666);


Move move = new Move
{
    x = 4.0f,
    y = 5.0f
};
//Move move = new Move(4.0f, 5.0f);
NetworkMessage networkMessage = new NetworkMessage(0, 0, move);

byte[] data = NetworkUtils.Serialize(networkMessage);
string json = Encoding.UTF8.GetString(data);
//Console.WriteLine($"反序列化前的 JSON: {json}");
//Console.WriteLine(BitConverter.ToString(data));
NetworkMessage n = NetworkUtils.Deserialize<NetworkMessage>(data);
JsonElement dataElement = (JsonElement)n.Data;
Move m = JsonSerializer.Deserialize<Move>(dataElement.GetRawText());
//Move m  = NetworkUtils.Deserialize<Move>(data);
Console.WriteLine(m.x.ToString() + m.y.ToString());



