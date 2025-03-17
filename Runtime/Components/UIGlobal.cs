using System;
using JD.OS;
using JD.Tween;
using UnityEngine;
using UnityEngine.SceneManagement;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;
#endif

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
            
#if ENABLE_INPUT_SYSTEM
            EnhancedTouchSupport.Enable();
#endif
            
#if UNITY_IOS || UNITY_ANDROID
            //Application.targetFrameRate = 60;
#endif
        }

        public void ChangeHandCursor()
        {
            //WindowsCursor.Change(WindowsCursors.Hand);
        }
        
        public void ChangeArrowCursor()
        {
            //WindowsCursor.Change(WindowsCursors.StandardArrow);
        }
        
        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            var mouseReleased = Mouse.current.leftButton.wasReleasedThisFrame;
            var touches = Touch.activeTouches;
            var touchesCount = touches.Count;
#else
            var mouseReleased = Input.GetMouseButtonUp(0);
            var touchesCount = Input.touchCount;
#endif
            
            // Listens for touch up event and broadcast it globally
            // Useful to auto-hide tooltips
            // TODO: Do not use delegate to prevent alloc?
            //var touchedUp = false;
            for (var i = 0; i < touchesCount; i++)
            {
#if ENABLE_INPUT_SYSTEM
                var touch = Touch.activeTouches[i];
#else
                var touch = Input.GetTouch(i);
#endif
                if (touch.phase == TouchPhase.Ended)
                {
                    //touchedUp = true;
                    OnTouchUp?.Invoke();
                    break;
                }
            }
            
            if (OSUtils.IsDesktop && mouseReleased)
            {
                //touchedUp = true;
                OnTouchUp?.Invoke();
            }
        }
        
        private void LateUpdate()
        {
            Tweens.Update();

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
#else
            if (Input.GetKeyUp(KeyCode.Escape))
#endif
                Screen.fullScreen = false;
            
            //WindowsCursor.Update();
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