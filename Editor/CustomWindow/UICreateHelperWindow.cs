using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
using System.Reflection;
using System;
using System.IO;

namespace FreeFramework.Editor
{
    public class UICreateHelperWindow : EditorWindow
    {
        private VisualTreeAsset panelAsset;
        private Vector2 scrollViewPos;

        private string jsonPath;
        private string configAssetPath;
        private const string uiConstantScriptPath = "Assets/Scripts/FreeFramework/_Script/UI/UIConstant.cs";
        private const string uiResourcesPath = "/Resources/UI/UIPrefabs";
        private const string uiConfigPath = "/Resources/UI/UIConfig/";
        private const string uxmlPath = "Packages/com.freeqin.freeuiframework/Editor/UXML/UIPanel.uxml";
        private const string namespaceTag = "#命名空间#";
        private const string classNameTag = "#类名#";
        private const string uiFormTypeTag = "#类型#";
        private const string uiFormShowTag = "#显示#";
        private const string uiFormPenetrabilityTag = "#穿透#";

        #region 设置资源路径相关
        private bool isOpenConfig = true;
        #endregion

        #region 创建资源路径相关
        private bool isOpenCreate = true;
        #endregion

        #region 创建UI窗体相关
        private bool isOpenUICreate = true;
        private string uiFormName;
        private UIFormType uiFormType = UIFormType.Normal;
        private UIFormShowMode uiFormShowMode = UIFormShowMode.Normal;
        private UIFormPenetrability uiFormPenetrability = UIFormPenetrability.Pentrate;
        private UIManager.LoadConfigType configLoadType;
        private string nameSpaceName;
        private string scriptsSavePath = "/Scripts/";
        #endregion

        #region 查看当前生成UI窗体信息
        private bool isOpenCheckUIFormInfo = true;
        #endregion
        [MenuItem("Tools/UIFrameworkPanel")]
        public static void OpenThisWindow()
        {
            var thisWindow = EditorWindow.GetWindow<UICreateHelperWindow>();
            thisWindow.minSize = new Vector2(300f, 500f);
            thisWindow.titleContent = new GUIContent("UIFrameWorkPanel");
            thisWindow.Show();
        }


        private void Awake()
        {
            //加载面板配置文件
            panelAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);

            if (panelAsset == null)
            {
                Debug.Log($"<color=red>资源文件为空！</color>:{uxmlPath}");
            }
        }

        private void OnEnable()
        {

            if (panelAsset == null)
            {
                Debug.Log($"<color=red>资源文件为空！</color>:{uxmlPath}");
            }

            panelAsset.CloneTree(rootVisualElement);
            rootVisualElement.Q<IMGUIContainer>("Content").onGUIHandler = ONGUIHandler;
            scrollViewPos = Vector2.zero;

            //更新路径
            jsonPath = $"Assets/Resources/{UIConstant.uiConfigJsonPath}.json";
            configAssetPath = $"Assets/Resources/{UIConstant.uiConfigAssetPath}.asset";
            nameSpaceName = EditorPrefs.GetString("scriptsNamespace","MyGame");

            UpdateConfigLoadType();
        }

        /// <summary>
        /// 更新当前cofig问价加载方式
        /// </summary>
        private void UpdateConfigLoadType() 
        {
            var uiManager = FindObjectOfType<UIManager>();
            if (uiManager != null)
            {
                configLoadType = uiManager.GetComponent<UIManager>().assetsType;
            }
        }

        private void OnDestroy()
        {
            //存储命名空间信息
            EditorPrefs.SetString("scriptsNamespace", nameSpaceName);
        }

