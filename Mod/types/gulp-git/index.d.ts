
declare interface gulp_git 
{
     add(opts?: any): any;
     addRemote(remote: any, url: any, opt: any, cb: Function): any;
     addSubmodule(url: any, name: any, opt: any, cb: Function): any;
     branch(branch: any, opt: any, cb: Function): any;
     catFile(opt: any): any;
     checkout(branch: any, opt: any, cb: Function): any;
     checkoutFiles(opt: any): any;
     clean(paths: any, opt: any, cb: Function): any;
     clone(remote: any, opt: any, cb: Function): any;
     commit(message: any, opt: any): any;
     diff(compare: any, opt: any): any;
     exec(opt: any, cb: Function): any;
     fetch(remote: any, branch: any, opt: any, cb: Function): any;
     init(opt: any, cb: Function): any;
     merge(branch: any, opt: any, cb: Function): any;
     pull(remote: any, branch: any, opt: any, cb: Function): any;
     push(remote: any, branch: any, opt: any, cb: Function): any;
     removeRemote(remote: any, opt: any, cb: Function): any;
     reset(commit: any, opt: any, cb: Function): any;
     revParse(opt: any, cb: Function): any;
     rm(opt: any): any;
     stash(opt: any, cb: Function): any;
     status(opt: any, cb: Function): any;
     tag(version: any, message: any, opt: any, cb: Function): any;
     updateSubmodule(opt: any, cb: Function): any;
}

declare const gulp_git: gulp_git;

export = gulp_git;