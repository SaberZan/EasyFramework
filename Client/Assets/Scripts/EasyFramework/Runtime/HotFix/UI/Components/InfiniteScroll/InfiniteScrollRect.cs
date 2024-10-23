using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;

namespace Easy
{
    public delegate void OnBeginDrag();
    public delegate void OnDrag();
    public delegate void OnEndDrag();

    public class InfiniteScrollRect : ScrollRect, IPointerDownHandler, IPointerUpHandler
    {
        /// <summary>
        /// 相关联的 item 对象
        /// </summary>
        [SerializeField] private GameObject m_Item;
        public GameObject Item
        {
            get => m_Item;
            set => m_Item = value;
        }

        /// <summary>
        /// xlua会忽略Item属性，需要加一个和Item属性一样的属性
        /// </summary>
        public GameObject ItemGameObject
        {
            get => m_Item;
            set => m_Item = value;
        }

        /// <summary>
        /// 起始 item 距离 边框的距离
        /// </summary>
        [SerializeField] private Vector2 m_Padding;

        public Vector2 Padding
        {
            get { return m_Padding; }
            set { m_Padding = value; }
        }

        /// <summary>
        /// 起始 item 距离 边框的距离
        /// </summary>
        [SerializeField] private Vector2 m_RightPadding;

        public Vector2 RightPadding
        {
            get { return m_RightPadding; }
            set { m_RightPadding = value; }
        }

        /// <summary>
        /// item 之间 的空隙
        /// </summary>
        [SerializeField] private Vector2 m_Spacing;

        public bool m_AutoLocate = false;
        private bool isDragging = false;
        private bool canLocation = false;
        public float m_DoLoacteTime = 0.2f;
        public Vector2 Spacing
        {
            get { return m_Spacing; }
            set { m_Spacing = value; }
        }

        private int cellCount;
        private float cellWidth;
        private float cellHeight;

        public float CellHeight
        {
            get { return cellHeight; }
        }

        public float CellWidth
        {
            get { return cellWidth; }
        }

        private List<System.Action<InfiniteScrollItem>> onCellUpdateList = new List<System.Action<InfiniteScrollItem>>();

        private int row;
        private int col;

        private List<GameObject> cellList = new List<GameObject>();

        private Coroutine snappingProcesser;

        public OnBeginDrag onBeginDrag;
        public OnDrag onDrag;
        public OnEndDrag onEndDrag;
        public event Action onPointerDown;
        public event Action onPointerUp;

        private bool _transfer = false;

        public void AddCellListener(System.Action<InfiniteScrollItem> call)
        {
            this.onCellUpdateList.Add(call);
            this.RefreshAllCells();
        }

        public void RemoveCellListener(System.Action<InfiniteScrollItem> call)
        {
            this.onCellUpdateList.Remove(call);
        }

		public void SetItem(GameObject go)
        {
            Item = go;
        }

        public void SetCellCount(int count)
        {
            this.cellCount = Mathf.Max(0, count);

            this.Init();



            if (this.horizontal)
            {
                float newContentWidth = this.cellWidth * Mathf.CeilToInt((float)this.cellCount / this.row);
                float padding = m_RightPadding.x == 0 ? m_Padding.x * 2 : m_Padding.x + m_RightPadding.x;
                float newMaxX = newContentWidth - this.viewport.rect.width + padding; //当minX==0时maxX的位置
                float minX = this.content.offsetMin.x;
                newMaxX += minX;
                newMaxX = Mathf.Max(minX, newMaxX);
                this.content.offsetMax = new Vector2(newMaxX, 0);
            } else
            {
                float newContentHeight = this.cellHeight * Mathf.CeilToInt((float)cellCount / this.col);
                float padding = m_RightPadding.y == 0 ? m_Padding.y * 2 : m_Padding.y + m_RightPadding.y;
                float newMinY = -newContentHeight + this.viewport.rect.height - padding;
                float maxY = this.content.offsetMax.y;
                newMinY += maxY;//保持位置
                newMinY = Mathf.Min(maxY, newMinY);//保证不小于viewport的高度。
                this.content.offsetMin = new Vector2(0, newMinY);
            }

            // this.ClearCells();
            this.CreateCells();
        }

