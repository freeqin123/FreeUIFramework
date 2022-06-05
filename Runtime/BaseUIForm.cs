using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FreeFramework 
{
    /// <summary>
    /// UI窗体基类
    /// </summary>
    public class BaseUIForm : MonoBehaviour, IUIForm
    {
        protected UIType currentUIType = new UIType();

        public virtual UIType CurrentUIType()
        {
            return currentUIType;
        }
        public virtual void SetCurrentUIType(UIType aimType)
        {
            currentUIType = aimType;
        }

        public virtual void Freeze()
        {
            gameObject.SetActive(true);
        }

        public virtual void HideUI()
        {
            gameObject.SetActive(false);
        }

        public virtual void ReDisplayUI()
        {
            gameObject.SetActive(true);
        }


        public virtual void ShowUI()
        {
            gameObject.SetActive(true);
        }


        public MonoBehaviour GetThisMonoBehavior()
        {
            return this;
        }

        /// <summary>
        /// 找到子物体中的元素
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        protected Transform FindChildElement(string name) 
        {
            return FindChildRecursive(transform,name);
        }

        protected T FindChildElement<T>(string name) where T: MonoBehaviour
        {
            var aimFind = FindChildRecursive(transform,name);
            return aimFind.GetComponent<T>();
        }

        /// <summary>
        /// 递归查找
        /// </summary>
        /// <param name="aim"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private Transform FindChildRecursive(Transform aim,string name) 
        {
            var aimChild = aim.Find(name);
            if (aimChild != null) 
            {
                return aimChild;
            }

            for (int i = 0; i < aim.childCount; i++)
            {
                var childItem = aim.GetChild(i);
                var againFind = FindChildRecursive(childItem,name);
                if (againFind != null)
                {
                    return againFind;
                }
            }
            return null;
        }

        /// <summary>
        /// 打开窗体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        protected void ShowUI<T>() where T: IUIForm 
        {
            var uiName = typeof(T).Name;
            UIManager.Instance.ShowUIForm(uiName);
        }

        /// <summary>
        /// 关闭UI窗体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        protected void CloseUI<T>() where T: IUIForm 
        {
            var uiName = typeof (T).Name;
            Debug.Log(uiName);
            UIManager.Instance.CloseUIForm(uiName);
        }

        /// <summary>
        /// 关掉自己
        /// </summary>
        protected void Close() 
        {
            var type = GetType();
            UIManager.Instance.CloseUIForm(type.Name);
        }
    }

    public interface IUIForm 
    {
        /// <summary>
        /// 获取当前UI信息
        /// </summary>
        /// <returns></returns>
        UIType CurrentUIType();
        /// <summary>
        /// 设置当前UI窗体属性
        /// </summary>
        /// <param name="aimType"></param>
        void SetCurrentUIType(UIType aimType);
        /// <summary>
        /// 显示页面
        /// </summary>
        void ShowUI();
        /// <summary>
        /// 隐藏页面
        /// </summary>
        void HideUI();
        /// <summary>
        /// 重新显示页面
        /// </summary>
        void ReDisplayUI();
        /// <summary>
        /// 冻结窗体
        /// </summary>
        void Freeze();
        /// <summary>
        /// 获取当前的MonoBehavior
        /// </summary>
        /// <returns></returns>
        MonoBehaviour GetThisMonoBehavior();
    }

}

