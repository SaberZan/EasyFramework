namespace Easy
{

    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class PatchBase
    {
        public virtual string patchVersion => "1.0.0";

        /// <summary>
        /// 针对 patchVersion 的补丁
        /// </summary>
        public virtual void AddPatch()
        {
            EasyLogger.Log("patch : " + patchVersion);
        }

    }

}
