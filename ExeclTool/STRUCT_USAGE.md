# 子结构数组功能使用说明

## 功能概述

现在所有格式（TS、JS、CS、FlatBuffers、ProtoBuffers）都支持子结构数组功能，与JSON格式保持一致。

## 1. 定义子结构

在 define 目录下创建Excel文件定义子结构：

**示例：define/Attr.xlsx**

| 字段名 | 字段类型 |
|--------|----------|
| name   | string   |
| value  | int      |
| desc   | string   |

Sheet名称即为结构体名称（如 Attr）

## 2. 配置define路径

在转换配置中可以自定义define路径：

\\\json
{
  \"sheets\": [[\"Sheet1\", \"ConfigName\"]],
  \"mergeName\": \"AllConfig\",
  \"toDir\": \"output\",
  \"definePath\": \"./define\"  // 可选，默认为 ../define
}
\\\

### 路径优先级
1. 配置的 definePath（绝对或相对路径）
2. 默认路径：../define（相对于Excel文件）
3. 如果路径不存在，输出警告但不影响转表

## 3. 在表格中使用子结构

### 方式1：直接使用结构体类型

**Excel表格列定义：**

| 字段名 | 字段类型 | 说明 |
|--------|----------|------|
| id     | int      | ID |
| attr   | Attr     | 属性结构体 |

**数据行：**
\\\
id: 1001
attr: {\"name\":\"攻击\",\"value\":50,\"desc\":\"攻击力\"}
\\\

### 方式2：使用结构体数组

**Excel表格列定义：**

| 字段名 | 字段类型 | 说明 |
|--------|----------|------|
| id     | int      | ID |
| attrs  | Attr[]   | 属性数组 |

**数据行：**
\\\
id: 1001
attrs: [{\"name\":\"攻击\",\"value\":50},{\"name\":\"防御\",\"value\":30}]
\\\

### 方式3：使用嵌套字段路径

**Excel表格列定义：**

| 字段名 | 字段类型 | 说明 |
|--------|----------|------|
| id     | int      | ID |
| attr.name   | string | 属性名称 |
| attr.value  | int    | 属性值 |

**数据行：**
\\\
id: 1001
attr.name: 攻击
attr.value: 50
\\\

生成的JSON：
\\\json
{
  \"1001\": {
    \"attr\": {
      \"name\": \"攻击\",
      \"value\": 50
    }
  }
}
\\\

### 方式4：数组索引字段

**Excel表格列定义：**

| 字段名 | 字段类型 | 说明 |
|--------|----------|------|
| id     | int      | ID |
| attrs[0].name  | string | 第1个属性名称 |
| attrs[0].value | int    | 第1个属性值 |
| attrs[1].name  | string | 第2个属性名称 |
| attrs[1].value | int    | 第2个属性值 |

**数据行：**
\\\
id: 1001
attrs[0].name: 攻击
attrs[0].value: 50
attrs[1].name: 防御
attrs[1].value: 30
\\\

生成的JSON：
\\\json
{
  \"1001\": {
    \"attrs\": [
      {\"name\": \"攻击\", \"value\": 50},
      {\"name\": \"防御\", \"value\": 30}
    ]
  }
}
\\\

## 4. 支持的格式

所有格式都支持子结构：

- ✅ **JSON** - 完整支持
- ✅ **TypeScript** - 完整支持
- ✅ **JavaScript** - 完整支持  
- ✅ **C#** - 完整支持（生成.cs代码）
- ✅ **FlatBuffers** - 完整支持（生成.fbs和.cs）
- ✅ **ProtoBuffers** - 完整支持（生成.proto和.cs）

## 5. FlatBuffers特殊说明

FlatBuffers会生成两个文件：

1. **.fbs schema文件** - 包含结构体定义和表格定义
2. **.cs解析代码** - C#静态类，用于读取FlatBuffer数据

## 6. ProtoBuffers特殊说明

ProtoBuffers会生成：

1. **.proto文件** - Protocol Buffers schema定义
2. **.cs解析代码** - C#类，用于序列化/反序列化

## 7. 类型映射

### 基本类型
- int/Int → int32
- float/Float → float
- bool/Bool → bool
- string/String → string

### 数组类型
- Type[] → Type数组
- Type[,] → Type二维数组

### 结构体类型
- StructName → 自定义结构体
- StructName[] → 结构体数组

## 8. 常见问题

**Q: define路径不存在会报错吗？**
A: 不会，只会输出警告，不影响转表。如果没有使用自定义结构体，可以忽略警告。

**Q: 如何添加新的结构体？**
A: 在define目录下创建新的Excel文件，Sheet名为结构体名，第一行为字段名，第二行为字段类型。

**Q: 结构体字段可以是另一个结构体吗？**
A: 可以，支持多层嵌套结构体。

**Q: 数组索引从0还是1开始？**
A: 从0开始，attrs[0]表示第一个元素。
