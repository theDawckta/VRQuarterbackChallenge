using System;
using System.Collections;
using UnityEngine;

public class StatsPanelContainer : MonoBehaviour
{
    public StatsPanel StatsPrefab;

    public void Init(string mediaURL)
    {
        if (StatsPrefab != null)
        {
            StatsPanel statsPanel = Instantiate(StatsPrefab) as StatsPanel;
            statsPanel.transform.SetParent(gameObject.transform, false);
            statsPanel.SetTexture(mediaURL);
        }
    }
}
