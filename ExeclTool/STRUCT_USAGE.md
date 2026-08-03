# 子结构功能使用说明

## 功能概述

所有格式（JSON、TS、JS、CS、FlatBuffers、ProtoBuffers）都支持子结构，
xlsx 与 csv 两种数据源行为一致。

## 1. 表头与类型行的分工

标准表布局：

| 行号 | 内容 |
|------|------|
| 第 0 行 | 字段名 / 结构声明 |
| 第 1 行 | 类型 / 子字段名 |
| 第 2 行 | 中文注释 |
| 第 3 行起 | 数据 |

**结构字段的声明规则：等号左边是输出字段名，等号右边是结构名，子字段信息写在类型行。**

## 2. 三种子结构写法

### 方式1：多列拼一个结构（`属性名=结构名`）

同一个结构名的多列会合并成一个对象，子字段名写在类型行。

| 第 0 行 | id | into=attr | into=attr | into=attr |
|---------|----|-----------|-----------|-----------|
| 第 1 行 | int | name:string | value:int | coeff:float |
| 第 2 行 | 物品ID | 属性名 | 属性值 | 系数 |
| 第 3 行 | 1001 | 攻击 | 10 | 1.5 |

输出：

```json
{
  "1001": {
    "id": 1001,
    "into": { "name": "攻击", "value": 10, "coeff": 1.5 }
  }
}
```

### 方式2：多列拼一个结构数组（`属性名[索引]=结构名`）

带 `[N]` 索引时，同一属性名的多组列会合并成数组。

| 第 0 行 | id | info[0]=StructTest | info[0]=StructTest | info[1]=StructTest | info[1]=StructTest |
|---------|----|--------------------|--------------------|--------------------|--------------------|
| 第 1 行 | int | value1 | value2 | value1 | value2 |
| 第 2 行 | 装备ID | 属性1名 | 属性1值 | 属性2名 | 属性2值 |
| 第 3 行 | 1001 | 攻击 | 50 | 暴击 | 20 |

输出：

```json
{
  "1001": {
    "id": 1001,
    "info": [
      { "value1": "攻击", "value2": 50 },
      { "value1": "暴击", "value2": 20 }
    ]
  }
}
```

> `StructTest` 已在 `design/define/Struct.xlsx` 定义，子字段类型从定义中读取，
> 类型行只写子字段名即可。

### 方式3：单格内联结构（类型行写 `{字段:类型,...}`）

整个格子承载一个完整结构值，类型行直接给出结构定义。

| 第 0 行 | id | infoNew=attr | infoNewArray=attr |
|---------|----|--------------|-------------------|
| 第 1 行 | int | `{name:string,value:int,coeff:float}` | `{name:string,value:int,coeff:float}[]` |
| 第 2 行 | 物品ID | 新参数 | 新参数数组 |
| 第 3 行 | 1001 | `{攻击,10,1.5}` | `[{攻击,10,1.5},{防御,20,2}]` |

输出：

```json
{
  "1001": {
    "id": 1001,
    "infoNew": { "name": "攻击", "value": 10, "coeff": 1.5 },
    "infoNewArray": [
      { "name": "攻击", "value": 10, "coeff": 1.5 },
      { "name": "防御", "value": 20, "coeff": 2 }
    ]
  }
}
```

数据格子里的值**按结构定义的字段顺序**填写，元素之间用 `,` 或 `|` 分隔：

```
[{攻击,10,1.5},{防御,20,2}]
[{攻击,10,1.5}|{防御,20,2}]
```

## 3. 结构类型的来源

结构字段的类型有两个来源，优先级如下：

1. **`design/define/Struct.xlsx` 已定义**（如 `StructTest`、`Reward`）——
   类型行只写子字段名，类型从定义中取；生成代码时直接引用**全局类型**。
2. **表内声明**（如 `attr`）——类型行必须写成 `字段名:类型` 或
   `{字段名:类型,...}`；生成代码时按 `表类名 + 结构名` 生成**局部类型**。

### 命名示例

| 表类名 | 结构声明 | 是否已在 Struct.xlsx 定义 | 生成的类型名 |
|--------|----------|---------------------------|--------------|
| ItemCfg | `into=attr` | 否 | `ItemCfgAttr` |
| EquipmentCfg | `info[0]=StructTest` | 是 | `StructTest` |

## 4. 校验规则

- **未定义的结构必须声明字段类型**。类型行只写 `name` 而不写 `name:string`
  会直接报错：

  ```
  表 ItemConfig 中结构 attr 的字段 name 未声明类型，请在类型行写成 name:类型
  ```

