namespace Easy
{

    using System;
    using System.Collections;
    using System.Collections.Generic;

    public class KeyData : SaveData
    {
        public SerializableDictionary<string, string> Keys = new SerializableDictionary<string, string>();

        public override void CopyValue(SaveData data)
        {
            if(data is KeyData keyData)
            {
                foreach (var key in Keys.Keys)
                {
                    if(!keyData.Keys.ContainsKey(key))
                    {
                        Keys.Remove(key);
                    }
                }

                foreach (var kv in keyData.Keys)
                {
                    if(Keys.ContainsKey(kv.Key))
                    {
                        Keys[kv.Key] = kv.Value;
                    }
                    else
                    {
                        Keys.Add(kv.Key, kv.Value);
                    }
                }
            }
        }
        
    }

}
