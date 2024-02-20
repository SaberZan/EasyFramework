namespace Easy
{

    using System.Collections.Generic;

    /// <summary>
    /// 有限状态机
    /// </summary>
    public class FMSMachine
    {
        /// <summary>
        /// 状态机所控制的状态对象字典
        /// </summary>
        public Dictionary<int, StateBase> stateCache;

        /// <summary>
        /// 前置状态
        /// </summary>
        public StateBase prviousState;

        /// <summary>
        /// 当前状态
        /// </summary>
        public StateBase currentState;

        /// <summary>
        /// 带参数构造
        /// </summary>
        /// <param name="beginState">开始状态对象</param>
        public FMSMachine(StateBase beginState)
        {
            prviousState = null;
            currentState = beginState;

            stateCache = new Dictionary<int, StateBase>();
            //把状态添加到集合中
            AddState(beginState);
            currentState.OnEnter();
        }

        /// <summary>
        /// 无参数构造
        /// </summary>
        public FMSMachine()
        {
            prviousState = null;
            currentState = null;
            stateCache = new Dictionary<int, StateBase>();
        }

        /// <summary>
        /// 给状态机添加状态
        /// </summary>
        /// <param name="state">待添加的状态</param>
        public void AddState(StateBase state)
        {
            if (!stateCache.ContainsKey(state.ID))
            {
                stateCache.Add(state.ID, state);
                state.fmsMachine = this;
            }
        }

        /// <summary>
        /// 获得状态机的状态类
        /// </summary>
        /// <typeparam name="T">状态类</typeparam>
        /// <param name="state">状态ID</param>
        /// <returns></returns>
        public T GetState<T>(int state) where T : StateBase
        {
            stateCache.TryGetValue(state, out StateBase tmp);
            return (T) tmp;
        }

        /// <summary>
        /// 切换状态，通过id
        /// </summary>
        /// <param name="id">状态id</param>
        public void TranslateState(int id)
        {
            if (!stateCache.ContainsKey(id))
            {
                return;
            }

            // 从当前状态离开
            if (stateCache[id] != currentState)
                prviousState = currentState;
            if (currentState != null)
                currentState.OnExit();
            // 设置新当前状态，并进入
            currentState = stateCache[id];
            currentState.OnEnter();
        }

        #region 当前状态数据获取

        /// <summary>
        /// 获取当前状态ID
        /// </summary>
        /// <returns>当前状态id</returns>
        public int GetCurrentStateID()
        {
            return currentState.ID;
        }

        /// <summary>
        /// 获取当前状态名字
        /// </summary>
        /// <returns>状态名字</returns>
        public string GetCurrentStateName()
        {
            return currentState.StateName;
        }

        #endregion

        #region 前置状态数据获取

        /// <summary>
        /// 获取前一个状态ID
        /// </summary>
        /// <returns>状态id</returns>
        public int GetPrviousStateID()
        {
            return prviousState.ID;
        }

        /// <summary>
        /// 获取前一个状态名字
        /// </summary>
        /// <returns>状态名字</returns>
        public string GetPrviousStateName()
        {
            return prviousState.StateName;
        }

        #endregion

        /// <summary>
        /// 刷新状态机
        /// </summary>
        public void Update()
        {
            if (currentState != null)
            {
                currentState.OnStay();
            }
        }
    }
}