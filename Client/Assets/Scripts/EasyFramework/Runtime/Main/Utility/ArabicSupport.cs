//-----------------------------------------------------------------------------
/// <summary>
/// This is an Open Source File Created by: Abdullah Konash. Twitter: @konash
/// This File allow the users to use arabic text in XNA and Unity platform.
/// It flips the characters and replace them with the appropriate ones to connect the letters in the correct way.
///
/// The project is available on GitHub here: https://github.com/Konash/arabic-support-unity
/// Unity Asset Store link: https://www.assetstore.unity3d.com/en/#!/content/2674
/// Please help in improving the plugin.
///
/// I would love to see the work you use this plugin for. Send me a copy at: abdullah.konash[at]gmail[dot]com
/// </summary>
///
/// <license>
/// MIT License
///
/// Copyright(c) 2018
/// Abdullah Konash
///
/// Permission is hereby granted, free of charge, to any person obtaining a copy
/// of this software and associated documentation files (the "Software"), to deal
/// /// in the Software without restriction, including without limitation the rights
/// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
/// copies of the Software, and to permit persons to whom the Software is
/// furnished to do so, subject to the following conditions:
///
/// The above copyright notice and this permission notice shall be included in all
/// copies or substantial portions of the Software.
///
/// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
/// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
/// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
/// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
/// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
/// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
/// SOFTWARE.
/// </license>

//-----------------------------------------------------------------------------

#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
#endregion

namespace ArabicSupport
{
    public static class ArabicFixer
    {
        private const int MaxBuilderSize = 360;
        private const int DefaultCapacity = 0x10;

        [ThreadStatic] private static StringBuilder _cachedInstance;

        public static StringBuilder Get(int capacity = DefaultCapacity)
        {
            if(capacity <= MaxBuilderSize)
            {
                StringBuilder builder = _cachedInstance;
                if (builder != null)
                {
                    // Avoid stringBuilder block fragmentation by getting a new StringBuilder
                    // when the requested size is larger than the current capacity
                    if(capacity <= builder.Capacity)
                    {
                        _cachedInstance = null;
                        builder.Clear();
                        return builder;
                    }
                }
            }
            return new StringBuilder(capacity);
        }

        public static string ToStringRecycle(this StringBuilder builder)
        {
            string s = builder.ToString();
            Recycle(builder);
            return s;
        }

        public static string ToStringRecycle(this StringBuilder builder, int startIndex, int length)
        {
            string s = builder.ToString(startIndex, length);
            Recycle(builder);
            return s;
        }

        public static void Recycle(StringBuilder builder)
        {
            if (builder == null)
            {
                return;
            }
            if (builder.Capacity <= MaxBuilderSize)
            {
                _cachedInstance = builder;
            }
        }
    

        
        /// <summary>
        /// Fix the specified string.
        /// </summary>
        /// <param name='str'>
        /// String to be fixed.
        /// </param>
        public static string Fix(string str)
        {
            return Fix(str, false, true);
        }

        public static string Fix(string str, bool rtl)
        {
            if (rtl)

            {
                return Fix(str);
            } else
            {
                string[] words = str.Split(' ');
                string result = "";
                string arabicToIgnore = "";
                foreach (string word in words)
                {
                    if (char.IsLower(word.ToLower()[word.Length / 2]))
                    {
                        result += Fix(arabicToIgnore) + word + " ";
                        arabicToIgnore = "";
                    } else
                    {
                        arabicToIgnore += word + " ";
                    }
                }
                if (arabicToIgnore != "")
                    result += Fix(arabicToIgnore);

                return result;
            }
        }

        /// <summary>
        /// Fix the specified string with customization options.
        /// </summary>
        /// <param name='str'>
        /// String to be fixed.
        /// </param>
        /// <param name='showTashkeel'>
        /// Show tashkeel.
        /// </param>
        /// <param name='useHinduNumbers'>
        /// Use hindu numbers.
        /// </param>
        public static string Fix(string str, bool showTashkeel, bool useHinduNumbers)
        {
            ArabicFixerTool.showTashkeel = showTashkeel;
            ArabicFixerTool.useHinduNumbers = useHinduNumbers;

            if (str.Contains("\n"))
                str = str.Replace("\n", Environment.NewLine);

            if (str.Contains(Environment.NewLine))
            {
                string[] stringSeparators = new string[] { Environment.NewLine };
                string[] strSplit = str.Split(stringSeparators, StringSplitOptions.None);

                if (strSplit.Length == 0)
                    return ArabicFixerTool.FixLine(str);
                else if (strSplit.Length == 1)
                    return ArabicFixerTool.FixLine(str);
                else
                {
                    string outputString = ArabicFixerTool.FixLine(strSplit[0]);
                    int iteration = 1;
                    if (strSplit.Length > 1)
                    {
                        while (iteration < strSplit.Length)
                        {
                            outputString += Environment.NewLine + ArabicFixerTool.FixLine(strSplit[iteration]);
                            iteration++;
                        }
                    }
                    return outputString;
                }
            } else
            {
                return ArabicFixerTool.FixLine(str);
            }
        }

        public static string Fix(string str, bool showTashkeel, bool combineTashkeel, bool useHinduNumbers)
        {
            ArabicFixerTool.combineTashkeel = combineTashkeel;
            return Fix(str, showTashkeel, useHinduNumbers);
        }

        /// <summary>
        /// 包含颜色特殊字符的处理优先级低
        /// </summary>
        /// <param name="Str"></param>
        /// <returns></returns>
        public static string ArabicWithColor(string Str)
        {
            //根据<>切分
            string[] OldAry = Str.Split(new char[] { '<', '>' });
            //颜色栈
            Stack<string> ColorData = new Stack<string>();
            string tempString = "";
            //颜色部分都用*代替（切分符号）
            for (int i = 0; i < OldAry.Length; i++)
            {
                if (OldAry[i].Contains("color"))
                {
                    tempString += ArabicDefine.RichTextSymbol;
                    if (OldAry[i].Contains("#"))
                    {
                        ColorData.Push(OldAry[i]);
                    }
                } else
                {
                    tempString += OldAry[i];
                }
            }
            //翻译字符
            tempString = Fix(tempString, false, false);
            //通过*切分（切分符号）
            string[] arry = tempString.Split(ArabicDefine.RichTextSymbol);

            string newString = "";
            //拼接
            for (int i = 0; i < arry.Length; i++)
            {
                if (ColorData.Count != 0)
                {
                    newString += (i % 2 == 0) ? (arry[i] + "<" + ColorData.Pop() + ">") : (arry[i] + "</color>");
                } else
                {
                    newString += (i == arry.Length - 1) ? (arry[i]) : (arry[i] + "</color>");
                }
            }
            return newString;
        }

        /// <summary>
        /// 包含换行特殊字符（\n）的处理，优先级高。主要针对多行文字
        /// </summary>
        /// <param name="Str"></param>
        /// <returns></returns>
        public static string ArabicWithRow(string Str)
        {
            //Str = Str.Replace(" ", "");
            if (string.IsNullOrEmpty(Str))
                return Str;
            string[] Holder = Str.Split('\n');

            string newStr = "";
            for (int i = 0; i < Holder.Length; i++)
            {
                newStr += ArabicWithRichText(Holder[i]);
                if (i != Holder.Length - 1)
                {
                    newStr += "\n";
                }
            }
            return newStr;
        }

