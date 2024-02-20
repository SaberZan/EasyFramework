import xlsx from 'node-xlsx';
import path from 'path';
import fs from "fs";
import _ from 'lodash';
import { exec } from 'child_process'
import Utils from '../../Utils';
import BaseTranslate from '../BaseTranslate';

export default class Xlsx2FlatBuffers extends BaseTranslate {

    private outputPathFbsStr: string = '';

    private outputPathBinStr: string = '';

    private outputPathCodeStr: string = '';

    private outputPathJsonStr: string = '';

    private toCode: string = "";

    private namespaceStart = "namespace CfgSpace; \n\n";

    private packageCommonImport = "include \"Common.fbs\"; \n\n";

    // private namespaceStart = "";

    // private namespaceEnd = "}\n";

    private tableStart = "table {0} {\n";

    private tableEnd = "}\n\n";

    private structStart = "struct {0} {\n";

    private structEnd = "}\n\n";

    private unionStart = "union {0} {\n";

    private unionEnd = "}\n\n";

    private enumStart = "enum {0} {\n";

    private enumEnd = "}\n\n";

    private fieldStr = "\t{0} : {1};\n"

    private rootType = "root_type {0};\n\n";

    private intArray = "table IntArray { \n\t data : [int];\n}\n\n";

    private boolArray = "table BoolArray { \n\t data : [bool];\n}\n\n";

    private floatArray = "table FloatArray { \n\t data : [float];\n}\n\n";

    private stringArray = "table StringArray { \n\t data : [string];\n}\n\n";


    public BeforeTranslate(outputPathStr: string, params: any) {
        this.outputPathFbsStr = path.join(outputPathStr, "fbs");
        this.outputPathCodeStr = path.join(outputPathStr, "code" , params.toCode);
        this.outputPathCsStr = path.join(outputPathStr, "code", "cs");
        this.toCode = params.toCode;
        if(!fs.existsSync(this.outputPathFbsStr)) {
            fs.mkdirSync(this.outputPathFbsStr, { recursive: true });
        }
        if(!fs.existsSync(this.outputPathCodeStr)) {
            fs.mkdirSync(this.outputPathCodeStr, { recursive: true });
        }
        if(!fs.existsSync(this.outputPathCsStr)) {
            fs.mkdirSync(this.outputPathCsStr, { recursive: true });
        }
        this.TransferCommonFbs();
        let fbsPath = path.join(this.outputPathFbsStr, "Common.fbs");
        this.GenCode(fbsPath, this.toCode, this.outputPathCodeStr); 
    }


