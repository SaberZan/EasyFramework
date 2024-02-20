import xlsx from 'node-xlsx';
import path from 'path';
import fs from "fs";
import _ from 'lodash';
import Utils from '../../Utils';
import BaseTranslate from '../BaseTranslate';

export default class Xlsx2Js extends BaseTranslate {

    private outputJsPathStr: string = '';

    public TranslateExcel(pathStr: string, outputPathStr: string, translate: any, params: any) {

        super.TranslateExcel(pathStr,outputPathStr,translate,params);

        this.outputJsPathStr = path.join(outputPathStr , "js");
        if(!fs.existsSync(this.outputJsPathStr)) {
            fs.mkdirSync(this.outputJsPathStr, { recursive: true });
        }

        if(this.toDir != undefined) {
            this.outputJsPathStr = path.join(this.outputJsPathStr , this.toDir);
            if(!fs.existsSync(this.outputJsPathStr)) {
                fs.mkdirSync(this.outputJsPathStr, { recursive: true });
            }
        }
 
        if (this.isDir) { 
            let files = fs.readdirSync(pathStr);
            for(let i in files) {
                let data = xlsx.parse(path.join(pathStr, files[i]));
                for (let i = 0; i < data.length; ++i) {
                    this.xlsxData[data[i].name] = data[i].data;
                }
                this.TransferTable(files[i].replace(path.extname(files[i]),""));
            }
        } else {
            let parsedPath = path.parse(pathStr);
            parsedPath.base += ".xlsx";
            parsedPath.ext = ".xlsx";
            let data = xlsx.parse(path.format(parsedPath));
            for (let i = 0; i < data.length; ++i) {
                this.xlsxData[data[i].name] = data[i].data;
            }
            this.TransferTable();
        }
    }

    private TransferTable(file: string = "") {
        if(this.merge) {
            let all: {[key: string]: any} = {};
            for (let i = 0; i < this.translateSheets.length; ++i) {
                let sheetName = this.translateSheets[i][0];
                let translateName = this.translateSheets[i][1];
                let jsonData = this.CreateJson(this.xlsxData[sheetName]);
                all[translateName] = jsonData;
            }
            this.saveJsToFile(all, path.join(this.outputJsPathStr,this.mergeName + file));
        }else{
            for (let i = 0; i < this.translateSheets.length; ++i) {
                let sheetName = this.translateSheets[i][0];
                let translateName = this.translateSheets[i][1];
                let jsonData = this.CreateJson(this.xlsxData[sheetName]);
                this.saveJsToFile(jsonData, path.join(this.outputJsPathStr, translateName));
            }
        }
    }

    private CreateJson(data: any) {
        let jsonOut: { [key: string]: any } = {};

        let dataArr = data;
        let keys = dataArr[0];
        let types = dataArr[1];

        //检测层级结构，有多少层级
        let layerNum = 0;
        for (let typeIndex = 0; typeIndex < types.length; ++typeIndex) {
            let type = types[typeIndex];
            if (type && type[0] == '#') {
                layerNum += 1;
            }
        }

        //若没有配置，必有一层层级
        if (layerNum === 0) {
            layerNum = 1;
        }

        for (let rowIndex = 3; rowIndex < dataArr.length; ++rowIndex) {
            let _arrLine = dataArr[rowIndex];

            if (_.isNil(_arrLine[0]) || _arrLine[0] == '') {
                continue;
            }

            // 遍历引用，方便定义对象。
            let tmp = jsonOut;
            for (let layIndex = 0; layIndex < layerNum - 1; ++layIndex) {
                if (!tmp[_arrLine[layIndex]]) {
                    tmp[_arrLine[layIndex]] = {};
                }
                tmp = tmp[_arrLine[layIndex]];
            }

            let subTmp: { [key: string]: any } = {};
            for (let colIndex = 0; colIndex < keys.length; ++colIndex) {
                let key = keys[colIndex];
                let type = types[colIndex];
                let value = _arrLine[colIndex];
                if (_.isNil(key) || _.isEmpty(key) || typeof(value) == "undefined") {
                    continue;
                }

                let result = this.TransformValue(type, _arrLine[colIndex], rowIndex, colIndex); //检测类型，传入所在的行和列，方便报错检查
                if (!_.isNil(result) && !_.isNaN(result)) {
                    let keyLower = Utils.GetFristUpperAndLowerStr(key)[1];
                    subTmp[keyLower] = result;
                }
            }

            tmp[_arrLine[layerNum - 1]] = subTmp;

        }

        let output = jsonOut;

        return output;
    }

