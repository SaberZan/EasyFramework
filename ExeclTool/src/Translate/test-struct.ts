import * as xlsx from 'node-xlsx';
import BaseTranslateStruct, { StructDefinition } from './BaseTranslateStruct';

async function testStructHelper() {
    const helper = new BaseTranslateStruct();
    
    // 解析子结构定义
    await helper.ParseStructDefinitions('./design/define');
    
    // 打印所有子结构
    console.log('=== Struct Definitions ===');
    for (const name in helper.structDefinitions) {
        const struct = helper.structDefinitions[name];
        console.log('Struct:', name);
        console.log('Fields:', JSON.stringify(struct, null, 2));
    }
    
    // 测试子结构类型检测
    console.log('\n=== Type Detection ===');
    console.log('Is StructTest a struct?', helper.IsStructType('StructTest'));
    console.log('Is StructTest[] a struct?', helper.IsStructType('StructTest[]'));
    console.log('Is int a struct?', helper.IsStructType('int'));
    console.log('Is string[] a struct?', helper.IsStructType('string[]'));
    
    // 测试数据转换
    console.log('\n=== Value Transform ===');
    const testJson = '{"value1":"test","value2":123,"value3":45.6}';
    const result = helper.TransformStructValue('StructTest', testJson);
    console.log('Input:', testJson);
    console.log('Output:', JSON.stringify(result, null, 2));
    
    // 测试数组转换
    console.log('\n=== Array Transform ===');
    const testArray = '[{"value1":"a","value2":1,"value3":1.1},{"value1":"b","value2":2,"value3":2.2}]';
    const arrayResult = helper.TransformStructValue('StructTest[]', testArray);
    console.log('Input:', testArray);
    console.log('Output:', JSON.stringify(arrayResult, null, 2));
}

testStructHelper().catch(console.error);
