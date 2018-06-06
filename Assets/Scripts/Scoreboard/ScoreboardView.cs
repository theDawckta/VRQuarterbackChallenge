using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ScoreboardView 
{
    void LoadRequiredComponents(GameObject parentComponent);

    void UpdateScoreboardUI(ScoreboardDataModel dataModel);

    void UpdateScoreboardData(string propName, string propValue);

    void ResetScoreboardData();

    void ShowScoreboardUI();

    void HideScoreboardUI();
}
