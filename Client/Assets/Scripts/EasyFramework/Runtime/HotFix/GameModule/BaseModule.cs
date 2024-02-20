using Easy;

public abstract class BaseModule 
{

    public abstract string GetName();

    public virtual void Start()
    {
        EventMgr.Instance.SubscribeByTarget(this);
    }

    public abstract void Update(float detailTime);

    public virtual void Destory() 
    {
        EventMgr.Instance.UnSubscribeByTarget(this);
    }
}