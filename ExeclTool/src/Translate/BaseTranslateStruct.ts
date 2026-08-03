import xlsx from 'node-xlsx';
import path from 'path';
import fs from "fs";
import { mkdir, readdir, writeFile } from "fs/promises";
import _ from 'lodash';
import Utils from '../utils';
import BaseTranslateConfig from './BaseTranslateConfig';
import CsvParser from '../CsvParser';

export interface StructField {
    name: string;
    type: string;
}

export interface StructDefinition {
    name: string;
    fields: StructField[];
}

export interface FieldInfo {
    /** 输出字段名，取等号左边（去掉 [N] 索引） */
    name: string;
    isStruct: boolean;
    /** 结构名，取等号右边 */
    structName: string;
    /** 是否数组（带 [N] 索引，或内联类型带 [] 后缀） */
    isArray: boolean;
    /** 子字段路径，取类型行 */
    fieldPath: string[];
    /** 数组索引，无索引为 -1 */
    index: number;
    /** 内联结构字段定义（类型行写成 {a:int,b:string} 时才有） */
    inlineFields: StructField[];
    /** 内联结构类型是否带 [] 后缀（整格是一组结构值） */
    inlineIsArray: boolean;
    /** 该列自身的值类型（普通字段或单个子字段） */
    valueType: string;
}

/** 结构属性：表头等号左边的属性名，及其引用的结构与数组信息 */
export interface StructProperty {
    propertyName: string;
    structName: string;
    isArray: boolean;
}

/** 表头解析结果：等号左边的属性名与等号右边的结构名 */
export interface StructHeaderInfo {
    /** 输出字段名，等号左边（已去掉 [N]） */
    propertyName: string;
    /** 结构名，等号右边 */
    structName: string;
    /** 数组索引，无索引为 -1 */
    index: number;
}

export default class BaseTranslateStruct extends BaseTranslateConfig {
    public structDefinitions: { [name: string]: StructDefinition } = {};

    /**
     * 解析 define 目录下的结构定义（Excel 或 CSV）
     */
    public async ParseStructDefinitions(definePath: string): Promise<void> {
        let data: any[] = [];

        // 先尝试 CSV，再回退到 xlsx
        if (CsvParser.canParseAsCsv(definePath)) {
            data = CsvParser.parse(definePath);
        }

        if (data.length === 0 && fs.existsSync(definePath)) {
            data = xlsx.parse(definePath);
        }

        for (let sheet of data) {
            let structName = sheet.name;
            let sheetData = sheet.data;

            if (sheetData.length < 2) {
                continue;
            }

            let fields: StructField[] = [];
            let fieldNames = sheetData[0] || [];
            let fieldTypes = sheetData[1] || [];

            for (let i = 0; i < fieldNames.length; ++i) {
                let fieldName = fieldNames[i];
                let fieldType = fieldTypes[i];

                if (_.isNil(fieldName) || _.isEmpty(fieldName)) {
                    continue;
                }

                fields.push({
                    name: fieldName,
                    type: fieldType
                });
            }

            this.structDefinitions[structName] = {
                name: structName,
                fields: fields
            };
        }
    }

    /**
     * 判断类型是否是 Struct.xlsx 中定义的结构类型
     */
    public IsStructType(type: string): boolean {
        if (typeof type !== 'string') return false;
        let baseType = type.replace('[]', '').replace('[,]', '');
        return this.structDefinitions[baseType] !== undefined;
    }

    /**
     * 解析表头中的结构声明，格式为 `属性名=结构名` 或 `属性名[索引]=结构名`
     * 例如 `into=attr`、`info[0]=StructTest`
     */
    public ParseStructHeader(key: string): StructHeaderInfo | null {
        if (typeof key !== 'string') return null;
        let match = key.match(/^(\w+)(?:\[(\d+)\])?=(\w+)$/);
        if (!match) return null;
        return {
            propertyName: match[1],
            index: match[2] !== undefined ? parseInt(match[2]) : -1,
            structName: match[3]
        };
    }

