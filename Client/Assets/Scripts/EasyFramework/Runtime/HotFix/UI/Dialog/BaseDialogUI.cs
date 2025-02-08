using System;
namespace Easy
{
    /// <summary>
    /// 基础对话框UI类，继承自BaseUI
    /// </summary>
    public class BaseDialogUI : BaseUI
    {
        /// <summary>
        /// 关闭当前对话框
        /// </summary>
        public void Close()
        {
            Easy.UIMgr.Instance.GetLayer<DialogUILayer>().CloseDialog(this);
        }

        /// <summary>
        /// 处理返回键事件，默认行为是关闭对话框
        /// </summary>
        public virtual void OnBackPressed()
        {
            Close();
        }

        /// <summary>
        /// 处理遮罩点击事件，默认行为是关闭对话框
        /// </summary>
        public virtual void OnClickMask()
        {
            Close();
        }
    }
}