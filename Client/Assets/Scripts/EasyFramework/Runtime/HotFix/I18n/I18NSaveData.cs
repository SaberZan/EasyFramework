namespace Easy
{
    public class I18NSaveData : SaveData
    {
        public string selectLanguageCode = "en";

        public override void CopyValue(SaveData data)
        {
            if(data is I18NSaveData i18NSaveData)
            {
                selectLanguageCode = i18NSaveData.selectLanguageCode;
            }
        }
    }
}