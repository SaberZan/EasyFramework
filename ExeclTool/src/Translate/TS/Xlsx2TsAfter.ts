import xlsx from 'node-xlsx';
import path from 'path';
import fs from "fs";
import { mkdir, readdir, writeFile } from "fs/promises";
import _ from 'lodash';
import Utils from '../../utils';
import BaseTranslateConfig from '../BaseTranslateConfig';
import BaseTranslateAfter from '../BaseTranslateAfter';

export default class Xlsx2TsAfter extends BaseTranslateAfter {

    public async AfterTranslate(tmpCfgPath: string) : Promise<void> {
        await super.AfterTranslate(tmpCfgPath);
        await this.genIndex(path.join(tmpCfgPath,"ts"));
    }

    /// ------------------生成Index文件-----------------start
    private _ignore_arr: string[] = [
        "/.",
        "tools",
        ".git",
        "node_modules",
        "index.js",
        "Index.js",
        "index.ts",
        "Index.ts"
    ];

    private isIgnore(fileName: string ,ignore_dir: string[]){
        for(var i = 0 ; i < ignore_dir.length ; i++ ){
            var item = ignore_dir[i];
            if(fileName.indexOf(item) >= 0){
                return true;
            }
        }
        return false;
    }
                      
    private async genTsIndexFile (root: string, files_ts: string[], files_dir: string[]) : Promise<void>{

        let _format_head: string[] = [
            "/**",
            "DO NOT EDIT IT!! ",
            "*/",
            "\n"
        ];

        let _format_before_this_module: string = "//当前模块";
        let _format_before_sub_module: string = "//子模块";

        let _format_ts: string  = "export {default as $1} from \'./$1\';";
        let _format_dir: string = "export * as $1 from \'./$1/Index\';";

        let _contents: string[] = [];
        _contents = _contents.concat(_format_head);
        _contents.push("\n");
        _contents = _contents.concat(_format_before_this_module);
        files_ts.forEach(function(item){
            let _basename: string = path.basename(item,".ts");
            _contents.push(_format_ts.replace("$1",_basename).replace("$1",_basename))
        });
        _contents.push("\n");
        _contents.push(_format_before_sub_module);
        files_dir.forEach(function(item){
            let _basename: string = path.basename(item,"");
            _contents.push(_format_dir.replace("$1",_basename).replace("$1",_basename))
        });

        var _str_contents: string = "";
        _contents.forEach(function (item) {
            _str_contents += ( item + "\n" );
        });

        let _file_name:string = "index.ts";
        _file_name  = _file_name[0].toUpperCase() + _file_name.substr(1);
        let _headers_file:string = path.join(root, _file_name);

        await writeFile(_headers_file, _str_contents);
    }

    public async genIndex (tmpCfgPath: string){
        let tsPath = tmpCfgPath;
        let files = await readdir(tsPath);

        let fileList_js:string[]= [];
        let fileList_dir:string[]= [];
        for(let i in files) {
            let fileName = files[i];
            var _full_path = tsPath + '/' + fileName;
            if(fs.statSync(_full_path).isDirectory()){
                //屏蔽目录
                if(!this.isIgnore(_full_path,this._ignore_arr) ){
                    fileList_dir.push(_full_path);
                    await this.genIndex(_full_path);
                }
            }else{
                if(!this.isIgnore(_full_path,this._ignore_arr) && (fileName.indexOf(".ts") > 0)){
                    fileList_js.push(_full_path);
                }
            }
        }
        await this.genTsIndexFile(tsPath,fileList_js,fileList_dir);
    }

    /// ------------------生成Index文件-----------------end
}