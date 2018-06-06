using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRStandardAssets.Utils;
using DG.Tweening;
using UnityEngine.UI;

public class EnvironmentController : MonoBehaviour
{
    public bool SetCurrentVideoEnvironment;
    public bool DisableModelChanges;

    [Serializable]
    public struct PlacementTarget
    {
        public string Name;
        public GameObject Target;
    }

    public GameObject BaseEnvironment;
    public GameObject EnvironmentHolder;
    public PlacementTarget[] Targets;
    public VRCameraFade CameraFade;
    public GameObject ConfigurableSponsorBillboards;

	private List<SponsorBillboardController> _sponsorBillboards = new List<SponsorBillboardController>();
    private IDictionary<string, Material> _InitialMaterials = new Dictionary<string, Material>();
    private string _CurrentEnvironmentID;
    private EnvironmentModelData _OldEnvironmentModel;

    void OnEnable()
    {
        DataCursorComponent.Instance.Selected += Instance_Selected;
    }

    void OnDisable()
    {
        if (!DataCursorComponent.ApplicationIsQuitting)
            DataCursorComponent.Instance.Selected -= Instance_Selected;
        for (int i = 0; i < _sponsorBillboards.Count; i++)
        {
			_sponsorBillboards[i].PlayButton.OnClick -= SponsorAdPlayed;
			_sponsorBillboards[i].ScreenPlayButton.OnClick -= SponsorAdPlayed;
			_sponsorBillboards[i].CloseButton.OnClick -= SponsorAdDone;
			_sponsorBillboards[i].SponsorMovie.OnEndOfStream -= SponsorAdDone;
        }
    }

    private IEnumerator Start()
    {
        yield return DataCursorComponent.Instance.GetCursorAsync(cursor =>
        {
            if (SetCurrentVideoEnvironment && cursor.CurrentVideo != null)
            {
                EnvironmentChange(cursor.CurrentVideo);
            }
            else
            {
                EnvironmentChange(cursor.CurrentParent);
            }
        });
    }

    private void Instance_Selected(object sender, ContentSelectedEventArgs e)
    {
        if (e.SelectedContent.Type == ContentType.Channel || SetCurrentVideoEnvironment)
        {

            EnvironmentChange(e.SelectedContent);
      	}
    }

    private string GetEnvironment(ContentViewModel content)
    {
        if (content == null)
            return null;

        if (!String.IsNullOrEmpty(content.EnvironmentID))
            return content.EnvironmentID;

        return GetEnvironment(content.Parent);
    }

    /// <summary>
    /// Load in new environment from URL
    /// (based off Voke early script)
    /// </summary>
    /// <param name="assetBundleURL">URL to return asset bundle</param>
    /// <param name="assetName">Name of prefab to use</param>
    /// <param name="assetVersion">Asset version number</param>
    public void EnvironmentChange(ContentViewModel content)
    {
        string environmentId = GetEnvironment(content);

        if (String.IsNullOrEmpty(environmentId)) // normalize empty and null
        {
            environmentId = null;
        }

        if (_CurrentEnvironmentID == environmentId)
        {
            EventManager.Instance.EnvironmentLoadComplete();
            return;
        }

        _CurrentEnvironmentID = environmentId;

        EnvironmentConfigurationLoader.Instance.GetDataAsync(data =>
        {
			if(data == null)
			{
				EventManager.Instance.EnvironmentLoadComplete();
			}
			else
			{
				StartCoroutine(DownloadAndCache(data, environmentId));
			}
        });
    }

