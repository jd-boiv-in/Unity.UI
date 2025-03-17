using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JD.Colors;
using JD.Extensions;
using JD.Text;
using JD.Tween;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.Networking;
using Rect = UnityEngine.Rect;

namespace JD.UI.Components
{
    public enum SyncType
    {
        None,
        List,
        InfiniteList,
        Button,
        BackButton
    }
    
    // Always include links to `RectTransform` and the `Canvas`
    // Helper function on Tweens since anyway they mostly happens on `RectTransform` or `Transform`
    // Automatically save common Components
    public class UIBehaviour : MonoBehaviour
    {
        public const float DefaultIn = 0.25f;
        public const float DefaultOut = 0.15f;
        public const Ease DefaultInEase = Ease.OutQuad;
        public const Ease DefaultOutEase = Ease.InQuad;

        [NonSerialized] public float In = DefaultIn;
        [NonSerialized] public float Out = DefaultOut;
        
        [NonSerialized] public Ease InEase = DefaultInEase;
        [NonSerialized] public Ease OutEase = DefaultOutEase;
        
        [HideInInspector] public RectTransform Rect;
        [HideInInspector] public bool HasRect;
        
        [HideInInspector] public Image Image;
        [HideInInspector] public bool HasImage;
        [HideInInspector] public Color ImageColor;
        
        [HideInInspector] public CanvasGroup Group;
        [HideInInspector] public bool HasGroup;
        
        [HideInInspector] public TextMeshUI Text;
        [HideInInspector] public bool HasText;
        
        // Also save all children (useful for tweening buttons)
        [HideInInspector] public Graphic[] Graphics;
        [HideInInspector] public TextMeshUI[] Texts;
        [HideInInspector] public Image[] Images;
        [HideInInspector] public Color[] ImagesColors;

        public GameObject Parent => transform.parent.gameObject;
        
        protected bool _hasGroupTween;
        protected bool _hasRectTween;
        protected bool _hasImageTween;
        protected bool _hasTextTween;
        protected bool _hasImagesTween;
        protected bool _hasGraphicsTween;
        protected bool _hasTextsTween;

        [NonSerialized] public bool Visible = true;
        
        // Default values
        [HideInInspector] public float RectLeft;
        [HideInInspector] public float RectRight;
        [HideInInspector] public float RectTop;
        [HideInInspector] public float RectBottom;
        [HideInInspector] public Vector2 RectSize;
        [HideInInspector] public Rect RectDimension;
        [HideInInspector] public Vector2 RectAnchoredPosition;
        [HideInInspector] public Vector3 RectLocalPosition;
        [HideInInspector] public Vector3 RectLocalScale;
        [HideInInspector] public float Alpha;
        
        public void ResetColors()
        {
            for (var i = 0; i < Images.Length; i++)
            {
                var img = Images[i];
                ImagesColors[i] = img.color;
            }
            
            if (HasImage) ImageColor = Image.color;
        }
        
        public void ResetColor(Image image)
        {
            for (var i = 0; i < Images.Length; i++)
            {
                var img = Images[i];
                if (img != Image) continue;
                
                ImagesColors[i] = image.color;
                break;
            }
            
            if (image == Image) ImageColor = image.color;
        }
        
        public void ResetColor(Image image, Color color)
        {
            for (var i = 0; i < Images.Length; i++)
            {
                var img = Images[i];
                if (img != Image) continue;
                
                ImagesColors[i] = color;
                break;
            }
            
            if (image == Image) ImageColor = color;
        }
        
        // Common transition is to `Show()` or `Hide()` component
        public virtual void Show()
        {
            if (Visible) return;

            Visible = true;
            ShowInternal();
        }
        
        public virtual void Hide()
        {
            if (!Visible) return;
            
            Visible = false;
            HideInternal();
        }

        public virtual void ShowImmediate()
        {
            Visible = true;
        }
        
        public virtual void HideImmediate()
        {
            Visible = false;
        }
        
        protected virtual void ShowInternal()
        {
            // Override me ;)
        }
        
