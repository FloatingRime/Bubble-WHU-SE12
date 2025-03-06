using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Move
{
    public Move(float xx,float yy)
    { x = xx;y = yy; }

    public float x;
    public float y;
}

public class 移动 : MonoBehaviour
{
    NetWork network;
    public float speed;
    public KeyCode LeftKey, RightKey; 
    
    // Start is called before the first frame update
    void Start()
    {
        network = GameObject.Find("NetWork").GetComponent<NetWork>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(LeftKey))
        {
            transform.position += new Vector3(speed * -1*Time.deltaTime,0,0);
        }
        if (Input.GetKey(RightKey))
        {
            transform.position += new Vector3(speed*Time.deltaTime,0,0);
        }
        network.Send<Move>(new Move(transform.position.x,transform.position.y));
    }
}