        public void Init()
        {
            if (m_Item == null)
            {
                return;
            }

            this.m_Item.SetActive(false);

            this.content = content.GetComponent<RectTransform>();
            this.viewport.localScale = Vector3.one;
            //this.viewport.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 0);
            //this.viewport.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, 0);
            //this.viewport.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 0, 0);
            //this.viewport.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 0, 0);
            this.viewport.anchorMin = Vector2.zero;
            this.viewport.anchorMax = Vector2.one;

            Rect viewRect = this.viewport.rect;
            Rect tempRect = m_Item.GetComponent<RectTransform>().rect;
            this.cellWidth = tempRect.width + m_Spacing.x;
            this.cellHeight = tempRect.height + m_Spacing.y;

            if (this.horizontal)
            {
                this.row = Mathf.FloorToInt(viewRect.height / this.cellHeight);
                this.row = Mathf.Max(1, this.row);
                this.col = Mathf.CeilToInt(viewRect.width / this.cellWidth);
            } else
            {
                this.col = (int)(viewRect.width / this.cellWidth);
                this.col = Mathf.Max(1, this.col);
                this.row = Mathf.CeilToInt(viewRect.height / this.cellHeight);
            }

            this.content.localScale = Vector3.one;
            this.content.offsetMax = new Vector2(0, 0);
            this.content.offsetMin = new Vector2(0, 0);
            this.content.anchorMin = Vector2.zero;
            this.content.anchorMax = Vector2.one;
            this.onValueChanged.RemoveAllListeners();
            this.onValueChanged.AddListener(this.OnValueChange);
        }

        public void RefreshAllCells()
        {
            foreach (GameObject cell in this.cellList)
            {
                this.cellUpdate(cell);
            }
        }

        public void ResetContent(int count, bool ignoreCreateCell = false)
        {
            this.cellCount = Mathf.Max(0, count);

            if (!ignoreCreateCell && this.cellList.Count < this.cellCount)
            {
                CreateCells();
            }

            if (this.horizontal)
            {
                float newContentWidth = this.cellWidth * Mathf.CeilToInt((float)this.cellCount / this.row);
                float padding = m_RightPadding.x == 0 ? m_Padding.x * 2 : m_Padding.x + m_RightPadding.x;
                float newMaxX = newContentWidth - this.viewport.rect.width + padding; //当minX==0时maxX的位置
                float minX = this.content.offsetMin.x;
                newMaxX += minX;
                newMaxX = Mathf.Max(minX, newMaxX);
                this.content.offsetMax = new Vector2(newMaxX, 0);
            } else
            {
                float newContentHeight = this.cellHeight * Mathf.CeilToInt((float)cellCount / this.col);
                float padding = m_RightPadding.y == 0 ? m_Padding.y * 2 : m_Padding.y + m_RightPadding.y;
                float newMinY = -newContentHeight + this.viewport.rect.height - padding;
                float maxY = this.content.offsetMax.y;
                newMinY += maxY;//保持位置
                newMinY = Mathf.Min(maxY, newMinY);//保证不小于viewport的高度。
                this.content.offsetMin = new Vector2(0, newMinY);
            }
            this.RefreshAllCells();
        }

        public void ClearCells()
        {
            foreach (GameObject cell in this.cellList)
            {
                GameObject.Destroy(cell);
            }
            this.cellList.Clear();
            onCellUpdateList?.Clear();
        }

        private void CreateCells()
        {
            if (this.horizontal)
            {
                for (int r = 0; r < this.row; r++)
                {
                    for (int l = 0; l < this.col + 1; l++)
                    {
                        int index = r * (this.col + 1) + l;
                        if (index < this.cellCount)
                        {
                            CreateItem(l, r, index);
                        }
                    }
                }
            } else
            {
                for (int r = 0; r < this.row + 1; r++)
                {
                    for (int l = 0; l < this.col; l++)
                    {
                        int index = r * this.col + l;
                        if (index < this.cellCount)
                        {
                            CreateItem(l, r, index);
                        }
                    }
                }
            }

            this.RefreshAllCells();
        }

