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
        MoveAction,
        PlayerConnect,
        PlayerOtherConnect,
    }

    public enum KeyInput
    {
        W,
        A,
        S,
        D
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
    public class MoveAction
    {
        public KeyInput key { get; set; }

        public int id { get; set; }

        public MoveAction() { }

        public MoveAction(KeyInput k)
        {
            key = k;
        }
    }

    [Serializable]
    public class PlayerConnect
    {
        public int id { get; set; }

        public PlayerConnect() { }

        public PlayerConnect(int i) { id = i; }
    }

    [Serializable]
    public class PlayerOtherConnect
    {
        public int id { get; set; }

        public PlayerOtherConnect() { }

        public PlayerOtherConnect(int i) { id = i; }
    }

    [Serializable]
    public class PlayerIdList
    {
        public List<int> ids { get; set; }

        public PlayerIdList() { ids = new List<int>(); }

        public PlayerIdList(List<int> ints) {  ids = ints; }
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