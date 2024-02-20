using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Easy
{

    [NormalInit]
    [OrderIndex((int)NormalInitOrderIndexEnum.QualityMgr)]
    public class QualityMgr : Singleton<QualityMgr>
    {
        public override void BeforeRestart()
        {

        }

        public override void Init(InitCompleteCallback complete)
        {

#if UNITY_ANDROID
            List<float> qualityParams = new List<float>()
            {
                SystemInfo.processorCount * 1.0f / 8,
                SystemInfo.processorFrequency * 1.0f / 3000,
                SystemInfo.systemMemorySize * 1.0f / 8000
            };
            List<float> qualityWeight = new List<float>()
            {
                1,
                1,
                0.5f
            };

            float score = 0;
            float allWeight = 0;
            for (int i = 0; i < qualityParams.Count; ++i)
            {
                score += qualityParams[i] * qualityWeight[i];
                allWeight += qualityWeight[i];
            }
            int level = 0;
            if (score >= allWeight * 0.95f)
                level = 4;
            else if (score >= allWeight * 0.9f)
                level = 3;
            else if (score >= allWeight * 0.8f)
                level = 2;
            else if (score >= allWeight * 0.7f)
                level = 1; 
            QualitySettings.SetQualityLevel(level, true);

#endif

#if UNITY_IOS
            int level = 0;
            if (SystemInfo.systemMemorySize >= 5400)
                level = 4;
            else if (SystemInfo.systemMemorySize >= 2700)
                level = 3;
            else if (SystemInfo.systemMemorySize >= 1800)
                level = 2;
            else if (SystemInfo.systemMemorySize >= 900)
                level = 1;
            QualitySettings.SetQualityLevel(level, true);
#endif
            EasyLogger.Log("Quality = " + QualitySettings.GetQualityLevel());
            complete.Invoke(true);
        }

        public int GetQualityLevel()
        {
            return QualitySettings.GetQualityLevel();
        }

        public string GetQualityName()
        {
            return QualitySettings.names[QualitySettings.GetQualityLevel()];
        }

        public void SetQualityLevel(int level)
        {
            QualitySettings.SetQualityLevel(level, true);
        }

        public bool IsOpenDepth()
        {
            return true; // QualitySettings.GetQualityLevel() >= 2;
        }

        public bool IsOpenParticle()
        {
            return QualitySettings.GetQualityLevel() >= 1;
        }
    }
}
