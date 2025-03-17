#if !ODIN_INSPECTOR
using System;
using System.Diagnostics;

namespace Sirenix.OdinInspector
{
    public enum ObjectFieldAlignment
    {
        Left,
        Center,
        Right,
    }
    
    [Conditional("UNITY_EDITOR")]
    public class FoldoutGroupAttribute : Attribute
    {
        public FoldoutGroupAttribute(string groupName, float order = 0.0f) { }
        public FoldoutGroupAttribute(string groupName, bool expanded, float order = 0.0f) { }
    }
    
    [Conditional("UNITY_EDITOR")]
    public class PropertySpaceAttribute : Attribute
    {
        public PropertySpaceAttribute(float spaceBefore, float spaceAfter) { }
    }
    
    [Conditional("UNITY_EDITOR")]
    public class PreviewFieldAttribute : Attribute
    {
        public PreviewFieldAttribute(ObjectFieldAlignment alignment = ObjectFieldAlignment.Right) { }
    }
}
#endif