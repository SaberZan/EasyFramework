import xlsx from 'node-xlsx';
import path from 'path';
import fs from "fs";
import { mkdir, readdir, writeFile } from "fs/promises";
import _ from 'lodash';
import Utils from '../../utils';
import BaseTranslateConfig from '../BaseTranslateConfig';

export default class Xlsx2Json extends BaseTranslateConfig {

    private outputPathJsonStr: string = '';

    public async TranslateExcel(pathStr: string, outputPathStr: string, translate: any, params: any): Promise<void> {

        await super.TranslateExcel(pathStr, outputPathStr, translate, params);
        console.log('-- isDir ' + this.isDir + '    ' + this.translateSheets);

        this.outputPathJsonStr = path.join(outputPathStr, 'json');
        if (!fs.existsSync(this.outputPathJsonStr)) {
            await mkdir(this.outputPathJsonStr, { recursive: true });
        }

        if (this.toDir != undefined) {
            this.outputPathJsonStr = path.join(this.outputPathJsonStr, this.toDir);
            if (!fs.existsSync(this.outputPathJsonStr)) {
                await mkdir(this.outputPathJsonStr, { recursive: true });
            }
        }

        if (this.isDir) {
            let files = await readdir(pathStr);
            for (let i in files) {
                let data = xlsx.parse(path.join(pathStr, files[i]));
                for (let j = 0; j < data.length; ++j) {
                    this.xlsxData[data[j].name] = data[j].data;
                }
                await this.TransferTable(files[i].replace(path.extname(files[i]), ''));
            }
        } else {
            let parsedPath = path.parse(pathStr);
            parsedPath.base += '.xlsx';
            parsedPath.ext = '.xlsx';
            let data = xlsx.parse(path.format(parsedPath));
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
                let upperAndLower = Utils.GetFristUpperAndLowerStr(translateName);
                let translateNamekeyUpper = upperAndLower[0];
                let translateNamekeyLower = upperAndLower[1];
                let jsonData = this.CreateJson(this.xlsxData[sheetName], translateName);
                all[translateNamekeyLower] = jsonData;
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

    private CreateJson(data: any, className: string) {
        let jsonOut: { [key: string]: any } = {};

        if (!data || data.length < 3) {
            console.warn('Invalid data for', className);
            return jsonOut;
        }

        let dataArr = data;
        let keys = dataArr[0] || [];
        let types = dataArr[1] || [];

        // 默锟较碉拷锟姐级锟斤拷锟斤拷锟斤拷锟揭伙拷锟斤拷锟斤拷莅锟斤拷锟角讹拷锟斤拷侄危锟斤拷锟斤拷锟斤拷锟街讹拷锟斤拷锟斤拷锟斤拷锟斤拷锟皆讹拷锟斤拷锟斤拷
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

            tmp[_arrLine[layerNum - 1]] = subTmp;
        }

        return jsonOut;
    }

    private async SaveJsonToFile(data: any, filePath: string): Promise<void> {
        var _str_all = '';
        _str_all += JSON.stringify(data, null, 4);
        return writeFile(filePath + '.json', _str_all, { flag: 'w', encoding: 'utf8' });
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
            case 'json':
            case 'Json':
                result = 'JSONObject';
                break;
            default:
                if (type.includes('serialize')) {
                    result = undefined;
                } else {
                    result = type;
                }
                break;
        }
        return result;
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
            result = this._TransformBasicsValue(type, data);
        }
        return result;
    }
}