    public TranslateExcel(pathStr: string, outputPathStr: string, translate: any, params: any) {

        super.TranslateExcel(pathStr,outputPathStr,translate,params);

        this.outputPathFbsStr = path.join(outputPathStr , "fbs");
        this.outputPathBinStr = path.join(outputPathStr , "bin");
        this.outputPathCodeStr = path.join(outputPathStr , "code" , params.toCode);
        this.outputPathJsonStr = path.join(outputPathStr , "json");
        this.toCode = params.toCode;

        if(!fs.existsSync(this.outputPathFbsStr)) {
            fs.mkdirSync(this.outputPathFbsStr, { recursive: true });
        }
        if(!fs.existsSync(this.outputPathBinStr)) {
            fs.mkdirSync(this.outputPathBinStr, { recursive: true });
        }
        if(!fs.existsSync(this.outputPathCodeStr)) {
            fs.mkdirSync(this.outputPathCodeStr, { recursive: true });
        }
        if(!fs.existsSync(this.outputPathJsonStr)) {
            fs.mkdirSync(this.outputPathJsonStr, { recursive: true });
        }

        if(this.toDir != undefined) {
            this.outputPathJsonStr = path.join(this.outputPathJsonStr, this.toDir);
            if(!fs.existsSync(this.outputPathJsonStr)) {
                fs.mkdirSync(this.outputPathJsonStr, { recursive: true });
            }
        }

        if (this.isDir) { 
            let files = fs.readdirSync(pathStr);
            for(let i in files) {
                let data = xlsx.parse(path.join(pathStr, files[i]));
                for (let i = 0; i < data.length; ++i) {
                    this.xlsxData[data[i].name] = data[i].data;
                }
                let fileName = files[i].replace(path.extname(files[i]),"");
                this.TransferTableJson(fileName);
                if(i == "0") {
                    this.TransferTableCs();
                    this.TransferTableFbs();
                    this.GenCode(path.join(this.outputPathFbsStr, this.mergeName + ".fbs"),
                    this.toCode,
                    this.outputPathCodeStr);
                }
                this.GenBin(path.join(this.outputPathFbsStr, this.mergeName + ".fbs"),
                                path.join(this.outputPathJsonStr, this.mergeName + fileName+".json"),
                                path.join(this.outputPathBinStr, this.mergeName));
            }
        } else {
            let parsedPath = path.parse(pathStr);
            parsedPath.base += ".xlsx";
            parsedPath.ext = ".xlsx";
            let data = xlsx.parse(path.format(parsedPath));
            for (let i = 0; i < data.length; ++i) {
                this.xlsxData[data[i].name] = data[i].data;
            }
            this.TransferTableJson();
            this.TransferTableFbs();
            this.TransferTableCs();
            for (let i = 0; i < this.translateSheets.length; ++i) {
                let sheetName = this.translateSheets[i][0];
                let translateName = this.translateSheets[i][1];
                this.GenBin(path.join(this.outputPathFbsStr, translateName + ".fbs") , 
                                path.join(this.outputPathJsonStr, translateName+".json"),
                                this.outputPathBinStr);
                this.GenCode(path.join(this.outputPathFbsStr, translateName + ".fbs") ,
                                this.toCode,
                                this.outputPathCodeStr);
            }
        }
    }

    private TransferCommonFbs() {
        let fbsContent = this.namespaceStart;
        fbsContent += this.intArray;
        fbsContent += this.boolArray;
        fbsContent += this.floatArray;
        fbsContent += this.stringArray;
        this.SaveFbsToFile(fbsContent, path.join(this.outputPathFbsStr, "Common"));
    }

    private TransferTableFbs() {
        if(this.merge) {

            let fbsContent = this.packageCommonImport; 
            fbsContent += this.namespaceStart;

            for (let i = 0; i < this.translateSheets.length; ++i) {
                let sheetName = this.translateSheets[i][0];
                let translateName = this.translateSheets[i][1];
                fbsContent += this.createFbs(this.xlsxData[sheetName],translateName);
            }

            fbsContent += Utils.FormatStr(this.tableStart, this.mergeName);
            for (let i = 0; i < this.translateSheets.length; ++i) {
                let sheetName = this.translateSheets[i][0];
                let translateName = this.translateSheets[i][1];
                let upAndLower = Utils.GetFristUpperAndLowerStr(translateName);
                let translateNamekeyUpper = upAndLower[0];
                let translateNamekeyLower = upAndLower[1];
                fbsContent += Utils.FormatStr(this.fieldStr, translateNamekeyLower + "Array", "[" + translateNamekeyUpper + "]");
            }
            fbsContent += this.tableEnd;

            fbsContent += Utils.FormatStr(this.rootType, this.mergeName);

            this.SaveFbsToFile(fbsContent, path.join(this.outputPathFbsStr, this.mergeName));
        }else{
            for (let i = 0; i < this.translateSheets.length; ++i) {

                let sheetName = this.translateSheets[i][0];
                let translateName = this.translateSheets[i][1];
                let upAndLower = Utils.GetFristUpperAndLowerStr(translateName);
                let translateNamekeyUpper = upAndLower[0];
                let translateNamekeyLower = upAndLower[1];
                let fbsContent = this.packageCommonImport;
                fbsContent += this.namespaceStart;
                fbsContent += this.createFbs(this.xlsxData[sheetName],translateName);
                fbsContent += Utils.FormatStr(this.tableStart, translateName + "s");
                fbsContent += Utils.FormatStr(this.fieldStr, translateNamekeyLower + "Array","[" + translateName + "]");
                fbsContent += this.tableEnd;
                fbsContent += Utils.FormatStr(this.rootType, translateName + "s");
                this.SaveFbsToFile(fbsContent, path.join(this.outputPathFbsStr, translateName));
            }
        }
    }

