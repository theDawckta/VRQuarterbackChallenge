using System;
using UnityEngine;

public interface IAuthenticationService
{
    Coroutine InitializeAsync();

    event EventHandler<UserAuthenticationEventArgs> UserAuthentication;
    event EventHandler<ProviderChangedEventArgs> ProviderChanged;

    ProviderInformation CurrentProvider { get; }

    void GetRegistrationUrl(string id, Action<RegistrationUrlEventArgs> callback);
    void GetTokenizedUrlAsync(string url, Action<UrlTokenizationEventArgs> callback);
    void Logout();
    bool CheckAuthenticationStatus();
}

public static class AuthenticationService
{
    public static IAuthenticationService Default
    {
        get { return TVEAuthenticator.Instance; }
    }
}