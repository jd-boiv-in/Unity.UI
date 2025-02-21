using System;
using JD.UI.Utility;
using JD.Tween;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace JD.UI.Components
{
    public class UIGlobal : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatic()
        {
            Instance = null;
            Tweens = null;
            
            _camera = null;
            _hasCamera = false;
            _initialized = false;
        }
        
        // Create this object on the scene
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void OnBeforeSceneLoadRuntimeMethod()
        {
            Load();
        }

        // For some reason `Camera.main` is expensive, not as much as before, but still,
        // technically we have only one per scene, so simply cache the ref in a static var
        public static Camera Camera
        {
            get
            {
                if (_hasCamera) return _camera;

                _hasCamera = true;
                _camera = Camera.main;
                return _camera;
            }
        }
        
        public static UIGlobal Instance;
        public static Tweens Tweens;
        
        private static Camera _camera;
        private static bool _hasCamera;
        
        private static bool _initialized = false;
        
        public Action OnTouchUp;
        
        private static void Load()
        {
            if (_initialized) return;
            _initialized = true;
            
            var obj = new GameObject("UI Global");
            Instance = obj.AddComponent<UIGlobal>();
            Instance.Init();
            
            DontDestroyOnLoad(obj);
        }

        private void Init()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            
            Tweens = Tweens.Load();
            
            EnhancedTouchSupport.Enable();
            
#if UNITY_IOS || UNITY_ANDROID
            //Application.targetFrameRate = 60;
#endif
        }
        
        private void Update()
        {
            // Listens for touch up event and broadcast it globally
            // Useful to auto-hide tooltips
            // TODO: Do not use delegate to prevent alloc?
            var touchedUp = false;
            for (var i = 0; i < Touch.activeTouches.Count; i++)
            {
                var touch = Touch.activeTouches[i];
                if (touch.phase == TouchPhase.Ended)
                {
                    touchedUp = true;
                    OnTouchUp?.Invoke();
                    break;
                }
            }
            
            if (OSUtils.IsDesktop && Mouse.current.leftButton.wasReleasedThisFrame)
            {
                touchedUp = true;
                OnTouchUp?.Invoke();
            }
        }
        
        private void LateUpdate()
        {
            Tweens.Update();

            if (Keyboard.current.escapeKey.wasPressedThisFrame)
                Screen.fullScreen = false;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _camera = Camera.main;
            _hasCamera = true;
        }
        
        private void OnSceneUnloaded(Scene scene)
        {
            Tweens.KillAll();
            
            OnTouchUp = null;
        }
        
        public void OnDestroy()
        {
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}