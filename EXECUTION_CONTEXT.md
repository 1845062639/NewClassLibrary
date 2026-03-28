# StandardTestNext 执行基线（EXECUTION_CONTEXT）

> 作用：这是 StandardTestNext 项目的长期执行基线文件。后续新开对话时，如涉及本项目，应优先读取本文件恢复项目状态，再继续执行。

## 1. 项目目标

在不改动旧系统交付代码的前提下，以旧 `StandardTest` / `StandardTestApp` / `ClassLibrary` / `stp.db` 为参考，在 `next-gen` 仓库中重建一套新的 StandardTestNext 系统，并优先在本周跑通 Motor_Y 试点业务的可运行最小闭环。

## 2. 当前正式方案基线

- 当前正式方案文档：`/root/.openclaw/workspace/output/StandardTestNext_系统改造方案_v1.0.docx`
- 后续代码改造、结构调整、任务拆解均以该方案为当前主基线。

## 3. 已确认的核心约束

### 3.1 职责边界
- `test = StandardTest = 核心测试平台层`
- `app = StandardTestApp = 现场项目定制适配层`
- 不得再反向理解。

### 3.1.1 旧系统目录职责补充（高优先级）
- `StandardTest.Library/Algorithm`：放的是国标计算方法，重要性极高；其中 `Algorithm/Motor/Algorithm_Motor_Y.cs` 是 Motor_Y 算法迁移的核心基线。
- `StandardTest.Library` 主要负责定义对象结构。
- `Data`：原始数据结构。
- `RatedParams`：型号对应额定参数结构。
- `TestData`：试验参数/试验数据结构。
- `Model` / `ViewModel`：主要是数据库相关对象。
- `StandTest` 文件夹：主要负责前端显示。
- 做 next-gen 算法迁移时，应优先围绕 `RatedParams + Data + TestData + Algorithm` 理解与映射，不要把前端显示层或数据库对象层误当算法权威来源。

### 3.1.2 数据库实体设计基线（高优先级）
- 做数据库实体设计时，优先直接查看 `stp.db` 的真实表结构与真实数据，不要先依赖代码里的数据库对象定义。
- `stp.db` 不只是结构样板，里面还有实际数据；应优先从中提炼实体、字段口径、关系、值域与历史兼容情况。
- `Model` / `ViewModel` 可作为辅助手段参考，但数据库实体设计以 `stp.db` 为一手基线。
- 已确认旧库核心业务表：`ProductTypes`、`TestRecords`、`TestRecordItems`、`TestRecordAttachments`、`TestRecordItemAttachments`、`FileAttachments`。
- 已确认旧库是真实业务库而非空样板：当前抽样统计约为 `ProductTypes=302`、`TestRecords=576`、`TestRecordItems=2147`、`TestRecordItemAttachments=1384`。
- 已确认旧库记录聚合模型：`TestRecords` 为主记录头，`TestRecordItems` 通过 `TestRecordId` 挂载业务试验项；业务项名称直接体现在 `TestRecordItems.Code`（如“直流电阻测定”“热试验”“空载试验”），`Method` 为整数枚举，`Data` 字段中保存旧 `TestData_*` JSON 结构；附件通过 `TestRecordAttachments` / `TestRecordItemAttachments` 与 `FileAttachments` 关联。

### 3.2 app / test 通讯方式
- app 与 test 之间使用 **ModbusTcp** 交互
- **app 为 client**
- **test 为 server**
- 支持同机部署，也支持局域网内分机部署；分机场景按工业现场同交换机、同局域网设计即可。

### 3.3 技术路线
- .NET 最新版本
- WPF
- SQLite + SqlSugar
- HslCommunication
- ModbusTcp
- 完整权限框架
- 多语言：简中 / 英文 / 繁中
- 后续预留 SCADA / 重型组态架构插槽

### 3.4 Motor_Y 第一阶段最小闭环
包含：
- 直流电阻
- 空载试验
- 热试验
- A 法负载试验
- B 法负载试验
- 堵转试验
- 报表导出
- 附件链路

