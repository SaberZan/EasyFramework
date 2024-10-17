import xlsx from 'node-xlsx';
import path from 'path';
import fs from "fs";
import { mkdir, readdir, writeFile } from "fs/promises";
import _, { reject } from 'lodash';
import { exec } from 'child_process'
import Utils from '../../Utils';
import protobuf from 'protobufjs'
import { pbjs, pbts } from 'protobufjs-cli';
import BaseTranslateBefore from '../BaseTranslateBefore';

export default class Xlsx2ProtoBuffersBefore extends BaseTranslateBefore {

    private outputPathProtosStr: string = '';

    private outputPathBytesStr: string = '';

    private outputPathCodeStr: string = '';

    private outputPathJsonStr: string = '';

    private toCode: string = "";

    private syntax = "syntax = \"proto3\"; \n\n";

    private packageStart = "package CfgSpace; \n\n";

    private intArray = "message IntArray {\n\t repeated int32 data = 1;\n}\n\n";

    private boolArray = "message BoolArray {\n\t repeated bool data = 1;\n}\n\n";

    private floatArray = "message FloatArray {\n\t repeated float data = 1;\n}\n\n";

    private stringArray = "message StringArray {\n\t repeated string data = 1;\n}\n\n";


    public async BeforeTranslate(outputPathStr: string, params: any) : Promise<void> {
        this.outputPathProtosStr = path.join(outputPathStr , "protos");
        this.outputPathCodeStr = path.join(outputPathStr , "code" , params.toCode);
        this.toCode = params.toCode;
        if(!fs.existsSync(this.outputPathProtosStr)) {
            await mkdir(this.outputPathProtosStr, { recursive: true });
        }
        if(!fs.existsSync(this.outputPathCodeStr)) {
            await mkdir(this.outputPathCodeStr, { recursive: true });
        }
        await this.TransferCommonProtos();
        let protoPath = path.join(this.outputPathProtosStr, "Common.proto");
        await this.GenCode(protoPath, this.toCode, this.outputPathCodeStr); 
    }

    private async TransferCommonProtos() : Promise<void> {
        let protosContent = this.syntax;
        protosContent += this.packageStart;
        protosContent += this.intArray;
        protosContent += this.boolArray;
        protosContent += this.floatArray;
        protosContent += this.stringArray;
        await this.SaveProtosToFile(protosContent, path.join(this.outputPathProtosStr, "Common"));
    }

    private async SaveProtosToFile(data: any, filePath: string) : Promise<void> {
        await writeFile(filePath + ".proto", data, { flag: 'w', encoding: 'utf8' });
    }

    private async GenCode(protoPath: string, toCode: string, outputPath: string) : Promise<void> {
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

                return new Promise((resolve,reject)=>{
                    pbjs.main(["-t", "static-module", "-w","commonjs", "-o", jsPath,  protoPath, "-p", path.dirname(protoPath)],(err, output)=>{
                        if(err){
                            console.log("err == " + err);
                            reject(err);
                            return;
                        }
                        pbts.main(["-o",dtsPath, jsPath]);
                        resolve();
                    });
                })

            default:
                return new Promise((resolve,reject)=>{
                    cmd = ".\\lib\\protoc\\protoc.exe" + " -I " + parsedPath.dir +  " --" + toCode +"_out " + outputPath + " " + protoPath;
                    console.log(cmd);
                    exec(cmd, (err, stdout, stderr) => {
                        if(err) {
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
}