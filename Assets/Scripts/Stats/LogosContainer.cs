using System;
using UnityEngine;
using UnityEngine.UI;

public class LogosContainer : MonoBehaviour {

    [Serializable]
    public struct LogoTarget
    {
        public string Name;
        public Sprite LogoSprite;
    }

    public LogoTarget[] LogoTargets;

    /// <summary>
    /// Sets the Logo Image 
    /// </summary>
    /// <param name="countryStr">Logo to lookup</param>
    /// <param name="toSet">Image component to Set</param>
    public void SetLogo(string logoStr, Image toSet)
    {
        CanvasGroup logoCanvas = toSet.GetComponent<CanvasGroup>();
        if (logoCanvas != null)
        {
            logoCanvas.alpha = 0.0f;
        }

        foreach (var logoTarget in LogoTargets)
        {
            if (String.Equals(logoTarget.Name, logoStr, StringComparison.OrdinalIgnoreCase))
            {
                if (toSet != null && logoTarget.LogoSprite != null)
                {
                    toSet.sprite = logoTarget.LogoSprite;
                    if (logoCanvas != null)
                    {
                        logoCanvas.alpha = 1.0f;
                    }
                    break;
                }
            }
        }
    }
}
