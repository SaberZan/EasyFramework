using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Easy
{

    public class I18NText : EventMonoBehaviour
    {
        public string Key;

        private Text textContext;

        protected override void Awake()
        {
            base.Awake();
            textContext = this.transform.GetComponent<Text>();
        }

        private void OnEnable()
        {
            ChangeText();
        }

        [BindEvent(EventName.Event_Change_Language)]
        private void ChangeText()
        {
            textContext.text = I18NMgr.Instance.GetShowContextString(Key);
        }
    }

}
