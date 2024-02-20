namespace Easy
{

    using UnityEngine;


    /// <summary>
    /// 状态基类
    /// 包含状态基础数据和属性
    /// </summary>
    public class StateBase
    {
        /// <summary>
        /// 每个状态设置一个id
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// 状态名称
        /// </summary>
        public string StateName { get; set; }

        /// <summary>
        /// 控制此状态的机器
        /// </summary>
        public FMSMachine fmsMachine;

        public StateBase(int id, string stateName)
        {
            this.ID = id;
            this.StateName = stateName;
        }

        //给子类提供的方法

        #region 虚函数

        /// <summary>
        /// 进入状态
        /// </summary>
        /// <param name="args">进入参数</param>
        public virtual void OnEnter(params object[] args)
        {
            //Debuger.Log(this.StateName + " Enter");
        }

        /// <summary>
        /// 停留在状态
        /// </summary>
        /// <param name="args">停留参数</param>
        public virtual void OnStay(params object[] args)
        {
            // Debuger.Log(this.StateName + " Stay");
        }

        /// <summary>
        /// 离开状态
        /// </summary>
        /// <param name="args">离开参数</param>
        public virtual void OnExit(params object[] args)
        {
            //Debuger.Log(this.StateName + " Exit");
        }

        #endregion
    }

}