        public static string ArabicWithRichText(string text)
        {
            string tempString = ArabicFixerTool.RichTextToSymbol(text, out var richTexts);
            //翻译字符
            tempString = Fix(tempString, true, false);
            return ArabicFixerTool.SymbolToRichText(tempString, richTexts);
        }

        public static void FixLine(Text textComponent, string str, bool anchorRight = false)
        {
            List<string> resultText = new List<string>();
            List<string> paragraphList = str.Split('\n').ToList();

            if (paragraphList.Count >= 1)
            {
                var stringBuilder = Get();

                foreach (string paragraph in paragraphList)
                {
                    textComponent.text = paragraph;
                    Vector2 realWH = new Vector2(textComponent.preferredWidth, textComponent.preferredHeight);
                    TextGenerationSettings tgs = textComponent.GetGenerationSettings(Vector2.zero);
                    //tgs.scaleFactor = scaleFactor;
                    if (textComponent.text.IndexOf(' ') < 0)
                    {
                        resultText.Add(textComponent.text);
                    } else
                    {
                        string tempString = paragraph;
                        List<string> lineList = new List<string>();
                        List<string> wordList = tempString.Split(' ').ToList();
                        Dictionary<string, List<Vector2>> ranges = new Dictionary<string, List<Vector2>>();
                        Stack<int> stack = new Stack<int>();

                        while (true)
                        {
                            var match = Regex.Match(tempString, ArabicDefine.RichTextFixLine, RegexOptions.RightToLeft);
                            if (!match.Success)
                            { break; }
                            tempString = tempString.Substring(0, match.Index);
                            if (match.Value.StartsWith("</"))
                            {
                                stack.Push(match.Index);
                            } else if (stack.Count > 0)
                            {
                                if (!ranges.TryGetValue(match.Value, out var list))
                                {
                                    list = new List<Vector2>();
                                    ranges.Add(match.Value, list);
                                }
                                int x = 0;
                                if (tempString.Length > 0 && match.Index > 0)
                                {
                                    var match2 = Regex.Match(tempString, "( |<|>)", RegexOptions.RightToLeft);
                                    if (match2.Success)
                                    {
                                        x = match2.Index + 1;
                                    }
                                }
                                var range = new Vector2(x, stack.Pop());
                                list.Add(range);
                            }
                        }

                        string singleLine = "";
                        int index = paragraph.Length;
                        var richTexts = new List<string>();
                        while (wordList.Count > 0)
                        {
                            string singleWord = wordList[wordList.Count - 1];
                            wordList.RemoveAt(wordList.Count - 1);
                            float compareWidth = 0f;
                            if (textComponent.rectTransform.rect.width > 0) //大于0不是根据文本内容自适应
                            {
                                compareWidth = textComponent.rectTransform.rect.width;
                            } else
                            {
                                compareWidth = textComponent.preferredWidth;
                            }

                            string centerWord = Regex.Replace(singleWord, ArabicDefine.RichTextPrefix, string.Empty);
                            string postWord = Regex.Replace(centerWord, ArabicDefine.RichTextFixLine, string.Empty);
                            int offset = singleWord.Length - centerWord.Length;
                            string richWord;
                            if (string.IsNullOrEmpty(postWord))
                            {
                                richWord = string.Empty;
                            } else
                            {
                                richTexts.Clear();
                                foreach (var kvp in ranges)
                                {
                                    foreach (var range in kvp.Value)
                                    {
                                        int startIndex = index - singleWord.Length;
                                        int endIndex = index - offset;
                                        if (Mathf.FloorToInt(range.y) > startIndex && Mathf.FloorToInt(range.x) < endIndex)
                                        {
                                            richTexts.Add(kvp.Key);
                                        }
                                    }
                                }
                                if (richTexts.Count > 0)
                                {
                                    stringBuilder.Clear();
                                    foreach (string richText in richTexts)
                                    {
                                        stringBuilder.Append(richText);
                                    }
                                    stringBuilder.Append(postWord);
                                    for (int j = richTexts.Count - 1; j >= 0; j--)
                                    {
                                        string richText = richTexts[j];
                                        stringBuilder.Append("</");
                                        int i = richText.IndexOf('=');
                                        if (i >= 0)
                                        {
                                            stringBuilder.Append(richText.Substring(1, i - 1));
                                        }
                                        stringBuilder.Append('>');
                                    }
                                    richWord = stringBuilder.ToString();
                                } else
                                {
                                    postWord = singleWord;
                                    richWord = singleWord;
                                }
                            }
                            index -= singleWord.Length + 1;

                            if (textComponent.cachedTextGenerator.GetPreferredWidth(postWord + ' ' + singleLine, tgs) / textComponent.pixelsPerUnit > compareWidth)
                            {
                                lineList.Add(singleLine);
                                singleLine = richWord;
                            } else
                            {
                                stringBuilder.Clear();
                                stringBuilder.Append(richWord);
                                stringBuilder.Append(' ');
                                stringBuilder.Append(singleLine);
                                singleLine = stringBuilder.ToString();
                            }
                        }

                        if (singleLine.Length > 0)
                        {
                            lineList.Add(singleLine);
                        }

                        resultText.Add(String.Join(Environment.NewLine, lineList.ToArray()));
                    }
                }

                textComponent.text = String.Join(Environment.NewLine, resultText.ToArray());
                Recycle(stringBuilder);
            }
            if (anchorRight)
            {
                if (textComponent.alignment == TextAnchor.LowerLeft || textComponent.alignment == TextAnchor.MiddleLeft ||
                    textComponent.alignment == TextAnchor.UpperLeft)
                {
                    textComponent.alignment = TextAnchor.MiddleRight;
                }
            }

        }
    }
}

/// <summary>
/// Arabic Contextual forms General - Unicode
/// </summary>
internal enum IsolatedArabicLetters
{
    Hamza = 0xFE80,
    Alef = 0xFE8D,
    AlefHamza = 0xFE83,
    WawHamza = 0xFE85,
    AlefMaksoor = 0xFE87,
    AlefMaksora = 0xFBFC,
    HamzaNabera = 0xFE89,
    Ba = 0xFE8F,
    Ta = 0xFE95,
    Tha2 = 0xFE99,
    Jeem = 0xFE9D,
    H7aa = 0xFEA1,
    Khaa2 = 0xFEA5,
    Dal = 0xFEA9,
    Thal = 0xFEAB,
    Ra2 = 0xFEAD,
    Zeen = 0xFEAF,
    Seen = 0xFEB1,
    Sheen = 0xFEB5,
    S9a = 0xFEB9,
    Dha = 0xFEBD,
    T6a = 0xFEC1,
    T6ha = 0xFEC5,
    Ain = 0xFEC9,
    Gain = 0xFECD,
    Fa = 0xFED1,
    Gaf = 0xFED5,
    Kaf = 0xFED9,
    Lam = 0xFEDD,
    Meem = 0xFEE1,
    Noon = 0xFEE5,
    Ha = 0xFEE9,
    Waw = 0xFEED,
    Ya = 0xFEF1,
    AlefMad = 0xFE81,
    TaMarboota = 0xFE93,
    PersianPe = 0xFB56, // Persian Letters;
    PersianChe = 0xFB7A,
    PersianZe = 0xFB8A,
    PersianGaf = 0xFB92,
    PersianGaf2 = 0xFB8E
}

