using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Reflection;

namespace Easy
{
    
    public class LayerParamsAttribute : Attribute
    {
        public LayerType layerType;

        public int order;

        public bool keep; //切换场景时保留
    }

    public enum LayerType
    {
        NORMAL_STATIC,
        NORMAL_DYNAMIC,
        DIALOG,
        ABOVE_DIALOG,
        TOP_LAYER,
        TOAST,
        TUTORIAL
    }
    public enum DialogType
    {
        NORMAL,
        LOW_PRIORITY,
        SECONDARY,  //二级弹窗
    }

    /// <summary>
    /// 控制层
    /// </summary>

    [NormalInit]
    [OrderIndex((int)NormalInitOrderIndexEnum.UIMgr)]
    public class UIMgr : Singleton<UIMgr>
    {
        private EventSystem _eventSystem;
        private GameObject _staticCanvasNode;
        private GameObject _dynamicCanvasNode;
        private GameObject _canvasNode;
        private GameObject _eventSystemNode;
        //private GameObject blackMask;

        private GameObject _dynamicLayerRootNode;
        private GameObject _staticLayerRootNode;
        private GameObject _aboveDialogLayerRootNode;
        private GameObject _topLayerRootNode;
        private GameObject _dialogLayerRootNode;
        private GameObject _tutorialRooNode;
        private GameObject _toastRootNode;

        private CanvasScaler _staticCanvasScaler;
        private CanvasScaler _dynamicCanvasScaler;

        private Dictionary<string, BaseUILayer> _layers = new Dictionary<string, BaseUILayer>();

        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        public override void Init(InitCompleteCallback complete)
        {
            InitGameObjects();
            InitLayers();
            complete.Invoke(true);
        }

        private void InitGameObjects()
        {
            //Canvas EventSystem
            GameObject[] rootObjs = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (GameObject obj in rootObjs)
            {
                if (obj.name == "Canvas" && obj.GetComponent<Canvas>() != null)
                {
                    _canvasNode = obj;
                }
                if (obj.name == "EventSystem" && obj.GetComponent<EventSystem>() != null)
                {
                    _eventSystemNode = obj;
                }
            }
            if (_canvasNode == null)
            {
                _canvasNode = new GameObject("Canvas");
                _canvasNode.layer = 5;//5:UI
            }

            if (_eventSystemNode == null)
            {
                _eventSystemNode = new GameObject("EventSystem");
            }
            UnityEngine.Object.DontDestroyOnLoad(_canvasNode);
            UnityEngine.Object.DontDestroyOnLoad(_eventSystemNode);
            Canvas canvas = _canvasNode.HasComponent<Canvas>() ? _canvasNode.GetComponent<Canvas>() : _canvasNode.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _staticCanvasScaler = _canvasNode.HasComponent<CanvasScaler>() ? _canvasNode.GetComponent<CanvasScaler>() : _canvasNode.AddComponent<CanvasScaler>();
            _staticCanvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            _staticCanvasScaler.referenceResolution = new Vector2(1080, 1920);
            _staticCanvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            GraphicRaycaster graphicRaycaster = _canvasNode.AddComponent<GraphicRaycaster>();
            //完善Canvas
            this.SetCanvasNode(_canvasNode);
            //EventSystem
            this._eventSystem = _eventSystemNode.HasComponent<EventSystem>() ? _eventSystemNode.GetComponent<EventSystem>() : _eventSystemNode.AddComponent<EventSystem>();
            StandaloneInputModule standaloneInputModule = _eventSystemNode.HasComponent<StandaloneInputModule>() ? _eventSystemNode.GetComponent<StandaloneInputModule>() : _eventSystemNode.AddComponent<StandaloneInputModule>();
            // UnityEngine.SceneManagement.SceneManager.sceneUnloaded += onSceneUnloaded;
        }

