import fs from "fs";
import _ from "lodash";
import xlsx from 'node-xlsx';
import CsvParser from '../CsvParser';

export interface EnumDefinition {
    name: string;
    fields: {[key :string] :number};
}


export default class BaseTranslateEnum {

    public enumDefinitions: { [name: string]: EnumDefinition } = {};
    
    /**
     * Parse enum definitions from Excel or CSV files
     */
    public async TranslateExcel(definePath: string) : Promise<void> {
        let data: any[] = [];
        
        // Try CSV first, then fall back to xlsx
        if (CsvParser.canParseAsCsv(definePath)) {
            data = CsvParser.parse(definePath);
        }
        
        // If no CSV data found, try xlsx
        if (data.length === 0 && fs.existsSync(definePath)) {
            data = xlsx.parse(definePath);
        }

        for (let sheet of data) {
            let enumName = sheet.name;
            let sheetData = sheet.data;

            if (sheetData.length < 2) {
                continue;
            }

            let fields: {[key :string] :number} = {};

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