    private createFbs(data: any, className: string) {

        let tableContent = Utils.FormatStr(this.tableStart, className);
        let dataArr = data;
        let keys = dataArr[0];
        let types = dataArr[1];
        let decs = dataArr[2];
        for (let keyIndex = 0; keyIndex < keys.length; ++keyIndex) {
            let key = keys[keyIndex];
            if (_.isNil(key) || _.isEmpty(key)) {
                continue;
            }
            let type = this.TransformType(types[keyIndex]);
            if(type != undefined){
                let keyLower = Utils.GetFristUpperAndLowerStr(key)[1];
                tableContent += Utils.FormatStr(this.fieldStr, keyLower, type);
            }
        }
        tableContent += this.tableEnd;
        return tableContent;
    }

    private SaveFbsToFile(data: any, filePath: string) {
        fs.writeFileSync(filePath + ".fbs", data, { flag: 'w', encoding: 'utf8' });
    }

    private TransferTableJson(file: string = "") {
        if(this.merge) {
            let all: {[key: string]: any} = {};
            for (let i = 0; i < this.translateSheets.length; ++i) {
                let sheetName = this.translateSheets[i][0];
                let translateName = this.translateSheets[i][1];
                let upAndLower = Utils.GetFristUpperAndLowerStr(translateName);
                let translateNamekeyUpper = upAndLower[0];
                let translateNamekeyLower = upAndLower[1];
                let jsonData = this.CreateJson(this.xlsxData[sheetName],translateName);
                all[translateNamekeyLower + "Array"] = jsonData[translateNamekeyLower + "Array"];
            }
            this.SaveJsonToFile(all, path.join(this.outputPathJsonStr, this.mergeName + file));
        }else{
            for (let i = 0; i < this.translateSheets.length; ++i) {
                let sheetName = this.translateSheets[i][0];
                let translateName = this.translateSheets[i][1];
                let jsonData = this.CreateJson(this.xlsxData[sheetName],translateName);
                this.SaveJsonToFile(jsonData, path.join(this.outputPathJsonStr, translateName));
            }
        }
    }

    private CreateJson(data: any, className: string) {
        let jsonOut: { [key: string]: any }[] = [];

        let dataArr = data;
        let keys = dataArr[0];
        let types = dataArr[1];

        //检测层级结构，有多少层级
        let layerNum = 0;
        for (let typeIndex = 0; typeIndex < types.length; ++typeIndex) {
            let type = types[typeIndex];
            if (type && type[0] == '#') {
                layerNum += 1;
            }
        }

        //若没有配置，必有一层层级
        if (layerNum === 0) {
            layerNum = 1;
        }

        let index = 0;
        for (let rowIndex = 3; rowIndex < dataArr.length; ++rowIndex) {
            let _arrLine = dataArr[rowIndex];

            if (_.isNil(_arrLine[0]) || _arrLine[0] == '') {
                continue;
            }

            let subTmp: { [key: string]: any } = {};
            for (let colIndex = 0; colIndex < keys.length; ++colIndex) {
                let key = keys[colIndex];
                let type = types[colIndex];
                let value = _arrLine[colIndex];
                if (_.isNil(key) || _.isEmpty(key) || typeof(value) == "undefined") {
                    continue;
                }

                let result = this.TransformValue(type, _arrLine[colIndex], rowIndex, colIndex); //检测类型，传入所在的行和列，方便报错检查
                if (!_.isNil(result) && !_.isNaN(result)) {
                    let keyLower = Utils.GetFristUpperAndLowerStr(key)[1];
                    subTmp[keyLower] = result;
                }
            }

            jsonOut[index] = subTmp;

            ++index;
        }

        let output : {[key:string]: object} = {};
        className = Utils.GetFristUpperAndLowerStr(className)[1];
        output[className + "Array"] = jsonOut;
        return output;
    }

    private SaveJsonToFile(data: any, filePath: string) {
        var _str_all = "";
        _str_all += JSON.stringify(data, null, 4);
        fs.writeFileSync(filePath + ".json", _str_all, { flag: 'w', encoding: 'utf8' });
    }

