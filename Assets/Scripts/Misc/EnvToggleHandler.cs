using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnvToggleHandler : MonoBehaviour
{
    private Interactible toggleBtn;
    private DataCursor _cursor;

    private void Awake()
    {
        toggleBtn = GetComponent<Interactible>();
    }

    private void OnEnable()
    {
        toggleBtn.OnClick += ToggleEnvironment;
    }

    private void OnDisable()
    {
        toggleBtn.OnClick -= ToggleEnvironment;
    }

    private void ToggleEnvironment()
    {
        //StartCoroutine(SetTargetURL());
        ToogleAppConfigEndpoint();
    }

    void ToogleAppConfigEndpoint()
    {
        //Debug.Log("TOGGLING ASSETS ENDPOINT: ");

        string assetsEndpoint = AppConfigurationLoader.Instance.FileUrl;

        if (assetsEndpoint.Contains("-Staging-"))
        {
            assetsEndpoint = assetsEndpoint.Remove(assetsEndpoint.IndexOf("-Staging"), 8);
        }
        else
        {
            assetsEndpoint = assetsEndpoint.Replace("AppConfig-", "AppConfig-Staging-");
        }

        //Debug.Log("Toggling to " + assetsEndpoint);
        AppConfigurationLoader.Instance.FileUrl = assetsEndpoint;
        AppConfigurationLoader.Instance.SetDataLoaded(false);
        SceneManager.LoadScene("TransitionScene");
    }
}

