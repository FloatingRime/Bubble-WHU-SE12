using UnityEngine;

public class 连接 : MonoBehaviour
{
    public NetWork NW;
    public string Ip;
    public int Port;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void CNT()
    {
        NW.StartNetWork(Ip,Port);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
