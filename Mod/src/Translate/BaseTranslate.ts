import fs from "fs";

export default class BaseTranslate {

    public xlsxData: { [Key: string]: any } = {};

    public translateSheets: any;

    public get merge(): boolean {
        return this.mergeName != undefined;
    }

    public mergeName: string = "";

    public toDir: string = "";

    public isDir: boolean = false;


    public BeforeTranslate(outputPathStr: string, params: any) {

    }

    /**
     * 
     * @param pathStr 表的路径
     * @param outputPathStr 输出路径
     * @param translate 转换规则
     * @param params 转换参数
     */
    public TranslateExcel(pathStr: string, outputPathStr: string, translate: any, params: any) {

        this.translateSheets = translate.sheets;
        this.mergeName = translate.mergeName;
        this.toDir = translate.toDir;
        this.isDir = false;
        if (fs.existsSync(pathStr) && fs.statSync(pathStr).isDirectory()) { 
            this.isDir = true;
        }
    }

    public GenIndex(tmpCfgPath: string) {

    }

}