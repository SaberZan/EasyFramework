export default {
    namespaceStart : "",

    namespaceEnd : "",

    configHead : "\t[Easy.Config(\"{0}\")]\r\n",

    classStart : "\t[System.Serializable]\r\n\tpublic class {0}\r\n \t{\r\n",

    classDictionaryStart : "\t[System.Serializable]\r\n\tpublic class {0} : System.Collections.Generic.Dictionary<{1}, {2}>\r\n \t{\r\n",

    classDictionaryStartWithConfig : "\t[Easy.Config(\"{0}\")]\r\n\t[System.Serializable]\r\n\tpublic class {0}Dictionary : System.Collections.Generic.Dictionary<{1}, {2}>\r\n \t{\r\n\t}\r\n\r\n",

    classEnd : "\t} \r\n",

    notes : "/**\r\n * {0}\r\n */\r\n",

    privateStr : "\t\t[Newtonsoft.Json.JsonProperty]\r\n \t\tprivate {0} {1};    //{2}\r\n\r\n",

    publicStr : "\t\t[Newtonsoft.Json.JsonIgnore]\r\n \t\tpublic {0} {1} => {2};\r\n\r\n",

    privateMergeStr : "\t\t[Newtonsoft.Json.JsonProperty]\r\n \t\tprivate System.Collections.Generic.Dictionary<{0}, {1}> {2}Dic;    //{3}\r\n\r\n",

    publicMergeStr : "\t\t[Newtonsoft.Json.JsonIgnore]\r\n \t\tpublic System.Collections.Generic.Dictionary<{0}, {1}> {2}Dic => {3}Dic;\r\n\r\n",
}