using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VOKE.VokeApp.DataModel;
using System;
using System.Linq;
using System.Text;

[RequireComponent(typeof(DynamicTexture))]
public class AuthenticationController : MonoBehaviour
{
	public float AuthImageAlpha = 1.0f;

    private string _UrlToOpenOnExit;
    private IAuthenticationService _AuthenticationService;
    private PopupController _Popup;
    private Interactible _Interactible;
    private DynamicTexture _ProviderImage;
    private float _DefaultProviderImageAlpha;

    private const string _LeaveToAuthenticateKey = "LeaveToAuthenticate";
    private const string _ShownAuthenticationPromptKey = "ShownAuthenticationPrompt";

    public bool IsAuthenticated
    {
        get { return _AuthenticationService.CheckAuthenticationStatus(); }
    }

    void Awake()
    {
        _ProviderImage = GetComponent<DynamicTexture>();
		_ProviderImage.SetFadeInAlphaTarget (AuthImageAlpha);
        _AuthenticationService = AuthenticationService.Default;
        _Popup = PopupController.Instance;
        _Interactible = GetComponent<Interactible>();
    }

    // Use this for initialization
    void Start()
    {
        _DefaultProviderImageAlpha = _ProviderImage.Alpha;

        SetProvider(_AuthenticationService.CurrentProvider);

        if (!GlobalVars.Instance.IsDaydream)
            ShowInitialSignInPrompt();
    }

    public void ShowInitialSignInPrompt()
    {
        bool isAuthenticated = _AuthenticationService.CheckAuthenticationStatus();

        if (PlayerPrefs.HasKey(_LeaveToAuthenticateKey))
        {
            PlayerPrefs.DeleteKey(_LeaveToAuthenticateKey);

            if (isAuthenticated)
            {
                _Popup.Setup("Successfully authenticated.  You now have access to all content.")
                        .WithButton("OK", () => StartCoroutine(LoadSpecifiedID()))
                        .Show();
            }
            else
            {
                ShowFirstTimeAuthenticationPrompt("Unfortunately, it looks like you haven't signed in with your TV provider successfully.");
            }
        }
        else if (!PlayerPrefs.HasKey(_ShownAuthenticationPromptKey) && !isAuthenticated)
        {
            ShowFirstTimeAuthenticationPrompt("It looks like you haven't signed in with your TV provider yet.");
            PlayerPrefs.SetString(_ShownAuthenticationPromptKey, "true");
        }
    }

    public void ShowLogoutPrompt()
    {
        var sb = new StringBuilder();
        sb.Append("Would you like to sign out of");

        if (_AuthenticationService.CurrentProvider != null)
        {
            sb.AppendFormat(" {0},", _AuthenticationService.CurrentProvider.DisplayName);
        }

        sb.Append("your TV provider? You'll lose access to any exclusive content.");

        string message = sb.ToString();

        _Popup.Setup(message)
            .WithButton("Yes", () => LogoutAndShowPopup(), false)
            .WithButton("Cancel")
            .Show();
    }

    private void LogoutAndShowPopup()
    {
        _Popup.HidePopup(true);

        _AuthenticationService.Logout();

        var sb = new StringBuilder();
        sb.Append("Thanks for watching");
        if (_AuthenticationService.CurrentProvider != null)
        {
            sb.AppendFormat(" with {0}", _AuthenticationService.CurrentProvider.DisplayName);
        }

        sb.Append(". You are now successfully logged out.");

        string message = sb.ToString();

        _Popup.Setup(message)
            .WithButton("OK", () => SceneChanger.Instance.ReloadSceneAsync())
            .Show();
    }

    private IEnumerator LoadSpecifiedID()
    {
        var arguments = PlatformEnvironment.GetArguments();

        string id;
        if (!arguments.TryGetValue("id", out id))
            yield break;

        DataCursor cursor = null;

        yield return DataCursorComponent.Instance.GetCursorAsync(c => cursor = c);

        var content = cursor.ReturnRoot().FindRecursive(id);

        if (content != null)
        {
            cursor.MoveTo(content);
            yield return SceneChanger.Instance.FadeToConsumptionAsync();
        }
    }

    private void OnEnable()
    {
        _AuthenticationService.ProviderChanged += _AuthenticationService_ProviderChanged;
        _AuthenticationService.UserAuthentication += _AuthenticationService_UserAuthentication;

        if (_Interactible != null)
        {
            _Interactible.OnClick += _Interactible_OnClick;
        }
    }


    private void OnDisable()
    {
        _AuthenticationService.ProviderChanged -= _AuthenticationService_ProviderChanged;
        _AuthenticationService.UserAuthentication -= _AuthenticationService_UserAuthentication;

        if (_Interactible != null)
        {
            _Interactible.OnClick -= _Interactible_OnClick;
        }
    }
    private void _AuthenticationService_ProviderChanged(object sender, ProviderChangedEventArgs e)
    {
        SetProvider(e.Provider);
    }

    private void _AuthenticationService_UserAuthentication(object sender, UserAuthenticationEventArgs e)
    {
        OnStatusChange(e);
    }

