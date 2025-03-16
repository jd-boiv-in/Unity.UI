using System;
using JD.Colors;
using JD.Tween;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JD.UI.Components
{
    public class UIButton : UIBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
    {
        public new const float In = 0.10f;
        public new const float Out = 0.075f;

        public float Scale = 0.90f;
        public float Ratio = 1.0f;
        public float Gray = 0.75f;

        public const float GrayDisabled = 0.5f;
        
        [HideInInspector] public bool Interactable = true;

        public float DefaultGray = 1.0f;
        
        public bool AllowScale = true;
        public bool AllowGray = true;
        [NonSerialized] public bool IsEnabled;
        
        public UnityEngine.UI.Button.ButtonClickedEvent OnClick = new();

        public Action<int, Vector2> OnPress;
        public Action OnRelease;
        
        public Image[] ExtraDarken;
        private bool _hasExtraDarkenTween;
        
        public TextMeshProUGUI[] ExtraDarkenText;
        private bool _hasExtraDarkenTextTween;
        
        public RectTransform[] ExtraScale;
        private bool _hasExtraScaleTween;

        [HideInInspector] public Color ExtraColorNormal = new Color(0, 0, 0, 0);
        [HideInInspector] public Color ExtraColorPressed = new Color(0, 0, 0, 0.2f);
        [HideInInspector] public Image[] ExtraColors;
        [HideInInspector] private bool _hasExtraColorTween;
        
        [HideInInspector] public CanvasGroup ParentGroup;
        [HideInInspector] public bool HasParentGroup;
        
        private bool _pressed;
        private bool _down;
        public bool IsDown => _down;

        private bool _clicked;
        private Vector2 _position;
        
        private Vector3 _hiddenPosition;

        public void OnEnable()
        {
            _down = false;
            if (AllowScale) SetRectScale(1.0f);
            if (AllowGray) SetAllGray(DefaultGray);

            // TODO: Can this have a big impact CPU wise? Meh only on touch up + even if it is 100 buttons all will just do a `if` so should be minimal...
            UIGlobal.Instance.OnTouchUp += OnGlobalTouchUp;
        }

        public override void OnDisable()
        {
            base.OnDisable();
            
            UIGlobal.Instance.OnTouchUp -= OnGlobalTouchUp;
        }

        public bool IsInteractable()
        {
            if (HasParentGroup)
            {
                if (!ParentGroup.interactable || ParentGroup.alpha < 1f)
                    return false;
            }
            
            return Interactable && Alpha > 0f;
        }

        public void SetInteractable(bool value)
        {
            Interactable = value;
            Image.raycastTarget = value;
            if (!value) TweenUp();
        }
        
        public void OnGlobalTouchUp()
        {
            TweenUp();
        }
        
        public override void Show()
        {
            if (Visible) return;
            
            Visible = true;
            Interactable = true;
            
            Rect.localPosition = _hiddenPosition;
        }
        
        public override void Hide()
        {
            if (!Visible) return;
            
            Visible = false;
            Interactable = false;

            // Trick to hide an element and not get it to render
            _hiddenPosition = Rect.localPosition;
            transform.position = new Vector3(-999999999, 0, 0);
            
            _down = false;
            if (AllowScale) SetRectScale(1.0f);
            if (AllowGray) SetAllGray(DefaultGray);
        }

        private void Press()
        {
            if (!Interactable)
                return;

            // Usually, we do a little fade to hide the button after clicking it
            // We don't want to call press again while it is tweening
            // A bit more optimized would have been to explicitly set interactable to false
            // but that require testing and writing that code everywhere
            // This will do for now...
            if (HasImage && Image.canvasRenderer.GetInheritedAlpha() < 0.9999f)
                return;
            
            OnClick.Invoke();
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {   
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (_clicked) return;
            
            if (_pressed) OnRelease?.Invoke();
            _pressed = false;
            TweenUp();
            Press();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _position = eventData.position;
            
            _clicked = false;
            _pressed = true;
            OnPress?.Invoke(eventData.pointerId, eventData.position);
            TweenDown();
            
            if (EventSystem.current == null || EventSystem.current.alreadySelecting)
                return;

            EventSystem.current.SetSelectedGameObject(gameObject);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            TweenUp();
            
            if (_pressed && eventData != null && !eventData.dragging && Interactable)
            {
                // If we didn't move the pointer too far from original position
                // This is to fix the issue where because of the scale down tween,
                // the hit area is no longer under the pointer...
                var start = NormalizeToScreen(_position); //UIGlobal.Camera.ScreenToWorldPoint(_position);
                var end = NormalizeToScreen(eventData.position); //UIGlobal.Camera.ScreenToWorldPoint(eventData.position);
                var diff = end - start;
                if (diff.magnitude < 0.01f) // 0.015f
                {
                    _clicked = true;
                    Press();
                }
            }
            
            if (_pressed) OnRelease?.Invoke();
            _pressed = false;
        }

        private Vector2 NormalizeToScreen(Vector2 position)
        {
            return new Vector2(position.x / Screen.width, position.y / Screen.height);
        }
        
        private void TweenDown()
        {
            if (!Interactable) return;
            
            _down = true;
            var wRatio = 1.0f - (1.0f - Scale) / Ratio;
            if (AllowScale) TweenRectScale(new Vector2(wRatio, Scale), In, InEase);
            
            _hasExtraScaleTween = ExtraScale.Length > 0;
            foreach (var rect in ExtraScale)
                rect.TweenScale(new Vector2(wRatio, Scale), In).SetEase(InEase);
            
            if (AllowGray) TweenAllGray(Gray * DefaultGray, In);
            
            _hasExtraColorTween = ExtraColors.Length > 0;
            foreach (var image in ExtraColors)
                image.TweenColor(ExtraColorPressed, In).SetEase(Ease.Linear);
        }

        private void TweenUp()
        {
            if (/*!Interactable ||*/ !_down) return;
            
            _down = false;
            if (AllowScale) TweenRectScale(1.0f, Out, OutEase);
            
            _hasExtraScaleTween = ExtraScale.Length > 0;
            foreach (var rect in ExtraScale)
                rect.TweenScale(1.0f, Out).SetEase(OutEase);
            
            if (AllowGray) TweenAllGray(DefaultGray, Out);
            
            _hasExtraColorTween = ExtraColors.Length > 0;
            foreach (var image in ExtraColors)
                image.TweenColor(ExtraColorNormal, Out).SetEase(Ease.Linear);
        }

        public void TweenUpImmediate()
        {
            //if (/*!Interactable ||*/ !_down) return;
            
            _down = false;
            if (AllowScale)
            {
                Rect.TweenKill();
                Rect.localScale = Vector3.one;
            }
            
            _hasExtraScaleTween = ExtraScale.Length > 0;
            foreach (var rect in ExtraScale)
            {
                rect.TweenKill();
                rect.localScale = Vector3.one;
            }
            
            if (AllowGray) SetAllGray(DefaultGray);
            
            _hasExtraColorTween = ExtraColors.Length > 0;
            foreach (var image in ExtraColors)
            {
                image.TweenKill();
                image.color = ExtraColorNormal;
            }
        }
        
        public void Clear()
        {
            TweenUp();
        }
        
        public void ClearImmediate()
        {
            TweenUpImmediate();
        }
        
        public void SetEnabled(bool enable)
        {
            if (enable) Enable();
            else Disable();
        }
        
        public void Enable()
        {
            IsEnabled = true;
            
            AllowGray = true;
            SetAllGray(1.0f);
        }
        
        public void Disable()
        {
            IsEnabled = false;
            
            AllowGray = false;
            SetAllGray(GrayDisabled);
        }

        public override void TweenAllGray(float value, float duration = DefaultIn)
        {
            base.TweenAllGray(value, duration);
            
            _hasExtraDarkenTween = ExtraDarken.Length > 0;
            foreach (var image in ExtraDarken)
                image.TweenColorNoAlpha(new Color(value, value, value), duration).SetEase(Ease.Linear);
            
            _hasExtraDarkenTextTween = ExtraDarkenText.Length > 0;
            foreach (var text in ExtraDarkenText)
                text.TweenColorNoAlpha(new Color(value, value, value), duration).SetEase(Ease.Linear);
        }
        
        public override void SetAllGray(float value)
        {
            base.SetAllGray(value);

            foreach (var image in ExtraDarken)
            {
                image.TweenKill();
                image.color = image.color.ToGray(value, image.color.a);
            }

            foreach (var text in ExtraDarkenText)
            {
                text.TweenKill();
                text.color = text.color.ToGray(value, text.color.a);
            }
        }
        
        public override void KillTweens()
        {
            base.KillTweens();
            
            if (_hasExtraDarkenTween) foreach (var image in ExtraDarken) image.TweenKill();
            if (_hasExtraDarkenTextTween) foreach (var text in ExtraDarkenText) text.TweenKill();
            if (_hasExtraScaleTween) foreach (var rect in ExtraScale) rect.TweenKill();
            if (_hasExtraColorTween) foreach (var image in ExtraColors) image.TweenKill();
            _hasExtraDarkenTween = false;
            _hasExtraDarkenTextTween = false;
            _hasExtraScaleTween = false;
            _hasExtraColorTween = false;
        }
        
        public override void OnValidate()
        {
            base.OnValidate();
            
            ParentGroup = null;
            HasParentGroup = false;
            
            var minChild = -1;
            var parents = gameObject.GetComponentsInParent<CanvasGroup>(true);
            foreach (var p in parents)
            {
                if (p.gameObject.GetInstanceID() == gameObject.GetInstanceID()) 
                    continue;
                
                var n = 0;
                var child = p.gameObject;
                while (child.transform.parent != null)
                {
                    child = child.transform.parent.gameObject;
                    n++;
                }
                
                if (n > minChild)
                {
                    minChild = n;
                    ParentGroup = p;
                    HasParentGroup = ParentGroup != null;
                }
            }
        }
    }
}