        private void CreateItem(int l, int r, int index)
        {
            GameObject newcell;
            if (this.cellList.Count > index)
            {
                newcell = this.cellList[index];
            } else
            {
                newcell = GameObject.Instantiate<GameObject>(this.m_Item);
                this.cellList.Add(newcell);
            }
            newcell.SetActive(true);
            RectTransform cellRect = newcell.GetComponent<RectTransform>();
            cellRect.anchorMin = new Vector2(0, 1);
            cellRect.anchorMax = new Vector2(0, 1);

            float x = this.cellWidth / 2 + l * this.cellWidth + m_Padding.x;
            float y = -r * this.cellHeight - this.cellHeight / 2 - m_Padding.y;
            cellRect.SetParent(this.content, false);
            cellRect.localScale = Vector3.one;
            cellRect.anchoredPosition = new Vector3(x, y);
            InfiniteScrollItem infiniteScrollItem = newcell.GetComponent<InfiniteScrollItem>();
            if (infiniteScrollItem == null)
            {
                infiniteScrollItem = newcell.AddComponent<InfiniteScrollItem>();
            }
            if (index < 2)
            {
                infiniteScrollItem.isTop = true;
            }
            infiniteScrollItem.SetObjIndex(index);
        }

        private void OnValueChange(Vector2 pos)
        {
            foreach (GameObject cell in this.cellList)
            {
                if (this.horizontal)
                {
                    OnValueChangeHor(cell);
                    AutoLocationH();
                } else
                {
                    OnValueChangeVer(cell);
                }
            }
        }
        //该计算方法仅适合content 中心点在左侧的左右滑动
        protected void AutoLocationH()
        {
            if (canLocation && m_AutoLocate && !this.isDragging)
            {
                float deltaTime = Time.unscaledDeltaTime;
                //GGDebug.Log("velocity:" + velocity  + " velocity.x * deltaTime:" + velocity.x * deltaTime + " horizontalNormalizedPosition:" + this.horizontalNormalizedPosition);
                var moveDis = velocity.x * deltaTime;
                if (moveDis > 5 || moveDis < -5)
                {
                    return;
                }
                if (this.horizontalNormalizedPosition >= 1 || this.horizontalNormalizedPosition <= 0)
                {
                    return;
                }
                //计算最近的一个
                int indexNear = 0;
                if (velocity.x > 0)
                {
                    indexNear = (int)Mathf.Floor(Mathf.Abs(content.anchoredPosition.x) / this.cellWidth);

                } else
                {
                    indexNear = (int)Mathf.Ceil(Mathf.Abs(content.anchoredPosition.x) / this.cellWidth);
                }
                this.Snap(-indexNear, m_DoLoacteTime);
                canLocation = false;
            }
        }
        private void OnValueChangeHor(GameObject cell)
        {
            RectTransform cellRect = cell.GetComponent<RectTransform>();
            float dist = this.content.offsetMin.x + cellRect.anchoredPosition.x;
            float minLeft = -this.cellWidth / 2;
            float maxRight = this.col * this.cellWidth + this.cellWidth / 2;
            //限定复用边界
            if (dist < minLeft)
            {
                //控制cell的anchoredPosition在content的范围内才重复利用。
                float newX = cellRect.anchoredPosition.x + (this.col + 1) * (this.cellWidth);
                if (newX < this.content.rect.width)
                {
                    cellRect.anchoredPosition = new Vector3(newX, cellRect.anchoredPosition.y);
                    this.cellUpdate(cell);
                }
            } else if (dist > maxRight)
            {
                float newX = cellRect.anchoredPosition.x - (this.col + 1) * (this.cellWidth);
                if (newX > 0)
                {
                    cellRect.anchoredPosition = new Vector3(newX, cellRect.anchoredPosition.y);
                    this.cellUpdate(cell);
                }
            }
        }


