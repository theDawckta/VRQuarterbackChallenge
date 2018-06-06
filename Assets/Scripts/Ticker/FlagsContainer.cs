using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class FlagsContainer : MonoBehaviour {

    [Serializable]
    public struct FlagTarget
    {
        public string Name;
        public Sprite FlagSprite;
    }

    public FlagTarget[] FlagTargets;

    /// <summary>
    /// Sets the Flag Image 
    /// </summary>
    /// <param name="countryStr">Country code to Lookup</param>
    /// <param name="toSet">Image component to Set</param>
    public void SetFlag(string countryStr,Image toSet)
    {
		CanvasGroup flagCanvas = toSet.GetComponent<CanvasGroup>();
		if (flagCanvas != null)
		{
			flagCanvas.alpha = 0.0f;
		}

        foreach (var flagTarget in FlagTargets)
        {
            if (String.Equals(flagTarget.Name, countryStr, StringComparison.OrdinalIgnoreCase))
            {
                if (toSet != null && flagTarget.FlagSprite != null)
                {
                    toSet.sprite = flagTarget.FlagSprite;
                    if (flagCanvas != null)
                    {
                        flagCanvas.alpha = 1.0f;
                    }
                    break;
                }
            }
        }
    }
}
