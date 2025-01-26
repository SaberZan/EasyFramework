import xlsx from 'node-xlsx';
import path from 'path';
import fs from "fs";
import { mkdir, readdir, writeFile } from "fs/promises";
import _ from 'lodash';
import Utils from '../../utils';
import BaseTranslate from '../BaseTranslate';
import BaseTranslateBefore from '../BaseTranslateBefore';

export default class Xlsx2CsBefore extends BaseTranslateBefore {
    public async BeforeTranslate(outputPathStr: string, params: any) : Promise<void> {
        let outputPathCsStr = path.join(outputPathStr, "code", "cs");
        let outputPathJsonStr = path.join(outputPathStr, "json");

        if(!fs.existsSync(outputPathCsStr)) {
            await mkdir(outputPathCsStr, { recursive: true });
        }
        if(!fs.existsSync(outputPathJsonStr)) {
            await mkdir(outputPathJsonStr, { recursive: true });
        }
    }
}