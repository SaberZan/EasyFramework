
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Easy
{
    [AddComponentMenu("Layout/Centered Grid Layout Group", 152)]
    public class CenteredGridLayoutGroup : GridLayoutGroup
    {
        /// <summary>
        /// Called by the layout system
        /// Also see ILayoutElement
        /// </summary>
        public override void SetLayoutHorizontal()
        {
            SetCellsAlongAxis(0);
        }
 
        /// <summary>
        /// Called by the layout system
        /// Also see ILayoutElement
        /// </summary>
        public override void SetLayoutVertical()
        {
            SetCellsAlongAxis(1);
        }
 
        private void SetCellsAlongAxis(int axis)
        {
            // Normally a Layout Controller should only set horizontal values when invoked for the horizontal axis
            // and only vertical values when invoked for the vertical axis.
            // However, in this case we set both the horizontal and vertical position when invoked for the vertical axis.
            // Since we only set the horizontal position and not the size, it shouldn't affect children's layout,
            // and thus shouldn't break the rule that all horizontal layout must be calculated before all vertical layout.
            var rectChildrenCount = rectChildren.Count;
            if (axis == 0)
            {
                // Only set the sizes when invoked for horizontal axis, not the positions.
 
                for (int i = 0; i < rectChildrenCount; i++)
                {
                    RectTransform rect = rectChildren[i];
 
                    m_Tracker.Add(this, rect,
                        DrivenTransformProperties.Anchors |
                        DrivenTransformProperties.AnchoredPosition |
                        DrivenTransformProperties.SizeDelta);
 
                    rect.anchorMin = Vector2.up;
                    rect.anchorMax = Vector2.up;
                    rect.sizeDelta = cellSize;
                }
                return;
            }
 
            float width = rectTransform.rect.size.x;
            float height = rectTransform.rect.size.y;
 
            int cellCountX = 1;
            int cellCountY = 1;
            if (m_Constraint == Constraint.FixedColumnCount)
            {
                cellCountX = m_ConstraintCount;
 
                if (rectChildrenCount > cellCountX)
                    cellCountY = rectChildrenCount / cellCountX + (rectChildrenCount % cellCountX > 0 ? 1 : 0);
            }
            else if (m_Constraint == Constraint.FixedRowCount)
            {
                cellCountY = m_ConstraintCount;
 
                if (rectChildrenCount > cellCountY)
                    cellCountX = rectChildrenCount / cellCountY + (rectChildrenCount % cellCountY > 0 ? 1 : 0);
            }
            else
            {
                if (cellSize.x + spacing.x <= 0)
                    cellCountX = int.MaxValue;
                else
                    cellCountX = Mathf.Max(1, Mathf.FloorToInt((width - padding.horizontal + spacing.x + 0.001f) / (cellSize.x + spacing.x)));
 
                if (cellSize.y + spacing.y <= 0)
                    cellCountY = int.MaxValue;
                else
                    cellCountY = Mathf.Max(1, Mathf.FloorToInt((height - padding.vertical + spacing.y + 0.001f) / (cellSize.y + spacing.y)));
            }
 
 
            //startCorner = 0
            int cornerX = (int)startCorner % 2;//0
            int cornerY = (int)startCorner / 2;//0
 
            int cellsPerMainAxis, actualCellCountX, actualCellCountY;
            if (startAxis == Axis.Horizontal)
            {
                cellsPerMainAxis = cellCountX;
                actualCellCountX = Mathf.Clamp(cellCountX, 1, rectChildrenCount);
                actualCellCountY = Mathf.Clamp(cellCountY, 1, Mathf.CeilToInt(rectChildrenCount / (float)cellsPerMainAxis));
            }
            else
            {
                cellsPerMainAxis = cellCountY;
                actualCellCountY = Mathf.Clamp(cellCountY, 1, rectChildrenCount);
                actualCellCountX = Mathf.Clamp(cellCountX, 1, Mathf.CeilToInt(rectChildrenCount / (float)cellsPerMainAxis));
            }
 
            //总长度 & 总高度
            Vector2 requiredSpace = new Vector2(
                actualCellCountX * cellSize.x + (actualCellCountX - 1) * spacing.x,
                actualCellCountY * cellSize.y + (actualCellCountY - 1) * spacing.y
            );
            Vector2 startOffset = new Vector2(
                GetStartOffset(0, requiredSpace.x),
                GetStartOffset(1, requiredSpace.y)
            );
 
            //最后一行/列的偏移参数
            Vector2 requiredSpaceNew = Vector2.zero;
            Vector2 startOffsetNew = Vector2.zero;
            //水平顺序布局，计算是否为整行
            if (startAxis == Axis.Horizontal)
            {
                //少于一行,按照原数据计算
                if (rectChildrenCount <= actualCellCountX)
                {
                    requiredSpaceNew = requiredSpace;
                    startOffsetNew = startOffset;
                }
                else
                {
                    //整行
                    var rem = rectChildrenCount % actualCellCountX;
                    if (rem == 0)
                    {
                        requiredSpaceNew = requiredSpace;
                        startOffsetNew = startOffset;
                    }
                    else
                    {
                        requiredSpaceNew = new Vector2(
                            rem * cellSize.x + (rem - 1) * spacing.x,
                            actualCellCountY * cellSize.y + (actualCellCountY - 1) * spacing.y);
                        startOffsetNew = new Vector2(
                            GetStartOffset(0, requiredSpaceNew.x),
                            GetStartOffset(1, requiredSpaceNew.y)
                        );
                    }
                }
                
            }
            else
            {
                //少于一列,按照原数据计算
                if (rectChildrenCount <= actualCellCountY)
                {
                    requiredSpaceNew = requiredSpace;
                    startOffsetNew = startOffset;
                }
                else
                {
                    //整列
                    var rem = rectChildrenCount % actualCellCountY;
                    if (rem == 0)
                    {
                        requiredSpaceNew = requiredSpace;
                        startOffsetNew = startOffset;
                    }
                    else
                    {
                        requiredSpaceNew = new Vector2(
                            actualCellCountX * cellSize.x + (actualCellCountX - 1) * spacing.x,
                            rem * cellSize.y + (rem - 1) * spacing.y);
                        startOffsetNew = new Vector2(
                            GetStartOffset(0, requiredSpaceNew.x),
                            GetStartOffset(1, requiredSpaceNew.y)
                        );
                    }
                }
            }
 
            //最后一行/列的起始索引
            int lastRowIndex = rectChildrenCount - (actualCellCountX > 0 ? rectChildrenCount % actualCellCountX : 0);
            int lastColIndex = rectChildrenCount - (actualCellCountY > 0 ? rectChildrenCount % actualCellCountY : 0);
 
 
            for (int i = 0; i < rectChildrenCount; i++)
            {
                int positionX;
                int positionY;
                //最后一行/列
                bool lastRowOrCol = false;
                if (startAxis == Axis.Horizontal)
                {
                    positionX = i % cellsPerMainAxis;
                    positionY = i / cellsPerMainAxis;
 
                    lastRowOrCol = i >= lastRowIndex;
                }
                else
                {
                    positionX = i / cellsPerMainAxis;
                    positionY = i % cellsPerMainAxis;
 
                    lastRowOrCol = i >= lastColIndex;
 
                }
 
                if (cornerX == 1)
                    positionX = actualCellCountX - 1 - positionX;
                if (cornerY == 1)
                    positionY = actualCellCountY - 1 - positionY;
 
                if (lastRowOrCol)
                {
                    SetChildAlongAxis(rectChildren[i], 0, startOffsetNew.x + (cellSize[0] + spacing[0]) * positionX, cellSize[0]);
                    SetChildAlongAxis(rectChildren[i], 1, startOffsetNew.y + (cellSize[1] + spacing[1]) * positionY, cellSize[1]);
                }
                else
                {
                    SetChildAlongAxis(rectChildren[i], 0, startOffset.x + (cellSize[0] + spacing[0]) * positionX, cellSize[0]);
                    SetChildAlongAxis(rectChildren[i], 1, startOffset.y + (cellSize[1] + spacing[1]) * positionY, cellSize[1]);
                }
 
            }
        }
    }
}