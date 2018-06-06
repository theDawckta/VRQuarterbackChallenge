using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VersionDetection : MonoBehaviour {

	public float WaitTimer = 2.0f;

	IEnumerator Start()
	{
		//yield return new WaitForSeconds (WaitTimer);
		//StartCoroutine (VersionCheck ());
		yield return null;
	}

	IEnumerator VersionCheck()
	{
		float curVersion;
		float newForceUpdateVersion;

		yield return AppConfigurationLoader.Instance.GetDataAsync(data =>
		{
			string forceUpdateVersion = data.GetResourceValueByID("ForceUpdateVersion");
			string forceUpdateMessage = data.GetResourceValueByID("ForceUpdateMessage");

			bool result = float.TryParse(forceUpdateVersion, out newForceUpdateVersion);
			if(result)
			{
				if(!float.IsNaN(newForceUpdateVersion) && !string.IsNullOrEmpty(forceUpdateMessage))
				{

					bool versionResult = float.TryParse(Application.version, out curVersion);

					if(versionResult)
					{
						if(curVersion <= newForceUpdateVersion)
						{
							PopupController.Instance.ShowPopup(forceUpdateMessage);
						}
					}
				}
			}
		});
	}
}
