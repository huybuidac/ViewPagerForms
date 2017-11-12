using System;
using System.Runtime.CompilerServices;

namespace ViewPagerForms.Forms
{
    public static class ObjectExtensions
    {
        public static void Log(this object obj, string content = "", [CallerMemberName] string methodName = "")
        {
            System.Diagnostics.Debug.WriteLine($"[{DateTime.Now.ToString("hh:mm:ss.fff")}]-[{obj?.GetType().Name}]-[{methodName}]\t{content}");
        }
    }
}
