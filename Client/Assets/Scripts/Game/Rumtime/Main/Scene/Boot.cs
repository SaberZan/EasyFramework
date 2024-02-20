using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Linq;

namespace Easy
{

    public class Boot : MonoBehaviour
    {
        void Start()
        {
            SceneManager.LoadScene("Launch");
        }
    }

}
