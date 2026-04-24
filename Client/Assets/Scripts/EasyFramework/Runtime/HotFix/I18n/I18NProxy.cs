using System;
using UnityEngine;

namespace Easy
{
    public class I18NProxy : Proxy
    {

        public void Init(Func<string> getDefaultLanguageCode)
        {
            if(!SaveDataMgr.Instance.Get<I18NSaveData>().changed)
            {
                SaveDataMgr.Instance.Get<I18NSaveData>().selectLanguageCode = getDefaultLanguageCode();
                SaveDataMgr.Instance.Get<I18NSaveData>().changed = true;
            }
        }

        public void ChangeLanguageCode(string code)
        {
            SaveDataMgr.Instance.Get<I18NSaveData>().selectLanguageCode = code;
            EventMgr.Instance.DispatchEvent(EventName.Event_Change_Language);
        }
        
        public string GetLanguageCode()
        {
            return SaveDataMgr.Instance.Get<I18NSaveData>().selectLanguageCode;
        }
    }
}