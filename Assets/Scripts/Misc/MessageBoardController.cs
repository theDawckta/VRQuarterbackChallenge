using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MessageBoardController : MonoBehaviour
{
	/*#region TEXT
	public Text ErrorTextField;
	public Text TapTextField;
	#endregion*/
	#region TMPro
	public TextMeshProUGUI ErrorTextField;
	public TextMeshProUGUI TapTextField;
	#endregion

    public GameObject MessageBoard;

    private float _ErrorMessageTime = 4.0f;
    private Coroutine _ErrorCoroutine;

    void Awake()
    {
       MessageBoard.SetActive(false);
    }

    public void SetErrorMessage(string message, bool overrideCurrentError = false)
    {
        if (!string.IsNullOrEmpty(message))
        {
            //is there an existing error up
            if (_ErrorCoroutine != null)
            {
                //there is an error up, should we override?
                if (overrideCurrentError)
                {
                    StopCoroutine(_ErrorCoroutine);
                    _ErrorCoroutine = StartCoroutine(SetError(message));
                }
            }
            else
            {
                _ErrorCoroutine = StartCoroutine(SetError(message));
            }
        }
        else
        {
            ClearErrorMessage();
        }
    }

    public void ClearErrorMessage()
    {
        ErrorTextField.text = "";
        MessageBoard.SetActive(false);
        if (_ErrorCoroutine != null)
        {
            StopCoroutine(_ErrorCoroutine);
            _ErrorCoroutine = null;
        }
    }

	void Update()
	{
		if (MessageBoard.activeSelf)
		{
			if (Input.GetMouseButtonDown (0))
			{
				ClearErrorMessage ();
			}
		}
	}

    IEnumerator SetError(string message)
    {
        ErrorTextField.text = message;
        MessageBoard.SetActive(true);
		Canvas.ForceUpdateCanvases();
		float yPos = Mathf.Floor((ErrorTextField.rectTransform.localPosition.y - (ErrorTextField.rectTransform.rect.height/2 + 10)));
		TapTextField.rectTransform.localPosition = new Vector3 (TapTextField.rectTransform.localPosition.x, yPos, TapTextField.rectTransform.localPosition.z);
		yield return new WaitForSeconds(_ErrorMessageTime);
        ClearErrorMessage();
    }

    private void OnDestroy()
    {
        if (_ErrorCoroutine != null)
        {
            StopCoroutine(_ErrorCoroutine);
            _ErrorCoroutine = null;
        }
    }
}