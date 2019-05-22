using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetController : MonoBehaviour 
{
    public ButtonControllerReplacement ResetButton;
    public HighScoresController HighScores;

    private int _clickNum = 0;

	void Awake()
	{
        ResetButton.Enable();
	}

    void ResetButtonClicked(object sender)
    {
        Debug.Log("Mouse Click " + _clickNum);
        _clickNum = _clickNum + 1;
        if (_clickNum == 5)
        {
            HighScores.GenerateTestScores();
            _clickNum = 5;
        }
    }

    void ResetButtonMouseOut(object sender)
    {
        _clickNum = 0;
        Debug.Log("Mouse Out " + _clickNum);
    }

	void OnEnable()
	{
        ResetButton.OnButtonClicked += ResetButtonClicked;
        ResetButton.OnButtonOut += ResetButtonMouseOut;
	}

    void OnDisable()
    {
        ResetButton.OnButtonClicked -= ResetButtonClicked;
        ResetButton.OnButtonOut -= ResetButtonMouseOut;
    }
}
