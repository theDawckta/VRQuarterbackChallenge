using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformEnvironment {

    /// <summary>
    /// Returns the dictionary of arguments based on platform
    /// </summary>
    /// <returns>Dictionary of tring and string arguments</returns>
    public static Dictionary<string, string> GetArguments()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        return AndroidActivityHelper.GetArguments();
#else
        return new Dictionary<string, string>();
#endif
    }
}
