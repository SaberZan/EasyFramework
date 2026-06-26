import xlsx from 'node-xlsx';
import DataParser from '../../DataParser';
import path from 'path';
import fs from 'fs';
import { mkdir, readdir, writeFile } from 'fs/promises';
import _ from 'lodash';
import { exec } from 'child_process';
import Utils from '../../utils';
import FbsDefine from './FbsDefine';
import BaseTranslateConfig from '../BaseTranslateConfig';
import BaseTranslateStruct from '../BaseTranslateStruct';

export default class Xlsx2FlatBuffers extends BaseTranslateConfig {

    private outputPathFbsStr: string = '';
    private outputPathBinStr: string = '';
    private outputPathCodeStr: string = '';
    private outputPathJsonStr: string = '';
    private outputPathCsStr: string = '';
    private toCode: string = '';


    public async TranslateExcel(pathStr: string, outputPathStr: string, translate: any, params: any): Promise<void> {
        await super.TranslateExcel(pathStr, outputPathStr, translate, params);

        this.outputPathFbsStr = path.join(outputPathStr, 'fbs');
        this.outputPathBinStr = path.join(outputPathStr, 'bin');
        this.outputPathCodeStr = path.join(outputPathStr, 'code', params.toCode);
        this.outputPathJsonStr = path.join(outputPathStr, 'json');
        this.outputPathCsStr = path.join(outputPathStr, 'code', 'cs');
        this.toCode = params.toCode;

        if (!fs.existsSync(this.outputPathFbsStr)) await mkdir(this.outputPathFbsStr, { recursive: true });
        if (!fs.existsSync(this.outputPathBinStr)) await mkdir(this.outputPathBinStr, { recursive: true });
        if (!fs.existsSync(this.outputPathCodeStr)) await mkdir(this.outputPathCodeStr, { recursive: true });
        if (!fs.existsSync(this.outputPathJsonStr)) await mkdir(this.outputPathJsonStr, { recursive: true });
        if (!fs.existsSync(this.outputPathCsStr)) await mkdir(this.outputPathCsStr, { recursive: true });

        if (this.toDir) {
            this.outputPathJsonStr = path.join(this.outputPathJsonStr, this.toDir);
            if (!fs.existsSync(this.outputPathJsonStr)) await mkdir(this.outputPathJsonStr, { recursive: true });
        }

        if (this.isDir) {
            let files = await readdir(pathStr);
            for (let i in files) {
                let data = DataParser.parse(path.join(pathStr, files[i]), params.format);
                for (let j = 0; j < data.length; ++j) {
                    this.xlsxData[data[j].name] = data[j].data;
                }
            }
            for (let i in files) {
                let fileName = files[i].replace(path.extname(files[i]), '');
                await this.TransferTableJson(fileName);
            }
            await this.TransferTableFbs();
            await this.GenCode(path.join(this.outputPathFbsStr, (this.merge ? this.mergeName : this.translateSheets[0][1]) + '.fbs'), this.toCode, this.outputPathCodeStr);
            for (let i in files) {
                let fileName = files[i].replace(path.extname(files[i]), '');
                await this.GenBin(path.join(this.outputPathFbsStr, (this.merge ? this.mergeName : this.translateSheets[0][1]) + '.fbs'), path.join(this.outputPathJsonStr, (this.merge ? this.mergeName : this.translateSheets[0][1]) + fileName + '.json'), path.join(this.outputPathBinStr, (this.merge ? this.mergeName : this.translateSheets[0][1])));
            }
        } else {
            // First try the path as-is (handles CSV directories without .xlsx extension)
            let data = DataParser.parse(pathStr, params.format);
            // If no data, try with .xlsx extension (legacy xlsx files)
            if (data.length === 0) {
                let parsedPath = path.parse(pathStr);
                parsedPath.base += '.xlsx';
                parsedPath.ext = '.xlsx';
                data = DataParser.parse(path.format(parsedPath), params.format);
            }
            for (let i = 0; i < data.length; ++i) {
                this.xlsxData[data[i].name] = data[i].data;
            }
            await this.TransferTableJson();
            await this.TransferTableFbs();
            for (let i = 0; i < this.translateSheets.length; ++i) {
                let sheetName = this.translateSheets[i][0];
                let translateName = this.translateSheets[i][1];
                await this.GenBin(path.join(this.outputPathFbsStr, translateName + '.fbs'), path.join(this.outputPathJsonStr, translateName + '.json'), path.join(this.outputPathBinStr, translateName));
                await this.GenCode(path.join(this.outputPathFbsStr, translateName + '.fbs'), this.toCode, this.outputPathCodeStr);
            }
        }
    }

