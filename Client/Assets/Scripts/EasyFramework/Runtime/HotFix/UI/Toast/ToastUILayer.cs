using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Easy
{
    [LayerParams(layerType = LayerType.TOP_LAYER, order = 100)]
    public class ToastUILayer : BaseUILayer
    {
        private GameObject _toastPrefab;        
        
        public override void Start()
        {
            base.Start();

            GameObject toast = new GameObject("ToastNode");
            Image black = toast.AddComponent<Image>();
            black.color = new Color(0, 0, 0, 0);

            GameObject text = new GameObject("Text");
            text.transform.SetParent(toast.transform, false);
            Text txt = text.AddComponent<Text>();
            txt.color = new Color(1, 1, 1, 0);
            txt.fontStyle = FontStyle.Normal;
            //txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            txt.alignment = TextAnchor.MiddleCenter;
            txt.fontSize = 40;
            txt.resizeTextForBestFit = true;
            txt.resizeTextMinSize = 36;
            txt.resizeTextMaxSize = 60;

            RectTransform rect = text.GetComponent<RectTransform>();
            rect.localPosition = Vector3.zero;
            rect.localScale = Vector3.one;
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, -40);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, -60);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            toast.GetComponent<RectTransform>().sizeDelta = new Vector2(900, 160);
            toast.GetComponent<RectTransform>().localPosition = new Vector2(0, 160);
            _toastPrefab = toast;
            
            GameObject.DontDestroyOnLoad(_toastPrefab);
        }
        
        /// <summary>
        /// Toast
        /// </summary>
        /// <param name="content"></param>
        /// <param name="time"></param>
        public void Toast(string content, float time = 2)
        {
            ShowToast(content, time);
        }

        /// <summary>
        /// ShowToast
        /// </summary>
        /// <param name="content"></param>
        /// <param name="time"></param>
        private async void ShowToast(string content, float time = 2)
        {
            GameObject toast = GameObjectPoolMgr.Instance.GetGameObject("ToastNode", _toastPrefab);
            toast.transform.SetParent(gameObject.transform, false);
            Image black = toast.GetComponent<Image>();
            black.color = new Color(0, 0, 0, 0);
            Text txt = toast.GetComponentInChildren<Text>();
            txt.text = content;
            await UniTask.Delay((int)(time * 1000));
            GameObjectPoolMgr.Instance.PutGameObject("ToastNode", toast);
        }
    }
}