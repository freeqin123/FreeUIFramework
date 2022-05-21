using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FreeFramework 
{
    /// <summary>
    ///静态实例，每一次的创建都会将其Instance进行重写覆盖
    /// </summary>
    public abstract class StaticInstance<T> : MonoBehaviour where T: MonoBehaviour
    {
        public static T Instance { get; private set; }

        protected virtual void Awake() => Instance = this as T;

        protected virtual void OnDestroy() 
        {
            Instance = null;
        }
        
    }

}
