using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// Apply to an object you want to feed a WWW URL or resource path to
/// </summary>
public class DynamicTexture : MonoBehaviour
{
	[HideInInspector]
	public string ID;

    private string _TextureURL;
    private float _AnimSpeed = 0.5f;
	private float _AnimOutSpeed = 0.2f;
	private float _FadeInAlphaTarget = 1.0f;
    private CanvasGroup uiImage;
	private IEnumerator LoadFunct;


    private void Awake()
    {
        if (gameObject.GetComponent<CanvasGroup>())
            uiImage = gameObject.GetComponent<CanvasGroup>();
    }

	public void SetTexture(string url, bool isUI=false, System.Action callback = null)
    {
        _TextureURL = url;
		if(LoadFunct != null)
			StopCoroutine (LoadFunct);

		LoadFunct = LoadTexture (isUI, callback);
		StartCoroutine(LoadFunct);
    }

	IEnumerator LoadTexture(bool isUI=false, System.Action callback = null)
    {
    	//gameObject.GetComponent<Renderer>().material.DOFade(0.0f, 0.0f);
		if (_TextureURL.IndexOf("http://") != -1 || _TextureURL.IndexOf("https://") != -1|| _TextureURL.IndexOf("file://") != -1)
        {
            // Start a download of the given URL
            WWW www = new WWW(_TextureURL);

            // wait until the download is done
            yield return www;
			if (www.error == null && www.responseHeaders != null)
			{

				var unwrapper = TextureUnwrapperQueue.Instance.CreateTextureUnwrapper (www);
				yield return unwrapper.WaitForUnwrapCompletion ();

				// assign the downloaded image to sprite

				if (!isUI)
				{
					gameObject.GetComponent<Renderer> ().material.mainTexture = unwrapper.Texture;
				} else
				{
					gameObject.GetComponent<RawImage> ().texture = unwrapper.Texture;
					//gameObject.GetComponent<RawImage> ().texture.filterMode = FilterMode.Point;
				}

				if (callback != null)
					callback ();

				AnimIn (isUI);
			} else
			{
				if (callback != null)
					callback ();
			}
        }
        else
        {
            //try to pull from local
            Texture newTexture = Resources.Load(_TextureURL) as Texture;
            if (newTexture != null)
            {
                if(!isUI)
                    gameObject.GetComponent<Renderer>().material.mainTexture = newTexture;
                else
                    gameObject.GetComponent<RawImage>().texture = newTexture;

				if (callback != null)
					callback ();
                AnimIn(isUI);
            }
        }
    }

    void AnimIn(bool isUI)
    {
		if (!isUI){
			Material mat = gameObject.GetComponent<Renderer> ().material;
			if(mat.HasProperty("_Color"))
			{
				DOTween.Kill (mat);
				mat.DOFade(_FadeInAlphaTarget, _AnimSpeed);
			}
		}else
        {
			if (uiImage != null)
			{
				DOTween.Kill (uiImage);
				uiImage.DOFade (_FadeInAlphaTarget, _AnimSpeed);
			}
        }
    }

	/// <summary>
	/// We want to animate out a texture before loading one in
	/// </summary>
	/// <param name="url">URL to load.</param>
	/// <param name="isUI">If set to <c>true</c> is U.</param>
	public void AnimOut(string url, bool isUI=false)
	{
		if (!isUI){
			Material mat = gameObject.GetComponent<Renderer> ().material;
			if(mat.HasProperty("_Color"))
			{
				DOTween.Kill (mat);
				mat.DOFade(0, _AnimOutSpeed).OnComplete(()=>SetTexture(url, isUI));
			}
		}else
		{
			if(uiImage!=null)
			{
				DOTween.Kill (uiImage);
				uiImage.DOFade(0, _AnimOutSpeed).OnComplete(()=>SetTexture(url, isUI));
			}
		}
	}

    public float Alpha
    {
        get { return uiImage.alpha; }
        set { uiImage.alpha = value; }
    }

	public void SetFadeInAlphaTarget(float alphaTarget)
	{
		_FadeInAlphaTarget = alphaTarget;
	}
}
