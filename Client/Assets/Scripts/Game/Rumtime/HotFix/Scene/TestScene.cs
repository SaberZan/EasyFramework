using System.Collections;
using System.Collections.Generic;
using Easy;
using UnityEngine;

public class TestScene : MonoBehaviour
{
    // Start is called before the first frame update

    private GameModules gameModules;
    void Start()
    {
        Easy.UIMgr.Instance.GetLayer<ToastUILayer>().Toast("???????777777!!!");
        
        gameModules = new GameModules();
        gameModules.Start();
    }

    // Update is called once per frame
    void Update()
    {
        gameModules.Update(Time.deltaTime);
    }

    private void OnDestroy()
    {
        gameModules.Destory();
    }
}
