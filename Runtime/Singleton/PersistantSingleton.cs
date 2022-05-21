using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FreeFramework 
{
    /// <summary>
    /// 单例基类，不随着的场景加载而销毁，始终存在
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class PersistantSingleton<T> : StaticInstance<T> where T :MonoBehaviour
    {
        protected override void Awake()
        {
            if (Instance != null) 
            {
                return;
            }   
            base.Awake();
            DontDestroyOnLoad(gameObject);
        }
    }
}


