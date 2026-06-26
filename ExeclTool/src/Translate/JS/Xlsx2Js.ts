import xlsx from 'node-xlsx';
import DataParser from '../../DataParser';
import path from 'path';
import fs from 'fs';
import { mkdir, readdir, writeFile } from 'fs/promises';
import _ from 'lodash';
import Utils from '../../utils';
import BaseTranslateConfig from '../BaseTranslateConfig';
import BaseTranslateStruct from '../BaseTranslateStruct';

export default class Xlsx2Js extends BaseTranslateConfig {


    private outputJsPathStr: string = '';

    public async TranslateExcel(pathStr: string, outputPathStr: string, translate: any, params: any): Promise<void> {

        await super.TranslateExcel(pathStr, outputPathStr, translate, params);

        this.outputJsPathStr = path.join(outputPathStr, 'js');
        if (!fs.existsSync(this.outputJsPathStr)) {
            await mkdir(this.outputJsPathStr, { recursive: true });
        }

        if (this.toDir != undefined) {
            this.outputJsPathStr = path.join(this.outputJsPathStr, this.toDir);
            if (!fs.existsSync(this.outputJsPathStr)) {
                await mkdir(this.outputJsPathStr, { recursive: true });
            }
        }

        if (this.isDir) {
            let files = await readdir(pathStr);
            for (let i in files) {
                let data = DataParser.parse(path.join(pathStr, files[i]), params.format);
                for (let i = 0; i < data.length; ++i) {
                    this.xlsxData[data[i].name] = data[i].data;
                }
                await this.TransferTable(files[i].replace(path.extname(files[i]), ''));
            }
        } else {
            let parsedPath = path.parse(pathStr);
            parsedPath.base += '.xlsx';
            parsedPath.ext = '.xlsx';
            let data = DataParser.parse(path.format(parsedPath), params.format);
            for (let i = 0; i < data.length; ++i) {
                this.xlsxData[data[i].name] = data[i].data;
            }
            await this.TransferTable();
        }
    }

    private async TransferTable(file: string = ''): Promise<void> {
        if (this.merge) {
            let all: { [key: string]: any } = {};
            for (let i = 0; i < this.translateSheets.length; ++i) {
                let sheetName = this.translateSheets[i][0];
                let translateName = this.translateSheets[i][1];
                let jsonData = this.CreateJson(this.xlsxData[sheetName], translateName);
                all[translateName] = jsonData;
            }
            await this.saveTsToFile(all, path.join(this.outputJsPathStr, this.mergeName + file));
        } else {
            for (let i = 0; i < this.translateSheets.length; ++i) {
                let sheetName = this.translateSheets[i][0];
                let translateName = this.translateSheets[i][1];
                let jsonData = this.CreateJson(this.xlsxData[sheetName], translateName);
                await this.saveTsToFile(jsonData, path.join(this.outputJsPathStr, translateName));
            }
        }
    }

    private CreateJson(data: any, className: string) {
        let jsonOut: { [key: string]: any } = {};

        if (!data || data.length < 3) {
            console.warn('Invalid data for', className);
            return jsonOut;
        }

        let dataArr = data;
        let keys = dataArr[0] || [];
        let typeRowIndex = this.FindTypeRowIndex(dataArr);
        let types = typeRowIndex >= 0 ? (dataArr[typeRowIndex] || []) : [];

        // 默认单层级，如果第一行数据包含嵌套字段（带点的字段名），会自动处理
        let layerNum = 1;

        // 检查是否是数组模式（第一列以@开头）
        let isArrayMode = false;
        let arrayKey = '';
        if (keys.length > 0 && keys[0] && keys[0].startsWith('@')) {
            isArrayMode = true;
            arrayKey = keys[0].substring(1);
            keys[0] = arrayKey; // 去掉@前缀
        }

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
                if (_.isNil(key) || _.isEmpty(key) || key.startsWith('#')) {
                    continue;
                }
                let type = types[colIndex] || 'string';
                let value = _arrLine[colIndex];
                if (_.isNil(value) || typeof (value) == 'undefined') {
                    continue;
                }

                let fieldPath = this.structHelper.ResolveFieldPath(key);

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

            if (isArrayMode) {
                // 数组模式：按第一列的值分组，相同值的行放在数组中
                let groupKey = _arrLine[layerNum - 1].toString();
                if (!tmp[groupKey]) {
                    tmp[groupKey] = [];
                }
                tmp[groupKey].push(subTmp);
            } else {
                tmp[_arrLine[layerNum - 1]] = subTmp;
            }
        }

        return jsonOut;
    }

    private async saveTsToFile(data: any, filePath: string): Promise<void> {
        var _str_all = '';
        _str_all += '//automatic generation,DO NOT EDIT IT!\nmodule.exports =\n';
        _str_all += JSON.stringify(data, null, 4);
        _str_all += ';';
        await writeFile(filePath + '.js', _str_all, { flag: 'w', encoding: 'utf8' });
    }

    private _TransformBasicsValue(type: string, data: any) {
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
                result = data;
                if (result == '') {
                    result = null;
                }
                break;
        }
        return result;
    }

    private TransformStructValue(type: string, data: string, row?: number, col?: number) {
        if (this.structHelper.IsStructType(type)) {
            return this.structHelper.TransformStructValue(type, data);
        }

        let result;
        if (typeof (data) == 'string') {
            data = data.replace(/[\r\n]/g, '');
        }
        if (type.includes('[]')) {
            type = type.replace('[]', '');
            result = [];
            let _datas = data.substring(1, data.length - 1).split(',');
            for (let i = 0; i < _datas.length; ++i) {
                result.push(this._TransformBasicsValue(type, _datas[i]));
            }
        } else if (type.includes('[,]')) {
            // 二维数组
            type = type.replace('[,]', '');
            result = [];
            let datas = data.substring(2, data.length - 2).split('],[');
            for (let i = 0; i < datas.length; ++i) {
                let tmpResult = [];
                let _datas = datas[i].split(',');
                for (let j = 0; j < _datas.length; ++j) {
                    tmpResult.push(this._TransformBasicsValue(type, _datas[j]));
                }
                result.push(tmpResult);
            }
        } else {
            // 普通值
            result = this._TransformBasicsValue(type, data);
        }
        return result;
    }
}