    private GenBin(fbsPath: string, jsonPath: string, outputPath: string) {
        let cmd = ".\\lib\\flatc\\flatc.exe " + " -I " + path.dirname(fbsPath)   + " -o " + outputPath + " -b " + fbsPath + " " + jsonPath;
        exec(cmd, (err, stdout, stderr) => {
            if(err) {
                throw err;
            }
            console.log(stdout);  // stdout为执行命令行操作后返回的正常结果
            console.log(stderr);  // stderr为执行命令行操作后返回的错误提示
        });
    }

    private GenCode(fbsPath: string, toCode: string, outputPath: string) {
        let cmd = ".\\lib\\flatc\\flatc.exe -o " + outputPath + " --" + toCode +" " + fbsPath;
        exec(cmd, (err, stdout, stderr) => {
            if(err) {
                throw err;
            }
            console.log(stdout);  // stdout为执行命令行操作后返回的正常结果
            console.log(stderr);  // stderr为执行命令行操作后返回的错误提示
        });
    }

    private TransformType(type: string) {
        let result;
        switch (type) {
            case 'int':
            case 'Int':
                result = 'int';
                break;
            case 'int[]':
            case 'Int[]':
                result = '[int]';
                break;   
            case 'int[,]':
            case 'Int[,]':
                result = '[CfgSpace.IntArray]';
                break;   
            case 'float':
            case 'Float':
                result = 'float';
                break;
            case 'float[]':
            case 'Float[]':
                result = '[float]';
                break;
            case 'float[,]':
            case 'Float[,]':
                result = '[CfgSpace.FloatArray]';
                break;    
            case 'bool':
            case 'Bool':
            case 'boolen':
            case 'Boolen':
                result = 'bool';
                break;
            case 'bool[]':
            case 'Bool[]':
            case 'boolen[]':
            case 'Boolen[]':
                result = '[bool]';
                break;
            case 'bool[,]':
            case 'Bool[,]':
            case 'boolen[,]':
            case 'Boolen[,]':
                result = '[CfgSpace.BoolArray]';
                break;
            case 'string':
            case 'String':
                result = 'string';
                break;
            case 'string[]':
            case 'String[]':
                result = '[string]';
            case 'string[,]':
            case 'String[,]':
                result = '[CfgSpace.StringArray]';
            default:
                if(type.includes('serialize')) {
                    result = undefined;
                }else{
                    result = type;
                }
                break;
        }
        return result;
    }

    //翻译配置的字段到对应应该有的数据类型。
    private _TransformBasicsValue (type: string, data: any) {
        let result;
        switch (type) {
            case 'int':
            case 'Int':
                result = parseInt(data);
                break;
            case 'float':
            case 'Float':
                result = parseFloat(data);
                break;
            case 'bool':
            case 'Bool':
            case 'boolen':
            case 'Boolen':
                result = Boolean(data);
                break;
            case 'string':
            case 'String':
                result = data;
                if (result == '') {
                    result = null;
                }
                break;
            default:
                result = data;
                if (result == '') {
                    result = null;
                }
                break;
        }
        return result;
    }

    //检测type 对应的值
    private TransformValue (type: string, data: string, row?: number, col?: number) {

        if(type.includes('serialize')) {
            return undefined;
        }
        
        let result;
        if(typeof(data) == 'string') {
            data = data.replace(/[\r\n]/g, '');
        }
        if(type.includes('serialize')) {
            result = this._TransformBasicsValue(type, data);
        }else if(type.includes('[]')) {   
            type = type.replace('[]','');
            result = [];
            let _datas = data.substring(1,data.length -1).split(',');
            for (let i = 0; i < _datas.length; ++i) {
                result.push(this._TransformBasicsValue(type, _datas[i]));
            }

        }else if(type.includes('[,]')) {
            //二维数组
            type = type.replace('[,]','');
            result = [];
            let datas = data.substring(2,data.length-2).split('],[');
            for (let i = 0; i < datas.length; ++i) {
                let tmpResult = []
                let _datas = datas[i].split(',');
                for (let j = 0; j < _datas.length; ++j) {
                    tmpResult.push(this._TransformBasicsValue(type, _datas[j]));
                }
                result.push({data : tmpResult});
            }

        }else{
            //正常值
            result = this._TransformBasicsValue(type, data);
        }
        return result;
    }