/// <summary>
/// Arabic Contextual forms - Isolated
/// </summary>
internal enum GeneralArabicLetters
{
    Hamza = 0x0621,
    Alef = 0x0627,
    AlefHamza = 0x0623,
    WawHamza = 0x0624,
    AlefMaksoor = 0x0625,
    AlefMagsora = 0x0649,
    HamzaNabera = 0x0626,
    Ba = 0x0628,
    Ta = 0x062A,
    Tha2 = 0x062B,
    Jeem = 0x062C,
    H7aa = 0x062D,
    Khaa2 = 0x062E,
    Dal = 0x062F,
    Thal = 0x0630,
    Ra2 = 0x0631,
    Zeen = 0x0632,
    Seen = 0x0633,
    Sheen = 0x0634,
    S9a = 0x0635,
    Dha = 0x0636,
    T6a = 0x0637,
    T6ha = 0x0638,
    Ain = 0x0639,
    Gain = 0x063A,
    Fa = 0x0641,
    Gaf = 0x0642,
    Kaf = 0x0643,
    Lam = 0x0644,
    Meem = 0x0645,
    Noon = 0x0646,
    Ha = 0x0647,
    Waw = 0x0648,
    Ya = 0x064A,
    AlefMad = 0x0622,
    TaMarboota = 0x0629,
    PersianPe = 0x067E, // Persian Letters;
    PersianChe = 0x0686,
    PersianZe = 0x0698,
    PersianGaf = 0x06AF,
    PersianGaf2 = 0x06A9
}

/// <summary>
/// Data Structure for conversion
/// </summary>
internal class ArabicMapping
{
    public int from;
    public int to;
    public ArabicMapping(int from, int to)
    {
        this.from = from;
        this.to = to;
    }
}

/// <summary>
/// Sets up and creates the conversion table
/// </summary>
internal class ArabicTable
{
    private static List<ArabicMapping> mapList;
    private static ArabicTable arabicMapper;

    /// <summary>
    /// Setting up the conversion table
    /// </summary>
    private ArabicTable()
    {
        mapList = new List<ArabicMapping>();

        mapList.Add(new ArabicMapping((int)GeneralArabicLetters.Hamza, (int)IsolatedArabicLetters.Hamza));
        mapList.Add(new ArabicMapping((int)GeneralArabicLetters.Alef, (int)IsolatedArabicLetters.Alef));
        mapList.Add(new ArabicMapping((int)GeneralArabicLetters.AlefHamza, (int)IsolatedArabicLetters.AlefHamza));
        mapList.Add(new ArabicMapping((int)GeneralArabicLetters.WawHamza, (int)IsolatedArabicLetters.WawHamza));
        mapList.Add(new ArabicMapping((int)GeneralArabicLetters.AlefMaksoor, (int)IsolatedArabicLetters.AlefMaksoor));
        mapList.Add(new ArabicMapping((int)GeneralArabicLetters.AlefMagsora, (int)IsolatedArabicLetters.AlefMaksora));
        mapList.Add(new ArabicMapping((int)GeneralArabicLetters.HamzaNabera, (int)IsolatedArabicLetters.HamzaNabera));
        mapList.Add(new ArabicMapping((int)GeneralArabicLetters.Ba, (int)IsolatedArabicLetters.Ba));
        mapList.Add(new ArabicMapping((int)GeneralArabicLetters.Ta, (int)IsolatedArabicLetters.Ta));
        mapList.Add(new ArabicMapping((int)GeneralArabicLetters.Tha2, (int)IsolatedArabicLetters.Tha2));
        mapList.Add(new ArabicMapping((int)GeneralArabicLetters.Jeem, (int)IsolatedArabicLetters.Jeem));
        mapList.Add(new ArabicMapping((int)GeneralArabicLetters.H7aa, (int)IsolatedArabicLetters.H7aa));
        mapList.Add(new ArabicMapping((int)GeneralArabicLetters.Khaa2, (int)IsolatedArabicLetters.Khaa2));
        mapList.Add(new ArabicMapping((int)GeneralArabicLetters.Dal, (int)IsolatedArabicLetters.Dal));
        mapList.Add(new ArabicMapping((int)GeneralArabicLetters.Thal, (int)IsolatedArabicLetters.Thal));
        mapList.Add(new ArabicMapping((int)GeneralArabicLetters.Ra2, (int)IsolatedArabicLetters.Ra2));
        mapList.Add(new ArabicMapping((int)GeneralArabicLetters.Zeen, (int)IsolatedArabicLetters.Zeen));
        mapList.Add(new ArabicMapping((int)GeneralArabicLetters.Seen, (int)IsolatedArabicLetters.Seen));
        mapList.Add(new ArabicMapping((int)GeneralArabicLetters.Sheen, (int)IsolatedArabicLetters.Sheen));
        mapList.Add(new ArabicMapping((int)GeneralArabicLetters.S9a, (int)IsolatedArabicLetters.S9a));
        mapList.Add(new ArabicMapping((int)GeneralArabicLetters.Dha, (int)IsolatedArabicLetters.Dha));
        mapList.Add(new ArabicMapping((int)GeneralArabicLetters.T6a, (int)IsolatedArabicLetters.T6a));
        mapList.Add(new ArabicMapping((int)GeneralArabicLetters.T6ha, (int)IsolatedArabicLetters.T6ha));
        mapList.Add(new ArabicMapping((int)GeneralArabicLetters.Ain, (int)IsolatedArabicLetters.Ain));
        mapList.Add(new ArabicMapping((int)GeneralArabicLetters.Gain, (int)IsolatedArabicLetters.Gain));
        mapList.Add(new ArabicMapping((int)GeneralArabicLetters.Fa, (int)IsolatedArabicLetters.Fa));
        mapList.Add(new ArabicMapping((int)GeneralArabicLetters.Gaf, (int)IsolatedArabicLetters.Gaf));
        mapList.Add(new ArabicMapping((int)GeneralArabicLetters.Kaf, (int)IsolatedArabicLetters.Kaf));
        mapList.Add(new ArabicMapping((int)GeneralArabicLetters.Lam, (int)IsolatedArabicLetters.Lam));
        mapList.Add(new ArabicMapping((int)GeneralArabicLetters.Meem, (int)IsolatedArabicLetters.Meem));
        mapList.Add(new ArabicMapping((int)GeneralArabicLetters.Noon, (int)IsolatedArabicLetters.Noon));
        mapList.Add(new ArabicMapping((int)GeneralArabicLetters.Ha, (int)IsolatedArabicLetters.Ha));
        mapList.Add(new ArabicMapping((int)GeneralArabicLetters.Waw, (int)IsolatedArabicLetters.Waw));
        mapList.Add(new ArabicMapping((int)GeneralArabicLetters.Ya, (int)IsolatedArabicLetters.Ya));
        mapList.Add(new ArabicMapping((int)GeneralArabicLetters.AlefMad, (int)IsolatedArabicLetters.AlefMad));
        mapList.Add(new ArabicMapping((int)GeneralArabicLetters.TaMarboota, (int)IsolatedArabicLetters.TaMarboota));
        mapList.Add(new ArabicMapping((int)GeneralArabicLetters.PersianPe, (int)IsolatedArabicLetters.PersianPe)); // Persian Letters;
        mapList.Add(new ArabicMapping((int)GeneralArabicLetters.PersianChe, (int)IsolatedArabicLetters.PersianChe));
        mapList.Add(new ArabicMapping((int)GeneralArabicLetters.PersianZe, (int)IsolatedArabicLetters.PersianZe));
        mapList.Add(new ArabicMapping((int)GeneralArabicLetters.PersianGaf, (int)IsolatedArabicLetters.PersianGaf));
        mapList.Add(new ArabicMapping((int)GeneralArabicLetters.PersianGaf2, (int)IsolatedArabicLetters.PersianGaf2));

        //for (int i = 0; i < generalArabic.Length; i++)
        //    mapList.Add(new ArabicMapping((int)generalArabic.GetValue(i), (int)isolatedArabic.GetValue(i)));    // I
    }

