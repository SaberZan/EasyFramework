import xlsx from 'node-xlsx';
import path from 'path';
import fs from "fs";
import { mkdir, readdir, writeFile } from "fs/promises";
import _ from 'lodash';
import Utils from '../../Utils';
import BaseTranslate from '../BaseTranslate';

export default class Xlsx2Cs extends BaseTranslate {

    private outputPathCsStr: string = '';

    private outputPathJsonStr: string = '';

    // private namespaceStart = 'using {0}; \r\n{ \r\n';

    // private namespaceEnd = "}";

    private namespaceStart = "";

    private namespaceEnd = "";

    private configHead = "\t[Easy.Config(\"{0}\")]\r\n";

    private classStart = "\tpublic class {0}\r\n \t{\r\n";

    private classDictionaryStart = "\tpublic class {0} : System.Collections.Generic.Dictionary<{1}, {2}>\r\n \t{\r\n";

    private classEnd = "\t} \r\n";

    private notes = "/**\r\n * {0}\r\n */\r\n";

    private privateStr = "\t\t[Newtonsoft.Json.JsonProperty]\r\n \t\tprivate {0} {1};    //{2}\r\n\r\n";

    private publicStr = "\t\t[Newtonsoft.Json.JsonIgnore]\r\n \t\tpublic {0} {1} => {2};\r\n\r\n";

    private privateMergeStr = "\t\t[Newtonsoft.Json.JsonProperty]\r\n \t\tprivate System.Collections.Generic.Dictionary<{0}, {1}> {2}Dic;    //{3}\r\n\r\n";

    private publicMergeStr = "\t\t[Newtonsoft.Json.JsonIgnore]\r\n \t\tpublic System.Collections.Generic.Dictionary<{0}, {1}> {2}Dic => {3}Dic;\r\n\r\n";

    public async TranslateExcel(pathStr: string, outputPathStr: string, translate: any, params: any) : Promise<void> {

        await super.TranslateExcel(pathStr,outputPathStr,translate,params);

        this.outputPathCsStr = path.join(outputPathStr, "code", "cs");
        this.outputPathJsonStr = path.join(outputPathStr, "json");

        if(!fs.existsSync(this.outputPathCsStr)) {
            await mkdir(this.outputPathCsStr, { recursive: true });
        }
        if(!fs.existsSync(this.outputPathJsonStr)) {
            await mkdir(this.outputPathJsonStr, { recursive: true });
        }

        if(this.toDir != undefined) {
            this.outputPathJsonStr = path.join(this.outputPathJsonStr, this.toDir);
            if(!fs.existsSync(this.outputPathJsonStr)) {
                await mkdir(this.outputPathJsonStr);
            }
        }

        if (this.isDir) { 
            let files = await readdir(pathStr);
            for(let i in files) {
                let data = xlsx.parse(path.join(pathStr, files[i]));
                for (let i = 0; i < data.length; ++i) {
                    this.xlsxData[data[i].name] = data[i].data;
                }
                await this.TransferTableJson(files[i].replace(path.extname(files[i]),""));
                if(i == "0") {
                    await this.TransferTableCs();
                }
            }
        } else {
            let parsedPath = path.parse(pathStr);
            parsedPath.base += ".xlsx";
            parsedPath.ext = ".xlsx";
            let data = xlsx.parse(path.format(parsedPath));
            for (let i = 0; i < data.length; ++i) {
                this.xlsxData[data[i].name] = data[i].data;
            }
            await this.TransferTableJson();
            await this.TransferTableCs();
        }
    }

    private async TransferTableCs() : Promise<void> {
        if(this.merge) {

            let classContent = this.namespaceStart;
            for (let i = 0; i < this.translateSheets.length; ++i) {
                let csData = this.CreateCs(this.xlsxData[this.translateSheets[i][0]],this.translateSheets[i][1]);
                classContent += csData;
            }

            classContent += this.notes.replace('{0}', this.mergeName);
            if(!this.isDir) {
                classContent += this.configHead.replace('{0}', this.mergeName);
            }
            classContent += this.classStart.replace('{0}', this.mergeName);
            for (let i = 0; i < this.translateSheets.length; ++i) {
                let type = this.xlsxData[this.translateSheets[i][0]][1][0];
                let sheetName = this.translateSheets[i][0];
                let translateName = this.translateSheets[i][1];
                let upperAndLower = Utils.GetFristUpperAndLowerStr(translateName);
                let translateNamekeyUpper = upperAndLower[0];
                let translateNamekeyLower = upperAndLower[1];
                classContent += Utils.FormatStr(this.privateMergeStr, type, translateNamekeyUpper, translateNamekeyLower);
                classContent += Utils.FormatStr(this.publicMergeStr, type, translateNamekeyUpper, translateNamekeyUpper, translateNamekeyLower);
            }
            classContent += this.classEnd;
            classContent += this.namespaceEnd;

            await this.SaveCsToFile(classContent, path.join(this.outputPathCsStr, this.mergeName));
        }else{
            for (let i = 0; i < this.translateSheets.length; ++i) {
                let sheetName = this.translateSheets[i][0];
                let translateName = this.translateSheets[i][1];
                let upAndLower = Utils.GetFristUpperAndLowerStr(translateName);
                let translateNamekeyUpper = upAndLower[0];
                let translateNamekeyLower = upAndLower[1];

                let classContent = this.namespaceStart;

                let csData = this.CreateCs(this.xlsxData[sheetName],translateName);
                classContent += csData;

                classContent += Utils.FormatStr(this.notes, translateName + "Dictionary");
                classContent += Utils.FormatStr(this.configHead, translateName);

                let type = this.xlsxData[this.translateSheets[i][0]][1][0];
                classContent += Utils.FormatStr(this.classDictionaryStart, translateName + "Dictionary", type, translateName);
                classContent += this.classEnd;
                classContent += this.namespaceEnd;
    
                await this.SaveCsToFile(classContent, path.join(this.outputPathCsStr, translateName));
            }
        }
    }