    private saveJsToFile(data: any, filePath: string) {
        var _str_all = "";
        _str_all += '//automatic generation,DO NOT EDIT IT!\nmodule.exports =\n';
        _str_all += JSON.stringify(data, null, 4);
        _str_all += ';';
        fs.writeFileSync(filePath + ".js", _str_all, { flag: 'w', encoding: 'utf8' });
    }

    
    //翻译配置的字段到对应应该有的数据类型。
    private _TransformBasicsValue (type: string, data: any) {
        let result;
        switch (type) {
            case 'int':
            case 'Int':
                result = parseInt(data);
                break;
            case 'float':
            case 'Float':
                result = parseFloat(data);
                break;
            case 'bool':
            case 'Bool':
            case 'boolen':
            case 'Boolen':
                result = Boolean(data);
                break;
            case 'string':
            case 'String':
                result = data;
                if (result == '') {
                    result = null;
                }
                break;
            case 'json':
            case 'Json':
                result = JSON.parse(data);
                break;
            default:
                result = data;
                if (result == '') {
                    result = null;
                }
                break;
        }
        return result;
    }

    private TransformType(type: string) {
        let result;
        switch (type) {
            case 'int':
            case 'Int':
                result = 'int';
                break;
            case 'float':
            case 'Float':
                result = 'float';
                break;
            case 'bool':
            case 'Bool':
            case 'boolen':
            case 'Boolen':
                result = 'bool';
                break;
            case 'string':
            case 'String':
                result = 'string';
                break;
            case 'json':
            case 'Json':
                result = "JSONObject";
                break;
            default:
                if(type.includes('serialize')) {
                    result = undefined;
                }else{
                    result = type;
                }
                break;
        }
        return result;
    }

    //检测type 对应的值
    private TransformValue (type: string, data: string, row?: number, col?: number) {

        if(type.includes('serialize')) {
            return undefined;
        }

        let result;
        if(typeof(data) == 'string') {
            data = data.replace(/[\r\n]/g, '');
        }

        if(type.includes('[]')) {   
            type = type.replace('[]','');
            result = [];
            let _datas = data.substring(1,data.length -1).split(',');
            for (let i = 0; i < _datas.length; ++i) {
                result.push(this._TransformBasicsValue(type, _datas[i]));
            }

        }else if(type.includes('[,]')) {
            //二维数组
            type = type.replace('[,]','');
            result = [];
            let datas = data.substring(2,data.length-2).split('],[');
            for (let i = 0; i < datas.length; ++i) {
                let tmpResult = []
                let _datas = datas[i].split(',');
                for (let j = 0; j < _datas.length; ++j) {
                    tmpResult.push(this._TransformBasicsValue(type, _datas[j]));
                }
                result.push(tmpResult);
            }

        }else{
            //正常值
            result = this._TransformBasicsValue(type, data);
        }
        return result;
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

    private genJSIndexFile (root: string, files_js: string[], files_dir: string[]){
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

        fs.writeFileSync(_headers_file, _str_contents);
    }

    public genFile (jsPath: string){
        jsPath = jsPath + "/js";
        let files = fs.readdirSync(jsPath);

        let fileList_js:string[]= [];
        let fileList_dir:string[]= [];
        for(let i in files) {
            let fileName = files[i];
            var _full_path = jsPath + '/' + fileName;
            if(fs.statSync(_full_path).isDirectory()){
                //屏蔽目录
                if(!this.isIgnore(_full_path,this._ignore_arr) ){
                    fileList_dir.push(_full_path);
                    this.genFile(_full_path);
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