    /// <summary>
    /// Singleton design pattern, Get the mapper. If it was not created before, create it.
    /// </summary>
    internal static ArabicTable ArabicMapper
    {
        get
        {
            if (arabicMapper == null)
                arabicMapper = new ArabicTable();
            return arabicMapper;
        }
    }

    internal int Convert(int toBeConverted)
    {
        foreach (ArabicMapping arabicMap in mapList)
            if (arabicMap.from == toBeConverted)
            {
                return arabicMap.to;
            }
        return toBeConverted;
    }
}

internal class TashkeelLocation
{
    public char tashkeel;
    public int position;
    public TashkeelLocation(char tashkeel, int position)
    {
        this.tashkeel = tashkeel;
        this.position = position;
    }
}

public static class ArabicDefine
{
    public const char RichTextSymbol = '^';
    public const string RichTextPattern = "(^/{0,1}color(=#[0-9a-zA-Z]{6}){0,1}$|^/{0,1}Playername$|^/{0,1}Dog$|^/{0,1}Cat$|^/{0,1}i$|^/{0,1}b$|^/{0,1}size(=[0-9]+){0,1}$)";
    public const string RichTextFixLine = "(</{0,1}color(=#[0-9a-zA-Z]{6}){0,1}>|</{0,1}Playername>|</{0,1}Dog>|</{0,1}Cat>|</{0,1}i>|</{0,1}b>|</{0,1}size(=[0-9]+){0,1}>)";
    public const string RichTextPrefix = "(<color(=#[0-9a-zA-Z]{6})>|<Playername>|<Dog>|<Cat>|<i>|<b>|<size(=[0-9]+)>)";
    public const string RichTextSuffix = "(</color>|</Playername>|</Dog>|</Cat>|</i>|</b>|</size>)";
}

internal class ArabicFixerTool
{
    internal static bool showTashkeel = true;
    internal static bool combineTashkeel = true;
    internal static bool useHinduNumbers = false;

    internal static string RemoveTashkeel(string str, out List<TashkeelLocation> tashkeelLocation)
    {
        tashkeelLocation = new List<TashkeelLocation>();
        char[] letters = str.ToCharArray();

        int index = 0;
        for (int i = 0; i < letters.Length; i++)
        {
            if (letters[i] == (char)0x064B)
            {
                // Tanween Fatha
                tashkeelLocation.Add(new TashkeelLocation((char)0x064B, i));
                index++;
            } else if (letters[i] == (char)0x064C)
            {
                // Tanween Damma
                tashkeelLocation.Add(new TashkeelLocation((char)0x064C, i));
                index++;
            } else if (letters[i] == (char)0x064D)
            {
                // Tanween Kasra
                tashkeelLocation.Add(new TashkeelLocation((char)0x064D, i));
                index++;
            } else if (letters[i] == (char)0x064E)
            {
                // Fatha
                if (index > 0 && combineTashkeel)
                {
                    if (tashkeelLocation[index - 1].tashkeel == (char)0x0651) // Shadda
                    {
                        tashkeelLocation[index - 1].tashkeel = (char)0xFC60; // Shadda With Fatha
                        continue;
                    }
                }

                tashkeelLocation.Add(new TashkeelLocation((char)0x064E, i));
                index++;
            } else if (letters[i] == (char)0x064F)
            {
                // DAMMA
                if (index > 0 && combineTashkeel)
                {
                    if (tashkeelLocation[index - 1].tashkeel == (char)0x0651)
                    {
                        // SHADDA
                        tashkeelLocation[index - 1].tashkeel = (char)0xFC61; // Shadda With DAMMA
                        continue;
                    }
                }
                tashkeelLocation.Add(new TashkeelLocation((char)0x064F, i));
                index++;
            } else if (letters[i] == (char)0x0650)
            {
                // KASRA
                if (index > 0 && combineTashkeel)
                {
                    if (tashkeelLocation[index - 1].tashkeel == (char)0x0651)
                    {
                        // SHADDA
                        tashkeelLocation[index - 1].tashkeel = (char)0xFC62; // Shadda With KASRA
                        continue;
                    }
                }
                tashkeelLocation.Add(new TashkeelLocation((char)0x0650, i));
                index++;
            } else if (letters[i] == (char)0x0651)
            {
                // SHADDA
                if (index > 0 && combineTashkeel)
                {
                    if (tashkeelLocation[index - 1].tashkeel == (char)0x064E) // FATHA
                    {
                        tashkeelLocation[index - 1].tashkeel = (char)0xFC60; // Shadda With Fatha
                        continue;
                    }

                    if (tashkeelLocation[index - 1].tashkeel == (char)0x064F) // DAMMA
                    {
                        tashkeelLocation[index - 1].tashkeel = (char)0xFC61; // Shadda With DAMMA
                        continue;
                    }

                    if (tashkeelLocation[index - 1].tashkeel == (char)0x0650) // KASRA
                    {
                        tashkeelLocation[index - 1].tashkeel = (char)0xFC62; // Shadda With KASRA
                        continue;
                    }
                }

                tashkeelLocation.Add(new TashkeelLocation((char)0x0651, i));
                index++;
            } else if (letters[i] == (char)0x0652)
            {
                // SUKUN
                tashkeelLocation.Add(new TashkeelLocation((char)0x0652, i));
                index++;
            } else if (letters[i] == (char)0x0653)
            {
                // MADDAH ABOVE
                tashkeelLocation.Add(new TashkeelLocation((char)0x0653, i));
                index++;
            }
        }

        string[] split = str.Split(new char[]
        {
            (char)0x064B, (char)0x064C, (char)0x064D,
            (char)0x064E, (char)0x064F, (char)0x0650,

            (char)0x0651, (char)0x0652, (char)0x0653, (char)0xFC60, (char)0xFC61, (char)0xFC62
        });
        str = "";

        foreach (string s in split)
        {
            str += s;
        }

        return str;
    }

    internal static char[] ReturnTashkeel(char[] letters, List<TashkeelLocation> tashkeelLocation)
    {
        char[] lettersWithTashkeel = new char[letters.Length + tashkeelLocation.Count];

        int letterWithTashkeelTracker = 0;
        for (int i = 0; i < letters.Length; i++)
        {
            lettersWithTashkeel[letterWithTashkeelTracker] = letters[i];
            letterWithTashkeelTracker++;
            foreach (TashkeelLocation hLocation in tashkeelLocation)
            {
                if (hLocation.position == letterWithTashkeelTracker)
                {
                    lettersWithTashkeel[letterWithTashkeelTracker] = hLocation.tashkeel;
                    letterWithTashkeelTracker++;
                }
            }
        }

        return lettersWithTashkeel;
    }

    internal static string RichTextToSymbol(string text, out List<string> richTexts, string pattern = ArabicDefine.RichTextPattern)
    {
        string[] array = text.Split('<', '>');
        richTexts = new List<string>();
        string tempString = "";
        //名字部分都用*代替（切分符号）
        for (int i = 0; i < array.Length; i++)
        {
            string str = array[i];
            if (Regex.IsMatch(str, pattern))
            {
                tempString += ArabicDefine.RichTextSymbol;
                richTexts.Add(str);
            } else
            {
                tempString += str;
            }
        }
        return tempString;
    }

