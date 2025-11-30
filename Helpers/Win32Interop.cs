using Microsoft.UI.Xaml;
using System;
using System.Reflection;

namespace NSSImporter.Helpers
{
    public static class Win32Interop
    {
        public static void InitializeWithWindow(object picker, Window window)
        {
            var wnType = Type.GetType("WinRT.Interop.WindowNative, WinRT.Runtime");
            var getHwnd = wnType?.GetMethod("GetWindowHandle", BindingFlags.Public | BindingFlags.Static);
            var hwndObj = getHwnd?.Invoke(null, new object[] { window });
            var hwnd = hwndObj is IntPtr ip ? ip : IntPtr.Zero;

            var initType = Type.GetType("WinRT.Interop.InitializeWithWindow, WinRT.Runtime");
            var initialize = initType?.GetMethod("Initialize", BindingFlags.Public | BindingFlags.Static);
            initialize?.Invoke(null, new object[] { picker, hwnd });
        }

        public static void InitializeWithWindow(object picker, IntPtr hwnd)
        {
            var initType = Type.GetType("WinRT.Interop.InitializeWithWindow, WinRT.Runtime");
            var initialize = initType?.GetMethod("Initialize", BindingFlags.Public | BindingFlags.Static);
            initialize?.Invoke(null, new object[] { picker, hwnd });
        }
    }
}
