using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using System;

namespace Easy 
{
    
    /// <summary>
    /// 弹窗显示黑遮罩
    /// </summary>
    public class DialogParamsAttribute : System.Attribute 
    {
        /// <summary>
        /// 是否显示遮罩
        /// </summary>
        public bool isShowMask;
        
        /// <summary>
        /// 是否触发遮罩点击
        /// </summary>
        public bool triggerMaskClick;
    }

    /// <summary>
    /// 弹窗执行枚举
    /// </summary>
    public enum DialogExecEnum
    {
        Open,
        Close,
    }

    /// <summary>
    /// 弹窗执行参数
    /// </summary>
    public class DialogExecParam
    {
        /// <summary>
        /// 执行类型
        /// </summary>
        public DialogExecEnum execEnum;
        
        /// <summary>
        /// 弹窗UI实例
        /// </summary>
        public BaseDialogUI dialogUI;
        
        /// <summary>
        /// 弹窗UI名称
        /// </summary>
        public string dialogUIName;
    }
    
    /// <summary>
    /// 对话框UI层
    /// </summary>
    [LayerParams(layerType = LayerType.DIALOG, order = 1)]
    public class DialogUILayer : BaseUILayer
    {
        /// <summary>
        /// 缓存所有对话框
        /// </summary>
        public Dictionary<string, BaseDialogUI> dialogCaches = new Dictionary<string, BaseDialogUI>();

        /// <summary>
        /// 当前显示的对话框列表
        /// </summary>
        public List<BaseDialogUI> dialogs = new List<BaseDialogUI>();

        /// <summary>
        /// 当前显示的对话框
        /// </summary>
        public BaseDialogUI nowDialog => dialogs.Count > 0 ? dialogs[^1] : null;

        /// <summary>
        /// 对话框执行队列
        /// </summary>
        public Queue<DialogExecParam> dialogsExecQueue = new Queue<DialogExecParam>();

        /// <summary>
        /// 执行锁
        /// </summary>
        private bool _execLock = false;

        /// <summary>
        /// 所有对话框类型
        /// </summary>
        private Dictionary<string,Type> _allDialogTypes = new Dictionary<string, Type>();

        /// <summary>
        /// 初始化
        /// </summary>
        public override void Awake()
        {
            base.Awake();
            InitDialogTypeInfos();
        }

        /// <summary>
        /// 反射初始化所有Dialog类型
        /// </summary>
        /// <exception cref="Exception"></exception>
        private void InitDialogTypeInfos()
        {
            //查找全部数据类
            List<Type> types = EasyFrameworkMain.Instance.GetTypes();
            var BaseDialogUIType = typeof(BaseDialogUI);
            foreach (var t in types)
            {
                if (BaseDialogUIType.IsAssignableFrom(t) && t != BaseDialogUIType)
                {
                    string key = t.Name;
                    if(_allDialogTypes.ContainsKey(key))
                    {
                        throw new Exception("DialogUI 不允许有重名");
                    }
                    _allDialogTypes.Add(key, t);
                }
            }
        }

        /// <summary>
        /// 显示弹窗
        /// </summary>
        /// <param name="dialogUIName">弹窗名称</param>
        /// <param name="prefabPath">预制路径</param>
        public void OpenDialog(string dialogUIName, string prefabPath)
        {
            if(_allDialogTypes.ContainsKey(dialogUIName))
            {
                BaseDialogUI obj = (BaseDialogUI)Activator.CreateInstance(_allDialogTypes[dialogUIName]);
                obj.SetPrefabPath(prefabPath);
                OpenDialog(obj);
            }
        }

        /// <summary>
        /// 显示弹窗
        /// </summary>
        /// <param name="dialogUIName">弹窗名称</param>
        /// <param name="dialogGameObject">显示对象</param>
        public void OpenDialog(string dialogUIName, GameObject dialogGameObject)
        {
            if(_allDialogTypes.ContainsKey(dialogUIName))
            {
                BaseDialogUI obj = (BaseDialogUI)Activator.CreateInstance(_allDialogTypes[dialogUIName]);
                obj.SetGameObject(dialogGameObject);
                OpenDialog(obj);
            }
        }

        /// <summary>
        /// 显示弹窗
        /// </summary>
        /// <param name="dialog">弹窗实例</param>
        public void OpenDialog(BaseDialogUI dialogUI)
        {
            dialogsExecQueue.Enqueue(new DialogExecParam()
            {
                execEnum = DialogExecEnum.Open,
                dialogUI = dialogUI
            });
            ExecQueue();
        }

        /// <summary>
        /// 移除弹窗
        /// </summary>
        public void CloseDialog(string dialogUIName)
        {
            dialogsExecQueue.Enqueue(new DialogExecParam()
            {
                execEnum = DialogExecEnum.Close,
                dialogUIName = dialogUIName
            });
            ExecQueue();
        }

        /// <summary>
        /// 移除弹窗
        /// </summary>
        public void CloseDialog(BaseDialogUI dialogUI)
        {
            dialogsExecQueue.Enqueue(new DialogExecParam()
            {
                execEnum = DialogExecEnum.Close,
                dialogUI = dialogUI
            });
            ExecQueue();
        }

        /// <summary>
        /// 移除所有弹窗
        /// </summary>
        public void CloseAllDialog()
        {
            foreach (var dialogUIView in dialogs)
            {
                CloseDialogInterval(dialogUIView);
            }
            dialogs.Clear();
            dialogsExecQueue.Clear();
            _execLock = false;
        }

