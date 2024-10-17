import * as gulp from 'gulp';
import fs from 'fs';
import path from 'path';
import git from 'gulp-git';
import minimist from 'minimist';
import { Target } from './src/Enum';
import BaseTranslate from './src/Translate/BaseTranslate';
import BaseTranslateBefore from './src/Translate/BaseTranslateBefore';
import BaseTranslateAfter from './src/Translate/BaseTranslateAfter';
import xlsx2Json from './src/Translate/JSON/Xlsx2Json';
import Xlsx2JsonBefore from './src/Translate/JSON/Xlsx2JsonBefore';
import Xlsx2JsAfter from './src/Translate/JS/Xlsx2JsAfter';
import Xlsx2Js from './src/Translate/JS/Xlsx2Js';
import Xlsx2JsBefore from './src/Translate/JS/Xlsx2JsBefore';
import Xlsx2TsAfter from './src/Translate/TS/Xlsx2TsAfter';
import Xlsx2TsBefore from './src/Translate/TS/Xlsx2TsBefore';
import Xlsx2Ts from './src/Translate/TS/Xlsx2Ts';
import Xlsx2Cs from './src/Translate/CS/Xlsx2Cs';
import Xlsx2CsBefore from './src/Translate/CS/Xlsx2CsBefore';
import Xlsx2FlatBuffersBefore from './src/Translate/FlatBuffers/Xlsx2FlatBuffersBefore';
import Xlsx2FlatBuffers from './src/Translate/FlatBuffers/Xlsx2FlatBuffers';
import Xlsx2ProtoBuffersBefore from './src/Translate/ProtoBuffers/Xlsx2ProtoBuffersBefore';
import Xlsx2ProtoBuffers from './src/Translate/ProtoBuffers/Xlsx2ProtoBuffers';


let knowOptions = {
    string: 'paramPath',
    default: {paramPath : process.env.NODE_ENV || './input/param.json' }
}

let options = minimist(process.argv.slice(2), knowOptions)
const param = require(options.paramPath);
const tmp_path = './tmp';

gulp.task("initEnv",(cb)=>{
    fs.rmSync(tmp_path, { recursive: true });
    fs.mkdirSync(tmp_path);
    cb();
});

gulp.task("git",(cb)=>{
    git.checkout('master', {cwd: './design'}, function (err: any) {
        git.pull("origin","master",{cwd:'./design'},(err:any)=>{
            cb();
        });
    });
});

gulp.task("genCfg",(cb)=>{
    let selectTranslateBeforeRule  = BaseTranslateBefore;
    let selectTranslateRule = BaseTranslate;
    let selectTranslateAfterRule = BaseTranslateAfter;

    let isValid = true;
    switch (param.target) {
        case Target.JS:
            selectTranslateBeforeRule = Xlsx2JsBefore
            selectTranslateRule = Xlsx2Js;
            selectTranslateAfterRule = Xlsx2JsAfter
            break;
        case Target.TS:
            selectTranslateBeforeRule = Xlsx2TsBefore
            selectTranslateRule = Xlsx2Ts;
            selectTranslateAfterRule = Xlsx2TsAfter
            break;
        case Target.JSON:
            selectTranslateBeforeRule = Xlsx2JsonBefore
            selectTranslateRule = xlsx2Json;
            break;
        case Target.CS:
            selectTranslateBeforeRule = Xlsx2CsBefore
            selectTranslateRule = Xlsx2Cs;
            break;
        case Target.FlatBuffers:
            selectTranslateBeforeRule = Xlsx2FlatBuffersBefore;
            selectTranslateRule = Xlsx2FlatBuffers;
            break;
        case Target.ProtoBuffers:
            selectTranslateBeforeRule = Xlsx2ProtoBuffersBefore;
            selectTranslateRule = Xlsx2ProtoBuffers;
            break;
        default:
            isValid = false;
            break;
    }

    if(isValid){
        async function trans() {
            let before = new selectTranslateBeforeRule();
            await before.BeforeTranslate(tmp_path, param.params);

            let promiseArr : Promise<void>[] = [];
            for(let key in param.rules) {
                let tmpKey = key;
                let tmpResult = param.rules[key];
                let translateRule = new selectTranslateRule();
                promiseArr.push(translateRule.TranslateExcel(path.join(param.excelPath, tmpKey) , tmp_path, tmpResult, param.params));

            }
            await Promise.all(promiseArr);
            let after = new selectTranslateAfterRule();
            await after.AfterTranslate(tmp_path);
            cb();
        }
        trans();
        
    }else{
        cb("error");
    }
});

gulp.task("copyOutput",(cb)=>{
    for(let key in param.output) {
        let outpath = path.join(__dirname, param.output[key]);
        if(fs.existsSync(outpath)){
            let files = fs.readdirSync(outpath);
            for(let i in files) {
                fs.rmSync(path.join(outpath, files[i]), { recursive: true });
            }
        }else{
            fs.mkdirSync(outpath);
        }
        fs.cpSync(path.join(tmp_path,key), outpath, { recursive: true });
    }
    cb();
});

gulp.task("build-cfg",gulp.series("initEnv","genCfg","copyOutput",(cb)=>{
    cb();
}));