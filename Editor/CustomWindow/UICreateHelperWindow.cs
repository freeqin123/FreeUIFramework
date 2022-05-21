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
        private const string namespaceTag = "#�����ռ�#";
        private const string classNameTag = "#����#";
        private const string uiFormTypeTag = "#����#";
        private const string uiFormShowTag = "#��ʾ#";
        private const string uiFormPenetrabilityTag = "#��͸#";

        #region ������Դ·�����
        private bool isOpenConfig = true;
        #endregion

        #region ������Դ·�����
        private bool isOpenCreate = true;
        #endregion

        #region ����UI�������
        private bool isOpenUICreate = true;
        private string uiFormName;
        private UIFormType uiFormType = UIFormType.Normal;
        private UIFormShowMode uiFormShowMode = UIFormShowMode.Normal;
        private UIFormPenetrability uiFormPenetrability = UIFormPenetrability.Pentrate;
        private UIManager.LoadConfigType configLoadType;
        private string nameSpaceName;
        private string scriptsSavePath = "/Scripts/";
        #endregion

        #region �鿴��ǰ����UI������Ϣ
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
            //������������ļ�
            panelAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);

            if (panelAsset == null)
            {
                Debug.Log($"<color=red>��Դ�ļ�Ϊ�գ�</color>:{uxmlPath}");
            }
        }

        private void OnEnable()
        {

            if (panelAsset == null)
            {
                Debug.Log($"<color=red>��Դ�ļ�Ϊ�գ�</color>:{uxmlPath}");
            }

            panelAsset.CloneTree(rootVisualElement);
            rootVisualElement.Q<IMGUIContainer>("Content").onGUIHandler = ONGUIHandler;
            scrollViewPos = Vector2.zero;

            //����·��
            jsonPath = $"Assets/Resources/{UIConstant.uiConfigJsonPath}.json";
            configAssetPath = $"Assets/Resources/{UIConstant.uiConfigAssetPath}.asset";
            nameSpaceName = EditorPrefs.GetString("scriptsNamespace","MyGame");

            UpdateConfigLoadType();
        }

        /// <summary>
        /// ���µ�ǰcofig�ʼۼ��ط�ʽ
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
            //�洢�����ռ���Ϣ
            EditorPrefs.SetString("scriptsNamespace", nameSpaceName);
        }

        private void ONGUIHandler()
        {
            scrollViewPos = EditorGUILayout.BeginScrollView(scrollViewPos);

            isOpenCreate = EditorGUILayout.BeginFoldoutHeaderGroup(isOpenCreate, "������Դ����");
            if (isOpenCreate)
            {
                ShowCreateConfigGUI();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space();

            isOpenConfig = EditorGUILayout.BeginFoldoutHeaderGroup(isOpenConfig, "������Դ�鿴");
            if (isOpenConfig)
            {
                ShowOpenConfigGUI();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space();
            isOpenUICreate = EditorGUILayout.BeginFoldoutHeaderGroup(isOpenUICreate, "UI��Դ����");
            if (isOpenUICreate)
            {
                ShowOpenUICreate();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space();
            isOpenCheckUIFormInfo = EditorGUILayout.BeginFoldoutHeaderGroup(isOpenCheckUIFormInfo, "UIForm��Ϣ");
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
                        EditorGUILayout.HelpBox("UI�����ļ���UIForm��Ϣ����Ϊ0", MessageType.Warning);
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
                                GUILayout.Label($"<color=#00000000>��</color><b><color=#ffff00ff>{i + 1}.{groupProperty.GetArrayElementAtIndex(i).FindPropertyRelative("uiID").stringValue}</color></b>", style);
                            }
                            else 
                            {
                                GUILayout.Label($"<color=#00000000>��</color><b><color=#008000ff>{i + 1}.{groupProperty.GetArrayElementAtIndex(i).FindPropertyRelative("uiID").stringValue}</color></b>", style);
                            }

                            if (GUILayout.Button("��λ"))
                            {
                             
                                var uiForm = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Resources/{groupProperty.GetArrayElementAtIndex(i).FindPropertyRelative("uiPrefabPath").stringValue}.prefab");
                                if (uiForm == null)
                                {
                                    Debug.Log("������Դʧ��");
                                    Debug.Log($"<b>��λAssets/Resources/{groupProperty.GetArrayElementAtIndex(i).FindPropertyRelative("uiPrefabPath").stringValue}.prefab</b>");
                                }
                                EditorGUIUtility.PingObject(uiForm);
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                }
                else 
                {
                    EditorGUILayout.HelpBox("UI�����ļ�Ŀǰ������", MessageType.Warning);
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
                        EditorGUILayout.HelpBox("UI�����ļ���UIForm��Ϣ����Ϊ0", MessageType.Warning);
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
                                GUILayout.Label($"<color=#00000000>��</color><b><color=#ffff00ff>{i + 1}.{config.infoGroup[i].uiID}</color></b>", style);
                            }
                            else
                            {
                                GUILayout.Label($"<color=#00000000>��</color><b><color=#008000ff>{i + 1}.{config.infoGroup[i].uiID}</color></b>", style);
                            }

                            if (GUILayout.Button("��λ"))
                            {
                                var uiForm = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Resources/{config.infoGroup[i].uiPrefabPath}.prefab");
                                if (uiForm == null)
                                {
                                    Debug.Log("������Դʧ��");
                                    Debug.Log($"<b>��λAssets/Resources/{config.infoGroup[i].uiPrefabPath}.prefab</b>");
                                }
                                EditorGUIUtility.PingObject(uiForm);
                            }
                            EditorGUILayout.EndHorizontal();

                        }
                    }

                }
                else
                {
                    EditorGUILayout.HelpBox("UI�����ļ�Ŀǰ������", MessageType.Warning);
                }

            }

        }

         private void ShowOpenUICreate()
         {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("1.�ڳ����м���UICanvas��Ԥ���壺");
            if (GUILayout.Button("����"))
            {
                //����Ԥ����
                GameObject canvasPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Packages/com.freeqin.freeuiframework/Runtime/UIFrameworkPrefabs/UIManager/Canvas.prefab");
                var sceneObj = PrefabUtility.InstantiatePrefab(canvasPrefab) as GameObject;
                sceneObj.transform.SetAsLastSibling();
                UpdateConfigLoadType();
            }
            //if (GUILayout.Button("��λ��Դ�ļ�"))
            //{
            //    //����CanvasԤ����
            //    GameObject canvasPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Scripts/FreeFramework/UIFrameworkPrefabs/UIManager/Canvas.prefab");
            //    EditorGUIUtility.PingObject(canvasPrefab);
            //}
            GUILayout.EndHorizontal();
            EditorGUILayout.LabelField("2.����UI���壺");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("   UI�������ƣ�����ΪӢ�ģ���");
            uiFormName = EditorGUILayout.TextField(uiFormName);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("   UI��������:  ");
            uiFormType = (UIFormType)EditorGUILayout.EnumPopup(uiFormType);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("   UI������ʾ����:  ");
            uiFormShowMode = (UIFormShowMode)EditorGUILayout.EnumPopup(uiFormShowMode);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("   UI���崩͸����:  ");
            uiFormPenetrability = (UIFormPenetrability)EditorGUILayout.EnumPopup(uiFormPenetrability);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("   UI����ű����������ռ�: ");
            nameSpaceName = EditorGUILayout.TextField(nameSpaceName);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("   UI����ű��洢·��: ");
            scriptsSavePath = EditorGUILayout.TextField(scriptsSavePath);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("   UI������Ϣ�洢��ʽ:  ");
            configLoadType = (UIManager.LoadConfigType)EditorGUILayout.EnumPopup(configLoadType);
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("����UIForm�ű�"))
            {
                if (CheckStringContentChineseChar(uiFormName))
                {
                    EditorUtility.DisplayDialog("�����Ƿ�", $"��������<{uiFormName}>�У����зǷ��ַ�����������ΪӢ���ַ���", "ȷ��");
                    return;
                }

                if (CheckStringContentChineseChar(nameSpaceName))
                {
                    EditorUtility.DisplayDialog("�����Ƿ�", $"����ű������ռ�<{nameSpaceName}>�У����зǷ��ַ�����������ΪӢ���ַ���", "ȷ��");
                    return;
                }

                Debug.Log($"��������{uiFormName}");

                CreateUIScripts();
            }

            if (GUILayout.Button("����UIFormԤ����"))
            {
                if (CheckStringContentChineseChar(uiFormName))
                {
                    EditorUtility.DisplayDialog("�����Ƿ�", $"��������<{uiFormName}>�У����зǷ��ַ�����������ΪӢ���ַ���", "ȷ��");
                    return;
                }

                if (CheckStringContentChineseChar(nameSpaceName))
                {
                    EditorUtility.DisplayDialog("�����Ƿ�", $"����ű������ռ�<{nameSpaceName}>�У����зǷ��ַ�����������ΪӢ���ַ���", "ȷ��");
                    return;
                }

                Debug.Log($"��������{uiFormName}");

                CreateUIPrefabAndConfig();
            }
        }


        /// <summary>
        /// ����UIԤ���壬UI����
        /// </summary>
        private void CreateUIPrefabAndConfig()
        {
            Type aimType = GetTypeByName(uiFormName);
            if (aimType == null)
            {
                EditorUtility.DisplayDialog("δ�����ű�", $"UI����ű�δ���д���<{uiFormName}>�����ȴ����ű���", "�ر�");
            }
            else 
            {
                //�ڳ���������һ��Ԥ����
                var objectAsset = AssetDatabase.LoadAssetAtPath<GameObject>("Packages/com.freeqin.freeuiframework/Runtime/UIFrameworkPrefabs/UIManager/UIForm.prefab");
                //�ҵ������е�Canvas
                var canvas = FindObjectOfType<UIManager>();
                if (canvas == null)
                {
                    EditorUtility.DisplayDialog("δ�ҵ�����", "δ�ҵ�UI�����壬��Ҫ���<����>��ť����CanvasԤ���壡", "��");
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

                //���¼���Ԥ����
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
                    Debug.LogWarning($"�������ļ����Ѵ���UIForm��{itemUIID}");
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
            Debug.Log("���������ļ�");
            
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
                            Debug.LogWarning($"�������Ѵ�����ͬID��UIForm:{prefabId}");
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
            //TODO:������Ӧ�ű�
            var template = AssetDatabase.LoadAssetAtPath<TextAsset>("Packages/com.freeqin.freeuiframework/Editor/UITemplate.txt");
            var content = template.text;
            content = content.Replace(classNameTag, uiFormName);
            content = content.Replace(namespaceTag, nameSpaceName);
            content = content.Replace(uiFormTypeTag, uiFormType.ToString());
            content = content.Replace(uiFormShowTag, uiFormShowMode.ToString());
            content = content.Replace(uiFormPenetrabilityTag, uiFormPenetrability.ToString());
            Debug.Log($"��ǰ�ű�����{content}");

            var filePath = Application.dataPath + $"{scriptsSavePath}{uiFormName}.cs";
            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, content);
                AssetDatabase.Refresh();
            }
            else
            {
                Debug.LogWarning("�ļ��Ѿ�����");
            }
        }

        private void ShowCreateConfigGUI()
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("1.����Json�����ļ�:");
            if (GUILayout.Button("����"))
            {
                if (System.IO.File.Exists(jsonPath))
                {
                    EditorUtility.DisplayDialog("���贴��", "�Ѵ��������ļ���������д�����", "ȷ��");
                }
                else
                {
                    CheckExistsAimFolder($"{Application.dataPath}/Resources/UI/UIConfig", true);
                    var jsonData = new UIConfigObject();
                    jsonData.infoGroup = new List<UIConfigInfoBase>();
                    jsonData.infoGroup.Add(new UIConfigInfoBase("ID���ű�����ID������һ�£�", "����·����Ĭ�ϸ�Ŀ¼ΪResources"));
                    var jsonContent = JsonUtility.ToJson(jsonData);
                    //д��
                    System.IO.File.WriteAllText(jsonPath, jsonContent);
                    //ˢ��
                    AssetDatabase.Refresh();
                    Debug.Log("<color=green>�����ɹ�</color>");
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("2.����ConfigAssets�ļ�:");
            if (GUILayout.Button("����"))
            {
                CheckExistsAimFolder($"{Application.dataPath}/Resources/UI/UIConfig", true);
                var configAsset = AssetDatabase.LoadAssetAtPath<UIConfig>(configAssetPath);
                if (configAsset == null)
                {
                    //�����ļ�
                    AssetDatabase.CreateAsset(CreateInstance<UIConfig>(), configAssetPath);
                }
                else
                {
                    if (EditorUtility.DisplayDialog("����", "�Ѵ���Config�ļ������贴����", "ȷ��"))
                    {
                        EditorGUIUtility.PingObject(configAsset);
                    }
                }

                AssetDatabase.Refresh();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("3.����UIPrefabs�ļ�Ŀ¼:");
            if (GUILayout.Button("����"))
            {
                CheckExistsAimFolder($"{Application.dataPath}/{uiResourcesPath}", true);
                AssetDatabase.Refresh();
            }
            GUILayout.EndHorizontal();
        }

        private void ShowOpenConfigGUI()
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"1.��ǰUIJson�����ļ�����·����{UIConstant.uiConfigJsonPath}");
            if (GUILayout.Button("��λ"))
            {
                var result = AssetDatabase.LoadAssetAtPath<TextAsset>(jsonPath);
                if (result == null)
                {
                    Debug.Log("δ��λ����Դ�ļ�");
                }

                //��λ��Դλ��
                EditorGUIUtility.PingObject(result);
            }

            if (GUILayout.Button("��"))
            {
                var result = AssetDatabase.LoadAssetAtPath<TextAsset>(jsonPath);
                if (result == null)
                {
                    EditorUtility.DisplayDialog("ȱʧ��Դ", "δ��λ����Դ�ļ�", "ȷ��");
                }
                else
                {
                    AssetDatabase.OpenAsset(result);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"2.��ǰUIԤ����洢·����{UIConstant.uiConfigAssetPath}");
            if (GUILayout.Button("��λ"))
            {
                var result = AssetDatabase.LoadAssetAtPath<UIConfig>(configAssetPath);
                if (result == null)
                {
                    Debug.Log("δ��λ����Դ�ļ�");
                }

                //��λ��Դλ��
                EditorGUIUtility.PingObject(result);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"3.��ǰUIԤ����洢·����Assets/Resources/{UIConstant.uiPrefabAssetPath}(Ĭ�ϼ���Resources�ļ���)");
            GUILayout.EndHorizontal();

            //ȥ���ù���
            //if (GUILayout.Button("�޸�·��"))
            //{
            //    var scripts = AssetDatabase.LoadAssetAtPath<Object>(uiConstantScriptPath);
            //    if (scripts == null)
            //    {
            //        Debug.Log("��ǰ�ǿ�");
            //    }
            //    AssetDatabase.OpenAsset(scripts);
            //}
        }

        /// <summary>
        /// ����Ƿ����Ŀ���ļ���
        /// </summary>
        /// <param name="path"></param>
        /// <param name="needCreate">�Ƿ���Ҫ����</param>
        /// <returns></returns>
        private bool CheckExistsAimFolder(string path, bool needCreate = false)
        {
            if (System.IO.Directory.Exists(path))
            {
                Debug.Log($"����Ŀ¼{path}");
                return true;
            }

            if (needCreate)
            {
                System.IO.Directory.CreateDirectory(path);
                Debug.Log($"�ɹ�����Ŀ¼{path}");
            }
            return false;
        }


        /// <summary>
        /// �ж��ַ������Ƿ���������ַ�
        /// </summary>
        /// <param name="value"></param>
        /// <returns>TrueΪ���к��֣�FalseΪ������</returns>
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
        /// �����ַ������ƻ�ȡ����
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