        protected virtual void HideInternal()
        {
            // Override me ;)
        }
        
#region Tweens
        public void TweenGray(float value, float duration = DefaultIn)
        {
            if (!HasImage /*|| !Enabled*/) return;

            _hasImageTween = true;
            Image.TweenColor(ImageColor.ToGray(value, Image.color.a), duration).SetEase(Ease.Linear);
        }

        public virtual void TweenAllGray(float value, float duration = DefaultIn)
        {
            //if (!Enabled) return;

            _hasImagesTween = Images.Length > 0;
            for (var i = 0; i < Images.Length; i++)
            {
                var image = Images[i];
                image.TweenColorNoAlpha(ImagesColors[i].ToGray(value), duration).SetEase(Ease.Linear);
            }
            
            _hasTextsTween = Texts.Length > 0;
            foreach (var text in Texts)
                text.TweenColorNoAlpha(text.DefaultColor.ToGray(value), duration).SetEase(Ease.Linear);

            _hasGraphicsTween = Graphics.Length > 0;
            foreach (var graphic in Graphics)
                graphic.TweenColorNoAlpha(new Color(value, value, value), duration).SetEase(Ease.Linear);
        }
        
        public void TweenRectX(float x, float duration = DefaultIn, Ease ease = DefaultInEase)
        {
            if (!HasRect /*|| !Enabled*/) return;

            _hasRectTween = true;
            Rect.TweenLocalMoveX(x, duration).SetEase(ease);
        }
        
        public void TweenRectY(float y, float duration = DefaultIn, Ease ease = DefaultInEase)
        {
            if (!HasRect /*|| !Enabled*/) return;

            _hasRectTween = true;
            Rect.TweenLocalMoveY(y, duration).SetEase(ease);
        }
        
        public void TweenRectXY(float x, float y, float duration = DefaultIn, Ease ease = DefaultInEase)
        {
            if (!HasRect /*|| !Enabled*/) return;

            _hasRectTween = true;
            Rect.TweenLocalMove(new Vector3(x, y, 0), duration).SetEase(ease);
        }

        public void TweenRectWidth(float width, float duration = DefaultIn, Ease ease = DefaultInEase)
        {
            if (!HasRect /*|| !Enabled*/) return;

            _hasRectTween = true;
            Rect.TweenSizeDelta(new Vector2(width, Rect.sizeDelta.y), duration).SetEase(ease);
        }
        
        public void TweenRectHeight(float height, float duration = DefaultIn, Ease ease = DefaultInEase)
        {
            if (!HasRect /*|| !Enabled*/) return;

            _hasRectTween = true;
            Rect.TweenSizeDelta(new Vector2(Rect.sizeDelta.x, height), duration).SetEase(ease);
        }
        
        public void TweenRectSize(float width, float height, float duration = DefaultIn, Ease ease = DefaultInEase)
        {
            if (!HasRect /*|| !Enabled*/) return;

            _hasRectTween = true;
            Rect.TweenSizeDelta(new Vector2(width, height), duration).SetEase(ease);
        }
        
        public void TweenRectSize(Vector2 size, float duration = DefaultIn, Ease ease = DefaultInEase)
        {
            if (!HasRect /*|| !Enabled*/) return;

            _hasRectTween = true;
            Rect.TweenSizeDelta(size, duration).SetEase(ease);
        }
        
        public void TweenRectLeft(float left, float duration = DefaultIn, Ease ease = DefaultInEase)
        {
            if (!HasRect /*|| !Enabled*/) return;

            _hasRectTween = true;
            Rect.TweenLeft(left, duration).SetEase(ease);
        }
        
        public void TweenRectRight(float right, float duration = DefaultIn, Ease ease = DefaultInEase)
        {
            if (!HasRect /*|| !Enabled*/) return;

            _hasRectTween = true;
            Rect.TweenRight(right, duration).SetEase(ease);
        }
        
        public void TweenRectTop(float top, float duration = DefaultIn, Ease ease = DefaultInEase)
        {
            if (!HasRect /*|| !Enabled*/) return;

            _hasRectTween = true;
            Rect.TweenTop(top, duration).SetEase(ease);
        }
        
