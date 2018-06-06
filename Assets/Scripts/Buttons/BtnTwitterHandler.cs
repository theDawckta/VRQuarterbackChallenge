using System;
using System.Collections;
using UnityEngine;

public class BtnTwitterHandler : MonoBehaviour
{
    private Interactible _Interactible;
    public GameObject TwitterController;

    void Awake()
    {
        _Interactible = GetComponent<Interactible>();

        if (_Interactible == null)
            throw new Exception("An _Interactible must exist on this button.");
    }

    void OnEnable()
    {
        _Interactible.OnClick += OnTwitterBtnClick;
    }

    void OnDisable()
    {
        _Interactible.OnClick -= OnTwitterBtnClick;
    }

    void OnTwitterBtnClick()
    {
        TwitterController.SetActive(!TwitterController.activeSelf);
    }

    //trigger the back button click via Gear VR hardware button
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            EventManager.Instance.BackBtnClickEvent();
        }
    }

    public void Enable()
    {
        _Interactible.Enable();
    }

    public void Disable()
    {
        _Interactible.Disable();
    }
}



