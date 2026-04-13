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
- 本轮把同样的聚合校验补齐到旧方法变体维度（`baseline / delivery / legacy-alias ...`）：现已为 `VariantKindDecisionAnchorPrimaryFieldFocuses/Summary` 与 `VariantKindRequiredResultPrimaryFieldFocuses/Summary` 增补 `stp.db` 真实快照 smoke test 与 `builder -> query -> App读取` smoke test，显式锁定 NoLoad 基线变体与 Load_B delivery 变体在 decision-anchor / required-result 两侧的 weighted focus 与 summary 文案；并执行 `dotnet test StandardTestNext.sln -c Debug --no-restore` 与 `dotnet run --project StandardTestNext.Test --no-build -- --persistence sqlite --sqlite-db-path /root/.openclaw/workspace/next-gen/artifacts/test-persistence/cron-motory-variant-kind.db` 验证通过，继续保持 `旧 Method 变体 -> 适配计划 -> App读取` 闭环可验证。
- 本轮继续把 Motor_Y 焦点聚合往“可直接指导旧算法 adapter 开发”推进：`MotorYPrimaryFieldFocusSnapshot/Contract` 新增 `MethodValues / MethodKeys / ProfileKeys`，`MotorYPrimaryFieldFocusFactory` 会在 cross-plan / algorithm-family / variant-kind 三类 decision-anchor 与 required-result 聚合中显式保留这些字段对应的旧 Method 号与 profile；`TestRecordQueryGatewayAdapter` 与 `StandardTestNext.Test` CLI/demo 也已同步输出 `methods=/profiles=` 摘要。这样后续针对某个焦点字段（如 `Pfw / CoefficientOfPfe / GB / θw`）推进适配时，可直接知道它主要由哪些旧 `Method` 分支驱动，而不必再回头从 plan 列表二次反查。并已执行 `dotnet test StandardTestNext.sln -c Debug --no-restore`、`dotnet run --project StandardTestNext.Test --no-build -- --persistence sqlite --sqlite-db-path /root/.openclaw/workspace/next-gen/artifacts/test-persistence/cron-motory-method-focus.db` 验证通过，继续保持 `Test -> SQLite -> App读取` 闭环可验证。
- 本轮又把同一批焦点聚合继续前推到“旧方法名称可读”层：`StandardTestNext.Test` 的 CLI/demo 现已在 `anchor-cross-plan / anchor-family / anchor-variant` 预览里同时输出 `method-keys / legacy-methods / settings-methods`，不再只有 Method 数值与 profile；同时补了 `TestBootstrapFormattingSmokeTests` 锁定 `B法负载试验 / 空载试验` 等名称，以及 `TestRecordQueryGatewayAdapterSmokeTests` 锁定 `builder -> query -> App读取` 闭环里 `CrossPlanDecisionAnchorPrimaryFieldFocuses` 对应的 `MethodKeys / LegacyMethodNames / SettingsMethodNames`。这样后续看到某个焦点字段（如 `GB / RConverseType`）时，可以直接从 next-gen 查询/CLI 输出定位它对应旧 `Algorithm_Motor_Y.cs` 入口名与 `Settings_CYDJ.json` 方法名，而不必再做二次映射。
- 本轮继续把旧 `Algorithm_Motor_Y.cs` / 旧窗体依赖证据往“开发时肉眼可读”推进：`StandardTestNext.Test` 的 `FormatMethodAdaptationPlanSnapshot()` 现已在 CLI/demo 中新增 `source-evidence` 与 `form-evidence` 片段，直接输出算法源码分段（section/method/line-range/关键字段）以及旧 `FrmMotor_Y_*` 对上游试验项的读取证据；同时补 `TestBootstrapFormattingSmokeTests` 锁定 `NoLoad` 场景下 `rconverse-branch` 与 `FrmMotor_Y_NoLoad` 的预览字符串，并执行 `dotnet test StandardTestNext.sln -c Debug --no-restore` 与 `dotnet run --project StandardTestNext.Test --no-build -- --persistence sqlite --sqlite-db-path /root/.openclaw/workspace/next-gen/artifacts/test-persistence/cron-motory-legacy-evidence.db` 验证通过。这样后续推进 Motor_Y adapter 时，跑一次 `Test -> SQLite -> App读取` 闭环就能直接看到“旧算法哪段源码/哪处旧窗体依赖了哪些字段和上游试验项”，不必再回头翻 catalog 或旧代码。
- 本轮又把上述“焦点聚合里的旧算法/旧窗体证据”正式透传到 `App读取` 闭环：`MotorYPrimaryFieldFocusContract` 新增 `LegacyAlgorithmEntries / SourceSections / SourceRanges / FormNames / FormSourceRanges`，`TestRecordQueryGatewayAdapter` 已将 cross-plan / algorithm-family / variant-kind 下的 decision-anchor 与 required-result 两类 primary-field focus 全量回投到 App contract；并补 `TestRecordQueryGatewayAdapterSmokeTests` 锁定 `NoLoad` 的 `GetNoLoadData + rconverse-branch + FrmMotor_Y_NoLoad(L263)` 与 `LoadB` 的 `GetLoadBData + gb-ratios-branch + FrmMotor_Y_LoadB` 证据字段，同时执行 `dotnet test StandardTestNext.sln -c Debug --no-restore` 与 `dotnet run --project StandardTestNext.Test --no-build -- --persistence sqlite --sqlite-db-path /root/.openclaw/workspace/next-gen/artifacts/test-persistence/cron-motory-focus-evidence.db` 验证通过。这样后续 App/报表/adapter 在看到某个 cross-plan 焦点字段时，就能直接追溯它由哪段旧算法入口、哪处旧窗体依赖驱动，而不必再回退到 CLI 或 snapshot 层反查。
- 本轮继续把同一批 cross-plan / algorithm-family / variant-kind 焦点聚合往“上游依赖可读”推进：`MotorYPrimaryFieldFocusSnapshot/Contract` 新增 `UpstreamCanonicalCodes / UpstreamSummaryHints`，`MotorYPrimaryFieldFocusFactory` 会把各业务项适配计划中的上游试验依赖与 `UpstreamDependencySummary` 一并沉到 focus 聚合摘要；`TestRecordQueryGatewayAdapter`、`TestRecordViewMapper` 与 `StandardTestNext.Test` CLI/demo 现也会把这些字段透传/打印出来。这样后续看到像 `GB / Pfw / CoefficientOfPfe` 这类跨试验项焦点字段时，可直接知道它们主要关联了哪些上游试验项（如 `MotorY.NoLoad / MotorY.HeatRun / MotorY.DcResistance`），不用再回到单个 plan 明细反查。并已执行 `dotnet test StandardTestNext.sln -c Debug --no-restore --filter "FullyQualifiedName~TestRecordQueryGatewayAdapterSmokeTests|FullyQualifiedName~TestBootstrapFormattingSmokeTests|FullyQualifiedName~StpDbMotorYMethodAdaptationPlanSmokeTests"` 与 `dotnet run --project StandardTestNext.Test --no-build -- --persistence sqlite --sqlite-db-path /root/.openclaw/workspace/next-gen/artifacts/test-persistence/cron-motory-upstream-focus.db` 验证通过，继续保持 `Test -> SQLite -> App读取` 闭环可验证。
- 本轮把 Motor_Y primary-field focus 再往“真实旧方法分支决策可读”推进了一格：`MotorYPrimaryFieldFocusSnapshot/Contract` 新增 `BaselineCount/BaselineShare/DominantCount/DominantShare/SelectedCount/SelectedShare`，`MotorYPrimaryFieldFocusFactory` 在 cross-plan / algorithm-family / variant-kind 两类焦点聚合 summary 中直接带出该字段由 baseline / dominant / selected 方法分支驱动的占比，`TestRecordViewMapper` 与 `TestRecordQueryGatewayAdapter` 也已完成闭环透传。这样后续看到诸如 `GB / Pfw / CoefficientOfPfe` 这类跨试验项焦点时，不只知道它来自哪些 Method/profile，还能直接判断它更偏向旧基线分支还是当前主流分支，为 `Algorithm_Motor_Y.cs` 适配优先级提供更直接锚点。已执行 `dotnet test StandardTestNext.sln -c Debug --no-restore` 与 `dotnet run --project StandardTestNext.Test --no-build -- --persistence sqlite --sqlite-db-path /root/.openclaw/workspace/next-gen/artifacts/test-persistence/cron-motory-focus-route-share.db` 验证通过，继续保持 `Test -> SQLite -> App读取` 闭环可验证。
- 本轮继续把这套焦点摘要往“旧 Method 数值可直接肉眼对照”推进：`MotorYPrimaryFieldFocusFactory` 现在在 `cross-plan / algorithm-family / variant-kind` 三类 primary-field summary 中，除 `method-keys / legacy-methods / settings-methods` 外，也会直接输出聚合后的 `methods=数值列表`，把 `stp.db` 真实 Method 分布与 `Algorithm_Motor_Y.cs` 分支号的对照进一步前推到 summary 文本层，减少排查时再从 `MethodKey` 反拆数字的步骤。已执行 `dotnet test StandardTestNext.sln -c Debug --no-restore` 与 `dotnet run --project StandardTestNext.Test --no-build -- --persistence sqlite --sqlite-db-path /root/.openclaw/workspace/next-gen/artifacts/test-persistence/cron-motory-method-values.db` 验证通过，并已提交 `4597459 补充Motor_Y焦点摘要中的方法号信息`。
- 本轮把 Motor_Y primary-field focus 中仍缺的一层旧路由可读性补齐了：`MotorYPrimaryFieldFocusSnapshot/Contract` 现已正式承载 `LegacyEnumNames / LegacyFormNames`，`MotorYPrimaryFieldFocusFactory` 在 cross-plan / algorithm-family / variant-kind 聚合时会从 `Selected/Dominant/Baseline route` 自动汇总旧枚举名与旧窗体名，`TestRecordQueryGatewayAdapter` 也已把这些字段打通到 App 读取 contract；同时 `StandardTestNext.Test` 的 cross-plan CLI 预览新增 `legacy-enums / legacy-forms` 片段，并补 `TestBootstrapFormattingSmokeTests` 锁定。已执行 `dotnet build StandardTestNext.sln -c Debug` 与 `dotnet run --project StandardTestNext.Test --no-build -- --persistence sqlite --sqlite-db-path /root/.openclaw/workspace/next-gen/artifacts/test-persistence/cron-motory-legacy-route-names.db` 验证通过，并已提交 `997175b 补齐Motor_Y焦点中的旧枚举与窗体名透传`。
- 本轮修正了 `stp.db` 真实结构驱动的 Motor_Y 方法推荐投影口径：`StpDbSnapshotQueryService.LoadMotorYMethodRecommendations()` 之前把 `DominantProfileKey / VariantKind / AlgorithmFamily / LegacyEnum/Form/AlgorithmEntry` 等 dominant 字段错误地取自 `RecommendedRoute`，在 baseline 与 dominant 不一致（如 A 法负载试验 4 vs 60）时会混淆“真实主流方法”与“最终推荐方法”；现已统一改为从 `DominantRoute` 投影，保持 `dominant` 语义与 `MotorYMethodDecisionSnapshot` / `stp.db` 真实分布一致。已执行 `dotnet build StandardTestNext.sln -c Debug --no-restore` 与 `dotnet run --project StandardTestNext.Test --no-build -- --persistence sqlite --sqlite-db-path /root/.openclaw/workspace/next-gen/artifacts/test-persistence/cron-motory-dominant-route-fix.db` 验证通过，继续保持 `Test -> SQLite -> App读取` 闭环可验证。
- **本轮进展（2026-04-01 17:01 Asia/Shanghai）**：已将本项目长期执行基线重新读取并维护到位；确认当前仓库 `next-gen` HEAD 为 `0e02406`（`补齐Motor_Y方法推荐的旧业务项名快照`），且工作区已有大批未提交改动，主线仍在围绕 Motor_Y 方法推荐 / 适配计划 / App 读取闭环推进。
- **当前状态**：`EXECUTION_CONTEXT.md` 已同步到最新已知推进面；当前定时心跳任务仍按每 30 分钟运行，具备“有实质进展才汇报”的行为约束。
- **下一步**：1) 核查并补齐“每日 9 点进度报告”任务的 docx 生成/投递链路；当前已确认任务存在，且 2026-04-01 已产出 `StandardTestNext_代码进度汇总_2026-04-01.md` 与 `.body.md`，但尚未看到同日 docx；2) 继续基于当前未提交代码推进 Motor_Y 推荐路由、视图映射与 App/Test 闭环的收口验证；3) 后续每轮有实质进展时持续回写本文件。
- **卡点 / 风险**：独立 9 点日报 cron 已存在（`72d1d5c6-7af7-4029-83ea-31494c02c0a1`），但当前可见产物仅确认到 2026-04-01 的 `md/body.md`，尚未看到同日 docx；因此现阶段的真实缺口已从“未建任务”切换为“docx 生成或后续投递链路仍待核查/修复”。
- **新增运维问题（2026-04-01 17:12 Asia/Shanghai）**：巡检发现原 `Heartbeat progress check` 定时任务被切回 `sessionTarget=main` 且未显式配置 `delivery.channel`，最近一次执行已报错：`Channel is required (no configured channels detected)`。该问题会导致后续 heartbeat 失效，需要恢复为带显式投递信息的配置或改回隔离会话执行。
- **修复动作（2026-04-01 18:35 Asia/Shanghai）**：已将不稳定的 heartbeat cron 重建：删除旧任务 `1e921fdb-1237-4d50-bd73-a93b4955a414`，改用新的 `systemEvent -> main session` 方案新建 `Heartbeat progress check`，新 jobId 为 `66708173-0486-487a-968d-9bb51392b199`。该方案不再依赖 isolated agentTurn + announce delivery 组合，优先把心跳触发恢复为更稳的主会话系统事件注入；同时在提示文本中显式要求维护执行基线时优先使用稳定写法，避免脆弱的精确 edit 匹配失败。
- **状态续跟（2026-04-01 21:05 Asia/Shanghai）**：新 heartbeat 已连续多个 30 分钟周期正常触发，未再出现 `Channel is required` 或 `EXECUTION_CONTEXT.md edit failed` 类报错，说明“systemEvent -> main session” 这一修复方向当前有效，heartbeat 触发链路已从失败态恢复到可用态。
- **状态校准（2026-04-02 06:35 Asia/Shanghai）**：巡检 `output/` 发现独立日报任务并非完全无产物：目录中已存在 `StandardTestNext_代码进度汇总_2026-04-01.md` 与 `StandardTestNext_代码进度汇总_2026-04-01.body.md`，说明 2026-04-01 的日报链路至少跑通了 markdown 产出；但同目录下仍未见对应日期的 docx，故当前应将问题收敛为“日报任务已落地且有部分产物，docx 生成/投递链路仍待核查”。
- **状态续跟（2026-04-02 09:05 Asia/Shanghai）**：今日 9 点窗口后再次巡检 `output/`，已看到新产物 `StandardTestNext_代码进度汇总_2026-04-02.md`（09:00）与 `StandardTestNext_代码进度汇总_2026-04-02.body.md`（09:01）。这说明独立日报 cron 已连续两天成功生成 markdown 类日报；但截至本次检查仍未见 `2026-04-02` 对应 docx，故当前最准确结论是：**定时触发与 markdown 产出链路已验证成功，docx 生成/后续投递链路仍未闭环**。
- **状态续跟（2026-04-02 09:00 Asia/Shanghai）**：今日 9 点日报巡检已再次读取并维护执行基线；确认 `next-gen` 当前 HEAD 仍为 `0e02406`，最近可见源码推进仍围绕 `StpDbSnapshotQueryService`、`MotorYMethodRecommendationSnapshot` 及对应 smoke tests，工作区仍存在大批未提交改动，说明主线仍在推进 Motor_Y 方法推荐 / 适配计划 / App 读取闭环，但截至本轮未形成新的已提交里程碑。已在 `output/` 生成今日 markdown 报告 `StandardTestNext_代码进度汇总_2026-04-02.md` 与 `.body.md`，但仍未见任何同日 docx，故日报链路当前结论维持为“markdown 已落地，docx 生成/投递仍未闭环验证成功”。
- **运维修正（2026-04-02 09:21 Asia/Shanghai）**：用户在 OpenClaw 仪表盘中无法直观看到 heartbeat 的执行记录。排查确认：虽然新 heartbeat job 为 `66708173-0486-487a-968d-9bb51392b199`，但其 `sessionKey` 仍脏绑定到旧链路 `agent:main:cron:1e921fdb-1237-4d50-bd73-a93b4955a414:run:0d7b9de3-906a-4d7a-8842-a5e9703f8e11`，导致 job id、sessionKey、会话可见性错位，用户难以自证执行情况。已决定按用户要求采用“方案1”：删除当前 heartbeat job 并重建一个干净的 `systemEvent -> main session` heartbeat，使新 job id 与后续运行上下文重新对齐，优先恢复可核验性。
- **运维验证（2026-04-02 09:23 Asia/Shanghai）**：实测发现单纯重建 `systemEvent -> main session` heartbeat 不能切断旧链路；新建 job 虽获得新 jobId，但平台仍自动复用旧 `sessionKey`。因此已按用户要求切换到“方案2”：放弃 main/systemEvent 绑定，改为重建一个真正独立的 `isolated agentTurn` heartbeat，以优先恢复 run/session 可核验性。
- **运维决策（2026-04-02 11:43 Asia/Shanghai）**：用户明确要求“重做”。因此不再继续围绕旧 heartbeat 名称/链路修补，而是删除当前 heartbeat，改为新建一条**全新命名**的 heartbeat v2 定时任务，并立即核验其 job id / sessionKey / run history 是否终于与旧链路解耦；若仍复用旧键空间，则可进一步确认问题属于平台级会话绑定行为，而非任务名或配置残留。
- **本轮进展（2026-04-02 09:54 Asia/Shanghai）**：再次按 heartbeat 要求重读并稳定写回执行基线，重新核查到 `next-gen` 当前 HEAD 仍为 `0e02406`，工作区仍存在大批未提交改动，说明 StandardTestNext 主线仍在围绕 Motor_Y 方法推荐 / 适配计划 / App 读取闭环推进，但自 09:00 后尚未形成新的已提交里程碑；同时确认今日 9 点日报链路目前仍只产出 `StandardTestNext_代码进度汇总_2026-04-02.md` 与 `.body.md`，未见同日 docx。另一次关键运维发现是：当前 heartbeat 虽已切到 `isolated agentTurn`，新 jobId 为 `09d56c84-2d11-4a3d-be05-694ae8438393`，但 `sessionKey` 仍继续沿用旧链路 `agent:main:cron:1e921fdb-1237-4d50-bd73-a93b4955a414`，说明“切隔离会话即可恢复 run/session 可核验性”的假设尚未真正验证成立，heartbeat 可见性问题仍未根治。
- **当前状态（2026-04-02 09:54 Asia/Shanghai）**：执行基线已同步到最新巡检结论；日报任务 `72d1d5c6-7af7-4029-83ea-31494c02c0a1` 今日 9 点再次生成 markdown 产物，但最近一次 run 仍以 `Channel is required (no configured channels detected)` 失败收尾，说明其 announce 投递/渠道绑定仍是明确故障点；heartbeat 任务虽在跑，但 job id 与 sessionKey 仍错位，用户侧可核验性仍不足。
- **下一步（更新于 2026-04-02 09:54 Asia/Shanghai）**：1) 优先修复每日 9 点进度报告任务的 delivery.channel 配置或改用可稳定投递的链路，并顺手补查 docx 生成缺口；2) 继续追查 heartbeat job 的 `sessionKey` 复用根因，确认是否需要彻底换任务名/新会话绑定策略，避免 run 记录继续挂在旧链路；3) 在上述运维链路稳定后，再继续收口 Motor_Y 方法推荐 / 适配计划 / App/Test 闭环源码验证。
- **卡点 / 风险（更新于 2026-04-02 09:54 Asia/Shanghai）**：当前最明确的根因型风险已不是“有没有任务”，而是两条自动化链路都存在可验证性/投递层缺陷：日报 cron 连续两天只有 markdown、且最近 run 明确报 `Channel is required`；heartbeat cron 虽重建多次但 `sessionKey` 仍复用旧链路，导致用户难以在仪表盘确认新 job 的独立运行记录。若不先修这两个根因，后续长期托管的低打扰可恢复性仍会继续失真。
- **本轮进展（2026-04-02 10:54 Asia/Shanghai）**：再次巡检后确认出现了一条新的可验证变化：heartbeat 任务 `09d56c84-2d11-4a3d-be05-694ae8438393` 的最近一次运行状态已从此前的 `error` 变为 `ok`，`lastDurationMs=27086`，`consecutiveErrors` 已清零；但其 `lastDeliveryStatus=not-delivered`，且 `sessionKey` 仍旧复用 `agent:main:cron:1e921fdb-1237-4d50-bd73-a93b4955a414`。这说明当前 heartbeat 至少执行链路已暂时恢复成功返回，不再像上一轮那样直接报错，但“执行成功 ≠ 可投递/可核验问题已解决”，因为投递仍未送达、会话绑定错位也还在。与此同时，`next-gen` 当前 HEAD 依然是 `0e02406`，工作区状态与 09:54 相比无新增已提交里程碑；今日日报目录中仍只有 `StandardTestNext_代码进度汇总_2026-04-02.md` 与 `.body.md`，没有新 docx 产物。
- **当前状态（更新于 2026-04-02 10:54 Asia/Shanghai）**：执行基线已按本轮巡检再次稳定写回；heartbeat 当前已从“运行报错”改善为“运行成功但未投递”，说明问题焦点已进一步收缩到 delivery/可见性层，而非 agentTurn 本体直接失败。日报任务的核心故障判断不变：9 点 cron 最近一次仍报 `Channel is required`，docx 仍未落地。
- **下一步（更新于 2026-04-02 10:54 Asia/Shanghai）**：1) 优先追查 heartbeat `announce` 为何 `not-delivered`，确认是否必须显式补 channel/to 或彻底换成用户可见的绑定方式；2) 并行修复 9 点日报 cron 的 delivery/channel 故障，再补查 docx 生成缺口；3) 仅在两条自动化链路稳定可核验后，再继续收口 Motor_Y 主线源码验证。
- **卡点 / 风险（更新于 2026-04-02 10:54 Asia/Shanghai）**：heartbeat 现在虽然不再直接 error，但“成功执行却未投递 + sessionKey 仍旧复用旧链路”的组合，意味着用户侧仍可能看不到可验证记录；而日报 cron 仍维持显式 delivery 错误且无 docx。换言之，当前风险已从“链路会不会跑起来”演变为“链路即使跑起来，用户也未必能看见/收到”，这仍然违背长期托管所需的低打扰但可核验原则。
- **本轮进展（2026-04-02 11:24 Asia/Shanghai）**：heartbeat 状态又出现一次反向波动：`09d56c84-2d11-4a3d-be05-694ae8438393` 最近一次运行已从上一轮的 `ok/not-delivered` 再次变回 `error`，`lastRunAt=11:14`、`lastDurationMs=325663`、`consecutiveErrors=1`，且 `lastDeliveryStatus` 重新回到 `unknown`；与此同时 `sessionKey` 仍持续复用旧链路 `agent:main:cron:1e921fdb-1237-4d50-bd73-a93b4955a414`。这说明 heartbeat 当前并非稳定停留在“执行成功但未投递”阶段，而是仍在 `ok` / `error` 之间摆动，根因尚未收敛。另一方面，`next-gen` HEAD 依旧为 `0e02406`，工作区状态与 10:54 相比无新增已提交里程碑；今日日报目录仍只有 `StandardTestNext_代码进度汇总_2026-04-02.md` 与 `.body.md`，docx 缺口依旧未补。
- **当前状态（更新于 2026-04-02 11:24 Asia/Shanghai）**：执行基线已再次稳定写回；heartbeat 的问题判断需要回退为“链路仍不稳定且可见性仍异常”，不能再乐观描述为仅剩 delivery 问题。日报 cron 的核心故障判断仍保持不变：配置里虽然写了 `channel=openclaw-weixin,to=wxid_lk1845062639`，但最近一次 run 依旧报 `Channel is required`，说明实际投递解析/绑定链路未按配置生效。
- **下一步（更新于 2026-04-02 11:24 Asia/Shanghai）**：1) 优先把 heartbeat 问题重新按“执行稳定性 + sessionKey 绑定 + delivery 可见性”三层一起排查，不再假设只需修投递层；2) 紧接着核查为何日报 cron 在显式配置 channel/to 后仍报 `Channel is required`，确认是 channel 名无效、announce 机制异常，还是 isolated session 下上下文丢失；3) 上述两条自动化链路未稳定前，不应把 StandardTestNext 的长期托管状态视为已恢复健康。
- **卡点 / 风险（更新于 2026-04-02 11:24 Asia/Shanghai）**：heartbeat 的 `ok -> error` 回摆说明当前修复策略还没有形成稳定闭环；而日报 cron 也持续表现出“配置看似存在但运行时不认”的症状。两者共同指向一个更深的运维风险：当前问题可能不只是任务配置文本，而是 cron runtime / delivery / session 绑定层存在平台级行为偏差。如果不先把这个层面的可验证性恢复，后续所有“有在跑”的长期任务都可能继续处于看似存在、实则难以自证的失真状态。