        public void TweenRectBottom(float bottom, float duration = DefaultIn, Ease ease = DefaultInEase)
        {
            if (!HasRect /*|| !Enabled*/) return;

            _hasRectTween = true;
            Rect.TweenBottom(bottom, duration).SetEase(ease);
        }
        
        public void TweenRectScaleX(float scale, float duration = DefaultIn, Ease ease = DefaultInEase, bool relative = true)
        {
            if (!HasRect /*|| !Enabled*/) return;

            _hasRectTween = true;
            Rect.TweenScaleX(relative ? RectLocalScale.x * scale : scale, duration).SetEase(ease);
        }
        
        public void TweenRectScaleY(float scale, float duration = DefaultIn, Ease ease = DefaultInEase, bool relative = true)
        {
            if (!HasRect /*|| !Enabled*/) return;

            _hasRectTween = true;
            Rect.TweenScaleY(relative ? RectLocalScale.y * scale : scale, duration).SetEase(ease);
        }
        
        public void TweenRectScale(float scale, float duration = DefaultIn, Ease ease = DefaultInEase, bool relative = true)
        {
            if (!HasRect /*|| !Enabled*/) return;

            _hasRectTween = true;
            var value = relative ? Mathf.Abs(RectLocalScale.x) * scale : scale;
            Rect.TweenScale(new Vector3((RectLocalScale.x < 0 ? -1 : 1) * value, (RectLocalScale.y < 0 ? -1 : 1) * value, 1f), duration).SetEase(ease);
        }
        
        public void TweenRectScale(Vector2 scale, float duration = DefaultIn, Ease ease = DefaultInEase, bool relative = true)
        {
            if (!HasRect /*|| !Enabled*/) return;

            _hasRectTween = true;
            Rect.TweenScale(new Vector3(relative ? RectLocalScale.x * scale.x : scale.x, relative ? RectLocalScale.y * scale.y : scale.y, 1f), duration, ease, false);
        }

        public virtual void FadeAllIn(float duration = DefaultIn)
        {
            //if (!Enabled) return;

            if (HasGroup)
            {
                FadeIn(duration);
                return;
            }
            
            _hasImagesTween = Images.Length > 0;
            foreach (var image in Images)
                image.TweenFade(1f, duration).SetEase(Ease.Linear);
            
            _hasTextsTween = Texts.Length > 0;
            foreach (var text in Texts)
                text.TweenFade(1f, duration).SetEase(Ease.Linear);
            
            _hasGraphicsTween = Graphics.Length > 0;
            foreach (var graphic in Graphics)
                graphic.TweenFade(1f, duration).SetEase(Ease.Linear);
        }
        
        public virtual void FadeAllOut(float duration = DefaultOut)
        {
            //if (!Enabled) return;

            if (HasGroup)
            {
                FadeOut(duration);
                return;
            }
            
            _hasImagesTween = Images.Length > 0;
            foreach (var image in Images)
                image.TweenFade(0f, duration).SetEase(Ease.Linear);
            
            _hasTextsTween = Texts.Length > 0;
            foreach (var text in Texts)
                text.TweenFade(0f, duration).SetEase(Ease.Linear);
            
            _hasGraphicsTween = Graphics.Length > 0;
            foreach (var graphic in Graphics)
                graphic.TweenFade(0f, duration).SetEase(Ease.Linear);
        }

        public void FadeIn(float duration = DefaultIn, float delay = 0f, bool oneFrameDelay = false)
        {
            //if (!Enabled) return;

            if (HasGroup)
            {
                _hasGroupTween = true;
                Group.TweenFade(Alpha, duration).SetDelay(delay).SetEase(Ease.Linear).AddOneFrameDelay(oneFrameDelay);
                Group.interactable = true;
            }
            else if (HasImage)
            {
                _hasImageTween = true;
                Image.TweenFade(Alpha, duration).SetDelay(delay).SetEase(Ease.Linear).AddOneFrameDelay(oneFrameDelay);
            }
            else if (HasText)
            {
                _hasTextTween = true;
                Text.TweenFade(Alpha, duration).SetDelay(delay).SetEase(Ease.Linear).AddOneFrameDelay(oneFrameDelay);
            }
        }
        
