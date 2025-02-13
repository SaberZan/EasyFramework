﻿using UnityEngine;

namespace Easy
{
    public class InfiniteScrollItem : MonoBehaviour
    {
        private int _x;
        private int _y;
       [SerializeField] private int _index;
       [SerializeField] private int _objIndex;
        private bool _isTop;

        public int x { get { return _x; } }
        public int y { get { return _y; } }
        /// <summary>
        /// ScrollView滑动时，根据所显示数据而刷新变化的数据索引index，不断变化。
        /// </summary>
        public int index { get { return _index; } }
        /// <summary>
        /// 每个克隆出来的cell的GameObject所标记的唯一且固定的objIndex，确定后不再变化。
        /// </summary>
        public int objIndex { get { return _objIndex; } }

        public bool isTop { get { return _isTop;  } set { _isTop = value;  } }

        /// <summary>
        /// 更新cell所滑动到的新的位置。
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="index"></param>
        public void UpdatePos(int x, int y, int index)
        {
            this._x = x;
            this._y = y;
            this._index = index;
        }
        public void SetObjIndex(int objIndex) {
            this._objIndex = objIndex;
        }
    }
}
