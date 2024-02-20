using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Easy
{
    public class I18NSprite : EventMonoBehaviour
    {
        
        public string key;

        private Image ImageContext;

        private ISingleUnityAssetHandle<Sprite> _handle;

        protected override void Awake()
        {
            base.Awake();
            ImageContext = this.transform.GetComponent<Image>();
        }

        private void OnEnable()
        {
            ChangeSprite();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_handle != null)
            {
                AssetsMgr.Instance.Release(_handle);
            }
        }
        
        [BindEvent(EventName.Event_Change_Language)]
        private void ChangeSprite()
        {
            string context = I18NMgr.Instance.GetShowContextString(key);
            _handle = AssetsMgr.Instance.LoadAsset<Sprite>(context);
            ImageContext.sprite = _handle.GetResult() as Sprite;
            ImageContext.SetNativeSize();
        }
    }

}