        private void OnValueChangeVer(GameObject cell)
        {
            RectTransform cellRect = cell.GetComponent<RectTransform>();
            float dist = this.content.offsetMax.y + cellRect.anchoredPosition.y;
            float maxTop = this.cellHeight / 2;
            float minBottom = -((this.row + 1) * this.cellHeight) + this.cellHeight / 2;
            InfiniteScrollItem scrollGridCell = cell.GetComponent<InfiniteScrollItem>();

            if (dist + this.cellHeight * 2 > 0)
            {
                scrollGridCell.isTop = true;
            } else
            {
                scrollGridCell.isTop = false;
            }

            if (dist > maxTop)
            {
                float newY = cellRect.anchoredPosition.y - (this.row + 1) * this.cellHeight;
                //保证cell的anchoredPosition只在content的高的范围内活动，下同理
                if (newY > -this.content.rect.height)
                {
                    //重复利用cell，重置位置到视野范围内。
                    cellRect.anchoredPosition = new Vector3(cellRect.anchoredPosition.x, newY);
                    if (newY + this.content.offsetMax.y <= maxTop + this.cellHeight)
                    {
                        this.cellUpdate(cell);
                    }
                }
            } else if (dist < minBottom)
            {
                float newY = cellRect.anchoredPosition.y + (this.row + 1) * this.cellHeight;
                if (newY < 0)
                {
                    cellRect.anchoredPosition = new Vector3(cellRect.anchoredPosition.x, newY);
                    if (newY + this.content.offsetMax.y >= minBottom - this.cellHeight)
                    {
                        this.cellUpdate(cell);
                    }
                    //this.cellUpdate(cell);
                }
            }
        }

        private void OnValueChange(Vector2 pos, float num)
        {
            foreach (GameObject cell in this.cellList)
            {
                if (this.horizontal)
                {
                    OnValueChangeHor(cell, num);
                    AutoLocationH();
                } else
                {
                    OnValueChangeVer(cell, num);
                }
            }
        }

        private void OnValueChangeHor(GameObject cell, float num)
        {
            RectTransform cellRect = cell.GetComponent<RectTransform>();
            //限定复用边界
            float newX = cellRect.anchoredPosition.x;
            for (int i = 0; i < num; i++)
            {
                float dist = this.content.offsetMin.x + newX;
                float minLeft = -this.cellWidth / 2;
                float maxRight = this.col * this.cellWidth + this.cellWidth / 2;
                if (dist < minLeft)
                {
                    //控制cell的anchoredPosition在content的范围内才重复利用。
                    if (newX < this.content.rect.width - ((this.col + 1) * (this.cellWidth)))
                    {
                        newX += (this.col + 1) * (this.cellWidth);
                    }
                } else if (dist > maxRight)
                {
                    if (newX > (this.col + 1) * (this.cellWidth))
                    {
                        newX -= (this.col + 1) * (this.cellWidth);
                    }
                }
            }
            cellRect.anchoredPosition = new Vector3(newX, cellRect.anchoredPosition.y);
            this.cellUpdate(cell);
        }

        private void OnValueChangeVer(GameObject cell, float num)
        {
            RectTransform cellRect = cell.GetComponent<RectTransform>();
            InfiniteScrollItem scrollGridCell = cell.GetComponent<InfiniteScrollItem>();
            float newY = cellRect.anchoredPosition.y;
            for (int i = 0; i < num; i++)
            {
                float dist = this.content.offsetMax.y + newY;
                float maxTop = this.cellHeight / 2;
                float minBottom = -((this.row + 1) * this.cellHeight) + this.cellHeight / 2;

                if (dist + this.cellHeight * 2 > 0)
                {
                    scrollGridCell.isTop = true;
                } else
                {
                    scrollGridCell.isTop = false;
                }

                if (dist > maxTop)
                {

                    //保证cell的anchoredPosition只在content的高的范围内活动，下同理
                    if (newY > (this.row + 1) * this.cellHeight-this.content.rect.height)
                    {
                        //重复利用cell，重置位置到视野范围内。
                        newY -= (this.row + 1) * this.cellHeight;
                    }
                } else if (dist < minBottom)
                {

                    if (newY < - (this.row + 1) * this.cellHeight)
                    {
                        newY +=(this.row + 1) * this.cellHeight;
                    }
                }
            }

            cellRect.anchoredPosition = new Vector3(cellRect.anchoredPosition.x, newY);
            this.cellUpdate(cell);
        }

