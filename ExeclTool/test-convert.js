const xlsx = require('node-xlsx');
const fs = require('fs');

// 测试多格子子结构转换
function testMultiCellStruct() {
    const data = xlsx.parse('./design/cfg/3_物品表.xlsx');
    const sheet = data[0].data;
    
    console.log('=== Multi-Cell Struct Test ===');
    console.log('Keys:', sheet[0]);
    console.log('Types:', sheet[1]);
    
    // 解析字段路径
    const keys = sheet[0];
    const types = sheet[1];
    const fieldPaths = keys.map(key => {
        if (key.includes('.')) {
            return key.split('.');
        }
        return [key];
    });
    
    console.log('\nField Paths:');
    fieldPaths.forEach((path, i) => {
        console.log(   -> );
    });
    
    // 转换数据
    console.log('\n=== Converted Data ===');
    for (let i = 3; i < sheet.length; i++) {
        const row = sheet[i];
        if (!row || !row[0]) continue;
        
        const obj = {};
        for (let j = 0; j < keys.length; j++) {
            const path = fieldPaths[j];
            const value = row[j];
            
            // 设置嵌套值
            setNestedValue(obj, path, value);
        }
        
        console.log(Row :, JSON.stringify(obj, null, 2));
    }
}

function setNestedValue(obj, path, value) {
    let current = obj;
    
    for (let i = 0; i < path.length - 1; i++) {
        const key = path[i];
        if (!current[key]) {
            current[key] = {};
        }
        current = current[key];
    }
    
    const lastKey = path[path.length - 1];
    current[lastKey] = value;
}

testMultiCellStruct();
