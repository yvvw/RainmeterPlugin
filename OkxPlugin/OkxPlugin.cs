using System;
using System.Runtime.InteropServices;

using Rainmeter;
using RMPlugin;

namespace OkxPlugin
{
    public class Plugin
    {
        [DllExport]
        public static void Initialize(ref IntPtr data, IntPtr rm)
        {
            API api = rm;
            RMLog.Api = api;
            RMLog.Debug("Plugin initialize.");
            data = GCHandle.ToIntPtr(GCHandle.Alloc(new Measure(api)));
        }

        [DllExport]
        public static void Finalize(IntPtr data)
        {
            RMLog.Debug("Plugin finalize.");

            ((Measure)data).Dispose();
            GCHandle.FromIntPtr(data).Free();
        }

        [DllExport]
        public static void Reload(IntPtr data, IntPtr rm, ref double maxValue)
        {
            RMLog.Debug("Plugin reload.");
        }

        [DllExport]
        public static double Update(IntPtr data)
        {
            RMLog.Debug("Plugin update.");

            Measure measure = (Measure)data;
            measure.Update();
            return 0.0;
        }

        [DllExport]
        public static IntPtr GetSpotTicker(IntPtr data, int argc,
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr, SizeParamIndex = 1)] string[] argv)
        {
            Measure measure = (Measure)data;
            return measure.GetSpotTicker(argv[0]);
        }

        [DllExport]
        public static IntPtr GetBalance(IntPtr data, int argc,
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr, SizeParamIndex = 1)] string[] argv)
        {
            Measure measure = (Measure)data;
            return measure.GetBalance(argv[0]);
        }

        [DllExport]
        public static IntPtr GetPosition(IntPtr data, int argc,
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr, SizeParamIndex = 1)] string[] argv)
        {
            Measure measure = (Measure)data;
            return measure.GetPosition(argv[0]);
        }
    }
}
