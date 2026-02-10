using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Rect = UnityEngine.Rect;


/// <summary>
/// 文本控件,支持超链接
/// </summary>
public class HyperlinkText : Text, IPointerClickHandler
{
    /// <summary>
    /// 超链接信息类
    /// </summary>
    private class HyperlinkInfo
    {
        public int startIndex;

        public int endIndex;

        public string name;

        public readonly List<Rect> boxes = new List<Rect>();

        public List<int> linefeedIndexList = new List<int>();
    }

    /// <summary>
    /// 解析完最终的文本
    /// </summary>
    private string m_OutputText;

    /// <summary>
    /// 超链接信息列表
    /// </summary>
    private readonly List<HyperlinkInfo> m_HrefInfos = new List<HyperlinkInfo>();

    /// <summary>
    /// 文本构造器
    /// </summary>
    protected static readonly StringBuilder s_TextBuilder = new StringBuilder();

    protected static readonly StringBuilder s_TextBuilder1 = new StringBuilder();

    [Serializable]
    public class HrefClickEvent : UnityEvent<string> { }

    [SerializeField]
    private HrefClickEvent m_OnHrefClick = new HrefClickEvent();

    public System.Action<string> onHrefClickAction;

#if UNITY_EDITOR
    [Tooltip("设置超链接颜色")]
#endif
    public bool setHyperColor = true;

    /// <summary>
    /// 超链接点击事件
    /// </summary>
    public HrefClickEvent onHrefClick
    {
        get { return m_OnHrefClick; }
        set { m_OnHrefClick = value; }
    }


    /// <summary>
    /// 超链接正则
    /// </summary>
    private static readonly Regex s_HrefRegex = new Regex(@"<link=([^>\n\s]+)>(.*?)(</link>)", RegexOptions.Singleline);


    public string GetHyperlinkInfo
    {
        get { return text; }
    }

    // protected override void Awake()
    // {
    //     base.Awake();
    // }

    // protected override void OnEnable()
    // {
    //     base.OnEnable();
    //     // if(mHyperlinkText!=null && mHyperlinkText.onHrefClick != null)
    //     //     mHyperlinkText.onHrefClick.AddListener(OnHyperlinkTextInfo);
    //     // if (Application.isPlaying && Game.App.GameApp.Instance!=null && font!=null && Game.App.GameApp.Instance.Language == Game.Config.Language.ru && font.name.Contains("Arial"))
    //     // {
    //     //     font = Resources.GetBuiltinResource<Font>("Arial.ttf");
    //     // }
    // }

    // protected override void OnDisable()
    // {
    //     base.OnDisable();
    //     // if(mHyperlinkText!=null && mHyperlinkText.onHrefClick != null)
    //     //     mHyperlinkText.onHrefClick.RemoveListener(OnHyperlinkTextInfo);
    // }


    public override void SetVerticesDirty()
    {
        base.SetVerticesDirty();
#if UNITY_EDITOR
        //if (UnityEditor.PrefabUtility.GetPrefabType(this) == UnityEditor.PrefabType.Prefab)
        if (UnityEditor.PrefabUtility.GetPrefabAssetType(this) == UnityEditor.PrefabAssetType.Regular)
        {
            return;
        }
#endif
        //  m_OutputText = GetOutputText(text);
        text = GetHyperlinkInfo;
        m_OutputText = GetOutputText(text);

    }


    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        var orignText = m_Text;
        m_Text = m_OutputText;
        base.OnPopulateMesh(toFill);
        m_Text = orignText;
        UIVertex vert = new UIVertex();

        // 处理超链接包围框
        foreach (var hrefInfo in m_HrefInfos)
        {
            hrefInfo.boxes.Clear();
            hrefInfo.linefeedIndexList.Clear();
            if (hrefInfo.startIndex >= toFill.currentVertCount)
            {
                continue;
            }

            // 将超链接里面的文本顶点索引坐标加入到包围框
            toFill.PopulateUIVertex(ref vert, hrefInfo.startIndex);
            var pos = vert.position;
            var bounds = new Bounds(pos, Vector3.zero);
            hrefInfo.linefeedIndexList.Add(hrefInfo.startIndex);
            for (int i = hrefInfo.startIndex, m = hrefInfo.endIndex; i < m; i++)
            {
                if (i >= toFill.currentVertCount)
                {
                    break;
                }

                toFill.PopulateUIVertex(ref vert, i);
                pos = vert.position;
                if (pos.x < bounds.min.x) // 换行重新添加包围框
                {
                    hrefInfo.boxes.Add(new Rect(bounds.min, bounds.size));
                    hrefInfo.linefeedIndexList.Add(i);
                    bounds = new Bounds(pos, Vector3.zero);
                }
                else
                {
                    bounds.Encapsulate(pos); // 扩展包围框
                }
            }
            hrefInfo.boxes.Add(new Rect(bounds.min, bounds.size));
        }
    }

    public string NewIndexJS(string str)
    {
        var str2 = string.Copy(str);
        str2 = Regex.Replace(str2, @"\s", "");
        str2 = Regex.Replace(str2, @"<color=(.*?)>|</color>", "");
        str2 = Regex.Replace(str2, @"<size=(.*?)>|</size>", "");
        str2 = Regex.Replace(str2, @"<b>|</b>", "");
        str2 = Regex.Replace(str2, @"<i>|</i>", "");
        return str2;
    }

    /// <summary>
    /// 获取超链接解析后的最后输出文本
    /// </summary>
    /// <returns></returns>
    protected virtual string GetOutputText(string outputText)
    {
        s_TextBuilder.Length = 0;
        s_TextBuilder1.Length = 0;
        m_HrefInfos.Clear();
        var indexText = 0;
        string newout = NewIndexJS(outputText);
        string newout2 = string.Copy(outputText);
        foreach (Match match in s_HrefRegex.Matches(newout))
        {
            s_TextBuilder.Append(newout.Substring(indexText, match.Index - indexText));
            var group = match.Groups[1];
            var hrefInfo = new HyperlinkInfo
            {
                startIndex = s_TextBuilder.Length * 4, // 超链接里的文本起始顶点索引

                endIndex = (s_TextBuilder.Length + match.Groups[2].Length - 1) * 4 + 3,
                name = group.Value
            };
            m_HrefInfos.Add(hrefInfo);
            s_TextBuilder.Append(match.Groups[2].Value);
            indexText = match.Index + match.Length;
        }
        indexText = 0;
        foreach (Match match in s_HrefRegex.Matches(newout2))
        {
            s_TextBuilder1.Append(newout2.Substring(indexText, match.Index - indexText));
            if (setHyperColor)
                s_TextBuilder1.Append("<color=blue>");
            s_TextBuilder1.Append(match.Groups[2].Value);
            if (setHyperColor)
                s_TextBuilder1.Append("</color>");
            indexText = match.Index + match.Length;
        }

        s_TextBuilder1.Append(newout2.Substring(indexText, newout2.Length - indexText));
        return s_TextBuilder1.ToString();
    }

    /// <summary>
    /// 点击事件检测是否点击到超链接文本
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData)
    {
        Vector2 lp = Vector2.zero;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out lp);
        foreach (var hrefInfo in m_HrefInfos)
        {
            var boxes = hrefInfo.boxes;
            for (var i = 0; i < boxes.Count; ++i)
            {
                if (boxes[i].Contains(lp))
                {
                    m_OnHrefClick?.Invoke(hrefInfo.name);
                    onHrefClickAction?.Invoke(hrefInfo.name);
                    return;
                }
            }
        }
    }
}
