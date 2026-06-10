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
        console.log("-1- isDir " + this.isDir)
        if (fs.existsSync(pathStr) && fs.statSync(pathStr).isDirectory()) { 
            this.isDir = true;
            console.log("-2- isDir " + this.isDir)
        }

        let enumPath = path.join(params.designPath, 'define', "Enum.xlsx");
        await this.enumHelper.TranslateExcel(enumPath);

        let structPath = path.join(params.designPath, 'define', "Struct.xlsx");
        await this.structHelper.ParseStructDefinitions(structPath);
    }
}
