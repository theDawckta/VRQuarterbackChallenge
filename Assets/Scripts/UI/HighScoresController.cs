using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InteAdPoc;
using CurvedVRKeyboard;
using TMPro;
using DG.Tweening;

public class HighScoresController : MonoBehaviour 
{
    public delegate void OnHighScoreEntryDoneEvent();
    public event OnHighScoreEntryDoneEvent OnHighScoreEntryDone;
    public delegate void OnHighScoreNotAchievedEvent();
    public event OnHighScoreNotAchievedEvent OnHighScoreNotAchieved;

    public List<HighScoreRowController> ScoreRows;
    public KeyboardStatus Keyboard;
	public GameObject KeyBack;
	public ThreeDButtonController DoneButton;
	public CanvasGroup HighScoresHolderCanvasGroup;
	public List<TextMeshPro> HighScoresText = new List<TextMeshPro>();
	public float HighScoreHolderYDelta = 4.6f;
    
	private TextMeshPro[] _keyboardText;
	private List<BoxCollider> _keyboardColliders = new List<BoxCollider>();
	private Renderer _keyBackRenderer;
    private HighScoresModel _highScores;
    private int _newHighScoreIndex = -1;
	private Vector3 _highScoresHolderOriginalPosition;

	void Awake() 
    {
        Keyboard.gameObject.GetComponent<KeyboardRaycaster>().SetRaycastingTransform(Camera.main.transform);
        if(PlayerPrefs.GetInt("firstrun") == 0)
        {
            GenerateTestScores();
            PlayerPrefs.SetInt("firstrun", 1);
		}

		_highScoresHolderOriginalPosition = HighScoresHolderCanvasGroup.transform.localPosition;
		_keyboardText = Keyboard.gameObject.GetComponentsInChildren<TextMeshPro>();
		_keyBackRenderer = KeyBack.gameObject.GetComponent<Renderer>();  
		_keyBackRenderer.sharedMaterial.DOFade(0.0f, 0.0f);
		HighScoresHolderCanvasGroup.alpha = 0;   

		for (int i = 0; i < HighScoresText.Count; i++)
        {
            HighScoresText[i].DOFade(0.0f, 0.0f);
        }  

		for (int j = 0; j < _keyboardText.Length; j++)
		{
			_keyboardColliders.Add(_keyboardText[j].transform.parent.GetComponent<BoxCollider>());
		}

		for (int k = 0; k < _keyboardColliders.Count; k++)
        {
			_keyboardColliders[k].enabled = false;
        }

		for (int l = 0; l < _keyboardText.Length; l++)
        {
            _keyboardText[l].DOFade(0.0f, 0.0f);
        }      
	}

    public void ShowHighScores(int newScore, float delay = 0.0f)
    {
        _newHighScoreIndex = -1;

        GetHighScores();

        for (int j = 0; j < ScoreRows.Count; j++)
        {
            if (_highScores.PlayerScores[j].PlayerScore < newScore)
            {
                _newHighScoreIndex = j;
                UpdateScores("", newScore);
                Keyboard.targetGameObject = ScoreRows[_newHighScoreIndex].Initials.gameObject;
                break;
            }
        }

        for (int i = 0; i < ScoreRows.Count; i++)
        {
			ScoreRows[i].SetRowText(_highScores.PlayerScores[i].PlayerInitials, _highScores.PlayerScores[i].PlayerScore);
        }

		if (_newHighScoreIndex != -1)
		{
			DoneButton.ShowButton();
			ShowKeyboard(delay * 2);
			ShowHighScoresHolder(true, delay);
		}
        else
		{
			ShowHighScoresHolder(false, delay);         
			OnHighScoreNotAchieved();
		}  
    }

	public void HideHighScores()
    {
		DoneButton.HideButton();
		if (_newHighScoreIndex != -1)
		{
			HideHighScoresHolder(true, 0.3f);
			HideKeyboard();
		}
		else
			HideHighScoresHolder(false);      
    }

	public bool CheckForTopScore(int score)
	{
		GetHighScores();
		for (int j = 0; j < _highScores.PlayerScores.Count; j++)
        {
			if (score > _highScores.PlayerScores[j].PlayerScore)
				return true;
        }

		return false;
	}