    /**
     * 判断表头是否是结构字段声明
     */
    public IsStructHeader(key: string): boolean {
        return this.ParseStructHeader(key) !== null;
    }

    /**
     * 判断类型行是否是内联结构写法，例如 `{name:string,value:int}` 或 `{name:string}[]`
     */
    public IsInlineStructType(type: string): boolean {
        if (typeof type !== 'string') return false;
        return /^\{.*\}(\[\])?$/.test(type.trim());
    }

    /**
     * 解析内联结构类型定义。
     * 例如 `{name:string,value:int,coeff:float}`   -> 字段列表，isArray = false
     * 例如 `{name:string,value:int,coeff:float}[]` -> 字段列表，isArray = true
     * 未写类型的字段（如 `{name,value}`）类型留空，交由调用方按数据推断或报错。
     */
    public ParseInlineStructType(type: string): { fields: StructField[]; isArray: boolean } | null {
        if (!this.IsInlineStructType(type)) return null;
        let trimmed = type.trim();
        let isArray = trimmed.endsWith('[]');
        let body = isArray ? trimmed.slice(1, -3) : trimmed.slice(1, -1);

        let fields: StructField[] = [];
        for (let part of body.split(',')) {
            let seg = part.trim();
            if (seg === '') continue;
            let colonIndex = seg.indexOf(':');
            if (colonIndex >= 0) {
                fields.push({
                    name: seg.substring(0, colonIndex).trim(),
                    type: seg.substring(colonIndex + 1).trim()
                });
            } else {
                fields.push({ name: seg, type: '' });
            }
        }

        return { fields: fields, isArray: isArray };
    }

    /**
     * 解析一列的字段信息。新格式下结构名在表头（等号右边），
     * 子字段名或内联结构定义在类型行。
     * @param key 表头单元格，例如 `id`、`into=attr`、`info[0]=StructTest`
     * @param type 类型行单元格，例如 `int`、`name:string`、`{name:string,value:int}[]`
     */
    public AnalyzeField(key: string, type: string): FieldInfo {
        let result: FieldInfo = {
            name: key,
            isStruct: false,
            structName: '',
            isArray: false,
            fieldPath: [],
            index: -1,
            inlineFields: [],
            inlineIsArray: false,
            valueType: type
        };

        let header = this.ParseStructHeader(key);
        if (!header) {
            return result;
        }

        result.name = header.propertyName;
        result.structName = header.structName;
        result.isStruct = true;
        result.index = header.index;
        result.isArray = header.index >= 0;

        // 类型行写成内联结构：整格承载一个（或一组）结构值
        let inline = this.ParseInlineStructType(type);
        if (inline) {
            result.inlineFields = inline.fields;
            result.inlineIsArray = inline.isArray;
            if (inline.isArray) {
                result.isArray = true;
            }
            result.valueType = type;
            return result;
        }

        // 类型行写成子字段名：`name` 或 `name:string`
        let cell = typeof type === 'string' ? type.trim() : '';
        let colonIndex = cell.indexOf(':');
        if (colonIndex >= 0) {
            result.fieldPath = [cell.substring(0, colonIndex).trim()];
            result.valueType = cell.substring(colonIndex + 1).trim();
        } else if (cell !== '') {
            result.fieldPath = [cell];
            result.valueType = '';
        }

        return result;
    }

    /**
     * 解析某列在输出对象中的路径。
     * `into=attr`  + 类型 `name:string`  -> ["attr", "name"]
     * `info[0]=X`  + 类型 `value1`       -> ["info", 0, "value1"]
     * `infoNew=attr` + 内联结构类型      -> ["infoNew"]（整格是一个结构值）
     */
    public ResolveFieldPath(key: string, type?: string): (string | number)[] {
        let info = this.AnalyzeField(key, type as string);
        if (!info.isStruct) {
            return [info.name];
        }

        let result: (string | number)[] = [info.name];
        if (info.index >= 0) {
            result.push(info.index);
        }
        for (let sub of info.fieldPath) {
            result.push(sub);
        }
        return result;
    }