        private void InitLayers()
        {
            //查找全部数据类
            List<Type> types = EasyFrameworkMain.Instance.GetTypes();
            foreach (var t in types)
            {
                if (typeof(BaseUILayer).IsInstanceOfType(t))
                {
                    BaseUILayer obj = (BaseUILayer)Activator.CreateInstance(t);
                    AddLayer(obj);
                }
            }
        }
        
        /// <summary>
        /// 场景移除
        /// </summary>
        /// <param name="scene"></param>
        private void onSceneUnloaded(UnityEngine.SceneManagement.Scene scene)
        {
            EasyLogger.Log("EasyFrameWork", "场景切换,UIView自动移除全部UI节点(请不要在Awake中添加UI,会被误移除).");
            RemoveAllLayer();
        }

        /// <summary>
        /// 重启
        /// </summary>
        public override void BeforeRestart()
        {
            UnityEngine.Object.Destroy(_canvasNode);
            UnityEngine.Object.Destroy(_eventSystemNode);
            UnityEngine.Object.Destroy(_dynamicCanvasNode);
        }

        /// <summary>
        /// 根据类型返回层根节点
        /// </summary>
        /// <param name="layerType"></param>
        /// <returns></returns>
        private GameObject SeakLayerNodeByType(LayerType layerType = LayerType.NORMAL_STATIC)
        {
            switch (layerType)
            {
                case LayerType.NORMAL_STATIC: return this._staticLayerRootNode;
                case LayerType.NORMAL_DYNAMIC: return this._dynamicLayerRootNode;
                case LayerType.DIALOG: return this._dialogLayerRootNode;
                case LayerType.ABOVE_DIALOG: return this._aboveDialogLayerRootNode;
                case LayerType.TOP_LAYER: return this._topLayerRootNode;
                case LayerType.TOAST: return this._toastRootNode;
                case LayerType.TUTORIAL: return this._tutorialRooNode;
                default: return null;
            }
        }
        /// <summary>
        /// 设置Canvas节点
        /// </summary>
        /// <param name="canvasNode"></param>
        private void SetCanvasNode(GameObject staticCanvasNode)
        {
            this._staticCanvasNode = staticCanvasNode;
            //Layer
            _staticLayerRootNode = new GameObject("Layer");
            _staticLayerRootNode.transform.SetParent(this._staticCanvasNode.transform, false);
            AddFullScreenRectTransform(_staticLayerRootNode, true);
            ///
            ///动态Canvas - 复制static
            ///仅普通Layer区分静态、动态
            ///
            this._dynamicCanvasNode = UnityEngine.Object.Instantiate(this._staticCanvasNode);
            this._dynamicCanvasNode.name = "DynamicCanvas";
            this._dynamicCanvasNode.GetComponent<Canvas>().sortingOrder = this._staticCanvasNode.GetComponent<Canvas>().sortingOrder + 1;
            _dynamicCanvasScaler = this._dynamicCanvasNode.GetComponent<CanvasScaler>();
            UnityEngine.Object.DontDestroyOnLoad(this._dynamicCanvasNode);
            this._dynamicLayerRootNode = this._dynamicCanvasNode.transform.Find("Layer").gameObject;

            //Dialog 放在动态Canvas上
            _dialogLayerRootNode = new GameObject("DialogLayer");
            _dialogLayerRootNode.transform.SetParent(this._dynamicCanvasNode.transform, false);
            AddFullScreenRectTransform(_dialogLayerRootNode, true);

            //AboveDialog 放在动态Canvas上
            _aboveDialogLayerRootNode = new GameObject("AboveDialogLayer");
            _aboveDialogLayerRootNode.transform.SetParent(this._dynamicCanvasNode.transform, false);
            AddFullScreenRectTransform(_aboveDialogLayerRootNode, true);

            //TopLayer 放在动态Canvas上
            _topLayerRootNode = new GameObject("TopLayer");
            _topLayerRootNode.transform.SetParent(this._dynamicCanvasNode.transform, false);
            AddFullScreenRectTransform(_topLayerRootNode, true);

            //tutorial 放在动态Canvas上
            _tutorialRooNode = new GameObject("Tutorial");
            _tutorialRooNode.transform.SetParent(this._dynamicCanvasNode.transform, false);
            AddFullScreenRectTransform(_tutorialRooNode, true);

            //Toast 放在动态Canvas上
            _toastRootNode = new GameObject("Toast");
            _toastRootNode.transform.SetParent(this._dynamicCanvasNode.transform, false);
            AddFullScreenRectTransform(_toastRootNode, true);
        }

