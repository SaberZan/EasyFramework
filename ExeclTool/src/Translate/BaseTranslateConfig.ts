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
     * 定位表中的“类型行”。
     * 常规布局是：第 0 行字段名，第 1 行类型，第 2 行注释，第 3 行起为数据。
     * 部分 CSV 会把注释行和类型行互换，因此这里检查前几行，
     * 返回最像类型行的那一行。可识别的写法包括：
     * 基础类型（int/float/string/bool/json）、数组（"[]"/"[,]"）、
     * 结构子字段（"name:string"）、内联结构（"{name:string,value:int}"）。
     * 找不到更优候选时默认返回 1。
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
                // 内联结构定义，如 {name:string,value:int}
                if (/^\{.*\}(\[\])?$/.test(trimmed)) { score += 2; continue; }
                // 结构子字段带类型，如 name:string
                if (/^\w+\s*:\s*\w+(\[\])?$/.test(trimmed)) { score += 2; continue; }
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