    /**
     * 按结构定义解析内联结构文本值。
     * 单个结构：`{攻击,10,1.5}`
     * 结构数组：`[{攻击,10,1.5},{防御,20,2}]`，元素间也支持 `|` 分隔
     */
    public TransformInlineStructValue(fields: StructField[], isArray: boolean, data: any): any {
        let text = _.isNil(data) ? '' : String(data).replace(/[\r\n]/g, '').trim();

        if (!isArray) {
            return this.ParseInlineStructBody(fields, text);
        }

        let result: any[] = [];
        // 剥掉最外层的 []
        if (text.startsWith('[') && text.endsWith(']')) {
            text = text.substring(1, text.length - 1).trim();
        }
        if (text === '') {
            return result;
        }

        for (let item of this.SplitInlineStructItems(text)) {
            result.push(this.ParseInlineStructBody(fields, item));
        }
        return result;
    }

    /**
     * 把 `{a,1},{b,2}` 或 `{a,1}|{b,2}` 切成单个 `{...}` 文本，忽略花括号内的分隔符
     */
    private SplitInlineStructItems(text: string): string[] {
        let items: string[] = [];
        let depth = 0;
        let current = '';
        for (let i = 0; i < text.length; ++i) {
            let ch = text[i];
            if (ch === '{') {
                depth++;
                current += ch;
            } else if (ch === '}') {
                depth--;
                current += ch;
            } else if ((ch === ',' || ch === '|') && depth === 0) {
                if (current.trim() !== '') items.push(current.trim());
                current = '';
            } else {
                current += ch;
            }
        }
        if (current.trim() !== '') items.push(current.trim());
        return items;
    }

    /**
     * 解析单个 `{值1,值2,...}` 文本，按字段顺序与类型赋值
     */
    private ParseInlineStructBody(fields: StructField[], text: string): any {
        let body = text.trim();
        if (body.startsWith('{') && body.endsWith('}')) {
            body = body.substring(1, body.length - 1);
        }

        let values = this.SplitInlineStructItems(body);
        let result: any = {};
        for (let i = 0; i < fields.length; ++i) {
            let field = fields[i];
            let raw = i < values.length ? values[i] : undefined;
            result[field.name] = this.TransformBasicValue(field.type, raw);
        }
        return result;
    }

    /**
     * 按结构定义把 JSON 文本转换为结构对象（Struct.xlsx 中定义的类型）
     */
    public TransformStructValue(type: string, data: string): any {
        if (type.includes('[]')) {
            let baseType = type.replace('[]', '');
            let structDef = this.structDefinitions[baseType];

            if (!structDef) {
                return JSON.parse(data);
            }

            let jsonArray = JSON.parse(data);
            let result: any[] = [];

            for (let item of jsonArray) {
                let structObj = this.ParseStructObject(structDef, item);
                result.push(structObj);
            }

            return result;
        }

        let structDef = this.structDefinitions[type];
        if (!structDef) {
            return JSON.parse(data);
        }

        return this.ParseStructObject(structDef, JSON.parse(data));
    }

    /**
     * 从行数据中解析结构对象
     */
    private ParseStructObject(structDef: StructDefinition, data: any): any {
        let result: any = {};

        for (let field of structDef.fields) {
            let fieldName = field.name;
            let fieldType = field.type;
            let value = data[fieldName];

            if (value !== undefined) {
                result[fieldName] = this.TransformBasicValue(fieldType, value);
            }
        }

        return result;
    }

    /**
     * 按指定类型转换基础值
     */
    public TransformBasicValue(type: string, data: any): any {
        let result;

        switch (type) {
            case 'int':
            case 'Int':
                result = parseInt(data);
                if (isNaN(result)) result = 0;
                break;
            case 'float':
            case 'Float':
                result = parseFloat(data);
                if (isNaN(result)) result = 0;
                break;
            case 'bool':
            case 'Bool':
            case 'boolean':
            case 'Boolean':
                result = Boolean(data);
                break;
            case 'string':
            case 'String':
                result = String(data);
                break;
            default:
                result = data;
                break;
        }

        return result;
    }

