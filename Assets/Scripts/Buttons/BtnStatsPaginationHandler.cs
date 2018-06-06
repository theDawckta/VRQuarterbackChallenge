using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BtnStatsPaginationHandler : MonoBehaviour 
{

	public GameObject ButtonIcon;
	public GameObject ToggledButtonIcon;

	private Interactible _interactible;

    // Use this for initialization
    void Awake()
    {
        _interactible = this.gameObject.GetComponent<Interactible>();
    }

    private void OnEnable()
	{
		_interactible.OnClick += TogglePitcherBatter;
	}

	private void OnDisable()
	{ 
		_interactible.OnClick -= TogglePitcherBatter;
	}


	private void TogglePitcherBatter()
	{
		if(ButtonIcon.activeSelf)
		{
			ButtonIcon.SetActive(false);
			ToggledButtonIcon.SetActive(true);
		}
		else
		{
			ButtonIcon.SetActive(true);
			ToggledButtonIcon.SetActive(false);
		}
	}	
}