### 3.5 app 参考对象
`CYDJ_20220921` 重点参考：
- 业务流程
- 配置方式
- 整体样板

### 3.6 权限与多语言
- 权限第一阶段进入较复杂的数据权限设计
- 部门权限影响数据可见范围
- 多语言第一阶段尽量覆盖全部页面与报表
- 报表第一阶段按“重构报表体系”处理

### 3.7 代码实施策略
- `next-gen` 仓库内新建更规范的目录骨架，再逐步迁移与替换
- 第一阶段先交付**可运行最小闭环**
- git 采用**小步频繁提交**，并使用中文提交信息

## 4. 当前待定项

以下仍未最终拍板：
1. `test` 的职责边界是否明确到：A 仅采集 / B 采集+基础计算 / C 采集+试验执行流程控制
2. 客户或外部角色是否进入新系统

答复为空的项目一律视为待定，不得自行脑补。

## 5. 当前代码执行状态

- `next-gen` 仓库已做一次保护性快照提交：
  - `chore: 快照保留现有 next-gen 状态，准备按 v1.0 方案重构`
- 当前已正式进入初版代码编写阶段。
- 当前重点主线：
  1. 深挖旧代码关键链路（STPApiClient / TestRecordHelper / CYDJ_20220921 / Motor_Y / ClassLibrary）
  2. 按 v1.0 方案重构 `next-gen` 骨架
  3. 准备 Motor_Y 最小闭环落地
