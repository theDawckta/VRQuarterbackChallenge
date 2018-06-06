using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


[CreateAssetMenu(fileName = "Config", menuName = "Custom/Runtime Configuration")]
public class RuntimeConfiguration : ScriptableObject
{
    public string AppConfigUrl;
    public string EnvironmentConfigUrl;
	public string OculusAppID;

    private static RuntimeConfiguration _Default;
    private static readonly object _DefaultLock = new object();
    public static RuntimeConfiguration Default
    {
        get
        {
            if (_Default == null)
            {
                lock (_DefaultLock)
                {
                    if (_Default == null)
                    {
                        _Default = Resources.Load<RuntimeConfiguration>("ConfigLive");
                    }
                }
            }

            return _Default;
        }
    }
}
