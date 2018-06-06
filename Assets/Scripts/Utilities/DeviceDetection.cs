using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VOKE.VokeApp.DataModel;

public class DeviceDetection : MonoBehaviour {
	// previous iteration kept the list local; will check with team to see if they prefer keeping list in XML

	IEnumerator Start()
	{
		//yield return AppConfigurationLoader.Instance.GetDataAsync(data =>
		//{			
		//	foreach (Device device in data.DeviceList)
		//	{
		//		if (SystemInfo.deviceModel.IndexOf(device.Value) != -1)
		//		{
		//			GlobalVars.Instance.UseHEVCStream = true;
		//			break;
		//		}
		//	}

		//});
		yield return null;
	}
}
