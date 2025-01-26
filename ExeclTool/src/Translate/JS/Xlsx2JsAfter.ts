import xlsx from 'node-xlsx';
import path from 'path';
import fs from "fs";
import { mkdir, readdir, writeFile } from "fs/promises";
import _ from 'lodash';
import Utils from '../../utils';
import BaseTranslate from '../BaseTranslate';
import BaseTranslateAfter from '../BaseTranslateAfter';

export default class Xlsx2JsAfter extends BaseTranslateAfter {

    public async AfterTranslate(tmpCfgPath: string) : Promise<void> {
        await super.AfterTranslate(tmpCfgPath);
        await this.genFile(path.join(tmpCfgPath,"js"));
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

    private async genJSIndexFile (root: string, files_js: string[], files_dir: string[]) : Promise<void> {
        let _format_head: string[] = [
            "/**",
            "DO NOT EDIT IT!! ",
            "*/",
            "\n",
            "var moduleBase = {};"
        ];
        let _format_tail: string[] = [
            "module.exports = moduleBase ;"
        ];
        let _format_before_this_module: string = "//当前模块";
        let _format_before_sub_module: string = "//子模块";

        let _format_js: string  = "moduleBase.$1 = require(\"./$1.js\");";
        let _format_dir: string = "moduleBase.$1 = require(\"./$1/Index.js\");";

        let _contents: string[] = [];
        _contents = _contents.concat(_format_head);
        _contents.push("\n");
        _contents = _contents.concat(_format_before_this_module);
        files_js.forEach(function(item){
            let _basename: string = path.basename(item,".js");
            _contents.push(_format_js.replace("$1",_basename).replace("$1",_basename))
        });
        _contents.push("\n");
        _contents.push(_format_before_sub_module);
        files_dir.forEach(function(item){
            let _basename: string = path.basename(item,"");
            _contents.push(_format_dir.replace("$1",_basename).replace("$1",_basename))
        });
        _contents.push("\n");
        _contents = _contents.concat(_format_tail);

        var _str_contents: string = "";
        _contents.forEach(function (item) {
            _str_contents += ( item + "\n" );
        });

        let _file_name:string = "index.js";
        _file_name  = _file_name[0].toUpperCase() + _file_name.substr(1);
        let _headers_file:string = path.join(root, _file_name);

        await writeFile(_headers_file, _str_contents);
    }

    public async genFile (jsPath: string) : Promise<void> {
        jsPath = jsPath;
        let files = await readdir(jsPath);
        console.log("jsPath == " + jsPath + "  " + files.length)

        let fileList_js:string[]= [];
        let fileList_dir:string[]= [];
        for(let i in files) {
            let fileName = files[i];
            console.log("filename == " + fileName)
            var _full_path = path.join(jsPath, fileName);
            if(fs.statSync(_full_path).isDirectory()){
                //屏蔽目录
                if(!this.isIgnore(_full_path,this._ignore_arr) ){
                    fileList_dir.push(_full_path);
                    await this.genFile(_full_path);
                }
            }else{
                if(!this.isIgnore(_full_path,this._ignore_arr) && (fileName.indexOf(".js") > 0)){
                    fileList_js.push(_full_path);
                }
            }
        }
        this.genJSIndexFile(jsPath,fileList_js,fileList_dir);
    }

    /// ------------------生成Index文件-----------------end
}