namespace Easy
{
    using System;

    public interface IAnalysis
    {
        void Init(InitCompleteCallback completeCallback);
        void SetAdvertiseId(string advertiseId);
        void SendEvent(string key, JSONObject param = null);
    }

}