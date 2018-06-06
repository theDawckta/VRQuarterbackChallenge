using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public abstract class AuthenticationEventArgs : EventArgs
{
    public AuthenticationError Error { get; private set; }

    protected AuthenticationEventArgs(AuthenticationError error)
    {
        Error = error;
    }

    public virtual bool Success
    {
        get { return Error == null; }
    }
}

public class RegistrationUrlEventArgs : AuthenticationEventArgs
{
    public string Url { get; private set; }

    public RegistrationUrlEventArgs(string url) : base(null)
    {
        Url = url;
    }

    public RegistrationUrlEventArgs(AuthenticationError error) : base(error)
    {
    }

    public override bool Success
    {
        get { return base.Success && !String.IsNullOrEmpty(Url); }
    }
}

public class UserAuthenticationEventArgs : AuthenticationEventArgs
{
    public bool IsAuthenticated { get; private set; }
    public ProviderInformation Provider { get; private set; }

    public UserAuthenticationEventArgs(bool isAuthenticated, ProviderInformation provider) : base(null)
    {
        IsAuthenticated = isAuthenticated;
        Provider = provider;
    }
}

public class UrlTokenizationEventArgs : AuthenticationEventArgs
{
    public string TokenizedUrl { get; private set; }

    public UrlTokenizationEventArgs(AuthenticationError error) : base(error) { }

    public UrlTokenizationEventArgs(string tokenizedUrl) : base(null)
    {
        TokenizedUrl = tokenizedUrl;
    }

    public override bool Success
    {
        get { return base.Success && !String.IsNullOrEmpty(TokenizedUrl); }
    }
}

public class AuthenticationError
{
    public AuthenticationError(string code, string description)
    {
        Code = code;
        Description = description;
    }

    public string Code { get; private set; }
    public string Description { get; private set; }
}

public class ProviderChangedEventArgs : EventArgs
{
    public ProviderInformation Provider { get; private set; }

    public ProviderChangedEventArgs(ProviderInformation provider)
    {
        Provider = provider;
    }
}