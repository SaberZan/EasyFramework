import xlsx from 'node-xlsx';
import path from 'path';
import fs from 'fs';
import { mkdir, readdir, writeFile } from 'fs/promises';
import _ from 'lodash';
import { exec } from 'child_process';
import Utils from '../../utils';
import BaseTranslateConfig from '../BaseTranslateConfig';
import BaseTranslateStruct from '../BaseTranslateStruct';

export default class Xlsx2FlatBuffers extends BaseTranslateConfig {

    private structHelper: BaseTranslateStruct = new BaseTranslateStruct();

    private outputPathFbsStr: string = '';
    private outputPathBinStr: string = '';
    private outputPathCodeStr: string = '';
    private outputPathJsonStr: string = '';
    private outputPathCsStr: string = '';
    private toCode: string = '';

    private namespaceStart = 'namespace CfgSpace; \n\n';
    private packageCommonImport = 'include \"Common.fbs\"; \n\n';

    private tableStart = 'table {0} {\n';
    private tableEnd = '}\n\n';
    private structStart = 'struct {0} {\n';
    private structEnd = '}\n\n';
    private fieldStr = '    {0} : {1};\n';
    private rootType = 'root_type {0};\n\n';

    private csNotes = '/**\n * {0}\n */\n';
    private csConfigHead = '    [Easy.Config(\"{0}\")]\n';
    private csClassDictionaryStart = 'public static class {0} : System.Collections.Generic.Dictionary<{1}, {2}>\n{\n';
    private csPrivateStr = '        private {0} _{1};\n';
    private csPublicStr = '        public static {0} {1} => _{1};\n\n';
    private csClassEnd = '}\n\n';

