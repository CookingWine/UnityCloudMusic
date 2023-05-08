using System;
using System.Collections.Generic;
using UnityEngine;

namespace CloudMusic
{
    public class UIComponentBinding :MonoBehaviour
    {
#if UNITY_EDITOR
        /// <summary>
        /// UI绑定数据
        /// </summary>
        [Serializable]
        public class UIBindingData
        {
            /// <summary>
            /// UI绑定数据
            /// </summary>
            /// <param name="name">物体名</param>
            /// <param name="component">组件</param>
            public UIBindingData( string name , Component component )
            {
                Name = name;
                BindComponent = component;
            }
            /// <summary>
            /// 物体名
            /// </summary>
            public string Name;

            /// <summary>
            /// 组件
            /// </summary>
            public Component BindComponent;

        }

        public List<UIBindingData> BindingDatas = new List<UIBindingData>( );

        ///<summary>类name</summary>
        [SerializeField]
        private string m_ClassName;

        ///<summary>命名空间</summary>
        [SerializeField]
        private string m_Namespace;

        ///<summary>代码路径</summary>
        [SerializeField]
        private string m_CodePath;

        [SerializeField]
        private string m_MainCodePath;

        ///<summary>类</summary>
        public string ClassName => m_ClassName;

        ///<summary>命名空间</summary>
        public string Namespace => m_Namespace;

        ///<summary>代码路径</summary>
        public string CodePath => m_CodePath;

        public string MainCodePath => m_MainCodePath;
        ///<summary>动绑定规则辅助器接口</summary>
        public IAutoBindingRuleHelper RuleHelper { get; set; }
#endif


        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Style" , "IDE0090:使用 \"new(...)\"" , Justification = "<挂起>" )]
        public List<Component> m_BindComponent = new List<Component>( );

        /// <summary>
        /// 获取绑定的组件
        /// </summary>
        /// <typeparam name="T">组件类型</typeparam>
        /// <param name="index">索引</param>
        /// <returns></returns>
        public T GetBindComponent<T>( int index ) where T : Component
        {
            if( index > m_BindComponent.Count )
            {
                return null;
            }
            T com = m_BindComponent[index] as T;
            if( com == null )
            {
                return null;
            }
            return com;
        }
    }
}