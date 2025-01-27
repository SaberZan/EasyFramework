import fs from "fs";

export default class BaseTranslateConfig {

    public xlsxData: { [Key: string]: any } = {};

    public translateSheets: any;

    /**
     * 
     * @param pathStr 表的路径
     * @param outputPathStr 输出路径
     * @param translate 转换规则
     * @param params 转换参数
     */
    public async TranslateExcel(pathStr: string, outputPathStr: string, translate: any, params: any) : Promise<void> {
        this.translateSheets = translate.sheets;
    }
}