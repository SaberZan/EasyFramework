using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Easy
{
    public class UISafeAreaLayout : MonoBehaviour
    {
        private void Awake()
        {
            RectTransform curTrans = transform.GetComponent<RectTransform>();
            if (curTrans != null)
            {
                float safeArea = Screen.safeArea.yMin;
                if ((Screen.height * 1.0f / Screen.width) > (16 / 8.9f))
                {
                    safeArea = Screen.height * 1 / 28f;
                }
                curTrans.offsetMax = new Vector2(0.0f, -safeArea);
            }
        }
    }
}