    private void ShowHighScoresHolder(bool highScoreAchieved, float delay = 0.0f)
	{
		HighScoresHolderCanvasGroup.gameObject.SetActive(true);
		for (int i = 0; i < HighScoresText.Count; i++)
        {
			HighScoresText[i].DOFade(1.0f, 0.4f).SetDelay(delay);
        }

		if (highScoreAchieved)
			HighScoresHolderCanvasGroup.transform.DOLocalMoveY(HighScoreHolderYDelta, 0.4f).SetEase(Ease.OutQuart).SetDelay(delay);

		HighScoresHolderCanvasGroup.DOFade(1.0f, 0.2f).SetDelay(delay);
	}
    
	private void HideHighScoresHolder(bool highScoreAchieved, float delay = 0.0f)
    {
		for (int i = 0; i < HighScoresText.Count; i++)
        {
			HighScoresText[i].DOFade(0.0f, 0.2f).SetDelay(delay);
        }

		if (highScoreAchieved)
			HighScoresHolderCanvasGroup.transform.DOLocalMoveY(_highScoresHolderOriginalPosition.y, 0.4f).SetDelay(delay).SetEase(Ease.OutQuart);
		
		HighScoresHolderCanvasGroup.DOFade(0.0f, 0.4f).SetDelay(delay).OnComplete(() => {
			HighScoresHolderCanvasGroup.gameObject.SetActive(false);
        });
    }

	public void ShowKeyboard(float delay = 0.0f)
    {
		Keyboard.gameObject.SetActive(true);
        for (int i = 0; i < _keyboardText.Length; i++)
        {
            _keyboardText[i].DOFade(1.0f, 0.4f).SetDelay(delay);
        }
      
		_keyBackRenderer.sharedMaterial.DOFade(1.0f, 0.3f).SetDelay(delay).OnComplete(() => {
			for (int k = 0; k < _keyboardColliders.Count; k++)
            {
                _keyboardColliders[k].enabled = true;
            }
		});
    }

	private void HideKeyboard()
	{
		for (int k = 0; k < _keyboardColliders.Count; k++)
        {
            _keyboardColliders[k].enabled = false;
        }

		for (int i = 0; i < _keyboardText.Length; i++)
        {
			_keyboardText[i].DOFade(0.0f, 0.2f);
        }

		_keyBackRenderer.sharedMaterial.DOFade(0.0f, 0.4f).OnComplete(() => {
			Keyboard.gameObject.SetActive(false);
        });
    }

    void InitialEntryFinished()
    {
        _highScores.PlayerScores[_newHighScoreIndex].PlayerInitials = ScoreRows[_newHighScoreIndex].Initials.text;
        SaveHighScores();
        OnHighScoreEntryDone();
    }

	public void UpdateScores(string initials, int score)
	{
        for (int i = 0; i < _highScores.PlayerScores.Count; i++)
        {
            if(score > _highScores.PlayerScores[i].PlayerScore)
            {
                PlayerScoreModel playerScore = new PlayerScoreModel(initials, score);
                _highScores.PlayerScores.Insert(i, playerScore);
                _highScores.PlayerScores.RemoveAt(_highScores.PlayerScores.Count - 1);
                SaveHighScores();
                break;
            }
        }
	}

    public void SaveHighScores()
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(_highScores));
        File.WriteAllBytes(Application.persistentDataPath + "/HighScores.json", bytes);
    }

    public void GetHighScores()
    {
        if (File.Exists(Application.persistentDataPath + "/HighScores.json"))
        {
            _highScores = JsonUtility.FromJson<HighScoresModel>(File.ReadAllText(Application.persistentDataPath + "/HighScores.json"));
        }
    }

	public void GenerateTestScores()
    {
        List<PlayerScoreModel> playerScores = new List<PlayerScoreModel>();

        PlayerScoreModel playerScore1 = new PlayerScoreModel("RCW", 500);
        PlayerScoreModel playerScore2 = new PlayerScoreModel("TPB", 400);
        PlayerScoreModel playerScore3 = new PlayerScoreModel("ACR", 300);
        PlayerScoreModel playerScore4 = new PlayerScoreModel("BTR", 200);
        PlayerScoreModel playerScore5 = new PlayerScoreModel("JMS", 100);

        playerScores.Add(playerScore1);
        playerScores.Add(playerScore2);
        playerScores.Add(playerScore3);
        playerScores.Add(playerScore4);
        playerScores.Add(playerScore5);

        _highScores = new HighScoresModel(playerScores);

        SaveHighScores();
    }
    
	void OnEnable()
	{
		DoneButton.OnButtonClicked += InitialEntryFinished;
	}

	void OnDisable()
    {
        DoneButton.OnButtonClicked -= InitialEntryFinished;
    }
}
