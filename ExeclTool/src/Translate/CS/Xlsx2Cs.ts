import xlsx from 'node-xlsx';
import path from 'path';
import fs from "fs";
import { mkdir, readdir, writeFile } from "fs/promises";
import _ from 'lodash';
import Utils from '../../utils';
import BaseTranslateConfig from '../BaseTranslateConfig';
import BaseTranslateStruct from '../BaseTranslateStruct';
import BaseTranslateEnum from '../BaseTranslateEnum';



export default class Xlsx2Cs extends BaseTranslateConfig {


    private outputPathCsStr: string = '';

    private outputPathJsonStr: string = '';

    private namespaceStart = "";

    private namespaceEnd = "";

    private configHead = "\t[Easy.Config(\"{0}\")]\r\n";

    private classStart = "\t[System.Serializable]\r\n\tpublic class {0}\r\n \t{\r\n";

    private classDictionaryStart = "\t[System.Serializable]\r\n\tpublic class {0} : System.Collections.Generic.Dictionary<{1}, {2}>\r\n \t{\r\n";

    private classEnd = "\t} \r\n";

    private notes = "/**\r\n * {0}\r\n */\r\n";

    private privateStr = "\t\t[Newtonsoft.Json.JsonProperty]\r\n \t\tprivate {0} {1};    //{2}\r\n\r\n";

    private publicStr = "\t\t[Newtonsoft.Json.JsonIgnore]\r\n \t\tpublic {0} {1} => {2};\r\n\r\n";

    private privateMergeStr = "\t\t[Newtonsoft.Json.JsonProperty]\r\n \t\tprivate System.Collections.Generic.Dictionary<{0}, {1}> {2}Dic;    //{3}\r\n\r\n";

    private publicMergeStr = "\t\t[Newtonsoft.Json.JsonIgnore]\r\n \t\tpublic System.Collections.Generic.Dictionary<{0}, {1}> {2}Dic => {3}Dic;\r\n\r\n";

