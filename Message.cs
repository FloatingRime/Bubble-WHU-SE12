using System;

namespace Multiplay
{
    //消息类型
    public enum MessageType
    {
        None, //空类型
        Move, //移动
        HpChange, //血量改变
        Boom, //炸弹放置
    }

    [Serializable]
    public class Move
    {
        public float x;
        public float y;
    }

    [Serializable]
    public class HpChange
    {
        int hp;
    }

    [Serializable]
    public class Boom
    {
        float x;
        float y;
    }
}