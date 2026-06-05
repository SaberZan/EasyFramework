import * as xlsx from 'node-xlsx';
import path from 'path';
import fs from "fs";
import { mkdir, readdir, writeFile } from "fs/promises";
import _ from 'lodash';
import Utils from '../utils';
import BaseTranslateConfig from './Translate/BaseTranslateConfig';
import BaseTranslateStruct, { StructDefinition } from './Translate/BaseTranslateStruct';

async function testMultiCellStruct() {
    console.log('=== Testing Multi-Cell Struct ===\n');
    
    // 测试数据：模拟一个包含多格子子结构的表格
    const testSheetData = {
        name: 'ItemConfig',
        data: [
            // 第一行：字段名（支持 field.subfield 格式）
            ['id', 'name', 'price', 'attr.name', 'attr.value', 'attr.coeff'],
            // 第二行：类型
            ['int', 'string', 'int', 'string', 'int', 'float'],
            // 第三行：描述
            ['物品ID', '物品名称', '价格', '属性名', '属性值', '系数'],
            // 数据行
            [1001, '剑', 100, '攻击', 10, 1.5],
            [1002, '盾', 80, '防御', 15, 1.2],
            [1003, '药水', 10, '回复', 50, 0]
        ]
    };

    console.log('Input Data:');
    console.log('Headers:', testSheetData.data[0]);
    console.log('Types:', testSheetData.data[1]);
    console.log('Row 1:', testSheetData.data[3]);
    console.log('Row 2:', testSheetData.data[4]);
    console.log('Row 3:', testSheetData.data[5]);
    
    // 解析字段映射
    const keys = testSheetData.data[0];
    const types = testSheetData.data[1];
    
    console.log('\n=== Field Mappings ===');
    for (let i = 0; i < keys.length; i++) {
        const key = keys[i];
        const path = ParseFieldPath(key as string);
        console.log(${key} ->  (isNested: ));
    }
    
    // 构建输出数据
    console.log('\n=== Output JSON ===');
    const jsonOut: any = {};
    
    for (let rowIndex = 3; rowIndex < testSheetData.data.length; rowIndex++) {
        const row = testSheetData.data[rowIndex];
        const id = row[0];
        
        if (!id) continue;
        
        const item: any = {};
        
        for (let colIndex = 0; colIndex < keys.length; colIndex++) {
            const key = keys[colIndex] as string;
            const type = types[colIndex] as string;
            const value = row[colIndex];
            
            if (!key || value === undefined) continue;
            
            const path = ParseFieldPath(key);
            
            if (path.length > 1) {
                // 子结构字段
                SetNestedValue(item, path, TransformValue(type, value));
            } else {
                // 普通字段
                item[path[0]] = TransformValue(type, value);
            }
        }
        
        jsonOut[id] = item;
    }
    
    console.log(JSON.stringify(jsonOut, null, 2));
}

/**
 * 解析字段路径
 * 例如："attr.name" -> ['attr', 'name']
 * 例如："attrs[0].value1" -> ['attrs', 0, 'value1']
 */
function ParseFieldPath(key: string): (string | number)[] {
    let path: (string | number)[] = [];
    let parts = key.split('.');

    for (let part of parts) {
        // 处理数组索引：field[0] -> field 和 0
        let arrayMatch = part.match(/^(.+?)\[(\d+)\]$/);
        if (arrayMatch) {
            path.push(arrayMatch[1]);
            path.push(parseInt(arrayMatch[2]));
        } else {
            // 检查是否只有数组索引：[0]
            let indexMatch = part.match(/^\[(\d+)\]$/);
            if (indexMatch) {
                path.push(parseInt(indexMatch[1]));
            } else {
                path.push(part);
            }
        }
    }

    return path;
}

/**
 * 设置嵌套字段的值
 */
function SetNestedValue(obj: any, path: (string | number)[], value: any): void {
    let current = obj;

    for (let i = 0; i < path.length - 1; ++i) {
        let part = path[i];
        let nextPart = path[i + 1];

        if (typeof part === 'number') {
            // 当前是数组索引
            if (!Array.isArray(current)) {
                current = [];
            }
            while (current.length <= part) {
                current.push({});
            }
            current = current[part];
        } else {
            // 当前是字段名
            if (!current[part]) {
                // 检查下一级是否是数组索引
                if (typeof nextPart === 'number') {
                    current[part] = [];
                } else {
                    current[part] = {};
                }
            }
            current = current[part];
        }
    }

    // 设置最终值
    let lastPart = path[path.length - 1];
    if (typeof lastPart === 'number') {
        if (!Array.isArray(current)) {
            current = [];
        }
        while (current.length <= lastPart) {
            current.push({});
        }
        current[lastPart] = value;
    } else {
        current[lastPart] = value;
    }
}

/**
 * 转换值类型
 */
function TransformValue(type: string, value: any): any {
    if (value === null || value === undefined || value === '') {
        return null;
    }
    
    switch (type) {
        case 'int':
        case 'Int':
            return parseInt(value) || 0;
        case 'float':
        case 'Float':
            return parseFloat(value) || 0;
        case 'bool':
        case 'Bool':
            return Boolean(value);
        case 'string':
        case 'String':
            return String(value);
        default:
            return value;
    }
}

testMultiCellStruct().catch(console.error);