    internal static string SymbolToRichText(string text, List<string> richTexts)
    {
        //通过*切分（切分符号）
        string[] array = text.Split(ArabicDefine.RichTextSymbol);
        //拼接
        var stringBuilder = new StringBuilder();
        int i = 0;
        var sourceStack = new Stack<int>();
        var targetStack = new Stack<int>();
        for (; i < richTexts.Count; i++)
        {
            int j = richTexts.Count - i - 1;
            string richText;
            string richText1 = richTexts[j];
            if (j > 0 && richText1.StartsWith("/"))
            {
                richText = richText1;
                int k = j - 1;
                int len = richText1.Length - 1;
                string compareText = richText1.Substring(1);
                while (k >= 0)
                {
                    string richText2 = richTexts[k];
                    if (compareText == richText2.Substring(0, len))
                    {
                        richText = richText2;
                        sourceStack.Push(j);
                        targetStack.Push(k);
                        break;
                    }
                    k--;
                }
            } else if (targetStack.Count > 0 && targetStack.Peek() == j)
            {
                richText = richTexts[sourceStack.Pop()];
                targetStack.Pop();
            } else
            {
                richText = richTexts[j];
            }
            stringBuilder.Append(array[i]);
            stringBuilder.Append('<');
            stringBuilder.Append(richText);
            stringBuilder.Append('>');
        }
        for (; i < array.Length; i++)
        {
            stringBuilder.Append(array[i]);
        }
        return stringBuilder.ToString();
    }

    /// <summary>
    /// Converts a string to a form in which the sting will be displayed correctly for arabic text.
    /// </summary>
    /// <param name="str">String to be converted. Example: "Aaa"</param>
    /// <returns>Converted string. Example: "aa aaa A" without the spaces.</returns>
    internal static string FixLine(string str)
    {
        string test = "";

        List<TashkeelLocation> tashkeelLocation;

        string originString = RemoveTashkeel(str, out tashkeelLocation);

        char[] lettersOrigin = originString.ToCharArray();
        char[] lettersFinal = originString.ToCharArray();

        for (int i = 0; i < lettersOrigin.Length; i++)
        {
            lettersOrigin[i] = (char)ArabicTable.ArabicMapper.Convert(lettersOrigin[i]);
        }

        for (int i = 0; i < lettersOrigin.Length; i++)
        {
            bool skip = false;

            //lettersOrigin[i] = (char)ArabicTable.ArabicMapper.Convert(lettersOrigin[i]);

            // For special Lam Letter connections.
            if (lettersOrigin[i] == (char)IsolatedArabicLetters.Lam)
            {
                if (i < lettersOrigin.Length - 1)
                {
                    //lettersOrigin[i + 1] = (char)ArabicTable.ArabicMapper.Convert(lettersOrigin[i + 1]);
                    if ((lettersOrigin[i + 1] == (char)IsolatedArabicLetters.AlefMaksoor))
                    {
                        lettersOrigin[i] = (char)0xFEF7;
                        lettersFinal[i + 1] = (char)0xFFFF;
                        skip = true;
                    } else if ((lettersOrigin[i + 1] == (char)IsolatedArabicLetters.Alef))
                    {
                        lettersOrigin[i] = (char)0xFEF9;
                        lettersFinal[i + 1] = (char)0xFFFF;
                        skip = true;
                    } else if ((lettersOrigin[i + 1] == (char)IsolatedArabicLetters.AlefHamza))
                    {
                        lettersOrigin[i] = (char)0xFEF5;
                        lettersFinal[i + 1] = (char)0xFFFF;
                        skip = true;
                    } else if ((lettersOrigin[i + 1] == (char)IsolatedArabicLetters.AlefMad))
                    {
                        lettersOrigin[i] = (char)0xFEF3;
                        lettersFinal[i + 1] = (char)0xFFFF;
                        skip = true;
                    }
                }
            }

            if (!IsIgnoredCharacter(lettersOrigin[i]))
            {
                if (IsMiddleLetter(lettersOrigin, i))
                    lettersFinal[i] = (char)(lettersOrigin[i] + 3);
                else if (IsFinishingLetter(lettersOrigin, i))
                    lettersFinal[i] = (char)(lettersOrigin[i] + 1);
                else if (IsLeadingLetter(lettersOrigin, i))
                    lettersFinal[i] = (char)(lettersOrigin[i] + 2);
            }

            //string strOut = String.Format(@"\x{0:x4}", (ushort)lettersOrigin[i]);
            //UnityEngine.Debug.Log(strOut);

            //strOut = String.Format(@"\x{0:x4}", (ushort)lettersFinal[i]);
            //UnityEngine.Debug.Log(strOut);

            test += Convert.ToString((int)lettersOrigin[i], 16) + " ";
            if (skip)
                i++;

            //chaning numbers to hindu
            if (useHinduNumbers)
            {
                if (lettersOrigin[i] == (char)0x0030)
                    lettersFinal[i] = (char)0x0660;
                else if (lettersOrigin[i] == (char)0x0031)
                    lettersFinal[i] = (char)0x0661;
                else if (lettersOrigin[i] == (char)0x0032)
                    lettersFinal[i] = (char)0x0662;
                else if (lettersOrigin[i] == (char)0x0033)
                    lettersFinal[i] = (char)0x0663;
                else if (lettersOrigin[i] == (char)0x0034)
                    lettersFinal[i] = (char)0x0664;
                else if (lettersOrigin[i] == (char)0x0035)
                    lettersFinal[i] = (char)0x0665;
                else if (lettersOrigin[i] == (char)0x0036)
                    lettersFinal[i] = (char)0x0666;
                else if (lettersOrigin[i] == (char)0x0037)
                    lettersFinal[i] = (char)0x0667;
                else if (lettersOrigin[i] == (char)0x0038)
                    lettersFinal[i] = (char)0x0668;
                else if (lettersOrigin[i] == (char)0x0039)
                    lettersFinal[i] = (char)0x0669;
            }
        }

        //Return the Tashkeel to their places.
        if (showTashkeel)
            lettersFinal = ReturnTashkeel(lettersFinal, tashkeelLocation);

        List<char> list = new List<char>();

        List<char> numberList = new List<char>();

        for (int i = lettersFinal.Length - 1; i >= 0; i--)
        {
            //				if (lettersFinal[i] == '(')
            //						numberList.Add(')');
            //				else if (lettersFinal[i] == ')')
            //					numberList.Add('(');
            //				else if (lettersFinal[i] == '<')
            //					numberList.Add('>');
            //				else if (lettersFinal[i] == '>')
            //					numberList.Add('<');
            //				else
            if (char.IsPunctuation(lettersFinal[i]) && i > 0 && i < lettersFinal.Length - 1 &&
                (char.IsPunctuation(lettersFinal[i - 1]) || char.IsPunctuation(lettersFinal[i + 1])))
            {
                if (lettersFinal[i] == '(')
                    list.Add(')');
                else if (lettersFinal[i] == ')')
                    list.Add('(');
                else if (lettersFinal[i] == '<')
                    list.Add('>');
                else if (lettersFinal[i] == '>')
                    list.Add('<');
                else if (lettersFinal[i] == '[')
                    list.Add(']');
                else if (lettersFinal[i] == ']')
                    list.Add('[');
                else if (lettersFinal[i] == '»')
                    list.Add('«');
                else if (lettersFinal[i] == '«')
                    list.Add('»');
                else if (lettersFinal[i] != 0xFFFF)
                    list.Add(lettersFinal[i]);
            }
            // For cases where english words and arabic are mixed. This allows for using arabic, english and numbers in one sentence.
            else if (lettersFinal[i] == ' ' && i > 0 && i < lettersFinal.Length - 1 &&
                     (char.IsLower(lettersFinal[i - 1]) || char.IsUpper(lettersFinal[i - 1]) || char.IsNumber(lettersFinal[i - 1])) &&
                     (char.IsLower(lettersFinal[i + 1]) || char.IsUpper(lettersFinal[i + 1]) || char.IsNumber(lettersFinal[i + 1])))

            {
                numberList.Add(lettersFinal[i]);
            } else if (char.IsNumber(lettersFinal[i]) || char.IsLower(lettersFinal[i]) ||
                       char.IsUpper(lettersFinal[i]) || char.IsSymbol(lettersFinal[i]) ||
                       char.IsPunctuation(lettersFinal[i])) // || lettersFinal[i] == '^') //)
            {
                if (lettersFinal[i] == '(')
                {
                    int index = 0;
                    while (index < numberList.Count && char.IsPunctuation(numberList[index]))
                    {
                        index++;
                    }
                    numberList.Insert(index, ')');
                } else if (lettersFinal[i] == ')')
                    numberList.Add('(');
                else if (lettersFinal[i] == '<')
                {
                    int index = 0;
                    while (index < numberList.Count && char.IsPunctuation(numberList[index]))
                    {
                        index++;
                    }
                    numberList.Insert(index, '>');
                } else if (lettersFinal[i] == '>')
                    numberList.Add('<');
                else if (lettersFinal[i] == '[')
                    list.Add(']');
                else if (lettersFinal[i] == ']')
                    list.Add('[');
                else if (lettersFinal[i] == '»')
                    list.Add('«');
                else if (lettersFinal[i] == '«')
                    list.Add('»');
                else
                    numberList.Add(lettersFinal[i]);
            } else if ((lettersFinal[i] >= (char)0xD800 && lettersFinal[i] <= (char)0xDBFF) ||
                       (lettersFinal[i] >= (char)0xDC00 && lettersFinal[i] <= (char)0xDFFF))
            {
                numberList.Add(lettersFinal[i]);
            } else
            {
                if (numberList.Count > 0)
                {
                    for (int j = 0; j < numberList.Count; j++)
                        list.Add(numberList[numberList.Count - 1 - j]);
                    numberList.Clear();
                }
                if (lettersFinal[i] != 0xFFFF)
                    list.Add(lettersFinal[i]);
            }
        }
        if (numberList.Count > 0)
        {
            for (int j = 0; j < numberList.Count; j++)
                list.Add(numberList[numberList.Count - 1 - j]);
            numberList.Clear();
        }

        // Moving letters from a list to an array.
        lettersFinal = new char[list.Count];
        for (int i = 0; i < lettersFinal.Length; i++)
            lettersFinal[i] = list[i];

        str = new string(lettersFinal);
        return str;
    }

