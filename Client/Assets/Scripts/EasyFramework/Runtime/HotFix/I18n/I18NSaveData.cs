namespace Easy
{
    public class I18NSaveData : SaveData
    {
        public string selectLanguageCode = "en";

        public bool changed = false;

        public override void CopyValue(SaveData data)
        {
            if(data is I18NSaveData i18NSaveData)
            {
                selectLanguageCode = i18NSaveData.selectLanguageCode;
                changed = i18NSaveData.changed;
            }
        }
    }
}