import xlsx from 'node-xlsx';
import path from 'path';
import fs from "fs";
import _ from 'lodash';
import { exec } from 'child_process'
import Utils from '../../Utils';
import protobuf from 'protobufjs'
import { pbjs, pbts } from 'protobufjs-cli';
import BaseTranslate from '../BaseTranslate';

export default class Xlsx2ProtoBuffers extends BaseTranslate {

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

    private fieldStr = "\t{0} {1} = {2};\n"

    private intArray = "message IntArray {\n\t repeated int32 data = 1;\n}\n\n";

    private boolArray = "message BoolArray {\n\t repeated bool data = 1;\n}\n\n";

    private floatArray = "message FloatArray {\n\t repeated float data = 1;\n}\n\n";

    private stringArray = "message StringArray {\n\t repeated string data = 1;\n}\n\n";


    public BeforeTranslate(outputPathStr: string, params: any) {
        this.outputPathProtosStr = path.join(outputPathStr , "protos");
        this.outputPathCodeStr = path.join(outputPathStr , "code" , params.toCode);
        this.toCode = params.toCode;
        if(!fs.existsSync(this.outputPathProtosStr)) {
            fs.mkdirSync(this.outputPathProtosStr, { recursive: true });
        }
        if(!fs.existsSync(this.outputPathCodeStr)) {
            fs.mkdirSync(this.outputPathCodeStr, { recursive: true });
        }
        this.TransferCommonProtos();
        let protoPath = path.join(this.outputPathProtosStr, "Common.proto");
        this.GenCode(protoPath, this.toCode, this.outputPathCodeStr); 
    }


    public TranslateExcel(pathStr: string, outputPathStr: string, translate: any, params: any) {

        super.TranslateExcel(pathStr,outputPathStr,translate,params);

        this.outputPathProtosStr = path.join(outputPathStr , "protos");
        this.outputPathBytesStr = path.join(outputPathStr , "bytes");
        this.outputPathCodeStr = path.join(outputPathStr , "code" , params.toCode);
        this.outputPathJsonStr = path.join(outputPathStr , "json");
        this.toCode = params.toCode;

        if(!fs.existsSync(this.outputPathProtosStr)) {
            fs.mkdirSync(this.outputPathProtosStr, { recursive: true });
        }
        if(!fs.existsSync(this.outputPathBytesStr)) {
            fs.mkdirSync(this.outputPathBytesStr, { recursive: true });
        }
        if(!fs.existsSync(this.outputPathCodeStr)) {
            fs.mkdirSync(this.outputPathCodeStr, { recursive: true });
        }
        if(!fs.existsSync(this.outputPathJsonStr)) {
            fs.mkdirSync(this.outputPathJsonStr, { recursive: true });
        }
        if(this.toDir != undefined) {
            this.outputPathJsonStr = path.join(this.outputPathJsonStr, this.toDir);
            if(!fs.existsSync(this.outputPathJsonStr)) {
                fs.mkdirSync(this.outputPathJsonStr);
            }
            this.outputPathBytesStr = path.join(this.outputPathBytesStr, this.toDir);
            if(!fs.existsSync(this.outputPathBytesStr)) {
                fs.mkdirSync(this.outputPathBytesStr);
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
                this.TransferTableJson(fileName);
                if(i == "0") {
                    this.TransferTableProtos();
                    this.GenCode(protoPath, this.toCode, this.outputPathCodeStr); 
                }
                this.GenBytes(protoPath, jsonPath, messageName, bytesPath);
            }

        } else {
            let parsedPath = path.parse(pathStr);
            parsedPath.base += ".xlsx";
            parsedPath.ext = ".xlsx";
            let data = xlsx.parse(path.format(parsedPath));
            for (let i = 0; i < data.length; ++i) {
                this.xlsxData[data[i].name] = data[i].data;
            }
            this.TransferTableJson();
            this.TransferTableProtos();
            for (let i = 0; i < this.translateSheets.length; ++i) {
                let sheetName = this.translateSheets[i][0];
                let translateName = this.translateSheets[i][1];

                let protoPath = path.join(this.outputPathProtosStr, translateName + ".proto");
                let jsonPath = path.join(this.outputPathJsonStr, translateName+".json");
                let bytesPath = path.join(this.outputPathBytesStr, translateName+".bytes");
                let messageName = "CfgSpace." + translateName + "s";
                this.GenBytes(protoPath, jsonPath, messageName, bytesPath);
                this.GenCode(protoPath,this.toCode,this.outputPathCodeStr);
            }
        }
    }