    /// <summary>
    /// Check cache for asset bundle, download if new
    /// </summary>
    IEnumerator DownloadAndCache(EnvironmentConfiguration config, string environmentID)
    {
        // Wait for the Caching system to be ready
        while (!Caching.ready)
        {
            yield return null;
        }

        var environment = config.Environments.Find(e => String.Equals(e.ID, environmentID, StringComparison.OrdinalIgnoreCase));

        // reset any materials that were changed previously
        foreach (var pair in _InitialMaterials)
        {
            var obj = FindReferencedAsset(pair.Key);

            if (obj == null)
                continue;

			if (obj.GetComponent<RawImage> () != null)
			{
				Debug.Log ("load in a raw image");
			} else
			{
				var renderer = obj.GetComponent<Renderer> ();

				renderer.material = pair.Value;
			}
        }

        // reset sponsorbillboards
		for(int i = 0; i < _sponsorBillboards.Count; i++)
    	{
			_sponsorBillboards[i].ScreenPlayButton.gameObject.SetActive(false);
			_sponsorBillboards[i].CloseButton.gameObject.SetActive(false);
			_sponsorBillboards[i].PauseButton.gameObject.SetActive(false);
			_sponsorBillboards[i].PlayButton.gameObject.SetActive(false);
			_sponsorBillboards[i].SponsorMovie._moviePath = string.Empty;
    	}

        if (environment == null)
        {
            if (_OldEnvironmentModel != null)
            {
                EventManager.Instance.DisableUserClickEvent();
				if (CameraFade != null)
				{
					if (CameraFade.Alpha == 0)
					{
						yield return StartCoroutine (CameraFade.BeginFadeOut (false));
					}
				}
                SetMainEnvironment(true);
                EventManager.Instance.EnableUserClickEvent();
            }
            else
            {
                SetMainEnvironment(true);
            }
            EventManager.Instance.EnvironmentLoadComplete();
            _OldEnvironmentModel = null;

            yield break;
        }

        using (var loader = new BundleLoader(config.Bundles, config.Base != null ? config.Base.Url : null))
        {
            if (environment.Model == null)
            {
                if (_OldEnvironmentModel != null)
                {
                    EventManager.Instance.DisableUserClickEvent();
					if (CameraFade != null)
					{
						if (CameraFade.Alpha == 0)
						{
							yield return StartCoroutine (CameraFade.BeginFadeOut (false));
						}
					}
                    SetMainEnvironment(true);
                    EventManager.Instance.EnableUserClickEvent();
                }
                else
                {
                    SetMainEnvironment(true);
                }
                EventManager.Instance.EnvironmentLoadComplete();
            }
            else if (!DisableModelChanges)
            {
                EventManager.Instance.DisableUserClickEvent();
					if (CameraFade != null)
					{
						if (CameraFade.Alpha == 0)
						{
							yield return StartCoroutine (CameraFade.BeginFadeOut (false));
						}
					}
                EventManager.Instance.BlackoutFadeInEvent();

                GameObject model = null;
				yield return StartCoroutine(loader.GetBundleAsset<GameObject>(environment.Model, "", result => model = result));

                if (model != null)
                {
                    var env = Instantiate(model);

                    #region Workaround for Unity issue with loading assetbundle model shaders

                    var shaderLookup = new Dictionary<string, Shader>();
                    foreach (var renderer in env.GetComponentsInChildren<Renderer>())
                    {
                        string key = renderer.material.shader.name;
                        Shader s;
                        if (!shaderLookup.TryGetValue(key, out s))
                        {
                            s = Shader.Find(key);
                            shaderLookup[key] = s;
                        }
                        renderer.material.shader = s;
                    }

                    #endregion

                    SetMainEnvironment(false);

                    env.transform.parent = EnvironmentHolder.gameObject.transform;
                }
                EventManager.Instance.BlackoutFadeCompleteEvent();
                EventManager.Instance.EnvironmentLoadComplete();
                EventManager.Instance.EnableUserClickEvent();
            }

            _OldEnvironmentModel = environment.Model;

            foreach (var asset in environment.Assets)
            {
                Texture tex = null;
				yield return StartCoroutine(loader.GetBundleAsset<Texture>(asset, asset.Slot, result => tex = result));

                if (tex == null)
                    continue;

                var obj = FindReferencedAsset(asset.Slot);

				if (asset != null && obj != null)
				{
					GameObject assetGO = obj as GameObject;
					SponsorBillboardController sponsorBillboardController = assetGO.GetComponentInParent<SponsorBillboardController>();

					if (assetGO != null && sponsorBillboardController != null)
					{
						if (!string.IsNullOrEmpty (asset.MediaUrl))
						{
							assetGO.transform.parent.gameObject.SetActive(true);
							_sponsorBillboards.Add(sponsorBillboardController);
							SetMediaURL (assetGO, asset.MediaUrl);
							sponsorBillboardController.ScreenPlayButton.gameObject.SetActive(true);
							sponsorBillboardController.CloseButton.gameObject.SetActive(false);
							sponsorBillboardController.PauseButton.gameObject.SetActive(false);
							sponsorBillboardController.PlayButton.gameObject.SetActive(false);
						}
						if (!string.IsNullOrEmpty (asset.Position))
						{
							assetGO.transform.parent.transform.localPosition = ParseVector3(asset.Position);
							sponsorBillboardController.ScreenMoved = true;
						}
						if (!string.IsNullOrEmpty (asset.Rotation))
						{
							assetGO.transform.parent.transform.eulerAngles = ParseVector3(asset.Rotation);
							sponsorBillboardController.ScreenMoved = true;
						}
						if (!string.IsNullOrEmpty (asset.Scale))
						{
							assetGO.transform.parent.transform.localScale = ParseVector3(asset.Scale);
							sponsorBillboardController.ScreenMoved = true;
						}
						if (!string.IsNullOrEmpty (asset.MediaScreenRotation))
						{
							SetScreenRotation(assetGO, float.Parse(asset.MediaScreenRotation));
							sponsorBillboardController.ScreenPositioned = true;
						}
					}
				}

                if (obj == null)
                    continue;

                var renderer = obj.GetComponent<Renderer>();

				if (renderer != null)
				{
					if (!_InitialMaterials.ContainsKey (asset.Slot))
					{
						_InitialMaterials [asset.Slot] = renderer.material; // save for reset
					}

					var newMat = new Material (renderer.material);

					if (asset.Slot == "Window")
					{
						FadeObjOut (obj, tex, newMat);
					} 
					else
					{
						newMat.mainTexture = tex;
						renderer.material = newMat;
					}
				}
            }

            for(int i = 0; i < _sponsorBillboards.Count; i++)
            {
				_sponsorBillboards[i].PlayButton.OnClick += SponsorAdPlayed;
				_sponsorBillboards[i].ScreenPlayButton.OnClick += SponsorAdPlayed;
				_sponsorBillboards[i].CloseButton.OnClick += SponsorAdDone;
				_sponsorBillboards[i].SponsorMovie.OnEndOfStream += SponsorAdDone;
            }
        }
    }

