using Multiplay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int id;
    public int moveSpeed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }

    public void Move()
    {
        if (Input.GetKeyDown(KeyCode.W))
            MoveAndSend(KeyInput.W);
        if (Input.GetKeyDown(KeyCode.S))
            MoveAndSend(KeyInput.S);
        if (Input.GetKeyDown(KeyCode.A))
            MoveAndSend(KeyInput.A);
        if (Input.GetKeyDown(KeyCode.D))
            MoveAndSend(KeyInput.D);
    }

    void MoveAndSend(KeyInput input)
    {
        // 本地移动
        Vector3 dir = Vector3.zero;
        switch (input)
        {
            case KeyInput.W: dir = Vector3.up; break;
            case KeyInput.S: dir = Vector3.down; break;
            case KeyInput.A: dir = Vector3.left; break;
            case KeyInput.D: dir = Vector3.right; break;
        }

        transform.position += dir * moveSpeed;

        // 构造并发送消息
        MoveAction action = new MoveAction(input, id);
        //string json = JsonUtility.ToJson(action);
        // NetworkManager.Instance.SendMoveMessage(json);
        SimpleClient.SendMessage<MoveAction>(action);
        //Debug.Log($"发送并移动: {json}");
    }
}
