using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class TVEAuthenticator : Singleton<TVEAuthenticator>, IAuthenticationService
{
    [SerializeField, Tooltip("Device type Identifider (e.g. 'firetv', 'androidtv', 'android'")]
    private string _osID;

    [SerializeField, Tooltip("Brand identifier (e.g. 'TNT')")]
    private string _requestorName;

    [SerializeField, Tooltip("The second screen activation page URL")]
    private string _registrationUrl;

    [SerializeField, Tooltip("Private key provided by Media enablement team")]
    private string _privateKey;

    [SerializeField, Tooltip("Public key provided by Media enablement team")]
    private string _publicKey;

    [SerializeField, Tooltip("Url to Turner MVPD config. It is provided by Media enablement team")]
    private string _providerConfigUrl;

    [SerializeField, Tooltip("Url to Turner token service. It is provided by Media enablement team")]
    private string _tokenServiceUrl;

    [Tooltip("Time in seconds until timeout on request attempts such as initialize")]
    public float RequestTimeout = 2f;

#if UNITY_EDITOR
    [Tooltip("Login Status for in editor")]
    public bool DemoLoginStatus;

    [Tooltip("Provider for in editor")]
    public ProviderInformation DemoProvider = new ProviderInformation("test", "Test Provider", "https://www.intel.com/etc/designs/intel/us/en/images/printlogo.png");
#endif

    private bool _IsInitialized;


    private ProviderInformation _CurrentProvider;
    public ProviderInformation CurrentProvider
    {
        get
        {
            return _CurrentProvider;
        }
        private set
        {
            _CurrentProvider = value;
            OnProviderChanged(new ProviderChangedEventArgs(value));
        }
    }

    protected virtual void OnProviderChanged(ProviderChangedEventArgs e)
    {
        var ev = ProviderChanged;

        if (ev != null)
            ev(this, e);
    }

    public event EventHandler<ProviderChangedEventArgs> ProviderChanged;

    private AndroidJavaObject _authBridgeObject;        // Java object for the auth module
    private AndroidJavaObject _unityActivityContext;	// Java object for the current activity context

    public string ProtocolHandler { get; private set; }



    void Awake()
    {
        // Since instance is already attached to a gameObject, singleton template doesn't add DontDestroyOnLoad
        DontDestroyOnLoad(this);
    }

    IEnumerator Start()
    {
        yield return Initialize();
    }

    public Coroutine InitializeAsync()
    {
        return StartCoroutine(Initialize());
    }

    /// <summary>
    /// Load the java authenitcation module and set up the object
    /// </summary>
    private IEnumerator Initialize()
    {
        if (_IsInitialized)
            yield break;

#if UNITY_EDITOR
        _IsInitialized = true;
#elif UNITY_ANDROID
        // Get the current activity context
        _unityActivityContext = AndroidActivityHelper.GetCurrentActivity();

		if (_unityActivityContext == null) {
			Debug.LogError("TVEAuthentication: Fail to get the activity context.");
			yield break;
		}

		// Find and instantiate the authenication library
		_authBridgeObject = new AndroidJavaObject ("com/vokevr/android/tveauthenticationbridge/AdobeAuthentication");
		if (_authBridgeObject == null) {
			Debug.LogError ("TVEAuthentication: Fail to find and instantiate a TVE Authentication object!!!");
			yield break;
		}

		// Initialize the auth module
		_authBridgeObject.Call<Boolean> ("initialize", _unityActivityContext);

		// Register the listener functions
		_authBridgeObject.Call ("setInfoListener", new InfoListener (this));

		// Configure the auth module
		_authBridgeObject.Call<Boolean> ("configure", new object[] { _osID, _requestorName, _registrationUrl, _privateKey,
																	 _publicKey, _providerConfigUrl, _tokenServiceUrl });

        var packageName = _unityActivityContext.Call<string>("getPackageName");

        var res = _unityActivityContext.Call<AndroidJavaObject>("getResources");

        int resid = res.Call<int>("getIdentifier", "protocol_handler", "string", packageName);

        ProtocolHandler = _unityActivityContext.Call<string>("getString", resid);
#endif

        float elapsedTime = 0;
        while (!_IsInitialized)
        {
            yield return null;

            elapsedTime += Time.deltaTime;

            if (elapsedTime > RequestTimeout)
                break;
        }
    }

    protected Queue<Action<string, AuthenticationError>> RegistrationUrlEvents = new Queue<Action<string, AuthenticationError>>();

    public void GetRegistrationUrl(string id, Action<RegistrationUrlEventArgs> callback)
    {
        Action<string, AuthenticationError> ev = (code, error) =>
        {
            if (error != null)
            {
                callback(new RegistrationUrlEventArgs(error));
            }
            else
            {
                string url = String.Format("{0}?input={1}&os={2}&id={3}&platform={4}",
                   _registrationUrl, // 0
                   code, // 1
                   _osID, // 2
                   id, // 3
                   ProtocolHandler // 4
                   );

                callback(new RegistrationUrlEventArgs(url));
            }

        };

#if UNITY_EDITOR
        ev("test", null);
#elif UNITY_ANDROID
        RegistrationUrlEvents.Enqueue(ev);

		_authBridgeObject.Call ("getRegistrationCodeAsync");
#endif
    }

    public event EventHandler<UserAuthenticationEventArgs> UserAuthentication;

    protected virtual void OnUserAuthentication(UserAuthenticationEventArgs e)
    {
        if (e.Success && e.IsAuthenticated)
        {
            Debug.Log("Turner Authentication: User is Authenticated Now ::::");
        }
        else
        {
            Debug.Log("Turner Authentication: USER COULD NOT BE AUTHENTICATED");
        }

        CurrentProvider = e.Provider;

        _IsInitialized = true;

        var ev = UserAuthentication;

        if (ev != null)
            ev(this, e);
    }


    /// <summary>
    /// Determines whether a current user is authenticated.
    /// </summary>
    /// <returns><c>true</c> if the user is authenticated; otherwise, <c>false</c>.</returns>
    public bool CheckAuthenticationStatus()
    {
#if UNITY_EDITOR
        return DemoLoginStatus;
#elif UNITY_ANDROID
        return _authBridgeObject.Call<Boolean> ("isAuthenticated");
#endif
    }

    /// <summary>
    /// Log a current user out
    /// </summary>
    public void Logout()
    {
#if UNITY_EDITOR
        DemoLoginStatus = false;
        OnUserAuthentication(new UserAuthenticationEventArgs(DemoLoginStatus, DemoProvider));
#elif UNITY_ANDROID
        _authBridgeObject.Call ("logout");
#endif

        CurrentProvider = null;
    }


    protected Queue<Action<UrlTokenizationEventArgs>> TokenizeUrlEvents = new Queue<Action<UrlTokenizationEventArgs>>();

    /// <summary>
    /// Get a tokenized URL based on the given video Url and channel ID. If successful, a TokenizedUrlAcquired event will be fired with a tokenized url.
    /// Otherwise, a onTokenizedUrlRequestError event will be fired.
    /// </summary>
    /// <param name="url">Video URL.</param>
    /// <param name="callback">callback to deliver url token to</param>
    public void GetTokenizedUrlAsync(string url, Action<UrlTokenizationEventArgs> callback)
    {
#if UNITY_EDITOR
        callback(new UrlTokenizationEventArgs(url));
#elif UNITY_ANDROID
        TokenizeUrlEvents.Enqueue(callback);

		_authBridgeObject.Call ("getTokenizedUrlAsync", new object[] { url, _requestorName });
#endif
    }

    void Update()
    {
        ExecuteQueue();

#if UNITY_EDITOR
        if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKey(KeyCode.L))
        {
            if (!DemoLoginStatus)
            {
                DemoLoginStatus = true;
                SceneChanger.Instance.ReloadSceneAsync();
            }
        }
#endif
    }

    class InfoListener : AndroidJavaProxy
    {
        private TVEAuthenticator _Auth;
        public InfoListener(TVEAuthenticator auth) : base("com.vokevr.android.tveauthenticationbridge.AdobeAuthentication$InfoListener")
        {
            _Auth = auth;
        }

        void onRegistrationCodeAcquired(string registrationCode)
        {
            Debug.Log("TVEAuthentication: onRegistrationCodeAcquired() Code=" + registrationCode);

            var action = _Auth.RegistrationUrlEvents.Dequeue();
            _Auth.Enqueue(()=>action(registrationCode, null));
        }

        void onRegistrationCodeRequestError(string errorCode, string errorDescription)
        {
            Debug.Log("TVEAuthentication: onRegistrationCodeRequestError");

            var error = new AuthenticationError(errorCode, errorDescription);
            var action = _Auth.RegistrationUrlEvents.Dequeue();

            _Auth.Enqueue(() => action(null, error));
        }

        void onUserIsAuthenticated(string providerMvpdName, string providerDisplayName, string providerBrandImageUrl)
        {
            Debug.Log("TVEAuthentication: onUserIsAuthenticated");

            var providerInfo = new ProviderInformation(providerMvpdName, providerDisplayName, providerBrandImageUrl);
            _Auth.Enqueue(() => _Auth.OnUserAuthentication(new UserAuthenticationEventArgs(true, providerInfo)));
        }

        void onUserIsNotAuthenticated()
        {
            Debug.Log("TVEAuthentication: onUserIsNotAuthenticated");
            _Auth.Enqueue(() => _Auth.OnUserAuthentication(new UserAuthenticationEventArgs(false, null)));
        }

        void onTokenizedUrlAcquired(string tokenizedUrl)
        {
            Debug.Log("TVEAuthentication: onTokenizedUrlAcquired");

            var action = _Auth.TokenizeUrlEvents.Dequeue();

            _Auth.Enqueue(() => action(new UrlTokenizationEventArgs(tokenizedUrl)));
        }

        void onTokenizedUrlRequestError(string errorCode, string errorDescription)
        {
            Debug.Log("TVEAuthentication: onTokenizedUrlRequestError");

            var action = _Auth.TokenizeUrlEvents.Dequeue();

            var error = new AuthenticationError(errorCode, errorDescription);
            _Auth.Enqueue(() => action(new UrlTokenizationEventArgs(error)));
        }

        void onPollingAuthStatusStarted()
        {
            Debug.Log("TVEAuthentication: onPollingAuthStatusStarted");
        }

        void onPollingAuthStatusStopped()
        {
            Debug.Log("TVEAuthentication: onPollingAuthStatusStopped");
        }
    }

    #region Workaround for main thead android callbacks
    private static readonly Queue<Action> _executionQueue = new Queue<Action>();

    private void ExecuteQueue()
    {
        lock (_executionQueue)
        {
            while (_executionQueue.Count > 0)
            {
                _executionQueue.Dequeue().Invoke();
            }
        }
    }

    /// <summary>
    /// Locks the queue and adds the IEnumerator to the queue
    /// </summary>
    /// <param name="action">IEnumerator function that will be executed from the main thread.</param>
    protected void Enqueue(IEnumerator action)
    {
        lock (_executionQueue)
        {
            _executionQueue.Enqueue(() =>
            {
                StartCoroutine(action);
            });
        }
    }

    /// <summary>
    /// Locks the queue and adds the Action to the queue
    /// </summary>
    /// <param name="action">function that will be executed from the main thread.</param>
    protected void Enqueue(Action action)
    {
        Enqueue(ActionWrapper(action));
    }

    IEnumerator ActionWrapper(Action a)
    {
        a();
        yield return null;
    }
    #endregion
}