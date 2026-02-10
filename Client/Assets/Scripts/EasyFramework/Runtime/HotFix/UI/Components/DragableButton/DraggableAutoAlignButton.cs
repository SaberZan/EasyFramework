
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableAutoAlignButton : MonoBehaviour , IBeginDragHandler, IEndDragHandler, IDragHandler
{
    public enum Alignment
    {
        Left,
        Right,
        Top,
        Bottom
    }

    public Alignment alignment = Alignment.Left;
    public float speed = 5f;
    private Camera _camera;
    private RectTransform _rectTransform;
    private RectTransform _parentTransform;
    private bool isDragging = false;
    private bool isInAlign = false;
    public bool IsCanClicked => !isDragging && !isInAlign;

    void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
        _parentTransform = transform.parent as RectTransform;
    }

    public void RefrehPos()
    {
        _rectTransform.anchoredPosition = GetTargetPosition();
    }

    void Update()
    {
        if(isDragging)
        {
#if UNITY_EDITOR
            if(!Input.GetMouseButton(0))
            {
                OnCancel();
            }
#else
            int count = Input.touchCount;
            if (count == 0)
            {
                OnCancel();
            }
#endif
        }

        if(isInAlign)
        {
            AlignToEdge();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        isInAlign = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector3 prePos = _rectTransform.anchoredPosition;
        _camera = eventData.enterEventCamera ? eventData.enterEventCamera : Camera.main;
        _rectTransform.anchoredPosition = ScreenToUILocalPos(_parentTransform, Input.mousePosition, _camera);
        if( _rectTransform.anchoredPosition.x > _parentTransform.rect.width/2 
        || _rectTransform.anchoredPosition.x < -_parentTransform.rect.width/2
        || _rectTransform.anchoredPosition.y > _parentTransform.rect.height/2 
        || _rectTransform.anchoredPosition.y < -_parentTransform.rect.height/2
        )
        {
            _rectTransform.anchoredPosition = prePos;
        }
        else if(_rectTransform.anchoredPosition == Vector2.zero)
        {
            _rectTransform.anchoredPosition = prePos;
        }

        isInAlign = false;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        CheckAlign();
    }

    public void OnCancel()
    {
        if(!isDragging)
        {
            return;
        }
        isDragging = false;
        CheckAlign();
    }
    
    private Vector2 ScreenToUILocalPos(RectTransform targetRect, Vector2 mousePos, Camera canvasCam = null)
    {
        //UI 的局部坐标
        Vector2 uiLocalPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(targetRect, mousePos, canvasCam, out uiLocalPos);
        return uiLocalPos;
    }
    
    public void CheckAlign()
    {
        float intervalTop = Mathf.Abs(_parentTransform.rect.height /2 -  _rectTransform.anchoredPosition.y);
        float intervalBottom = Mathf.Abs(_parentTransform.rect.height /2 +  _rectTransform.anchoredPosition.y);
        float intervalLeft = Mathf.Abs(_parentTransform.rect.width /2 +  _rectTransform.anchoredPosition.x);
        float intervalRight = Mathf.Abs(_parentTransform.rect.width /2 -  _rectTransform.anchoredPosition.x);

        float[] intervals = new float[4] {intervalLeft, intervalRight, intervalTop, intervalBottom};
        int minIndex = 0;
        float minValue = intervals[0];
        for(int i = 0;i < 4; i++)
        {
            if(intervals[i] < minValue)
            {
                minIndex = i;
                minValue = intervals[i];
            }
        }
        alignment = (Alignment)minIndex;

        isInAlign = true;
    }
    
    void AlignToEdge()
    {
        Vector3 targetPosition = GetTargetPosition();
        _rectTransform.anchoredPosition = Vector3.Lerp(_rectTransform.anchoredPosition, targetPosition, Time.deltaTime * speed);
        if (Vector3.Distance(_rectTransform.anchoredPosition, GetTargetPosition()) < 0.0001f)
        {
            _rectTransform.anchoredPosition = targetPosition;
            isInAlign = false;
        }
    }

    Vector3 GetTargetPosition()
    {
        Vector3 targetPosition = _rectTransform.anchoredPosition;
        switch (alignment)
        {
            case Alignment.Left:
                targetPosition.x = -_parentTransform.rect.width / 2 + (_rectTransform.rect.width/2);
                break;
            case Alignment.Right:
                targetPosition.x = _parentTransform.rect.width/2 - (_rectTransform.rect.width/2);
                break;
            case Alignment.Top:
                targetPosition.y = _parentTransform.rect.height/2 - (_rectTransform.rect.height/2);
                break;
            case Alignment.Bottom:
                targetPosition.y = -_parentTransform.rect.height/2 + (_rectTransform.rect.height/2);
                break;
        }
        return targetPosition;
    }

}