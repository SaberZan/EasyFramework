import fs from "fs";
import path from "path";

export default class BaseTranslateConfig {


    private _enumHelper: any = null;
    private _structHelper: any = null;

    public get enumHelper(): any {
        if (!this._enumHelper) {
            const m = require("./BaseTranslateEnum");
            this._enumHelper = new m.default();
        }
        return this._enumHelper;
    }

    public get structHelper(): any {
        if (!this._structHelper) {
            const m = require("./BaseTranslateStruct");
            this._structHelper = new m.default();
        }
        return this._structHelper;
    }
    public xlsxData: { [Key: string]: any } = {};

    public translateSheets: any;

    public get merge(): boolean {
        return this.mergeName != undefined;
    }

    public mergeName: string = "";

    public toDir: string = "";

    public isDir: boolean = false;



    /**
     * 
     * @param pathStr 表的路径
     * @param outputPathStr 输出路径
     * @param translate 转换规则
     * @param params 转换参数
     */
    public async TranslateExcel(pathStr: string, outputPathStr: string, translate: any, params: any) : Promise<void> {

        this.translateSheets = translate.sheets;
        this.mergeName = translate.mergeName;
        this.toDir = translate.toDir;
        this.isDir = false;

        if(params.format == "xlsx") {
            if (fs.existsSync(pathStr) && fs.statSync(pathStr).isDirectory()) { 
                this.isDir = true;
                console.log("-2- isDir " + this.isDir)
            }
        }
        else if(params.format == "csv") {
            if (fs.existsSync(pathStr) && fs.statSync(pathStr).isDirectory()) { 
                this.isDir = true;
                let allFiles = fs.readdirSync(pathStr);
                for(var file of allFiles) {
                    if(fs.statSync( path.join(pathStr,file)).isFile()) {
                        this.isDir = false;
                        break;
                    }
                }
            }
        }
        console.log("-1- isDir " + this.isDir)

        let enumPath = path.join(params.designPath, 'define', "Enum.xlsx");
        await this.enumHelper.TranslateExcel(enumPath);

        let structPath = path.join(params.designPath, 'define', "Struct.xlsx");
       await this.structHelper.ParseStructDefinitions(structPath);
   }

    /**
     * Detect the index of the "type" row within a sheet's data.
     * Sheets normally use row 0 = field names, row 1 = types, row 2 = comments, row 3+ = data.
     * Some CSVs swap rows 1 and 2 (comments first, types second). This helper inspects the
     * first few rows and returns the index whose cells look like real field types
     * (int/float/string/bool/json, or anything containing "[]"/"[,]", or a known struct/enum name).
     * Returns 1 by default when no better candidate is found.
     */
    public FindTypeRowIndex(data: any): number {
        if (!data || data.length < 2) return 1;
        let knownTypes = ['int', 'Int', 'float', 'Float', 'bool', 'Bool', 'boolean', 'Boolean', 'string', 'String', 'json', 'Json'];
        let bestRow = 1;
        let bestScore = -1;
        let limit = Math.min(data.length, 3);
        for (let r = 1; r < limit; ++r) {
            let row = data[r] || [];
            let score = 0;
            for (let c = 0; c < row.length; ++c) {
                let cell = row[c];
                if (typeof cell !== 'string') continue;
                let trimmed = cell.trim();
                if (knownTypes.indexOf(trimmed) >= 0) { score += 2; continue; }
                if (trimmed.includes('[]') || trimmed.includes('[,]')) { score += 2; continue; }
            }
            if (score > bestScore) { bestScore = score; bestRow = r; }
        }
        return bestRow;
    }

    /**
     * Detect the index of the first data row. This is normally one row after the type row,
     * so a sheet with [names, types, comments] starts data at index 3.
     */
    public FindFirstDataRow(data: any, typeRowIndex: number): number {
        return typeRowIndex + 2;
    }
}
