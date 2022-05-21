namespace FreeFramework 
{
    /// <summary>
    /// UI窗体初始化类型
    /// </summary>
    public class UIType 
    {
        //是否需要清除反向切换的缓存
        public bool isClearReverseChange = false;
        //UI窗体类型
        public UIFormType uiFormType = UIFormType.Normal;
        //UI窗体显示方式
        public UIFormShowMode uiFormShowMode = UIFormShowMode.Normal;
        //UI窗体穿透性
        public UIFormPenetrability uiFormPenetrability = UIFormPenetrability.Pentrate;

        public UIType() { }

        public UIType(UIFormType formType,UIFormShowMode showType,UIFormPenetrability penetrability,bool isClearStack = false) 
        {
            uiFormPenetrability = penetrability;
            uiFormShowMode = showType;
            uiFormType = formType;
            isClearReverseChange = isClearStack;
        }
    }

    /// <summary>
    /// UI窗体类型
    /// </summary>
    public enum UIFormType 
    {
        //普通
        Normal,
        //固定
        Fixed,
        //弹窗
        PopUp
    }

    /// <summary>
    /// 窗体显示类型
    /// </summary>
    public enum UIFormShowMode 
    {
        //正常叠加显示
        Normal,
        //反向切换显示
        ReverseChange,
        //显示自己，隐藏其他窗体
        HideOther
    }

    /// <summary>
    /// UIForm的可穿透性,弹出“模态窗体”不同透明度的类型
    /// </summary>
    public enum UIFormPenetrability 
    {
        //可以穿透，空白区域透明
        Pentrate,
        //空白区域透明但是不能穿透
        LucencyImPentrate,
        //空白区域低透明度，不可穿透
        ImPenetrable
    }

    /// <summary>
    /// 关闭所有窗体的方式
    /// </summary>
    public enum CloseUIType
    {
        //关闭所有窗体
        CloseAll,
        //关闭普通窗体
        CloseNormal,
        //关闭固定窗体
        CloseFixed,
        //关闭弹出窗体
        ClosePopUp
    }
}