        private void ONGUIHandler()
        {
            scrollViewPos = EditorGUILayout.BeginScrollView(scrollViewPos);

            isOpenCreate = EditorGUILayout.BeginFoldoutHeaderGroup(isOpenCreate, "配置资源生成");
            if (isOpenCreate)
            {
                ShowCreateConfigGUI();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space();

            isOpenConfig = EditorGUILayout.BeginFoldoutHeaderGroup(isOpenConfig, "配置资源查看");
            if (isOpenConfig)
            {
                ShowOpenConfigGUI();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space();
            isOpenUICreate = EditorGUILayout.BeginFoldoutHeaderGroup(isOpenUICreate, "UI资源创建");
            if (isOpenUICreate)
            {
                ShowOpenUICreate();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space();
            isOpenCheckUIFormInfo = EditorGUILayout.BeginFoldoutHeaderGroup(isOpenCheckUIFormInfo, "UIForm信息");
            if (isOpenCheckUIFormInfo)
            {
                ShowUIFormInfo();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.EndScrollView();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }


        private void ShowUIFormInfo() 
        {
            if (configLoadType == UIManager.LoadConfigType.UnityAssets)
            {

                if (File.Exists($"{Application.dataPath}/Resources/{UIConstant.uiConfigAssetPath}.asset"))
                {
                    var uiConfigAsset = AssetDatabase.LoadAssetAtPath<UIConfig>(configAssetPath);

                    SerializedObject uiObject = new SerializedObject(uiConfigAsset);
                    SerializedProperty groupProperty = uiObject.FindProperty("infoGroup");
                    var groupSize = groupProperty.arraySize;

                    if (groupSize <= 0)
                    {
                        EditorGUILayout.HelpBox("UI配置文件中UIForm信息个数为0", MessageType.Warning);
                    }
                    else 
                    {
                        for (int i = 0; i < groupSize; i++)
                        {
                            EditorGUILayout.BeginHorizontal();
                            GUIStyle style = new GUIStyle();
                            style.richText = true;

                            if (EditorGUIUtility.isProSkin)
                            {
                                GUILayout.Label($"<color=#00000000>口</color><b><color=#ffff00ff>{i + 1}.{groupProperty.GetArrayElementAtIndex(i).FindPropertyRelative("uiID").stringValue}</color></b>", style);
                            }
                            else 
                            {
                                GUILayout.Label($"<color=#00000000>口</color><b><color=#008000ff>{i + 1}.{groupProperty.GetArrayElementAtIndex(i).FindPropertyRelative("uiID").stringValue}</color></b>", style);
                            }

                            if (GUILayout.Button("定位"))
                            {
                             
                                var uiForm = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Resources/{groupProperty.GetArrayElementAtIndex(i).FindPropertyRelative("uiPrefabPath").stringValue}.prefab");
                                if (uiForm == null)
                                {
                                    Debug.Log("加载资源失败");
                                    Debug.Log($"<b>定位Assets/Resources/{groupProperty.GetArrayElementAtIndex(i).FindPropertyRelative("uiPrefabPath").stringValue}.prefab</b>");
                                }
                                EditorGUIUtility.PingObject(uiForm);
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                }
                else 
                {
                    EditorGUILayout.HelpBox("UI配置文件目前无内容", MessageType.Warning);
                }
            }
            else if (configLoadType == UIManager.LoadConfigType.Json)
            {
                UIConfigObject config = new UIConfigObject();
                if (File.Exists(jsonPath))
                {
                    var fileContent = File.ReadAllText(Application.dataPath + uiConfigPath + "UIFormsPath.json");

                    config = JsonUtility.FromJson<UIConfigObject>(fileContent);

                    if (config.infoGroup.Count <= 0)
                    {
                        EditorGUILayout.HelpBox("UI配置文件中UIForm信息个数为0", MessageType.Warning);
                    }
                    else 
                    {

                        for (int i = 0; i < config.infoGroup.Count; i++)
                        {
                            EditorGUILayout.BeginHorizontal();
                            GUIStyle style = new GUIStyle();
                            style.richText = true;

                            if (EditorGUIUtility.isProSkin)
                            {
                                GUILayout.Label($"<color=#00000000>口</color><b><color=#ffff00ff>{i + 1}.{config.infoGroup[i].uiID}</color></b>", style);
                            }
                            else
                            {
                                GUILayout.Label($"<color=#00000000>口</color><b><color=#008000ff>{i + 1}.{config.infoGroup[i].uiID}</color></b>", style);
                            }

                            if (GUILayout.Button("定位"))
                            {
                                var uiForm = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Resources/{config.infoGroup[i].uiPrefabPath}.prefab");
                                if (uiForm == null)
                                {
                                    Debug.Log("加载资源失败");
                                    Debug.Log($"<b>定位Assets/Resources/{config.infoGroup[i].uiPrefabPath}.prefab</b>");
                                }
                                EditorGUIUtility.PingObject(uiForm);
                            }
                            EditorGUILayout.EndHorizontal();

                        }
                    }

                }
                else
                {
                    EditorGUILayout.HelpBox("UI配置文件目前无内容", MessageType.Warning);
                }

            }

        }

         private void ShowOpenUICreate()
         {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("1.在场景中加载UICanvas根预制体：");
            if (GUILayout.Button("加载"))
            {
                //加载预制体
                GameObject canvasPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Packages/com.freeqin.freeuiframework/Runtime/UIFrameworkPrefabs/UIManager/Canvas.prefab");
                var sceneObj = PrefabUtility.InstantiatePrefab(canvasPrefab) as GameObject;
                sceneObj.transform.SetAsLastSibling();
                UpdateConfigLoadType();
            }
            //if (GUILayout.Button("定位资源文件"))
            //{
            //    //加载Canvas预制体
            //    GameObject canvasPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Scripts/FreeFramework/UIFrameworkPrefabs/UIManager/Canvas.prefab");
            //    EditorGUIUtility.PingObject(canvasPrefab);
            //}
            GUILayout.EndHorizontal();
            EditorGUILayout.LabelField("2.创建UI窗体：");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("   UI窗体名称（必须为英文）：");
            uiFormName = EditorGUILayout.TextField(uiFormName);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("   UI窗体类型:  ");
            uiFormType = (UIFormType)EditorGUILayout.EnumPopup(uiFormType);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("   UI窗体显示类型:  ");
            uiFormShowMode = (UIFormShowMode)EditorGUILayout.EnumPopup(uiFormShowMode);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("   UI窗体穿透类型:  ");
            uiFormPenetrability = (UIFormPenetrability)EditorGUILayout.EnumPopup(uiFormPenetrability);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("   UI窗体脚本所处命名空间: ");
            nameSpaceName = EditorGUILayout.TextField(nameSpaceName);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("   UI窗体脚本存储路径: ");
            scriptsSavePath = EditorGUILayout.TextField(scriptsSavePath);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("   UI窗体信息存储方式:  ");
            configLoadType = (UIManager.LoadConfigType)EditorGUILayout.EnumPopup(configLoadType);
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("创建UIForm脚本"))
            {
                if (CheckStringContentChineseChar(uiFormName))
                {
                    EditorUtility.DisplayDialog("命名非法", $"窗体名称<{uiFormName}>中，含有非法字符。命名必须为英文字符！", "确定");
                    return;
                }

                if (CheckStringContentChineseChar(nameSpaceName))
                {
                    EditorUtility.DisplayDialog("命名非法", $"窗体脚本命名空间<{nameSpaceName}>中，含有非法字符。命名必须为英文字符！", "确定");
                    return;
                }

                Debug.Log($"创建窗体{uiFormName}");

                CreateUIScripts();
            }

            if (GUILayout.Button("创建UIForm预制体"))
            {
                if (CheckStringContentChineseChar(uiFormName))
                {
                    EditorUtility.DisplayDialog("命名非法", $"窗体名称<{uiFormName}>中，含有非法字符。命名必须为英文字符！", "确定");
                    return;
                }

                if (CheckStringContentChineseChar(nameSpaceName))
                {
                    EditorUtility.DisplayDialog("命名非法", $"窗体脚本命名空间<{nameSpaceName}>中，含有非法字符。命名必须为英文字符！", "确定");
                    return;
                }

                Debug.Log($"创建窗体{uiFormName}");

                CreateUIPrefabAndConfig();
            }
        }


        /// <summary>
        /// 创建UI预制体，UI创建
        /// </summary>
        private void CreateUIPrefabAndConfig()
        {
            Type aimType = GetTypeByName(uiFormName);
            if (aimType == null)
            {
                EditorUtility.DisplayDialog("未创建脚本", $"UI窗体脚本未进行创建<{uiFormName}>，请先创建脚本！", "关闭");
            }
            else 
            {
                //在场景中生成一个预制体
                var objectAsset = AssetDatabase.LoadAssetAtPath<GameObject>("Packages/com.freeqin.freeuiframework/Runtime/UIFrameworkPrefabs/UIManager/UIForm.prefab");
                //找到场景中的Canvas
                var canvas = FindObjectOfType<UIManager>();
                if (canvas == null)
                {
                    EditorUtility.DisplayDialog("未找到物体", "未找到UI跟物体，需要点击<加载>按钮加载Canvas预制体！", "是");
                    return;
                }

                var objScene = (GameObject)PrefabUtility.InstantiatePrefab(objectAsset);

                switch (uiFormType)
                {
                    case UIFormType.Fixed:
                        objScene.transform.SetParent(canvas.transform.Find("Fixed"));
                        break;
                    case UIFormType.Normal:
                        objScene.transform.SetParent(canvas.transform.Find("Normal"));
                        break;
                    case UIFormType.PopUp:
                        objScene.transform.SetParent(canvas.transform.Find("PopUp"));
                        break;
                }
                objScene.name = uiFormName;
                objScene.transform.SetAsLastSibling();
                objScene.AddComponent(aimType);
                var path = $"{Application.dataPath}/Resources/UI/UIPrefabs/{uiFormName}.prefab";
                PrefabUtility.SaveAsPrefabAsset(objScene, path);
                DestroyImmediate(objScene);
                //PrefabUtility.UnloadPrefabContents(objScene);
                switch (configLoadType)
                {
                    case UIManager.LoadConfigType.UnityAssets:
                        UpdateUnityAssetsConfig(uiFormName, UIConstant.uiPrefabAssetPath + uiFormName);
                        break;
                    case UIManager.LoadConfigType.Json:
                        UpdateJsonUnityConfig(uiFormName,UIConstant.uiPrefabAssetPath + uiFormName);
                        break;
                    default:
                        break;
                }

                //重新加载预制体
                var newObjSceneAsset = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Resources/UI/UIPrefabs/{uiFormName}.prefab");
                var newObjScene = (GameObject)PrefabUtility.InstantiatePrefab(newObjSceneAsset);

                switch (uiFormType)
                {
                    case UIFormType.Fixed:
                        newObjScene.transform.SetParent(canvas.transform.Find("Fixed"));
                        break;
                    case UIFormType.Normal:
                        newObjScene.transform.SetParent(canvas.transform.Find("Normal"));
                        break;
                    case UIFormType.PopUp:
                        newObjScene.transform.SetParent(canvas.transform.Find("PopUp"));
                        break;
                }
            }
        }

        private void UpdateUnityAssetsConfig(string prefabId,string prefabPath) 
        {

            UIConfig uiConfigAsset;

            if (File.Exists($"{Application.dataPath}/Resources/UI/{UIConstant.uiConfigAssetPath}.asset"))
            {
               uiConfigAsset = AssetDatabase.LoadAssetAtPath<UIConfig>(configAssetPath);
            }
            else 
            {
               AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<UIConfig>(), configAssetPath);
               uiConfigAsset = AssetDatabase.LoadAssetAtPath<UIConfig>(configAssetPath);
            }

            SerializedObject uiObject = new SerializedObject(uiConfigAsset);
            SerializedProperty groupProperty = uiObject.FindProperty("infoGroup");
            var groupSize = groupProperty.arraySize;

            for (int i = 0; i < groupSize; i++)
            {
                string itemUIID = groupProperty.GetArrayElementAtIndex(i).FindPropertyRelative("uiID").stringValue;

                if (itemUIID == prefabId)
                {
                    Debug.LogWarning($"再配置文件中已存在UIForm：{itemUIID}");
                    return;
                }
            }
            groupProperty.InsertArrayElementAtIndex(groupSize);
            var newInsertElement = groupProperty.GetArrayElementAtIndex(groupSize);
            newInsertElement.FindPropertyRelative("uiID").stringValue = prefabId;
            newInsertElement.FindPropertyRelative("uiPrefabPath").stringValue = prefabPath;
            uiObject.ApplyModifiedProperties();

        }

        private void UpdateJsonUnityConfig(string prefabId,string prefabPath) 
        {
            Debug.Log("更新配置文件");
            
            UIConfigObject config = new UIConfigObject();

            if (File.Exists(jsonPath))
            {
                var fileContent = File.ReadAllText( Application.dataPath + uiConfigPath + "UIFormsPath.json");

                config = JsonUtility.FromJson<UIConfigObject>(fileContent);

                if (config.infoGroup != null)
                {

                    foreach (var item in config.infoGroup) 
                    {
                        if (item.uiID == prefabId)
                        {
                            Debug.LogWarning($"配置中已存在相同ID的UIForm:{prefabId}");
                            return;
                        }
                    }

                    config.infoGroup.Add(new UIConfigInfoBase(prefabId,prefabPath));
                }

            }
            else 
            {

                config.infoGroup = new List<UIConfigInfoBase>();
                config.infoGroup.Add(new UIConfigInfoBase(prefabId,prefabPath));
            }

            string newContent = JsonUtility.ToJson(config);

            File.WriteAllText(Application.dataPath + uiConfigPath + "UIFormsPath.json", newContent);

            AssetDatabase.Refresh();
        }

        private void CreateUIScripts() 
        {
            //TODO:创建对应脚本
            var template = AssetDatabase.LoadAssetAtPath<TextAsset>("Packages/com.freeqin.freeuiframework/Editor/UITemplate.txt");
            var content = template.text;
            content = content.Replace(classNameTag, uiFormName);
            content = content.Replace(namespaceTag, nameSpaceName);
            content = content.Replace(uiFormTypeTag, uiFormType.ToString());
            content = content.Replace(uiFormShowTag, uiFormShowMode.ToString());
            content = content.Replace(uiFormPenetrabilityTag, uiFormPenetrability.ToString());
            Debug.Log($"当前脚本数据{content}");

            var filePath = Application.dataPath + $"{scriptsSavePath}{uiFormName}.cs";
            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, content);
                AssetDatabase.Refresh();
            }
            else
            {
                Debug.LogWarning("文件已经存在");
            }
        }

        private void ShowCreateConfigGUI()
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("1.创建Json配置文件:");
            if (GUILayout.Button("创建"))
            {
                if (System.IO.File.Exists(jsonPath))
                {
                    EditorUtility.DisplayDialog("无需创建", "已存在配置文件，无需进行创建！", "确定");
                }
                else
                {
                    CheckExistsAimFolder($"{Application.dataPath}/Resources/UI/UIConfig", true);
                    var jsonData = new UIConfigObject();
                    jsonData.infoGroup = new List<UIConfigInfoBase>();
                    jsonData.infoGroup.Add(new UIConfigInfoBase("ID（脚本名和ID名保持一致）", "加载路径，默认根目录为Resources"));
                    var jsonContent = JsonUtility.ToJson(jsonData);
                    //写入
                    System.IO.File.WriteAllText(jsonPath, jsonContent);
                    //刷新
                    AssetDatabase.Refresh();
                    Debug.Log("<color=green>创建成功</color>");
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("2.创建ConfigAssets文件:");
            if (GUILayout.Button("创建"))
            {
                CheckExistsAimFolder($"{Application.dataPath}/Resources/UI/UIConfig", true);
                var configAsset = AssetDatabase.LoadAssetAtPath<UIConfig>(configAssetPath);
                if (configAsset == null)
                {
                    //创建文件
                    AssetDatabase.CreateAsset(CreateInstance<UIConfig>(), configAssetPath);
                }
                else
                {
                    if (EditorUtility.DisplayDialog("存在", "已存在Config文件，无需创建！", "确定"))
                    {
                        EditorGUIUtility.PingObject(configAsset);
                    }
                }

                AssetDatabase.Refresh();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("3.创建UIPrefabs文件目录:");
            if (GUILayout.Button("创建"))
            {
                CheckExistsAimFolder($"{Application.dataPath}/{uiResourcesPath}", true);
                AssetDatabase.Refresh();
            }
            GUILayout.EndHorizontal();
        }

        private void ShowOpenConfigGUI()
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"1.当前UIJson配置文件加载路径：{UIConstant.uiConfigJsonPath}");
            if (GUILayout.Button("定位"))
            {
                var result = AssetDatabase.LoadAssetAtPath<TextAsset>(jsonPath);
                if (result == null)
                {
                    Debug.Log("未定位到资源文件");
                }

                //定位资源位置
                EditorGUIUtility.PingObject(result);
            }

            if (GUILayout.Button("打开"))
            {
                var result = AssetDatabase.LoadAssetAtPath<TextAsset>(jsonPath);
                if (result == null)
                {
                    EditorUtility.DisplayDialog("缺失资源", "未定位到资源文件", "确定");
                }
                else
                {
                    AssetDatabase.OpenAsset(result);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"2.当前UI预制体存储路径：{UIConstant.uiConfigAssetPath}");
            if (GUILayout.Button("定位"))
            {
                var result = AssetDatabase.LoadAssetAtPath<UIConfig>(configAssetPath);
                if (result == null)
                {
                    Debug.Log("未定位到资源文件");
                }

                //定位资源位置
                EditorGUIUtility.PingObject(result);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"3.当前UI预制体存储路径：Assets/Resources/{UIConstant.uiPrefabAssetPath}(默认加载Resources文件夹)");
            GUILayout.EndHorizontal();

            //去掉该功能
            //if (GUILayout.Button("修改路径"))
            //{
            //    var scripts = AssetDatabase.LoadAssetAtPath<Object>(uiConstantScriptPath);
            //    if (scripts == null)
            //    {
            //        Debug.Log("当前是空");
            //    }
            //    AssetDatabase.OpenAsset(scripts);
            //}
        }

        /// <summary>
        /// 检查是否存在目标文件夹
        /// </summary>
        /// <param name="path"></param>
        /// <param name="needCreate">是否需要创建</param>
        /// <returns></returns>
        private bool CheckExistsAimFolder(string path, bool needCreate = false)
        {
            if (System.IO.Directory.Exists(path))
            {
                Debug.Log($"存在目录{path}");
                return true;
            }

            if (needCreate)
            {
                System.IO.Directory.CreateDirectory(path);
                Debug.Log($"成功创建目录{path}");
            }
            return false;
        }


        /// <summary>
        /// 判断字符串中是否带有中文字符
        /// </summary>
        /// <param name="value"></param>
        /// <returns>True为含有汉字，False为不含有</returns>
        private bool CheckStringContentChineseChar(string value)
        {
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] > 127)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 根据字符串名称获取类名
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static Type GetTypeByName(string typeName) 
        {
            Type type = null;
            Assembly[] projectAssemblyArray = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in projectAssemblyArray) 
            {
                var typeArray =  assembly.GetTypes();
                foreach (var typeChild in typeArray)
                {
                    if (typeChild.Name.Equals(typeName))
                    {
                        return typeChild;
                    }
                }
            }

            return type;
        } 
    }

}