    private outputPathCsStr: string = '';

    // private csNamespaceStart = "namespace ReadCfgSpace\r\n{\r\n";
    private csNamespaceStart = "";
    private csConfigHead = "\t[Easy.Config(\"{0}\")]\r\n"
    private csClassDictionaryStart = "\tpublic class {0}Dictionary : System.Collections.Generic.Dictionary<{1}, CfgSpace.{2}>\r\n\t{\r\n" 
    private csNotes = "/**\r\n * {0}\r\n */\r\n"
    private csCreateFunStart = "\t\tpublic static {0}Dictionary CreateConfig(Google.FlatBuffers.ByteBuffer byteBuffer)\r\n \t\t{\r\n"
    private csMergeCreateFunStart = "\t\tpublic static {0} CreateConfig(Google.FlatBuffers.ByteBuffer byteBuffer)\r\n \t\t{\r\n"
    private csCreateFunLine1 = "\t\t\t{0}Dictionary cfgs = new {1}Dictionary();\r\n"
    private csMergeCreateFunLine1 = "\t\t\t\{0} cfgs = new {1}();\r\n"
    private csCreateFunLine2 = "\t\t\tCfgSpace.{0} configData = CfgSpace.{1}.GetRootAs{2}(byteBuffer);\r\n"
    private csCreateFunLine3 = "\t\t\tfor(int i = 0; i < configData.{0}ArrayLength; ++i)\r\n\t\t\t{\r\n"
    private csCreateFunLine4 = "\t\t\t\tCfgSpace.{0} cfg = (CfgSpace.{1})configData.{2}Array(i);\r\n"
    private csCreateFunLine5 = "\t\t\t\tcfgs.Add(cfg.{0}, cfg);\r\n\t\t\t}"
    private csCreateFunLine6 = "\r\n\t\t\treturn cfgs;\r\n"
    private csMergeCreateFunLine7 = "\t\t\t\cfgs.{0}Dictionary = {1}Dictionary.CreateConfig(configData);\r\n"
    private csCreateFunEnd = "\t\t}\r\n";
    private csClassEnd = "\t}\r\n";
    // private csNamespaceEnd = "}\r\n";
    private csNamespaceEnd = "";
    private csSubMergeCreateFunStart = "\t\tpublic static {0}Dictionary CreateConfig(CfgSpace.{1} configData)\r\n \t\t{\r\n"
    private csClassStart = "\tpublic class {0}\r\n\t{\r\n"
    private publicDictionaryStr = "\t\tpublic {0}Dictionary {1}Dictionary;\r\n\r\n";
      
