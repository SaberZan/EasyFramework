import xlsx from 'node-xlsx';
import path from 'path';
import fs from "fs";
import { mkdir, readdir, writeFile } from "fs/promises";
import _ from 'lodash';
import Utils from '../../utils';
import BaseTranslateConfig from '../BaseTranslateConfig';
import BaseTranslateBefore from '../BaseTranslateBefore';
import BaseTranslateEnum from '../BaseTranslateEnum';

export default class Xlsx2TsBefore extends BaseTranslateBefore {
    public async BeforeTranslate(outputPathStr: string, params: any) : Promise<void> {
        outputPathStr = path.join(outputPathStr , "ts");
        if(!fs.existsSync(outputPathStr)) {
            await mkdir(outputPathStr, { recursive: true });
        }

        // Preprocess enum types - generate TS enum files
        let enumPath = path.join(params.designPath, 'define', "Enum.xlsx");
        let enumHelper = new BaseTranslateEnum();
        await enumHelper.TranslateExcel(enumPath);
        //        // Generate TS enum files
        let enumTsDir = path.join(outputPathStr, 'ts');
        if (!fs.existsSync(enumTsDir)) {
            await mkdir(enumTsDir, { recursive: true });
        }
        for (let enumName in enumHelper.enumDefinitions) {
            let def = enumHelper.enumDefinitions[enumName];
            let tsContent = 'export enum ' + enumName +' {\n';
            let parts: string[] = [];
            for (let fieldName in def.fields) {
                parts.push('    ' + fieldName + ' = ' + def.fields[fieldName]);
            }
            tsContent += parts.join(',\n');
            if (parts.length > 0) tsContent += '\n';
            tsContent += '}\n';
            tsContent += 'export default ' + enumName + ';\n';
            await writeFile(path.join(enumTsDir, enumName + '.ts'), tsContent, { flag: 'w', encoding: 'utf8' });
        }
        
    }
}