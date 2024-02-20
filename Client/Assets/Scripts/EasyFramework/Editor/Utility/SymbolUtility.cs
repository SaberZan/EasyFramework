using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Easy
{
    public class SymbolUtility
    {
        public static void AddSymbol(string symbol, BuildTargetGroup buildTargetGroup)
        {
            if(!SymbolUtility.IsDefinedSymbol(symbol, buildTargetGroup))
            {
                SymbolUtility.ToogleSymbol(symbol, buildTargetGroup);
            }
        }

        public static void RemoveSymbol(string symbol, BuildTargetGroup buildTargetGroup)
        {
            if(SymbolUtility.IsDefinedSymbol(symbol, buildTargetGroup))
            {
                SymbolUtility.ToogleSymbol(symbol, buildTargetGroup);
            }
        }

        public static bool ToogleSymbol(string symbol, BuildTargetGroup buildTargetGroup)
        {
            string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            List<string> symbolList = symbols.Split(';').ToList();
            bool isContain = symbolList.Contains(symbol);
            if(isContain)
            {
                symbolList.Remove(symbol);
            }
            else
            {
                symbolList.Add(symbol);
            }
            symbols = string.Join(";", symbolList); 
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, symbols);
            return !isContain;
        }

        public static bool IsDefinedSymbol(string symbol, BuildTargetGroup buildTargetGroup)
        {
            string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            List<string> symbolList = symbols.Split(';').ToList();
            bool isContain = symbolList.Contains(symbol);
            return isContain;
        }
    }    
}