	private void SponsorAdPlayed()
	{
		for (int i = 0; i < _sponsorBillboards.Count; i++)
		{
			if(!_sponsorBillboards[i].IsPlaying)
			{
				_sponsorBillboards[i].ScreenPlayButton.gameObject.SetActive(false);
			}
		}
	}

	private void SponsorAdDone()
	{
		for (int i = 0; i < _sponsorBillboards.Count; i++)
		{
			if(!string.IsNullOrEmpty(_sponsorBillboards[i].SponsorMovie._moviePath))
				_sponsorBillboards[i].ScreenPlayButton.gameObject.SetActive(true);

			else
				_sponsorBillboards[i].ScreenPlayButton.gameObject.SetActive(false);
		}
	}

	private void SetMediaURL(GameObject asset, string mediaUrl)
	{
		if (!string.IsNullOrEmpty (mediaUrl))
		{
			JackalopeMediaPlayer mediaContainer = asset.GetComponentInChildren<JackalopeMediaPlayer> (true);
			if (mediaContainer != null)
			{
				mediaContainer.MoviePath = mediaUrl;
			}
		}
	}

	private void SetScreenRotation(GameObject asset, float eulerRotation)
	{
		SponsorBillboardController sponsorBillboardController = asset.GetComponentInParent<SponsorBillboardController>();
		Transform oldParent = sponsorBillboardController.TwoDPlayerContainer.transform.parent;
		sponsorBillboardController.ScreenPositioned = true;
		sponsorBillboardController.PlayButton.gameObject.SetActive(false);
		sponsorBillboardController.ScreenPlayButton.gameObject.SetActive(true);
		sponsorBillboardController.TwoDPlayerContainer.transform.parent = null;
		sponsorBillboardController.TwoDPlayerContainer.transform.position = ResourceManager.Instance.MainCamera.transform.position;
		sponsorBillboardController.TwoDPlayerContainer.transform.eulerAngles = new Vector3(0.0f, eulerRotation, 0.0f);

		if(SceneManager.GetActiveScene().name == "HomeScene")
			sponsorBillboardController.TwoDPlayerContainer.transform.position += sponsorBillboardController.TwoDPlayerContainer.transform.forward * -100.0f;
		else
			sponsorBillboardController.TwoDPlayerContainer.transform.position += sponsorBillboardController.TwoDPlayerContainer.transform.forward * -30.0f;

		sponsorBillboardController.TwoDPlayerContainer.transform.SetParent(oldParent, true);
	}

