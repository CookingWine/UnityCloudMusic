using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using CloudMusic;
using UIBindingData = CloudMusic.UIComponentBinding.UIBindingData;
using System.Reflection;
using System.IO;

namespace CloudMusicEditor
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Style" , "IDE0090:使用 \"new(...)\"" , Justification = "<挂起>" )]
    [CustomEditor( typeof( UIComponentBinding ) )]
    internal class UIComponentAutoBindToolInspector :Editor
    {
        private UIComponentBinding m_Target;

        private SerializedProperty m_BindDatas;
        private SerializedProperty m_BindComs;
        private readonly List<UIBindingData> m_TempList = new List<UIBindingData>( );
        private readonly List<string> m_TempFiledNames = new List<string>( );
        private readonly List<string> m_TempComponentTypeNames = new List<string>( );
        private readonly string[] s_AssemblyNames = { "Assembly-CSharp" };
        private string[] m_HelperTypeNames;
        private string m_HelperTypeName;
        private int m_HelperTypeNameIndex;

        private UIAutonBindingGlobalSetting m_Setting;

        private SerializedProperty m_Namespace;
        private SerializedProperty m_ClassName;
        private SerializedProperty m_CodePath;
        private SerializedProperty m_MainCodePath;

        private void OnEnable( )
        {
            m_Target = (UIComponentBinding)target;
            m_BindDatas = serializedObject.FindProperty( "BindingDatas" );
            m_BindComs = serializedObject.FindProperty( "m_BindComponent" );

            m_HelperTypeNames = GetTypeNames( typeof( IAutoBindingRuleHelper ) , s_AssemblyNames );

            string[] paths = AssetDatabase.FindAssets( "UIAutonBindingData" );
            if( paths.Length == 0 )
            {
                Debug.LogError( "不存在UIAutonBindingData" );
                return;
            }
            if( paths.Length > 1 )
            {
                Debug.LogError( "UIAutonBindingData数量大于1" );
                return;
            }
            string path = AssetDatabase.GUIDToAssetPath( paths[0] );
            m_Setting = AssetDatabase.LoadAssetAtPath<UIAutonBindingGlobalSetting>( path );


            m_Namespace = serializedObject.FindProperty( "m_Namespace" );
            m_ClassName = serializedObject.FindProperty( "m_ClassName" );
            m_CodePath = serializedObject.FindProperty( "m_CodePath" );
            m_MainCodePath = serializedObject.FindProperty( "m_MainCodePath" );
            m_Namespace.stringValue = string.IsNullOrEmpty( m_Namespace.stringValue ) ? m_Setting.Namespace : m_Namespace.stringValue;
            m_ClassName.stringValue = string.IsNullOrEmpty( m_ClassName.stringValue ) ? m_Target.gameObject.name : m_ClassName.stringValue;
            m_CodePath.stringValue = string.IsNullOrEmpty( m_CodePath.stringValue ) ? m_Setting.CodePath : m_CodePath.stringValue;
            m_MainCodePath.stringValue = string.IsNullOrEmpty( m_MainCodePath.stringValue ) ? m_Setting.MainCodePath : m_MainCodePath.stringValue;
            serializedObject.ApplyModifiedProperties( );
        }

        public override void OnInspectorGUI( )
        {
            serializedObject.Update( );

            DrawTopButton( );

            DrawHelperSelect( );

            DrawSetting( );

            DrawKvData( );

            serializedObject.ApplyModifiedProperties( );


        }

        /// <summary>
        /// 绘制顶部按钮
        /// </summary>
        private void DrawTopButton( )
        {
            EditorGUILayout.BeginHorizontal( );

            if( GUILayout.Button( "排序" ) )
            {
                Sort( );
            }

            if( GUILayout.Button( "全部删除" ) )
            {
                RemoveAll( );
            }

            if( GUILayout.Button( "删除空引用" ) )
            {
                RemoveNull( );
            }

            if( GUILayout.Button( "自动绑定组件" ) )
            {
                AutoBindComponent( );
            }

            if( GUILayout.Button( "生成绑定代码" ) )
            {
                GenAutoBindCode( );
            }

            EditorGUILayout.EndHorizontal( );
        }

        /// <summary>
        /// 排序
        /// </summary>
        private void Sort( )
        {
            m_TempList.Clear( );
            foreach( UIBindingData data in m_Target.BindingDatas )
            {
                m_TempList.Add( new UIBindingData( data.Name , data.BindComponent ) );
            }
            m_TempList.Sort( ( x , y ) =>
            {
                return string.Compare( x.Name , y.Name , StringComparison.Ordinal );
            } );

            m_BindDatas.ClearArray( );
            foreach( UIBindingData data in m_TempList )
            {
                AddBindData( data.Name , data.BindComponent );
            }

            SyncBindComs( );
        }

        /// <summary>
        /// 全部删除
        /// </summary>
        private void RemoveAll( )
        {
            m_BindDatas.ClearArray( );

            SyncBindComs( );
        }

        /// <summary>
        /// 删除空引用
        /// </summary>
        private void RemoveNull( )
        {
            for( int i = m_BindDatas.arraySize - 1 ; i >= 0 ; i-- )
            {
                SerializedProperty element = m_BindDatas.GetArrayElementAtIndex( i ).FindPropertyRelative( "BindComponent" );
                if( element.objectReferenceValue == null )
                {
                    m_BindDatas.DeleteArrayElementAtIndex( i );
                }
            }

            SyncBindComs( );
        }

        /// <summary>
        /// 自动绑定组件
        /// </summary>
        private void AutoBindComponent( )
        {
            m_BindDatas.ClearArray( );

            Transform[] childs = m_Target.gameObject.GetComponentsInChildren<Transform>( true );
            foreach( Transform child in childs )
            {
                m_TempFiledNames.Clear( );
                m_TempComponentTypeNames.Clear( );
                if( m_Target.RuleHelper.IsValidBind( child , m_TempFiledNames , m_TempComponentTypeNames ) )
                {
                    for( int i = 0 ; i < m_TempFiledNames.Count ; i++ )
                    {
                        Component com = child.GetComponent( m_TempComponentTypeNames[i] );
                        if( com == null )
                        {
                            Debug.LogError( $"{child.name}上不存在{m_TempComponentTypeNames[i]}的组件" );
                        }
                        else
                        {
                            AddBindData( m_TempFiledNames[i] , child.GetComponent( m_TempComponentTypeNames[i] ) );
                        }

                    }
                }
            }

            SyncBindComs( );
        }

        /// <summary>
        /// 绘制辅助器选择框
        /// </summary>
        private void DrawHelperSelect( )
        {
            m_HelperTypeName = m_HelperTypeNames[0];

            if( m_Target.RuleHelper != null )
            {
                m_HelperTypeName = m_Target.RuleHelper.GetType( ).Name;

                for( int i = 0 ; i < m_HelperTypeNames.Length ; i++ )
                {
                    if( m_HelperTypeName == m_HelperTypeNames[i] )
                    {
                        m_HelperTypeNameIndex = i;
                    }
                }
            }
            else
            {
                IAutoBindingRuleHelper helper = (IAutoBindingRuleHelper)CreateHelperInstance( m_HelperTypeName , s_AssemblyNames );
                m_Target.RuleHelper = helper;
            }

            foreach( GameObject go in Selection.gameObjects )
            {
                UIComponentBinding autoBindTool = go.GetComponent<UIComponentBinding>( );
                if( autoBindTool == null )
                {
                    continue;
                }
                if( autoBindTool.RuleHelper == null )
                {
                    IAutoBindingRuleHelper helper = (IAutoBindingRuleHelper)CreateHelperInstance( m_HelperTypeName , s_AssemblyNames );
                    autoBindTool.RuleHelper = helper;
                }
            }

            int selectedIndex = EditorGUILayout.Popup( "AutoBindRuleHelper" , m_HelperTypeNameIndex , m_HelperTypeNames );
            if( selectedIndex != m_HelperTypeNameIndex )
            {
                m_HelperTypeNameIndex = selectedIndex;
                m_HelperTypeName = m_HelperTypeNames[selectedIndex];
                IAutoBindingRuleHelper helper = (IAutoBindingRuleHelper)CreateHelperInstance( m_HelperTypeName , s_AssemblyNames );
                m_Target.RuleHelper = helper;

            }
        }

        /// <summary>
        /// 绘制设置项
        /// </summary>
        private void DrawSetting( )
        {
            EditorGUILayout.BeginHorizontal( );
            m_Namespace.stringValue = EditorGUILayout.TextField( new GUIContent( "命名空间：" ) , m_Namespace.stringValue );
            if( GUILayout.Button( "默认设置" ) )
            {
                m_Namespace.stringValue = m_Setting.Namespace;
            }
            EditorGUILayout.EndHorizontal( );

            EditorGUILayout.BeginHorizontal( );
            m_ClassName.stringValue = EditorGUILayout.TextField( new GUIContent( "类名：" ) , m_ClassName.stringValue );
            if( GUILayout.Button( "物体名" ) )
            {
                m_ClassName.stringValue = m_Target.gameObject.name;
            }
            EditorGUILayout.EndHorizontal( );

            EditorGUILayout.LabelField( "UI组件保存路径：" );
            EditorGUILayout.LabelField( Application.dataPath + m_CodePath.stringValue );
            EditorGUILayout.BeginHorizontal( );
            //if( GUILayout.Button( "选择路径" ) )
            //{
            //    string temp = m_CodePath.stringValue;
            //    m_CodePath.stringValue = EditorUtility.OpenFolderPanel( "选择代码保存路径" , Application.dataPath , "" );
            //    if( string.IsNullOrEmpty( m_CodePath.stringValue ) )
            //    {
            //        m_CodePath.stringValue = temp;
            //    }
            //}
            //if( GUILayout.Button( "默认设置" ) )
            //{
            //    m_CodePath.stringValue = m_Setting.CodePath;
            //}
            EditorGUILayout.EndHorizontal( );

            EditorGUILayout.LabelField( "UICode保存路径" );
            EditorGUILayout.LabelField( Application.dataPath + m_MainCodePath.stringValue );

            EditorGUILayout.BeginHorizontal( );
            //if( GUILayout.Button( "更新路径" ) )
            //{
            //    string temp = m_MainCodePath.stringValue;
            //    m_MainCodePath.stringValue = EditorUtility.OpenFolderPanel( "选择代码保存路径" , Application.dataPath , "" );
            //    if( string.IsNullOrEmpty( m_MainCodePath.stringValue ) )
            //    {
            //        m_MainCodePath.stringValue = temp;
            //    }
            //}
            //if( GUILayout.Button( "恢复默认" ) )
            //{
            //    m_MainCodePath.stringValue = m_Setting.MainCodePath;
            //}
            EditorGUILayout.EndHorizontal( );
            EditorGUILayout.BeginHorizontal( );
            EditorGUILayout.LabelField( "是否生成主体代码" );
            m_BuidMainCode = EditorGUILayout.Toggle( m_BuidMainCode );
            EditorGUILayout.EndHorizontal( );

        }
        private bool m_BuidMainCode;

        /// <summary>
        /// 绘制键值对数据
        /// </summary>
        private void DrawKvData( )
        {
            //绘制key value数据

            int needDeleteIndex = -1;

            EditorGUILayout.BeginVertical( );
            SerializedProperty property;

            for( int i = 0 ; i < m_BindDatas.arraySize ; i++ )
            {

                EditorGUILayout.BeginHorizontal( );
                EditorGUILayout.LabelField( $"[{i}]" , GUILayout.Width( 25 ) );
                property = m_BindDatas.GetArrayElementAtIndex( i ).FindPropertyRelative( "Name" );
                property.stringValue = EditorGUILayout.TextField( property.stringValue , GUILayout.Width( 150 ) );
                property = m_BindDatas.GetArrayElementAtIndex( i ).FindPropertyRelative( "BindComponent" );
                property.objectReferenceValue = EditorGUILayout.ObjectField( property.objectReferenceValue , typeof( Component ) , true );
                if( GUILayout.Button( "X" ) )
                {
                    //将元素下标添加进删除list
                    needDeleteIndex = i;
                }
                EditorGUILayout.EndHorizontal( );
            }

            //删除data
            if( needDeleteIndex != -1 )
            {
                m_BindDatas.DeleteArrayElementAtIndex( needDeleteIndex );
                SyncBindComs( );
            }

            EditorGUILayout.EndVertical( );
        }



        /// <summary>
        /// 添加绑定数据
        /// </summary>
        private void AddBindData( string name , Component bindCom )
        {
            int index = m_BindDatas.arraySize;
            m_BindDatas.InsertArrayElementAtIndex( index );
            SerializedProperty element = m_BindDatas.GetArrayElementAtIndex( index );
            element.FindPropertyRelative( "Name" ).stringValue = name;
            element.FindPropertyRelative( "BindComponent" ).objectReferenceValue = bindCom;
        }

        /// <summary>
        /// 同步绑定数据
        /// </summary>
        private void SyncBindComs( )
        {
            m_BindComs.ClearArray( );

            for( int i = 0 ; i < m_BindDatas.arraySize ; i++ )
            {
                SerializedProperty property = m_BindDatas.GetArrayElementAtIndex( i ).FindPropertyRelative( "BindComponent" );
                m_BindComs.InsertArrayElementAtIndex( i );
                m_BindComs.GetArrayElementAtIndex( i ).objectReferenceValue = property.objectReferenceValue;
            }
        }

        /// <summary>
        /// 获取指定基类在指定程序集中的所有子类名称
        /// </summary>
        private string[] GetTypeNames( Type typeBase , string[] assemblyNames )
        {
            List<string> typeNames = new List<string>( );
            foreach( string assemblyName in assemblyNames )
            {
                Assembly assembly = null;
                try
                {
                    assembly = Assembly.Load( assemblyName );
                }
                catch
                {
                    continue;
                }

                if( assembly == null )
                {
                    continue;
                }

                Type[] types = assembly.GetTypes( );
                foreach( Type type in types )
                {
                    if( type.IsClass && !type.IsAbstract && typeBase.IsAssignableFrom( type ) )
                    {
                        typeNames.Add( type.FullName );
                    }
                }
            }

            typeNames.Sort( );
            return typeNames.ToArray( );
        }

        /// <summary>
        /// 创建辅助器实例
        /// </summary>
        private object CreateHelperInstance( string helperTypeName , string[] assemblyNames )
        {
            foreach( string assemblyName in assemblyNames )
            {
                Assembly assembly = Assembly.Load( assemblyName );

                object instance = assembly.CreateInstance( helperTypeName );
                if( instance != null )
                {
                    return instance;
                }
            }

            return null;
        }


        /// <summary>
        /// 生成自动绑定代码
        /// </summary>
        private void GenAutoBindCode( )
        {
            GameObject go = m_Target.gameObject;

            string className = !string.IsNullOrEmpty( m_Target.ClassName ) ? m_Target.ClassName : go.name;
            string codePath = !string.IsNullOrEmpty( m_Target.CodePath ) ? m_Target.CodePath : m_Setting.CodePath;
            string mainPath = !string.IsNullOrEmpty( m_Target.MainCodePath ) ? m_Target.MainCodePath : m_Setting.MainCodePath;
            codePath = Application.dataPath + codePath;
            mainPath = Application.dataPath + mainPath;
            if( !Directory.Exists( codePath ) )
            {
                Debug.LogError( $"{go.name}的代码保存路径{codePath}无效" );
            }

            using( StreamWriter sw = new StreamWriter( $"{codePath}/{className}.BindComponents.cs" ) )
            {
                sw.WriteLine( "using TMPro;" );
                sw.WriteLine( "using UnityEngine;" );
                sw.WriteLine( "using UnityEngine.UI;" );
                sw.WriteLine( "" );

                sw.WriteLine( "//自动生成于：" + DateTime.Now );

                if( !string.IsNullOrEmpty( m_Target.Namespace ) )
                {
                    //命名空间
                    sw.WriteLine( "namespace " + m_Target.Namespace );
                    sw.WriteLine( "{" );
                    sw.WriteLine( "" );

                    //类名
                    sw.WriteLine( $"\tpublic partial class {className}" );
                    sw.WriteLine( "\t{" );
                    sw.WriteLine( "" );

                    //组件字段
                    foreach( UIBindingData data in m_Target.BindingDatas )
                    {
                        sw.WriteLine( $"\t\tprivate {data.BindComponent.GetType( ).Name} m_{data.Name};" );
                    }
                    sw.WriteLine( "" );

                    sw.WriteLine( "\t\tprivate void GetBindComponent(GameObject go)" );
                    sw.WriteLine( "\t\t{" );

                    //获取autoBindTool上的Component
                    sw.WriteLine( $"\t\t\tUIComponentBinding autoBindTool = go.GetComponent<UIComponentBinding>();" );

                    //根据索引获取

                    for( int i = 0 ; i < m_Target.BindingDatas.Count ; i++ )
                    {
                        UIBindingData data = m_Target.BindingDatas[i];
                        string filedName = $"m_{data.Name}";
                        sw.WriteLine( $"\t\t\t{filedName} = autoBindTool.GetBindComponent<{data.BindComponent.GetType( ).Name}>({i});" );
                    }

                    sw.WriteLine( "\t\t}" );

                    sw.WriteLine( "\t}" );
                    sw.WriteLine( "}" );
                }
                else
                {
                    //类名
                    sw.WriteLine( $"public partial class {className}" );
                    sw.WriteLine( "{" );
                    sw.WriteLine( "" );

                    //组件字段
                    foreach( UIBindingData data in m_Target.BindingDatas )
                    {
                        sw.WriteLine( $"\tprivate {data.BindComponent.GetType( ).Name} m_{data.Name};" );
                    }
                    sw.WriteLine( "" );

                    sw.WriteLine( "\tprivate void InitBindComponent(GameObject go)" );
                    sw.WriteLine( "\t{" );

                    //获取autoBindTool上的Component
                    sw.WriteLine( $"\t\tUIComponentBinding autoBindTool = go.GetComponent<UIComponentBinding>();" );

                    //根据索引获取

                    for( int i = 0 ; i < m_Target.BindingDatas.Count ; i++ )
                    {
                        UIBindingData data = m_Target.BindingDatas[i];
                        string filedName = $"m_{data.Name}";
                        sw.WriteLine( $"\t\t{filedName} = autoBindTool.GetBindComponent<{data.BindComponent.GetType( ).Name}>({i});" );
                    }

                    sw.WriteLine( "\t}" );

                    sw.WriteLine( "}" );
                }
            }

            if( m_BuidMainCode )
            {
                using( StreamWriter sw = new StreamWriter( $"{mainPath}/{className}.cs" ) )
                {
                    sw.WriteLine( "using UnityEngine;" );
                    sw.WriteLine( "using UnityEngine.UI;" );
                    sw.WriteLine( "" );
                    sw.WriteLine( "//自动生成于:" + DateTime.Now );
                    if( !string.IsNullOrEmpty( m_Target.Namespace ) )
                    {
                        //命名空间
                        sw.WriteLine( "namespace " + m_Target.Namespace );
                        sw.WriteLine( "{" );
                        //类名
                        sw.WriteLine( $"\tpublic partial class {className} :MonoBehaviour" );
                        sw.WriteLine( "\t{" );
                        sw.WriteLine( "\t}" );
                        sw.WriteLine( "}" );
                    }
                    else
                    {
                        //类名
                        sw.WriteLine( $"public partial class {className} :MonoBehaviour" );
                        sw.WriteLine( "{" );
                        sw.WriteLine( "}" );
                    }
                }
            }
            AssetDatabase.Refresh( );
            EditorUtility.DisplayDialog( "提示" , "代码生成完毕" , "OK" );
        }
    }
}