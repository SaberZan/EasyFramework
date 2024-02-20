#if MATCH3_VIEW

using Easy;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
namespace Match3
{
    
    [Update]
    [NormalInit]
    public class InputMgr : Singleton<InputMgr>
    {
        /// <summary>
        /// 按下的事件
        /// </summary>
        public event UnityAction<Vector3> OnHandleEventDown;
        /// <summary>
        /// 弹起的事件
        /// </summary>
        public event UnityAction<Vector3> OnHandleEventUp;
        /// <summary>
        /// 点击弹起的事件
        /// </summary>
        public event UnityAction<Vector3> OnHandleEventClickUp;
        /// <summary>
        /// 移动的事件
        /// </summary>
        public event UnityAction<Vector3, Vector3> OnHandleEventMoving;
        /// <summary>
        /// 缩放的事件
        /// 鼠标滚轮、触屏两指
        /// </summary>
        public event UnityAction<float> OnHandleEventScaling;
        /// <summary>
        /// 返回按钮事件
        /// </summary>
        public event UnityAction OnHandleEscape;

        /// <summary>
        /// 是否按下
        /// </summary>
        private bool _bDown = false;
        /// <summary>
        /// 按下的坐标
        /// </summary>
        private Vector3 _downPos;
        private Vector3 _curDownPos;
        /// <summary>
        /// 移动临界距离
        /// </summary>
        private readonly float _CLICK_DIS = 4;
        /// <summary>
        /// 点击时间临界点
        /// </summary>
        private readonly float _DOWN_TIME = 0.4f;
        private float _downTime;
        private float _curDownTime;

        //#if !UNITY_EDITOR
        /// <summary>
        /// 当前两触碰点间的距离
        /// </summary>
        private float _curTouchesDis;
        //#endif

        /// <summary>
        /// 是否促发过缩放
        /// </summary>
        private bool _isTriggerScroll = false;
        
        public override void BeforeRestart()
        {
            
        }
        
        public override void Init(InitCompleteCallback complete)
        {
            complete.Invoke(true);
        }
        
        public override void Update(float deltaTime)
        {
#if !UNITY_EDITOR
            if (Input.touchCount == 1)  // 点击处理
            {
                Touch touch = Input.touches[0];
#endif
            if (_bDown == false)
            {
#if UNITY_EDITOR
                if (Input.GetMouseButtonDown(0))
                {
#else
                if (touch.phase == TouchPhase.Began || _isTriggerScroll)
                {
#endif
                    OnTouchDown();
                }
            } else
            {
#if UNITY_EDITOR
                if (Input.GetMouseButtonUp(0))
#else
                if (touch.phase == TouchPhase.Canceled || touch.phase == TouchPhase.Ended)
#endif
                {
                    _isTriggerScroll = false;
                    OnTouchUp();
                }
#if UNITY_EDITOR
                else
#else
                else if (touch.phase == TouchPhase.Moved)
#endif
                {
                    OnTouchMove();
                }
            }
#if !UNITY_EDITOR
            }
#endif

#if UNITY_EDITOR
            // 滚轮
            if (Input.GetAxis("Mouse ScrollWheel") != 0)
            {
                float scrollValue = Input.GetAxis("Mouse ScrollWheel");
                OnHandleEventScaling?.Invoke(scrollValue * Time.deltaTime * 50);
            }
#else
            else if (Input.touchCount == 2)     // 缩放处理
            {
                float _scaleDis = 0;
                Touch touch_1 = Input.touches[0];
                Touch touch_2 = Input.touches[1];
                _bDown = false;
                _isTriggerScroll = true;
                if (touch_2.phase == TouchPhase.Began)
                {
                    Vector3 nowPos_1 = touch_1.position;
                    Vector3 nowPos_2 = touch_2.position;
                    _curTouchesDis = Vector3.Distance(nowPos_1, nowPos_2);
                }
                else if (touch_1.phase == TouchPhase.Moved || touch_2.phase == TouchPhase.Moved)
                {
                    Vector3 nowPos_1 = touch_1.position;
                    Vector3 nowPos_2 = touch_2.position;
                    float nowDis = Vector3.Distance(nowPos_1, nowPos_2);
                    if (nowDis > _curTouchesDis)
                    {
                        _scaleDis = 0.16f;
                    }
                    else if (nowDis < _curTouchesDis)
                    {
                        _scaleDis = -0.16f;
                    }
                    else
                    {
                        _scaleDis = 0;
                    }
                    OnHandleEventScaling?.Invoke(_scaleDis);
                    _curTouchesDis = nowDis;

                    //Vector3 nowPos_1 = touch_1.position;
                    //Vector3 nowPos_2 = touch_2.position;
                    //float nowDis = Vector3.Distance(nowPos_1, nowPos_2);
                    //OnHandleEventScaling?.Invoke(nowDis - curTouchesDis);
                    //curTouchesDis = nowDis;
                }
                else if((touch_1.phase == TouchPhase.Canceled || touch_1.phase == TouchPhase.Ended) && (touch_2.phase == TouchPhase.Canceled || touch_2.phase == TouchPhase.Ended))
                {
                    _isTriggerScroll = false;
                }
            }
#endif
            // 键盘上 ESC；Android手机上返回键； iOS没有用
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                OnHandleEscape?.Invoke();
            }
        }

        private void OnTouchMove()
        {
#if UNITY_EDITOR
            // 左键拖动
            Vector3 nowPos = Input.mousePosition;
#else
            Touch touch = Input.touches[0];
            Vector3 nowPos = touch.position;
#endif
            OnHandleEventMoving?.Invoke(_curDownPos, nowPos);
            _curDownPos = nowPos;
        }

        private void OnTouchUp()
        {
            // 左键弹起
            _bDown = false;
#if UNITY_EDITOR
            Vector3 endPos = Input.mousePosition;
#else
            Touch touch = Input.touches[0];
            Vector3 endPos = touch.position;
#endif
            _curDownTime = Time.realtimeSinceStartup;
            float interval = _curDownTime - _downTime;
            //Debuger.Log("Mouse End Position : " + endPos.ToString());
            float dis = Vector3.Distance(_downPos, endPos);
            if (dis < _CLICK_DIS && interval < _DOWN_TIME)
                OnHandleEventClickUp?.Invoke(endPos);
            OnHandleEventUp?.Invoke(endPos);
        }

        private void OnTouchDown()
        {
#if UNITY_EDITOR
            // 是否点击在UI上
            if (EventSystem.current.IsPointerOverGameObject())
                return;
            _curDownPos = _downPos = Input.mousePosition;
#else
            Touch touch = Input.touches[0];
            if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                return;
            _curDownPos = _downPos = touch.position;
#endif
            _curDownTime = _downTime = Time.realtimeSinceStartup;
            // 左键按下
            _bDown = true;
            OnHandleEventDown?.Invoke(_downPos);
        }
        
    }
}

#endif