- **状态续跟（2026-04-02 11:59 Asia/Shanghai）**：本轮再次执行 StandardTestNext 日报巡检，确认 `next-gen` 当前 HEAD 仍为 `0e02406`，最近可见源码推进点仍集中在 `StpDbSnapshotQueryService`、`MotorYMethodRecommendationSnapshot` 与对应 smoke tests，工作区仍为大范围未提交修改态，因此截至当前仍无新的已提交实质里程碑。与此同时，已使用本地 `python-docx` 成功补生成今日日报产物 `/root/.openclaw/workspace/output/StandardTestNext_代码进度汇总_2026-04-02.docx`，说明当前至少可以稳定落地 docx 文件；但这仍只能证明“本地补生成链路可用”，尚不能等同于 cron 原生 docx 生成/投递链路已经彻底闭环，后续仍需单独核查日报任务自身的 docx 生成机制。

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

---

## 9. 最近执行更新（2026-04-09）

- **本轮进展（2026-04-09 14:36 Asia/Shanghai）**：已按用户最新协作协议先重新读取并恢复现场：复核 `EXECUTION_CONTEXT.md`、新增恢复对照文档 `docs/PROJECT_RECOVERY_2026-04-09.md`，并确认当前 `next-gen` 仓库 HEAD 已推进到 `a663e15`（`chore: 提交近期 StandardTestNext 本地改动`），相较此前长期基线记录的 `0e02406` 已形成新的已提交里程碑。
- **本轮验证（2026-04-09 14:18 Asia/Shanghai）**：执行 `dotnet test StandardTestNext.sln -c Debug --no-restore` 已通过，说明当前主干在该提交点仍保持可构建、可回归验证状态；工作区当前仅见未跟踪项 `docs/PROJECT_RECOVERY_2026-04-09.md` 与 `artifacts/test-persistence/`。
- **本轮进展续推（2026-04-09 14:44 Asia/Shanghai）**：已开始把恢复文档里定义的 NoLoad 切口落到真实代码：重读旧 `TestData_Motor_Y_NoLoad` 与 `Algorithm_Motor_Y.NoLoad(...)` 后，将 `MotorYTrialRecordBuilder.BuildNoLoadItem()` 从“顶层结果直接取最后一个采样点硬填”改为更贴近旧算法的聚合口径——按最接近额定电压的采样点回填 `I0/P0/Pcu/R0/θ0`，按 `<0.51 pu` 低压窗口聚合 `Pcon` 生成 `Pfw`，并据此回推额定点 `Pfe`；同时新增 smoke test 锁定该聚合行为，防止后续又退回到 last-point 假值口径。
- **本轮进展再续推（2026-04-09 14:48 Asia/Shanghai）**：继续把 NoLoad 从“builder 顶层聚合”推进到“可执行 adapter 入口”：在 `MotorYNoLoadLegacyShape.cs` 内新增 `MotorYNoLoadExecutionAdapter` / `MotorYNoLoadExecutionSnapshot`，让 next-gen 可直接从 NoLoad payload 派生出一版明确的执行结果快照（rated-point、Pfw/Pfe estimate、pfw-fit readiness、缺失输入）；同时把该 execution preview 接进 `StandardTestNext.Test` CLI 输出，并新增 smoke test 锁定 `rated-point-only` 阶段行为，作为后续真正迁入旧 NoLoad 算法的过渡入口。
- **本轮进展继续前推（2026-04-09 14:52 Asia/Shanghai）**：已把旧 `Algorithm_Motor_Y.NoLoad(...)` 中最明确的一条执行分支正式收进 next-gen execution adapter：NoLoad 现在不仅有 rated-point/pfw window 入口，还会依据 payload 中的 `RConverseType` 显式区分 `R0->θ0` 与 `θ0->R0` 分支，并计算 `ComputedTheta0 / ComputedR0`，让 adapter 开始具备旧算法 `RConverseType` 分支语义，而不再只是“看一眼 payload 顶层值”。同时同步更新 CLI preview 与 smoke test，锁定当前 `rconverse+rated-point-only` 阶段行为。
- **本轮修复（2026-04-09 15:01 Asia/Shanghai）**：按照“先验证已完成项是否真的完成”的要求，先主动执行 `dotnet build StandardTestNext.sln -c Debug --no-restore` 做全量编译复核，发现当前仓库并非完全健康：`MotorYLegacyAlgorithmDependencyCatalog.Get(...)` 与 `DistinctNonEmpty(...)` 实际缺失，导致 solution build 直接失败。已先补回这两个缺口（为 dependency catalog 恢复兼容 `Get` 入口，并在 `StpDbSnapshotQueryService` 补回 `DistinctNonEmpty`），随后重新执行 build 与 test，现已恢复为 `build succeeded` + `dotnet test` 通过。
- **本轮进展再推进（2026-04-09 15:11 Asia/Shanghai）**：已继续把 NoLoad 从“分支骨架”推进到“结果字段本体”一小步：`MotorYTrialRecordBuilder.BuildNoLoadItem()` 不再给 `CoefficientOfPfe` 写固定假数组，而是基于当前 `DataList` 的 `U0/Un -> Pfe` 真值点，新增最小多项式拟合（法方程 + 高斯消元）生成 `CoefficientOfPfe`，并用拟合结果回算 1.0pu 的 `Pfe`。同时补强 smoke test，明确锁定 `CoefficientOfPfe` 已由真实数据生成、长度与当前样本规模匹配，避免又退回固定占位系数。
- **本轮进展继续前推（2026-04-09 15:20 Asia/Shanghai）**：已把 NoLoad 的 `Pfw` 计算从“低压点平均值占位”推进到更贴近旧 `Algorithm_Motor_Y.NoLoad(...)` 的口径：现在对 `<0.51 pu` 窗口样本使用 `U0DivideUnSquare -> Pcon` 最小线性回归，并取截距作为 `Pfw`，不再直接取均值；同时同步调整 smoke test，使其按同样的线性回归截距规则验证 `Pfw` / `Pfe`。
- **本轮进展再收口（2026-04-09 15:31 Asia/Shanghai）**：已把 builder 与 execution adapter 里重复的 NoLoad 计算正式收敛到统一 helper：新增 `MotorYNoLoadComputation.cs`，把 rated-point 选择、`Pfw` 低压段线性拟合、`CoefficientOfPfe` 多项式拟合与 `Pfe` 求值抽到共用计算层；`MotorYTrialRecordBuilder` 与 `MotorYNoLoadExecutionAdapter` / smoke test 现都复用同一套计算结果，避免后续继续在两处各自维护分叉口径。
- **本轮验证补充（2026-04-09 15:32 Asia/Shanghai）**：抽 helper 过程中先暴露出一次真实编译回归（新 helper 英文字段名与原 builder 里的 `θ0/ΔI0` 旧命名未完全对齐），已当场修平；随后重新执行 `dotnet build StandardTestNext.sln -c Debug --no-restore` 与 `dotnet test StandardTestNext.sln -c Debug --no-restore`，均通过。当前 solution 仍仅剩两个既有 warning，无新增 build/test 阻塞。
- **本轮进展继续前推（2026-04-09 16:24 Asia/Shanghai）**：已补上 NoLoad 第二条关键执行分支的最小 smoke 覆盖，不再只锁 `RConverseType=0 / θ0->R0`。`TestRecordLegacyPayloadReaderSmokeTests` 现在新增 `RConverseType=1 / R0->θ0` 场景：直接构造 legacy-shape NoLoad payload，显式给入 `R0` 初值、三组 `DataList` 点，并同时校验 `MotorYNoLoadComputation` 与 `MotorYNoLoadExecutionAdapter` 对 `ComputedTheta0/ComputedR0` 的结果一致，且 rated-point 仍稳定落在 `U0=380`，从而把第二分支的 execution 入口也纳入可回归验证范围。
- **本轮验证补充（2026-04-09 16:24 Asia/Shanghai）**：新增 `R0->θ0` 分支 smoke 后，重新执行 `dotnet build StandardTestNext.sln -c Debug --no-restore` 与 `dotnet test StandardTestNext.sln -c Debug --no-restore`，均通过；当前 solution 仍只有既有两个 warning，无新增阻塞。
- **本轮进展继续前推（2026-04-09 17:33 Asia/Shanghai）**：已把 NoLoad 分支输入从“分析判断”推进到“builder 真正可接收的最短主链”。这轮没有去污染通用 `MotorRatedParamsContract`，而是沿 `query seed -> aggregate builder -> trial builder` 做了局部显式建模：`TestRecordQuerySeedContract` 新增 `NoLoadRConverseType`（默认由 `TestRecordQuerySeedFactory.CreateDefault()` 赋值为 `0`）；`InProcAppQueryGatewaySeedFactory.Seed()` 现在会把该字段传给 `TestRecordAggregateBuilder.BuildDemoRecord(...)`；`BuildDemoRecord(...)` 再把它透传给 `MotorYTrialRecordBuilder.BuildTrialItems(...)`；`BuildNoLoadItem(...)` 现已使用上游传入的 `rConverseType`，不再只依赖内部固定常量。这样当前 seeded demo / app query / aggregate 自动产物链路里，NoLoad 的 `RConverseType` 已首次成为一个真正来自上游 seed 的 builder 输入，而不是仅存在于 command 元数据或 builder 内部默认值。
- **本轮验证补充（2026-04-09 17:33 Asia/Shanghai）**：完成上述最小透传改造后，已执行 `dotnet build StandardTestNext.sln -c Debug --no-restore`，通过；当前 solution 仍只有既有两个 warning，无新增阻塞。
- **当前状态（2026-04-09 17:33 Asia/Shanghai）**：关于 `RConverseType` 的链路状态已从“主链签名太薄、builder 无法接收”前推到“seeded 主链已具备显式输入位，builder 已开始消费该输入”。目前默认值仍是 `0`，所以行为结果不会立刻变化，但语义上已经从“写死默认分支”升级为“可由上游 seed 控制的默认分支输入”。
- **下一步（更新于 2026-04-09 17:33 Asia/Shanghai）**：1) 立刻补一条 builder 自动产物级 smoke：传入 `noLoadRConverseType = 1` 后，验证 NoLoad payload 顶层 `RConverseType`、`θ0/R0` 计算分支与 execution adapter 观察一致，避免只改签名不锁行为；2) 评估是否要把同一输入继续扩展到非 seeded demo 的运行入口（例如 `TestBootstrap` 或后续真实 session seed），但前提是先用 smoke 把现有最短主链锁稳；3) 如果 smoke 通过，再考虑是否需要把先前 command 层的 `Parameters["RConverseType"]` 接到同一 seed/options 模型，避免双入口分叉。
- **卡点 / 风险（更新于 2026-04-09 17:33 Asia/Shanghai）**：虽然 builder 已能接收上游 `rConverseType`，但本轮还没有新增 smoke 证明 `rConverseType = 1` 时自动产物确实按 `R0->θ0` 分支生成；当前只是“输入位打通”，还不是“第二分支行为已锁定”。
- **状态续跟（2026-04-13 16:28 Asia/Shanghai）**：已按用户要求重新完整读取旧项目入口（`CYDJ_20220921`、`StandardTestApp/ClassLibrary`、`ClassLibary/StandardTest*`）、`stp.db` 与《StandardTestNext_系统改造方案_v1.0.md》，确认当前推进前置上下文仍完整有效；并对“已完成项是否真的完成”做了进一步复核。
- **验证收敛（2026-04-13 16:21 Asia/Shanghai）**：已完成两处遗留 nullable warning 修补：`MotorYDecisionAnchorResolutionFactory.cs` 的 `SuggestedNextStepPriority` 排序告警（CS8604）已消除，`StpDbSnapshotQueryService.cs` 对 `selectedRoute?.CanonicalCode` 增加最小回退保护后 `CS8602` 已消除。随后执行 `dotnet build /root/.openclaw/workspace/next-gen/StandardTestNext.Test/StandardTestNext.Test.csproj && dotnet run --project /root/.openclaw/workspace/next-gen/StandardTestNext.Test -- --run-smoke-tests`，结果为 **0 Warning / 0 Error / Smoke tests passed**。当前状态已从“smoke 绿但仍有 warning”收敛为“build 干净 + smoke 全绿”。
- **当前推进判断（2026-04-13 16:28 Asia/Shanghai）**：Motor_Y 这一轮优先事项已不再是继续追红灯，而是把已收敛的 LoadB legacy root shape、NoLoad legacy-shape/computation 前置条件、adaptation-plan/evidence 聚合修正与 warning 清零结果正式固化到恢复文档与执行基线，随后再处理 dirty working tree 分离与下一阶段真实算法迁移。
- **工作树再核对（2026-04-13 16:48 Asia/Shanghai）**：已重新执行 `git status --short` 复核未提交项，确认当前除了大量已修改的 Motor_Y 主线代码/测试文件外，仍有 3 类需要明确区分的未跟踪/非主线项：1) `StandardTestNext.Test/Application/Services/MotorYNoLoadComputation.cs` 属于本轮 NoLoad 计算统一 helper 主线代码，后续提交时不应遗漏；2) `docs/PROJECT_RECOVERY_2026-04-09.md` 属于本轮恢复与工作树分层基线文档，也应纳入正式版本控制；3) `artifacts/test-persistence/` 属于测试产物目录，更适合作为运行产物处理，不应混入主线语义提交。当前对 dirty working tree 的判断已从“粗略知道很脏”推进到“已能明确区分主线代码、恢复文档、测试产物与历史 tmp/log 删除项”。
- **全量暂存风险验真（2026-04-13 18:48 Asia/Shanghai）**：已做 `git add -n .` 全量 dry-run，现场确认如果直接使用 `git add .`，当前会一次性卷入四类不应混装的内容：1) B 组文档基线（`EXECUTION_CONTEXT.md`、`docs/PROJECT_RECOVERY_2026-04-09.md`）；2) A 组 Motor_Y 主线代码/测试；3) C 组历史噪音删除项（`CHANGELOG_2026-03-19.md`、`WORKLOG_2026-03-19.md`、多个 `tmp_app_*` / `tmp_test_*`）；4) 不应进入主线语义提交的测试产物 `artifacts/test-persistence/standardtest-next.db`。因此“不要直接 `git add .`，而应按 B → A → C 显式分组 staging，并额外检查 artifact 排除”现在已从结构性建议升级为经过 dry-run 证实的执行规则。
