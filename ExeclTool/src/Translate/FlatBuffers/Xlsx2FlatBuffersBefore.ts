import xlsx from 'node-xlsx';
import path from 'path';
import fs from "fs";
import { mkdir, readdir, writeFile } from "fs/promises";
import _ from 'lodash';
import Utils from '../../utils';
import FbsDefine from './FbsDefine';
import BaseTranslateEnum from '../BaseTranslateEnum';
import BaseTranslateConfig from '../BaseTranslateConfig';
import BaseTranslateBefore from '../BaseTranslateBefore';
import BaseTranslateStruct from '../BaseTranslateStruct'
import { exec } from 'child_process';

export default class Xlsx2FlatBuffersBefore extends BaseTranslateBefore {

    private outputPathFbsStr: string = '';

    private outputPathBinStr: string = '';

    private outputPathCodeStr: string = '';

    private outputPathJsonStr: string = '';

    private toCode: string = "";



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
        await this.TransferEnumFbs();
        let fbsPath = path.join(this.outputPathFbsStr, "Common.fbs");
        await this.GenCode(fbsPath, this.toCode, this.outputPathCodeStr);
        let enumFbsPath = path.join(this.outputPathFbsStr, "Enum.fbs");
        await this.GenCode(enumFbsPath, this.toCode, this.outputPathCodeStr); 
    }

    private async TransferCommonFbs() : Promise<void> {
        let fbsContent = FbsDefine.namespaceStart;
        fbsContent += FbsDefine.intArray;
        fbsContent += FbsDefine.boolArray;
        fbsContent += FbsDefine.floatArray;
        fbsContent += FbsDefine.stringArray;

        // Add struct definitions from Struct.xlsx
        let structDefPath = path.join(__dirname, '..', '..', '..', 'design', 'define', 'Struct.xlsx');
        let structHelper = new BaseTranslateStruct();
        await structHelper.ParseStructDefinitions(structDefPath);
        for (let structName in structHelper.structDefinitions) {
            let def = structHelper.structDefinitions[structName];
            fbsContent += 'table ' + structName + ' {\n';
            for (let field of def.fields) {
                fbsContent += '    ' + field.name + ' : ' + field.type + ';\n';
            }
            fbsContent += '}\n\n';
        }

        await this.SaveFbsToFile(fbsContent, path.join(this.outputPathFbsStr, "Common"));
    }

    private async SaveFbsToFile(data: any, filePath: string) : Promise<void> {
        await writeFile(filePath + ".fbs", data, { flag: 'w', encoding: 'utf8' });
    }

    private async TransferEnumFbs(): Promise<void> {
        let enumDefPath = path.join(__dirname, '..', '..', '..', 'design', 'define', 'Enum.xlsx');
        let enumHelper = new BaseTranslateEnum();
        await enumHelper.TranslateExcel(enumDefPath);
        
        let fbsContent = FbsDefine.namespaceStart;
        for (let enumName in enumHelper.enumDefinitions) {
            let def = enumHelper.enumDefinitions[enumName];
            fbsContent += 'enum ' + enumName + ' : int {\n';
            let first = true;
            for (let fieldName in def.fields) {
                let val = def.fields[fieldName];
                if (!first) fbsContent += ',\n';
                fbsContent += '    ' + fieldName + ' = ' + (val != null ? val : '0');
                first = false;
            }
            if (!first) fbsContent += '\n';
            fbsContent += '}\n\n';
        }
        
        if (fbsContent) {
            await this.SaveFbsToFile(fbsContent, path.join(this.outputPathFbsStr, 'Enum'));
        }
    }


    private async GenCode(fbsPath: string, toCode: string, outputPath: string): Promise<void>  {
        return new Promise((resolve, reject)=>{
            let cmd = ".\\lib\\flatc\\flatc.exe -o " + outputPath + " --" + toCode +" " + fbsPath;
            exec(cmd, (err, stdout, stderr) => {
                if(err) {
                    reject(err);
                    return;
                }
                console.log(stdout);  // stdout涓烘墽琛屽懡浠よ鎿嶄綔鍚庤繑鍥炵殑姝ｅ父缁撴灉
                console.log(stderr);  // stderr涓烘墽琛屽懡浠よ鎿嶄綔鍚庤繑鍥炵殑閿欒鎻愮ず
                resolve();
            });
        })
    }
}
