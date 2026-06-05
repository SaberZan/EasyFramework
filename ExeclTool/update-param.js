const param = {
    'target': 'JSON',
    'params': {
        'toCode': 'csharp'
    },
    'designPath': '.\\\\design',
    'output': {
        'code': './tmp/json',
        'bin': './tmp/json'
    },
    'enum': {
        'Enum1': {
            'sheets': [
                'EnumTest'
            ]
        }
    },
    'struct': {
        'Struct1': {
            'sheets': [
                'StructTest'
            ]
        }
    },
    'config': {
        '1_常量表': {
            'sheets': [
                ['常量', 'GameConstCfg']
            ]
        },
        '2_关卡表': {
            'sheets': [
                ['Map', 'MapCfg'],
                ['Monster', 'MonsterCfg']
            ],
            'mergeName': 'LevelCfg',
            'toDir': 'Level'
        },
        '3_物品表': {
            'sheets': [
                ['ItemConfig', 'ItemCfg']
            ]
        }
    }
};

const fs = require('fs');
fs.writeFileSync('./input/param.json', JSON.stringify(param, null, 4));
console.log('Updated param.json with struct test');
