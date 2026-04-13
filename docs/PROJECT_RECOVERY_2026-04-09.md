# StandardTestNext 旧/新项目恢复对照文档（2026-04-09）

## 1. 目的

按用户要求，先**重新读取整套旧项目与当前 next-gen 代码，并整理成文档**，再开始新的编码推进。

本文件是这次恢复现场的正式落地物，不再只靠口头“我已经知道了”。

---

## 2. 本次恢复范围

### 2.1 旧项目主参考源

1. **旧算法权威源**
   - `ClassLibary/StandardTest.Library/Algorithm/Motor/Algorithm_Motor_Y.cs`
   - 作用：Motor_Y 国标算法入口与核心计算逻辑权威来源。

2. **旧试验 DTO / 数据形状权威源**
   - `ClassLibary/StandardTest.Library/TestData/Motor/Y/*`
   - 作用：旧 `TestData_Motor_Y_*` 对象命名、字段形状、DataList/RawDataList/Data1List/Data2List 等结构口径来源。

3. **旧配置 / 方法命名权威源**
   - `ClassLibary/ConfigBack/Settings_CYDJ.json`
   - 作用：旧方法名、附件导出命名、中文业务项名称、布局元数据来源。

4. **旧现场 App / 流程权威源**
   - `app-repos/StandardTestApp/CYDJ_20220921`
   - `app-repos/StandardTestApp/SINEN_TEST_11KW`
   - 作用：现场流程、MonitorViewModel 状态、FrmControl 控件与自动化流程线索来源。

5. **旧真实数据库权威源**
   - `stp.db`
   - 作用：真实表结构、真实 Method 分布、真实 RatedParams / TestRecord / TestRecordItems / Attachments 数据口径来源。

### 2.2 next-gen 当前实现源

1. `next-gen/StandardTestNext.Contracts`
   - 共享 contracts、查询契约、Motor_Y 适配计划 contract。

2. `next-gen/StandardTestNext.Test`
   - 试验侧主干、builder、query、snapshot、smoke tests、SQLite 回读闭环。

3. `next-gen/StandardTestNext.App`
   - App 侧主干、默认 query gateway、detail/list 消费路径。

4. `next-gen/docs/*`
   - 历史迁移计划与垂直切片说明。

5. `next-gen/EXECUTION_CONTEXT.md`
   - 当前项目长期执行基线。

---

## 3. 旧项目权威来源分层

### 3.1 算法层：`Algorithm_Motor_Y.cs`

旧项目里，Motor_Y 真正不可替代的核心不是 UI，也不是 ViewModel，而是：

- `Algorithm_Motor_Y.cs`

它定义了：
- `NoLoad(...)`
- `Lock_Rotor(...)`
- `Thermal(...)`
- `Load_A(...)`
- `Load_B(...)`
- 以及相关 `Torque(...)`

这层是 next-gen 做算法迁移时的**第一权威源**。

### 3.2 DTO / payload 结构层：`TestData/Motor/Y/*`

旧项目真正决定 payload 形状的，不是零散 JSON，而是这些 DTO：

- `TestData_Motor_Y_Direct_Current_Resistance`
- `TestData_Motor_Y_NoLoad`
- `TestData_Motor_Y_Lock_Rotor`
- `TestData_Motor_Y_Thermal`
- `TestData_Motor_Y_Load_A`
- `TestData_Motor_Y_Load_B`
- 以及其他扩展变体

这层决定：
- 字段名
- 列表结构名（`DataList` / `Data1List` / `Data2List` / `RawDataList` / `ResultDataList`）
- 结果字段与中间字段语义

因此 next-gen 不该继续“猜字段”，而应直接对齐这些旧对象形状。

### 3.3 配置 / 命名层：`Settings_CYDJ.json`

这层给出：
- 旧方法名（如 `Motor_Y_NoLoad` / `Motor_Y_Thermal` / `Motor_Y_Lock_Rotor` / `Motor_Y_Load_A` / `Motor_Y_Load_B`）
- 中文业务项命名
- 附件导出名称与布局元数据

因此它是：
- **方法命名权威源**
- **附件/报表命名权威源**

### 3.4 现场流程 / UI 状态层：`CYDJ_20220921` / `SINEN_TEST_11KW`

