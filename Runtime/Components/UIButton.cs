using System;
using System.Linq;
using JD.Colors;
using JD.Extensions;
using JD.Tween;
using TMPro;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace JD.UI.Components
{
    public class UIButton : UIBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public new const float In = 0.10f;
        public new const float Out = 0.075f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatic()
        {
            Holding = false;
        }
        
        public static bool Holding = false;
        
        [FoldoutGroup("Transitions")]
        public float SelectScale = 1.075f;
        
        [FoldoutGroup("Transitions")]
        public float DownScale = 0.90f;
        
        [FoldoutGroup("Transitions")]
        public float DownGray = 0.75f;
        [HideInInspector] public float Ratio = 1.0f;

        public const float GrayDisabled = 0.5f;
        
        [HideInInspector] public bool Interactable = true;

        [FoldoutGroup("Transitions")]
        public float DefaultGray = 1.0f;
        
        [FoldoutGroup("Transitions")]
        public bool AllowScale = true;
        
        [FoldoutGroup("Transitions"), PropertySpace(0, 8)]
        public bool AllowGray = true;
        
        [NonSerialized] public bool IsEnabled;

        [FoldoutGroup("Images", true)] 
        public Image MainImage; 
        [HideInInspector] public bool HasMainImage;
        
        [FoldoutGroup("Images")]
        public Image DownImage;
        [HideInInspector] public bool HasDownImage;
        
        [FoldoutGroup("Images")]
        public Image SelectImage;
        [HideInInspector] public bool HasSelectImage;
        
        [FoldoutGroup("Images"), PreviewField(ObjectFieldAlignment.Left)]
        public Sprite Normal;
        public bool HasNormalSprite;
        
        [FoldoutGroup("Images"), PreviewField(ObjectFieldAlignment.Left)]
        public Sprite Down;
        public bool HasDownSprite;
        
        [FoldoutGroup("Images"), PreviewField(ObjectFieldAlignment.Left)]
        public Sprite Select;
        public bool HasSelectSprite;
        
        [PropertySpace(8, 8)]
        public Button.ButtonClickedEvent OnClick = new();

        public Action<int, Vector2> OnPress;
        public Action OnRelease;
        
        [FoldoutGroup("Extra")]
        public Image[] ExtraDarken;
        private bool _hasExtraDarkenTween;
        
        [FoldoutGroup("Extra")]
        public TextMeshProUGUI[] ExtraDarkenText;
        private bool _hasExtraDarkenTextTween;
        
        [FoldoutGroup("Extra")]
        public RectTransform[] ExtraScale;
        private bool _hasExtraScaleTween;

        [HideInInspector] public Color ExtraColorNormal = new Color(0, 0, 0, 0);
        [HideInInspector] public Color ExtraColorPressed = new Color(0, 0, 0, 0.2f);
        [HideInInspector] public Image[] ExtraColors;
        [HideInInspector] private bool _hasExtraColorTween;
        
        [HideInInspector] public CanvasGroup ParentGroup;
        [HideInInspector] public bool HasParentGroup;

        private bool _inside;
        
        private bool _pressed;
        private bool _down;
        public bool IsDown => _down;

        private bool _clicked;
        private Vector2 _position;
        
        private Vector3 _hiddenPosition;

        public void OnEnable()
        {
/*#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                if (_tempImage == null) // || _tempImage.transform.parent != transform)
                    EditorApplication.delayCall += CreateTempImage;
                return;
            }
#endif*/
            _down = false;
            if (AllowScale) SetRectScale(1.0f);
            if (AllowGray) SetAllGray(DefaultGray);

            // TODO: Can this have a big impact CPU wise? Meh only on touch up + even if it is 100 buttons all will just do a `if` so should be minimal...
            UIGlobal.Instance.OnTouchUp += OnGlobalTouchUp;
        }

        // This was tricky, even if I didn't used it, keeping it, maybe I'll need it
/*#if UNITY_EDITOR
        private void CreateImage()
        {
            
        }
        
        private void CreateTempImage()
        {
            Debug.Log($"Created Objects");
            _tempImage = Instantiate(Image, transform);
            _tempImage.gameObject.hideFlags = HideFlags.HideInHierarchy;

            // Hide flags not being saved in prefab, but this works?
            var serializedObject = new SerializedObject(_tempImage.gameObject);
            var hideFlagsProp = serializedObject.FindProperty("m_ObjectHideFlags");
            if (hideFlagsProp != null)
            {
                hideFlagsProp.intValue = (int) HideFlags.HideInHierarchy;
                serializedObject.ApplyModifiedProperties();
            }
            
            _tempImage.name = "Temp Image";
            EditorUtility.SetDirty(gameObject);
            
            // If we're in prefab mode, we need to save the prefab also
            if (PrefabUtility.IsPartOfAnyPrefab(gameObject))
                EditorUtility.SetDirty(PrefabUtility.GetPrefabInstanceHandle(gameObject));
            
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null && prefabStage.IsPartOfPrefabContents(gameObject))
                EditorUtility.SetDirty(prefabStage.prefabContentsRoot);
        }
#endif*/
        
        public override void OnDisable()
        {
/*#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif*/
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
            if (!_down && _inside) TweenHover(true);
            else TweenUp();
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

            if (_down)
            {
                _down = false;
                Holding = false;
            }
            
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
        
        public void OnPointerEnter(PointerEventData eventData)
        {
#if UNITY_IOS || UNITY_ANDROID
            return;
#endif
            if (_inside) return;
            _inside = true;
            
            if (!_down && !Holding) TweenHover(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
#if UNITY_IOS || UNITY_ANDROID
            return;
#endif
            if (!_inside) return;
            _inside = false;
            
            if (!_down && !Holding) TweenUp(true);
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
                var start = NormalizeToScreen(_position);
                var end = NormalizeToScreen(eventData.position);
                var diff = end - start;
                if (diff.sqrMagnitude < 0.0001f)
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
        
        private void TweenHover(bool force = false, bool easeOut = true)
        {
            if (!Interactable || (!force && !_down)) return;

            UIGlobal.Instance.ChangeHandCursor();
            
            var duration = easeOut ? Out : In;
            var ease = easeOut ? OutEase : InEase;
            
            Holding = false;
            _down = false;
            var wRatio = 1.0f - (1.0f - SelectScale) / Ratio;
            if (AllowScale) TweenRectScale(new Vector2(wRatio, SelectScale), duration, ease);
            
            _hasExtraScaleTween = ExtraScale.Length > 0;
            foreach (var rect in ExtraScale)
                rect.TweenScale(new Vector2(wRatio, SelectScale), duration).SetEase(ease);
            
            if (AllowGray) TweenAllGray(DefaultGray, duration);
            
            _hasExtraColorTween = ExtraColors.Length > 0;
            foreach (var image in ExtraColors)
                image.TweenColor(ExtraColorNormal, duration).SetEase(Ease.Linear);

            if (HasSelectImage && HasSelectSprite) SelectImage.TweenFade(1f, duration);
            if (HasDownImage && HasDownSprite) DownImage.TweenFade(0f, duration);
        }
        
        private void TweenDown()
        {
            if (!Interactable) return;
            
            UIGlobal.Instance.ChangeHandCursor();

            Holding = true;
            _down = true;
            var wRatio = 1.0f - (1.0f - DownScale) / Ratio;
            if (AllowScale) TweenRectScale(new Vector2(wRatio, DownScale), In, InEase);
            
            _hasExtraScaleTween = ExtraScale.Length > 0;
            foreach (var rect in ExtraScale)
                rect.TweenScale(new Vector2(wRatio, DownScale), In).SetEase(InEase);
            
            if (AllowGray) TweenAllGray(DownGray * DefaultGray, In);
            
            _hasExtraColorTween = ExtraColors.Length > 0;
            foreach (var image in ExtraColors)
                image.TweenColor(ExtraColorPressed, In).SetEase(Ease.Linear);
            
            if (HasSelectImage && HasSelectSprite) SelectImage.TweenFade(0f, In);
            if (HasDownImage && HasDownSprite) DownImage.TweenFade(1f, In);
        }

        private void TweenUp(bool force = false)
        {
            if (/*!Interactable ||*/ (!force && !_down)) return;

            if (_inside)
            {
                TweenHover(force);
                return;
            }

            UIGlobal.Instance.ChangeArrowCursor();
            
            Holding = false;
            _down = false;
            if (AllowScale) TweenRectScale(1.0f, Out, OutEase);
            
            _hasExtraScaleTween = ExtraScale.Length > 0;
            foreach (var rect in ExtraScale)
                rect.TweenScale(1.0f, Out).SetEase(OutEase);
            
            if (AllowGray) TweenAllGray(DefaultGray, Out);
            
            _hasExtraColorTween = ExtraColors.Length > 0;
            foreach (var image in ExtraColors)
                image.TweenColor(ExtraColorNormal, Out).SetEase(Ease.Linear);
            
            if (HasSelectImage && HasSelectSprite) SelectImage.TweenFade(0f, Out);
            if (HasDownImage && HasDownSprite) DownImage.TweenFade(0f, Out);
        }

        public void TweenUpImmediate()
        {
            //if (/*!Interactable ||*/ !_down) return;
            
            Holding = false;
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
            
            if (HasDownImage) DownImage.TweenKill();
            if (HasSelectImage) SelectImage.TweenKill();
            if (_hasExtraDarkenTween) foreach (var image in ExtraDarken) image.TweenKill();
            if (_hasExtraDarkenTextTween) foreach (var text in ExtraDarkenText) text.TweenKill();
            if (_hasExtraScaleTween) foreach (var rect in ExtraScale) rect.TweenKill();
            if (_hasExtraColorTween) foreach (var image in ExtraColors) image.TweenKill();
            _hasExtraDarkenTween = false;
            _hasExtraDarkenTextTween = false;
            _hasExtraScaleTween = false;
            _hasExtraColorTween = false;
        }
        
#if UNITY_EDITOR
        public override void OnValidate()
        {
            base.OnValidate();

            HasMainImage = MainImage != null;
            HasDownImage = DownImage != null;
            HasSelectImage = SelectImage != null;
            HasNormalSprite = Normal != null;
            HasDownSprite = Down != null;
            HasSelectSprite = Select != null;

            if (HasDownImage) DownImage.SetAlpha(0f);
            if (HasSelectImage) SelectImage.SetAlpha(0f);
            
            // Remove from images array to skip tweens since the transition is done by swapping sprite instead
            if ((HasMainImage && HasDownImage && HasDownSprite) || HasSelectImage)
            {
                var mainId = HasMainImage ? MainImage.GetInstanceID() : 0;
                var downId = HasDownImage ? DownImage.GetInstanceID() : 0;
                var selectId = HasSelectImage ? SelectImage.GetInstanceID() : 0;
                var count = Images.Length;
                Images = Images.Where(i =>
                {
                    var id = i.GetInstanceID();
                    return ((!HasDownImage || !HasDownSprite) || (id != mainId && id != downId)) && (id != selectId);
                }).ToArray();
                //if (count != Images.Length) EditorUtility.SetDirty(gameObject);
            }
            
            // Set proper sprites
            if (HasMainImage && HasNormalSprite && MainImage.sprite != Normal)
            {
                MainImage.sprite = Normal;
                EditorUtility.SetDirty(MainImage);
            }

            if (HasDownImage && HasDownSprite && DownImage.sprite != Down)
            {
                DownImage.sprite = Down;
                EditorUtility.SetDirty(DownImage);
            }
                
            if (HasSelectImage && HasSelectSprite && SelectImage.sprite != Select)
            {
                SelectImage.sprite = Select;
                EditorUtility.SetDirty(SelectImage);
            }
            
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
#endif
    }
}