    private async TransferTableJson(file: string = ''): Promise<void> {
        if (this.merge) {
            let all: { [key: string]: any } = {};
            for (let i = 0; i < this.translateSheets.length; ++i) {
                let sheetName = this.translateSheets[i][0];
                let translateName = this.translateSheets[i][1];
                let jsonData = this.CreateJson(this.xlsxData[sheetName], translateName);
                // Use lowercase field name to match FBS field names
                let fieldName = translateName.charAt(0).toLowerCase() + translateName.slice(1);
                // Wrap in records to match _Array table structure
                all[fieldName] = { records: jsonData };
            }
            await this.SaveJsonToFile(all, path.join(this.outputPathJsonStr, this.mergeName + file));
        } else {
            for (let i = 0; i < this.translateSheets.length; ++i) {
                let sheetName = this.translateSheets[i][0];
                let translateName = this.translateSheets[i][1];
                let jsonData = this.CreateJson(this.xlsxData[sheetName], translateName);
                let wrapper: any = { records: jsonData };
                await this.SaveJsonToFile(wrapper, path.join(this.outputPathJsonStr, translateName));
            }
        }
    }

    private async TransferTableFbs(): Promise<void> {
        if (this.merge) {
            let fbsContent = FbsDefine.packageCommonImport + FbsDefine.namespaceStart;
            for (let i = 0; i < this.translateSheets.length; ++i) {
                let sheetName = this.translateSheets[i][0];
                let translateName = this.translateSheets[i][1];
                fbsContent += this.CreateFbs(this.xlsxData[sheetName], translateName);
            }
            // Generate merge wrapper table
            fbsContent += FbsDefine.tableStart.replace('{0}', this.mergeName);
            for (let i = 0; i < this.translateSheets.length; ++i) {
                let sheetName = this.translateSheets[i][0];
                let translateName = this.translateSheets[i][1];
                let data = this.xlsxData[sheetName];
                let rootType = translateName + '_Array';
                if (data && data.length > 0 && data[0] && data[0].length > 0 && typeof data[0][0] === 'string' && data[0][0].startsWith('@')) {
                    let atFieldName = data[0][0].substring(1);
                    let subTableName = this.getSubTableName(translateName, atFieldName);
                    rootType = subTableName + '_Array';
                }
                // Use lowercase field name to avoid conflict with table names
                let fieldName = translateName.charAt(0).toLowerCase() + translateName.slice(1);
                fbsContent += FbsDefine.fieldStr.replace('{0}', fieldName).replace('{1}', rootType);
            }
            fbsContent += FbsDefine.tableEnd;
            fbsContent += FbsDefine.rootType.replace('{0}', this.mergeName);
            fs.writeFileSync(path.join(this.outputPathFbsStr, this.mergeName + '.fbs'), fbsContent, 'utf8');
        } else {
            for (let i = 0; i < this.translateSheets.length; ++i) {
                let sheetName = this.translateSheets[i][0];
                let translateName = this.translateSheets[i][1];
                let fbsContent = FbsDefine.packageCommonImport + FbsDefine.namespaceStart;
                fbsContent += this.CreateFbs(this.xlsxData[sheetName], translateName);
                let data = this.xlsxData[sheetName];
                let rootType = translateName + '_Array';
                if (data && data.length > 0 && data[0] && data[0].length > 0 && typeof data[0][0] === 'string' && data[0][0].startsWith('@')) {
                    let atFieldName = data[0][0].substring(1);
                    let subTableName = this.getSubTableName(translateName, atFieldName);
                    rootType = subTableName + '_Array';
                }
                fbsContent += FbsDefine.rootType.replace('{0}', rootType);
                fs.writeFileSync(path.join(this.outputPathFbsStr, translateName + '.fbs'), fbsContent, 'utf8');
            }
        }
    }

