using UnityEngine;

namespace Easy
{
    
    public delegate void UpdateCallBack(float detailTime);
    /// <summary>
    /// 单例Update
    /// </summary>
    public class SingletonUpdateMonoBehaviour : MonoBehaviour
    {

        public UpdateCallBack updateCallBack;
        private void Update()
        {
            updateCallBack?.Invoke(Time.deltaTime);
        }
    }
}