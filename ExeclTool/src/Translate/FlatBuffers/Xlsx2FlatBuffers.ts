import xlsx from 'node-xlsx';
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
                let data = xlsx.parse(path.join(pathStr, files[i]));
                for (let j = 0; j < data.length; ++j) {
                    this.xlsxData[data[j].name] = data[j].data;
                }
                let fileName = files[i].replace(path.extname(files[i]), '');
                await this.TransferTableJson(fileName);
                if (i == '0') {
                    await this.TransferTableFbs();
                    if(this.toCode == "csharp") { 
                        await this.TransferTableCs();
                    }
                    await this.GenCode(path.join(this.outputPathFbsStr, (this.merge ? this.mergeName : this.translateSheets[0][1]) + '.fbs'), this.toCode, this.outputPathCodeStr);
                }
                await this.GenBin(path.join(this.outputPathFbsStr, (this.merge ? this.mergeName : this.translateSheets[0][1]) + '.fbs'), path.join(this.outputPathJsonStr, (this.merge ? this.mergeName : this.translateSheets[0][1]) + fileName + '.json'), path.join(this.outputPathBinStr, (this.merge ? this.mergeName : this.translateSheets[0][1])));
            }
        } else {
            let parsedPath = path.parse(pathStr);
            parsedPath.base += '.xlsx';
            parsedPath.ext = '.xlsx';
            let data = xlsx.parse(path.format(parsedPath));
            for (let i = 0; i < data.length; ++i) {
                this.xlsxData[data[i].name] = data[i].data;
            }
            await this.TransferTableJson();
            await this.TransferTableFbs();
            if(this.toCode == "csharp") { 
                await this.TransferTableCs();
            }
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
                all[translateName] = jsonData;
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
        let fbsContent = FbsDefine.packageCommonImport + FbsDefine.namespaceStart;


        if (this.merge) {
            for (let i = 0; i < this.translateSheets.length; ++i) {
                let sheetName = this.translateSheets[i][0];
                let translateName = this.translateSheets[i][1];
                fbsContent += this.CreateFbs(this.xlsxData[sheetName], translateName);
            }
            fbsContent += FbsDefine.rootType.replace('{0}', this.mergeName);
        } else {
            for (let i = 0; i < this.translateSheets.length; ++i) {
                let sheetName = this.translateSheets[i][0];
                let translateName = this.translateSheets[i][1];
                fbsContent += this.CreateFbs(this.xlsxData[sheetName], translateName);
                fbsContent += FbsDefine.rootType.replace('{0}', translateName + '_Array');
            }
        }

        fs.writeFileSync(path.join(this.outputPathFbsStr, (this.merge ? this.mergeName : this.translateSheets[0][1]) + '.fbs'), fbsContent, 'utf8');
    }

    private async TransferTableCs(): Promise<void> {
        if (this.merge) {
            let content = '';
            for (let i = 0; i < this.translateSheets.length; ++i) {
                let sheetName = this.translateSheets[i][0];
                let translateName = this.translateSheets[i][1];
                content += this.CreateCs(this.xlsxData[sheetName], translateName, this.mergeName);
            }
            content += Utils.FormatStr(FbsDefine.csNotes, this.mergeName);
            if (!this.isDir) content += Utils.FormatStr(FbsDefine.csConfigHead, this.mergeName);
            content += Utils.FormatStr(FbsDefine.csClassDictionaryStart, this.mergeName, 'int', this.mergeName);
            content += Utils.FormatStr(FbsDefine.csPrivateStr, 'int', 'id');
            content += Utils.FormatStr(FbsDefine.csPublicStr, 'int', 'id');
            content += FbsDefine.csClassEnd;
            this.SaveCsToFile(content, path.join(this.outputPathCsStr, this.mergeName));
        } else {
            for (let i = 0; i < this.translateSheets.length; ++i) {
                let sheetName = this.translateSheets[i][0];
                let translateName = this.translateSheets[i][1];
                let content = Utils.FormatStr(FbsDefine.csNotes, translateName);
                content += Utils.FormatStr(FbsDefine.csConfigHead, translateName);
                content += this.CreateCs(this.xlsxData[sheetName], translateName, '');
                this.SaveCsToFile(content, path.join(this.outputPathCsStr, translateName));
            }
        }
    }

    private CreateJson(data: any, className: string): any {
        let jsonOut: any[] = [];

        if (!data || data.length < 3) {
            console.warn('Invalid data for FlatBuffers', className);
            return jsonOut;
        }

        let keys = data[0] || [];
        let types = data[1] || [];

        for (let rowIndex = 3; rowIndex < data.length; ++rowIndex) {
            let _arrLine = data[rowIndex];
            if (_.isNil(_arrLine) || _.isNil(_arrLine[0]) || _arrLine[0] === '') continue;

            let subTmp: { [key: string]: any } = {};
            for (let colIndex = 0; colIndex < keys.length; ++colIndex) {
                let key = keys[colIndex];
                let type = types[colIndex];
                let value = _arrLine[colIndex];
                if (_.isNil(key) || _.isEmpty(key) || typeof value === 'undefined') continue;

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

        let keys: string[] = data[0] || [];
        let types: string[] = data[1] || [];

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
            if (_.isNil(key) || _.isEmpty(key)) continue;

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
            if (this.structHelper.IsStructType(field.type)) {
                content += FbsDefine.fieldStr.replace('{0}', field.key).replace('{1}', field.type);
            } else {
                content += FbsDefine.fieldStr.replace('{0}', field.key).replace('{1}', fbsType);
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

        // 3. Generate wrapper table for flatc binary generation (array root type)
        content += FbsDefine.tableStart.replace('{0}', className + '_Array');
        content += FbsDefine.fieldStr.replace('{0}', 'records').replace('{1}', '[' + className + ']');
        content += FbsDefine.tableEnd;

        return content;
    }

    private CreateCs(data: any, className: string, mergeName: string): string {
        if (!data || data.length < 2) {
            console.warn('Invalid data for FlatBuffers CS', className);
            return '';
        }

        let keys = data[0] || [];
        let types = data[1] || [];
        let content = '';

        if (mergeName !== className) {
            content += Utils.FormatStr(FbsDefine.csNotes, className);
        }
        content += Utils.FormatStr(FbsDefine.csClassDictionaryStart, className, 'int', className);

        let structFields: { [fieldName: string]: { typeName: string; isArray: boolean } } = {};

        for (let colIndex = 0; colIndex < keys.length; ++colIndex) {
            let key = keys[colIndex];
            let type = types[colIndex];
            if (_.isNil(key) || _.isEmpty(key)) continue;

            let fieldInfo = this.structHelper.AnalyzeField(key, type);
            if (fieldInfo.isStruct && fieldInfo.fieldPath.length > 0) {
                let parentName = fieldInfo.name;
                if (!structFields[parentName]) {
                    let structName = fieldInfo.structName || (parentName.charAt(0).toUpperCase() + parentName.slice(1));
                    let isArray = fieldInfo.isArray;
                    let typeName = this.structHelper.IsStructType(structName)
                        ? 'CfgSpace.' + structName
                        : 'CfgSpace.' + className + '_' + structName;
                    structFields[parentName] = { typeName, isArray };
                }
                continue;
            }

            let csType = this.TransformCsType(type);
            let keyUpperLower = Utils.GetFristUpperAndLowerStr(key);
            let keyUpper = keyUpperLower[0];
            let keyLower = keyUpperLower[1];

            content += Utils.FormatStr(FbsDefine.csPrivateStr, csType, keyLower);
            content += Utils.FormatStr(FbsDefine.csPublicStr, csType, keyUpper, keyLower);
        }

        // Add struct field references
        for (let fieldName in structFields) {
            let info = structFields[fieldName];
            let keyUpperLower = Utils.GetFristUpperAndLowerStr(fieldName);
            let keyUpper = keyUpperLower[0];
            let keyLower = keyUpperLower[1];
            let fieldType = info.isArray ? info.typeName + "[]" : info.typeName;

            content += Utils.FormatStr(FbsDefine.csPrivateStr, fieldType, keyLower);
            content += Utils.FormatStr(FbsDefine.csPublicStr, fieldType, keyUpper, keyLower);
        }

        content += FbsDefine.csClassEnd;
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

