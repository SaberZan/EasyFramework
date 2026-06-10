import xlsx from 'node-xlsx';
import path from 'path';
import fs from "fs";
import { mkdir, readdir, writeFile } from "fs/promises";
import _ from 'lodash';
import Utils from '../../utils';
import BaseTranslateConfig from '../BaseTranslateConfig';
import BaseTranslateBefore from '../BaseTranslateBefore';
import BaseTranslateEnum from '../BaseTranslateEnum';

export default class Xlsx2JsBefore extends BaseTranslateBefore {
    public async BeforeTranslate(outputPathStr: string, params: any) : Promise<void> {
        outputPathStr = path.join(outputPathStr , "js");
        if(!fs.existsSync(outputPathStr)) {
            await mkdir(outputPathStr, { recursive: true });
        }

        // 解析子结构定义 - 支持自定义路径
        let enumPath = path.join(params.designPath, 'define', "Enum.xlsx");
        let enumHelper = new BaseTranslateEnum();
        await enumHelper.TranslateExcel(enumPath);
        //        // Generate JS enum files
        let enumJsDir = path.join(outputPathStr, 'js');
        if (!fs.existsSync(enumJsDir)) {
            await mkdir(enumJsDir, { recursive: true });
        }
        for (let enumName in enumHelper.enumDefinitions) {
            let def = enumHelper.enumDefinitions[enumName];
            let jsContent = 'var ' + enumName + ' = {\n';
            let parts: string[] = [];
            for (let fieldName in def.fields) {
                parts.push('    ' + fieldName + ': ' + def.fields[fieldName]);
            }
            jsContent += parts.join(',\n');
            if (parts.length > 0) jsContent += '\n';
            jsContent += '};\n';
            await writeFile(path.join(enumJsDir, enumName + '.js'), jsContent, { flag: 'w', encoding: 'utf8' });
        }

    }
}