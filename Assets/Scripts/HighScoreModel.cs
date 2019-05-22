using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InteAdPoc
{
    [Serializable]
    public class HighScoresModel
    {
        public List<PlayerScoreModel> PlayerScores;

        public HighScoresModel(List<PlayerScoreModel> playerScores)
        {
            PlayerScores = playerScores;
        }
    }

    [Serializable]
    public class PlayerScoreModel
    {
        public string PlayerInitials;
        public int PlayerScore;

        public PlayerScoreModel(string playerName, int playerScore)
        {
            PlayerInitials = playerName;
            PlayerScore = playerScore;
        }
    }
}