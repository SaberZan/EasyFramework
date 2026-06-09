import xlsx from 'node-xlsx';
import path from 'path';
import fs from "fs";
import { mkdir, readdir, writeFile } from "fs/promises";
import _ from 'lodash';
import { exec } from 'child_process'
import Utils from '../../utils';
import BaseTranslateConfig from '../BaseTranslateConfig';
import BaseTranslateEnum from '../BaseTranslateEnum';
import BaseTranslateStruct from '../BaseTranslateStruct';

export default class Xlsx2ProtoBuffers extends BaseTranslateConfig {


    private outputPathProtosStr: string = '';
    private outputPathBytesStr: string = '';
    private outputPathCodeStr: string = '';
    private outputPathJsonStr: string = '';
    private toCode: string = "";
    private syntax = "syntax = \"proto3\"; \n\n";
    private packageStart = "package CfgSpace; \n\n";
    private packageCommonImport = "import \"Common.proto\"; \n\n";
    private messageStart = "message {0} {\n";
    private messageEnd = "}\n\n";
    private enumStart = "enum {0} {\n";
    private enumEnd = "}\n\n";
    private fieldEnumStr = "{0} = {1};\n";
    private fieldMapStr = "\t Map<{0},{1}> {2} = {3};\n"
    private fieldStr = "\t{0} {1} = {2};\n";
    private intArray = "message IntArray {\n\t repeated int32 data = 1;\n}\n\n";
    private boolArray = "message BoolArray {\n\t repeated bool data = 1;\n}\n\n";
    private floatArray = "message FloatArray {\n\t repeated float data = 1;\n}\n\n";
    private stringArray = "message StringArray {\n\t repeated string data = 1;\n}\n\n";

