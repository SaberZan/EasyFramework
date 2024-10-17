import xlsx from 'node-xlsx';
import path from 'path';
import fs from "fs";
import { mkdir, readdir, writeFile } from "fs/promises";
import _ from 'lodash';
import Utils from '../../Utils';
import BaseTranslate from '../BaseTranslate';
import BaseTranslateBefore from '../BaseTranslateBefore';
import { exec } from 'child_process';

export default class Xlsx2FlatBuffersBefore extends BaseTranslateBefore {

    private outputPathFbsStr: string = '';

    private outputPathBinStr: string = '';

    private outputPathCodeStr: string = '';

    private outputPathJsonStr: string = '';

    private toCode: string = "";

    private namespaceStart = "namespace CfgSpace; \n\n";

    private intArray = "table IntArray { \n\t data : [int];\n}\n\n";

    private boolArray = "table BoolArray { \n\t data : [bool];\n}\n\n";

    private floatArray = "table FloatArray { \n\t data : [float];\n}\n\n";

    private stringArray = "table StringArray { \n\t data : [string];\n}\n\n";

    private outputPathCsStr: string = '';

    public async BeforeTranslate(outputPathStr: string, params: any) : Promise<void> {
        this.outputPathFbsStr = path.join(outputPathStr, "fbs");
        this.outputPathCodeStr = path.join(outputPathStr, "code" , params.toCode);
        this.outputPathCsStr = path.join(outputPathStr, "code", "cs");
        this.toCode = params.toCode;
        if(!fs.existsSync(this.outputPathFbsStr)) {
            await mkdir(this.outputPathFbsStr, { recursive: true });
        }
        if(!fs.existsSync(this.outputPathCodeStr)) {
            await mkdir(this.outputPathCodeStr, { recursive: true });
        }
        if(!fs.existsSync(this.outputPathCsStr)) {
            await mkdir(this.outputPathCsStr, { recursive: true });
        }
        await this.TransferCommonFbs();
        let fbsPath = path.join(this.outputPathFbsStr, "Common.fbs");
        await this.GenCode(fbsPath, this.toCode, this.outputPathCodeStr); 
    }

    private async TransferCommonFbs() : Promise<void> {
        let fbsContent = this.namespaceStart;
        fbsContent += this.intArray;
        fbsContent += this.boolArray;
        fbsContent += this.floatArray;
        fbsContent += this.stringArray;
        await this.SaveFbsToFile(fbsContent, path.join(this.outputPathFbsStr, "Common"));
    }

    private async SaveFbsToFile(data: any, filePath: string) : Promise<void> {
        await writeFile(filePath + ".fbs", data, { flag: 'w', encoding: 'utf8' });
    }

    private async GenCode(fbsPath: string, toCode: string, outputPath: string): Promise<void>  {
        return new Promise((resolve, reject)=>{
            let cmd = ".\\lib\\flatc\\flatc.exe -o " + outputPath + " --" + toCode +" " + fbsPath;
            exec(cmd, (err, stdout, stderr) => {
                if(err) {
                    reject(err);
                    return;
                }
                console.log(stdout);  // stdout为执行命令行操作后返回的正常结果
                console.log(stderr);  // stderr为执行命令行操作后返回的错误提示
                resolve();
            });
        })
    }
}