        /// <summary>
        /// 屏蔽所有触摸
        /// </summary>
        public void StopTouches()
        {
            if (_eventSystem != null) _eventSystem.gameObject.SetActive(false);
        }
        /// <summary>
        /// 恢复触摸
        /// </summary>
        public void ResumeTouches()
        {
            if (_eventSystem != null) _eventSystem.gameObject.SetActive(true);
        }

        /// <summary>
        /// 添加层
        /// </summary>
        public void AddLayer(BaseUILayer baseUI)
        {
            baseUI.Awake();
            baseUI.Start();
            if(baseUI.gameObject == null)
            {
                throw new Exception("Start 中需要完成 gameObject的初始化");
            }

            Type uiType = baseUI.GetType();
            _layers.Add(uiType.Name, baseUI);
            
            LayerType layerType = LayerType.NORMAL_DYNAMIC;
            int siblingIndex = 0;
            if(uiType.IsDefined(typeof(LayerParamsAttribute), false))
            {
                LayerParamsAttribute layerParamsAttribute = uiType.GetCustomAttribute<LayerParamsAttribute>();
                layerType = layerParamsAttribute.layerType;
                siblingIndex = layerParamsAttribute.order;
            }
            baseUI.gameObject.transform.SetParent(SeakLayerNodeByType(layerType).transform, false);
            baseUI.gameObject.transform.SetSiblingIndex(siblingIndex);
            AddFullScreenRectTransform(baseUI.gameObject);
            baseUI.ShowLayer(this);
        }

        /// <summary>
        /// 添加层
        /// </summary>
        public void RemoveLayer(string layerName)
        {
            if (_layers.TryGetValue(layerName, out BaseUILayer layerUIView))
            {
                layerUIView.Destroy();
                layerUIView.Destroyed();
                _layers.Remove(layerName);
            }
        }

        /// <summary>
        /// 添加层
        /// </summary>
        public void RemoveAllLayer()
        {
            foreach (var kv in _layers)
            {
                kv.Value.Destroy();
            }
            _layers.Clear();
        }

        /// <summary>
        /// 显示Layer
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="layerType"></param>
        public void ShowLayer(string layerName, object who)
        {
            if(_layers.TryGetValue(layerName, out BaseUILayer layerUIView))
            {
                layerUIView.ShowLayer(who);
            }
        }

        /// <summary>
        /// 隐藏Layer
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="layerType"></param>
        public void HideLayer(string layerName, object who)
        {
            if (_layers.TryGetValue(layerName, out BaseUILayer layerUIView))
            {
                layerUIView.HideLayer(who);
            }
        }

        /// <summary>
        /// 显示全部Layer - 与HideAllLayer配合使用
        /// </summary>
        /// <param name="layerType"></param>
        public void ShowAllLayer(object who)
        {
            foreach (var  kv in _layers)
            {
                kv.Value.ShowLayer(who);
            }
        }
        
        /// <summary>
        /// 隐藏全部Layer - 与ShowAllLayer配合使用
        /// </summary>
        /// <param name="layerType"></param>
        public void HideAllLayer(object who)
        {
            foreach (var kv in _layers)
            {
                kv.Value.HideLayer(who);
            }
        }


        /// <summary>
        /// 显示全部Layer - 与HideAllLayer配合使用
        /// </summary>
        /// <param name="layerType"></param>
        /// <param name="layerName"></param>
        public T GetLayer<T>() where T : BaseUILayer
        {
            _layers.TryGetValue(typeof(T).Name, out BaseUILayer uiLayer);
            return (T)uiLayer;
        }
        