    public async TranslateExcel(pathStr: string, outputPathStr: string, translate: any, params: any) : Promise<void> {

        await super.TranslateExcel(pathStr,outputPathStr,translate,params);

        let enumPath = path.join(params.designPath, 'define', "Enum.xlsx");
        await this.enumHelper.TranslateExcel(enumPath);
        //TODO 生成枚举的protos文件, 以及bytes文件 Code 文件, json文件

        let structPath = path.join(params.designPath, 'define', "Struct.xlsx");
        await this.structHelper.ParseStructDefinitions(structPath);

        this.outputPathProtosStr = path.join(outputPathStr , "protos");
        this.outputPathBytesStr = path.join(outputPathStr , "bytes");
        this.outputPathCodeStr = path.join(outputPathStr , "code" , params.toCode);
        this.outputPathJsonStr = path.join(outputPathStr , "json");
        this.toCode = params.toCode;

        if(!fs.existsSync(this.outputPathProtosStr)) {
            await mkdir(this.outputPathProtosStr, { recursive: true });
        }
        if(!fs.existsSync(this.outputPathBytesStr)) {
            await mkdir(this.outputPathBytesStr, { recursive: true });
        }
        if(!fs.existsSync(this.outputPathCodeStr)) {
            await mkdir(this.outputPathCodeStr, { recursive: true });
        }
        if(!fs.existsSync(this.outputPathJsonStr)) {
            await mkdir(this.outputPathJsonStr, { recursive: true });
        }
        if(this.toDir != undefined) {
            this.outputPathJsonStr = path.join(this.outputPathJsonStr, this.toDir);
            if(!fs.existsSync(this.outputPathJsonStr)) {
                await mkdir(this.outputPathJsonStr);
            }
            this.outputPathBytesStr = path.join(this.outputPathBytesStr, this.toDir);
            if(!fs.existsSync(this.outputPathBytesStr)) {
                await mkdir(this.outputPathBytesStr);
            }
        }

        if (this.isDir) { 

            let files = fs.readdirSync(pathStr);
            for(let i in files) {
                let data = xlsx.parse(path.join(pathStr, files[i]));
                for (let i = 0; i < data.length; ++i) {
                    this.xlsxData[data[i].name] = data[i].data;
                }
                let fileName = files[i].replace(path.extname(files[i]),"");
                let protoPath = path.join(this.outputPathProtosStr, this.mergeName + ".proto");
                let jsonPath = path.join(this.outputPathJsonStr, this.mergeName + fileName+".json");
                let bytesPath = path.join(this.outputPathBytesStr, this.mergeName + fileName+".bytes");
                let messageName = "CfgSpace." + this.mergeName;
                await this.TransferTableJson(fileName);
                if(i == "0") {
                    await this.TransferTableProtos();
                    await this.GenCode(protoPath, this.toCode, this.outputPathCodeStr); 
                }
                await this.GenBytes(protoPath, jsonPath, messageName, bytesPath);
            }

        } else {
            let data = xlsx.parse(pathStr);
            for (let i = 0; i < data.length; ++i) {
                this.xlsxData[data[i].name] = data[i].data;
            }
            await this.TransferTableJson();
            await this.TransferTableProtos();
            for (let i = 0; i < this.translateSheets.length; ++i) {
                let sheetName = this.translateSheets[i][0];
                let translateName = this.translateSheets[i][1];
                let protoPath = path.join(this.outputPathProtosStr, translateName + ".proto");
                let jsonPath = path.join(this.outputPathJsonStr, translateName + ".json");
                let bytesPath = path.join(this.outputPathBytesStr, translateName + ".bytes");
                let messageName = "CfgSpace." + translateName;
                await this.GenBytes(protoPath, jsonPath, messageName, bytesPath);
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

    private async TransferTableProtos() : Promise<void> {
        if(this.merge) {
            let protoContent = '';
            protoContent += this.syntax;
            protoContent += this.packageStart;
            
            // 锟斤拷锟斤拷锟斤拷锟叫结构锟藉定锟斤拷
            for (let structName in this.structHelper.structDefinitions) {
                let structDef = this.structHelper.structDefinitions[structName];
                protoContent += this.CreateProtoStruct(structDef);
            }
            
            for (let i = 0; i < this.translateSheets.length; ++i) {
                let sheetName = this.translateSheets[i][0];
                let translateName = this.translateSheets[i][1];
                protoContent += this.CreateProto(this.xlsxData[sheetName], translateName);
            }
            this.SaveProtosToFile(protoContent, path.join(this.outputPathProtosStr, this.mergeName));
        }else{
            for (let i = 0; i < this.translateSheets.length; ++i) {
                let sheetName = this.translateSheets[i][0];
                let translateName = this.translateSheets[i][1];
                let protoContent = '';
                protoContent += this.syntax;
                protoContent += this.packageStart;
                
                // 锟斤拷锟斤拷锟斤拷锟叫结构锟藉定锟斤拷
                for (let structName in this.structHelper.structDefinitions) {
                    let structDef = this.structHelper.structDefinitions[structName];
                    protoContent += this.CreateProtoStruct(structDef);
                }
                
                protoContent += this.CreateProto(this.xlsxData[sheetName], translateName);
                this.SaveProtosToFile(protoContent, path.join(this.outputPathProtosStr, translateName));
            }
        }
    }

    private CreateProtoStruct(structDef: any) : string {
        let content = this.messageStart.replace('{0}', structDef.name);
        let fieldIndex = 1;
        for (let field of structDef.fields) {
            content += this.fieldStr.replace('{0}', this.TransformType(field.type))
                                     .replace('{1}', field.name)
                                     .replace('{2}', fieldIndex.toString());
            fieldIndex++;
        }
        content += this.messageEnd;
        return content;
    }

    private CreateProto(data: any, className: string) : string {
        let dataArr = data;
        let keys = dataArr[0];
        let types = dataArr[1];
        let content = this.messageStart.replace('{0}', className);
        let fieldIndex = 1;
        for (let i = 0; i < keys.length; ++i) {
            let key = keys[i];
            let type = types[i];
            if (_.isNil(key) || _.isEmpty(key)) {
                continue;
            }
            content += this.fieldStr.replace('{0}', this.TransformType(type))
                                     .replace('{1}', key)
                                     .replace('{2}', fieldIndex.toString());
            fieldIndex++;
        }
        content += this.messageEnd;
        return content;
    }

    private CreateJson(data: any, className: string) {
        let jsonOut: { [key: string]: any } = {};

        if (!data || data.length < 3) {
            console.warn('Invalid data for ProtoBuffers', className);
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
                if (_.isNil(value) || typeof(value) == "undefined") {
                    continue;
                }

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

            tmp[_arrLine[layerNum - 1]] = subTmp;
        }

        return jsonOut;
    }

    private async SaveJsonToFile(data: any, filePath: string) {
        var _str_all = "";
        _str_all += JSON.stringify(data, null, 4);
        return writeFile(filePath + ".json", _str_all, { flag: 'w', encoding: 'utf8' });
    }

    private SaveProtosToFile(content: string, filePath: string) {
        fs.writeFileSync(filePath + ".proto", content, { flag: 'w', encoding: 'utf8' });
    }

    private TransformType(type: any) {
        if (typeof type !== 'string') return '';
        let result = type;
        switch (type) {
            case 'int':
            case 'Int':
                result = 'int32';
                break;
            case 'int[]':
            case 'Int[]':
                result = 'repeated int32';
                break;   
            case 'int[,]':
            case 'Int[,]':
                result = 'repeated CfgSpace.IntArray';
                break;   
            case 'float':
            case 'Float':
                result = 'float';
                break;
            case 'float[]':
            case 'Float[]':
                result = 'repeated float';
                break;
            case 'float[,]':
            case 'Float[,]':
                result = 'repeated CfgSpace.FloatArray';
                break;    
            case 'bool':
            case 'Bool':
            case 'boolen':
            case 'Boolen':
                result = 'bool';
                break;
            case 'bool[]':
            case 'Bool[]':
            case 'boolen[]':
            case 'Boolen[]':
                result = 'repeated bool';
                break;
            case 'bool[,]':
            case 'Bool[,]':
            case 'boolen[,]':
            case 'Boolen[,]':
                result = 'repeated CfgSpace.BoolArray';
                break;
            case 'string':
            case 'String':
                result = 'string';
                break;
            case 'string[]':
            case 'String[]':
                result = 'repeated string';
                break;
            case 'string[,]':
            case 'String[,]':
                result = 'repeated CfgSpace.StringArray';
                break;
            default:
                if(type.includes('serialize')) {
                    result = "";
                }else if (this.structHelper.IsStructType(type)) {
                    result = type;
                } else if(type.includes('[,]')) {
                    let baseType = type.replace('[,]', '');
                    result = 'repeated CfgSpace.' + baseType + 'Array';
                } else if(type.includes("[]")) {
                    let baseType = type.replace("[]","");
                    result = "repeated " + this.TransformType(baseType);
                }else{
                    result = type;
                }
                break;
        }
        return result;
    }

    // 转锟斤拷锟斤拷锟斤拷锟叫碉拷锟街碉拷锟斤拷应锟斤拷锟斤拷锟斤拷锟斤拷锟斤拷
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
            default:
                result = data;
                if (result == '') {
                    result = null;
                }
                break;
        }
        return result;
    }

    // 锟斤拷锟?type 锟角凤拷为锟接结构
    private TransformStructValue (type: string, data: string, row?: number, col?: number) {
        if (this.structHelper.IsStructType(type)) {
            return this.structHelper.TransformStructValue(type, data);
        }

        let result;
        if(typeof(data) == 'string') {
            data = data.replace(/[\r\n]/g, '');
        }
        if(type.includes('serialize')) {
            result = this._TransformBasicsValue(type, data);
        }else if(type.includes('[,]')) {
            // 锟斤拷维锟斤拷锟斤拷
            type = type.replace('[,]','');
            result = [];
            let datas = data.substring(2,data.length-2).split('],[');
            for (let i = 0; i < datas.length; ++i) {
                let tmpResult = []
                let _datas = datas[i].split(',');
                for (let j = 0; j < _datas.length; ++j) {
                    tmpResult.push(this._TransformBasicsValue(type, _datas[j]));
                }
                result.push({data : tmpResult});
            }

        }else if(type.includes('[]')) {   
            type = type.replace('[]','');
            result = [];
            let _datas = data.substring(1,data.length -1).split(',');
            for (let i = 0; i < _datas.length; ++i) {
                result.push(this._TransformBasicsValue(type, _datas[i]));
            }

        }else{
            // 锟斤拷通值
            result = this._TransformBasicsValue(type, data);
        }
        return result;
    }

    private async GenCode(protoPath: string, toCode: string, outputPath: string) {
        let cmd = `pbjs -t static-module -w commonjs -o ${path.join(outputPath, toCode + ".js")} ${protoPath}`;
        console.log(cmd);
        return new Promise<void>((resolve, reject) => {
            exec(cmd, (err, stdout, stderr) => {
                if(err) {
                    reject(err);
                    return;
                }
                console.log(stdout);  
                console.log(stderr);    
                resolve();
            });
        });
    }

    private async GenBytes(protoPath: string, jsonPath: string, messageName: string, bytesPath: string) {
        let cmd = `pbjs -t static-module -w commonjs -o ${bytesPath}.js ${protoPath}`;
        console.log(cmd);
        return new Promise<void>((resolve, reject) => {
            exec(cmd, (err, stdout, stderr) => {
                if(err) {
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
