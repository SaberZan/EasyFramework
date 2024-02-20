using UnityEngine;

namespace Easy
{
    public interface WeightUIInterface
    {

    }

    public class WeightUI<T> : BaseUI where T : WeightUIInterface, new()
    {
        public T weightUIInterface;

        public WeightUI()
        {
            weightUIInterface = new T();
        }

        public WeightUI(string prefabPath)
        {
            weightUIInterface = new T();
            SetPrefabPath(prefabPath);
        }

        public WeightUI(GameObject gameObject)
        {
            weightUIInterface = new T();
            SetGameObject(gameObject);
        }
    }
}