    private CreateJson(data: any, className: string): any {
        let jsonOut: any[] = [];

        if (!data || data.length < 3) {
            console.warn('Invalid data for FlatBuffers', className);
            return jsonOut;
        }

        let dataArr = data;
        let keys = (dataArr[0] || []).slice();
        let typeRowIndex = this.FindTypeRowIndex(dataArr);
        let types = typeRowIndex >= 0 ? (dataArr[typeRowIndex] || []) : [];

        // Check if first key has @ prefix for array mode grouping
        let isArrayMode = false;
        let atFieldLower = '';
        let firstKey = keys.length > 0 ? keys[0] : '';
        if (typeof firstKey === 'string' && firstKey.startsWith('@')) {
            isArrayMode = true;
            atFieldLower = Utils.GetFristUpperAndLowerStr(firstKey.substring(1))[1];
        }

        for (let rowIndex = 3; rowIndex < data.length; ++rowIndex) {
            let _arrLine = data[rowIndex];
            if (_.isNil(_arrLine) || _.isNil(_arrLine[0]) || _arrLine[0] === '') continue;

            let subTmp: { [key: string]: any } = {};
            for (let colIndex = 0; colIndex < keys.length; ++colIndex) {
                let key = keys[colIndex];
                let type = types[colIndex];
                let value = _arrLine[colIndex];
                // Strip @ prefix for flatc compatibility
                if (typeof key === 'string' && key.startsWith('@')) {
                    key = key.substring(1);
                }
                if (_.isNil(key) || _.isEmpty(key) || key.startsWith('#') || typeof value === 'undefined') continue;

                let fieldPath = this.structHelper.ResolveFieldPath(key);
                if (fieldPath.length > 1) {
                    // Lowercase first element if it matches a known struct type (flatc naming compatibility)
                    let firstField = fieldPath[0].toString();
                    let capitalized = firstField.charAt(0).toUpperCase() + firstField.slice(1);
                    if (this.structHelper.IsStructType(capitalized)) {
                        fieldPath[0] = firstField.charAt(0).toLowerCase() + firstField.slice(1);
                    }
                    this.structHelper.SetNestedValue(subTmp, fieldPath, this.TransformStructValue(type, value));
                } else {
                    let result = this.TransformStructValue(type, value, rowIndex, colIndex);
                    if (!_.isNil(result) && !_.isNaN(result)) {
                        let keyLower = Utils.GetFristUpperAndLowerStr(key)[1];
                        subTmp[keyLower] = result;
                    }
                }
            }

            jsonOut.push(subTmp);
        }

        // If @ mode, group rows by the first field value
        if (isArrayMode) {
            let groups: { [key: string]: any[] } = {};
            for (let item of jsonOut) {
                let groupVal = item[atFieldLower];
                let groupStr = String(groupVal);
                if (!groups[groupStr]) {
                    groups[groupStr] = [];
                }
                groups[groupStr].push(item);
            }
            let result: any[] = [];
            let keysOrder = Object.keys(groups);
            let allNumbers = keysOrder.every(k => !isNaN(Number(k)));
            if (allNumbers) {
                keysOrder.sort((a, b) => Number(a) - Number(b));
            } else {
                keysOrder.sort();
            }
            for (let gk of keysOrder) {
                let keyVal = allNumbers ? Number(gk) : gk;
                result.push({ key: keyVal, records: groups[gk] });
            }
            return result;
        }

        return jsonOut;
    }

    private CreateFbsStruct(structDef: any): string {
        let content = FbsDefine.structStart.replace('{0}', structDef.name);
        for (let field of structDef.fields) {
            content += FbsDefine.fieldStr.replace('{0}', field.name).replace('{1}', this.TransformType(field.type));
        }
        content += FbsDefine.structEnd;
        return content;
    }

    /**
     * Generate a sub-table name for a nested field within a parent table.
     * e.g. (EquipmentCfg, attrs) -> EquipmentCfg_Attrs
     */
    private getSubTableName(className: string, fieldName: string): string {
        let parts = Utils.GetFristUpperAndLowerStr(fieldName);
        return className + '_' + parts[0];
    }

