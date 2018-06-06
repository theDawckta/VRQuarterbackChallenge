using System;
using UnityEngine;

public class Interactible : MonoBehaviour
{
    public event Action OnOver;
    public event Action OnOut;
    public event Action OnClick;
    public event Action OnDoubleClick;
    public event Action OnUp;
    public event Action OnDown;
    public bool GazeSelectable = false;
    public bool UseGazeTime = false;
    public float GazeTime = 1.0f;
    public float GazeDelay = 0f;
    public bool TriggerImmediateClick = false;
    public bool TriggerReticleLoader = false;

    private Collider hitBox;

    protected bool _isOver;

    void Awake()
    {
        DoAwake();
    }

    protected void DoAwake()
    {
        hitBox = gameObject.GetComponent<Collider>();
    }

    public void ResizeCollider(Vector2 newsize)
    {
        if (hitBox != null)
            if (hitBox.GetType() == typeof(BoxCollider))
                ((BoxCollider)hitBox).size = new Vector3(newsize.x, newsize.y, ((BoxCollider)hitBox).size.z);
    }

    public bool IsOver
    {
        get { return _isOver; }
    }

    public void Over()
    {
        _isOver = true;

        if (OnOver != null)
            OnOver();
    }

    public void Out()
    {
        _isOver = false;

        if (OnOut != null)
            OnOut();
    }

    public void Click()
    {
        if (OnClick != null)
        {
            OnClick();
        }
    }

    public void DoubleClick()
    {
        if (OnDoubleClick != null)
            OnDoubleClick();
    }

    public void Up()
    {
        if (OnUp != null)
            OnUp();
    }

    public void Down()
    {
        if (OnDown != null)
            OnDown();
    }

    public void Enable()
    {
        if (hitBox != null)
            hitBox.enabled = true;
    }

    public void Disable()
    {
        if (hitBox != null)
            hitBox.enabled = false;
    }
}