    internal static readonly string[] RichTextList =
    {
        "Playername",
        "b", "i", "size", "material", "quad"
    };

    internal static string FlitRichTextInSingleWord(string singleWord)
    {
        if (singleWord.StartsWith("<color/>"))
        {
            if (singleWord.EndsWith("<color=#"))
            {
                return singleWord.Substring(8, singleWord.Length - 23);
            }
            return singleWord.Substring(8);
        }
        foreach (string richText in RichTextList)
        {
            int length = richText.Length;
            if (singleWord.StartsWith($"<{richText}/>"))
            {
                if (singleWord.EndsWith($"<{richText}>"))
                {
                    return singleWord.Substring(length + 3, singleWord.Length - 2 * length - 5);
                }
                return singleWord.Substring(length + 3, singleWord.Length - length - 3);
            }
        }
        return singleWord;
    }

    /// <summary>
    /// English letters, numbers and punctuation characters are ignored. This checks if the ch is an ignored character.
    /// </summary>
    /// <param name="ch">The character to be checked for skipping</param>
    /// <returns>True if the character should be ignored, false if it should not be ignored.</returns>
    internal static bool IsIgnoredCharacter(char ch)
    {
        bool isPunctuation = char.IsPunctuation(ch);
        bool isNumber = char.IsNumber(ch);
        bool isLower = char.IsLower(ch);
        bool isUpper = char.IsUpper(ch);
        bool isSymbol = char.IsSymbol(ch);
        bool isPersianCharacter = ch == (char)0xFB56 || ch == (char)0xFB7A || ch == (char)0xFB8A || ch == (char)0xFB92 || ch == (char)0xFB8E;
        bool isPresentationFormB = (ch <= (char)0xFEFF && ch >= (char)0xFE70);
        bool isAcceptableCharacter = isPresentationFormB || isPersianCharacter || ch == (char)0xFBFC;

        return isPunctuation ||
               isNumber ||
               isLower ||
               isUpper ||
               isSymbol ||
               !isAcceptableCharacter ||
               ch == 'a' || ch == '>' || ch == '<' || ch == (char)0x061B;

        //            return char.IsPunctuation(ch) || char.IsNumber(ch) || ch == 'a' || ch == '>' || ch == '<' ||
        //                    char.IsLower(ch) || char.IsUpper(ch) || ch == (char)0x061B || char.IsSymbol(ch)
        //					|| !(ch <= (char)0xFEFF && ch >= (char)0xFE70) // Presentation Form B
        //					|| ch == (char)0xFB56 || ch == (char)0xFB7A || ch == (char)0xFB8A || ch == (char)0xFB92; // Persian Characters

        //					PersianPe = 0xFB56,
        //		PersianChe = 0xFB7A,
        //		PersianZe = 0xFB8A,
        //		PersianGaf = 0xFB92
        //lettersOrigin[i] <= (char)0xFEFF && lettersOrigin[i] >= (char)0xFE70
    }

