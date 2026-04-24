using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Easy
{

    public class I18NMgr : Singleton<I18NMgr>
    {
        public SystemLanguage currentLanguage;

        public override void BeforeRestart()
        {

        }

        public override void Init(InitCompleteCallback complete)
        {
            ProxyMgr.Instance.Get<I18NProxy>().Init(() =>
            {
                return GetLanguageCode(Application.systemLanguage);
            });
            currentLanguage = GetLanguageByCode(ProxyMgr.Instance.Get<I18NProxy>().GetLanguageCode());
            complete.Invoke(true);
        }

        /// <summary>
        /// 更新语言并保存
        /// </summary>
        /// <param name="code"></param>
        public void UpdateLanguageByCode(string code)
        {
            SystemLanguage language = GetLanguageByCode(code);
            if (currentLanguage != language)
            {
                currentLanguage = language;
                ProxyMgr.Instance.Get<I18NProxy>().ChangeLanguageCode(code);
            }
        }
        
        public SystemLanguage GetLanguageByCode(string code)
        {
            SystemLanguage language = SystemLanguage.English;
            switch (code)
            {
                case "zh":
                    language = SystemLanguage.ChineseSimplified;
                    break;
                case "ja":
                    language = SystemLanguage.Japanese;
                    break;
                case "zh_tw":
                    language = SystemLanguage.ChineseTraditional;
                    break;
                case "de":
                    language = SystemLanguage.German;
                    break;
                case "fr":
                    language = SystemLanguage.French;
                    break;
                case "it":
                    language = SystemLanguage.Italian;
                    break;
                case "ru":
                    language = SystemLanguage.Russian;
                    break;
                case "es":
                    language = SystemLanguage.Spanish;
                    break;
                case "en":
                default:
                    language = SystemLanguage.English;
                    break;
            }
            // 更新到玩家数据TODO
            return language;
        }

        public string GetCurrentLanguageCode()
        {
            return GetLanguageCode(currentLanguage);
        }
        
        public string GetLanguageCode(SystemLanguage language)
        {
            switch (language)
            {
                case SystemLanguage.ChineseSimplified:
                    return "zh";
                case SystemLanguage.Japanese:
                    return "ja";
                case SystemLanguage.ChineseTraditional:
                    return "zh_tw";
                case SystemLanguage.German:
                    return "de";
                case SystemLanguage.French:
                    return "fr";
                case SystemLanguage.Italian:
                    return "it";
                case SystemLanguage.Russian:
                    return "ru";
                case SystemLanguage.Spanish:
                    return "es";
                case SystemLanguage.Arabic:
                    return "ar";
                case SystemLanguage.English:
                default:
                    return "en";
            }
        }

        public string GetShowContextString(string key, params object[] args)
        {
            string content = key;
            content = content.Replace("\\n", "\n");
            content = args.Length == 0 ? content : string.Format(content, args);
            if(currentLanguage == SystemLanguage.Arabic)
            {
                content = ArabicSupport.ArabicFixer.ArabicWithRow(content);
            }
            return content;
        }

        public string GetShowSpritePath(string key)
        {
            string content = key;
            content = content.Replace("\\n", "\n");
            return content;
        }
    }

}