这层不应拿来当算法权威，
但它决定：
- 现场流程怎么跑
- 控制页有哪些步骤/状态
- 1MW / 45kW 等自动化空载控制流程怎么呈现
- MonitorViewModel 与 FrmControl 如何组织试验过程

因此这层是：
- **现场流程权威源**
- **App 适配层权威源**
不是算法权威源。

### 3.5 数据库实体与真实分布层：`stp.db`

这层不是参考样例，而是真实业务库。
它的职责是：
- 决定真实表结构
- 决定真实 Method 分布
- 决定真实 RatedParams 枚举值域
- 决定真实 TestRecord/TestRecordItems/Attachments 关系

next-gen 做实体设计、Method 映射、推荐策略时，必须以它为一手数据基线。

---

## 4. next-gen 当前实现分层

### 4.1 Contracts 层

当前 `StandardTestNext.Contracts` 已经不是空壳，包含：
- `TestRecordContracts.cs`
- `MotorYMethodDecisionContract.cs`
- `MotorYMethodAdaptationPlanContract.cs`
- `TestRecordQueryGatewayFactory.cs`
- 共享消息总线/配置 contracts

说明：
- next-gen 已经把大量 Motor_Y 方法决策、适配计划、query 合同沉到 contracts 层
- 但这不等于算法迁移已经完成，它更多是在为算法迁移准备可消费的语义平面

### 4.2 Test 层

当前 `StandardTestNext.Test` 已经具备：
- builder / snapshot / query / adapter
- SQLite 持久化与回读路径
- smoke tests
- CLI/demo 输出

执行基线明确显示：
- 已跑通 `Test -> SQLite -> App读取` 闭环
- 已开始将 6 个 Motor_Y 核心业务项 payload 对齐到旧 `TestData_*` 形状
- 已大量沉淀 method decision / adaptation plan / cross-plan focus / source evidence 等分析能力

结论：
- Test 层现在**强在分析、投影、验证骨架**
- **弱在真正把 `Algorithm_Motor_Y.cs` 算法迁移落进主干**

### 4.3 App 层

当前 `StandardTestNext.App` 已经具备：
- `Program.cs`
- `AppBootstrap.cs`
- `InProcAppQueryGatewayFactory.cs`
- query detail/list 消费路径
- query resolution smoke tests

结论：
- App 侧已能消费 query 合同与 detail 信息
- 但它当前消费到的更多是 Test/query 投影结果，不是完整成熟的现场 WPF 业务前端

### 4.4 文档 / 执行基线层

当前存在：
- `docs/MIGRATION_PLAN.md`
- `docs/VERTICAL_SLICE_01.md`
- `EXECUTION_CONTEXT.md`

其中 `EXECUTION_CONTEXT.md` 已经极长，说明过去积累了大量状态；
但它也暴露一个问题：
- 当前 next-gen 沉淀了很多“适配准备 / 分析投影 / query 语义”能力
- 但尚未形成足够清晰的“真正开始迁移算法”的短路径

---

## 5. 旧/新对应关系

### 5.1 已对齐的地方

1. **方法族已对齐**
   - next-gen 已明确围绕 Motor_Y 六个核心试验项推进：
     - DcResistance
     - NoLoad
     - HeatRun
     - LoadA
     - LoadB
     - LockedRotor

2. **payload 形状开始向旧 DTO 收口**
   - 已开始对齐：
     - `DataList`
     - `RawDataList`
     - `ResultDataList`
     - `Data1List`
     - `Data2List`

3. **Method / Route / Profile 分析能力已沉淀**
   - next-gen 已能表达旧 Method 分布、dominant/baseline/recommended route、adaptation plan 等。

4. **query / SQLite / App 读取闭环已具备**
   - 这说明 next-gen 不是空仓库，主干骨架已能验证数据链路。

### 5.2 仍未真正打穿的地方

1. **旧算法本体尚未真正迁入**
   - 当前更多是：
     - legacy-shape
     - source evidence
     - adaptation plan
     - query/projected readiness
   - 而不是 `Algorithm_Motor_Y.NoLoad(...)` 在 net8 主干上真正跑起来。

2. **NoLoad 仍停在“可被旧形状消费”的前置阶段**
   - 即：
     - payload 更像旧对象了
     - 缺口更可见了
     - 但算法 adapter 本身并未真正打穿

3. **现场 App 流程尚未真正落成 next-gen 业务前端**
   - 当前 App 更像 query 消费主干，不是完整现场项目定制适配层。