    /// <summary>
    /// Checks if the letter at index value is a leading character in Arabic or not.
    /// </summary>
    /// <param name="letters">The whole word that contains the character to be checked</param>
    /// <param name="index">The index of the character to be checked</param>
    /// <returns>True if the character at index is a leading character, else, returns false</returns>
    internal static bool IsLeadingLetter(char[] letters, int index)
    {
        bool lettersThatCannotBeBeforeALeadingLetter = index == 0
                                                       || letters[index - 1] == ' '
                                                       || letters[index - 1] == '*' // ??? Remove?
                                                       || letters[index - 1] == 'A' // ??? Remove?
                                                       || char.IsPunctuation(letters[index - 1])
                                                       || letters[index - 1] == '>'
                                                       || letters[index - 1] == '<'
                                                       || letters[index - 1] == (int)IsolatedArabicLetters.Alef
                                                       || letters[index - 1] == (int)IsolatedArabicLetters.Dal
                                                       || letters[index - 1] == (int)IsolatedArabicLetters.Thal
                                                       || letters[index - 1] == (int)IsolatedArabicLetters.Ra2
                                                       || letters[index - 1] == (int)IsolatedArabicLetters.Zeen
                                                       || letters[index - 1] == (int)IsolatedArabicLetters.PersianZe
                                                       //|| letters[index - 1] == (int)IsolatedArabicLetters.AlefMaksora
                                                       || letters[index - 1] == (int)IsolatedArabicLetters.Waw
                                                       || letters[index - 1] == (int)IsolatedArabicLetters.AlefMad
                                                       || letters[index - 1] == (int)IsolatedArabicLetters.AlefHamza
                                                       || letters[index - 1] == (int)IsolatedArabicLetters.Hamza
                                                       || letters[index - 1] == (int)IsolatedArabicLetters.AlefMaksoor
                                                       || letters[index - 1] == (int)IsolatedArabicLetters.WawHamza;

        bool lettersThatCannotBeALeadingLetter = letters[index] != ' '
                                                 && letters[index] != (int)IsolatedArabicLetters.Dal
                                                 && letters[index] != (int)IsolatedArabicLetters.Thal
                                                 && letters[index] != (int)IsolatedArabicLetters.Ra2
                                                 && letters[index] != (int)IsolatedArabicLetters.Zeen
                                                 && letters[index] != (int)IsolatedArabicLetters.PersianZe
                                                 && letters[index] != (int)IsolatedArabicLetters.Alef
                                                 && letters[index] != (int)IsolatedArabicLetters.AlefHamza
                                                 && letters[index] != (int)IsolatedArabicLetters.AlefMaksoor
                                                 && letters[index] != (int)IsolatedArabicLetters.AlefMad
                                                 && letters[index] != (int)IsolatedArabicLetters.WawHamza
                                                 && letters[index] != (int)IsolatedArabicLetters.Waw
                                                 && letters[index] != (int)IsolatedArabicLetters.Hamza;

        bool lettersThatCannotBeAfterLeadingLetter = index < letters.Length - 1
                                                     && letters[index + 1] != ' '
                                                     && !char.IsPunctuation(letters[index + 1])
                                                     && !char.IsNumber(letters[index + 1])
                                                     && !char.IsSymbol(letters[index + 1])
                                                     && !char.IsLower(letters[index + 1])
                                                     && !char.IsUpper(letters[index + 1])
                                                     && letters[index + 1] != (int)IsolatedArabicLetters.Hamza;

        if (lettersThatCannotBeBeforeALeadingLetter && lettersThatCannotBeALeadingLetter && lettersThatCannotBeAfterLeadingLetter)

        //		if ((index == 0 || letters[index - 1] == ' ' || letters[index - 1] == '*' || letters[index - 1] == 'A' || char.IsPunctuation(letters[index - 1])
        //		     || letters[index - 1] == '>' || letters[index - 1] == '<'
        //		     || letters[index - 1] == (int)IsolatedArabicLetters.Alef
        //		     || letters[index - 1] == (int)IsolatedArabicLetters.Dal || letters[index - 1] == (int)IsolatedArabicLetters.Thal
        //		     || letters[index - 1] == (int)IsolatedArabicLetters.Ra2
        //		     || letters[index - 1] == (int)IsolatedArabicLetters.Zeen || letters[index - 1] == (int)IsolatedArabicLetters.PersianZe
        //		     || letters[index - 1] == (int)IsolatedArabicLetters.AlefMaksora || letters[index - 1] == (int)IsolatedArabicLetters.Waw
        //		     || letters[index - 1] == (int)IsolatedArabicLetters.AlefMad || letters[index - 1] == (int)IsolatedArabicLetters.AlefHamza
        //		     || letters[index - 1] == (int)IsolatedArabicLetters.AlefMaksoor || letters[index - 1] == (int)IsolatedArabicLetters.WawHamza)
        //		    && letters[index] != ' ' && letters[index] != (int)IsolatedArabicLetters.Dal
        //		    && letters[index] != (int)IsolatedArabicLetters.Thal
        //		    && letters[index] != (int)IsolatedArabicLetters.Ra2
        //		    && letters[index] != (int)IsolatedArabicLetters.Zeen && letters[index] != (int)IsolatedArabicLetters.PersianZe
        //		    && letters[index] != (int)IsolatedArabicLetters.Alef && letters[index] != (int)IsolatedArabicLetters.AlefHamza
        //		    && letters[index] != (int)IsolatedArabicLetters.AlefMaksoor
        //		    && letters[index] != (int)IsolatedArabicLetters.AlefMad
        //		    && letters[index] != (int)IsolatedArabicLetters.WawHamza
        //		    && letters[index] != (int)IsolatedArabicLetters.Waw
        //		    && letters[index] != (int)IsolatedArabicLetters.Hamza
        //		    && index < letters.Length - 1 && letters[index + 1] != ' ' && !char.IsPunctuation(letters[index + 1] ) && !char.IsNumber(letters[index + 1])
        //		    && letters[index + 1] != (int)IsolatedArabicLetters.Hamza )
        {
            return true;
        } else
            return false;
    }

    /// <summary>
    /// Checks if the letter at index value is a finishing character in Arabic or not.
    /// </summary>
    /// <param name="letters">The whole word that contains the character to be checked</param>
    /// <param name="index">The index of the character to be checked</param>
    /// <returns>True if the character at index is a finishing character, else, returns false</returns>
    internal static bool IsFinishingLetter(char[] letters, int index)
    {
        bool indexZero = index != 0;
        bool lettersThatCannotBeBeforeAFinishingLetter = (index == 0)
            ? false
            : letters[index - 1] != ' '
              //				&& char.IsDigit(letters[index-1])
              //				&& char.IsLower(letters[index-1])
              //				&& char.IsUpper(letters[index-1])
              //				&& char.IsNumber(letters[index-1])
              //				&& char.IsWhiteSpace(letters[index-1])
              //				&& char.IsPunctuation(letters[index-1])
              //				&& char.IsSymbol(letters[index-1])
              && letters[index - 1] != (int)IsolatedArabicLetters.Dal
              && letters[index - 1] != (int)IsolatedArabicLetters.Thal
              && letters[index - 1] != (int)IsolatedArabicLetters.Ra2
              && letters[index - 1] != (int)IsolatedArabicLetters.Zeen
              && letters[index - 1] != (int)IsolatedArabicLetters.PersianZe
              //&& letters[index - 1] != (int)IsolatedArabicLetters.AlefMaksora
              && letters[index - 1] != (int)IsolatedArabicLetters.Waw
              && letters[index - 1] != (int)IsolatedArabicLetters.Alef
              && letters[index - 1] != (int)IsolatedArabicLetters.AlefMad
              && letters[index - 1] != (int)IsolatedArabicLetters.AlefHamza
              && letters[index - 1] != (int)IsolatedArabicLetters.AlefMaksoor
              && letters[index - 1] != (int)IsolatedArabicLetters.WawHamza
              && letters[index - 1] != (int)IsolatedArabicLetters.Hamza
              && !char.IsPunctuation(letters[index - 1])
              && !char.IsSymbol(letters[index - 1])
              && letters[index - 1] != '>'
              && letters[index - 1] != '<';

        bool lettersThatCannotBeFinishingLetters = letters[index] != ' ' && letters[index] != (int)IsolatedArabicLetters.Hamza;

        if (lettersThatCannotBeBeforeAFinishingLetter && lettersThatCannotBeFinishingLetters)

        //		if (index != 0 && letters[index - 1] != ' ' && letters[index - 1] != '*' && letters[index - 1] != 'A'
        //		    && letters[index - 1] != (int)IsolatedArabicLetters.Dal && letters[index - 1] != (int)IsolatedArabicLetters.Thal
        //		    && letters[index - 1] != (int)IsolatedArabicLetters.Ra2
        //		    && letters[index - 1] != (int)IsolatedArabicLetters.Zeen && letters[index - 1] != (int)IsolatedArabicLetters.PersianZe
        //		    && letters[index - 1] != (int)IsolatedArabicLetters.AlefMaksora && letters[index - 1] != (int)IsolatedArabicLetters.Waw
        //		    && letters[index - 1] != (int)IsolatedArabicLetters.Alef && letters[index - 1] != (int)IsolatedArabicLetters.AlefMad
        //		    && letters[index - 1] != (int)IsolatedArabicLetters.AlefHamza && letters[index - 1] != (int)IsolatedArabicLetters.AlefMaksoor
        //		    && letters[index - 1] != (int)IsolatedArabicLetters.WawHamza && letters[index - 1] != (int)IsolatedArabicLetters.Hamza
        //		    && !char.IsPunctuation(letters[index - 1]) && letters[index - 1] != '>' && letters[index - 1] != '<'
        //		    && letters[index] != ' ' && index < letters.Length
        //		    && letters[index] != (int)IsolatedArabicLetters.Hamza)
        {
            //try
            //{
            //    if (char.IsPunctuation(letters[index + 1]))
            //        return true;
            //    else
            //        return false;
            //}
            //catch (Exception e)
            //{
            //    return false;
            //}

            return true;
        }
        //return true;
        else
            return false;
    }

