import * as gulp from 'gulp';
import fs from 'fs';
import path from 'path';
import git from 'gulp-git';
import minimist from 'minimist';
import { Target } from './src/Enum';
import xlsx2Js from './src/Translate/JS/Xlsx2Js';
import xlsx2Cs from './src/Translate/CS/Xlsx2Cs';
import xlsx2Json from './src/Translate/JSON/Xlsx2Json';
import xlsx2FlatBuffers from './src/Translate/FlatBuffers/Xlsx2FlatBuffers';
import xlsx2ProtoBuffers from './src/Translate/ProtoBuffers/Xlsx2ProtoBuffers';
import BaseTranslate from './src/Translate/BaseTranslate';

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
    let translateRule: BaseTranslate;
    switch (param.target) {
        case Target.JS:
            translateRule = translateRule = new xlsx2Js();
            break;
        case Target.TS:
            translateRule = new xlsx2Js();
            break;
        case Target.JSON:
            translateRule = new xlsx2Json();
            break;
        case Target.CS:
            translateRule = new xlsx2Cs();
            break;
        case Target.FlatBuffers:
            translateRule = new xlsx2FlatBuffers();
            break;
        case Target.ProtoBuffers:
            translateRule = new xlsx2ProtoBuffers();
            break;
        default:
            translateRule = new BaseTranslate();
            break;
    }
    if(translateRule != undefined){
        translateRule.BeforeTranslate(tmp_path, param.params);
        for(let key in param.rules) {
            translateRule.TranslateExcel(path.join(param.excelPath, key) , tmp_path, param.rules[key], param.params);
        }
        translateRule.GenIndex(tmp_path);
        cb();
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