        public void FadeOut(float duration = DefaultOut, bool oneFrameDelay = false)
        {
            //if (!Enabled) return;

            if (HasGroup)
            {
                _hasGroupTween = true;
                Group.TweenFade(0f, duration).SetEase(Ease.Linear).AddOneFrameDelay(oneFrameDelay);
                Group.interactable = false;
            }
            else if (HasImage)
            {
                _hasImageTween = true;
                Image.TweenFade(0f, duration).SetEase(Ease.Linear).AddOneFrameDelay(oneFrameDelay);
            }
            else if (HasText)
            {
                _hasTextTween = true;
                Text.TweenFade(0f, duration).SetEase(Ease.Linear).AddOneFrameDelay(oneFrameDelay);
            }
        }

        public void SetGray(float value)
        {
            if (!HasImage /*|| !Enabled*/) return;
            
            Image.color = ImageColor.ToGray(value, Image.color.a);
        }
        
        public virtual void SetAllGray(float value)
        {
            //if (!Enabled) return;

            for (var i = 0; i < Images.Length; i++)
            {
                var image = Images[i];
                image.TweenKill();
                image.color = ImagesColors[i].ToGray(value, image.color.a);
            }

            foreach (var text in Texts)
            {
                text.TweenKill();
                text.color = text.DefaultColor.ToGray(value, text.color.a);
            }

            foreach (var graphic in Graphics)
            {
                graphic.TweenKill();
                graphic.color = new Color(value, value, value, graphic.color.a);
            }
        }
        
        public virtual void SetAlpha(float alpha = 0f)
        {
            //if (!Enabled) return;

            if (HasGroup)
            {
                Group.alpha = alpha;
                Group.interactable = alpha > 0;
            }
            else if (HasImage)
            {
                Image.color = Image.color.ToAlpha(alpha);
            }
            else if (HasText)
            {
                Text.color = Text.color.ToAlpha(alpha);
            }
        }

        public void SetAllAlpha(float value)
        {
            //if (!Enabled) return;
            
            foreach (var image in Images)
                image.color = image.color.ToAlpha(value);
            
            foreach (var text in Texts)
                text.color = text.color.ToAlpha(value);
            
            foreach (var graphic in Graphics)
                graphic.color = graphic.color.ToAlpha(value);
        }
        
        public void SetRectScale(float scale, bool relative = true)
        {
            if (!HasRect /*|| !Enabled*/) return;

            var value = relative ? Mathf.Abs(RectLocalScale.x) * scale : scale;
            Rect.localScale = new Vector3((RectLocalScale.x < 0 ? -1 : 1) * value, (RectLocalScale.y < 0 ? -1 : 1) * value, 1f);
        }
        
        public void SetRectScale(Vector2 scale, bool relative = true)
        {
            if (!HasRect /*|| !Enabled*/) return;

            Rect.localScale = new Vector3(relative ? RectLocalScale.x * scale.x : scale.x, relative ? RectLocalScale.y * scale.y : scale.y, 1f);
        }

        public void SetRectSize(float width, float height)
        {
            if (!HasRect /*|| !Enabled*/) return;

            Rect.sizeDelta = new Vector2(width, height);
        }
        
        public void SetRectWidth(float width)
        {
            if (!HasRect /*|| !Enabled*/) return;
            
            Rect.sizeDelta = new Vector2(width, Rect.sizeDelta.y);
        }
        
        public void SetRectHeight(float height)
        {
            if (!HasRect /*|| !Enabled*/) return;
            
            Rect.sizeDelta = new Vector2(Rect.sizeDelta.x, height);
        }
        
        public void SetRectLeft(float left)
        {
            if (!HasRect /*|| !Enabled*/) return;
            
            Rect.SetLeft(left);
        }
        
        public void SetRectRight(float right)
        {
            if (!HasRect /*|| !Enabled*/) return;
            
            Rect.SetRight(right);
        }
        
        public void SetRectTop(float top)
        {
            if (!HasRect /*|| !Enabled*/) return;
            
            Rect.SetTop(top);
        }
        