    private CreateFbs(data: any, className: string): string {
        if (!data || data.length < 2) {
            console.warn("Invalid data for FlatBuffers FBS", className);
            return '';
        }

        let keys: string[] = (data[0] || []).slice();
        let typeRowIndex = this.FindTypeRowIndex(data);
        let types: string[] = typeRowIndex >= 0 ? (data[typeRowIndex] || []) : [];
        
        // Check @ mode before stripping
        let isArrayMode = false;
        let atFieldName = '';
        if (keys.length > 0 && keys[0] && keys[0].startsWith("@")) {
            isArrayMode = true;
            atFieldName = keys[0].substring(1);
        }
        
        // Strip @ prefix from all keys for flatc compatibility
        for (let i = 0; i < keys.length; ++i) {
            if (keys[i] && keys[i].startsWith('@')) {
                keys[i] = keys[i].substring(1);
            }
        }

        // Group nested fields by their top-level parent name.
        // e.g. 'attrs[0].value1' -> parentName='attrs', subField='value1'
        //      'attr.name'       -> parentName='attr',  subField='name'
       interface NestedGroup {
           fields: { [fieldName: string]: string };
           isArray: boolean;
            structName: string;
        }
        let nestedGroups: { [parentName: string]: NestedGroup } = {};
        interface SimpleField {
            key: string;
            type: string;
        }
        let simpleFields: SimpleField[] = [];

        for (let colIndex = 0; colIndex < keys.length; ++colIndex) {
            let key = keys[colIndex];
            let type = types[colIndex];
            if (_.isNil(key) || _.isEmpty(key) || key.startsWith('#')) continue;

            let fieldInfo = this.structHelper.AnalyzeField(key, type);

            if (fieldInfo.isStruct && fieldInfo.fieldPath.length > 0) {
                let parentName = fieldInfo.name;

               if (!nestedGroups[parentName]) {
                    let resolvedStructName = fieldInfo.structName || (parentName.charAt(0).toUpperCase() + parentName.slice(1));
                    nestedGroups[parentName] = { fields: {}, isArray: fieldInfo.isArray, structName: resolvedStructName };
                }

                if (fieldInfo.isArray) {
                    nestedGroups[parentName].isArray = true;
                }

                let subFieldName = fieldInfo.fieldPath[fieldInfo.fieldPath.length - 1];

                if (!nestedGroups[parentName].fields[subFieldName]) {
                    nestedGroups[parentName].fields[subFieldName] = type;
                }

                continue;
            }

            simpleFields.push({ key, type });
        }

        let content = '';

       // 1. Generate sub-table definitions for each nested group (skip if matching Struct.xlsx type)
       for (let parentName of Object.keys(nestedGroups)) {
           let group = nestedGroups[parentName];
            let structName = group.structName;
            if (this.structHelper.IsStructType(structName)) {
                continue; // Skip - type is defined in Struct.xlsx
            }
            let subTableName = this.getSubTableName(className, structName);

            content += FbsDefine.tableStart.replace('{0}', subTableName);
            for (let [fieldName, fieldType] of Object.entries(group.fields)) {
                let fbsType = this.TransformType(fieldType);
                content += FbsDefine.fieldStr.replace('{0}', fieldName).replace('{1}', fbsType);
            }
            content += FbsDefine.tableEnd;
        }

        // 2. Generate the main table
        content += FbsDefine.tableStart.replace('{0}', className);

        for (let field of simpleFields) {
            let fbsType = this.TransformType(field.type);
            // Use lowercase-first letter to match CreateJson output (flatc is case-sensitive)
            let fieldNameLower = Utils.GetFristUpperAndLowerStr(field.key)[1];
            if (this.structHelper.IsStructType(field.type)) {
                content += FbsDefine.fieldStr.replace('{0}', fieldNameLower).replace('{1}', field.type);
            } else {
                content += FbsDefine.fieldStr.replace('{0}', fieldNameLower).replace('{1}', fbsType);
            }
        }

       // 3. Add nested field references with proper FlatBuffers types
       for (let parentName of Object.keys(nestedGroups)) {
           let group = nestedGroups[parentName];
            let structName = group.structName;
            
            if (this.structHelper.IsStructType(structName)) {
                // Reference the struct type from Struct.xlsx directly (lowercase field name to avoid flatc naming conflict)
                let fieldKey = parentName.charAt(0).toLowerCase() + parentName.slice(1);
                if (group.isArray) {
                    content += FbsDefine.fieldStr.replace('{0}', fieldKey).replace('{1}', '[' + structName + ']');
                } else {
                    content += FbsDefine.fieldStr.replace('{0}', fieldKey).replace('{1}', structName);
                }
            } else {
                let subTableName = this.getSubTableName(className, structName);
                if (group.isArray) {
                    content += FbsDefine.fieldStr.replace('{0}', parentName).replace('{1}', '[' + subTableName + ']');
                } else {
                    content += FbsDefine.fieldStr.replace('{0}', parentName).replace('{1}', subTableName);
                }
            }
        }

        content += FbsDefine.tableEnd;

        // Generate wrapper table(s) based on @ mode
        if (isArrayMode) {
            let subTableName = this.getSubTableName(className, atFieldName);
            let keyFbsType = types[0] ? this.TransformType(types[0]) : 'int';
            // Generate group sub-table: ClassName_AtFieldName { key:type; records:[ClassName]; }
            content += FbsDefine.tableStart.replace('{0}', subTableName);
            content += FbsDefine.fieldStr.replace('{0}', 'key').replace('{1}', keyFbsType);
            content += FbsDefine.fieldStr.replace('{0}', 'records').replace('{1}', '[' + className + ']');
            content += FbsDefine.tableEnd;
            
            // Generate wrapper for the sub-table array
            content += FbsDefine.tableStart.replace('{0}', subTableName + '_Array');
            content += FbsDefine.fieldStr.replace('{0}', 'records').replace('{1}', '[' + subTableName + ']');
            content += FbsDefine.tableEnd;
        } else {
            // Generate wrapper table for flatc binary generation (array root type)
            content += FbsDefine.tableStart.replace('{0}', className + '_Array');
            content += FbsDefine.fieldStr.replace('{0}', 'records').replace('{1}', '[' + className + ']');
            content += FbsDefine.tableEnd;
        }

        return content;
    }

