import xlsx from 'node-xlsx';
import path from 'path';
import fs from "fs";
import { mkdir, readdir, writeFile } from "fs/promises";
import _ from 'lodash';
import Utils from '../utils';
import BaseTranslateConfig from './BaseTranslateConfig';

export interface StructField {
    name: string;
    type: string;
}

export interface StructDefinition {
    name: string;
    fields: StructField[];
}

export interface FieldInfo {
    name: string;
    isStruct: boolean;
    structName: string;
    isArray: boolean;
    fieldPath: string[];
}

export default class BaseTranslateStruct extends BaseTranslateConfig {
    public structDefinitions: { [name: string]: StructDefinition } = {};

    /**
     * 解析所有子结构定义文件
     */
    public async ParseStructDefinitions(definePath: string): Promise<void> {
        if (!fs.existsSync(definePath)) {
            console.warn('Struct define path not found:', definePath);
            return;
        }

        let files = await readdir(definePath);
        for (let file of files) {
            if (!file.endsWith('.xlsx')) {
                continue;
            }

            let filePath = path.join(definePath, file);
            let data = xlsx.parse(filePath);

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
    }

    /**
     * 检查类型是否为子结构类型
     */
    public IsStructType(type: string): boolean {
        let baseType = type.replace('[]', '').replace('[,]', '');
        return this.structDefinitions[baseType] !== undefined;
    }

    /**
     * 检查是否是数组索引字段（如 items[0].name）
     */
    public IsArrayField(key: string): boolean {
        return key.includes('[') && key.includes(']');
    }

    /**
     * 获取字段名和索引
     * 例如："items[0].name" -> { fieldName: "items", index: 0, subField: "name" }
     */
    public ParseArrayField(key: string): { fieldName: string; index: number; subField?: string } | null {
        let arrayMatch = key.match(/^(.+?)\[(\d+)\](?:\.(.+))?$/);
        if (arrayMatch) {
            return {
                fieldName: arrayMatch[1],
                index: parseInt(arrayMatch[2]),
                subField: arrayMatch[3]
            };
        }
        return null;
    }

    /**
     * 转换子结构数据（JSON字符串 -> 对象）
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
     * 解析子结构对象
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
     * 转换基本类型值
     */
    private TransformBasicValue(type: string, data: any): any {
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
     * 解析字段路径（支持子结构字段格式）
     * 例如："attr.value" -> ["attr", "value"]
     * 例如："attrs[0].value" -> ["attrs", "0", "value"]
     * 例如："items[0].name" -> ["items", "0", "name"]
     */
    public ParseFieldPath(key: string): (string | number)[] {
        let result: (string | number)[] = [];
        let parts = key.split('.');

        for (let part of parts) {
            // 处理数组索引：field[0] -> field 和 0
            let arrayMatches = part.split(/[\[\]]/).filter(s => s !== '');
            for (let match of arrayMatches) {
                result.push(match);
            }
        }

        return result;
    }

    /**
     * 设置嵌套对象的值
     * 自动处理数组合并
     */
    public SetNestedValue(obj: any, path: (string | number)[], value: any): void {
        let current = obj;

        for (let i = 0; i < path.length - 1; ++i) {
            let key = path[i];
            let nextKey = path[i + 1];

            // 判断下一级是否是数组索引
            let isNextArrayIndex = !isNaN(parseInt(nextKey.toString()));

            if (current[key] === undefined || current[key] === null) {
                if (isNextArrayIndex) {
                    current[key] = [];
                } else {
                    current[key] = {};
                }
            }

            current = current[key];
        }

        // 设置最终值
        let lastKey = path[path.length - 1];
        let arrayMatch = lastKey.match(/(.+)\[(\d*)\]$/);
        if (arrayMatch) {
            let arrKey = arrayMatch[1];
            let index = parseInt(arrayMatch[2]) || 0;

            if (!current[arrKey]) {
                current[arrKey] = [];
            }
            current[arrKey][index] = value;
        } else {
            current[lastKey] = value;
        }
    }

    /**
     * 分析字段信息
     */
    public AnalyzeField(key: string, type: string): FieldInfo {
        let result: FieldInfo = {
            name: key,
            isStruct: false,
            structName: '',
            isArray: false,
            fieldPath: []
        };

        if (key.includes('.')) {
            let parts = key.split('.');
            result.name = parts[0];
            result.fieldPath = parts.slice(1) as string[];
            result.isStruct = true;

            let arrayMatch = result.name.match(/(.+)\[(\d*)\]$/);
            if (arrayMatch) {
                result.name = arrayMatch[1];
                result.isArray = true;
            }
        }

        return result;
    }
}
