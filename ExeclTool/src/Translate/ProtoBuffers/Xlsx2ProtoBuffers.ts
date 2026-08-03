import xlsx from 'node-xlsx';
import DataParser from '../../DataParser';
import path from 'path';
import fs from "fs";
import { mkdir, readdir, writeFile } from "fs/promises";
import _ from 'lodash';
import { exec } from 'child_process'
import os from "os";
import ProtoDefine from './ProtoDefine';
import BaseTranslateConfig from '../BaseTranslateConfig';
import BaseTranslateEnum from '../BaseTranslateEnum';
import BaseTranslateStruct from '../BaseTranslateStruct';

export default class Xlsx2ProtoBuffers extends BaseTranslateConfig {


    private outputPathProtosStr: string = '';
    private outputPathBytesStr: string = '';
    private outputPathCodeStr: string = '';
    private outputPathJsonStr: string = '';
    private toCode: string = "";


    public async TranslateExcel(pathStr: string, outputPathStr: string, translate: any, params: any): Promise<void> {

        await super.TranslateExcel(pathStr, outputPathStr, translate, params);

        let enumPath = path.join(params.designPath, 'define', "Enum.xlsx");
        await this.enumHelper.TranslateExcel(enumPath);

        let structPath = path.join(params.designPath, 'define', "Struct.xlsx");
        await this.structHelper.ParseStructDefinitions(structPath);

        this.outputPathProtosStr = path.join(outputPathStr, "protos");
        this.outputPathBytesStr = path.join(outputPathStr, "bytes");
        this.outputPathCodeStr = path.join(outputPathStr, "code", params.toCode);
        this.outputPathJsonStr = path.join(outputPathStr, "json");
        this.toCode = params.toCode;

        if (!fs.existsSync(this.outputPathProtosStr)) {
            await mkdir(this.outputPathProtosStr, { recursive: true });
        }
        if (!fs.existsSync(this.outputPathBytesStr)) {
            await mkdir(this.outputPathBytesStr, { recursive: true });
        }
        if (!fs.existsSync(this.outputPathCodeStr)) {
            await mkdir(this.outputPathCodeStr, { recursive: true });
        }
        if (!fs.existsSync(this.outputPathJsonStr)) {
            await mkdir(this.outputPathJsonStr, { recursive: true });
        }
        if (this.toDir != undefined) {
            this.outputPathJsonStr = path.join(this.outputPathJsonStr, this.toDir);
            if (!fs.existsSync(this.outputPathJsonStr)) {
                await mkdir(this.outputPathJsonStr);
            }
            this.outputPathBytesStr = path.join(this.outputPathBytesStr, this.toDir);
            if (!fs.existsSync(this.outputPathBytesStr)) {
                await mkdir(this.outputPathBytesStr);
            }
        }

        if (this.isDir) {

            let files = fs.readdirSync(pathStr);
            for (let i in files) {
                let data = DataParser.parse(path.join(pathStr, files[i]), params.format);
                for (let i = 0; i < data.length; ++i) {
                    this.xlsxData[data[i].name] = data[i].data;
                }
                let fileName = files[i].replace(path.extname(files[i]), "");
                let fileSuffix = this.StripSheetPrefix(fileName);
                let protoPath = path.join(this.outputPathProtosStr, this.mergeName + ".proto");
                let jsonPath = path.join(this.outputPathJsonStr, this.mergeName + fileSuffix + ".json");
                let bytesPath = path.join(this.outputPathBytesStr, this.mergeName + fileSuffix + ".bytes");
                let messageName = "CfgSpace." + this.mergeName;
                await this.TransferTableJson(fileName);
                if (i == "0") {
                    await this.TransferTableProtos();
                    await this.GenCode(protoPath, this.toCode, this.outputPathCodeStr);
                }
                await this.GenBytes(protoPath, jsonPath, messageName, bytesPath);
            }
        } else {
            let data = DataParser.parseWithOptionalExtension(pathStr, params.format);
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
                await this.GenCode(protoPath, this.toCode, this.outputPathCodeStr);
            }
        }

    }
    private async TransferTableJson(file: string = ""): Promise<void> {
        if (this.merge) {
            let mergedRecord: { [key: string]: any } = {};
            for (let i = 0; i < this.translateSheets.length; ++i) {
                let sheetName = this.translateSheets[i][0];
                let translateName = this.translateSheets[i][1];
                let jsonData = this.CreateJson(this.xlsxData[sheetName], translateName);
                // Convert {"id": record, ...} to {"data": [record, ...]} for *Data wrapper
                let dataArray = [];
                for (let id in jsonData) {
                    dataArray.push(jsonData[id]);
                }
                mergedRecord[translateName + "Data"] = { "data": dataArray };
            }
            let wrapper: { [key: string]: any } = {};
            wrapper[this.mergeName] = mergedRecord;
            await this.SaveJsonToFile(wrapper, path.join(this.outputPathJsonStr, this.mergeName + this.StripSheetPrefix(file)));
        } else {
            for (let i = 0; i < this.translateSheets.length; ++i) {
                let sheetName = this.translateSheets[i][0];
                let translateName = this.translateSheets[i][1];
                let jsonData = this.CreateJson(this.xlsxData[sheetName], translateName);
                await this.SaveJsonToFile(jsonData, path.join(this.outputPathJsonStr, translateName));
            }
        }
    }

    private async TransferTableProtos(): Promise<void> {
        if (this.merge) {
            let protoContent = '';
            protoContent += ProtoDefine.syntax;
            protoContent += ProtoDefine.packageStart;

            // Struct definitions from Struct.xlsx
            // Import Common.proto for struct definitions
            protoContent += ProtoDefine.packageCommonImport;



            for (let i = 0; i < this.translateSheets.length; ++i) {
                let sheetName = this.translateSheets[i][0];
                let translateName = this.translateSheets[i][1];
                protoContent += this.CreateProto(this.xlsxData[sheetName], translateName);
            }
            // When merging multiple sheets, the generated .proto must also contain a
            // top鈥憀evel message named after the merge (e.g., Level) with fields for
            // each sheet's *Data type, and a corresponding wrapper (LevelData) so that
            // the byte鈥慻eneration step can encode the data.
            protoContent += ProtoDefine.messageStart.replace("{0}", this.mergeName);
            let fieldIndex = 1;
            for (let i = 0; i < this.translateSheets.length; ++i) {
                let translateName = this.translateSheets[i][1];
                let dataTypeName = translateName + "Data";
                protoContent += ProtoDefine.fieldStr.replace("{0}", dataTypeName)
                    .replace("{1}", dataTypeName)
                    .replace("{2}", fieldIndex.toString());
                fieldIndex++;
            }
            protoContent += ProtoDefine.messageEnd;
            protoContent += ProtoDefine.messageStart.replace("{0}", this.mergeName + "Data");
            protoContent += ProtoDefine.fieldStr.replace("{0}", "repeated " + this.mergeName)
                .replace("{1}", "data")
                .replace("{2}", "1");
            protoContent += ProtoDefine.messageEnd;
            this.SaveProtosToFile(protoContent, path.join(this.outputPathProtosStr, this.mergeName));
        } else {
            for (let i = 0; i < this.translateSheets.length; ++i) {
                let sheetName = this.translateSheets[i][0];
                let translateName = this.translateSheets[i][1];
                let protoContent = '';
                protoContent += ProtoDefine.syntax;
                protoContent += ProtoDefine.packageStart;

                // Struct definitions from Struct.xlsx
                // Import Common.proto for struct definitions
                protoContent += ProtoDefine.packageCommonImport;



                protoContent += this.CreateProto(this.xlsxData[sheetName], translateName);
                this.SaveProtosToFile(protoContent, path.join(this.outputPathProtosStr, translateName));
            }
        }
    }

    private CreateProtoStruct(structDef: any): string {
        let content = ProtoDefine.messageStart.replace('{0}', structDef.name);
        let fieldIndex = 1;
        for (let field of structDef.fields) {
            content += ProtoDefine.fieldStr.replace('{0}', this.TransformType(field.type))
                .replace('{1}', field.name)
                .replace('{2}', fieldIndex.toString());
            fieldIndex++;
        }
        content += ProtoDefine.messageEnd;
        return content;
    }

    private CreateProto(data: any, className: string): string {
        let dataArr = data;
        let keys = (dataArr[0] || []).slice();
        let typeRowIndex = this.FindTypeRowIndex(dataArr);
        let types = typeRowIndex >= 0 ? (dataArr[typeRowIndex] || []) : [];

        // 检测数组模式（第一列以@开头）
        let isArrayMode = false;
        let arrayKey = '';
        let arrayKeyType = '';
        if (keys.length > 0 && keys[0] && keys[0].startsWith('@')) {
            isArrayMode = true;
            arrayKey = keys[0].substring(1);
            arrayKeyType = types[0] || 'int';
            keys[0] = arrayKey;
        }

        let layout = this.structHelper.AnalyzeSheet([keys, types], className, 1, className);

        let content = "";

        // 1. 为本表未定义的结构生成子 message
        for (let structMsgName in layout.localStructs) {
            content += ProtoDefine.messageStart.replace("{0}", structMsgName);
            let idx = 1;
            for (let field of layout.localStructs[structMsgName].fields) {
                content += ProtoDefine.fieldStr
                    .replace("{0}", this.TransformType(field.type))
                    .replace("{1}", field.name)
                    .replace("{2}", idx.toString());
                idx++;
            }
            content += ProtoDefine.messageEnd;
        }

        // 2. 主 message
        content += ProtoDefine.messageStart.replace("{0}", className);
        let fi = 1;
        for (let field of layout.simpleFields) {
            content += ProtoDefine.fieldStr
                .replace("{0}", this.TransformType(field.type))
                .replace("{1}", field.key)
                .replace("{2}", fi.toString());
            fi++;
        }

        // 3. 结构字段引用
        for (let prop of layout.structProperties) {
            let typeName = prop.isArray ? "repeated " + prop.structName : prop.structName;
            content += ProtoDefine.fieldStr
                .replace("{0}", typeName)
                .replace("{1}", prop.propertyName)
                .replace("{2}", fi.toString());
            fi++;
        }
        content += ProtoDefine.messageEnd;

        if (isArrayMode) {
            let groupMsgName = className + arrayKey;
            content += ProtoDefine.messageStart.replace("{0}", groupMsgName);
            content += ProtoDefine.fieldStr.replace("{0}", "repeated " + className).replace("{1}", "data").replace("{2}", "1");
            content += ProtoDefine.messageEnd;
            content += ProtoDefine.messageStart.replace("{0}", className + "Data");
            let mapType = "map<" + this.TransformType(arrayKeyType) + "," + groupMsgName + ">";
            content += ProtoDefine.fieldStr.replace("{0}", mapType).replace("{1}", "data").replace("{2}", "1");
            content += ProtoDefine.messageEnd;
        } else {
            // 包装 message：XXXData { repeated XXX data = 1; }
            content += ProtoDefine.messageStart.replace("{0}", className + "Data");
            content += ProtoDefine.fieldStr.replace("{0}", "repeated " + className).replace("{1}", "data").replace("{2}", "1");
            content += ProtoDefine.messageEnd;
        }
        return content;
    }

    private CreateJson(data: any, className: string) {
        let jsonOut: { [key: string]: any } = {};

        if (!data || data.length < 3) {
            console.warn('Invalid data for ProtoBuffers', className);
            return jsonOut;
        }

        let dataArr = data;
        let keys = dataArr[0] ? dataArr[0].slice() : []; // 创建副本，避免修改共享数据
        let typeRowIndex = this.FindTypeRowIndex(dataArr);
        let types = typeRowIndex >= 0 ? (dataArr[typeRowIndex] || []) : [];

        // 默认单层级，结构字段由表头等号声明，统一交给 structHelper 处理
        let layerNum = 1;

        // 检查是否是数组模式（第一列以@开头）
        let isArrayMode = false;
        let arrayKey = '';
        if (keys.length > 0 && keys[0] && keys[0].startsWith('@')) {
            isArrayMode = true;
            arrayKey = keys[0].substring(1);
            keys[0] = arrayKey; // 去掉@前缀
        }

        // 校验同名结构的字段类型是否一致，不一致直接抛错
        this.structHelper.CollectStructFields(keys, types, className);

        for (let rowIndex = 3; rowIndex < dataArr.length; ++rowIndex) {
            let _arrLine = dataArr[rowIndex];

            if (_.isNil(_arrLine) || _.isNil(_arrLine[0]) || _arrLine[0] === '') {
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

                // proto 字段名与表头保持一致，不做首字母小写
                this.structHelper.ApplyCellValue(subTmp, key, type, value,
                    (t: string, d: any) => this.TransformStructValue(t, d), false);
            }

            if (isArrayMode) {
                // 数组模式：按第一列的值分组，相同值的行放在数组中
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
                if (type.includes('serialize')) {
                    result = "";
                } else if (this.structHelper.IsStructType(type)) {
                    result = type;
                } else if (type.includes('[,]')) {
                    let baseType = type.replace('[,]', '');
                    result = 'repeated CfgSpace.' + baseType + 'Array';
                } else if (type.includes("[]")) {
                    let baseType = type.replace("[]", "");
                    result = "repeated " + this.TransformType(baseType);
                } else {
                    result = type;
                }
                break;
        }
        return result;
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
        if (type.includes('serialize')) {
            result = this._TransformBasicsValue(type, data);
        } else if (type.includes('[,]')) {
            // Two-dimensional array
            type = type.replace('[,]', '');
            result = [];
            if(data.length > 2) {
                let splitChar = ',';
                if(data.includes('|')) {
                    splitChar = '|'
                }
                if(data.startsWith("[[")) {
                    data = data.substring(2,data.length-2);
                } else if(data.startsWith("[")) {
                    data = data.substring(1,data.length-1)
                }
                let datas = data.split(`]${splitChar}[`);
                for (let i = 0; i < datas.length; ++i) {
                    let tmpResult = []
                    let _datas = datas[i].split(splitChar);
                    for (let j = 0; j < _datas.length; ++j) {
                        tmpResult.push(this._TransformBasicsValue(type, _datas[j]));
                    }
                    result.push(tmpResult);
                }
            }

        } else if (type.includes('[]')) {
            type = type.replace('[]', '');
            result = [];
            if(data.length > 2) {
                let splitChar = ',';
                if(data.includes('|')) {
                    splitChar = '|'
                }
                if(data.startsWith("[")) {
                    data = data.substring(1,data.length-1)
                }
                let _datas = data.split(splitChar);
                for (let i = 0; i < _datas.length; ++i) {
                    result.push(this._TransformBasicsValue(type, _datas[i]));
                }
            }

        } else {
            // Ordinary value
            result = this._TransformBasicsValue(type, data);
        }
        return result;
    }

    private async GenCode(protoPath: string, toCode: string, outputPath: string) {
        let parsedPath = path.parse(protoPath);
        let plugin = "";
        let outExtra = " ";
        if (toCode == "ts") {
            if (os.platform() == "win32") {
                plugin = " --plugin=protoc-gen-ts=.\\node_modules\\.bin\\protoc-gen-ts.cmd"
                    + " --js_out=import_style=commonjs,binary:" + outputPath;
            }
            else {
                plugin = " --plugin=protoc-gen-ts=./node_modules/.bin/protoc-gen-ts"
                    + " --js_out=import_style=commonjs,binary:" + outputPath;
            }
        } else if (toCode == "js") {
            if (os.platform() == "win32") {
                plugin = " --plugin=protoc-gen-js=.\\node_modules\\.bin\\protoc-gen-js.cmd";
            }
            else {
                plugin = " --plugin=protoc-gen-js=./node_modules/.bin/protoc-gen-js";
            }
            outExtra = "=import_style=commonjs,binary:"
        }

        let protocCmd = ".\\lib\\protoc\\protoc.exe -I " + parsedPath.dir + plugin + " --" + toCode + "_out" + outExtra + outputPath + " " + protoPath;
        console.log(protocCmd);
        return new Promise<void>((resolve, reject) => {
            exec(protocCmd, (err, stdout, stderr) => {
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

    private JsonToProtoText(data: any): string {
        function toText(v: any, ind: number): string[] {
            let p = "  ".repeat(ind);
            let r: string[] = [];
            if (v === null || v === undefined) return r;
            if (typeof v === "string") return [JSON.stringify(v)];
            if (typeof v === "number" || typeof v === "boolean") return [String(v)];
            if (Array.isArray(v)) {
                for (let item of v) {
                    if (typeof item === "object" && item !== null) {
                        r.push("{");
                        r.push(...toText(item, ind + 1));
                        r.push("}");
                    } else {
                        r.push(...toText(item, ind));
                    }
                }
                return r;
            }
            for (let k in v) {
                let val = v[k];
                if (val === null || val === undefined) continue;
                if (Array.isArray(val)) {
                    for (let item of val) {
                        if (typeof item === "object" && item !== null) {
                            r.push(p + k + " {", ...toText(item, ind + 1), p + "}");
                        } else {
                            r.push(p + k + ": " + toText(item, 0)[0]);
                        }
                    }
                } else if (typeof val === "object" && val !== null) {
                    r.push(p + k + " {", ...toText(val, ind + 1), p + "}");
                } else {
                    r.push(p + k + ": " + toText(val, 0)[0]);
                }
            }
            return r;
        }
        return toText(data, 0).join("\n");
    }
    private async GenBytes(protoPath: string, jsonPath: string, messageName: string, bytesPath: string) {
        let jsonStr = fs.readFileSync(jsonPath, "utf8");
        let jsonData = JSON.parse(jsonStr);
        // Build proto text for wrapper XXXData message containing all records
        let dataContent = "";
        // 检测数组模式：JSON值是否为数组
        let firstValue = Object.values(jsonData)[0];
        let isArrayMode = Array.isArray(firstValue);
        if (isArrayMode) {
            // 数组模式：map<key, group> 结构，每个key对应一个value包含repeated data
            for (let key in jsonData) {
                let records = jsonData[key];
                dataContent += "data {\n";
                dataContent += `  key: ${key}\n`;
                dataContent += "  value {\n";
                for (let record of records) {
                    dataContent += "    data {\n";
                    let textLines = this.JsonToProtoText(record).split("\n");
                    for (let line of textLines) {
                        if (line.trim()) {
                            dataContent += "      " + line + "\n";
                        }
                    }
                    dataContent += "    }\n";
                }
                dataContent += "  }\n";
                dataContent += "}\n";
            }
        } else {
            // 普通模式：repeated 结构
            for (let key in jsonData) {
                let record = jsonData[key];
                dataContent += "data {\n";
                dataContent += this.JsonToProtoText(record);
                dataContent += "}\n";
            }
        }
        if (dataContent.length > 0) {
            let txtPath = bytesPath + ".txt";
            let binPath = bytesPath + ".bin";
            fs.writeFileSync(txtPath, dataContent, "utf8");
            let wrapperMessage = messageName + "Data";
            let cmd = `.\\lib\\protoc\\protoc.exe --encode=${wrapperMessage} -I ${path.dirname(protoPath)} ${protoPath} < "${txtPath}" > "${binPath}"`;
            try {
                const execSync = require("child_process").execSync;
                execSync(cmd);
                if (fs.existsSync(binPath)) {
                    let binData = fs.readFileSync(binPath);
                    fs.writeFileSync(bytesPath, binData);
                    console.log("Generated:", bytesPath);
                }
            } catch (e) {
                console.error("Failed to encode", messageName, "Data:", (e instanceof Error ? e.message : String(e)));
            }
            try { fs.unlinkSync(txtPath); } catch (e2) { }
            try { fs.unlinkSync(binPath); } catch (e2) { }
        }
    }
}


