using System;
namespace Easy
{
    public class BaseDialogUI : BaseUI
    {

        public void Close()
        {
            Easy.UIMgr.Instance.GetLayer<DialogUILayer>().RemoveDialog(this);
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