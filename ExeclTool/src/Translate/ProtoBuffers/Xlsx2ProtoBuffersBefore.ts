import xlsx from 'node-xlsx';
import path from 'path';
import fs from "fs";
import { mkdir, readdir, writeFile } from "fs/promises";
import _, { reject } from 'lodash';
import { exec } from 'child_process'
import os from "os";
import Utils from '../../utils';
import ProtoDefine from './ProtoDefine';
import protobuf from 'protobufjs'
import BaseTranslateEnum from '../BaseTranslateEnum'
import BaseTranslateStruct from '../BaseTranslateStruct'
import BaseTranslateBefore from '../BaseTranslateBefore';

export default class Xlsx2ProtoBuffersBefore extends BaseTranslateBefore {

    private outputPathProtosStr: string = '';

    private outputPathBytesStr: string = '';

    private outputPathCodeStr: string = '';

    private outputPathJsonStr: string = '';

    private toCode: string = "";

    public async BeforeTranslate(outputPathStr: string, params: any): Promise<void> {
        this.outputPathProtosStr = path.join(outputPathStr, "protos");
        this.outputPathCodeStr = path.join(outputPathStr, "code", params.toCode);
        this.toCode = params.toCode;
        if (!fs.existsSync(this.outputPathProtosStr)) {
            await mkdir(this.outputPathProtosStr, { recursive: true });
        }
        if (!fs.existsSync(this.outputPathCodeStr)) {
            await mkdir(this.outputPathCodeStr, { recursive: true });
        }
        await this.TransferCommonProtos();
        await this.TransferEnumProtos();
        let protoPath = path.join(this.outputPathProtosStr, "Common.proto");
        await this.GenCode(protoPath, this.toCode, this.outputPathCodeStr);
    }

    private async TransferCommonProtos(): Promise<void> {
        let protosContent = ProtoDefine.syntax;
        protosContent += ProtoDefine.packageStart;
        protosContent += ProtoDefine.intArray;
        protosContent += ProtoDefine.boolArray;
        protosContent += ProtoDefine.floatArray;
        protosContent += ProtoDefine.stringArray;

        // Add struct definitions from Struct.xlsx
        let structDefPath = path.join(__dirname, '..', '..', '..', 'design', 'define', 'Struct.xlsx');
        let structHelper = new BaseTranslateStruct();
        await structHelper.ParseStructDefinitions(structDefPath);
        for (let structName in structHelper.structDefinitions) {
            let def = structHelper.structDefinitions[structName];
            protosContent += 'message ' + structName + ' {\n';
            let idx = 1;
            for (let field of def.fields) {
                let protoType = this.TransformProtoType(field.type);
                protosContent += '  ' + protoType + ' ' + field.name + ' = ' + idx + ';\n';
                idx++;
            }
            protosContent += '}\n\n';
        }

        await this.SaveProtosToFile(protosContent, path.join(this.outputPathProtosStr, "Common"));
    }

    private TransformProtoType(type: string): string {
        switch (type) {
            case 'int': case 'Int': return 'int32';
            case 'float': case 'Float': return 'float';
            case 'string': case 'String': return 'string';
            case 'bool': case 'Bool': return 'bool';
            case 'int[]': return 'repeated int32';
            default: return type;
        }
    }

    private async TransferEnumProtos(): Promise<void> {
        let enumDefPath = path.join(__dirname, '..', '..', '..', 'design', 'define', 'Enum.xlsx');
        let enumHelper = new BaseTranslateEnum();
        await enumHelper.TranslateExcel(enumDefPath);

        let protoContent = ProtoDefine.syntax;
        protoContent += ProtoDefine.packageStart;
        for (let enumName in enumHelper.enumDefinitions) {
            let def = enumHelper.enumDefinitions[enumName];
            protoContent += 'enum ' + enumName + ' {\n';
            for (let fieldName in def.fields) {
                let val = def.fields[fieldName];
                protoContent += '    ' + fieldName + ' = ' + val + ';\n';
            }
            protoContent += '}\n\n';
        }

        if (protoContent.length > ProtoDefine.syntax.length + ProtoDefine.packageStart.length) {
            await this.SaveProtosToFile(protoContent, path.join(this.outputPathProtosStr, 'Enum'));
        }
    }


    private async SaveProtosToFile(data: any, filePath: string): Promise<void> {
        await writeFile(filePath + ".proto", data, { flag: 'w', encoding: 'utf8' });
    }

    private async GenCode(protoPath: string, toCode: string, outputPath: string): Promise<void> {
        let cmd = "";
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

        return new Promise((resolve, reject) => {
            cmd = ".\\lib\\protoc\\protoc.exe" + " -I " + parsedPath.dir + plugin + " --" + toCode + "_out" + outExtra + outputPath + " " + protoPath;
            console.log(cmd);
            exec(cmd, (err, stdout, stderr) => {
                if (err) {
                    reject(err);
                    return;
                }
                console.log(stdout);  // stdout为执行命令行操作后返回的正常结果
                console.log(stderr);  // stderr为执行命令行操作后返回的错误提示    
                resolve();
            });
        });
    }

}