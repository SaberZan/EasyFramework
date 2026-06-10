import xlsx from 'node-xlsx';
import path from 'path';
import fs from "fs";
import { mkdir, readdir, writeFile } from "fs/promises";
import _ from 'lodash';
import Utils from '../../utils';
import CSDefine from '../CS/CSDefine';
import BaseTranslateConfig from '../BaseTranslateConfig';
import BaseTranslateBefore from '../BaseTranslateBefore';
import BaseTranslateStruct from '../BaseTranslateStruct';
import BaseTranslateEnum from '../BaseTranslateEnum';


export default class Xlsx2CsBefore extends BaseTranslateBefore {

    public async BeforeTranslate(outputPathStr: string, params: any) : Promise<void> {
        let outputPathCsStr = path.join(outputPathStr, "code", "cs");
        let outputPathJsonStr = path.join(outputPathStr, "json");

        if(!fs.existsSync(outputPathCsStr)) {
            await mkdir(outputPathCsStr, { recursive: true });
        }
        if(!fs.existsSync(outputPathJsonStr)) {
            await mkdir(outputPathJsonStr, { recursive: true });
        }

        let enumPath = path.join(params.designPath, 'define', "Enum.xlsx");
        let enumHelper = new BaseTranslateEnum();
        await enumHelper.TranslateExcel(enumPath);

        // Generate CS enum files
        let enumCsDir = path.join(outputPathStr, 'code', 'cs');
        if (!fs.existsSync(enumCsDir)) {
            await mkdir(enumCsDir, { recursive: true });
        }
        for (let enumName in enumHelper.enumDefinitions) {
            let def = enumHelper.enumDefinitions[enumName];
            let csContent = CSDefine.notes.replace('{0}', enumName);
            csContent += '\tpublic enum ' + enumName + ' {\r\n';
            let first = true;
            for (let fieldName in def.fields) {
                if (!first) csContent += ',\r\n';
                csContent += '\t\t' + fieldName + ' = ' + def.fields[fieldName];
                first = false;
            }
            if (!first) csContent += '\r\n';
            csContent += '\t}\r\n';
            await writeFile(path.join(enumCsDir, enumName + '.cs'), csContent, { flag: 'w', encoding: 'utf8' });
        }
        let structPath = path.join(params.designPath, 'define', "Struct.xlsx");
        let structHelper = new BaseTranslateStruct();
        await structHelper.ParseStructDefinitions(structPath);

        if(!fs.existsSync(outputPathCsStr)) {
            await mkdir(outputPathCsStr, { recursive: true });
        }
        // Generate standalone struct CS files from Struct.xlsx
        for (let structName in structHelper.structDefinitions) {
            let structDef = structHelper.structDefinitions[structName];
            let content = this.BuildStructClassContent(structName, structDef.fields);
            await writeFile(path.join(outputPathCsStr, structName) + ".cs", content, { flag: 'w', encoding: 'utf8' });
        }
    }

    private BuildStructClassContent(structName: string, fields: { name: string; type: string }[]): string {
        let content = CSDefine.notes.replace('{0}', structName);
        content += CSDefine.classStart.replace('{0}', structName);
        for (let field of fields) {
            let keyUpperAndLower = Utils.GetFristUpperAndLowerStr(field.name);
            let keyUpper = keyUpperAndLower[0];
            let keyLower = keyUpperAndLower[1];
            let fieldType = this.TransformType(field.type);
            content += Utils.FormatStr(CSDefine.privateStr, fieldType, keyLower, field.name);
            content += Utils.FormatStr(CSDefine.publicStr, fieldType, keyUpper, keyLower);
        }
        content += CSDefine.classEnd;
        return content;
    }

    private TransformType(type: any) {
        if (typeof type !== 'string') return type;
        let result;
        switch (type) {
            case 'int':
            case 'Int':
                result = 'int';
                break;
            case 'float':
            case 'Float':
                result = 'float';
                break;
            case 'bool':
            case 'Bool':
            case 'boolen':
            case 'Boolen':
                result = 'bool';
                break;
            case 'string':
            case 'String':
                result = 'string';
                break;
            default:
                 result = type;
                break;
        }
        return result;
    }
}