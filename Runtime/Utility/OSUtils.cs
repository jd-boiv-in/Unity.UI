using System;
using System.Globalization;
using UnityEngine;

namespace JD.UI.Utility
{
    public enum DeviceType
    {
        Tablet,
        Phone,
        Other
    }
    
    public static class OSUtils
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatic()
        {
            ForceDesktop = false;
            _checkDesktop = false;
            _isDesktop = false;

            _isTablet = false;
            _isTabletCheck = false;
        }

        public static bool ForceDesktop;
        private static bool _checkDesktop;
        private static bool _isDesktop;
      
        private static bool _isTablet;
        private static bool _isTabletCheck;
        
        public static bool IsWebGLMobile = false;
        public static bool IsWebGLIPad = false;
        public static bool IsWebGLIPhone = false;
        public static bool IsWebGLLandscape = false;
        public static int WebGLWidth = -1;
        public static int WebGLHeight = -1;
        
        public static bool IsArmV7()
        {
            if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(SystemInfo.processorType, "ARM",
                    CompareOptions.IgnoreCase) >= 0)
                return !Environment.Is64BitProcess;

            return false;
        }

        public static Rect GetSafeArea()
        {
            return Screen.safeArea;
        }

        public static bool IsDesktop
        {
            get
            {
#if (UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX) && !UNITY_EDITOR
                return true;
#endif

                if (!_checkDesktop)
                {
                    _checkDesktop = true;
                    _isDesktop = UnityEngine.Device.SystemInfo.deviceType == UnityEngine.DeviceType.Desktop;
                }

                return _isDesktop;
            }
        }

        public static bool IsTablet
        {
            get
            {
                if (_isTabletCheck) return _isTablet;

                _isTablet = GetDeviceType() == DeviceType.Tablet;
                _isTabletCheck = true;

                return _isTablet;
            }
        }
        
        private static DeviceType GetDeviceType()
        {
#if UNITY_EDITOR
            // TODO: Fucking Unity saying that they support `SystemInfo.deviceModel` or whatever but that's not true... At least this should trick to detect iPad in editor...
            var aspectRatio = Mathf.Max(Screen.width, Screen.height) / (float) Mathf.Min(Screen.width, Screen.height);
            var isTablet = (DeviceDiagonalSizeInInches() > 6.5f && aspectRatio < 2f);
            
            if (isTablet)
            {
                return DeviceType.Tablet;
            }
            else
            {
                return DeviceType.Phone;
            }
#elif UNITY_IOS
            var deviceIsIpad = UnityEngine.iOS.Device.generation.ToString().ToLowerInvariant().Contains("ipad");
            if (deviceIsIpad)
            {
                return DeviceType.Tablet;
            }

            var deviceIsIphone = UnityEngine.iOS.Device.generation.ToString().ToLowerInvariant().Contains("iphone");
            if (deviceIsIphone)
            {
                return DeviceType.Phone;
            }
            
#elif UNITY_ANDROID
            var aspectRatio = Mathf.Max(Screen.width, Screen.height) / (float) Mathf.Min(Screen.width, Screen.height);
            var isTablet = (DeviceDiagonalSizeInInches() > 6.5f && aspectRatio < 2f);
     
            if (isTablet)
            {
                return DeviceType.Tablet;
            }
            else
            {
                return DeviceType.Phone;
            }
            
#elif UNITY_WEBGL
            // Mimic check from above
            if (!IsWebGLMobile) return DeviceType.Other;

            if (IsWebGLIPad)
            {
                return DeviceType.Tablet;
            }

            if (IsWebGLIPhone)
            {
                return DeviceType.Phone;
            }
            
            var aspectRatio = Mathf.Max(WebGLWidth, WebGLHeight) / (float) Mathf.Min(WebGLWidth, WebGLHeight);
            var isTablet = (DeviceDiagonalSizeInInchesWebGL() > 6.5f && aspectRatio < 2f);
            
            if (isTablet)
            {
                return DeviceType.Tablet;
            }
            else
            {
                return DeviceType.Phone;
            }
#endif
            
            return DeviceType.Other;
        }
        
        private static float DeviceDiagonalSizeInInches()
        {
            var dpi = Screen.dpi;
            if (dpi <= 0) dpi = 1;
            
            var screenWidth = Screen.width / dpi;
            var screenHeight = Screen.height / dpi;
            var diagonalInches = Mathf.Sqrt(Mathf.Pow(screenWidth, 2) + Mathf.Pow(screenHeight, 2));
 
            return diagonalInches;
        }
        
        private static float DeviceDiagonalSizeInInchesWebGL()
        {
            var dpi = Screen.dpi;
            if (dpi <= 0) dpi = 1;
            
            var screenWidth = Screen.width / dpi;
            var screenHeight = Screen.height / dpi;
            
            // TODO: Need to be captured from html
            //var screenWidth = WebGLWidth / dpi;
            //var screenHeight = WebGLHeight / dpi;
            
            var diagonalInches = Mathf.Sqrt(Mathf.Pow(screenWidth, 2) + Mathf.Pow(screenHeight, 2));
 
            return diagonalInches;
        }
        
        // Application.isMobilePlatform will report mobile even on web
        public static readonly bool IsWeb =
#if UNITY_WEBGL
            true;
#else
            false;
#endif

        public static bool IsMobile =>
#if UNITY_EDITOR
            // The Device Simulator package is broken, some shim doesn't work...
            //SimulatorFix.IsIOS || SimulatorFix.IsAndroid;
            !ForceDesktop;
#elif UNITY_IOS || UNITY_ANDROID
            true;
#else
            false;
#endif

        public static void CollectGC()
        {
#if !UNITY_WEBGL
            // Collect GC
#if !UNITY_2020_1_OR_NEWER
            var isDisabled = GarbageCollector.GCMode == GarbageCollector.Mode.Disabled;
            if (isDisabled) GarbageCollector.GCMode = GarbageCollector.Mode.Enabled;
#endif

            GC.Collect();

#if !UNITY_2020_1_OR_NEWER
            if (isDisabled) GarbageCollector.GCMode = GarbageCollector.Mode.Disabled;
#endif
#endif
        }
    }
}