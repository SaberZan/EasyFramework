using System.Collections.Generic;
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