        public void SetRectBottom(float bottom)
        {
            if (!HasRect /*|| !Enabled*/) return;
            
            Rect.SetBottom(bottom);
        }
        
        public virtual void KillTweens()
        {
            if (_hasGroupTween) Group.TweenKill();
            if (_hasRectTween) Rect.TweenKill();
            if (_hasImageTween) Image.TweenKill();
            if (_hasTextTween) Text.TweenKill();

            if (_hasImagesTween) foreach (var image in Images) image.TweenKill();
            if (_hasGraphicsTween) foreach (var graphic in Graphics) graphic.TweenKill();
            if (_hasTextsTween) foreach (var text in Texts) text.TweenKill();
            
            _hasGroupTween = false;
            _hasRectTween = false;
            _hasImageTween = false;
            _hasTextTween = false;
            _hasImagesTween = false;
            _hasTextsTween = false;
        }
#endregion
        
        public virtual void OnDisable()
        {
            KillTweens();
        }

        public void RefreshTexts()
        {
            Texts = gameObject.GetComponentsInChildren<TextMeshUI>();
            
            if (HasText && !Texts.Contains(Text))
            {
                Array.Resize(ref Texts, Texts.Length + 1);
                Texts[Texts.Length - 1] = Text;
            }
        }

        public void ResetComponents()
        {
            Texts = gameObject.GetComponentsInChildren<TextMeshUI>(true);
            Images = gameObject.GetComponentsInChildren<Image>(true);
            
            ImagesColors = new Color[Images.Length];
            for (var i = 0; i < Images.Length; i++)
            {
                var image = Images[i];
                ImagesColors[i] = image.color;
            }
        }
        
        public virtual void OnValidate()
        {
            Rect = gameObject.GetComponent<RectTransform>();
            HasRect = Rect != null;
            
            Group = gameObject.GetComponent<CanvasGroup>();
            HasGroup = Group != null;
            
            Image = gameObject.GetComponent<Image>();
            HasImage = Image != null;
            
            Text = gameObject.GetComponent<TextMeshUI>();
            HasText = Text != null;

            if (!HasText) // Usually, it is on a child
            {
                Text = gameObject.GetComponentInChildren<TextMeshUI>(true);
                HasText = Text != null;
            }

            if (!HasImage)
            {
                Image = gameObject.GetComponentInChildren<Image>(true);
                HasImage = Image != null;
            }
            
            // Get all children as well
            Texts = gameObject.GetComponentsInChildren<TextMeshUI>(true);
            Images = gameObject.GetComponentsInChildren<Image>(true);

            // Getting graphics is a little bit trickier as we need to specify the root class
            //var list = new List<Graphic>();
            //foreach (var obj in gameObject.GetComponentsInChildren<SkeletonGraphic>(true))
            //    list.Add(obj);
            //Graphics = list.ToArray();
            
            // Makes sure to include non children component as well
            if (HasText && !Texts.Contains(Text))
            {
                Array.Resize(ref Texts, Texts.Length + 1);
                Texts[Texts.Length - 1] = Text;
            }
            
            if (HasImage && !Images.Contains(Image))
            {
                Array.Resize(ref Images, Images.Length + 1);
                Images[Images.Length - 1] = Image;
            }

            if (HasImage) ImageColor = Image.color;
            
            ImagesColors = new Color[Images.Length];
            for (var i = 0; i < Images.Length; i++)
            {
                var image = Images[i];
                ImagesColors[i] = image.color;
            }
            
            // Default values
            if (HasRect)
            {
                RectDimension = Rect.rect;
                RectSize = Rect.sizeDelta;
                RectLocalPosition = Rect.localPosition;
                RectLocalScale = Rect.localScale;
                RectAnchoredPosition = Rect.anchoredPosition;

                RectLeft = Rect.offsetMin.x;
                RectRight = -Rect.offsetMax.x;
                RectBottom = Rect.offsetMin.y;
                RectTop = -Rect.offsetMax.y;
            }
            
            if (HasGroup)
                Alpha = Group.alpha;
            else if (HasImage)
                Alpha = Image.color.a;
            else if (HasText)
                Alpha = Text.color.a;
        }
    }
}