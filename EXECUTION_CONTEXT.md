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
- 本轮进一步把 `Algorithm_Motor_Y.cs` / `Settings_CYDJ.json` 的方法入口名沉到查询快照层：`MotorYMethodProfileCatalog` 现在除 `LegacyAlgorithmEntry` 外，还显式暴露 `LegacyMethodName / LegacySettingsMethodName`，`StpDbSnapshotQueryService` 会把它们挂到 `StpDbTestRecordItemSnapshot` 上，并由 `MotorYMethodProfileCatalogSmokeTests` 校验基线方法号（1/0/3/4/5/11）与旧算法入口、旧业务项名称、旧配置方法名三者一致，方便后续直接做 Motor_Y 算法/配置适配，不必再靠字符串猜测。
- 本轮继续把旧 Motor_Y 的“方法号 -> 旧枚举名/旧窗体名”路由信息补齐：`MotorYMethodProfileCatalog` 新增 `LegacyEnumName / LegacyFormName`，并把 `stp.db` 中真实出现的 53/59/47/48/46 等变体方法号映射到 `FrmMotor_Y_*` 与对应旧枚举别名；`StpDbSnapshotQueryService` 已把这些字段挂到 `StpDbTestRecordItemSnapshot`，并由 smoke test 锁定，方便后续直接做 `stp.db Method -> 旧页面/旧算法适配入口 -> next-gen 适配层` 闭环对齐。
- 本轮又把上述字符串路由再前推一格：新增 `MotorYLegacyAlgorithmRouteResolver`，把 `CanonicalCode + Method` 解析为结构化 `LegacyAlgorithmRoute`（含 MethodKey/ProfileKey/旧枚举名/旧窗体名/旧算法入口/旧配置方法名/基线标记），并在 `StpDbSnapshotQueryService` 快照项上直接暴露 `LegacyAlgorithmRoute`；同时补 smoke test 锁定 route/profile 投影一致性，方便后续 Motor_Y 适配层直接消费，而不是重复拼字符串。
- 本轮继续把 Motor_Y 的旧方法分支语义再结构化一层：`MotorYMethodProfile` / `LegacyAlgorithmRoute` / `StpDbTestRecordItemSnapshot` 已新增 `VariantKind + AlgorithmFamily`，把 stp.db 里的 baseline / delivery / companion / legacy-alias 等真实变体标签与 DirectCurrentResistance / NoLoad / Thermal / LoadA / LoadB / LockedRotor 算法家族直接沉到 next-gen 查询模型；并补 smoke test 锁定 profile -> route -> snapshot 三层投影一致，便于后续 builder / adapter / 报表直接按真实旧方法分支做适配。
- 本轮已把这套结构化旧方法语义从 `stp.db` 快照层继续打通到 demo/builder/query 闭环：`MotorYTrialRecordBuilder` 产出的业务试验项经 `BuildProfile` 携带 baseline `MethodKey/ProfileKey/LegacyEnumName/LegacyFormName/LegacyAlgorithmEntry/LegacySettingsMethodName`，`TestRecordQueryGatewayAdapter` 也已把这些字段投影到 App 查询 contract，并由 smoke test 锁定 `builder -> query detail` 一致性，后续 App/报表可直接消费旧算法路由而不必只靠 snapshot 或字符串猜测。
- 本轮又把 `stp.db` 的真实 Method 分布次数直接沉到 next-gen 查询层：`StpDbSnapshotQueryService` 新增 `ListMotorYMethodDistribution()`，按 `CanonicalCode + Method` 输出真实出现次数及结构化 legacy route 元数据（ProfileKey / VariantKind / AlgorithmFamily / Enum/Form/Algorithm 入口），并新增 `StpDbMotorYMethodDistributionSmokeTests` 锁定当前旧库主分布（如直流电阻 1=431、热试验 3=430、A法负载 60=61 等），为后续按真实主流分支确定 Motor_Y 算法适配优先级与报表默认口径提供依据。
- 本轮继续把 `stp.db` 真实额定参数口径沉到 next-gen 查询层：`StpDbSnapshotQueryService` 新增 `ListMotorRatedParamsValueDistribution()`，直接统计 `ProductTypes.RatedParams` 中 `Duty / Connection` 的原始值与归一化值分布，并新增 `StpDbMotorRatedParamsDistributionSmokeTests` 锁定当前旧库真实值域（如 `Duty=0 -> ""`、`Connection=0/1 -> Y/Δ`），避免后续 Motor_Y builder / 算法适配只凭单条样本假设额定参数枚举口径。
- 本轮又把 `stp.db` 的“基线方法 vs 当前真实主流方法”差异收敛成结构化决策快照：`StpDbSnapshotQueryService` 新增 `ListMotorYMethodDecisions()`，直接输出 `BaselineRoute / DominantRoute / DominantShare / ShouldPrioritizeDominantOverBaseline`，并补 `StpDbMotorYMethodDecisionSmokeTests` 锁定当前旧库真实口径（例如当前直流电阻主流已是 `1` 而非 `53`）；`ListMotorYMethodRecommendations()` 也改为复用该决策层，方便后续 builder / 算法适配 / 报表直接按“旧基线 + 真实分布”双视角消费，而不必在多处重复拼判定逻辑。
- 本轮继续把这套方法决策能力向 App 查询闭环前推：`MotorYMethodDecisionContract` / `MotorYMethodDecisionSnapshot` 新增 `Distributions`，`TestRecordViewMapper` 会从同一记录内的 `BuildProfile` 聚合出各 Method 的次数与占比，`TestRecordQueryGatewayAdapter` 也已把该分布明细投影到 App detail contract，并由 smoke test 锁定 `detail -> MotorYMethodDecisions -> Distributions` 的数值与 baseline 标记一致，方便后续 App/报表直接按“基线 + 主流 + 全量分布”消费真实 Method 口径。
- 本轮又把同一能力补到 `stp.db` 真实快照决策层：`StpDbSnapshotQueryService.ListMotorYMethodDecisions()` 现在也直接返回每个核心业务项的 `Distributions(MethodValue/Count/Share/Route)`，不再只有 baseline/dominant 两个点；并补强 `StpDbMotorYMethodDecisionSmokeTests` 校验分布条数、占比与 route 投影一致性。这样后续做 Motor_Y 算法适配、报表默认方法选择时，可直接复用 `stp.db -> method decision` 的统一结构，而不是在 snapshot/app/builder 三处重复聚合。
- 本轮继续把这套“基线 vs 主流”决策结果收敛成更适合算法适配层直接消费的摘要：`MotorYMethodDecisionSnapshot/Contract` 新增 `BaselineShare / DominantLeadCount / DominantLeadPercentagePoints / RecommendationReason`，并同步打通 `StpDbSnapshotQueryService`、`TestRecordViewMapper`、`TestRecordQueryGatewayAdapter` 与 smoke test，保证无论来自 `stp.db` 真实分布还是 builder->query 闭环，App/后续适配层都能直接拿到统一的推荐原因与领先幅度，而不必再次手算 baseline/dominant 差值。
- 本轮又把同一批 share/gap 摘要补进 `MotorYMethodAdaptationPlanContract`：适配计划现在除 `DominantShare` 外，也直接暴露 `BaselineShare / SelectedShare / SelectedLeadCountVsBaseline / SelectedLeadPercentagePointsVsBaseline`，并由 query smoke test 锁定 NoLoad 场景下的数值。这样后续 Motor_Y 算法适配器、报表默认方法选择或 UI 提示，可直接消费“基线 vs 主流 vs 实际选中”三路占比与领先幅度，无需在 App 侧再次手算。
- 本轮继续把同一套方法选择摘要向 App detail 闭环补齐：`MotorYMethodDecisionContract` 新增 `RecommendedMethodSummary / BaselineDominantComparisonSummary`，直接复用 `MotorYMethodRouteSelectionSnapshotFactory` 产出“推荐方法摘要 + 基线/主流对比摘要”，并由 query smoke test 锁定字符串内容。这样后续 App 页面、报表或算法适配提示层读取 detail 时，不必再自行拼装 `baseline vs dominant vs recommended` 的说明文案。
- 本轮继续把同一套“推荐方法摘要 + 基线/主流对比摘要”打通到 `stp.db` 真实快照决策层：`MotorYMethodDecisionSnapshot` 新增 `RecommendedMethodSummary / BaselineDominantComparisonSummary`，`StpDbSnapshotQueryService.ListMotorYMethodDecisions()` 现已直接复用 `MotorYMethodRouteSelectionSnapshotFactory` 产出统一摘要，避免 `stp.db snapshot` 与 `builder -> query detail` 两套文案口径漂移；并补 `StpDbMotorYMethodDecisionSmokeTests` 锁定真实库下 6 个 Motor_Y 核心试验项的摘要字符串与推荐策略一致性，方便后续算法适配/报表直接消费真实库决策摘要。
- 本轮又把这套摘要能力继续补到 `stp.db -> MotorYMethodAdaptationPlanSnapshot`：适配计划快照现在也直接暴露 `BaselineShare / SelectedShare / SelectedLeadCountVsBaseline / SelectedLeadPercentagePointsVsBaseline / SelectedMethodSummary / BaselineDominantComparisonSummary`，并改为复用 `MotorYMethodRouteSelectionSnapshotFactory` 统一产出，`StpDbMotorYMethodAdaptationPlanSmokeTests` 已锁定真实库下 6 个核心试验项的数值与摘要，避免 snapshot/contract/app 三层再次各自手算、各自拼文案。
- 本轮继续把 Motor_Y 方法选择逻辑从“多处各写一遍”收敛成共享工厂：新增 `MotorYMethodDecisionFactory`，统一产出 `Baseline/Dominant/Recommended/Share/Lead/Reason/Summary` 全套决策字段，并让 `stp.db` 真实快照层与 `builder -> query detail` 闭环同时复用同一实现；同时把阈值口径收敛为共享常量，避免后续算法适配、报表提示或 App 查询层在 70% override 规则上出现漂移。
- 本轮继续把旧 `Algorithm_Motor_Y.cs` 的“算法入口依赖什么上游试验/额定参数/关键字段”从代码隐式逻辑沉成显式模型：新增 `MotorYLegacyAlgorithmDependencyCatalog`，为 6 个核心试验项结构化记录 `RequiresRatedParams / UpstreamCanonicalCodes / RequiredPayloadFields / RequiredRatedParamFields / Notes`；同时把这些依赖画像投影进 `MotorYMethodAdaptationPlanSnapshot/Contract` 与 `stp.db` 适配计划查询结果，并新增 smoke test 锁定。例如 Load_B 现可直接在 next-gen 适配计划中看出其依赖 `NoLoad + HeatRun`、需要 `θw/θb/Pfw/CoefficientOfPfe` 与额定参数 `GB`，为后续真正迁移算法 adapter 做输入清单基线。
- 本轮又把上游依赖画像从“只看缺没缺”前推到“看见了哪些已满足上游”：`MotorYMethodAdaptationPlanSnapshot/Contract` 新增 `ObservedUpstreamCanonicalCodeCount / ObservedUpstreamCanonicalCodes`，`MotorYUpstreamDependencySnapshotFactory` 与 `stp.db / builder->query` 两条链路统一输出 `observed x/y required upstream codes` 摘要；并由 smoke test 锁定 builder 场景下 `NoLoad` 缺 `DcResistance`、stp.db 场景下 `Load_B` 已观测到 `NoLoad + HeatRun`。这样后续做 Motor_Y 算法适配时，能直接区分是 demo/build 闭环没挂上游项，还是历史实库本身缺依赖数据。
- 本轮继续把旧 `Algorithm_Motor_Y.cs` 的“算法需要产出哪些关键结果字段”也显式沉到 next-gen 适配计划：`MotorYLegacyAlgorithmDependencyCatalog` 为 6 个核心试验项新增 `RequiredResultFields`（例如 NoLoad 的 `Pfw/Pfe/CoefficientOfPfe`、Thermal 的 `Rw/Rn/Δθ/θw`、Load_B 的 `A/B/R/θs/ResultDataList`、LockedRotor 的 `Ikn/Pkn/Tkn`），并同步投影到 `MotorYMethodAdaptationPlanSnapshot/Contract` 与 `stp.db` 查询结果；同时补 smoke test 锁定 Load_B 的结果字段清单。这样后续真正写 Motor_Y adapter 时，既能看见“需要什么输入”，也能直接看见“至少应回填哪些旧算法关键结果”，减少只补入参不补产出的半截迁移风险。
- 本轮又把 `FormulaSignals / LegacyAlgorithmRules` 从“静态清单展示”推进到“真实 payload 观测覆盖”口径：`MotorYMethodAdaptationPlanContractMapper` 与 `StpDbSnapshotQueryService` 现在不再把这些条目默认算作 100% 覆盖，而是改为基于 `MotorYObservedAlgorithmEvidenceCatalog` 的命中字段去计算覆盖率、缺口与摘要；并通过 smoke test 锁定 `builder -> query` 场景下 Load_B 公式/规则覆盖应为 0、`stp.db` 真实样本下应为 100%，让后续 Motor_Y 算法适配前的“语义证据是否到位”可直接在闭环里验证。
- 本轮继续收敛 `MotorYMethodAdaptationPlan` 在 `builder -> query` 与 `stp.db` 快照两条链路上的“算法输入就绪”判定口径：`StpDbSnapshotQueryService` 现已复用与 `MotorYMethodAdaptationPlanContractMapper` 同结构的摘要规则，把 `上游依赖 + 必需 payload 字段 + 额定参数字段 + 结果字段 + RawData 信号 + Structured payload/result 信号` 一起纳入 `LegacyAlgorithmInputsReady / LegacyAlgorithmInputReadinessSummary`；并通过 Motor_Y / stp.db / query 相关 smoke tests 验证通过，避免后续 App、报表或适配器面对同一业务项时出现 snapshot 说 ready、query 说 incomplete 的口径漂移。
- 本轮继续把旧 `Algorithm_Motor_Y.cs` 的“阶段性中间结果产物”从结果字段里拆出来单独建模：`MotorYLegacyAlgorithmDependencyCatalog` 为 6 个核心试验项新增 `RequiredIntermediateResultFields`（例如 NoLoad 的 `R0/θ0/Pcon/P0cu1`，HeatRun 的 `firstSecondsInterval/Rws`，Load_B 的 `Tx/P2tx/Pl/Ps/cuC`），并同步打通到 `MotorYMethodAdaptationPlanSnapshot/Contract`、`StpDbSnapshotQueryService` 与 `builder -> query` 映射层；同时补 smoke test 锁定 Load_B 在真实 `stp.db` 下的中间结果覆盖摘要。这样后续真正迁移 Motor_Y adapter 时，可以直接区分“最终回填结果”与“算法过程必须生成的中间锚点”，减少只对齐终值却遗漏关键计算阶段的风险。
- 本轮又把上述“中间结果锚点”正式纳入 `LegacyAlgorithmInputsReady / ObservedAlgorithmInputFields / MissingAlgorithmInputFields` 的统一口径：`MotorYMethodAdaptationPlanContractMapper` 与 `StpDbSnapshotQueryService` 现在会把 `RequiredIntermediateResultFields` 与 payload/result/raw-data/structured-signal 一起计入算法输入覆盖摘要，并由 query smoke test 锁定 NoLoad 场景下缺失 `R1` 等冷态中间量的说明文案。这样后续判断某个 Motor_Y 业务项是否真正具备旧算法迁移条件时，不会再出现“终值字段齐了但关键中间锚点仍缺失却被误判 ready”的偏差。
- 本轮继续把这套口径真正收敛到 ready 判定本身：`MotorYMethodAdaptationPlanContractMapper` 与 `StpDbSnapshotQueryService` 现已把 `RequiredIntermediateResultFields` 缺口正式纳入 `LegacyAlgorithmInputsReady / LegacyAlgorithmInputReadinessSummary` 判定条件，不再只在覆盖摘要里展示却不影响 ready 结果；并补 query smoke test 锁定 `NoLoad` 场景下 ready=false 且 readiness summary 必须显式包含中间结果覆盖段。这样后续无论来自 `builder -> query detail` 还是 `stp.db` 真实快照，面对缺失 `R1/P0cu1/Pcon/Pfw` 等关键中间锚点的 Motor_Y 业务项，都不会再被误判为“旧算法输入已就绪”。
- 本轮又补齐了 `Test -> Query -> App读取` 闭环中的一个字段投影缺口：`TestRecordQueryGatewayAdapter` 现在已把 `RequiredIntermediateResultFields` 及其 covered/missing/count/ratio/summary 全量映射到 App query contract，并由 `TestRecordQueryGatewayAdapterSmokeTests` 明确断言 `NoLoad` 场景下中间结果字段清单与覆盖摘要，避免 Test/SQLite 快照层已有的 Motor_Y 算法中间锚点信息在 App 读取侧被静默丢失。
- 本轮把 `StandardTestNext.Test` 的 CLI/demo 输出继续前推到 Motor_Y 适配计划可读层：`TestBootstrap.FormatMethodAdaptationPlanSnapshot()` 现会直接打印 raw/structured sample gate、decision anchor readiness 与 payload/result/intermediate/raw/structured 覆盖率摘要。这样每次跑 `dotnet run --project StandardTestNext.Test` 时，不仅能验证 `Test -> SQLite -> App读取` 闭环，还能在控制台一眼看到 `stp.db` 真实方法分布下各核心试验项距离旧 `Algorithm_Motor_Y.cs` 输入就绪还差什么，便于后续 adapter 迁移按缺口推进。
- 本轮又把上述 CLI/demo 可读层再补一格：`FormatMethodAdaptationPlanSnapshot()` 现在会额外输出 `DependencyBuckets` 摘要（upstream/rated-params/payload/result/intermediate/raw/structured/formula/rules/decision anchors 等每类依赖的 covered/missing 比例与缺口预览），这样每次做 `Test -> SQLite -> App读取` 验证时，控制台可以直接按依赖分类观察 Motor_Y 当前最缺哪一类输入，为后续优先补 builder payload、真实库字段映射或算法 adapter 提供更直接的推进锚点。
- 本轮继续把 Motor_Y 决策锚点能力往 `App读取` 闭环补齐：`MotorYMethodAdaptationPlanContractMapper` 现在会把 `LegacyDecisionAnchorResolutions` 的完整字段（`RequiredPayloadFields / ObservedPayloadFields / CoverageRatio / CoveragePercentagePoints / ResolutionStage`）一并映射到 contract，不再只透出 `MissingPayloadFields + Summary`；同时补了 query smoke test 明确锁定 `NoLoad` 场景下 3 个决策锚点 resolution 的完整结构。这样后续 App 页面、报表提示或算法 adapter 可直接消费“这个锚点为什么 missing/partial、已经观测到了哪些字段、覆盖率是多少”，避免再回退到 Test/snapshot 层重算。
- 本轮又把上述决策锚点信息进一步收敛成统一的“缺口预览摘要”：新增 `LegacyDecisionAnchorGapPreviewSummary`，由 `MotorYDecisionAnchorResolutionFactory` 统一输出 Top 缺口锚点及其缺失 payload 字段预览，并打通 `stp.db` 快照层、`builder -> query detail -> App contract` 映射与 `StandardTestNext.Test` CLI/demo 输出；同时补 query smoke test 锁定 `NoLoad` 场景摘要字符串。这样后续推进 `Algorithm_Motor_Y.cs` 真正适配时，可以直接按闭环里暴露的摘要优先补 `RConverseType / Pfw / CoefficientOfPfe...` 等关键字段，而不必再手工翻 resolution 列表找最先该补什么。
- 本轮继续把这套决策锚点闭环往“开发时肉眼可读”推进：`StandardTestNext.Test` 的 `FormatDecisionAnchorResolutionPreview()` 现已在 CLI/demo 的 `anchor-resolutions=` 片段中直接输出每个锚点的 `SuggestedNextStepSummary`，例如可直接看到 `rconverse-branch -> RConverseType`、`ps-iteration -> Ps` 之类的补齐建议；同时新增 `TestBootstrapFormattingSmokeTests` 锁定该格式，确保每次执行 `Test -> SQLite -> App读取` 闭环验证时，控制台就能直接暴露 Motor_Y 当前最该补的旧算法决策字段，而不必再回头翻 contract/snapshot 明细。
- 本轮把 `HeatRun / LoadA` 的决策锚点 next-step 提示也补齐到与 `DcResistance / NoLoad / LoadB / LockedRotor` 同一口径：`MotorYDecisionAnchorResolutionFactory` 新增针对热试验与 A 法负载试验的 `SuggestedNextStepCategory / Focus` 映射，并补 `TestRecordQueryGatewayAdapterSmokeTests` 锁定 `builder -> query -> App contract` 闭环下的建议文案、英文 next-action 摘要与 resolution 字段结构，确保后续 App/报表读取时能直接看到诸如 `Pn / HotStateType / GB...`、`CoefficientOfPfe / Pfw / θa...` 的优先补齐建议。
- 本轮继续把 Motor_Y 决策锚点的“建议动作”语义完整沉到 App 读取闭环：`MotorYMethodAdaptationPlanContract` / `MotorYMethodAdaptationPlanContractMapper` / `TestRecordViewMapper` 已统一承载并回投 `SuggestedNextStepPriority / SuggestedNextStepPrioritySummary / SuggestedNextStepCoverageSummary`，让 `builder -> query -> App contract` 不只知道“下一步补什么字段”，还能直接知道优先级、覆盖率与摘要；并通过 `TestRecordQueryGatewayAdapterSmokeTests` 锁定相关字段，随后执行 `dotnet build StandardTestNext.sln -c Debug --no-restore` 验证通过，确保 Test -> SQLite -> App读取 闭环里的 Motor_Y 决策锚点建议语义可稳定消费。
- 本轮又补齐了 Motor_Y 决策锚点在 App 读取闭环中的一个字段投影缺口：`MotorYDecisionAnchorResolutionContract` 里已有 `SuggestedPrimaryNextField / SuggestedPrimaryNextFieldSummary`，但 `TestRecordQueryGatewayAdapter` 之前未从 snapshot 回投，导致 Test/stp.db 层已算出的“首要补哪个字段”在 App detail 侧被静默丢失；现已完成映射，并新增 `TestRecordQueryGatewayAdapterSmokeTests` 锁定 Load_B 四类决策锚点（`GB / R / Ps / θw`）的主字段与摘要，同时执行 `dotnet build` 与 `dotnet run --project StandardTestNext.Test` 验证通过，确保 `Test -> SQLite -> App读取` 闭环可直接消费首要补字段建议。
- 本轮继续把 `stp.db` 真实结构驱动的 Motor_Y 适配计划往“跨试验项优先级总览”推进：`StpDbSnapshotQueryService` 新增 `ListMotorYDecisionAnchorPrimaryFieldFocuses()`，会基于 6 个核心试验项各自的 `DecisionAnchorPrimaryFieldDistributions` 聚合出跨 plan 的 primary-field 焦点分布（字段出现于多少核心试验项、对应哪些 canonical code / anchor / priority），并补 `StpDbMotorYMethodAdaptationPlanSmokeTests` 锁定该聚合结果；`StandardTestNext.Test` CLI/demo 也已新增 `anchor-cross-plan=` 摘要。这样每次跑 `Test -> SQLite -> App读取` / `stp.db` 快照验证时，不只看单个业务项缺什么字段，还能直接看到真实旧库口径下“哪些字段跨多个 Motor_Y 核心试验项反复成为首要补齐目标”，便于后续优先补 builder payload、算法 adapter 输入或 App 提示策略。
- 本轮把这份“跨试验项 decision-anchor primary-field 焦点”正式打通到 `App读取` 闭环：`MotorYMethodAdaptationPlanContract` 新增 `CrossPlanDecisionAnchorPrimaryFieldFocuses / CrossPlanDecisionAnchorPrimaryFieldSummary`，`TestRecordQueryGatewayAdapter` 与 `TestRecordViewMapper` 已完成映射，`builder -> query -> App contract` 不再只能看到单个业务项内的 primary-field 分布，也能直接看到当前记录内多试验项聚合后的跨 plan 焦点总览；同时补 `TestRecordQueryGatewayAdapterSmokeTests` 锁定 `NoLoad + LoadB` 组合下的 cross-plan 摘要与字段分布，并执行 `dotnet test StandardTestNext.sln -c Debug --no-restore`、`dotnet run --project StandardTestNext.Test --no-build` 验证通过，继续保持 `Test -> SQLite -> App读取` 闭环可验证。
- 本轮进一步把 cross-plan 焦点能力补完整到 App 适配计划 contract：除 `CrossPlanDecisionAnchorPrimaryFieldFocuses` 外，`MotorYPrimaryFieldFocusContract` / snapshot / adapter 现已统一承载 `WeightedCount / WeightedShare`，让 App 在读取多试验项焦点总览时不仅知道“多少个核心试验项都指向同一字段”，还能直接按 `SelectedCount` 权重看到该字段在真实主流方法样本中的加权占比；相关 smoke tests、`dotnet test StandardTestNext.sln -c Debug --no-restore` 与 `dotnet run --project StandardTestNext.Test --no-build` 已验证通过，继续保持 `Test -> SQLite -> App读取` 闭环可验证。
- 本轮继续把 `stp.db` 真实结构驱动的 cross-plan required-result 焦点校验补强到“显式锁定”级别：`StpDbMotorYMethodAdaptationPlanSmokeTests` 新增对 `CoefficientOfPfe / Pfw / Pcu2` 的跨试验项 `Count / Share / WeightedCount / WeightedShare / CanonicalCodes / Summary` 断言，不再只依赖与同源工厂回算结果做弱比较，从而把 `stp.db -> method adaptation plan -> App读取/CLI` 这条链路上的加权结果字段优先级口径真正锁死，避免后续回归时 weighted summary 漂移却未被 smoke test 捕获。
- 本轮继续把 `stp.db` 驱动的 Motor_Y cross-plan 焦点输出往“开发时肉眼可读”推进：`StandardTestNext.Test` 的 `FormatCrossPlanPrimaryFieldFocuses()` 现在会在 CLI/demo 输出里直接附带每个 focus 的 `Summary`，让 `stp.db -> method adaptation plan -> CLI` 验证时不必再回头对照详情对象才能理解该字段为何被提升为 cross-plan 优先补齐项；同时补 `TestBootstrapFormattingSmokeTests` 锁定格式，避免后续格式回退。
- 本轮又把 CLI/demo 中 `anchor-cross-plan` 与 `result-cross-plan` 的权重口径补齐到一致：`FormatMethodAdaptationPlanSnapshot()` 现会在跨试验项 decision-anchor primary-field 预览里同时输出 `WeightedCount / WeightedShare`，不再只有 required-result 侧带 weighted 信息；并新增 `TestBootstrapFormattingSmokeTests` 锁定 `anchor-cross-plan=...:weighted=...` 字符串，同时执行 `dotnet test StandardTestNext.sln -c Debug --no-restore` 与 `dotnet run --project StandardTestNext.Test --no-build -- --persistence sqlite --sqlite-db-path /root/.openclaw/workspace/next-gen/artifacts/test-persistence/cli-cross-plan-weighted.db` 验证通过，继续保持 `Test -> SQLite -> App读取` 闭环与 `stp.db` cross-plan 焦点可读性一致。
- 本轮继续把 Motor_Y 适配计划中的“按算法家族聚合的 primary-field 焦点”打通到 `stp.db` 快照层与 `builder -> query -> App读取` 闭环：`MotorYMethodAdaptationPlanSnapshot/Contract` 新增 `AlgorithmFamilyDecisionAnchorPrimaryFieldFocuses/Summary` 与 `AlgorithmFamilyRequiredResultPrimaryFieldFocuses/Summary`，`StpDbSnapshotQueryService`、`TestRecordViewMapper`、`TestRecordQueryGatewayAdapter` 已统一回投，并补 `StpDbMotorYMethodAdaptationPlanSmokeTests`、`TestRecordQueryGatewayAdapterSmokeTests` 锁定 `NoLoad + LoadB` 场景下的 family 级 weighted 聚合结果；同时执行 `dotnet test StandardTestNext.sln -c Debug --no-restore` 与 `dotnet run --project StandardTestNext.Test --no-build -- --persistence sqlite --sqlite-db-path /root/.openclaw/workspace/next-gen/artifacts/test-persistence/cron-motory-cross-plan.db` 验证通过，继续保持 `stp.db -> method adaptation plan -> App读取` 与 `Test -> SQLite -> App读取` 两条闭环都可直接消费算法家族维度的焦点摘要。

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