    private void FadeObjOut(GameObject obj, Texture tex, Material newMat)
    {
    	Material objMaterial = obj.GetComponent<Renderer>().material;
		objMaterial.DOFade(0.0f, 0.5f).OnComplete(()=>FadeObjIn(obj, tex, newMat));
    }

    private void FadeObjIn(GameObject obj, Texture newTex, Material newMat)
    {
        var renderer = obj.GetComponent<Renderer>();
        //store old material to reset alpha values
        Material oldMat = renderer.material;
        newMat.mainTexture = newTex;
        //set the color for the new material
        Color color = newMat.color;
        color.a = 0.0f;
        newMat.color = color;
        //set the new mat to the object
        renderer.material = newMat;
        //reset old material color
        color = oldMat.color;
        color.a = 255.0f;
        oldMat.color = color;
        //fade our new material up
        newMat.DOFade(1.0f, 0.5f).SetDelay(0.1f);
    }

    private void SetMainEnvironment(bool visible)
    {
        if (BaseEnvironment != null)
            BaseEnvironment.SetActive(visible);

        if (ConfigurableSponsorBillboards != null)
        {
            ConfigurableSponsorBillboards.SetActive(!visible);
        }

        if (visible)
        {
            if (EnvironmentHolder != null)
            {
                foreach (Transform child in EnvironmentHolder.transform)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
        }
    }

    private class BundleLoader : IDisposable
    {
        IDictionary<string, AssetBundle> _BundleCache = new Dictionary<string, AssetBundle>(StringComparer.OrdinalIgnoreCase);
        private List<BundleData> _Bundles;
        private readonly string _BaseUrl;
        private bool environmentLoadPopupShown;

        public BundleLoader(List<BundleData> bundles, string baseUrl)
        {
            _Bundles = bundles;
            _BaseUrl = baseUrl;
        }

		public IEnumerator GetBundleAsset<T>(EnvironmentBundleReference asset, string assetName, Action<T> callback) where T : UnityEngine.Object
        {
            if (!String.IsNullOrEmpty(asset.Url) && IsTypeOf(typeof(T), typeof(Texture)))
            {
                using (WWW www = new WWW(ProcessUrl(asset.Url)))
                {
                    yield return www;

                    if (www.error != null)
                    {
                        Debug.LogWarning(String.Format("WWW url {0} download had an error '{1}'", asset.Url, www.error));
                        yield break;
                    }

                    // texture size and format will be replaced when loaded

                    var unwrapper = TextureUnwrapperQueue.Instance.CreateTextureUnwrapper(www);
					if(assetName == "backRight_Panel_geo" || assetName == "backLeft_Panel_geo")
						unwrapper = TextureUnwrapperQueue.Instance.CreateTextureUnwrapper(www, false);
                    yield return unwrapper.WaitForUnwrapCompletion();

					// fix for nba desk textures, didn't want to effect anything else at this point, hence the hard coded stuff
//					if(asset.Name == "backLeft_Panel_geo" || asset.Name == "backRight_Panel_geo")
//					{
//						Debug.Log ("loading desk panels");
//					}

                    callback((T)(object)unwrapper.Texture);

                    yield break;
                }
            }

            if (String.IsNullOrEmpty(asset.Name))
                yield break;

            if (String.IsNullOrEmpty(asset.BundleID))
            {
                var result = Resources.Load<T>(asset.Name);
                callback(result);
                yield break;
            }

            AssetBundle bundle;
            if (!_BundleCache.TryGetValue(asset.BundleID, out bundle))
            {
                var bundleConfig = _Bundles.Find(b => String.Equals(b.ID, asset.BundleID, StringComparison.OrdinalIgnoreCase));

                if (bundleConfig == null)
                    yield break; // invalid bundle referenced by asset

                string suffix = null;

                if (bundleConfig.Suffix != null)
                {
#if UNITY_STANDALONE || UNITY_EDITOR
                    suffix = bundleConfig.Suffix.Standalone;
#elif UNITY_ANDROID
                    suffix = bundleConfig.Suffix.Mobile;
#endif
                }

                if (!String.IsNullOrEmpty(bundleConfig.Url))
                {
                    environmentLoadPopupShown = false;
                    float elapsedTime = 0f;

                    string url = ProcessUrl(bundleConfig.Url) + suffix;
                    // Load the AssetBundle file from Cache if it exists with the same version or download and store it in the cache


                    using (WWW www = WWW.LoadFromCacheOrDownload(url, bundleConfig.Version))
                    {
                        float lastProgress = 0;

                        while (!www.isDone)
                        {
                            elapsedTime += Time.deltaTime;
                            if (!environmentLoadPopupShown && elapsedTime >= 4f)
                            {
                                environmentLoadPopupShown = true;
                                PopupController.Instance.ShowPopup("\n \n \n \n The environment is still loading.", false);
                            }

                            if (www.progress - lastProgress >= .05)
                            {
                                lastProgress = www.progress;
                                //Debug.LogFormat("loading asset bundle {0}: {1:P}", asset.BundleID, www.progress);
                                //PopupController.Instance.SetImagePopupText(string.Format("\n \n \n \n Loading environment {0:P}", www.progress));
                            }
                            yield return null;
                        }

                        yield return www;
                        PopupController.Instance.HidePopup(false);

                        //if there is an error,
                        if (www.error != null)
                        {
                            Debug.LogError(String.Format("WWW asset bundle {0} download had an error '{1}'", asset.BundleID, www.error));
                            yield break;
                        }

                        bundle = www.assetBundle;
                    }
                }
                else if (!String.IsNullOrEmpty(bundleConfig.Path))
                {
                    string path = Path.Combine(Application.streamingAssetsPath, bundleConfig.Path + suffix);
                    var loadRequest = AssetBundle.LoadFromFileAsync(path);

                    while (!loadRequest.isDone)
                        yield return null;

                    bundle = loadRequest.assetBundle;
                }
                else
                {
                    Debug.LogError("Must specify either url or path for bundle id: " + asset.BundleID);
                    yield break;
                }

                if (bundle == null)
                    yield break;

                _BundleCache.Add(bundleConfig.ID, bundle);
            }

            var tex = bundle.LoadAsset<T>(asset.Name);
            callback(tex);
        }

        private string ProcessUrl(string url)
        {
            if (String.IsNullOrEmpty(url) || String.IsNullOrEmpty(_BaseUrl))
                return url;

            if (url.Contains(Uri.SchemeDelimiter))
                return url;

            return _BaseUrl + url;
        }

        private bool IsTypeOf(Type type, Type inherits)
        {
            return type == inherits || type.IsSubclassOf(inherits);
        }

        public void Dispose()
        {
            foreach (var bundle in _BundleCache.Values)
            {
                bundle.Unload(false);
            }
        }
    }

    /// <summary>
    /// Centralize how assets are found by tag
    /// </summary>
    private GameObject FindReferencedAsset(string key)
    {
        if (Targets == null)
            return null;

        foreach (var pair in Targets)
        {
            if (String.Equals(pair.Name, key, StringComparison.OrdinalIgnoreCase))
                return pair.Target;
        }

        return null;
    }

	private static Vector3 ParseVector3(string vectorValue)
    {
        string[] vectors = vectorValue.Split(',');

        Vector3 vector3 = new Vector3();
        if (vectors.Length > 0)
        {
            vector3.x = Convert.ToSingle(vectors[0]);
        }

        if (vectors.Length > 1)
        {
            vector3.y = Convert.ToSingle(vectors[1]);
        }

        if (vectors.Length > 2)
        {
            vector3.z = Convert.ToSingle(vectors[2]);
        }

        return vector3;
    }
}