- **同一张表内同名结构的字段类型必须一致**，否则报错：

  ```
  表 ItemConfig 中结构 attr 的字段 value 类型不一致：int 与 float
  ```

  这条规则对三种写法统一生效，即 `into=attr`、`infoNew=attr`、
  `infoNewArray=attr` 只要结构名都是 `attr`，字段类型就必须完全相同，
  并且只会生成一份共用的 `ItemCfgAttr` 类。

## 5. 生成产物示例

以 `3_物品表 copy.xlsx`（表类名 `ItemCfg`）为例。

**TypeScript**（`ItemCfgClass.ts`）：

```typescript
export class ItemCfgAttr {
    public name : string = "";
    public value : number = 0;
    public coeff : number = 0;
}

export class ItemCfg {
    public id : number = 0;
    public name : string = "";
    public price : number = 0;
    public into : ItemCfgAttr = new ItemCfgAttr();
    public infoNew : ItemCfgAttr = new ItemCfgAttr();
    public infoNewArray : ItemCfgAttr[] = [];
}
```

**FlatBuffers**（`ItemCfg.fbs`）：

```
table ItemCfgAttr {
    name : string;
    value : int;
    coeff : float;
}

table ItemCfg {
    id : int;
    name : string;
    price : int;
    into : ItemCfgAttr;
    infoNew : ItemCfgAttr;
    infoNewArray : [ItemCfgAttr];
}
```

**ProtoBuffers**（`ItemCfg.proto`）：

```protobuf
message ItemCfgAttr {
	string name = 1;
	int32 value = 2;
	float coeff = 3;
}

message ItemCfg {
	int32 id = 1;
	string name = 2;
	int32 price = 3;
	ItemCfgAttr into = 4;
	ItemCfgAttr infoNew = 5;
	repeated ItemCfgAttr infoNewArray = 6;
}
```

`EquipmentCfg` 引用的是全局结构，因此 `.fbs` / `.proto` / `.cs` 直接使用
`StructTest`，无需重复定义（定义统一放在 `Common.fbs` / `Common.proto`）。

## 6. 定义子结构

在 `design/define/Struct.xlsx` 中，一个 Sheet 即一个结构：

**Sheet 名：StructTest**

| 第 0 行 | value1 | value2 | value3 |
|---------|--------|--------|--------|
| 第 1 行 | string | int | float |

## 7. 定义枚举

在 `design/define/Enum.xlsx` 中，一个 Sheet 即一个枚举：

**Sheet 名：EnumTest**

| key | value |
|-----|-------|
| A | 1 |
| B | 2 |

## 8. 类型映射

### 基本类型

| 表内类型 | C# | TS | proto | fbs |
|----------|----|----|-------|-----|
| int / Int | int | number | int32 | int |
| float / Float | float | number | float | float |
| bool / Bool | bool | boolean | bool | bool |
| string / String | string | string | string | string |

### 数组类型

- `Type[]` → 一维数组
- `Type[,]` → 二维数组

### 结构类型

- `属性名=结构名` → 单个结构
- `属性名[N]=结构名` → 结构数组
- 类型行 `{...}` → 单格内联结构
- 类型行 `{...}[]` → 单格内联结构数组

## 9. CSV 支持

把 xlsx 换成同名目录、每个 Sheet 一个 `.csv` 文件即可，
表头和类型行的写法完全相同。含逗号的内联结构格子需要用双引号包裹：

```csv
id,name,price,into=attr,infoNewArray=attr
int,string,int,name:string,"{name:string,value:int,coeff:float}[]"
物品ID,物品名称,价格,属性名,新参数数组
1001,剑,100,攻击,"[{攻击,10,1.5},{防御,20,2}]"
```

转表时把 `param.json` 的 `format` 设为 `csv`。

## 10. 常见问题

**Q: 结构名一定要和输出字段名相同吗？**
A: 不需要。输出字段名取等号左边，结构名取等号右边，两者互相独立。
例如 `into=attr` 输出字段是 `into`，用的结构是 `attr`。

**Q: 同一个结构可以在一张表里用多次吗？**
A: 可以，且会共用同一个生成类，但字段类型必须完全一致。

**Q: 数组索引从 0 还是 1 开始？**
A: 从 0 开始，`info[0]` 是第一个元素。

**Q: 内联结构的数据格子要写字段名吗？**
A: 不需要，按类型行声明的字段顺序依次填值即可。