---

## 6. 当前不要再猜的地方

1. **不要猜 NoLoad/Load_B 等字段名**
   - 直接以旧 `TestData_Motor_Y_*` 为准。

2. **不要猜方法名/中文业务项名/附件命名**
   - 直接以 `Settings_CYDJ.json` 为准。

3. **不要把旧 UI/ViewModel 当算法权威源**
   - UI/流程只负责现场流程与状态，不负责公式权威。

4. **不要把 next-gen 当前的 adaptation plan 当作算法已迁移完成**
   - 它是准备层，不是完成层。

---

## 7. 当前唯一主线

### 主线
**NoLoad 算法迁移试点**

原因：
- 它是 Motor_Y 第一阶段最关键核心试验项之一
- 旧 DTO、旧算法入口、旧配置方法名、旧现场流程都已有较明确参考
- next-gen 已经具备 legacy-shape 与 readiness 分析基础
- 最适合拿来打穿“旧算法 -> next-gen adapter -> verify”第一条真实业务链

---

## 8. 开始写代码前的首个切口

### 首个正式代码 checkpoint
**把 NoLoad 从“legacy-shape 可读”推进到“可执行的算法适配入口”**

也就是下一步写代码时，应优先做：

1. 确认 next-gen 里当前 NoLoad legacy-shape 与 builder 的真实实现位置
2. 明确 adapter 需要的最小输入字段集合
3. 先做一版 `NoLoad` adapter / mapper / executable path
4. 让它至少能在 next-gen 内被调用并做最小 verify

### 这一步的验收标准
不是“又多了一个分析 contract”，而是至少有一种：
- 新增/修改源码
- build/test 通过
- smoke test/CLI 验证输出
- commit

---

## 9. 当前恢复结论

本轮恢复后的明确判断是：

1. **旧项目权威来源已经明确分层**：
   - 算法：`Algorithm_Motor_Y.cs`
   - DTO：`TestData/Motor/Y/*`
   - 配置命名：`Settings_CYDJ.json`
   - 现场流程：`CYDJ_20220921` / `SINEN_TEST_11KW`
   - 真实数据：`stp.db`

2. **next-gen 当前主干并不空，但重心偏“迁移准备层”**：
   - 强在 contracts / query / snapshot / adaptation plan / source evidence / SQLite/App 读取闭环
   - 弱在真正把旧算法跑起来

3. **下一步不该继续扩分析层，而该进入 NoLoad 算法适配本体**。

---

## 11. 2026-04-13 收敛补记

### 11.1 本轮完成确认

截至 2026-04-13 下午，Motor_Y 这一轮围绕 smoke 稳定性、legacy shape 对齐与 query/adaptation-plan 语义修正的工作，已经从“局部通过”推进到“当前测试主工程 build 干净 + smoke 全绿”的状态。

已确认的收敛点包括：

1. **LoadB legacy root payload shape 已补齐**
   - `MotorYTrialRecordBuilder.BuildLoadBItem()` 现采用“先正常序列化匿名对象，再向 root JSON 注入 legacy key”的方式补齐旧系统字段：
     - `bad-point-refit`
     - `ratios`
     - `cuC`
   - 该实现避免了匿名对象无法直接声明带连字符成员名的问题，同时满足当前 smoke 对 root JSON / extension-data 的旧字段检查。

2. **NoLoad legacy-shape/computation smoke 前置条件已补齐**
   - `MotorYNoLoadLegacyShape` 已直接兼容 `U0DivideUnIsEquesToOne_*` 字段，并保留 `Theta0` 历史 key fallback。
   - `MotorYStpDbShapeAlignmentSmokeTests` 中 NoLoad 样本已补到“2 个低压点 + 3 个近额定点 + 更合理功率值”，使 `Pfw/Pfe` 拟合链路在 smoke 中真正进入可执行态。

3. **adaptation-plan / decision-anchor / formula-rule 一轮语义修正已形成当前工作树基线**
   - app-query/detail 已接入真实 item payload，适配计划覆盖率不再建立在空 payload 假设上。
   - LoadB 的 formula/rule 聚合已从旧的叙述字符串精确匹配切换为基于 `MotorYObservedAlgorithmEvidenceCatalog` 的 evidence-gap 语义。
   - 旧的 `cuC` required-intermediate-result 误判已移除，当前 bucket 语义以最新产品实现为准。

