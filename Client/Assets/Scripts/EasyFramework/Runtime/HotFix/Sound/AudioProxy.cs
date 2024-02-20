namespace Easy
{
    public class AudioProxy : Proxy
    {
        /// <summary>
        /// 设置音效开关
        /// </summary>
        /// <param name="value"></param>
        public void SetEffectSwitch(bool value)
        {
            SaveDataMgr.Instance.Get<AudioSaveData>().effectSwitch = value;
        }

        /// <summary>
        /// 获得音效开关
        /// </summary>
        /// <param name="value"></param>
        public bool GetEffectSwitch()
        {
            return SaveDataMgr.Instance.Get<AudioSaveData>().effectSwitch;
        }

        /// <summary>
        /// 设置BGM开关
        /// </summary>
        /// <param name="mute"></param>
        public void SetBGMSwitch(bool value)
        {
            SaveDataMgr.Instance.Get<AudioSaveData>().bgmSwitch = value;
        }

        /// <summary>
        /// 获得BGM开关
        /// </summary>
        /// <param name="mute"></param>
        public bool GetBGMSwitch()
        {
            return SaveDataMgr.Instance.Get<AudioSaveData>().bgmSwitch;
        }
    }
}