    /**
     * 按路径写入嵌套对象，路径中的数字视为数组下标
     */
    public SetNestedValue(obj: any, path: (string | number)[], value: any): void {
        let current = obj;

        for (let i = 0; i < path.length - 1; ++i) {
            let key = path[i];
            let nextKey = path[i + 1];

            // 下一级是数字则当前节点应为数组
            let isNextArrayIndex = typeof nextKey === "number" || (!isNaN(parseInt(nextKey.toString())));

            if (current[key] === undefined || current[key] === null) {
                if (isNextArrayIndex) {
                    current[key] = [];
                } else {
                    current[key] = {};
                }
            }

            current = current[key];
        }

        current[path[path.length - 1]] = value;
    }

    /**
     * 收集一张表中所有结构字段的定义，并校验同名结构的字段类型一致。
     * @param keys 表头行
     * @param types 类型行
     * @param sheetName 表名，仅用于报错信息
     */
    public CollectStructFields(keys: any[], types: any[], sheetName: string): { [structName: string]: StructDefinition } {
        let collected: { [structName: string]: StructDefinition } = {};

        for (let colIndex = 0; colIndex < keys.length; ++colIndex) {
            let key = keys[colIndex];
            let type = types[colIndex];
            if (_.isNil(key) || _.isEmpty(key) || String(key).startsWith('#')) {
                continue;
            }

            let info = this.AnalyzeField(key, type);
            if (!info.isStruct) continue;

            // Struct.xlsx 中已定义的结构直接引用，不参与推断
            if (this.IsStructType(info.structName)) continue;

            let fields: StructField[] = info.inlineFields.length > 0
                ? info.inlineFields
                : (info.fieldPath.length > 0 ? [{ name: info.fieldPath[0], type: info.valueType }] : []);

            if (fields.length === 0) continue;

            if (!collected[info.structName]) {
                collected[info.structName] = { name: info.structName, fields: [] };
            }
            let target = collected[info.structName];

            for (let field of fields) {
                if (!field.type) {
                    throw new Error(`表 ${sheetName} 中结构 ${info.structName} 的字段 ${field.name} 未声明类型，请在类型行写成 ${field.name}:类型`);
                }
                let exist = target.fields.find(f => f.name === field.name);
                if (!exist) {
                    target.fields.push({ name: field.name, type: field.type });
                } else if (exist.type !== field.type) {
                    throw new Error(`表 ${sheetName} 中结构 ${info.structName} 的字段 ${field.name} 类型不一致：${exist.type} 与 ${field.type}`);
                }
            }
        }

        return collected;
    }

