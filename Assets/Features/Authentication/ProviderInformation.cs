using System.Collections;
using System.Collections.Generic;

public class ProviderInformation
{
    public ProviderInformation(string mvpdName, string displayName, string brandImageUrl)
    {
        MvpdName = mvpdName;
        DisplayName = displayName;
        BrandImageUrl = brandImageUrl;
    }

    public string MvpdName { get; private set; }
    public string DisplayName { get; private set; }
    public string BrandImageUrl { get; private set; }
}
