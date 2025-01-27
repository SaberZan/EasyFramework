import fs from "fs";

export default class BaseTranslateConfig {

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
    }
}