        private int allCol
        {
            get { return Mathf.CeilToInt((float)this.cellCount / this.row); }
        }

        private int allRow
        {
            get { return Mathf.CeilToInt((float)this.cellCount / this.col); }
        }

        private void cellUpdate(GameObject cell)
        {
            if (cell == null)
            {
                return;
            }

            RectTransform cellRect = cell.GetComponent<RectTransform>();
            int x = Mathf.FloorToInt((cellRect.anchoredPosition.x - cellWidth / 2 - m_Padding.x) / cellWidth + 0.5f);
            int y = Mathf.FloorToInt(Mathf.Abs((cellRect.anchoredPosition.y + cellHeight / 2) / cellHeight) + 0.5f);

            int index = 0;
            if (this.horizontal)
            {
                index = y * allCol + x;
            } else
            {
                index = y * this.col + x;
            }

            InfiniteScrollItem scrollGridCell = cell.GetComponent<InfiniteScrollItem>();
            scrollGridCell.UpdatePos(x, y, index);
            if (index >= cellCount || index < 0 || x >= (this.horizontal ? this.allCol : this.col))
            {
                cell.SetActive(false);
            } else
            {
                if (cell.activeSelf == false)
                {
                    cell.SetActive(true);
                }

                foreach (var call in this.onCellUpdateList)
                {
                    call(scrollGridCell);
                }
            }

        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            base.OnBeginDrag(eventData);
            onBeginDrag?.Invoke();
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (!IsActive())
                return;

            isDragging = true;
            canLocation = false;
        }

        public override void OnDrag(PointerEventData eventData)
        {
            base.OnDrag(eventData);
            onDrag?.Invoke();
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            base.OnEndDrag(eventData);
            onEndDrag?.Invoke();
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            isDragging = false;
            canLocation = true;
        }

        public void Snap(int index, float duration, bool transfer = false)
        {
            if (m_Item == null)
                return;

            if (index >= cellCount)
                return;
            //transfer为true则直接传送到指定位置，不会有滚动过程
            _transfer = transfer;
            if (horizontal)
            {
                SnapHorizontal(index, duration);
            } else
            {
                SnapVertical(index, duration);
            }

        }

        private void SnapHorizontal(int index, float duration)
        {
            if (content.rect.width < viewport.rect.width)
                return;

            float width = this.cellWidth * index;
            width = Mathf.Min(content.rect.width - viewport.rect.width, width);
            if (content.anchoredPosition.x != width)
            {
                DoSnapping(new Vector2(width, 0), duration);
            }
        }

        private void SnapVertical(int index, float duration)
        {
            if (content.rect.height < viewport.rect.height)
                return;

            float height = this.cellHeight * (index / col);
            height = Mathf.Min(content.rect.height - viewport.rect.height, height);
            if (content.anchoredPosition.y != height)
            {
                DoSnapping(new Vector2(0, height), duration);
            }
        }

        protected void DoSnapping(Vector2 target, float duration)
        {
            if (!gameObject.activeInHierarchy)
                return;
            StopSnapping();
            if (_transfer)
            {
                ProcessSnappingSync(target, duration);
            } else
            {
                snappingProcesser = StartCoroutine(ProcessSnapping(target, duration));
            }
        }

        public void DoSnappingPos(Vector2 target)
        {
            if (!gameObject.activeInHierarchy)
                return;
            StopSnapping();
            ProcessSnappingSync(target,0);

        }

        public void StopSnapping()
        {
            if (snappingProcesser != null)
            {
                StopCoroutine(snappingProcesser);
                snappingProcesser = null;
            }
        }

