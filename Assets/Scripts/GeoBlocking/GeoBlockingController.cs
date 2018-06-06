using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;
using VOKE.VokeApp.Util;
using VOKE.VokeApp.GeoDataModel;
using UnityEngine.VR;
using VRStandardAssets.Utils;

public class GeoBlockingController : Singleton<GeoBlockingController>
{
	#region Public Fields

	[HideInInspector]
	public GeocodeResponse geocodeResponse;
	[HideInInspector]
	public bool IsGeoBlockedValid;

	#endregion

	#region Private Fields

	private string _GEO_BLOCK_URL = "";
	private string _country;
	private string _zipcode;
	private string _leagueID;
	private string _teamID;
	private string _isBlockedTxt;
	private float  _deltaTime = 0.0f;
  private bool   _isLocalBlocked=true;

	#endregion

	public IEnumerator Init_LocationService()
	{
#if !RemoveGeoBlocking
        // First, check if user has location service enabled
        if (!Input.location.isEnabledByUser)
			yield break;

		// Start service before querying location
		Input.location.Start();


        // Wait until service initializes
        int maxWait = 20;
		while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
		{
			yield return new WaitForSeconds(1);
			maxWait--;
		}

		// Service didn't initialize in 20 seconds
		if (maxWait < 1)
		{
			Debug.Log("Location service timed out");
			yield break;
		}

		// Connection has failed
		if (Input.location.status == LocationServiceStatus.Failed)
		{
			Debug.Log("Unable to determine device location");
			yield break;
		}
		else
		{
			// Access granted and location value could be retrieved
			Debug.Log("Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp);
			string url1 = "https://maps.googleapis.com/maps/api/geocode/xml?latlng=";
			string url2 = Input.location.lastData.latitude + "," + Input.location.lastData.longitude;
			string url3 = "&key=AIzaSyD3El9FKSmfFNan3YJ1LGxrq5CDg3_dJyc";
			string url = url1 + url2 + url3;

			WWW w = new WWW(url);
			yield return w;

			if(string.IsNullOrEmpty(w.error))
			{
				StartCoroutine(ParseLocationText(w.text));
			}
		}

		// Stop service if there is no need to query location updates continuously
		Input.location.Stop();
#endif
        yield return null;
    }

    public void SetLocationData(bool isLocalBlocked, string leagueID, string teamID)
	{
		_leagueID = leagueID;
		_teamID   = teamID;
        _isLocalBlocked = isLocalBlocked;
		Debug.Log("Set values for location data: " + _leagueID + ".." + _teamID);
	}

	public IEnumerator ParseLocationText(string locationText)
	{
		yield return new WaitForSeconds(0.1f);

		try
		{
			//XML serialize/Deserialize
			var serializer = new XmlSerializer(typeof(GeocodeResponse));
			var reader = new StringReader(locationText);
			var data = (GeocodeResponse)serializer.Deserialize(reader);

			geocodeResponse = data;
			foreach(AddressComponent comp in geocodeResponse.ResultList)
			{
				if(comp.AddressType.Contains("country"))
				{
					_country = comp.ShortName;
					Debug.Log("THE USER BELONGS TO: "+_country);
				}
				if(comp.AddressType.Contains("postal_code"))
				{
					Debug.Log("THE LOCATION IS:" + comp.AddressType + "...." + comp.LongName);
					_zipcode = comp.LongName;
				}
			}
			data = null;
		}
		catch(XmlException ex)
		{
			Debug.LogError("Fail to load the xml file. ErrorMessage=" + ex.Message);
		}

		StartCoroutine(GetGeoblockURL());
	}

	public void DoZipCodeBlackOutCheck(Action<bool> callback)
	{
		CheckRequired(_leagueID);

		switch(_leagueID)
		{
			case "MLB":
				{
					StartCoroutine(MLBBlackOutCheck(callback));
					break;
				}
			default:
				break;
		}
		//return retVal;
	}

	private void CheckRequired(string thing)
	{
		if(string.IsNullOrEmpty(thing))
		{
			Debug.Log(String.Format("A {0} is required to run this scene.", thing));
		}
	}

	private IEnumerator GetGeoblockURL()
	{
		yield return AppConfigurationLoader.Instance.GetDataAsync(data =>
		{
			_GEO_BLOCK_URL = data.GetResourceValueByID("GeoblockURL");
		});	
	}

	private IEnumerator MLBBlackOutCheck(Action<bool> callback)
	{
		//This is the endpoint url for MLB to check geoblocking
		//http://54.68.21.16/mlb_geoblock/api/1.0/GeoBlockedCheck.php?team_id=ATL&zipcode=
		CheckRequired(_teamID);
		CheckRequired(_zipcode);
		CheckRequired(_GEO_BLOCK_URL);

		bool retVal = true;
		if(!string.IsNullOrEmpty(_country))
		{
			if(_country.Contains("US"))
			{
				string url1 = "team_id=";
				string url2 = "&zipcode=";
				string url = _GEO_BLOCK_URL + url1 + _teamID + url2 + _zipcode;
				Debug.Log("API CALL: " + url);
				WWW w = new WWW(url);
				yield return w;

				if(string.IsNullOrEmpty(w.error))
				{
					_isBlockedTxt = w.text;

					if(!string.IsNullOrEmpty(_isBlockedTxt))
					{
						if(_isBlockedTxt.Contains("true"))
						{
							Debug.Log("Invoke true");
							retVal = true;
						}
						else
						{
							Debug.Log("Invoke false");
							retVal = false;
						}

            if(!_isLocalBlocked)
            {
              retVal = !retVal;
            }
					}
				}
			}
			else
			{
				Debug.Log("Outside USA. Invoke true");
				retVal = true;
			}
		}
		callback.Invoke(retVal);
	}
}
