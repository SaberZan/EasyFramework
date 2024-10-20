import xlsx from 'node-xlsx';
import path from 'path';
import fs from "fs";
import { mkdir, readdir, writeFile } from "fs/promises";
import _ from 'lodash';
import Utils from '../../Utils';
import BaseTranslate from '../BaseTranslate';
import BaseTranslateBefore from '../BaseTranslateBefore';

export default class Xlsx2TsBefore extends BaseTranslateBefore {
    public async BeforeTranslate(outputPathStr: string, params: any) : Promise<void> {
        outputPathStr = path.join(outputPathStr , "ts");
        if(!fs.existsSync(outputPathStr)) {
            await mkdir(outputPathStr, { recursive: true });
        }
    }
}