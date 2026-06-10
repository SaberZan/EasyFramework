export default {

    namespaceStart : 'namespace CfgSpace; \n\n',
    packageCommonImport : 'include \"Common.fbs\"; \n\n',

    tableStart : 'table {0} {\n',
    tableEnd : '}\n\n',
    structStart : 'table {0} {\n',
    structEnd : '}\n\n',
    fieldStr : '    {0} : {1};\n',
    rootType : 'root_type {0};\n\n',

    csNotes : '/**\n * {0}\n */\n',
    csConfigHead : '    [Easy.Config(\"{0}\")]\n',
    csClassDictionaryStart : 'public static class {0} : System.Collections.Generic.Dictionary<{1}, {2}>\n{\n',
    csPrivateStr : '        private {0} _{1};\n',
    csPublicStr : '        public static {0} {1} => _{1};\n\n',
    csClassEnd : '}\n\n',

    intArray : "table IntArray { \n\t data : [int];\n}\n\n",
    boolArray : "table BoolArray { \n\t data : [bool];\n}\n\n",
    floatArray : "table FloatArray { \n\t data : [float];\n}\n\n",
    stringArray : "table StringArray { \n\t data : [string];\n}\n\n",
}