#define DEBUG_LEVEL_LOG
#define DEBUG_LEVEL_WARN
#define DEBUG_LEVEL_ERROR

using System;
using UnityEngine;
using System.Collections;
using Conditional = System.Diagnostics.ConditionalAttribute;

namespace VOKE.VokeApp.Util
{
    public class Trace
    {
        [Conditional("DEBUG_LEVEL_LOG")]
        public static void Log(string format, params object[] paramList)
        {
            Debug.Log(string.Format(format, paramList));
        }

        [Conditional("DEBUG_LEVEL_WARN")]
        public static void Warn(string format, params object[] paramList)
        {
            Debug.LogWarning(string.Format(format, paramList));
        }

        [Conditional("DEBUG_LEVEL_ERROR")]
        public static void Error(string format, params object[] paramList)
        {
            Debug.LogError(string.Format(format, paramList));
        }
    }
}