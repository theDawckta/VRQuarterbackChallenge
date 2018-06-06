using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class PersistentObject : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
    }
}