    private TransferTableCs() {
        if(this.merge) {
            let classContent = this.csNamespaceStart;
            for (let i = 0; i < this.translateSheets.length; ++i) {
                let csData = this.CreateMergeCs(this.xlsxData[this.translateSheets[i][0]],this.translateSheets[i][1], this.mergeName);
                classContent += csData;
            }

            classContent += Utils.FormatStr(this.csNotes, this.mergeName);
            if(!this.isDir) {
                classContent += Utils.FormatStr(this.csConfigHead, this.mergeName);
            }
            classContent += Utils.FormatStr(this.csClassStart, this.mergeName);
            for (let i = 0; i < this.translateSheets.length; ++i) {
                let type = this.xlsxData[this.translateSheets[i][0]][1][0];
                let translateName = this.translateSheets[i][1];
                let upperAndLower = Utils.GetFristUpperAndLowerStr(translateName);
                let translateNamekeyUpper = upperAndLower[0];
                let translateNamekeyLower = upperAndLower[1];
                classContent += Utils.FormatStr(this.publicDictionaryStr, translateNamekeyUpper, translateNamekeyLower);
            }

            classContent += Utils.FormatStr(this.csMergeCreateFunStart, this.mergeName);
            classContent += Utils.FormatStr(this.csMergeCreateFunLine1, this.mergeName, this.mergeName);
            classContent += Utils.FormatStr(this.csCreateFunLine2, this.mergeName, this.mergeName, this.mergeName);

            for (let i = 0; i < this.translateSheets.length; ++i) {
                let type = this.xlsxData[this.translateSheets[i][0]][1][0];
                let translateName = this.translateSheets[i][1];
                let upperAndLower = Utils.GetFristUpperAndLowerStr(translateName);
                let translateNamekeyUpper = upperAndLower[0];
                let translateNamekeyLower = upperAndLower[1];
                classContent += Utils.FormatStr(this.csMergeCreateFunLine7, translateNamekeyLower, translateNamekeyUpper);
            }
            classContent += this.csCreateFunLine6;
            classContent += this.csCreateFunEnd;
            classContent += this.csClassEnd;
            classContent += this.csNamespaceEnd;

            this.SaveCsToFile(classContent, path.join(this.outputPathCsStr, this.mergeName));
        }else{
            for (let i = 0; i < this.translateSheets.length; ++i) {
                let sheetName = this.translateSheets[i][0];
                let translateName = this.translateSheets[i][1];
                let classContent = this.csNamespaceStart;
                classContent += Utils.FormatStr(this.csNotes, translateName);
                classContent += Utils.FormatStr(this.csConfigHead, translateName);
                let csData = this.CreateCs(this.xlsxData[sheetName],translateName);
                classContent += csData;
                classContent += this.csNamespaceEnd;
                this.SaveCsToFile(classContent, path.join(this.outputPathCsStr, translateName));
            }
        }
    }

    private CreateCs(data: any, className: string) {
        let dataArr = data;
        let keys = dataArr[0];
        let types = dataArr[1];
        let key = keys[0];
        let keyUpperAndLower = Utils.GetFristUpperAndLowerStr(key);
        let keyUpper = keyUpperAndLower[0];
        let type = this.TransformCsType(types[0]);
        let classContent = Utils.FormatStr(this.csClassDictionaryStart, className, type, className);
        classContent += Utils.FormatStr(this.csCreateFunStart, className);
        classContent += Utils.FormatStr(this.csCreateFunLine1, className, className);
        classContent += Utils.FormatStr(this.csCreateFunLine2, className + "s", className + "s", className + "s");
        classContent += Utils.FormatStr(this.csCreateFunLine3, className);
        classContent += Utils.FormatStr(this.csCreateFunLine4, className, className, className);
        classContent += Utils.FormatStr(this.csCreateFunLine5, keyUpper);
        classContent += this.csCreateFunLine6;
        classContent += this.csCreateFunEnd;
        classContent += this.csClassEnd;
        return classContent;
    }

    private CreateMergeCs(data: any, className: string, mergeName: string) {
        let dataArr = data;
        let keys = dataArr[0];
        let types = dataArr[1];
        let key = keys[0];
        let type = this.TransformCsType(types[0]);
        let classContent = Utils.FormatStr(this.csNotes, className);
        classContent += Utils.FormatStr(this.csClassDictionaryStart, className, type, className);
        classContent += Utils.FormatStr(this.csSubMergeCreateFunStart, className, mergeName);
        classContent += Utils.FormatStr(this.csCreateFunLine1, className, className);
        classContent += Utils.FormatStr(this.csCreateFunLine3, className);
        classContent += Utils.FormatStr(this.csCreateFunLine4, className, className, className);
        classContent += Utils.FormatStr(this.csCreateFunLine5, key);
        classContent += this.csCreateFunLine6;
        classContent += this.csCreateFunEnd;
        classContent += this.csClassEnd;
        return classContent;
    }

    private TransformCsType(type: string) {
        let result;
        switch (type) {
            case 'int':
            case 'Int':
                result = 'int';
                break;
            case 'float':
            case 'Float':
                result = 'float';
                break;
            case 'bool':
            case 'Bool':
            case 'boolen':
            case 'Boolen':
                result = 'bool';
                break;
            case 'string':
            case 'String':
                result = 'string';
                break;
            default:
                result = type;
                break;
        }
        return result;
    }

    private SaveCsToFile(data: any, filePath: string) {
        fs.writeFileSync(filePath + ".cs", data, { flag: 'w', encoding: 'utf8' });
    }
}