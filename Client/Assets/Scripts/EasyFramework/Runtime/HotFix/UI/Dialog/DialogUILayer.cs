using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;

namespace Easy 
{
    
    // 弹窗显示黑遮罩
    public class DialogParamsAttribute : System.Attribute 
    {
        public bool isShowMask;
        public bool triggerMaskClick;
    }
    
    [LayerParams(layerType = LayerType.DIALOG, order = 1)]
    public class DialogUILayer : BaseUILayer
    {
        public Dictionary<string, BaseDialogUI> dialogCaches = new Dictionary<string, BaseDialogUI>();

        public List<BaseDialogUI> dialogs = new List<BaseDialogUI>();

        public BaseDialogUI nowDialog;

        /// <summary>
        /// 显示弹窗
        /// </summary>
        /// <param name="prbDialog"></param>
        /// <param name="dialog"></param>
        public void ShowDialog(BaseDialogUI dialog)
        {
            var preDialog = nowDialog;
            if (preDialog != null)
            {
                preDialog.Hide(()=>
                {
                    dialogs.Push(dialog);
                    preDialog.gameObject.transform.parent.gameObject.SetActive(false);
                    ShowDialogInterval(dialog);
                });
            }
            else
            {
                ShowDialogInterval(dialog);
            }
        }

        private void ShowDialogInterval(BaseDialogUI dialog)
        {
            nowDialog = dialog;
            nowDialog.Awake();
            nowDialog.Start();

            bool isShowMask = true;
            bool isTriggerMaskClick = true;
            if(nowDialog.GetType().IsDefined(typeof(DialogParamsAttribute), false))
            {
                DialogParamsAttribute dialogParams = nowDialog.GetType().GetCustomAttribute<DialogParamsAttribute>();
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
                button.onClick.AddListener(nowDialog.OnClickMask);
            }
            UIMgr.Instance.AddFullScreenRectTransform(blackMask);
            //blackMask.GetComponent<RectTransform>().offsetMax = new Vector2(0.0f, 200);
            dialog.gameObject.transform.SetParent(blackMask.transform);
            blackMask.transform.SetParent(gameObject.transform, false);
            dialog.gameObject.transform.localPosition = Vector3.zero;
            dialog.gameObject.transform.localScale = Vector3.one;
            UIMgr.Instance.AddFullScreenRectTransform(dialog.gameObject);

            nowDialog.Show();
        }

        /// <summary>
        /// 移除弹窗
        /// </summary>
        public void RemoveDialog(string uiName)
        {
            for (int i = 0, count = dialogs.Count; i < count; ++i)
            {
                if(dialogs[i].GetUIName() == uiName)
                {
                    RemoveDialog(dialogs[i]);
                    break;
                }
            }
        }

        /// <summary>
        /// 移除弹窗
        /// </summary>
        public void RemoveDialog(BaseDialogUI dialog)
        {
            if(nowDialog == dialog)
            {
                nowDialog.Hide(RemoveNowDialogHideCallback);
            }
            else
            {
                RemoveDialogInterval(dialog);
            }
        }

        /// <summary>
        /// 正在显示的弹窗关闭后的回调
        /// </summary>
        private void RemoveNowDialogHideCallback()
        {
            RemoveDialogInterval(nowDialog);
            nowDialog = null;
            if (dialogs.Count > 0)
            {
                nowDialog = dialogs.Pop();
                nowDialog.gameObject.transform.parent.gameObject.SetActive(true);
                nowDialog.Show();
            }
        }

        /// <summary>
        /// 移除弹窗没有hide
        /// </summary>
        private void RemoveDialogInterval(BaseDialogUI dialog)
        {
            dialogs.Remove(dialog);
            dialog.Destroy();
            dialog.Destroyed();
            var backMask = dialog.gameObject.transform.parent.gameObject;
            Object.Destroy(backMask);
        }

        /// <summary>
        /// 移除所有弹窗
        /// </summary>
        public void RemoveAllDialog()
        {
            foreach (var dialogUIView in dialogs)
            {
                RemoveDialogInterval(dialogUIView);
            }
            dialogs.Clear();
            if(nowDialog != null)
            {
                RemoveDialogInterval(nowDialog);
            }
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
            RemoveAllDialog();
        }
    }
}
