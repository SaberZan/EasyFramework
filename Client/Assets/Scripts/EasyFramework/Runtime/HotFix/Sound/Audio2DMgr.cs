namespace Easy
{

    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// 简单音效
    /// 3D音效请自行添加节点控制
    /// </summary>
    public class Audio2DMgr : Singleton<Audio2DMgr>
    {
        private Dictionary<int, AudioSource> _audioEffects;
        private AudioSource _audioBGM;
        private GameObject _audioRoot;
        private Dictionary<string, ISingleUnityAssetHandle<AudioClip>> _assetHandles = new Dictionary<string, ISingleUnityAssetHandle<AudioClip>>();

        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        public override void Init(InitCompleteCallback complete)
        {
            _audioRoot = new GameObject("Audio");
            Object.DontDestroyOnLoad(_audioRoot);
            //预加载5个
            _audioEffects = new Dictionary<int, AudioSource>();
            for (var i = 1; i <= 5; i++)
            {
                AddEffectNode(i);
            }

            //背景音乐唯一
            GameObject bgmNode = new GameObject("BGM");
            _audioBGM = bgmNode.AddComponent<AudioSource>();
            _audioBGM.playOnAwake = false;
            _audioBGM.loop = true;
            bgmNode.transform.parent = _audioRoot.transform;
            complete.Invoke(true);
        }

        public override void BeforeRestart()
        {
            foreach (var kv in _assetHandles)
            {
                Easy.AssetsMgr.Instance.Release(kv.Value);
            }
            _assetHandles.Clear();
            Object.Destroy(_audioRoot);
        }

        /// <summary>
        /// 添加一个音效节点
        /// </summary>
        /// <param name="id"></param>
        private void AddEffectNode(int id)
        {
            GameObject effectNode = new GameObject("Effect");
            AudioSource audioEffect = effectNode.AddComponent<AudioSource>();
            audioEffect.playOnAwake = false;
            _audioEffects.Add(id, audioEffect);
            effectNode.transform.parent = _audioRoot.transform;
        }

        /// <summary>
        /// 播放音效
        /// </summary>
        public int PlayEffect(string path, bool loop = false)
        {
            if(!ProxyMgr.Instance.Get<AudioProxy>().GetEffectSwitch())
            {
                return 0;
            }
            if (string.IsNullOrEmpty(path))
                return 0;

            int sourceId = 0;
            for (int i = 1, count = _audioEffects.Count; i <= count; i++)
            {
                if (!_audioEffects[i].isPlaying)
                {
                    sourceId = i;
                    break;
                }
            }

            //没有闲置的节点
            if (sourceId == 0)
            {
                sourceId = _audioEffects.Count + 1;
                AddEffectNode(sourceId);
            }

            if (!_assetHandles.ContainsKey(path))
            {
                var handle = Easy.AssetsMgr.Instance.LoadAsset<AudioClip>(path);
                _assetHandles.Add(path, handle);
            }
            AudioClip audioClip = _assetHandles[path].GetResult();
            _audioEffects[sourceId].clip = audioClip;
            _audioEffects[sourceId].loop = loop;
            _audioEffects[sourceId].Play();
            return sourceId;
        }

        /// <summary>
        /// 停止音效
        /// </summary>
        public void StopEffect(int sourceId)
        {
            if (_audioEffects.ContainsKey(sourceId)) _audioEffects[sourceId].Stop();
        }

        /// <summary>
        /// 播放背景音乐
        /// </summary>
        /// <param name="path"></param>
        public void PlayBGM(string path)
        {
            if(!ProxyMgr.Instance.Get<AudioProxy>().GetBGMSwitch())
            {
                return;
            }
            if (!_assetHandles.ContainsKey(path))
            {
                var handle = Easy.AssetsMgr.Instance.LoadAsset<AudioClip>(path);
                _assetHandles.Add(path, handle);
            }
            AudioClip audioClip = _assetHandles[path].GetResult();
            _audioBGM.clip = audioClip;
            _audioBGM.Play();
        }

        /// <summary>
        /// 停止背景音乐
        /// </summary>
        public void StopBGM()
        {
            if (_audioBGM != null)
                _audioBGM.Stop();
        }

        /// <summary>
        /// 设置音效静音
        /// </summary>
        /// <param name="mute"></param>
        public void SetEffectSwitch(bool mute)
        {
            for (int i = 1, count = _audioEffects.Count; i <= count; i++)
            {
                _audioEffects[i].mute = mute;
            }
            ProxyMgr.Instance.Get<AudioProxy>().SetEffectSwitch(mute);
        }

        /// <summary>
        /// 设置BGM静音
        /// </summary>
        /// <param name="mute"></param>
        public void SetBGMSwitch(bool mute)
        {
            _audioBGM.mute = mute;
            ProxyMgr.Instance.Get<AudioProxy>().SetBGMSwitch(mute);
        }
    }
}