    /**
     * 汇总一张表的列布局，供各语言生成器共用，避免每种格式各写一遍表头解析。
     * @param data 整张表的二维数据
     * @param className 生成的类名，用于给未定义结构拼局部类名
     * @param typeRowIndex 类型行下标
     * @param sheetName 表名，仅用于报错信息
     */
    public AnalyzeSheet(data: any, className: string, typeRowIndex: number, sheetName: string): {
        /** 普通字段（非结构） */
        simpleFields: { key: string; type: string; colIndex: number }[];
        /** 结构属性，按表头出现顺序 */
        structProperties: StructProperty[];
        /** 需要为本表生成的局部结构类：类名 -> 字段定义 */
        localStructs: { [className: string]: StructDefinition };
        /** 引用到的 Struct.xlsx 全局结构名 */
        globalStructs: string[];
    } {
        let keys = (data && data[0]) ? data[0] : [];
        let types = (data && data[typeRowIndex]) ? data[typeRowIndex] : [];

        let simpleFields: { key: string; type: string; colIndex: number }[] = [];
        let structProperties: StructProperty[] = [];
        let localStructs: { [className: string]: StructDefinition } = {};
        let globalStructs: string[] = [];

        // 先校验并收集同名结构的字段定义（类型不一致会在此抛错）
        let collected = this.CollectStructFields(keys, types, sheetName);

        let seenProperty: { [name: string]: StructProperty } = {};

        for (let colIndex = 0; colIndex < keys.length; ++colIndex) {
            let key = keys[colIndex];
            let type = types[colIndex];
            if (_.isNil(key) || _.isEmpty(key) || String(key).startsWith('#')) {
                continue;
            }

            let info = this.AnalyzeField(key, type);

            if (!info.isStruct) {
                simpleFields.push({ key: info.name, type: info.valueType, colIndex: colIndex });
                continue;
            }

            let isGlobal = this.IsStructType(info.structName);
            // 未在 Struct.xlsx 定义的结构生成局部类，类名为 表类名 + 结构名首字母大写
            let typeName = isGlobal
                ? info.structName
                : className + Utils.GetFristUpperAndLowerStr(info.structName)[0];

            if (isGlobal) {
                if (globalStructs.indexOf(info.structName) < 0) {
                    globalStructs.push(info.structName);
                }
            } else if (collected[info.structName]) {
                localStructs[typeName] = {
                    name: typeName,
                    fields: collected[info.structName].fields
                };
            }

            let exist = seenProperty[info.name];
            if (!exist) {
                let prop: StructProperty = {
                    propertyName: info.name,
                    structName: typeName,
                    isArray: info.isArray
                };
                seenProperty[info.name] = prop;
                structProperties.push(prop);
            } else if (info.isArray) {
                exist.isArray = true;
            }
        }

        return {
            simpleFields: simpleFields,
            structProperties: structProperties,
            localStructs: localStructs,
            globalStructs: globalStructs
        };
    }

    /**
     * 解析一行中某列的值，并写入输出对象。结构列会按新格式规则归并。
     * @param target 输出对象
     * @param key 表头单元格
     * @param type 类型行单元格
     * @param value 数据单元格
     * @param transformBasic 基础类型/数组的转换回调，由各生成器提供
     * @param lowerCaseKey 普通字段名是否首字母小写
     */
    public ApplyCellValue(
        target: any,
        key: string,
        type: string,
        value: any,
        transformBasic: (type: string, data: any) => any,
        lowerCaseKey: boolean = true
    ): void {
        let info = this.AnalyzeField(key, type);
        let fieldName = lowerCaseKey ? Utils.GetFristUpperAndLowerStr(info.name)[1] : info.name;

        if (!info.isStruct) {
            let result = transformBasic(info.valueType, value);
            if (!_.isNil(result) && !_.isNaN(result)) {
                target[fieldName] = result;
            }
            return;
        }

        // 内联结构：整格是一个结构值或一组结构值
        if (info.inlineFields.length > 0) {
            let parsed = this.TransformInlineStructValue(info.inlineFields, info.inlineIsArray, value);
            if (info.index >= 0) {
                if (!Array.isArray(target[fieldName])) target[fieldName] = [];
                target[fieldName][info.index] = parsed;
            } else {
                target[fieldName] = parsed;
            }
            return;
        }

        // 子字段列：按 结构名/索引/子字段名 归并
        let subField = info.fieldPath.length > 0 ? info.fieldPath[0] : '';
        if (subField === '') return;

        let subType = info.valueType;
        if (!subType && this.IsStructType(info.structName)) {
            // Struct.xlsx 已定义类型时，子字段类型从定义中取
            let def = this.structDefinitions[info.structName];
            let defField = def.fields.find(f => f.name === subField);
            if (defField) subType = defField.type;
        }

        let parsed = subType
            ? this.TransformBasicValue(subType, value)
            : transformBasic(subType, value);

        if (info.index >= 0) {
            if (!Array.isArray(target[fieldName])) target[fieldName] = [];
            if (!target[fieldName][info.index]) target[fieldName][info.index] = {};
            target[fieldName][info.index][subField] = parsed;
        } else {
            if (!target[fieldName] || typeof target[fieldName] !== 'object') target[fieldName] = {};
            target[fieldName][subField] = parsed;
        }
    }
}
