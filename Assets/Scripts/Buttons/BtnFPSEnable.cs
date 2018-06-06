using System;
using System.Collections;
using UnityEngine;

public class BtnFPSEnable : MonoBehaviour
{
    public GameObject FPS;

    private Interactible _Interactible;

    void Awake()
    {
        _Interactible = GetComponent<Interactible>();

        if (_Interactible == null)
            throw new Exception("An _Interactible must exist on this button.");
    }

    void OnEnable()
    {
        _Interactible.OnClick += OnFPSClick;
    }

    void OnDisable()
    {
        _Interactible.OnClick -= OnFPSClick;
    }

    void OnFPSClick()
    {
        if (FPS != null)
        {
            Debug.Log("FPS.activeInHierarchy: " + FPS.activeInHierarchy);
            if (FPS.activeInHierarchy)
            {
                FPS.SetActive(false);
            }
            else
            {
                FPS.SetActive(true);
            }
        }
    }
}