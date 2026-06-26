import xlsx from 'node-xlsx';
import path from 'path';
import fs from 'fs';
import { mkdir, readdir, writeFile } from 'fs/promises';
import _ from 'lodash';
import Utils from '../../utils';
import BaseTranslateConfig from '../BaseTranslateConfig';
import DataParser from '../../DataParser';

export default class Xlsx2Ts extends BaseTranslateConfig {

    private outputTsPathStr: string = '';

    public async TranslateExcel(pathStr: string, outputPathStr: string, translate: any, params: any): Promise<void> {

        await super.TranslateExcel(pathStr, outputPathStr, translate, params);

        this.outputTsPathStr = path.join(outputPathStr, 'ts');
        if (!fs.existsSync(this.outputTsPathStr)) {
            await mkdir(this.outputTsPathStr, { recursive: true });
        }

        if (this.toDir != undefined) {
            this.outputTsPathStr = path.join(this.outputTsPathStr, this.toDir);
            if (!fs.existsSync(this.outputTsPathStr)) {
                await mkdir(this.outputTsPathStr, { recursive: true });
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
            let data = DataParser.parseWithOptionalExtension(pathStr, params.format);
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
                let jsonData = this.CreateJson(this.xlsxData[sheetName]);
                all[translateName] = jsonData;
            }
            await this.SaveTsToFile(all, path.join(this.outputTsPathStr, this.mergeName + file));
        } else {
            for (let i = 0; i < this.translateSheets.length; ++i) {
                let sheetName = this.translateSheets[i][0];
                let translateName = this.translateSheets[i][1];
                let jsonData = this.CreateJson(this.xlsxData[sheetName]);
                await this.SaveTsToFile(jsonData, path.join(this.outputTsPathStr, translateName));
            }
        }
    }

    private CreateJson(data: any) {
        let jsonOut: { [key: string]: any } = {};

        if (!data || data.length < 3) {
            console.warn('Invalid data for TS');
            return jsonOut;
        }

        let dataArr = data;
        let keys = dataArr[0] || [];
        let typeRowIndex = this.FindTypeRowIndex(dataArr);
        let types = typeRowIndex >= 0 ? (dataArr[typeRowIndex] || []) : [];

        // 榛樿鐨勫眰绾х粨鏋勬槸绗竴灞傦紝鍗砶ey鐨勪綅缃?
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
                if (_.isNil(value) || typeof (value) == "undefined") {
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

    private async SaveTsToFile(data: any, filePath: string): Promise<void> {
        var _str_all = '';
        _str_all += '//automatic generation,DO NOT EDIT IT!\nexport default \n';
        _str_all += JSON.stringify(data, null, 4);
        _str_all += ';';
        await writeFile(filePath + '.ts', _str_all, { flag: 'w', encoding: 'utf8' });
    }

    // Transform type to corresponding field value type
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

    // Check if type is a struct type
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
            // Two-dimensional array
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
            // Ordinary value
            result = this._TransformBasicsValue(type, data);
        }
        return result;
    }
}