    private TransformType(type: any): string {
        if (typeof type !== 'string') return '';
        let result;
        switch (type) {
            case 'int': case 'Int': result = 'int'; break;
            case 'int[]': case 'Int[]': result = '[int]'; break;
            case 'int[,]': case 'Int[,]': result = '[IntArray]'; break;
            case 'float': case 'Float': result = 'float'; break;
            case 'float[]': case 'Float[]': result = '[float]'; break;
            case 'float[,]': case 'Float[,]': result = '[FloatArray]'; break;
            case 'bool': case 'Bool': case 'boolen': case 'Boolen': result = 'bool'; break;
            case 'bool[]': case 'Bool[]': case 'boolen[]': case 'Boolen[]': result = '[bool]'; break;
            case 'bool[,]': case 'Bool[,]': case 'boolen[,]': case 'Boolen[,]': result = '[BoolArray]'; break;
            case 'string': case 'String': result = 'string'; break;
            case 'string[]': case 'String[]': result = '[string]'; break;
            case 'string[,]': case 'String[,]': result = '[StringArray]'; break;
            default:
                if (this.enumHelper.IsEnumType(type)) {
                    result = type;
                } else if (this.structHelper.IsStructType(type)) {
                    result = type;
                } else if (type.includes('[]')) {
                    result = '[' + this.TransformType(type.replace('[]', '')) + ']';
                } else {
                    result = type;
                }
                break;
        }
        return result;
    }

