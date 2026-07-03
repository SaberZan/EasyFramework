import xlsx from 'node-xlsx';
import path from 'path';
import fs from "fs";
import { mkdir, readdir, writeFile } from "fs/promises";
import _ from 'lodash';
import Utils from '../../utils';
import BaseTranslateConfig from '../BaseTranslateConfig';
import BaseTranslateBefore from '../BaseTranslateBefore';
import BaseTranslateEnum from '../BaseTranslateEnum';
import BaseTranslateStruct from '../BaseTranslateStruct';

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
            await writeFile(path.join(outputPathStr, enumName + '.ts'), tsContent, { flag: 'w', encoding: 'utf8' });
        }

        // Generate TS struct class files from Struct.xlsx
        let structPath = path.join(params.designPath, 'define', "Struct.xlsx");
        let structHelper = new BaseTranslateStruct();
        await structHelper.ParseStructDefinitions(structPath);
        for (let structName in structHelper.structDefinitions) {
            let def = structHelper.structDefinitions[structName];
            let content = this.BuildStructClassContent(structName, def.fields, enumHelper);
            await writeFile(path.join(outputPathStr, structName + 'Class.ts'), content, { flag: 'w', encoding: 'utf8' });
        }
    }

    private BuildStructClassContent(structName: string, fields: { name: string; type: string }[], enumHelper: BaseTranslateEnum): string {
        let content = 'export class ' + structName + ' {\n';
        for (let field of fields) {
            let keyLower = Utils.GetFristUpperAndLowerStr(field.name)[1];
            let tsType = this.TransformTsType(field.type);
            let defVal = this.DefaultValueForType(tsType, enumHelper);
            content += '    public ' + keyLower + ' : ' + tsType + ' = ' + defVal + ';\n';
        }
        content += '}\n';
        return content;
    }

    private TransformTsType(type: any): string {
        if (typeof type !== 'string') return 'any';
        let baseType = type.replace('[]', '').replace('[,]', '');
        let tsBase: string;
        switch (baseType) {
            case 'int':
            case 'Int':
            case 'float':
            case 'Float':
                tsBase = 'number';
                break;
            case 'bool':
            case 'Bool':
            case 'boolen':
            case 'Boolen':
            case 'boolean':
            case 'Boolean':
                tsBase = 'boolean';
                break;
            case 'string':
            case 'String':
                tsBase = 'string';
                break;
            case 'json':
            case 'Json':
                tsBase = 'any';
                break;
            default:
                tsBase = baseType;
                break;
        }
        if (type.includes('[,]')) return tsBase + '[][]';
        if (type.includes('[]')) return tsBase + '[]';
        return tsBase;
    }

    private DefaultValueForType(tsType: string, enumHelper: BaseTranslateEnum): string {
        if (tsType.endsWith('[]')) return '[]';
        switch (tsType) {
            case 'number': return '0';
            case 'string': return '""';
            case 'boolean': return 'false';
            case 'any': return 'null';
            default:
                if (enumHelper && enumHelper.enumDefinitions && enumHelper.enumDefinitions[tsType]) {
                    return '0 as ' + tsType;
                }
                return 'new ' + tsType + '()';
        }
    }
}