using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using System;

namespace Easy 
{
    
    // 弹窗显示黑遮罩
    public class DialogParamsAttribute : System.Attribute 
    {
        public bool isShowMask;
        public bool triggerMaskClick;
    }

    public enum DialogExecEnum
    {
        Open,
        Close,
    }

    public class DialogExecParam
    {
        public DialogExecEnum execEnum;
        public BaseDialogUI dialogUI;
        public string dialogUIName;
    }
    
    [LayerParams(layerType = LayerType.DIALOG, order = 1)]
    public class DialogUILayer : BaseUILayer
    {
        public Dictionary<string, BaseDialogUI> dialogCaches = new Dictionary<string, BaseDialogUI>();

        public List<BaseDialogUI> dialogs = new List<BaseDialogUI>();

        public BaseDialogUI nowDialog => dialogs.Count > 0 ? dialogs[^1] : null;

        public Queue<DialogExecParam> dialogsExecQueue = new Queue<DialogExecParam>();

        private bool _execLock = false;

        private Dictionary<string,Type> _allDialogTypes = new Dictionary<string, Type>();

        public override void Awake()
        {
            base.Awake();
            InitDialogTypeInfos();
        }

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
        /// <param name="dialogUIName"></param>
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
        /// <param name="dialogUIName"></param>
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
        /// <param name="dialog"></param>
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

        public void UnLockAndExecQueue()
        {
            _execLock = false;
            ExecQueue();
        }


        private void ExecOpenDialog(DialogExecParam dialogExecParam)
        {
            HideDialogInterval(nowDialog,()=>
            {
                ShowDialogInterval(dialogExecParam.dialogUI, UnLockAndExecQueue);
            });
        }

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
        /// 关闭弹窗
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
                dialogs.Push(dialog);
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
        public BaseDialogUI GetDialog(string uiName)
        {
            for (int i = 0, count = dialogs.Count; i < count; ++i)
            {
                if(dialogs[i].GetUIName() == uiName)
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
