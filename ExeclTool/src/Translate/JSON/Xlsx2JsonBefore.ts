import xlsx from 'node-xlsx';
import path from 'path';
import fs from "fs";
import { mkdir, readdir, writeFile } from "fs/promises";
import _ from 'lodash';
import Utils from '../../utils';
import BaseTranslateConfig from '../BaseTranslateConfig';
import BaseTranslateBefore from '../BaseTranslateBefore';
import BaseTranslateEnum from '../BaseTranslateEnum';

export default class Xlsx2JsonBefore extends BaseTranslateBefore {
    public async BeforeTranslate(outputPathStr: string, params: any) : Promise<void> {
        outputPathStr = path.join(outputPathStr , "json");
        if(!fs.existsSync(outputPathStr)) {
            await mkdir(outputPathStr, { recursive: true });
        }

        let enumPath = path.join(params.designPath, 'define', "Enum.xlsx");
        let enumHelper = new BaseTranslateEnum();
        await enumHelper.TranslateExcel(enumPath);

        // Generate JSON for enum definitions
        let enumJsonDir = path.join(outputPathStr, 'json');
        if (!fs.existsSync(enumJsonDir)) {
            await mkdir(enumJsonDir, { recursive: true });
        }
        
        for (let enumName in enumHelper.enumDefinitions) {
            let def = enumHelper.enumDefinitions[enumName];
            let enumJson: any = {};
            for (let fieldName in def.fields) {
                let val = def.fields[fieldName];
                let numVal = parseInt(val.toString());
                enumJson[fieldName] = isNaN(numVal) ? val : numVal;
            }
            await this.SaveJsonToFile(enumJson, path.join(enumJsonDir, enumName));
        }
    }

    private async SaveJsonToFile(data: any, filePath: string): Promise<void> {
        var _str_all = '';
        _str_all += JSON.stringify(data, null, 4);
        return writeFile(filePath + '.json', _str_all, { flag: 'w', encoding: 'utf8' });
    }
}