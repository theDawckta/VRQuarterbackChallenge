using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HighScoreRowController : MonoBehaviour 
{
    public TextMeshPro Initials;
    public TextMeshPro Score;

    public void SetRowText(string initials, int score)
    {
        Initials.text = initials;
		Score.text = string.Format("{0:n0}", score);;
    }
}
