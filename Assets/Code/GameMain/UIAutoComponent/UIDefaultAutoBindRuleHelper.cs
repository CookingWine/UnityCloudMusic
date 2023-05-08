using System.Collections.Generic;
using UnityEngine;

namespace CloudMusic
{
    /// <summary>
    /// 默认自动绑定规则辅助器
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Style" , "IDE0090:使用 \"new(...)\"" , Justification = "<挂起>" )]
    public class UIDefaultAutoBindRuleHelper :IAutoBindingRuleHelper
    {

        /// <summary>
        /// 命名前缀与类型的映射
        /// </summary>
        private readonly Dictionary<string , string> m_PrefixesDict = new Dictionary<string , string>( )
    {
        {"Trans","Transform" },
        {"OldAnim","Animation"},
        {"NewAnim","Animator"},

        {"Rect","RectTransform"},
        {"Canvas","Canvas"},
        {"Group","CanvasGroup"},
        {"VGroup","VerticalLayoutGroup"},
        {"HGroup","HorizontalLayoutGroup"},
        {"GGroup","GridLayoutGroup"},
        {"TGroup","ToggleGroup"},

        {"Btn","Button"},
        {"Img","Image"},
        {"RImg","RawImage"},
        {"Txt","Text"},
        {"Input","InputField"},
        {"Slider","Slider"},
        {"Mask","Mask"},
        {"Mask2D","RectMask2D"},
        {"Tog","Toggle"},
        {"Sbar","Scrollbar"},
        {"SRect","ScrollRect"},
        {"Drop","Dropdown"},
        { "Source","AudioSource"},

        { "TInput","TMP_InputField"},
        { "TTxt","TextMeshProUGUI"},
    };

        public bool IsValidBind( Transform target , List<string> filedNames , List<string> componentTypeNames )
        {
            string[] strArray = target.name.Split( '_' );

            if( strArray.Length == 1 )
            {
                return false;
            }

            string filedName = strArray[strArray.Length - 1];

            for( int i = 0 ; i < strArray.Length - 1 ; i++ )
            {
                string str = strArray[i];
                if( m_PrefixesDict.TryGetValue( str , out string comName ) )
                {
                    filedNames.Add( $"{str}_{filedName}" );
                    componentTypeNames.Add( comName );
                }
                else
                {
                    Debug.LogError( $"{target.name}的命名中{str}不存在对应的组件类型，绑定失败" );
                    return false;
                }
            }

            return true;
        }
    }
}