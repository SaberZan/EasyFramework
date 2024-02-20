namespace Easy
{
    public class I18NProxy : Proxy
    {
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