using UnityEditor;
using UnityEngine;

namespace CloudMusicEditor
{
    [CustomEditor( typeof( UIAutonBindingGlobalSetting ) )]
    internal class UIAutonBindingGlobalSettingInspector :Editor
    {
        private SerializedProperty m_Namespace;
        private SerializedProperty m_CodePath;
        private SerializedProperty m_MainCodePath;
        private void OnEnable( )
        {
            m_Namespace = serializedObject.FindProperty( "m_Namespace" );
            m_CodePath = serializedObject.FindProperty( "m_CodePath" );
            m_MainCodePath = serializedObject.FindProperty( "m_MainCodePath" );
        }

        public override void OnInspectorGUI( )
        {

            m_Namespace.stringValue = EditorGUILayout.TextField( new GUIContent( "默认命名空间" ) , m_Namespace.stringValue );

            EditorGUILayout.LabelField( "构建组件保存路径：" );
            EditorGUILayout.LabelField( m_CodePath.stringValue );
            m_CodePath.stringValue = "/Code/GameMain/UIBindingComponentBase";
            EditorGUILayout.LabelField( "构建ui保存路径:" );
            EditorGUILayout.LabelField( m_MainCodePath.stringValue );
            m_MainCodePath.stringValue = "/Code/GameMain/UIInterface";

            serializedObject.ApplyModifiedProperties( );

        }
    }
}