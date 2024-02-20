namespace Easy
{

    /// <summary>
    /// 状态模板
    /// </summary>
    /// <typeparam name="T">拥有者类型</typeparam>
    public class StateTemplate<T> : StateBase
    {
        //拥有者(范型)
        public T owner;

        public StateTemplate(int id, string stateName, T o)
            : base(id, stateName)
        {
            owner = o;
        }
    }

}
