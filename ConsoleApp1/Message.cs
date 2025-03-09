using System;
using System.Text.Json.Serialization;

namespace Multiplay
{
    //消息类型
    public enum MessageType
    {
        None, //空类型
        HeartBeat, //心跳包验证
        Move, //移动
        HpChange, //血量改变
        Boom, //炸弹放置
    }

    [Serializable]
    public class Move
    {
        public float x { get; set; }
        public float y { get; set; }

        public Move() { }
        public Move(float xx, float yy)
        {
            x = xx;
            y = yy;
        }
    }

    [Serializable]
    public class HpChange
    {
        int hp {  get; set; }
        public HpChange() { }

        public HpChange(int hp)
        {
            this.hp = hp;
        }
    }

    [Serializable]
    public class Boom
    {
        float x {  get; set; }
        float y {  get; set; }

        public Boom() { }
        public Boom(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
    }
}