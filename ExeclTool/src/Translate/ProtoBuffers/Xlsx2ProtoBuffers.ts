import xlsx from 'node-xlsx';
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
                let data = xlsx.parse(path.join(pathStr, files[i]));
                for (let i = 0; i < data.length; ++i) {
                    this.xlsxData[data[i].name] = data[i].data;
                }
                let fileName = files[i].replace(path.extname(files[i]), "");
                let protoPath = path.join(this.outputPathProtosStr, this.mergeName + ".proto");
                let jsonPath = path.join(this.outputPathJsonStr, this.mergeName + fileName + ".json");
                let bytesPath = path.join(this.outputPathBytesStr, this.mergeName + fileName + ".bytes");
                let messageName = "CfgSpace." + this.mergeName;
                await this.TransferTableJson(fileName);
                if (i == "0") {
                    await this.TransferTableProtos();
                    await this.GenCode(protoPath, this.toCode, this.outputPathCodeStr);
                }
                await this.GenBytes(protoPath, jsonPath, messageName, bytesPath);
            }
        } else {
            let parsedPath = path.parse(pathStr);
            parsedPath.base += ".xlsx";
            parsedPath.ext = ".xlsx";
            let data = xlsx.parse(path.format(parsedPath));
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
            await this.SaveJsonToFile(wrapper, path.join(this.outputPathJsonStr, this.mergeName + file));
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
            // top‑level message named after the merge (e.g., Level) with fields for
            // each sheet's *Data type, and a corresponding wrapper (LevelData) so that
            // the byte‑generation step can encode the data.
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
        let keys = dataArr[0] || [];
        let types = dataArr[1] || [];
        let nestedMap: { [parent: string]: { fields: { [name: string]: string }; isArray: boolean; structName: string } } = {};
        let simpleKeys: { key: string; type: string }[] = [];
        for (let i = 0; i < keys.length; ++i) {
            let key = keys[i];
            let type = types[i];
            if (_.isNil(key) || _.isEmpty(key)) continue;
            let fieldInfo = this.structHelper.AnalyzeField(key, type);
            if (fieldInfo.isStruct && fieldInfo.fieldPath.length > 0) {
                let parentName = fieldInfo.name;
                let subFieldName = fieldInfo.fieldPath[fieldInfo.fieldPath.length - 1];
               if (!nestedMap[parentName]) {
                    let resolvedStructName = fieldInfo.structName || (parentName.charAt(0).toUpperCase() + parentName.slice(1));
                    nestedMap[parentName] = { fields: {}, isArray: fieldInfo.isArray, structName: resolvedStructName };
                }
                if (fieldInfo.isArray) nestedMap[parentName].isArray = true;
                if (!nestedMap[parentName].fields[subFieldName]) {
                    nestedMap[parentName].fields[subFieldName] = type;
                }
                continue;
            }
            simpleKeys.push({ key, type });
        }
        let content = "";
       // Generate sub-messages for non-Struct.xlsx types
       for (let parentName in nestedMap) {
            let structName = nestedMap[parentName].structName;
            if (this.structHelper.IsStructType(structName)) continue;
            let subName = className + "_" + structName;
            content += ProtoDefine.messageStart.replace("{0}", subName);
            let idx = 1;
            for (let fn in nestedMap[parentName].fields) {
                let ft = nestedMap[parentName].fields[fn];
                content += ProtoDefine.fieldStr.replace("{0}", this.TransformType(ft)).replace("{1}", fn).replace("{2}", idx.toString());
                idx++;
            }
            content += ProtoDefine.messageEnd;
        }
        // Main message
        content += ProtoDefine.messageStart.replace("{0}", className);
        let fi = 1;
        for (let f of simpleKeys) {
            content += ProtoDefine.fieldStr.replace("{0}", this.TransformType(f.type)).replace("{1}", f.key).replace("{2}", fi.toString());
            fi++;
        }
       // Add nested field references
       for (let parentName in nestedMap) {
            let structName = nestedMap[parentName].structName;
            let tn = this.structHelper.IsStructType(structName) ? structName : className + "_" + structName;
            let isArr = false;
            for (let k of keys) { if (k && (k.startsWith(parentName + "[") || k.startsWith(parentName + ".")) && k !== parentName) { isArr = k.includes("["); break; } }
            if (isArr) tn = "repeated " + tn;
            let fld = this.structHelper.IsStructType(structName) ? parentName.charAt(0).toLowerCase() + parentName.slice(1) : parentName;
            content += ProtoDefine.fieldStr.replace("{0}", tn).replace("{1}", fld).replace("{2}", fi.toString());
            fi++;
        }
        content += ProtoDefine.messageEnd;
        // Add wrapper message XXXData { repeated XXX data = 1; }
        content += ProtoDefine.messageStart.replace("{0}", className + "Data");
        content += ProtoDefine.fieldStr.replace("{0}", "repeated " + className).replace("{1}", "data").replace("{2}", "1");
        content += ProtoDefine.messageEnd;
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

        // Default layerNum: first data layer may contain nested fields
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

                let firstField = fieldPath[0].toString();
                let capitalized = firstField.charAt(0).toUpperCase() + firstField.slice(1);
                if (this.structHelper.IsStructType(capitalized)) {
                    fieldPath[0] = firstField.charAt(0).toLowerCase() + firstField.slice(1);
                }

                if (fieldPath.length > 1) {
                    this.structHelper.SetNestedValue(subTmp, fieldPath, this.TransformStructValue(type, value));
                } else {
                    let result = this.TransformStructValue(type, value, rowIndex, colIndex);
                    if (!_.isNil(result) && !_.isNaN(result)) {
                        subTmp[key] = result;
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
            let datas = data.substring(2, data.length - 2).split('],[');
            for (let i = 0; i < datas.length; ++i) {
                let tmpResult = []
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
        for (let key in jsonData) {
            let record = jsonData[key];
            dataContent += "data {\n";
            dataContent += this.JsonToProtoText(record);
            dataContent += "}\n";
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
