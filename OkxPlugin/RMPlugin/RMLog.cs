using System;
using System.Runtime.CompilerServices;
using Rainmeter;

namespace RMPlugin
{
    public static class RMLog
    {
        public static API Api { private get; set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Log(API.LogType type, string message) => Api?.Log(type, message);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Debug(string message) => Log(API.LogType.Debug, message);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Notice(string message) => Log(API.LogType.Notice, message);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Warning(string message) => Log(API.LogType.Warning, message);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Error(string message) => Log(API.LogType.Error, message);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Log(API.LogType type, string format, params Object[] args) => Api?.LogF(type, format, args);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DebugF(string format, params Object[] args) => Log(API.LogType.Debug, format, args);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NoticeF(string format, params Object[] args) => Log(API.LogType.Notice, format, args);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WarningF(string format, params Object[] args) => Log(API.LogType.Warning, format, args);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ErrorF(string format, params Object[] args) => Log(API.LogType.Error, format, args);
    }
}
