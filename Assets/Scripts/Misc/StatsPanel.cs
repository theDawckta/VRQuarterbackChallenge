using System;
using System.Collections;
using UnityEngine;

public class StatsPanel : MonoBehaviour
{
    public DynamicTexture TextureUI;

    public void SetTexture(string mediaURL)
    {
        if (TextureUI != null)
        {
            TextureUI.SetTexture(mediaURL);
        }
    }
}
