using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Easy
{
    public class BaseUILayer : BaseUI
    {
        private List<object> _uIReference = new List<object>();

        public virtual void ShowLayer(object reference)
        {
            _uIReference.Add(reference);
            if(_uIReference.Count == 1)
            {
                if(gameObject != null)
                {
                    CanvasGroup canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();
                    canvasGroup.alpha = 1;
                    canvasGroup.interactable = true;
                    canvasGroup.blocksRaycasts = true;
                }
                Show();
            }
        }

        public virtual void HideLayer(object reference)
        {
            _uIReference.Remove(reference);
            if(_uIReference.Count == 0)
            {
                if(gameObject != null)
                {
                    CanvasGroup canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();
                    canvasGroup.alpha = 0;
                    canvasGroup.interactable = false;
                    canvasGroup.blocksRaycasts = false;
                }
                Hide();
            }
        }

        public override void OnDestroy()
        {
            _uIReference.Clear();
            base.Destroy();
        }
    }
}