    /// <summary>
    /// Checks if the letter at index value is a middle character in Arabic or not.
    /// </summary>
    /// <param name="letters">The whole word that contains the character to be checked</param>
    /// <param name="index">The index of the character to be checked</param>
    /// <returns>True if the character at index is a middle character, else, returns false</returns>
    internal static bool IsMiddleLetter(char[] letters, int index)
    {
        bool lettersThatCannotBeMiddleLetters = (index == 0)
            ? false
            : letters[index] != (int)IsolatedArabicLetters.Alef
              && letters[index] != (int)IsolatedArabicLetters.Dal
              && letters[index] != (int)IsolatedArabicLetters.Thal
              && letters[index] != (int)IsolatedArabicLetters.Ra2
              && letters[index] != (int)IsolatedArabicLetters.Zeen
              && letters[index] != (int)IsolatedArabicLetters.PersianZe
              //&& letters[index] != (int)IsolatedArabicLetters.AlefMaksora
              && letters[index] != (int)IsolatedArabicLetters.Waw
              && letters[index] != (int)IsolatedArabicLetters.AlefMad
              && letters[index] != (int)IsolatedArabicLetters.AlefHamza
              && letters[index] != (int)IsolatedArabicLetters.AlefMaksoor
              && letters[index] != (int)IsolatedArabicLetters.WawHamza
              && letters[index] != (int)IsolatedArabicLetters.Hamza;

        bool lettersThatCannotBeBeforeMiddleCharacters = (index == 0)
            ? false
            : letters[index - 1] != (int)IsolatedArabicLetters.Alef
              && letters[index - 1] != (int)IsolatedArabicLetters.Dal
              && letters[index - 1] != (int)IsolatedArabicLetters.Thal
              && letters[index - 1] != (int)IsolatedArabicLetters.Ra2
              && letters[index - 1] != (int)IsolatedArabicLetters.Zeen
              && letters[index - 1] != (int)IsolatedArabicLetters.PersianZe
              //&& letters[index - 1] != (int)IsolatedArabicLetters.AlefMaksora
              && letters[index - 1] != (int)IsolatedArabicLetters.Waw
              && letters[index - 1] != (int)IsolatedArabicLetters.AlefMad
              && letters[index - 1] != (int)IsolatedArabicLetters.AlefHamza
              && letters[index - 1] != (int)IsolatedArabicLetters.AlefMaksoor
              && letters[index - 1] != (int)IsolatedArabicLetters.WawHamza
              && letters[index - 1] != (int)IsolatedArabicLetters.Hamza
              && !char.IsPunctuation(letters[index - 1])
              && letters[index - 1] != '>'
              && letters[index - 1] != '<'
              && letters[index - 1] != ' '
              && letters[index - 1] != '*';

        bool lettersThatCannotBeAfterMiddleCharacters = (index >= letters.Length - 1)
            ? false
            : letters[index + 1] != ' '
              && letters[index + 1] != '\r'
              && letters[index + 1] != (int)IsolatedArabicLetters.Hamza
              && !char.IsNumber(letters[index + 1])
              && !char.IsSymbol(letters[index + 1])
              && !char.IsPunctuation(letters[index + 1]);
        if (lettersThatCannotBeAfterMiddleCharacters && lettersThatCannotBeBeforeMiddleCharacters && lettersThatCannotBeMiddleLetters)

        //		if (index != 0 && letters[index] != ' '
        //		    && letters[index] != (int)IsolatedArabicLetters.Alef && letters[index] != (int)IsolatedArabicLetters.Dal
        //		    && letters[index] != (int)IsolatedArabicLetters.Thal && letters[index] != (int)IsolatedArabicLetters.Ra2
        //		    && letters[index] != (int)IsolatedArabicLetters.Zeen && letters[index] != (int)IsolatedArabicLetters.PersianZe
        //		    && letters[index] != (int)IsolatedArabicLetters.AlefMaksora
        //		    && letters[index] != (int)IsolatedArabicLetters.Waw && letters[index] != (int)IsolatedArabicLetters.AlefMad
        //		    && letters[index] != (int)IsolatedArabicLetters.AlefHamza && letters[index] != (int)IsolatedArabicLetters.AlefMaksoor
        //		    && letters[index] != (int)IsolatedArabicLetters.WawHamza && letters[index] != (int)IsolatedArabicLetters.Hamza
        //		    && letters[index - 1] != (int)IsolatedArabicLetters.Alef && letters[index - 1] != (int)IsolatedArabicLetters.Dal
        //		    && letters[index - 1] != (int)IsolatedArabicLetters.Thal && letters[index - 1] != (int)IsolatedArabicLetters.Ra2
        //		    && letters[index - 1] != (int)IsolatedArabicLetters.Zeen && letters[index - 1] != (int)IsolatedArabicLetters.PersianZe
        //		    && letters[index - 1] != (int)IsolatedArabicLetters.AlefMaksora
        //		    && letters[index - 1] != (int)IsolatedArabicLetters.Waw && letters[index - 1] != (int)IsolatedArabicLetters.AlefMad
        //		    && letters[index - 1] != (int)IsolatedArabicLetters.AlefHamza && letters[index - 1] != (int)IsolatedArabicLetters.AlefMaksoor
        //		    && letters[index - 1] != (int)IsolatedArabicLetters.WawHamza && letters[index - 1] != (int)IsolatedArabicLetters.Hamza
        //		    && letters[index - 1] != '>' && letters[index - 1] != '<'
        //		    && letters[index - 1] != ' ' && letters[index - 1] != '*' && !char.IsPunctuation(letters[index - 1])
        //		    && index < letters.Length - 1 && letters[index + 1] != ' ' && letters[index + 1] != '\r' && letters[index + 1] != 'A'
        //		    && letters[index + 1] != '>' && letters[index + 1] != '>' && letters[index + 1] != (int)IsolatedArabicLetters.Hamza
        //		    )
        {
            try
            {
                if (char.IsPunctuation(letters[index + 1]))
                    return false;
                else
                    return true;
            } catch
            {
                return false;
            }
            //return true;
        } else
            return false;
    }
}
