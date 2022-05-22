using System.Collections.Generic;
using UnityEngine;

namespace FreeFramework 
{
    /// <summary>
    /// 消息处理类
    /// </summary>
    public class MessageInfo
    {
        public ushort key;

        public object[] dataS;

        public MessageInfo(ushort keyValue)
        {
            key = keyValue;
        }

        public MessageInfo(ushort keyValue, params object[] valueDatas)
        {
            key = keyValue;
            dataS = valueDatas;
        }

        public T GetDataInfo<T>(int dataID)
        {
            if (dataS != null && dataID < dataS.Length)
            {
                return (T)dataS[dataID];
            }
            else
            {
                Debug.Log("当前数据ID位置不存在！");
                return default(T);
            }
        }

        public T GetDataInfo<T>()
        {
            if (dataS != null && dataS.Length > 0)
            {
                return (T)dataS[0];
            }
            else
            {
                Debug.Log("当前数据ID位置不存在！");
                return default(T);
            }
        }

        public bool CheckIsThisKey(ushort keyValue) 
        {
            if (keyValue == key)
            {
                return true;
            }
            else 
            {
                return false;
            }
        }

    }

    public delegate void MessageHandler(MessageInfo messageInfo);

    public interface IMessageReciver
    {
        /// <summary>
        /// 检测是否收到了消息
        /// </summary>
        /// <param name="messageInfo"></param>
        void DetectMessage(MessageInfo messageInfo);
    }

    /*
     * 基于委托的消息分发类，用于消息的发布/订阅
     */
    public class MessageCenter
    {
        public static Dictionary<ushort, MessageHandler> disMsgs = new Dictionary<ushort, MessageHandler>();

        /// <summary>
        /// 添加消息订阅者
        /// </summary>
        public static void AddMessageListener(ushort key, IMessageReciver reciver)
        {

            if (!disMsgs.ContainsKey(key))
            {
                disMsgs.Add(key, null);
            }

            disMsgs[key] += reciver.DetectMessage;
        }

        public static void AddMessageListener(ushort[] keys, IMessageReciver reciver)
        {

            for (int i = 0; i < keys.Length; i++)
            {
                if (!disMsgs.ContainsKey(keys[i]))
                {
                    disMsgs.Add(keys[i], null);
                }

                disMsgs[keys[i]] += reciver.DetectMessage;
            }
        }

        /// <summary>
        /// 取消事件订阅
        /// </summary>
        public static void RemoveMessageListener(ushort key, IMessageReciver reciver)
        {
            if (disMsgs.ContainsKey(key))
            {
                disMsgs[key] -= reciver.DetectMessage;
            }
        }

        public static void RemoveMessageListener(ushort[] keys, IMessageReciver reciver)
        {
            for (int i = 0; i < keys.Length; i++)
            {
                if (disMsgs.ContainsKey(keys[i]))
                {
                    disMsgs[keys[i]] -= reciver.DetectMessage;
                }
            }
        }

        /// <summary>
        /// 发送该消息
        /// </summary>
        public static void SendMessage(ushort key, object data)
        {
            MessageInfo messageInfo = new MessageInfo(key, data);

            if (disMsgs.TryGetValue(key, out MessageHandler handler))
            {
                handler(messageInfo);
            }
        }

        public static void SendMessage(ushort key, params object[] dataS)
        {
            MessageInfo messageInfo = new MessageInfo(key, dataS);

            if (disMsgs.TryGetValue(key, out MessageHandler handler))
            {
                handler(messageInfo);
            }
        }

        public static void SendMessage(ushort key)
        {
            MessageInfo messageInfo = new MessageInfo(key);

            if (disMsgs.TryGetValue(key, out MessageHandler handler))
            {
                handler(messageInfo);
            }
        }
    }

}
