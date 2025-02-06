using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Easy
{
    public class UISafeAreaLayout : MonoBehaviour
    {
        private float curTime = 0f;
        private ScreenOrientation preOrientation;
        private void Awake()
        {
            CheckSafeArea();
        }

        private void Update()
        {
            curTime += Time.deltaTime;
            if (curTime >= 0.2f)
            {
                CheckSafeArea();
                curTime = 0;
            }
        }

        private void CheckSafeArea()
        {
            if(preOrientation == Screen.orientation)
            {
                return;
            }
            preOrientation = Screen.orientation;
            RectTransform curTrans = transform.GetComponent<RectTransform>();
            if (curTrans != null)
            {
                if(Screen.orientation == ScreenOrientation.Portrait)
                {
                    float safeArea = Screen.safeArea.yMin;
                    if ((Screen.height * 1.0f / Screen.width) > (16 / 8.9f))
                    {
                        safeArea = Screen.height * 1 / 28f;
                    }
                    curTrans.offsetMin = Vector2.zero;
                    curTrans.offsetMax = new Vector2(0.0f, -safeArea);
                }
                else if(Screen.orientation == ScreenOrientation.PortraitUpsideDown)
                {
                    float safeArea = Screen.safeArea.yMin;
                    if ((Screen.height * 1.0f / Screen.width) > (16 / 8.9f))
                    {
                        safeArea = Screen.height * 1 / 28f;
                    }
                    curTrans.offsetMin = new Vector2(0.0f, safeArea);
                    curTrans.offsetMax = Vector2.zero;
                }
                else if(Screen.orientation == ScreenOrientation.LandscapeLeft)
                {
                    float safeArea = Screen.safeArea.xMin;
                    if ((Screen.width * 1.0f / Screen.height) > (16 / 8.9f))
                    {
                        safeArea = Screen.height * 1 / 28f;
                    }
                    curTrans.offsetMin = new Vector2(safeArea, 0);
                    curTrans.offsetMax = Vector2.zero;
                }
                else if(Screen.orientation == ScreenOrientation.LandscapeRight)
                {
                    float safeArea = Screen.safeArea.xMin;
                    if ((Screen.width * 1.0f / Screen.height) > (16 / 8.9f))
                    {
                        safeArea = Screen.height * 1 / 28f;
                    }
                    curTrans.offsetMin = Vector2.zero;
                    curTrans.offsetMax = new Vector2(-safeArea, 0);
                }
            }
        }
    }
}