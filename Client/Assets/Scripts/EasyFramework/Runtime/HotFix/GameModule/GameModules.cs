using System.Collections.Generic;
namespace Easy
{
    public class GameModules 
    {

        private Dictionary<string, BaseModule> _modules = new Dictionary<string, BaseModule>();

        public void Register(BaseModule module)
        {
            this._modules.Add(module.GetName(), module);
        }

        public void UnRegister(BaseModule module)
        {
            this._modules.Remove(module.GetName());
        }
        
        public void UnRegister(string moduleName)
        {
            this._modules.Remove(moduleName);
        }

        public T GetInterface<T>(string moduleName) where T : IModuleInterface, new()
        {
            if (this._modules.ContainsKey(moduleName))
            {
                return (T)this._modules[moduleName].moduleInterface;
            }
            return default(T);
        }

        public void Start()
        {
            foreach (var kv in this._modules)
            {
                kv.Value.Start();
            }
        }

        public void Update(float detailTime)
        {
            foreach (var kv in this._modules)
            {
                kv.Value.Update(detailTime);
            }
        }

        public void Destory() 
        {
            foreach (var kv in this._modules)
            {
                kv.Value.Destory();
            }
            _modules.Clear();
        }

    }
}