        /// <summary>
        /// 执行队列
        /// </summary>
        public void ExecQueue()
        {
            if (!_execLock && dialogsExecQueue.Count > 0)
            {
                _execLock = true;
                var dialogExecParam = dialogsExecQueue.Dequeue();
                if (dialogExecParam.execEnum == DialogExecEnum.Open)
                {
                    ExecOpenDialog(dialogExecParam);
                }
                else if (dialogExecParam.execEnum == DialogExecEnum.Close)
                {
                    ExecCloseDialog(dialogExecParam);
                }
            }
        }

        /// <summary>
        /// 解除队列锁并执行队列
        /// </summary>
        public void UnLockAndExecQueue()
        {
            _execLock = false;
            ExecQueue();
        }

        /// <summary>
        /// 执行打开弹窗
        /// </summary>
        private void ExecOpenDialog(DialogExecParam dialogExecParam)
        {
            HideDialogInterval(nowDialog,()=>
            {
                ShowDialogInterval(dialogExecParam.dialogUI, UnLockAndExecQueue);
            });
        }

        /// <summary>
        /// 执行关闭弹窗
        /// </summary>
        private void ExecCloseDialog(DialogExecParam dialogExecParam)
        {
            for (int i = 0, count = dialogs.Count; i < count; ++i)
            {
                if(dialogs[i].GetUIName() == dialogExecParam.dialogUIName)
                {
                    ExecCloseDialog(dialogs[i]);
                    break;
                }
            }
        }

        /// <summary>
        /// 执行关闭弹窗
        /// </summary>
        private void ExecCloseDialog(BaseDialogUI dialog)
        {
            if(nowDialog == dialog)
            {
                HideDialogInterval(nowDialog, ()=>{
                    CloseDialogInterval(nowDialog);
                    if(dialogsExecQueue.Count == 0)
                    {
                        dialogsExecQueue.Enqueue(new DialogExecParam()
                        {
                            execEnum = DialogExecEnum.Open,
                            dialogUI = nowDialog
                        });
                    }
                    UnLockAndExecQueue();
                });
            }
            else
            {
                CloseDialogInterval(dialog);
                UnLockAndExecQueue();
            }
        }

        /// <summary>
        /// 展示弹窗内部接口
        /// </summary>
        /// <param name="dialog">弹窗实例</param>
        /// <param name="callback">回调函数</param>
        private void ShowDialogInterval(BaseDialogUI dialog, Action callback)
        {
            if(dialog == null)
            {
                callback();
                return;
            }

            if(dialogs.Contains(dialog))
            {
                dialog.gameObject.transform.parent.gameObject.SetActive(true);
                dialog.Show(callback);
            }
            else
            {
                dialogs.Add(dialog);
                dialog.Awake();
                dialog.Start();

                bool isShowMask = true;
                bool isTriggerMaskClick = true;
                if(dialog.GetType().IsDefined(typeof(DialogParamsAttribute), false))
                {
                    DialogParamsAttribute dialogParams = dialog.GetType().GetCustomAttribute<DialogParamsAttribute>();
                    isShowMask = dialogParams.isShowMask;
                    isTriggerMaskClick = dialogParams.triggerMaskClick;
                }

                GameObject blackMask = new GameObject();
                Image image = blackMask.AddComponent<Image>();
                image.raycastTarget = false;
                image.material = new Material(Shader.Find("UI/Default"));
                image.color = new Color(0, 0, 0, 0);
                if (isShowMask)
                {
                    image.raycastTarget = true;
                }
                if (isTriggerMaskClick)
                {
                    //是否点击关闭
                    image.raycastTarget = true;
                    Button button = blackMask.AddComponent<Button>();
                    button.onClick.AddListener(dialog.OnClickMask);
                }
                UIMgr.Instance.AddFullScreenRectTransform(blackMask);
                //blackMask.GetComponent<RectTransform>().offsetMax = new Vector2(0.0f, 200);
                dialog.gameObject.transform.SetParent(blackMask.transform);
                blackMask.transform.SetParent(gameObject.transform, false);
                dialog.gameObject.transform.localPosition = Vector3.zero;
                dialog.gameObject.transform.localScale = Vector3.one;
                UIMgr.Instance.AddFullScreenRectTransform(dialog.gameObject);
                dialog.Show(callback);
            }
        }

        /// <summary>
        /// 隐藏弹窗内部接口
        /// </summary>
        /// <param name="dialog">弹窗实例</param>
        /// <param name="callback">回调函数</param>
        private void HideDialogInterval(BaseDialogUI dialog, Action callback)
        {
            if(dialog == null)
            {
                callback();
                return;
            }
            dialog.Hide(()=>{
                dialog.gameObject.transform.parent.gameObject.SetActive(false);
                callback();
            });
        }

        /// <summary>
        /// 移除弹窗没有hide
        /// </summary>
        private void CloseDialogInterval(BaseDialogUI dialog)
        {
            dialogs.Remove(dialog);
            dialog.Destroy();
            dialog.Destroyed();
            var backMask = dialog.gameObject.transform.parent.gameObject;
            GameObject.Destroy(backMask);
        }

        /// <summary>
        /// 获得弹窗
        /// </summary>
        /// <param name="dialogUiName">弹窗名称</param>
        /// <returns>弹窗实例</returns>
        public BaseDialogUI GetDialog(string dialogUiName)
        {
            for (int i = 0, count = dialogs.Count; i < count; ++i)
            {
                if(dialogs[i].GetUIName() == dialogUiName)
                {
                    return dialogs[i];
                }
            }
            return null;
        }

        /// <summary>
        /// 安卓返回键
        /// </summary>
        public void OnDialogBackPressed()
        {
            nowDialog?.OnBackPressed();
        }

        /// <summary>
        /// 销毁
        /// </summary>
        public override void Destroy()
        {
            base.Destroy();
            CloseAllDialog();
        }
    }
}