    public async TranslateExcel(pathStr: string, outputPathStr: string, translate: any, params: any) : Promise<void> {

        await super.TranslateExcel(pathStr,outputPathStr,translate,params);
        
        let enumPath = path.join(params.designPath, 'define', "Enum.xlsx");
        await this.enumHelper.TranslateExcel(enumPath);

                // Generate CS enum files
        let enumCsDir = path.join(outputPathStr, 'code', 'cs');
        if (!fs.existsSync(enumCsDir)) {
            await mkdir(enumCsDir, { recursive: true });
        }
        for (let enumName in this.enumHelper.enumDefinitions) {
            let def = this.enumHelper.enumDefinitions[enumName];
            let csContent = this.notes.replace('{0}', enumName);
            csContent += '\tpublic enum ' + enumName + ' {\r\n';
            let first = true;
            for (let fieldName in def.fields) {
                if (!first) csContent += ',\r\n';
                csContent += '\t\t' + fieldName + ' = ' + def.fields[fieldName];
                first = false;
            }
            if (!first) csContent += '\r\n';
            csContent += '\t}\r\n';
            await writeFile(path.join(enumCsDir, enumName + '.cs'), csContent, { flag: 'w', encoding: 'utf8' });
        }
        let structPath = path.join(params.designPath, 'define', "Struct.xlsx");
        await this.structHelper.ParseStructDefinitions(structPath);

        this.outputPathCsStr = path.join(outputPathStr, "code", "cs");
        this.outputPathJsonStr = path.join(outputPathStr, "json");

        if(!fs.existsSync(this.outputPathCsStr)) {
            await mkdir(this.outputPathCsStr, { recursive: true });
        }
        // Generate standalone struct CS files from Struct.xlsx
        for (let structName in this.structHelper.structDefinitions) {
            let structDef = this.structHelper.structDefinitions[structName];
            let content = this.BuildStructClassContent(structName, structDef.fields);
            await this.SaveCsToFile(content, path.join(this.outputPathCsStr, structName));
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
            let data = xlsx.parse(pathStr + ".xlsx");
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
                
                let nestedFields = this.CollectNestedFields(this.xlsxData[sheetName]);
                
                
                let classContent = this.namespaceStart;
                classContent += this.notes.replace('{0}', translateName);
                if(!this.isDir) {
                    classContent += this.configHead.replace('{0}', translateName);
                }
                let csData = this.CreateCs(this.xlsxData[sheetName], translateName, nestedFields);
                classContent += csData;
                classContent += this.namespaceEnd;
                await this.SaveCsToFile(classContent, path.join(this.outputPathCsStr, translateName));
            }
        }
    }

    private async TransferTableJson(file: string = "") : Promise<void> {
        if(this.merge) {
            let all: {[key: string]: any} = {};
            for (let i = 0; i < this.translateSheets.length; ++i) {
                let sheetName = this.translateSheets[i][0];
                let translateName = this.translateSheets[i][1];
                let jsonData = this.CreateJson(this.xlsxData[sheetName], translateName);
                all[translateName] = jsonData;
            }
            await this.SaveJsonToFile(all, path.join(this.outputPathJsonStr, this.mergeName + file));
        }else{
            for (let i = 0; i < this.translateSheets.length; ++i) {
                let sheetName = this.translateSheets[i][0];
                let translateName = this.translateSheets[i][1];
                let jsonData = this.CreateJson(this.xlsxData[sheetName], translateName);
                await this.SaveJsonToFile(jsonData, path.join(this.outputPathJsonStr, translateName));
            }
        }
    }

    private CreateCs(data: any, className: string, nestedFieldDefs?: { [parentField: string]: { name: string; type: string }[] }) {
        if (!data || data.length < 2) {
            console.warn('Invalid data for CS generation:', className);
            return '';
        }

        let dataArr = data;
        let keys = dataArr[0] || [];
        let types = dataArr[1] || [];
        
        if (keys.length === 0) {
            console.warn('No keys found for CS generation:', className);
            return '';
        }

        let key = keys[0];
        let keyUpperAndLower = Utils.GetFristUpperAndLowerStr(key);
        let keyUpper = keyUpperAndLower[0];
        let type = this.TransformType(types[0]);
        let classContent = Utils.FormatStr(this.classDictionaryStart, className, type, className);

        // Generate nested struct class definitions INSIDE the main class
        if (nestedFieldDefs) {
            for (let fieldName in nestedFieldDefs) {
                let capitalized = fieldName.charAt(0).toUpperCase() + fieldName.slice(1);
                if (this.structHelper.IsStructType(capitalized)) { continue; }
                let structClassName = className + capitalized;
                let fields = this.DeduplicateFields(nestedFieldDefs[fieldName]);
                classContent += this.BuildStructClassContent(structClassName, fields);
            }
        }

        classContent += Utils.FormatStr(this.privateStr, "int", "_id", "id");
        classContent += Utils.FormatStr(this.publicStr, "int", "id", "_id");

        // ???????????????
        let structFields: { [fieldName: string]: { type: string; isArray: boolean } } = {};

        for (let colIndex = 1; colIndex < keys.length; ++colIndex) {
            let key = keys[colIndex];
            let type = types[colIndex];
            if (_.isNil(key) || _.isEmpty(key)) {
                continue;
            }
            
            let fieldPath = this.structHelper.ParseFieldPath(key);
            if (fieldPath.length > 1) {
                // ?????????????? attrs[0].value1
                // ?????????? attr.name
                let fieldName = fieldPath[0].toString();
                let isArrayStruct = fieldPath.length > 2 && !isNaN(Number(fieldPath[1]));
                let capitalized = fieldName.charAt(0).toUpperCase() + fieldName.slice(1);
                let structClassName = this.structHelper.IsStructType(capitalized) ? capitalized : className + capitalized;
                
                if (!structFields[fieldName]) {
                    structFields[fieldName] = {
                        type: structClassName,
                        isArray: isArrayStruct
                    };
                }
                continue;
            }
            
            let keyUpperAndLower = Utils.GetFristUpperAndLowerStr(key);
            let keyUpper = keyUpperAndLower[0];
            let keyLower = keyUpperAndLower[1];
            let fieldType = this.TransformType(type);
            
            classContent += Utils.FormatStr(this.privateStr, fieldType, keyLower, key);
            classContent += Utils.FormatStr(this.publicStr, fieldType, keyUpper, keyLower);
        }

        // ??????
        for (let fieldName in structFields) {
            let keyUpperAndLower = Utils.GetFristUpperAndLowerStr(fieldName);
            let keyUpper = keyUpperAndLower[0];
            let keyLower = keyUpperAndLower[1];
            let fieldInfo = structFields[fieldName];
            let fieldType = fieldInfo.isArray ? fieldInfo.type + '[]' : fieldInfo.type;
            
            classContent += Utils.FormatStr(this.privateStr, fieldType, keyLower, fieldName);
            classContent += Utils.FormatStr(this.publicStr, fieldType, keyUpper, keyLower);
        }

        classContent += this.classEnd;
        return classContent;
    }

    private CollectNestedFields(data: any): { [parentField: string]: { name: string; type: string }[] } {
        let nestedFields: { [parentField: string]: { name: string; type: string }[] } = {};
        
        if (!data || data.length < 2) {
            return nestedFields;
        }
        
        let keys = data[0] || [];
        let types = data[1] || [];
        
        for (let colIndex = 0; colIndex < keys.length; ++colIndex) {
            let key = keys[colIndex];
            let type = types[colIndex];
            if (_.isNil(key) || _.isEmpty(key)) {
                continue;
            }
            
            let fieldPath = this.structHelper.ParseFieldPath(key);
            if (fieldPath.length > 1) {
                let parentField = fieldPath[0].toString();
                if (!nestedFields[parentField]) {
                    nestedFields[parentField] = [];
                }
                let actualFieldName = fieldPath.slice(1).filter((p: any) => isNaN(parseInt(p.toString()))).join("_");
                if (actualFieldName) {
                    nestedFields[parentField].push({
                        name: actualFieldName,
                        type: type
                    });
                }
            }
        }
        
        return nestedFields;
    }
    
    private DeduplicateFields(fields: { name: string; type: string }[]): { name: string; type: string }[] {
        let seen: { [name: string]: boolean } = {};
        let result: { name: string; type: string }[] = [];
        
        for (let field of fields) {
            if (!seen[field.name]) {
                seen[field.name] = true;
                result.push(field);
            }
        }
        
        return result;
    }
    
    private BuildStructClassContent(structName: string, fields: { name: string; type: string }[]): string {
        let content = this.notes.replace('{0}', structName);
        content += this.classStart.replace('{0}', structName);
        for (let field of fields) {
            let keyUpperAndLower = Utils.GetFristUpperAndLowerStr(field.name);
            let keyUpper = keyUpperAndLower[0];
            let keyLower = keyUpperAndLower[1];
            let fieldType = this.TransformType(field.type);
            content += Utils.FormatStr(this.privateStr, fieldType, keyLower, field.name);
            content += Utils.FormatStr(this.publicStr, fieldType, keyUpper, keyLower);
        }
        content += this.classEnd;
        return content;
    }

    private async SaveStructClassToFile(structName: string, fields: { name: string; type: string }[]): Promise<void> {
        let classContent = this.notes.replace('{0}', structName);
        classContent += this.classStart.replace('{0}', structName);
        
        for (let field of fields) {
            let keyUpperAndLower = Utils.GetFristUpperAndLowerStr(field.name);
            let keyUpper = keyUpperAndLower[0];
            let keyLower = keyUpperAndLower[1];
            let fieldType = this.TransformType(field.type);
            
            classContent += Utils.FormatStr(this.privateStr, fieldType, keyLower, field.name);
            classContent += Utils.FormatStr(this.publicStr, fieldType, keyUpper, keyLower);
        }
        
        classContent += this.classEnd;
        
        await this.SaveCsToFile(classContent, path.join(this.outputPathCsStr, structName));
    }

    private CreateJson(data: any, className: string) {
        let jsonOut: { [key: string]: any } = {};

        if (!data || data.length < 3) {
            console.warn('Invalid data for', className);
            return jsonOut;
        }

        let dataArr = data;
        let keys = dataArr[0] || [];
        let types = dataArr[1] || [];

        let layerNum = 1;

        for (let rowIndex = 3; rowIndex < dataArr.length; ++rowIndex) {
            let _arrLine = dataArr[rowIndex];

            if (_.isNil(_arrLine) || _.isNil(_arrLine[0]) || _arrLine[0] == '') {
                continue;
            }

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
                if (_.isNil(key) || _.isEmpty(key)) {
                    continue;
                }
                let type = types[colIndex] || 'string';
                let value = _arrLine[colIndex];
                if (_.isNil(value) || typeof (value) == "undefined") {
                    continue;
                }

                let fieldPath = this.structHelper.ParseFieldPath(key);

                if (fieldPath.length > 1) {
                    this.structHelper.SetNestedValue(subTmp, fieldPath, this.TransformStructValue(type, value));
                } else {
                    let result = this.TransformStructValue(type, value, rowIndex, colIndex);
                    if (!_.isNil(result) && !_.isNaN(result)) {
                        if(type == "json" || type == "Json") {
                            result["$type"] = "JSONObject, Assembly-CSharp";
                        }
                        let keyLower = Utils.GetFristUpperAndLowerStr(key)[1];
                        subTmp[keyLower] = result;
                    }
                }
            }

            subTmp["$type"] = className + ", Assembly-CSharp";

            tmp[_arrLine[layerNum - 1]] = subTmp;

        }

        return jsonOut;
    }

    private SaveJsonToFile(data: any, filePath: string) {
        let _str_all = "";
        _str_all += JSON.stringify(data, null, 4);
        fs.writeFileSync(filePath + ".json", _str_all, { flag: 'w', encoding: 'utf8' });
    }

    private TransformType(type: any) {
        if (typeof type !== 'string') return type;
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
                if(this.enumHelper.enumDefinitions[type]) { 
                    result = type;
                }else{
                    result = type;
                }
                break;
        }
        return result;
    }

    private _TransformBasicsValue (type: any, data: any) {
        if (typeof type !== 'string') return data;
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
                if(this.enumHelper.enumDefinitions[type]) { 
                    result = data;
                    if (result == '') {
                        result = null;
                    }
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

    private TransformStructValue (type: string, data: string, row?: number, col?: number) {
        let result;
        if(typeof(data) == 'string') {
            data = data.replace(/[\r\n]/g, '');
        }
        if(type.includes('[]')) {
            type = type.replace('[]','');
            result = [];
            let _datas = data.substring(1,data.length -1).split(',');
            for (let i = 0; i < _datas.length; ++i) {
                result.push(this._TransformBasicsValue(type, _datas[i]));
            }
        }else if(type.includes('[,]')) {
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
            result = this._TransformBasicsValue(type, data);
        }
        return result;
    }

    private async SaveCsToFile(data: any, filePath: string) : Promise<void> {
        await writeFile(filePath + ".cs", data, { flag: 'w', encoding: 'utf8' });
    }
}

