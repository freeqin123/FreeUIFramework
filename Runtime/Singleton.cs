using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FreeFramework 
{
    /// <summary>
    /// 普通单例，只存在一个，但是当出现其他的例子的时候，不会对其进行覆盖
    /// </summary>
    public abstract class Singleton<T> : StaticInstance<T> where T: MonoBehaviour
    {
        protected override void Awake()
        {
            if (Instance != null)
                Destroy(gameObject);
            base.Awake();
        }

    }
}