    private TransformCsType(type: any): string {
        if (typeof type !== 'string') return '';
        let result;
        switch (type) {
            case 'int': case 'Int': result = 'int'; break;
            case 'int[]': case 'Int[]': result = 'int[]'; break;
            case 'int[,]': case 'Int[,]': result = 'CfgSpace.IntArray[]'; break;
            case 'float': case 'Float': result = 'float'; break;
            case 'float[]': case 'Float[]': result = 'float[]'; break;
            case 'float[,]': case 'Float[,]': result = 'CfgSpace.FloatArray[]'; break;
            case 'bool': case 'Bool': case 'boolen': case 'Boolen': result = 'bool'; break;
            case 'bool[]': case 'Bool[]': case 'boolen[]': case 'Boolen[]': result = 'bool[]'; break;
            case 'bool[,]': case 'Bool[,]': case 'boolen[,]': case 'Boolen[,]': result = 'CfgSpace.BoolArray[]'; break;
            case 'string': case 'String': result = 'string'; break;
            case 'string[]': case 'String[]': result = 'string[]'; break;
            case 'string[,]': case 'String[,]': result = 'CfgSpace.StringArray[]'; break;
            default:
                if (this.enumHelper.IsEnumType(type)) {
                    result = 'CfgSpace.' + type;
                } else if (this.structHelper.IsStructType(type)) {
                    result = 'CfgSpace.' + type;
                } else if (type.includes('[]')) {
                    result = this.TransformCsType(type.replace('[]', '')) + '[]';
                } else {
                    result = type;
                }
                break;
        }
        return result;
    }

    private SaveJsonToFile(data: any, filePath: string): void {
        fs.writeFileSync(filePath + '.json', JSON.stringify(data, null, 4), 'utf8');
    }

    private SaveCsToFile(data: string, filePath: string): void {
        fs.writeFileSync(filePath + '.cs', data, 'utf8');
    }

    private _TransformBasicsValue(type: string, data: any): any {
        let result;
        switch (type) {
            case 'int': case 'Int': result = parseInt(data); break;
            case 'float': case 'Float': result = parseFloat(data); break;
            case 'bool': case 'Bool': case 'boolen': case 'Boolen': result = Boolean(data); break;
            case 'string': case 'String': result = data; if (result === '') result = null; break;
            case 'json': case 'Json': result = JSON.parse(data); break;
            default: result = data; if (result === '') result = null; break;
        }
        return result;
    }

    private TransformStructValue(type: string, data: string, row?: number, col?: number): any {
        if (this.structHelper.IsStructType(type)) {
            return this.structHelper.TransformStructValue(type, data);
        }

        // Ensure data is a string before calling string methods
        if (typeof data !== 'string') {
            data = String(data);
        }

        let result;
        if (typeof data === 'string') {
            data = data.replace(/[\r\n]/g, '');
        }

        if (type.includes('[,]')) {
            type = type.replace('[,]', '');
            result = [];
            let datas = data.substring(2, data.length - 2).split('],[');
            for (let i = 0; i < datas.length; ++i) {
                let tmpResult = [];
                let _datas = datas[i].split(',');
                for (let j = 0; j < _datas.length; ++j) {
                    tmpResult.push(this._TransformBasicsValue(type, _datas[j]));
                }
                result.push({ data: tmpResult });
            }
        } else if (type.includes('[]')) {
            type = type.replace('[]', '');
            result = [];
            let _datas = data.substring(1, data.length - 1).split(',');
            for (let i = 0; i < _datas.length; ++i) {
                result.push(this._TransformBasicsValue(type, _datas[i]));
            }
        } else {
            result = this._TransformBasicsValue(type, data);
        }
        return result;
    }

    private async GenCode(fbsPath: string, toCode: string, outputPath: string): Promise<void> {
        let cmd = '.\\lib\\flatc\\flatc.exe --' + toCode + ' -o ' + outputPath + ' ' + fbsPath;
        console.log(cmd);
        return new Promise<void>((resolve, reject) => {
            exec(cmd, (err, stdout, stderr) => {
                if (err) {
                    reject(err);
                    return;
                }
                console.log(stdout);
                console.log(stderr);
                resolve();
            });
        });
    }

    private async GenBin(fbsPath: string, jsonPath: string, binPath: string): Promise<void> {
        let cmd = '.\\lib\\flatc\\flatc.exe -b --json -o ' + path.dirname(binPath) + ' -I ' + path.dirname(fbsPath) + ' ' + fbsPath + ' ' + jsonPath;
        console.log(cmd);
        return new Promise<void>((resolve, reject) => {
            exec(cmd, (err, stdout, stderr) => {
                if (err) {
                    reject(err);
                    return;
                }
                console.log(stdout);
                console.log(stderr);
                resolve();
            });
        });
    }
}