- 已新增 App 查询网关 `sqlite-inproc` 模式：App 可直接读取 Test 持久化到 SQLite 的真实记录/附件/报告摘要，不再只能消费 seeded in-proc 假数据。
- 已完成一轮真实闭环验证：使用 Test `--persistence sqlite` 写入 `/root/.openclaw/workspace/next-gen/artifacts/test-persistence/app-readback-demo.db`，随后由 App `--query-gateway sqlite-inproc` 成功回读同一库中的 recent list / detail / report artifact。
- 本轮同时修复 SQLite 聚合回读缺口：`TestRecordAggregate.Items` 与 `TestRecordItemAggregate.Attachments` 已改为可反序列化回填，解决了“SQLite 中 AggregateJson 完整但回读后 item/sample 统计为 0”的假通问题。
- 当前 demo 记录骨架已从纯技术分桶推进到一版 Motor_Y 业务试验项骨架：在保留 `RealtimeKeyPoints` / `RealtimeContinuous` 的同时，新增 `MotorY.DcResistance`、`MotorY.NoLoad`、`MotorY.HeatRun`、`MotorY.LoadA`、`MotorY.LoadB`、`MotorY.LockedRotor` 六类试验项，用于让后续详情页、报表与方法映射围绕业务项而非仅围绕采样桶推进。
- 已锁定旧系统对应基线：`Settings_CYDJ.json` 中的 Motor_Y 方法名与 `StandardTest.Library/TestData/Motor/Y/*`、`Algorithm/Motor/Algorithm_Motor_Y.cs` 中的对象/算法入口已确认可作为 next-gen 对齐来源；当前已开始让 payload reader 识别 `DataList / RawDataList / ResultDataList / Data1List / Data2List` 这类旧结构形状，避免业务试验项在详情查询中全部显示为 0 样本空壳。
- `MotorYTrialRecordBuilder` 已开始把 6 个业务试验项 payload 直接改造成接近旧 `TestData_Motor_Y_*` 的 JSON 结构：直流电阻对齐 `Ruv/Rvw/Rwu/R1/θ1c/ΔR/R`，空载对齐 `DataList + P0/I0/Pfw/Pfe`，堵转对齐 `DataList + Ikn/Pkn/Tkn`，热试验对齐 `Data1List/Data2List + θw/Δθ`，A/B 法对齐 `RawDataList/ResultDataList` 主字段，为后续迁移算法与报表字段打底。
- 已在 next-gen 内新增轻量兼容层 `MotorYNoLoadLegacyShape` + `MotorYNoLoadLegacyPreviewFormatter`：先不强行引入旧 net48 `StandardTest.Library`，而是在当前 net8 主干中验证 NoLoad payload 是否已经具备“可按旧对象形状成功反序列化”的条件，为后续逐项增加 legacy-shape mapper/adapter 做试点。
- 该模式已开始扩展到 `Lock_Rotor / Thermal / Load_A / Load_B`：当前 next-gen 已补 `MotorYLockRotorLegacyShape`、`MotorYThermalLegacyShape`、`MotorYLoadALegacyShape`、`MotorYLoadBLegacyShape` 及预览器，方向是先在 net8 主干内证明“这些业务项 payload 已具备被旧对象形状稳定消费的条件”，再决定是否有必要引入更深的算法适配层。
- 本轮已把 Motor_Y 业务项闭环验证再向前推进一格：新增 smoke tests，直接用 `MotorYTrialRecordBuilder` 生成 6 个核心试验项，再校验 `TestRecordItemPayloadReader` 与 `TestRecordQueryGatewayAdapter` 对 `DataList / RawDataList / Data1List / Data2List` 推断出的 `SampleCount / RecordMode` 是否稳定，确保“builder -> payload reader -> app query”链路对 Motor_Y 业务 payload 可验证。
- 已继续把 `stp.db` 真实结构驱动往前推进：`StpDbSnapshotQueryService` 现在除返回 `ProductTypes.RatedParams` 原始 JSON 外，也会直接归一化解析为 next-gen `MotorRatedParamsContract`（含功率单位、接法枚举、极对数补全等），并通过 smoke test 强约束最近 Motor_Y 记录的额定参数必须可被 next-gen 查询层直接消费，为后续 `stp.db -> next-gen 实体/查询模型 -> builder/算法适配` 打基础。
- 本轮进一步把旧库额定参数“归一化口径 + 原始枚举口径”同时保留下来：`MotorRatedParamsContract` 新增 `DutyRaw / ConnectionRaw`，`StpDbSnapshotQueryService` 会把 `RatedParams` 里的旧枚举值原样暴露，同时保留 next-gen 友好的 `Duty / Connection` 归一化字段，并由 smoke test 锁定，方便后续 Motor_Y 算法适配层直接按旧库枚举做映射校验。
- 本轮继续把 `stp.db` 的真实方法枚举口径纳入 next-gen 查询模型：`StpDbTestRecordItemSnapshot` 新增 `CanonicalCode / MethodKey`，显式保留 `TestRecordItems.Method` 与归一化业务项编码的组合（如 `NoLoad:0`、`LoadA:60`、`LockedRotor:47`），并新增 `StpDbMotorYMethodMappingSmokeTests` + snapshot smoke 断言，锁定当前旧库中 Motor_Y 核心试验项的真实 Method 值域，为后续对齐旧 `Algorithm_Motor_Y.cs` 的方法分支映射做准备。

## 6. 参考范围

### 旧系统/业务参考
- `/root/.openclaw/workspace/app-repos/StandardTestApp`
- `/root/.openclaw/workspace/ClassLibary/StandardTest*`
- `/root/.openclaw/workspace/ClassLibrary`
- `/root/.openclaw/workspace/stp.db`
- 旧改造文档

### 新架构/新代码参考
- `/root/.openclaw/workspace/next-gen`
- `/root/.openclaw/workspace/reference/MotorTest`

## 7. 新对话恢复规则

凡是后续新开对话只要涉及 StandardTestNext / StandardTest 改造任务：
1. 必须优先读取本文件。
2. 以本文件作为项目当前状态恢复入口。
3. 再去读取最新方案文档与相关代码。
4. 不要让用户重复介绍项目背景。

## 8. 维护规则

本文件需要持续维护：
- 方案基线变化时更新
- 关键约束变化时更新
- 代码阶段变化时更新
- 新确认 / 新待定项变化时更新

