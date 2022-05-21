using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FreeFramework 
{
    public class UIManager : PersistantSingleton<UIManager>
    {
        //路径文件字典
        private Dictionary<string, string> dicUIFormPath = new Dictionary<string, string>();
        //缓存UI窗体库
        private Dictionary<string, IUIForm> dicCacheUIForms = new Dictionary<string, IUIForm>();
        //当前处于显示状态的UI
        private Dictionary<string, IUIForm> dicCurrentShowUIForms = new Dictionary<string, IUIForm>();
        //反向切换窗体存储
        private Stack<IUIForm> reverseUIStack = new Stack<IUIForm>();
        //最大缓存UI数量
        public int maxCacheCount = 35;
        #region UIForms节点
        private Transform normalNode;
        private Transform fixedNode;
        private Transform popUpNode;
        private Transform uiMask;
        #endregion

        //加载资源方式
        public enum LoadConfigType
        {
            UnityAssets,
            Json
        }

        public LoadConfigType assetsType = LoadConfigType.Json;
        [Header("UIMask颜色")]
        public Color maskColor = new Color(0, 0, 0, 0.5f);
        protected override void Awake()
        {
            base.Awake();
            //初始话UI节点
            normalNode = transform.Find("Normal");
            fixedNode = transform.Find("Fixed");
            popUpNode = transform.Find("PopUp");
            uiMask = transform.Find("UIMask");
            InitUIPathAssets();
            uiMask.GetComponent<Image>().color = new Color(0, 0, 0, 0);
        }
        /// <summary>
        ///初始化对应的
        /// </summary>
        private void InitUIPathAssets()
        {
            switch (assetsType)
            {
                case LoadConfigType.UnityAssets:
                    var uiConfig = Resources.Load<UIConfig>(UIConstant.uiConfigAssetPath);
                    foreach (var item in uiConfig.infoGroup)
                    {
                        if (!dicUIFormPath.ContainsKey(item.uiID))
                        {
                            dicUIFormPath.Add(item.uiID, item.uiPrefabPath);
                        }
                        else { Debug.Log($"<b>配置文件UIConfigAssets路径重复:</b><color=red>{item.uiID}</color>"); }
                    }
                    break;
                case LoadConfigType.Json:
                    var uiJsonContent = Resources.Load<TextAsset>(UIConstant.uiConfigJsonPath);
                    UIConfigObject uiConfigData = JsonUtility.FromJson<UIConfigObject>(uiJsonContent.text);
                    foreach (var item in uiConfigData.infoGroup)
                    {
                        if (!dicUIFormPath.ContainsKey(item.uiID))
                        {
                            dicUIFormPath.Add(item.uiID, item.uiPrefabPath);
                        }
                        else { Debug.Log($"<b>配置文件UIConfigJson路径重复:</b><color=red>{item.uiID}</color>"); }
                    }
                    break;
                default:
                    break;
            }
        }

        public void ShowUIForm<T>() where T : IUIForm
        {
            var currentUIName = typeof(T).Name;
            IUIForm aimForm = LoadAimUIFormFromCacheDic(currentUIName);
            if (aimForm == null)
            {
                return;
            }

            if (aimForm.CurrentUIType().isClearReverseChange)
            {
                if (ClearReverseUIStack())
                {
                    Debug.Log("已清除反向切换堆栈");
                }
            }

            switch (aimForm.CurrentUIType().uiFormShowMode)
            {
                case UIFormShowMode.Normal:
                    ShowUIFormByNormal(currentUIName);
                    break;
                case UIFormShowMode.ReverseChange:
                    ShowUIFormAndPushToStack(currentUIName);
                    break;
                case UIFormShowMode.HideOther:
                    ShowUIFormAndHideOther(currentUIName);
                    break;
                default:
                    break;
            }

            UpdateUIMask(aimForm);
            //将要显示的UIForm显示在最前面
            aimForm.GetThisMonoBehavior().transform.SetAsLastSibling();
        }

        private void UpdateUIMask(IUIForm aimForm)
        {

            switch (aimForm.CurrentUIType().uiFormPenetrability)
            {
                case UIFormPenetrability.Pentrate:
                    var uiMaskImg = uiMask.GetComponent<Image>();
                    uiMaskImg.color = new Color(0, 0, 0, 0);
                    uiMaskImg.raycastTarget = false;
                    break;
                case UIFormPenetrability.LucencyImPentrate:
                    var uiMaskImg2 = uiMask.GetComponent<Image>();
                    uiMaskImg2.color = new Color(0, 0, 0, 0);
                    uiMaskImg2.raycastTarget = true;
                    aimForm.GetThisMonoBehavior().transform.SetParent(uiMask);
                    break;
                case UIFormPenetrability.ImPenetrable:
                    var uiMaskImg3 = uiMask.GetComponent<Image>();
                    uiMaskImg3.color = maskColor;
                    uiMaskImg3.raycastTarget = true;
                    aimForm.GetThisMonoBehavior().transform.SetParent(uiMask);
                    break;
                default:
                    break;
            }
        }

        public void ShowUIForm(string uiFormName)
        {
            if (string.IsNullOrEmpty(uiFormName))
            {
                Debug.Log("要打开的UIForm不存在");
                return;
            }

            IUIForm aimForm = LoadAimUIFormFromCacheDic(uiFormName);
            if (aimForm.CurrentUIType().isClearReverseChange)
            {
                if (ClearReverseUIStack())
                {
                    Debug.Log("已清除反向切换堆栈");
                }
            }

            switch (aimForm.CurrentUIType().uiFormShowMode)
            {
                case UIFormShowMode.Normal:
                    ShowUIFormByNormal(uiFormName);
                    break;
                case UIFormShowMode.ReverseChange:
                    ShowUIFormAndPushToStack(uiFormName);
                    break;
                case UIFormShowMode.HideOther:
                    ShowUIFormAndHideOther(uiFormName);
                    break;
                default:
                    break;
            }

            UpdateUIMask(aimForm);
        }

        private IUIForm LoadAimUIFormFromCacheDic(string name)
        {
            IUIForm aimUIForm;
            dicCacheUIForms.TryGetValue(name, out aimUIForm);
            if (aimUIForm == null)
            {
                aimUIForm = LoadUIFormResource(name);
            }
            return aimUIForm;
        }

        private IUIForm LoadUIFormResource(string name)
        {
            string uiFormPath;
            IUIForm uiForm;
            dicUIFormPath.TryGetValue(name, out uiFormPath);
            if (string.IsNullOrEmpty(uiFormPath))
            {
                Debug.Log($"路径为空：{name}");
                return null;
            }
            var uiFormObject = Instantiate(Resources.Load<GameObject>(uiFormPath));
            if (uiFormObject != null)
            {
                uiForm = uiFormObject.GetComponent<IUIForm>();
                if (uiForm != null)
                {
                    AddToCacheDic(name, uiForm);
                    switch (uiForm.CurrentUIType().uiFormType)
                    {
                        case UIFormType.Normal:
                            uiFormObject.transform.SetParent(normalNode, false);
                            break;
                        case UIFormType.Fixed:
                            uiFormObject.transform.SetParent(fixedNode, false);
                            break;
                        case UIFormType.PopUp:
                            uiFormObject.transform.SetParent(popUpNode, false);
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    Debug.LogError("未找到对应脚本或对应脚本不规范");
                    return null;
                }

                uiFormObject.SetActive(false);
                return uiForm;
            }
            else
            {
                return null;
            }
        }
        private void AddToCacheDic(string name, IUIForm item)
        {
            if (dicCacheUIForms.Count >= maxCacheCount)
            {
                foreach (var dicItem in dicCacheUIForms)
                {
                    if (!dicItem.Value.GetThisMonoBehavior().gameObject.activeSelf)
                    {
                        Destroy(dicItem.Value.GetThisMonoBehavior().gameObject);
                        dicCacheUIForms.Remove(dicItem.Key);
                        if (dicCacheUIForms.Count < maxCacheCount)
                        {
                            break;
                        }
                    }
                }
            }
            dicCacheUIForms.Add(name, item);
        }
        /// <summary>
        /// 清空反向切换堆栈
        /// </summary>
        /// <returns></returns>
        private bool ClearReverseUIStack()
        {
            if (reverseUIStack != null && reverseUIStack.Count >= 1)
            {
                reverseUIStack.Clear();
                return true;
            }
            return false;
        }

        private void ShowUIFormByNormal(string uiName)
        {
            IUIForm activeUI;
            IUIForm cacheUI;
            dicCurrentShowUIForms.TryGetValue(uiName, out activeUI);
            if (activeUI != null)
            {
                return;
            }
            dicCacheUIForms.TryGetValue(uiName, out cacheUI);
            if (cacheUI != null)
            {
                dicCurrentShowUIForms.Add(uiName, cacheUI);
                cacheUI.ShowUI();
            }
        }

        private void ShowUIFormAndPushToStack(string name)
        {
            IUIForm uiForm;
            if (reverseUIStack.Count > 0)
            {
                reverseUIStack.Peek().Freeze();
            }
            dicCacheUIForms.TryGetValue(name, out uiForm);
            if (uiForm != null)
            {
                uiForm.ShowUI();
                reverseUIStack.Push(uiForm);
            }
            else
            {
                Debug.Log($"未找到缓存中的UIForm:{uiForm}");
            }
        }

        private void ShowUIFormAndHideOther(string name)
        {
            IUIForm uiForm;
            IUIForm cacheUIForm;

            if (string.IsNullOrEmpty(name))
            {
                return;
            }
            dicCurrentShowUIForms.TryGetValue(name, out uiForm);
            if (uiForm != null)
            {
                return;
            }

            foreach (var item in dicCurrentShowUIForms)
            {
                item.Value.HideUI();
            }

            foreach (var item in reverseUIStack)
            {
                item.HideUI();
            }
            dicCacheUIForms.TryGetValue(name, out cacheUIForm);
            if (cacheUIForm != null)
            {
                dicCurrentShowUIForms.Add(name, cacheUIForm);
                cacheUIForm.ShowUI();
            }

        }
        /// <summary>
        /// 关闭窗体
        /// </summary>
        /// <param name="name"></param>
        public void CloseUIForm(string name)
        {
            IUIForm baseUI;
            if (string.IsNullOrEmpty(name))
            {
                return;
            }
            dicCacheUIForms.TryGetValue(name, out baseUI);

            if (baseUI == null)
            {
                return;
            }

            AfterCloseToRestoreParent(baseUI);

            switch (baseUI.CurrentUIType().uiFormShowMode)
            {
                case UIFormShowMode.Normal:
                    CloseUIFormNormal(name);
                    break;
                case UIFormShowMode.ReverseChange:
                    CloseUIFormPopUIStack(name);
                    break;
                case UIFormShowMode.HideOther:
                    CloseUIFormAndShowOther(name);
                    break;
                default:
                    break;
            }


        }
        /// <summary>
        /// 关闭UI窗体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void CloseUIForm<T>() where T : IUIForm
        {
            var uiFormName = typeof(T).Name;
            IUIForm baseUI;
            if (string.IsNullOrEmpty(uiFormName))
            {
                return;
            }
            dicCacheUIForms.TryGetValue(uiFormName, out baseUI);

            if (baseUI == null)
            {
                return;
            }

            AfterCloseToRestoreParent(baseUI);

            switch (baseUI.CurrentUIType().uiFormShowMode)
            {
                case UIFormShowMode.Normal:
                    CloseUIFormNormal(uiFormName);
                    break;
                case UIFormShowMode.ReverseChange:
                    CloseUIFormPopUIStack(uiFormName);
                    break;
                case UIFormShowMode.HideOther:
                    CloseUIFormAndShowOther(uiFormName);
                    break;
                default:
                    break;
            }
        }


        private void RestoreUIMask()
        {
            var uiMaskImg = uiMask.GetComponent<Image>();
            uiMaskImg.color = new Color(0, 0, 0, 0);
            uiMaskImg.raycastTarget = false;
        }

        private void AfterCloseToRestoreParent(IUIForm baseUI)
        {
            switch (baseUI.CurrentUIType().uiFormType)
            {
                case UIFormType.Normal:
                    if (baseUI.GetThisMonoBehavior().transform.parent != normalNode)
                    {
                        baseUI.GetThisMonoBehavior().transform.SetParent(normalNode);
                    }
                    break;
                case UIFormType.Fixed:
                    if (baseUI.GetThisMonoBehavior().transform.parent != fixedNode)
                    {
                        baseUI.GetThisMonoBehavior().transform.SetParent(fixedNode);
                    }
                    break;
                case UIFormType.PopUp:
                    if (baseUI.GetThisMonoBehavior().transform.parent != popUpNode)
                    {
                        baseUI.GetThisMonoBehavior().transform.SetParent(popUpNode);
                    }
                    break;
                default:
                    break;
            }

            RestoreUIMask();
        }

        //移除显示uiForm缓存中的UI
        private void CloseUIFormNormal(string uiFormName)
        {
            IUIForm aimUIForm;
            dicCurrentShowUIForms.TryGetValue(uiFormName, out aimUIForm);
            if (aimUIForm == null)
            {
                return;
            }
            aimUIForm.HideUI();
            dicCurrentShowUIForms.Remove(uiFormName);
        }

        private void CloseUIFormPopUIStack(string uiFormName)
        {
            //判断一下栈内是否有元素
            if (reverseUIStack.Count == 0)
            {
                IUIForm resultForm;
                dicCacheUIForms.TryGetValue(uiFormName, out resultForm);
                if (resultForm != null)
                {
                    resultForm.HideUI();
                }
                return;
            }
            IUIForm aimUIForm = reverseUIStack.Pop();
            aimUIForm.HideUI();
            if (reverseUIStack.Count >= 1)
            {
                reverseUIStack.Peek().ReDisplayUI();
                UpdateUIMask(reverseUIStack.Peek());
            }
        }


        private void CloseUIFormAndShowOther(string uiFormName)
        {
            IUIForm uiForm;
            dicCurrentShowUIForms.TryGetValue(uiFormName, out uiForm);
            if (uiForm == null)
            {
                return;
            }

            uiForm.HideUI();
            dicCurrentShowUIForms.Remove(uiFormName);

            foreach (var item in dicCurrentShowUIForms)
            {
                item.Value.ReDisplayUI();
            }

            foreach (var item in reverseUIStack)
            {
                item.ReDisplayUI();
            }
        }

        /// <summary>
        /// 关闭所有的UI窗体,调用该方法会将Reverse模式缓存的Stack清空
        /// </summary>
        /// <param name="closeType">关闭方式</param>
        public void CloseAllUIForm(CloseUIType closeType = CloseUIType.CloseNormal)
        {
            //记录需要删除的Key，无法在Foreach中remove会报错
            List<string> needToRemoveKey = new List<string>();
            switch (closeType)
            {
                case CloseUIType.CloseAll:
                    foreach (var itemUI in dicCurrentShowUIForms)
                    {
                        AfterCloseToRestoreParent(itemUI.Value);
                        itemUI.Value.HideUI();
                        needToRemoveKey.Add(itemUI.Key);
                    }

                    foreach (var itemInStack in reverseUIStack)
                    {
                        if (itemInStack.GetThisMonoBehavior().gameObject.activeSelf)
                        {
                            AfterCloseToRestoreParent(itemInStack);
                            itemInStack.HideUI();
                        }
                    }
                    break;
                case CloseUIType.CloseNormal:
                    foreach (var itemUI in dicCurrentShowUIForms)
                    {
                        if (itemUI.Value.CurrentUIType().uiFormType == UIFormType.Normal)
                        {
                            AfterCloseToRestoreParent(itemUI.Value);
                            itemUI.Value.HideUI();
                            needToRemoveKey.Add(itemUI.Key);
                        }
                    }

                    foreach (var itemInStack in reverseUIStack)
                    {
                        if (itemInStack.GetThisMonoBehavior().gameObject.activeSelf)
                        {
                            AfterCloseToRestoreParent(itemInStack);
                            itemInStack.HideUI();
                        }
                    }
                    break;
                case CloseUIType.CloseFixed:
                    foreach (var itemUI in dicCurrentShowUIForms)
                    {
                        if (itemUI.Value.CurrentUIType().uiFormType == UIFormType.Fixed)
                        {
                            AfterCloseToRestoreParent(itemUI.Value);
                            itemUI.Value.HideUI();
                            needToRemoveKey.Add(itemUI.Key);
                        }
                    }
                    foreach (var itemInStack in reverseUIStack)
                    {
                        if (itemInStack.GetThisMonoBehavior().gameObject.activeSelf)
                        {
                            AfterCloseToRestoreParent(itemInStack);
                            itemInStack.HideUI();
                        }
                    }
                    break;
                case CloseUIType.ClosePopUp:
                    foreach (var itemUI in dicCurrentShowUIForms)
                    {
                        if (itemUI.Value.CurrentUIType().uiFormType == UIFormType.PopUp)
                        {
                            AfterCloseToRestoreParent(itemUI.Value);
                            itemUI.Value.HideUI();
                            needToRemoveKey.Add(itemUI.Key);
                        }
                    }
                    foreach (var itemInStack in reverseUIStack)
                    {
                        if (itemInStack.GetThisMonoBehavior().gameObject.activeSelf)
                        {
                            AfterCloseToRestoreParent(itemInStack);
                            itemInStack.HideUI();
                        }
                    }
                    break;
            }
            //清空缓存栈
            ClearReverseUIStack();
            //重置UIMask遮罩
            RestoreUIMask();
            for (int i = 0; i < needToRemoveKey.Count; i++)
            {
                if (dicCurrentShowUIForms.ContainsKey(needToRemoveKey[i]))
                {
                    dicCurrentShowUIForms.Remove(needToRemoveKey[i]);
                }
            }
        }

        /// <summary>
        /// 获取目标UIForm
        /// </summary>
        /// <typeparam name="T">目标UIForm脚本</typeparam>
        public T GetUIForm<T>() where T : IUIForm
        {
            string formName = typeof(T).Name;
            IUIForm resultForm;
            dicCacheUIForms.TryGetValue(formName, out resultForm);
            if (resultForm == null)
            {
                resultForm = LoadAimUIFormFromCacheDic(formName);
            }
            return resultForm.GetThisMonoBehavior().GetComponent<T>();
        }
    }
}


