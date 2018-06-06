using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class UserPresenceController : Singleton<UserPresenceController>
{
    public bool IsMounted
    {
        get
        {
#if UNITY_EDITOR
            return !Input.GetKey(KeyCode.ScrollLock);
#else

			if(OVRManager.instance != null) {
				return OVRManager.instance.isUserPresent;
			}
			return true;
#endif
        }
    }

    private bool _WasMounted;

    private void Awake()
    {
        _WasMounted = IsMounted;
    }

    private void Update()
    {
        bool isMounted = IsMounted;
        if (_WasMounted != isMounted)
        {
            _WasMounted = isMounted;

            var ev = PresenceChanged;
            if (ev != null)
            {
                var args = new UserPresenceEventArgs(isMounted);
                ev(this, args);
            }
        }
    }

    public event EventHandler<UserPresenceEventArgs> PresenceChanged;
}