    private TransferCommonProtos() {
        let protosContent = this.syntax;
        protosContent += this.packageStart;
        protosContent += this.intArray;
        protosContent += this.boolArray;
        protosContent += this.floatArray;
        protosContent += this.stringArray;
        this.SaveProtosToFile(protosContent, path.join(this.outputPathProtosStr, "Common"));
    }

    private TransferTableProtos() {
        if(this.merge) {

            let protosContent = this.syntax;
            protosContent += this.packageStart;
            protosContent += this.packageCommonImport;

            for (let i = 0; i < this.translateSheets.length; ++i) {
                let sheetName = this.translateSheets[i][0];
                let translateName = this.translateSheets[i][1];
                protosContent += this.CreateProtos(this.xlsxData[sheetName],translateName);
            }

            protosContent += Utils.FormatStr(this.messageStart, this.mergeName);
            for (let i = 0; i < this.translateSheets.length; ++i) {
                let sheetName = this.translateSheets[i][0];
                let translateName = this.translateSheets[i][1];
                let upAndLower = Utils.GetFristUpperAndLowerStr(translateName);
                let translateNamekeyUpper = upAndLower[0];
                let translateNamekeyLower = upAndLower[1];
                protosContent += Utils.FormatStr(this.fieldStr, "repeated " + translateNamekeyUpper , translateNamekeyLower + "Array", " "+ (i + 1));
            }
            protosContent += this.messageEnd;

            this.SaveProtosToFile(protosContent, path.join(this.outputPathProtosStr, this.mergeName));
        }else{
            for (let i = 0; i < this.translateSheets.length; ++i) {

                let sheetName = this.translateSheets[i][0];
                let translateName = this.translateSheets[i][1];
                let upAndLower = Utils.GetFristUpperAndLowerStr(translateName);
                let translateNamekeyUpper = upAndLower[0];
                let translateNamekeyLower = upAndLower[1];
                let protosContent = this.syntax;
                protosContent += this.packageStart;
                protosContent += this.packageCommonImport;
                protosContent += this.CreateProtos(this.xlsxData[sheetName],translateName);
                protosContent += Utils.FormatStr(this.messageStart, translateName + "s");
                protosContent += Utils.FormatStr(this.fieldStr, "repeated " +  translateName, translateNamekeyLower + "Array", "1");
                protosContent += this.messageEnd;
                this.SaveProtosToFile(protosContent, path.join(this.outputPathProtosStr, translateName));
            }
        }
    }

    private CreateProtos(data: any, className: string) {

        let tableContent = Utils.FormatStr(this.messageStart, className);
        let dataArr = data;
        let keys = dataArr[0];
        let types = dataArr[1];
        let decs = dataArr[2];
        for (let keyIndex = 0; keyIndex < keys.length; ++keyIndex) {
            let key = keys[keyIndex];
            if (_.isNil(key) || _.isEmpty(key)) {
                continue;
            }
            let type = this.TransformType(types[keyIndex])
            if(type != undefined && type != "") {
                let keyLower = Utils.GetFristUpperAndLowerStr(key)[1];
                tableContent += Utils.FormatStr(this.fieldStr, type, keyLower, "" + (keyIndex + 1));
            }
        }
        tableContent += this.messageEnd;
        return tableContent;
    }

    private SaveProtosToFile(data: any, filePath: string) {
        fs.writeFileSync(filePath + ".proto", data, { flag: 'w', encoding: 'utf8' });
    }