4. **遗留 build warning 已清零**
   - `MotorYDecisionAnchorResolutionFactory.cs` 的 `SuggestedNextStepPriority` 可空排序告警（CS8604）已修复。
   - `StpDbSnapshotQueryService.cs` 的 `selectedRoute?.CanonicalCode` 可能空引用告警（CS8602）已通过最小回退保护修复。

### 11.2 本轮验证结果

已实际执行并通过：

- `dotnet build /root/.openclaw/workspace/next-gen/StandardTestNext.Test/StandardTestNext.Test.csproj`
- `dotnet run --project /root/.openclaw/workspace/next-gen/StandardTestNext.Test -- --run-smoke-tests`

验证结果为：

- **Build succeeded**
- **0 Warning(s)**
- **0 Error(s)**
- **Smoke tests passed**

### 11.5 建议中的提交拆分方案（下一步执行依据）

基于当前 working tree 状态，后续若要进入真正提交阶段，建议至少拆为以下 3 组，而不要一次性混提：

#### Commit A：Motor_Y 主线语义与验证收敛

建议包含：

- contracts / seed / aggregate / query 透传
- Motor_Y builder / snapshot / evaluator / adaptation-plan / evidence 聚合
- NoLoad helper 与 legacy-shape 收敛
- 与上述逻辑直接绑定的 smoke / formatting 更新

目标：让审阅者能直接看到“这一轮产品语义修正 + smoke 收敛”的完整主线，不被日志/临时文件清理干扰。

#### Commit B：恢复/执行基线文档固化

建议包含：

- `docs/PROJECT_RECOVERY_2026-04-09.md`
- `EXECUTION_CONTEXT.md`

目标：把“现场如何恢复、working tree 如何分层、当前为何判断已收敛”独立沉淀，便于后续会话和审阅直接读取，不与产品代码 diff 混在一起。

#### Commit C：历史噪音与临时文件清理（可选独立提交）

建议包含：

- `CHANGELOG_2026-03-19.md`
- `WORKLOG_2026-03-19.md`
- `tmp_app_*`
- `tmp_test_*`

目标：把清理动作明确标记为“工作区卫生整理”，避免审阅者误以为这些删除与 Motor_Y 业务语义修复直接相关。

#### 非提交主线项

以下内容当前更适合作为运行产物或本地状态处理，不应默认进入主线提交：

- `artifacts/test-persistence/`

如果后续确认该目录承载必须版本化的测试基线，再单独讨论；在当前阶段，它应先被视为产物目录，而不是默认代码主线的一部分。

### 11.14 `git add .` 风险已被 dry-run 现场证实

此前虽然已经从分组设计上主张后续 staging 不应直接使用 `git add .`，但当时仍主要属于基于 working tree 分类的“结构性判断”。

本轮已进一步做了**全量 dry-run 验证**：

```bash
git add -n .
```

结果已现场证实：如果直接使用 `git add .`，当前会同时卷入以下几类本不应混在同一主线提交中的内容：

1. **B 组文档基线**
   - `EXECUTION_CONTEXT.md`
   - `docs/PROJECT_RECOVERY_2026-04-09.md`

2. **A 组 Motor_Y 主线代码 / 测试**
   - 多个 contracts / app-side / services / smoke / formatting 主线文件
   - 包括此前已验真的 `MotorYNoLoadComputation.cs`

3. **C 组历史噪音删除项**
   - `CHANGELOG_2026-03-19.md`
   - `WORKLOG_2026-03-19.md`
   - 多个 `tmp_app_*` / `tmp_test_*`

4. **明确不应混入主线语义提交的测试产物**
   - `artifacts/test-persistence/standardtest-next.db`

因此现在这件事已经从“建议”升级为“有 dry-run 证据支撑的提交规则”：

- **当前工作树绝不能直接使用 `git add .` 作为提交准备入口。**
- 后续 staging 必须继续按既定的 **B → A → C** 分组显式执行。
- 并且在任何正式暂存前，都应继续保留对 `artifacts/test-persistence/standardtest-next.db` 的显式排除检查。

这条规则的价值不只是“更整洁”，而是它已经被现场证明可以避免：

- 文档基线、主线代码、历史噪音、运行产物四类语义不同的变更被一次性混装；
- 从而降低后续 commit 审阅、回滚、恢复和归档时的混淆风险。
