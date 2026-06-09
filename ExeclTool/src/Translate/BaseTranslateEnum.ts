import fs from "fs";
import _ from "lodash";
import xlsx from 'node-xlsx';

export interface EnumDefinition {
    name: string;
    fields: {[key :string] : number};
}


export default class BaseTranslateEnum {

    public enumDefinitions: { [name: string]: EnumDefinition } = {};
    
    /**
     * 
     * @param pathStr 表的路径
     * @param outputPathStr 输出路径
     * @param translate 转换规则
     * @param params 转换参数
     */
    public async TranslateExcel(definePath: string) : Promise<void> {
        if (!fs.existsSync(definePath)) {
            console.warn('Struct define path not found:', definePath);
            return;
        }

        let data = xlsx.parse(definePath);

        for (let sheet of data) {
            let enumName = sheet.name;
            let sheetData = sheet.data;

            if (sheetData.length < 2) {
                continue;
            }

            let fields: {[key :string] : number} = {};

            for (let row = 1; row < sheetData.length; ++row) {
                let fieldName = sheetData[row][0];
                let fieldValue = sheetData[row][1];
                if (_.isNil(fieldName) || _.isEmpty(fieldName)) {
                    continue;
                }
                fields[fieldName] = fieldValue;
            }

            this.enumDefinitions[enumName] = {
                name: enumName,
                fields: fields
            };
        }
    }

    public IsEnumType(type: string) : boolean {
        if (typeof type !== 'string') return false;
        let baseType = type.replace('[]', '').replace('[,]', '');
        return this.enumDefinitions[baseType] !== undefined;
    }
}