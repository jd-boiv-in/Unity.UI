using System;
using JD.OS;
using UnityEngine;

namespace JD.UI.Components
{
    public class SafeArea : MonoBehaviour
    {
        [HideInInspector] public Canvas Canvas;
        [HideInInspector] public bool HasCanvas;
        [HideInInspector] public RectTransform Rect;
        
        private float _bottom;
        private Vector2Int _screen;
        private bool _update;
        private bool _checkCanvas;
        
        private void UpdateRect()
        {
            if (!HasCanvas && !_checkCanvas)
            {
                Canvas = gameObject.GetComponentInParent<Canvas>();
                HasCanvas = Canvas != null;
                _checkCanvas = true;
            }
            
            var safeArea = OSUtils.GetSafeArea();
            var scale = HasCanvas ? Canvas.scaleFactor : 1;
            
            var top = (Screen.height - safeArea.height - safeArea.y) / scale;
            var bottom = safeArea.y / scale;
            var left = (Screen.width - safeArea.width - safeArea.x) / scale;
            var right = safeArea.x / scale;
            
#if !UNITY_ANDROID
            _bottom = safeArea.y / scale / 2f;
#else
            _bottom = safeArea.y / scale;
#endif
            
            Rect.offsetMax = new Vector2(-left, -top);
            Rect.offsetMin = new Vector2(right, _bottom);
        }

        private void LateUpdate()
        {
            var screen = new Vector2Int(Screen.width, Screen.height);
            if (screen.x != _screen.x || screen.y != _screen.y || _update)
            {
                _screen = screen;
                UpdateRect();
                _update = !_update;
            }
        }

        private void OnValidate()
        {
            Rect = gameObject.GetComponent<RectTransform>();
            Canvas = gameObject.GetComponentInParent<Canvas>();
            HasCanvas = Canvas != null;
        }
    }
}