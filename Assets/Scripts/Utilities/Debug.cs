//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Diagnostics;
//using RenderHeads.Media.AVProVideo;
//using UnityEngine;
//using UnityDebug = UnityEngine.Debug;
//
//internal static class Debug
//{
//    [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
//    public static void Log(object message)
//    {
//        UnityDebug.Log(message);
//    }
//
//    [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
//    public static void Log(object message, UnityEngine.Object context)
//    {
//        UnityDebug.Log(message, context);
//    }
//
//    [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
//    public static void LogWarning(object message)
//    {
//        UnityDebug.LogWarning(message);
//    }
//
//    [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
//    public static void LogWarning(object message, UnityEngine.Object context)
//    {
//        UnityDebug.LogWarning(message, context);
//    }
//
//    [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
//    public static void LogError(object message)
//    {
//        UnityDebug.LogError(message);
//    }
//
//    [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
//    public static void LogError(object message, UnityEngine.Object context)
//    {
//        UnityDebug.LogError(message, context);
//    }
//
//    [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
//    public static void LogErrorFormat(string format, params object[] args)
//    {
//        UnityDebug.LogErrorFormat(format, args);
//    }
//
//    [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
//    public static void LogFormat(string format, params object[] args)
//    {
//        UnityDebug.LogFormat(format, args);
//    }
//
//    [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
//    public static void LogFormat(UnityEngine.Object context, string format, params object[] args)
//    {
//        UnityDebug.LogFormat(context, format, args);
//    }
//
//    [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
//    public static void LogWarningFormat(string format, params object[] args)
//    {
//        UnityDebug.LogWarningFormat(format, args);
//    }
//
//    [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
//    public static void Assert(bool condition)
//    {
//        UnityDebug.Assert(condition);
//    }
//
//    [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
//    public static void DrawLine(Vector3 start, Vector3 end, Color color)
//    {
//        UnityDebug.DrawLine(start, end, color);
//    }
//
//    [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
//    public static void DrawRay(Vector3 start, Vector3 dir, Color color)
//    {
//        UnityDebug.DrawRay(start, dir, color);
//    }
//
//    [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
//    public static void DrawRay(Vector3 start, Vector3 dir, Color color, float duration)
//    {
//        UnityDebug.DrawRay(start, dir, color, duration);
//    }
//
//    [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
//    public static void DrawLine(Vector3 start, Vector3 end)
//    {
//        UnityDebug.DrawLine(start, end);
//    }
//}