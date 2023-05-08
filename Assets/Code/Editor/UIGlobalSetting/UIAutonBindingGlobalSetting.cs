using UnityEditor;
using UnityEngine;

namespace CloudMusicEditor
{
    /// <summary>
    /// 自动绑定组件全局配置
    /// </summary>
    internal class UIAutonBindingGlobalSetting :ScriptableObject
    {
        /// <summary>
        /// 代码路径
        /// </summary>
        [SerializeField]
        private string m_CodePath;

        [SerializeField]
        private string m_MainCodePath;

        /// <summary>
        /// 命名空间
        /// </summary>
        [SerializeField]
        private string m_Namespace;

        /// <summary>
        /// 代码路径
        /// </summary>
        public string CodePath => m_CodePath;

        public string MainCodePath => m_MainCodePath;

        /// <summary>
        /// 命名空间
        /// </summary>
        public string Namespace => m_Namespace;


        [MenuItem( "GameGlobalSetting/CreationUIAutonBindingData" )]
        private static void CreationUIAutonBindingGlobalSetting( )
        {
            string[] datas = AssetDatabase.FindAssets( "t:UIAutonBindingData" );
            if( datas.Length >= 1 )
            {
                string data = AssetDatabase.GUIDToAssetPath( datas[0] );
                EditorUtility.DisplayDialog( "警告" , $"已存在UIAutonBindingData，路径:{data}" , "确认" );
                return;
            }
            UIAutonBindingGlobalSetting auto = CreateInstance<UIAutonBindingGlobalSetting>( );
            AssetDatabase.CreateAsset( auto , "Assets/CodeAssetData/UIAutonBindingData.asset" );
            AssetDatabase.SaveAssets( );
            AssetDatabase.Refresh( );
        }
    }
}