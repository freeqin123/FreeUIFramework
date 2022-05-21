using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FreeFramework 
{
    [CreateAssetMenu(fileName ="UIConfig",menuName ="UIConfig",order = 1)]
    public class UIConfig : ScriptableObject
    {
        public List<UIConfigInfoBase> infoGroup = new List<UIConfigInfoBase>();
    }

    [System.Serializable]
    public class UIConfigObject 
    {
        public List<UIConfigInfoBase> infoGroup = new List<UIConfigInfoBase>();
    }

    [System.Serializable]
    public class UIConfigInfoBase 
    {
        public string uiID;
        public string uiPrefabPath;

        public UIConfigInfoBase() { }

        public UIConfigInfoBase(string id,string path) 
        {
            uiID = id;
            uiPrefabPath = path;
        }
    }

}