    private TransferTableJson(file: string = "") {
        if(this.merge) {
            let all: {[key: string]: any} = {};
            for (let i = 0; i < this.translateSheets.length; ++i) {
                let sheetName = this.translateSheets[i][0];
                let translateName = this.translateSheets[i][1];
                let upAndLower = Utils.GetFristUpperAndLowerStr(translateName);
                let translateNamekeyUpper = upAndLower[0];
                let translateNamekeyLower = upAndLower[1];
                let jsonData = this.CreateJson(this.xlsxData[sheetName],translateName);
                all[translateNamekeyLower + "Array"] = jsonData[translateNamekeyLower + "Array"];
            }
            this.SaveJsonToFile(all, path.join(this.outputPathJsonStr, this.mergeName + file));
        }else{
            for (let i = 0; i < this.translateSheets.length; ++i) {
                let sheetName = this.translateSheets[i][0];
                let translateName = this.translateSheets[i][1];
                let jsonData = this.CreateJson(this.xlsxData[sheetName],translateName);
                this.SaveJsonToFile(jsonData, path.join(this.outputPathJsonStr, translateName));
            }
        }
    }

    private CreateJson(data: any, className: string) {
        let jsonOut: { [key: string]: any }[] = [];

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

        let index = 0;
        for (let rowIndex = 3; rowIndex < dataArr.length; ++rowIndex) {
            let _arrLine = dataArr[rowIndex];

            if (_.isNil(_arrLine[0]) || _arrLine[0] == '') {
                continue;
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

            jsonOut[index] = subTmp;

            ++index;
        }

        let output : {[key:string]: object} = {};
        className = Utils.GetFristUpperAndLowerStr(className)[1];
        output[className + "Array"] = jsonOut;
        return output;
    }

    private SaveJsonToFile(data: any, filePath: string) {
        var _str_all = "";
        _str_all += JSON.stringify(data, null, 4);
        fs.writeFileSync(filePath + ".json", _str_all, { flag: 'w', encoding: 'utf8' });
    }

    private GenBytes(protoPath: string, jsonPath: string, messageName: string ,bytesPath: string) {
        let jsonData = JSON.parse(fs.readFileSync(jsonPath).toString());
        let root = protobuf.loadSync(protoPath)
        if(root != null)
        {
            let allMessages= root.lookupType(messageName);
            let message = allMessages.create(jsonData);
            let buffer = allMessages.encode(message).finish();
            fs.writeFileSync(bytesPath, buffer);
        }
    }

    private GenCode(protoPath: string, toCode: string, outputPath: string) {
        let cmd = "";
        let parsedPath = path.parse(protoPath);
        switch (toCode) {
            case "ts":
                parsedPath.dir = outputPath;
                parsedPath.base = "";
                parsedPath.ext = ".js";
                let jsPath = path.format(parsedPath);
                parsedPath.ext = ".d.ts";
                let dtsPath = path.format(parsedPath);
                pbjs.main(["-t", "static-module", "-w","commonjs", "-o", jsPath,  protoPath, "-p", path.dirname(protoPath)],(err, output)=>{
                    if(err){
                        console.log("err == " + err);
                    }
                    pbts.main(["-o",dtsPath, jsPath]);
                });
                break;

            default:
                cmd = ".\\lib\\protoc\\protoc.exe" + " -I " + parsedPath.dir +  " --" + toCode +"_out " + outputPath + " " + protoPath;
                console.log(cmd);
                exec(cmd, (err, stdout, stderr) => {
                    if(err) {
                        throw err;
                    }
                    console.log(stdout);  // stdout为执行命令行操作后返回的正常结果
                    console.log(stderr);  // stderr为执行命令行操作后返回的错误提示    
                });
                break;
        }
    }

    private TransformType(type: string) {
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
            case 'string[,]':
            case 'String[,]':
                result = 'repeated CfgSpace.StringArray';
            default:
                if(type.includes('serialize')) {
                    result = "";
                }else if(type.includes("[]")) {
                    result = "repeated " + this.TransformType(type.replace("[]",""));
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
    private TransformValue (type: string, data: string, row?: number, col?: number) {

        if(type.includes('serialize')) {
            return undefined;
        }

        let result;
        if(typeof(data) == 'string') {
            data = data.replace(/[\r\n]/g, '');
        }
        if(type.includes('serialize')) {
            result = this._TransformBasicsValue(type, data);
        }else if(type.includes('[]')) {   
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
                result.push({data : tmpResult});
            }

        }else{
            //正常值
            result = this._TransformBasicsValue(type, data);
        }
        return result;
    }
}