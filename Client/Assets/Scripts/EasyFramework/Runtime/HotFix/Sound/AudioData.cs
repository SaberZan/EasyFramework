namespace Easy
{
    public class AudioSaveData : SaveData
    {
        public bool bgmSwitch;
        public bool effectSwitch;

        public override void CopyValue(SaveData data)
        {
            if(data is AudioSaveData audioSaveData)
            {
                bgmSwitch = audioSaveData.bgmSwitch;
                effectSwitch = audioSaveData.effectSwitch;
            }
        }
    }
}