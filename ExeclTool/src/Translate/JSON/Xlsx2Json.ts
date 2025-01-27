import * as xlsx from 'node-xlsx';
import path from 'path';
import fs from "fs";
import { mkdir, readdir, writeFile } from "fs/promises";
import _ from 'lodash';
import Utils from '../../utils';
import BaseTranslateConfig from '../BaseTranslateConfig';

export default class Xlsx2Json extends BaseTranslateConfig {

    private outputPathJsonStr: string = '';

    public async TranslateExcel(pathStr: string, outputPathStr: string, translate: any, params: any) : Promise<void> {

        await super.TranslateExcel(pathStr,outputPathStr,translate,params);
        console.log("-- isDir " + this.isDir + "    " + this.translateSheets)

        this.outputPathJsonStr = path.join(outputPathStr , "json");
        if(!fs.existsSync(this.outputPathJsonStr)) {
            await mkdir(this.outputPathJsonStr, { recursive: true });
        }

        if(this.toDir != undefined) {
            this.outputPathJsonStr = path.join(this.outputPathJsonStr,this.toDir);
            if(!fs.existsSync(this.outputPathJsonStr)) {
                await mkdir(this.outputPathJsonStr, { recursive: true });
            }
        }

        if (this.isDir) { 
            let files = await readdir(pathStr);
            for(let i in files) {
                let data = xlsx.parse(path.join(pathStr, files[i]));
                for (let i = 0; i < data.length; ++i) {
                    this.xlsxData[data[i].name] = data[i].data;
                }
                await this.TransferTable(files[i].replace(path.extname(files[i]),""));
            }
        } else {
            let parsedPath = path.parse(pathStr);
            parsedPath.base += ".xlsx";
            parsedPath.ext = ".xlsx";
            let data = xlsx.parse(path.format(parsedPath));
            for (let i = 0; i < data.length; ++i) {
                this.xlsxData[data[i].name] = data[i].data;
            }
            await this.TransferTable();
        }
    }

    private async TransferTable(file: string = "") : Promise<void> {
        if(this.merge) {
            let all: {[key: string]: any} = {};
            for (let i = 0; i < this.translateSheets.length; ++i) {
                let sheetName = this.translateSheets[i][0];
                let translateName = this.translateSheets[i][1];
                let upperAndLower = Utils.GetFristUpperAndLowerStr(translateName);
                let translateNamekeyUpper = upperAndLower[0];
                let translateNamekeyLower = upperAndLower[1];
                let jsonData = this.CreateJson(this.xlsxData[sheetName],translateName);
                all[translateNamekeyLower] = jsonData;
            }
            await this.SaveJsonToFile(all, path.join(this.outputPathJsonStr,this.mergeName + file));
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
                    let keyLower = Utils.GetFristUpperAndLowerStr(key)[1];
                    subTmp[keyLower] = result;
                }
            }
            tmp[_arrLine[layerNum - 1]] = subTmp;

        }

        let output = jsonOut;

        return output;
    }

    private async SaveJsonToFile(data: any, filePath: string) : Promise<void>{
        var _str_all = "";
        _str_all += JSON.stringify(data, null, 4);
        return writeFile(filePath + ".json", _str_all, { flag: 'w', encoding: 'utf8' });
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

    //检测type 对应的值
    public TransformValue (type: string, data: string, row?: number, col?: number) {

        if(type.includes('serialize')) {
            return undefined;
        }

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