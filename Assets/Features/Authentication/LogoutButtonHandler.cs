using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Analytics;
using System.Text;
using TMPro;

[RequireComponent(typeof(Interactible), typeof(CanvasGroup))]
public class LogoutButtonHandler : MonoBehaviour
{
    public string LoginText = "Login";
    public string LogoutText = "Logout";

    private Interactible _Interactible;
    private AudioController _AudioController;
    private TextMeshProUGUI _TextItem;
    private AuthenticationController _Authentication;
    private bool _LoggedInStatus;

    void Awake()
    {
        _Interactible = GetComponent<Interactible>();

        if (_Interactible == null)
            throw new Exception("An Interactible must exist on this button.");

        _Authentication = Extensions.GetRequired<AuthenticationController>();
        _AudioController = Extensions.GetRequired<AudioController>();
        _TextItem = GetComponentInChildren<TextMeshProUGUI>();
    }

    void Start()
    {
        if(_Authentication != null)
            SetStatus(_Authentication.IsAuthenticated);
    }

    void OnEnable()
    {
        _Interactible.OnClick += OnBtnClick;
        if(_Authentication != null)
            _Authentication.StatusChange += _Authentication_StatusChange;
    }

    void OnDisable()
    {
        _Interactible.OnClick -= OnBtnClick;
        if (_Authentication != null)
            _Authentication.StatusChange -= _Authentication_StatusChange;
    }
    private void OnBtnClick()
    {
        _AudioController.PlayAudio(AudioController.AudioClips.GenericClick);

        if (_LoggedInStatus)
        {
            Debug.Log("Logout button clicked");
            _Authentication.ShowLogoutPrompt();
        }
        else
        {
            Debug.Log("Login button clicked");
            _Authentication.PrepareToExitAsync(null);
        }
    }

    private void _Authentication_StatusChange(object sender, UserAuthenticationEventArgs e)
    {
        SetStatus(e.IsAuthenticated);
    }

    private void SetStatus(bool status)
    {
        _LoggedInStatus = status;

        if (_TextItem != null)
        {
            _TextItem.text = status ? LogoutText : LoginText;
        }
    }
}