        /// <summary>
        /// 设置全屏transform
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public RectTransform AddFullScreenRectTransform(GameObject gameObject, bool safeAreaComponent = false)
        {
            //铺满屏幕
            RectTransform rect = gameObject.HasComponent<RectTransform>() ? gameObject.GetComponent<RectTransform>() : gameObject.AddComponent<RectTransform>();
            rect.localPosition = Vector3.zero;
            rect.localScale = Vector3.one;
            rect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 0);
            rect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 0, 0);
            rect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, 0);
            rect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 0, 0);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            if (safeAreaComponent) gameObject.AddComponent<UISafeAreaLayout>();
            return rect;
        }


        /// <summary>
        /// 3D世界坐标转其他UI坐标
        /// </summary> 
        /// <param name="worldPos">3D世界坐标</param>
        /// <param name="camera">映射的相机</param>
        /// <param name="dynamicCanvas">默认true</param>
        /// <returns></returns>
        public Vector2 WorldToUIPoint(Vector3 worldPos, Camera camera, bool dynamicCanvas = true)
        {
            // 先将3D坐标转换成屏幕坐标
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(camera, worldPos);
            // 再将屏幕坐标转换成UGUI坐标
            RectTransformUtility.ScreenPointToLocalPointInRectangle(dynamicCanvas ? _dynamicCanvasNode.GetComponent<RectTransform>() : _canvasNode.GetComponent<RectTransform>(), screenPoint, null, out Vector2 resultPos);
            return resultPos;
        }

        /// <summary>
        /// UI世界坐标转其他UI坐标
        /// </summary> 
        /// <param name="worldPos">UI世界坐标</param>
        /// <param name="dynamicCanvas">默认true</param>
        /// <returns></returns>
        public Vector2 UIWorldToScreenPos(Vector3 worldPos, bool dynamicCanvas = true)
        {

            RectTransform rectTransform = dynamicCanvas ? _dynamicCanvasNode.GetComponent<RectTransform>() : _canvasNode.GetComponent<RectTransform>();

            Camera uiCamera = rectTransform.GetComponent<Canvas>().worldCamera;
            // 先将UI坐标转换成屏幕坐标
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, worldPos);

            return screenPoint;
        }

        /// <summary>
        /// UI世界坐标转其他UI坐标
        /// </summary> 
        /// <param name="worldPos">UI世界坐标</param>
        /// <param name="dynamicCanvas">默认true</param>
        /// <returns></returns>
        public Vector2 UIWorldToUIPoint(Vector3 worldPos, bool dynamicCanvas = true)
        {

            RectTransform rectTransform = dynamicCanvas ? _dynamicCanvasNode.GetComponent<RectTransform>() : _canvasNode.GetComponent<RectTransform>();

            Camera UICamera = rectTransform.GetComponent<Canvas>().worldCamera;
            // 先将UI坐标转换成屏幕坐标
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(UICamera, worldPos);
            // 再将屏幕坐标转换成UGUI坐标
            bool value = RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, UICamera, out Vector2 resultPos);
            return resultPos;
        }

        /// <summary>
        /// 设计分辨率
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void SetResolution(int width, int height)
        {
            if (_staticCanvasScaler == null)
            {
                EasyLogger.LogError("EasyFrameWork", "UIView SetResolution Failed ,Please Init TJFramework First!");
                return;
            }
            _staticCanvasScaler.referenceResolution = new Vector2(width, height);
            _dynamicCanvasScaler.referenceResolution = new Vector2(width, height);
        }

        public override void RemoveInstance()
        {
            base.RemoveInstance();
            RemoveAllLayer();
            UnityEngine.Object.DestroyImmediate(_staticCanvasNode);
            UnityEngine.Object.DestroyImmediate(_dynamicCanvasNode);
            UnityEngine.Object.DestroyImmediate(_eventSystemNode);
        }
    }

}