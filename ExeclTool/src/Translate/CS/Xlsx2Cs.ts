import xlsx from 'node-xlsx';
import DataParser from '../../DataParser';
import path from 'path';
import fs from "fs";
import { mkdir, readdir, writeFile } from "fs/promises";
import _ from 'lodash';
import CSDefine from '../CS/CSDefine';
import Utils from '../../utils';
import BaseTranslateConfig from '../BaseTranslateConfig';
import BaseTranslateStruct from '../BaseTranslateStruct';
import BaseTranslateEnum from '../BaseTranslateEnum';



export default class Xlsx2Cs extends BaseTranslateConfig {


    private outputPathCsStr: string = '';

    private outputPathJsonStr: string = '';

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
                let data = DataParser.parse(path.join(pathStr, files[i]), params.format);
                for (let j = 0; j < data.length; ++j) {
                    this.xlsxData[data[j].name] = data[j].data;
                }
                if (this.merge) {
                    await this.TransferTableJson(files[i].replace(path.extname(files[i]),""));
                }
            }
            if (!this.merge) {
                await this.TransferTableJson();
            }
            await this.TransferTableCs();
        } else {
            let data = DataParser.parseWithOptionalExtension(pathStr, params.format);
            for (let i = 0; i < data.length; ++i) {
                this.xlsxData[data[i].name] = data[i].data;
            }
            await this.TransferTableJson();
            await this.TransferTableCs();
        }
    }

    private async TransferTableCs() : Promise<void> {
        if(this.merge) {
            let classContent = CSDefine.namespaceStart;
            // Generate only the record class for each sheet (no individual Dictionary classes)
            for (let i = 0; i < this.translateSheets.length; ++i) {
                let sheetName = this.translateSheets[i][0];
                let translateName = this.translateSheets[i][1];
                let nestedFields = this.CollectNestedFields(this.xlsxData[sheetName]);
                let csData = this.CreateCsRecordOnly(this.xlsxData[sheetName], translateName, nestedFields);
                classContent += csData;
            }

            // Generate merge wrapper class with Dictionary fields
            classContent += CSDefine.notes.replace('{0}', this.mergeName);
            classContent += CSDefine.configHead.replace('{0}', this.mergeName);
            classContent += CSDefine.classStart.replace('{0}', this.mergeName);
            for (let i = 0; i < this.translateSheets.length; ++i) {
                let sheetName = this.translateSheets[i][0];
                let translateName = this.translateSheets[i][1];
                let data = this.xlsxData[sheetName];
                let keyType = 'int';
                // Check for array mode (@ prefix)
                let isArrayMode = data && data.length > 0 && data[0] && data[0].length > 0 && typeof data[0][0] === 'string' && data[0][0].startsWith('@');
                let valueType = translateName;
                if (isArrayMode) {
                    let atFieldName = data[0][0].substring(1);
                    valueType = translateName + atFieldName;
                }
                let keyUpperAndLower = Utils.GetFristUpperAndLowerStr(translateName);
                let keyLower = keyUpperAndLower[1];
                let keyUpper = keyUpperAndLower[0];
                // Generate Dictionary field
                classContent += Utils.FormatStr(CSDefine.privateMergeStr, keyType, valueType, keyLower, '');
                classContent += Utils.FormatStr(CSDefine.publicMergeStr, keyType, valueType, keyUpper, keyLower);
            }
            classContent += CSDefine.classEnd;
            classContent += CSDefine.namespaceEnd;

            await this.SaveCsToFile(classContent, path.join(this.outputPathCsStr, this.mergeName));
        }else{
            for (let i = 0; i < this.translateSheets.length; ++i) {
                let sheetName = this.translateSheets[i][0];
                let translateName = this.translateSheets[i][1];
                
                let nestedFields = this.CollectNestedFields(this.xlsxData[sheetName]);
                
                let classContent = CSDefine.namespaceStart;
                let csData = this.CreateCs(this.xlsxData[sheetName], translateName, nestedFields);
                
                classContent += csData;
                classContent += CSDefine.namespaceEnd;
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
                // Use lowercase start + Dic suffix to match C# private field name for JSON deserialization
                let keyLower = translateName.charAt(0).toLowerCase() + translateName.slice(1);
                all[keyLower + 'Dic'] = jsonData;
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
        // Copy keys so stripping the @ prefix does not mutate the shared sheet data.
        let keys = (dataArr[0] || []).slice();
        let typeRowIndex = this.FindTypeRowIndex(dataArr);
        let types = typeRowIndex >= 0 ? (dataArr[typeRowIndex] || []) : [];

        if (keys.length === 0) {
            console.warn('No keys found for CS generation:', className);
            return '';
        }

        let key = keys[0];
        // Array mode: first column key starts with @, e.g. @ItemSeries.
        // The table becomes Dictionary<key, List<Record>> instead of Dictionary<key, Record>.
        let isArrayMode = false;
        if (typeof key === 'string' && key.startsWith('@')) {
            isArrayMode = true;
            key = key.substring(1);
            keys[0] = key;
        }
        let keyType = this.TransformType(types[0]);
        let valueType = isArrayMode
            ? 'System.Collections.Generic.List<' + className + '>'
            : className;

        let classContent = '';

        // Generate nested struct class definitions BEFORE the main class
        if (nestedFieldDefs) {
            for (let fieldName in nestedFieldDefs) {
                let capitalized = fieldName.charAt(0).toUpperCase() + fieldName.slice(1);
                if (this.structHelper.IsStructType(capitalized)) { continue; }
                let structClassName = className + capitalized;
                let fields = this.DeduplicateFields(nestedFieldDefs[fieldName]);
                classContent += this.BuildStructClassContent(structClassName, fields);
            }
        }

        // Record class: plain data class (not a Dictionary)
        classContent += CSDefine.notes.replace('{0}', className);
        classContent += CSDefine.classStart.replace('{0}', className);

        // Collect struct fields for nested struct support.
        let structFields: { [fieldName: string]: { type: string; isArray: boolean } } = {};

        for (let colIndex = 0; colIndex < keys.length; ++colIndex) {
            let key = keys[colIndex];
            let type = types[colIndex];
            if (_.isNil(key) || _.isEmpty(key) || key.startsWith('#')) {
                continue;
            }
            
            let fieldInfo = this.structHelper.AnalyzeField(key, type);
            if (fieldInfo.isStruct && fieldInfo.fieldPath.length > 0) {
                let fieldName = fieldInfo.name;
                let structClassName = fieldInfo.structName || (fieldName.charAt(0).toUpperCase() + fieldName.slice(1));
                if (!this.structHelper.IsStructType(structClassName)) {
                    structClassName = className + structClassName;
                }
                
                if (!structFields[fieldName]) {
                    structFields[fieldName] = {
                        type: structClassName,
                        isArray: fieldInfo.isArray
                    };
                }
                continue;
            }
            
            let keyUpperAndLower = Utils.GetFristUpperAndLowerStr(key);
            let keyUpper = keyUpperAndLower[0];
            let keyLower = keyUpperAndLower[1];
            let fieldType = this.TransformType(type);
            
            classContent += Utils.FormatStr(CSDefine.privateStr, fieldType, keyLower, key);
            classContent += Utils.FormatStr(CSDefine.publicStr, fieldType, keyUpper, keyLower);
        }

        // ??????
        for (let fieldName in structFields) {
            let keyUpperAndLower = Utils.GetFristUpperAndLowerStr(fieldName);
            let keyUpper = keyUpperAndLower[0];
            let keyLower = keyUpperAndLower[1];
            let fieldInfo = structFields[fieldName];
            let fieldType = fieldInfo.isArray ? fieldInfo.type + '[]' : fieldInfo.type;
            
            classContent += Utils.FormatStr(CSDefine.privateStr, fieldType, keyLower, fieldName);
            classContent += Utils.FormatStr(CSDefine.publicStr, fieldType, keyUpper, keyLower);
        }

        classContent += CSDefine.classEnd;

        // Array mode: create List wrapper class (e.g., MergeUpgradeDataItemSeries : List<MergeUpgradeData>)
        let listClassName = '';
        if (isArrayMode) {
            listClassName = className + key;
            classContent += "\t[System.Serializable]\r\n\tpublic class " + listClassName + " : System.Collections.Generic.List<" + className + ">\r\n\t{\r\n\t}\r\n\r\n";
        }

        // Dictionary class: Dictionary<int, className> or Dictionary<int, ListClassName>
        let dictValueType = isArrayMode ? listClassName : className;
        classContent += Utils.FormatStr(CSDefine.classDictionaryStartWithConfig, className, keyType, dictValueType);

        return classContent;
    }

    private CreateCsRecordOnly(data: any, className: string, nestedFieldDefs?: { [parentField: string]: { name: string; type: string }[] }) {
        if (!data || data.length < 2) {
            console.warn('Invalid data for CS generation:', className);
            return '';
        }

        let dataArr = data;
        let keys = (dataArr[0] || []).slice();
        let typeRowIndex = this.FindTypeRowIndex(dataArr);
        let types = typeRowIndex >= 0 ? (dataArr[typeRowIndex] || []) : [];

        if (keys.length === 0) {
            console.warn('No keys found for CS generation:', className);
            return '';
        }

        let key = keys[0];
        let isArrayMode = false;
        if (typeof key === 'string' && key.startsWith('@')) {
            isArrayMode = true;
            key = key.substring(1);
            keys[0] = key;
        }
        let keyType = this.TransformType(types[0]);

        let classContent = '';

        // Generate nested struct class definitions BEFORE the main class
        if (nestedFieldDefs) {
            for (let fieldName in nestedFieldDefs) {
                let capitalized = fieldName.charAt(0).toUpperCase() + fieldName.slice(1);
                if (this.structHelper.IsStructType(capitalized)) { continue; }
                let structClassName = className + capitalized;
                let fields = this.DeduplicateFields(nestedFieldDefs[fieldName]);
                classContent += this.BuildStructClassContent(structClassName, fields);
            }
        }

        // Record class: plain data class (not a Dictionary)
        classContent += CSDefine.notes.replace('{0}', className);
        classContent += CSDefine.classStart.replace('{0}', className);

        // Collect struct fields for nested struct support.
        let structFields: { [fieldName: string]: { type: string; isArray: boolean } } = {};

        for (let colIndex = 0; colIndex < keys.length; ++colIndex) {
            let key = keys[colIndex];
            let type = types[colIndex];
            if (_.isNil(key) || _.isEmpty(key) || key.startsWith('#')) {
                continue;
            }
            
            let fieldInfo = this.structHelper.AnalyzeField(key, type);
            if (fieldInfo.isStruct && fieldInfo.fieldPath.length > 0) {
                let fieldName = fieldInfo.name;
                let structClassName = fieldInfo.structName || (fieldName.charAt(0).toUpperCase() + fieldName.slice(1));
                if (!this.structHelper.IsStructType(structClassName)) {
                    structClassName = className + structClassName;
                }
                
                if (!structFields[fieldName]) {
                    structFields[fieldName] = {
                        type: structClassName,
                        isArray: fieldInfo.isArray
                    };
                }
                continue;
            }
            
            let keyUpperAndLower = Utils.GetFristUpperAndLowerStr(key);
            let keyUpper = keyUpperAndLower[0];
            let keyLower = keyUpperAndLower[1];
            let fieldType = this.TransformType(type);
            
            classContent += Utils.FormatStr(CSDefine.privateStr, fieldType, keyLower, key);
            classContent += Utils.FormatStr(CSDefine.publicStr, fieldType, keyUpper, keyLower);
        }

        // Add nested struct fields
        for (let fieldName in structFields) {
            let keyUpperAndLower = Utils.GetFristUpperAndLowerStr(fieldName);
            let keyUpper = keyUpperAndLower[0];
            let keyLower = keyUpperAndLower[1];
            let fieldInfo = structFields[fieldName];
            let fieldType = fieldInfo.isArray ? fieldInfo.type + '[]' : fieldInfo.type;
            
            classContent += Utils.FormatStr(CSDefine.privateStr, fieldType, keyLower, fieldName);
            classContent += Utils.FormatStr(CSDefine.publicStr, fieldType, keyUpper, keyLower);
        }

        classContent += CSDefine.classEnd;

        // Array mode: create List wrapper class (e.g., MergeUpgradeDataItemSeries : List<MergeUpgradeData>)
        if (isArrayMode) {
            let listClassName = className + key;
            classContent += "\t[System.Serializable]\r\n\tpublic class " + listClassName + " : System.Collections.Generic.List<" + className + ">\r\n\t{\r\n\t}\r\n\r\n";
        }

        return classContent;
    }

    private CollectNestedFields(data: any): { [parentField: string]: { name: string; type: string }[] } {
        let nestedFields: { [parentField: string]: { name: string; type: string }[] } = {};
        
        if (!data || data.length < 2) {
            return nestedFields;
        }
        
       let keys = data[0] || [];
        let typeRowIndex = this.FindTypeRowIndex(data);
        let types = typeRowIndex >= 0 ? (data[typeRowIndex] || []) : [];
       
       for (let colIndex = 0; colIndex < keys.length; ++colIndex) {
            let key = keys[colIndex];
            let type = types[colIndex];
            if (_.isNil(key) || _.isEmpty(key) || key.startsWith('#')) {
                continue;
            }
            
            let fieldInfo = this.structHelper.AnalyzeField(key, type);
            if (fieldInfo.isStruct && fieldInfo.fieldPath.length > 0) {
                let parentField = fieldInfo.name;
                if (!nestedFields[parentField]) {
                    nestedFields[parentField] = [];
                }
                let actualFieldName = fieldInfo.fieldPath.filter((p: any) => isNaN(parseInt(p.toString()))).join("_");
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
        let content = CSDefine.notes.replace('{0}', structName);
        content += CSDefine.classStart.replace('{0}', structName);
        for (let field of fields) {
            let keyUpperAndLower = Utils.GetFristUpperAndLowerStr(field.name);
            let keyUpper = keyUpperAndLower[0];
            let keyLower = keyUpperAndLower[1];
            let fieldType = this.TransformType(field.type);
            content += Utils.FormatStr(CSDefine.privateStr, fieldType, keyLower, field.name);
            content += Utils.FormatStr(CSDefine.publicStr, fieldType, keyUpper, keyLower);
        }
        content += CSDefine.classEnd;
        return content;
    }

    private async SaveStructClassToFile(structName: string, fields: { name: string; type: string }[]): Promise<void> {
        let classContent = CSDefine.notes.replace('{0}', structName);
        classContent += CSDefine.classStart.replace('{0}', structName);
        
        for (let field of fields) {
            let keyUpperAndLower = Utils.GetFristUpperAndLowerStr(field.name);
            let keyUpper = keyUpperAndLower[0];
            let keyLower = keyUpperAndLower[1];
            let fieldType = this.TransformType(field.type);
            
            classContent += Utils.FormatStr(CSDefine.privateStr, fieldType, keyLower, field.name);
            classContent += Utils.FormatStr(CSDefine.publicStr, fieldType, keyUpper, keyLower);
        }
        
        classContent += CSDefine.classEnd;
        
        await this.SaveCsToFile(classContent, path.join(this.outputPathCsStr, structName));
    }

    private CreateJson(data: any, className: string) {
        let jsonOut: { [key: string]: any } = {};

        if (!data || data.length < 3) {
            console.warn('Invalid data for', className);
            return jsonOut;
        }

        let dataArr = data;
        // Copy keys so stripping the '@' prefix does not mutate the shared sheet data.
        let keys = (dataArr[0] || []).slice();
        let typeRowIndex = this.FindTypeRowIndex(dataArr);
        let types = typeRowIndex >= 0 ? (dataArr[typeRowIndex] || []) : [];

        let layerNum = 1;

        // Array mode: first column key starts with '@', e.g. @ItemSeries.
        // Rows are grouped into arrays keyed by the first column value.
        let isArrayMode = false;
        let arrayKey = '';
        if (keys.length > 0 && keys[0] && keys[0].startsWith('@')) {
            isArrayMode = true;
            arrayKey = keys[0].substring(1);
            keys[0] = arrayKey;
        }

        // Both header layouts (keys/types/desc and keys/desc/types) keep data at row 3.
        let dataStartRow = 3;
        for (let rowIndex = dataStartRow; rowIndex < dataArr.length; ++rowIndex) {
            let _arrLine = dataArr[rowIndex];

            if (_.isNil(_arrLine) || _.isNil(_arrLine[0]) || _arrLine[0] === '') {
                continue;
            }

            let tmp = jsonOut;
            for (let layIndex = 0; layIndex < layerNum - 1; ++layIndex) {
                if (!(_arrLine[layIndex] in tmp)) {
                    tmp[_arrLine[layIndex]] = {};
                }
                tmp = tmp[_arrLine[layIndex]];
            }

            let subTmp: { [key: string]: any } = {};

            for (let colIndex = 0; colIndex < keys.length; ++colIndex) {
                let key = keys[colIndex];
                if (_.isNil(key) || _.isEmpty(key) || key.startsWith('#')) {
                    continue;
                }
                let type = types[colIndex] || 'string';
                let value = _arrLine[colIndex];
                if (_.isNil(value) || typeof (value) == "undefined") {
                    continue;
                }

                let fieldPath = this.structHelper.ResolveFieldPath(key);

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

            if (isArrayMode) {
                //                                                 
                let groupKey = _arrLine[layerNum - 1].toString();
                if (!(groupKey in tmp)) {
                    tmp[groupKey] = [];
                }
                tmp[groupKey].push(subTmp);
            } else {
                tmp[_arrLine[layerNum - 1]] = subTmp;
            }

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



