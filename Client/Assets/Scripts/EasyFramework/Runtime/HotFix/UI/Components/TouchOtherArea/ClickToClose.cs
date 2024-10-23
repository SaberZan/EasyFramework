using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Easy
{
    public class TouchOtherArea : MonoBehaviour
    {
        public GraphicRaycaster raycaster;
        
        public UnityEngine.EventSystems.EventSystem eventSystem;

        public List<RectTransform> ingnoreRects = new List<RectTransform>();

        private Action _action;
        
        private Camera _camera;

        private void Awake()
        {
            ingnoreRects.Add(gameObject.transform as RectTransform);
        }

        public TouchOtherArea SetCamera(Camera camera)
        {
            _camera = camera;
            return this;
        }

        public TouchOtherArea SetClickAction(Action action)
        {
            _action = action;
            return this;
        }

        public TouchOtherArea AddIngnoreRect(RectTransform rectTransform)
        {
            ingnoreRects.Add(rectTransform);
            return this;
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 mousePos = Input.mousePosition;
                bool touchIngnore = false;
                foreach(RectTransform tf in ingnoreRects)
                {   
                    if (RectTransformUtility.RectangleContainsScreenPoint(tf, mousePos, _camera))
                    {
                        touchIngnore = true;
                    }
                }
                if(!touchIngnore)
                {
                    _action.DynamicInvoke();
                }
            }

            if(Input.touchCount > 0)
            {
                bool touchIngnore = false;
                foreach(Touch touch in Input.touches)
                {
                    if(touch.phase == TouchPhase.Began)
                    {
                        Vector2 touchPos = touch.position;
                        foreach(RectTransform tf in ingnoreRects)
                        {   
                            if (RectTransformUtility.RectangleContainsScreenPoint(tf, touchPos, _camera))
                            {
                                touchIngnore = true;
                                break;
                            }
                        }
                    }
                    if(touchIngnore)
                    {
                        break;
                    }
                }
                if(!touchIngnore)
                {
                    _action.DynamicInvoke();
                }
            }
        }
        
    }
}