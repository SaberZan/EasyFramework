using System;
namespace Easy
{
    public class BaseDialogUI : BaseUI
    {

        public void Close()
        {
            Easy.UIMgr.Instance.GetLayer<DialogUILayer>().CloseDialog(this);
        }

        public virtual void OnBackPressed()
        {
            Close();
        }

        public virtual void OnClickMask()
        {
            Close();
        }
    }
}