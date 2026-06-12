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
export interface IntoFieldInfo {
    structName: string;
    field: string;
}
export interface IndexedStructFieldInfo {
    propertyName: string;
    index: number;
    structName: string;
    field: string;
}

export default class BaseTranslateStruct extends BaseTranslateConfig {
    public structDefinitions: { [name: string]: StructDefinition } = {};

    /**
     * Parse struct definitions from the specified Excel file
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
     * Check if the given type is a struct type
     */
    public IsStructType(type: string): boolean {
        if (typeof type !== 'string') return false;
        let baseType = type.replace('[]', '').replace('[,]', '');
        return this.structDefinitions[baseType] !== undefined;
    }

    /**
     * Check if the key represents an array field, e.g. items[0].name
     */
    public IsArrayField(key: string): boolean {
        return key.includes('[') && key.includes(']');
    }

    /**
     * e.g. "items[0].name" -> { fieldName: "items", index: 0, subField: "name" }
     * e.g. "items[0].name" -> { fieldName: "items", index: 0, subField: "name" }
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
     * Parse "into=structLower.field" format, e.g. "into=attr.name"
     * Returns the struct name (capitalized) and the sub-field name, or null if not matching.
     */
    public ParseIntoFieldInfo(key: string): IntoFieldInfo | null {
        let match = key.match(/^into=(\w+)\.(\w+)$/);
        if (match) {
            return {
                structName: match[1].charAt(0).toUpperCase() + match[1].slice(1),
                field: match[2]
            };
        }
        return null;
    }
    /**
     * Parse "propName[index]=StructName.field" format, e.g. "info[0]=StructTest.value1"
     * Returns the property name, index, struct name, and field, or null if not matching.
     */
    public ParseIndexedStructFieldInfo(key: string): IndexedStructFieldInfo | null {
        let match = key.match(/^(\w+)\[(\d+)\]=(\w+)\.(\w+)$/);
        if (match) {
            return {
                propertyName: match[1],
                index: parseInt(match[2]),
                structName: match[3],
                field: match[4]
            };
        }
        return null;
    }
    /**
     * Check if key is an "into=..." struct field
     */
    public IsIntoField(key: string): boolean {
        return /^into=\w+\.\w+$/.test(key);
    }
    /**
     * Check if key is an "X[N]=Y.Z" indexed struct field
     */
    public IsIndexedStructField(key: string): boolean {
        return /^\w+\[\d+\]=\w+\.\w+$/.test(key);
    }
    /**
     * Resolve a column header key to a field path for data output.
     * Handles all formats: "into=X.Y", "X[N]=Y.Z", and the existing "X.Y" / "X[N].Y".
     */
    public ResolveFieldPath(key: string): (string | number)[] {
        let intoInfo = this.ParseIntoFieldInfo(key);
        if (intoInfo) {
            return ["info", intoInfo.field];
        }
        let indexedInfo = this.ParseIndexedStructFieldInfo(key);
        if (indexedInfo) {
            return [indexedInfo.propertyName, indexedInfo.index, indexedInfo.field];
        }
        return this.ParseFieldPath(key);
    }

    /**
     * Transform a struct value from string format into the appropriate type
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
     * Parse a struct object from the given row data
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
     * Transform a basic value from string into the specified type
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
     * e.g. "attr.value" -> ["attr", "value"]
     * e.g. "attrs[0].value" -> ["attrs", "0", "value"]
     * e.g. "items[0].name" -> ["items", "0", "name"]
     * e.g. "items[0].name" -> ["items", "0", "name"]
     */
    public ParseFieldPath(key: string): (string | number)[] {
        let result: (string | number)[] = [];
        let parts = key.split('.');

        for (let part of parts) {
            // Handle array index, e.g. field[0] -> field and 0
            let arrayMatches = part.split(/[\[\]]/).filter(s => s !== '');
            for (let match of arrayMatches) {
                result.push(match);
            }
        }

        return result;
    }

    /**
     * Set the value at the given path in the nested object
     * Set the value at the given path in the nested object
     */
    public SetNestedValue(obj: any, path: (string | number)[], value: any): void {
        let current = obj;

        for (let i = 0; i < path.length - 1; ++i) {
            let key = path[i];
            let nextKey = path[i + 1];

            // Check if the next key is an array index
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

        // Handle the last key in the path
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
     * Analyze a field and return field info
     */
    public AnalyzeField(key: string, type: string): FieldInfo {
        let result: FieldInfo = {
            name: key,
            isStruct: false,
            structName: '',
            isArray: false,
            fieldPath: []
        };
        let intoInfo = this.ParseIntoFieldInfo(key);
        if (intoInfo) {
            result.name = "info";
            result.isStruct = true;
            result.structName = intoInfo.structName;
            result.fieldPath = [intoInfo.field];
            result.isArray = false;
            return result;
        }
        let indexedInfo = this.ParseIndexedStructFieldInfo(key);
        if (indexedInfo) {
            result.name = indexedInfo.propertyName;
            result.isStruct = true;
            result.structName = indexedInfo.structName;
            result.fieldPath = [indexedInfo.field];
            result.isArray = true;
            return result;
        }
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



