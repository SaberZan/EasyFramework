namespace Easy
{

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [OrderIndex(1)]
    public class PrimaryData : SaveData
    {
        public string version = "1.0.0";

        public string deviceId = "";

        public string userId = "";

        public long time = 0;

        public override void CopyValue(SaveData data)
        {
            if(data is PrimaryData primaryData)
            {
                version = primaryData.version;
                deviceId = primaryData.deviceId;
                userId = primaryData.userId;
                time = primaryData.time;
            }
        }
    }

}