    public async TranslateExcel(pathStr: string, outputPathStr: string, translate: any, params: any): Promise<void> {
        await super.TranslateExcel(pathStr, outputPathStr, translate, params);

        let structPath = this.definePath || path.join(pathStr, '..', 'define');
        await this.structHelper.ParseStructDefinitions(structPath);

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
                    await this.TransferTableCs();
                    await this.GenCode(path.join(this.outputPathFbsStr, this.mergeName + '.fbs'), this.toCode, this.outputPathCodeStr);
                }
                await this.GenBin(path.join(this.outputPathFbsStr, this.mergeName + '.fbs'), path.join(this.outputPathJsonStr, this.mergeName + fileName + '.json'), path.join(this.outputPathBinStr, this.mergeName));
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
            await this.TransferTableCs();
            for (let i = 0; i < this.translateSheets.length; ++i) {
                let sheetName = this.translateSheets[i][0];
                let translateName = this.translateSheets[i][1];
                await this.GenBin(path.join(this.outputPathFbsStr, translateName + '.fbs'), path.join(this.outputPathJsonStr, translateName + '.json'), path.join(this.outputPathBinStr, translateName));
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
                await this.SaveJsonToFile(jsonData, path.join(this.outputPathJsonStr, translateName));
            }
        }
    }

    private async TransferTableFbs(): Promise<void> {
        let fbsContent = this.namespaceStart + this.packageCommonImport;

        for (let structName in this.structHelper.structDefinitions) {
            fbsContent += this.CreateFbsStruct(this.structHelper.structDefinitions[structName]);
        }

        if (this.merge) {
            for (let i = 0; i < this.translateSheets.length; ++i) {
                let sheetName = this.translateSheets[i][0];
                let translateName = this.translateSheets[i][1];
                fbsContent += this.CreateFbs(this.xlsxData[sheetName], translateName);
            }
            fbsContent += this.rootType.replace('{0}', this.mergeName);
        } else {
            for (let i = 0; i < this.translateSheets.length; ++i) {
                let sheetName = this.translateSheets[i][0];
                let translateName = this.translateSheets[i][1];
                fbsContent += this.CreateFbs(this.xlsxData[sheetName], translateName);
                fbsContent += this.rootType.replace('{0}', translateName);
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
            content += Utils.FormatStr(this.csNotes, this.mergeName);
            if (!this.isDir) content += Utils.FormatStr(this.csConfigHead, this.mergeName);
            content += Utils.FormatStr(this.csClassDictionaryStart, this.mergeName, 'int', this.mergeName);
            content += Utils.FormatStr(this.csPrivateStr, 'int', 'id');
            content += Utils.FormatStr(this.csPublicStr, 'int', 'id');
            content += this.csClassEnd;
            this.SaveCsToFile(content, path.join(this.outputPathCsStr, this.mergeName));
        } else {
            for (let i = 0; i < this.translateSheets.length; ++i) {
                let sheetName = this.translateSheets[i][0];
                let translateName = this.translateSheets[i][1];
                let content = Utils.FormatStr(this.csNotes, translateName);
                content += Utils.FormatStr(this.csConfigHead, translateName);
                content += this.CreateCs(this.xlsxData[sheetName], translateName, '');
                this.SaveCsToFile(content, path.join(this.outputPathCsStr, translateName));
            }
        }
    }

    private CreateJson(data: any, className: string): any {
        let jsonOut: { [key: string]: any } = {};

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

                let fieldPath = this.structHelper.ParseFieldPath(key);
                if (fieldPath.length > 1) {
                    this.structHelper.SetNestedValue(subTmp, fieldPath, this.TransformStructValue(type, value));
                } else {
                    let result = this.TransformStructValue(type, value, rowIndex, colIndex);
                    if (!_.isNil(result) && !_.isNaN(result)) {
                        let keyLower = Utils.GetFristUpperAndLowerStr(key)[1];
                        subTmp[keyLower] = result;
                    }
                }
            }

            jsonOut[_arrLine[0]] = subTmp;
        }

        return jsonOut;
    }

    private CreateFbsStruct(structDef: any): string {
        let content = this.structStart.replace('{0}', structDef.name);
        for (let field of structDef.fields) {
            content += this.fieldStr.replace('{0}', field.name).replace('{1}', this.TransformType(field.type));
        }
        content += this.structEnd;
        return content;
    }

    private CreateFbs(data: any, className: string): string {
        if (!data || data.length < 2) {
            console.warn('Invalid data for FlatBuffers FBS', className);
            return '';
        }

        let keys = data[0] || [];
        let types = data[1] || [];
        let content = this.tableStart.replace('{0}', className);

        for (let colIndex = 0; colIndex < keys.length; ++colIndex) {
            let key = keys[colIndex];
            let type = types[colIndex];
            if (_.isNil(key) || _.isEmpty(key)) continue;

            let fieldType = this.TransformType(type);
            if (this.structHelper.IsStructType(type)) {
                content += this.fieldStr.replace('{0}', key).replace('{1}', type);
            } else {
                content += this.fieldStr.replace('{0}', key).replace('{1}', fieldType);
            }
        }

        content += this.tableEnd;
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
            content += Utils.FormatStr(this.csNotes, className);
        }
        content += Utils.FormatStr(this.csClassDictionaryStart, className, 'int', className);

        for (let colIndex = 0; colIndex < keys.length; ++colIndex) {
            let key = keys[colIndex];
            let type = types[colIndex];
            if (_.isNil(key) || _.isEmpty(key)) continue;

            let csType = this.TransformCsType(type);
            let keyUpperLower = Utils.GetFristUpperAndLowerStr(key);
            let keyUpper = keyUpperLower[0];
            let keyLower = keyUpperLower[1];

            content += Utils.FormatStr(this.csPrivateStr, csType, keyLower);
            content += Utils.FormatStr(this.csPublicStr, csType, keyUpper, keyLower);
        }

        content += this.csClassEnd;
        return content;
    }

    private TransformType(type: string): string {
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
                if (this.structHelper.IsStructType(type)) {
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

    private TransformCsType(type: string): string {
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
                if (this.structHelper.IsStructType(type)) {
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
        let cmd = 'flatc --' + toCode + ' -o ' + outputPath + ' ' + fbsPath;
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
        let cmd = 'flatc -b -o ' + path.dirname(binPath) + ' -I ' + path.dirname(fbsPath) + ' ' + fbsPath + ' ' + jsonPath;
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
