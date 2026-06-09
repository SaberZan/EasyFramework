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
     * ���������ӽṹ�����ļ�
     */
    public async ParseStructDefinitions(definePath: string): Promise<void> {
        if (!fs.existsSync(definePath)) {
            console.warn('Struct define path not found:', definePath);
            return;
        }

        let data = xlsx.parse(definePath);

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
     * ��������Ƿ�Ϊ�ӽṹ����
     */
    public IsStructType(type: string): boolean {
        if (typeof type !== 'string') return false;
        let baseType = type.replace('[]', '').replace('[,]', '');
        return this.structDefinitions[baseType] !== undefined;
    }

    /**
     * ����Ƿ������������ֶΣ��� items[0].name��
     */
    public IsArrayField(key: string): boolean {
        return key.includes('[') && key.includes(']');
    }

    /**
     * ��ȡ�ֶ���������
     * ���磺"items[0].name" -> { fieldName: "items", index: 0, subField: "name" }
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
     * ת���ӽṹ���ݣ�JSON�ַ��� -> ����
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
     * �����ӽṹ����
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
     * ת����������ֵ
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
     * �����ֶ�·����֧���ӽṹ�ֶθ�ʽ��
     * ���磺"attr.value" -> ["attr", "value"]
     * ���磺"attrs[0].value" -> ["attrs", "0", "value"]
     * ���磺"items[0].name" -> ["items", "0", "name"]
     */
    public ParseFieldPath(key: string): (string | number)[] {
        let result: (string | number)[] = [];
        let parts = key.split('.');

        for (let part of parts) {
            // ��������������field[0] -> field �� 0
            let arrayMatches = part.split(/[\[\]]/).filter(s => s !== '');
            for (let match of arrayMatches) {
                result.push(match);
            }
        }

        return result;
    }

    /**
     * ����Ƕ�׶����ֵ
     * �Զ���������ϲ�
     */
    public SetNestedValue(obj: any, path: (string | number)[], value: any): void {
        let current = obj;

        for (let i = 0; i < path.length - 1; ++i) {
            let key = path[i];
            let nextKey = path[i + 1];

            // �ж���һ���Ƿ�����������
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

        // ��������ֵ
        let lastKey = path[path.length - 1];
        let arrayMatch = lastKey.toString().match(/(.+)\[(\d*)\]$/);
        if (arrayMatch) {
            let arrKey = arrayMatch[1];
            let index = parseInt(arrayMatch[2]) || 0;

            if (!current[arrKey]) { current[arrKey] = []; }
            current[arrKey][index] = value;
        } else {
            current[lastKey] = value;
        }
    }

    /**
     * �����ֶ���Ϣ
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



