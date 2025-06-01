using Multiplay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int id;

    public void OnReceiveMoveMessage(MoveAction action)
    {

        Debug.Log($"玩家{action.id}发生移动");

        if (action.id != id)
            return;

        Vector3 direction = Vector3.zero;
        switch (action.key)
        {
            case KeyInput.W:
                direction = Vector3.up;
                break;
            case KeyInput.S:
                direction = Vector3.down;
                break;
            case KeyInput.A:
                direction = Vector3.left;
                break;
            case KeyInput.D:
                direction = Vector3.right;
                break;
        }

        transform.position += direction; // 你也可以加速度、速度平滑等
    }
}