    private CreateCs(data: any, className: string) {

        let classContent = this.notes.replace('{0}', className);
        classContent += this.classStart.replace('{0}', className);

        let dataArr = data;
        let keys = dataArr[0];
        let types = dataArr[1];
        let decs = dataArr[2];
        for (let keyIndex = 0; keyIndex < keys.length; ++keyIndex) {
            let key = keys[keyIndex];
            if (_.isNil(key) || _.isEmpty(key)) {
                continue;
            }
            // console.log("key = " + key);
            let type = this.TransformType(types[keyIndex]);
            let upperAndLower = Utils.GetFristUpperAndLowerStr(key);
            let keyUpper = upperAndLower[0];
            let keyLower = upperAndLower[1];
            let dec = decs[keyIndex];
            if(dec != undefined)
                dec = dec.replace(/[\r\n]/g, '');
            classContent += Utils.FormatStr(this.privateStr, type, keyLower, dec);
            classContent += Utils.FormatStr(this.publicStr, type, keyUpper, keyLower);
        }
        classContent += this.classEnd;
        return classContent;
    }

    private async SaveCsToFile(data: any, filePath: string) : Promise<void> {
        await writeFile(filePath + ".cs", data, { flag: 'w', encoding: 'utf8' });
    }

    private async TransferTableJson(file: string = "") : Promise<void> {
        if(this.merge) {
            let all: {[key: string]: any} = {};
            for (let i = 0; i < this.translateSheets.length; ++i) {
                let sheetName = this.translateSheets[i][0];
                let translateName = this.translateSheets[i][1];
                let upperAndLower = Utils.GetFristUpperAndLowerStr(translateName);
                let translateNamekeyUpper = upperAndLower[0];
                let translateNamekeyLower = upperAndLower[1];
                let jsonData = this.CreateJson(this.xlsxData[sheetName],translateName);
                all[translateNamekeyLower + "Dic"] = jsonData;
            }
            await this.SaveJsonToFile(all, path.join(this.outputPathJsonStr, this.mergeName + file));
        }else{
            for (let i = 0; i < this.translateSheets.length; ++i) {
                let sheetName = this.translateSheets[i][0];
                let translateName = this.translateSheets[i][1];
                let jsonData = this.CreateJson(this.xlsxData[sheetName],translateName);
                await this.SaveJsonToFile(jsonData, path.join(this.outputPathJsonStr, translateName));
            }
        }
    }

    private CreateJson(data: any, className: string) {
        let jsonOut: { [key: string]: any } = {};

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

        for (let rowIndex = 3; rowIndex < dataArr.length; ++rowIndex) {
            let _arrLine = dataArr[rowIndex];

            if (_.isNil(_arrLine[0]) || _arrLine[0] == '') {
                continue;
            }

            // 遍历引用，方便定义对象。
            let tmp = jsonOut;
            for (let layIndex = 0; layIndex < layerNum - 1; ++layIndex) {
                if (!tmp[_arrLine[layIndex]]) {
                    tmp[_arrLine[layIndex]] = {};
                }
                tmp = tmp[_arrLine[layIndex]];
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
                    if(type == "json" || type == "Json") {
                        result["$type"] = "JSONObject, Assembly-CSharp";
                    }
                    let keyLower = Utils.GetFristUpperAndLowerStr(key)[1];
                    subTmp[keyLower] = result;
                }
            }
            subTmp["$type"] = className + ", Assembly-CSharp";

            tmp[_arrLine[layerNum - 1]] = subTmp;

        }

        let output = jsonOut
        return output;
    }

    private SaveJsonToFile(data: any, filePath: string) {
        var _str_all = "";
        _str_all += JSON.stringify(data, null, 4);
        fs.writeFileSync(filePath + ".json", _str_all, { flag: 'w', encoding: 'utf8' });
    }

    private TransformType(type: string) {
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
            case 'json':
            case 'Json':
                result = "JSONObject";
                break;
            default:
                if(type.includes('serialize')) {
                    result = type.replace('serialize-','');
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
            case 'json':
            case 'Json':
                result = JSON.parse(data);
                break;
            default:
                if(type.includes('serialize')) {
                    type = type.replace('serialize-','');
                    result = JSON.parse(data);
                }else{
                    result = data;
                    if (result == '') {
                        result = null;
                    }
                }
                break;
        }
        return result;
    }

    //检测type 对应的值
    private TransformValue (type: string, data: string, row?: number, col?: number) {

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
                result.push(tmpResult);
            }

        }else{
            //正常值
            result = this._TransformBasicsValue(type, data);
        }
        return result;
    }
}