        private IEnumerator ProcessSnapping(Vector2 target, float duration)
        {
            velocity = Vector2.zero;
            Vector2 startPos = content.anchoredPosition;
            float t = 0;
            float moveDelta = 0;
            while (t < 1f)
            {
                if (duration <= 0)
                    t = 1;
                else
                    t += Time.deltaTime / duration;

                if (this.horizontal)
                {
                    float curX = content.anchoredPosition.x;
                    content.anchoredPosition = Vector2.Lerp(startPos, target, t);

                    //处理瞬间移动超高距离时候的刷新
                    float moveWidth = Mathf.Abs(content.anchoredPosition.x - curX);
                    moveDelta = moveWidth;
                    if (moveWidth > viewport.rect.size.x && !_transfer)
                    {
                        for (int i = 0; i < Mathf.Ceil(moveWidth / viewport.rect.size.x); i++)
                        {
                            OnValueChange(content.anchoredPosition);
                        }
                    }

                } else
                {
                    float curY = content.anchoredPosition.y;
                    content.anchoredPosition = Vector2.Lerp(startPos, target, t);

                    //处理瞬间移动超高距离时候的刷新
                    float moveHeight = Mathf.Abs(content.anchoredPosition.y - curY);
                    if (moveHeight > viewport.rect.size.y && !_transfer)
                    {
                        for (int i = 0; i < Mathf.Ceil(moveHeight / viewport.rect.size.y); i++)
                        {
                            OnValueChange(content.anchoredPosition);
                        }
                    }
                }

                var normalizedPos = normalizedPosition;
                if (normalizedPos.y < 0 || normalizedPos.x > 1)
                {
                    break;
                }

                yield return null;
            }

            if (_transfer)
            {
                _transfer = false;
                OnValueChange(content.anchoredPosition, Mathf.Ceil(moveDelta / viewport.rect.size.x));
            }
            if (duration <= 0)
                RefreshAllCells();
            
            snappingProcesser = null;


        }

        private void ProcessSnappingSync(Vector2 target, float duration)
        {
            velocity = Vector2.zero;
            Vector2 startPos = content.anchoredPosition;
            float t = 0;
            float moveDelta = 0;
            while (t < 1f)
            {
                if (duration <= 0)
                    t = 1;
                else
                    t += Time.deltaTime / duration;

                if (this.horizontal)
                {
                    float curX = content.anchoredPosition.x;
                    content.anchoredPosition = Vector2.Lerp(startPos, target, t);

                    //处理瞬间移动超高距离时候的刷新
                    float moveWidth = Mathf.Abs(content.anchoredPosition.x - curX);
                    moveDelta = moveWidth;
                    OnValueChange(content.anchoredPosition, Mathf.Ceil(moveDelta / viewport.rect.size.x));

                } else
                {
                    float curY = content.anchoredPosition.y;
                    content.anchoredPosition = Vector2.Lerp(startPos, target, t);
                    //处理瞬间移动超高距离时候的刷新
                    float moveHeight = Mathf.Abs(content.anchoredPosition.y - curY);
                    moveDelta = moveHeight;
                    OnValueChange(content.anchoredPosition, Mathf.Ceil(moveDelta / viewport.rect.size.y));
                }

                var normalizedPos = normalizedPosition;
                if (normalizedPos.y < 0 || normalizedPos.x > 1)
                {
                    break;
                }
            }

            _transfer = false;
            if (duration <= 0)
            {
                RefreshAllCells();
            }
            content.anchoredPosition = Vector2.Lerp(startPos, target, t);
            snappingProcesser = null;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            onPointerDown?.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            onPointerUp?.Invoke();
        }

        protected override void OnDestroy()
        {
            onCellUpdateList?.Clear();
            onCellUpdateList = null;
            cellList?.Clear();
            cellList = null;
            m_Item = null;
            onBeginDrag = null;
            onDrag = null;
            onEndDrag = null;
            onPointerDown = null;
            onPointerUp = null;
            base.OnDestroy();
        }
    }
}