    protected virtual void OnStatusChange(UserAuthenticationEventArgs e)
    {
        var ev = StatusChange;
        if (ev != null)
        {
            ev(this, e);
        }
    }

    public event EventHandler<UserAuthenticationEventArgs> StatusChange;

    private void _Interactible_OnClick()
    {
        PlayerPrefs.DeleteKey(_ShownAuthenticationPromptKey);
    }

    private void SetProvider(ProviderInformation provider)
    {
        if (_ProviderImage == null)
            return;

        if (provider == null)
        {
            _ProviderImage.Alpha = 0;
        }
        else
        {
            _ProviderImage.Alpha = _DefaultProviderImageAlpha;
            _ProviderImage.SetTexture(provider.BrandImageUrl, true);
        }
    }

    private void ShowFirstTimeAuthenticationPrompt(string message)
    {
        _Popup.Setup(message + "\n\nYou may continue without signing in, but you will need to sign in before you can watch videos.")
            .WithButton("Sign In Now", () => PrepareToExitAsync(null), false)
            .WithButton("Sign In Later")
            .Show();
    }

    public void ShowAuthenticationPrompt(ContentViewModel video)
    {
        _Popup.Setup("Sign in is required for videos.\n\nYou may continue without signing in, but you will need to sign in before you can watch videos.")
            .WithButton("Sign In", () => PrepareToExitAsync(video), false)
            .WithButton("Cancel")
            .Show();
    }

    public void PrepareToExitAsync(ContentViewModel video)
    {
        StartCoroutine(PrepareToExit(video));
    }

    private IEnumerator PrepareToExit(ContentViewModel video)
    {
        _Popup.HidePopup(false);

        bool gotUrl = false;

        AuthenticationError error = null;
        _AuthenticationService.GetRegistrationUrl(video != null ? video.ID : null, e =>
        {
            if (e.Success)
            {
                _UrlToOpenOnExit = e.Url;
            }
            else
            {
                error = e.Error;
            }
            gotUrl = true;
        });

        while (!gotUrl)
        {
            yield return null;
        }

        if (error != null)
        {
            Debug.Log("Error getting registration url: " + error.Description);

            _UrlToOpenOnExit = null;
            _Popup.ShowPopup("Something has gone wrong connecting to the login provider.  Please try again later.");
        }
        else
        {
            var sb = new StringBuilder();

            sb.Append("Please remove your phone from the headset");

            if (GlobalVars.Instance.IsDaydream)
            {
                sb.Append(", exit the app by clicking the X button, ");
            }

            sb.Append(" and follow the instructions to sign in.");

            string message = sb.ToString();

            _Popup.Setup(message)
                .WithButton("Cancel", () => _UrlToOpenOnExit = null, gazeDelay: 4.0f)
                .Show();
        }
    }

    private void OnApplicationQuit() // preferred method for opening browser on taking off the headset
    {
        if (!String.IsNullOrEmpty(_UrlToOpenOnExit))
        {
            PlayerPrefs.SetString(_LeaveToAuthenticateKey, "true");
            Application.OpenURL(_UrlToOpenOnExit);
        }
    }

    public void Logout()
    {
        _AuthenticationService.Logout();
    }

    public bool RequiresAuthentication(ContentViewModel video)
    {
        return IsAuthenticatedVideo(video) && !_AuthenticationService.CheckAuthenticationStatus();
    }

    private bool IsAuthenticatedVideo(ContentViewModel video)
    {
        if (video == null)
            return false;

        if (video.RequiresAuthentication != null && video.RequiresAuthentication.Value)
            return true;

        return RequiresAuthentication(video.Parent);
    }

    public bool ShouldSignStreams(ContentViewModel video)
    {
        return IsAuthenticatedVideo(video) && !video.Streams.All(_ => _.IsTokenized);
    }

    public Coroutine SignAnyStreamsAsync(ContentViewModel video)
    {
        return StartCoroutine(SignAnyStreams(video));
    }

    private IEnumerator SignAnyStreams(ContentViewModel video)
    {
        if (!IsAuthenticatedVideo(video))
            yield break;

        Debug.Log("Start fetching tokenized urls");

        foreach (CameraStreamModel cm in video.Streams)
        {
            if (!cm.IsTokenized)
            {
                bool gotResponse = false;
                string errorMessage = null;

                _AuthenticationService.GetTokenizedUrlAsync(cm.Url, e =>
                {
                    if (e.Success)
                    {
                        cm.Url = e.TokenizedUrl;
                        cm.IsTokenized = true;
                    }
                    else
                    {
                        Debug.Log("Error in retrieving the tokenized URL: " + e.Error.Description);

                        switch (e.Error.Code)
                        {
                            case "403":
                            case "":
                            default:
                                errorMessage = "We're sorry, an error occurred.\nPlease try logging in again with your TV provider.";
                                break;

                            case "404":
                                errorMessage = e.Error.Description;
                                break;
                        }
                    }

                    gotResponse = true;
                });

                while (!gotResponse)
                {
                    yield return null;

                    if (!String.IsNullOrEmpty(errorMessage))
                    {
                        _Popup.ShowPopup(errorMessage);
                        break;
                    }
                }
            }
        }
    }
}
