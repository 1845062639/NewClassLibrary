## 2026-03-22 16:41
- app/contracts/test: keep App's default query path on a real seeded in-proc gateway, but remove the compile-time `StandardTestNext.App -> StandardTestNext.Test` project reference; `StandardTestNext.App/Application/InProcAppQueryGatewayFactory.cs` now reflects into `StandardTestNext.Test.Application.AppSide.InProcAppQueryGatewaySeedFactory` at runtime so App still only explicitly depends on `StandardTestNext.Contracts`.
- contracts/test: add `TestRecordQuerySeedContracts.cs` and centralize the default seeded rated params + realtime samples behind `TestRecordQuerySeedFactory`, so the App preview path and later smoke/demo hosts can reuse one stable seed contract instead of hand-rolling duplicate fake data.
- test: route `TestRecordQueryViewAssembler` through `TestReportSelection.SelectPrimary/SelectLightweight`, keeping recent/detail report picking on explicit rules instead of repository ordering; lightweight fallback now prefers `manifest` before oldest-entry fallback.
- build/docs: re-run `dotnet build StandardTestNext.sln --no-restore` and keep the solution green at 0 warning / 0 error; sync App/Test READMEs and worklog to the reflected seeded-gateway baseline.

## 2026-03-22 11:41
- app/test: add `InProcAppQueryGatewaySeedFactory` so App default record-query path now resolves to a seeded in-proc gateway instead of a real adapter backed by empty in-memory repositories.
- app: switch `StandardTestNext.App/Program.cs` to build its default query dependency through the seeded gateway factory, making recent-list/detail/report-summary preview exercise a real query chain with actual seeded data.
- docs: sync worklog with the new seeded gateway baseline so the next step can move from `seeded in-proc` to configurable `remote/sqlite-backed/null-fallback` strategies.

## 2026-03-22 09:11
- test: add `TestReportSelection` and move primary/lightweight report picking out of `TestRecordQueryViewAssembler`, so both recent-list and detail assembly stop depending on incidental repository ordering.
- test: make summary/detail assembly choose reports through explicit fallback rules (`IsPrimaryEntry` / `json` / latest and `IsLightweightEntry` / `manifest` / oldest fallback), leaving one reuse point for later App/API/report-history consumers.
- docs: sync worklog with the tightened report-selection boundary so the next step stays focused on replacing the App query stub with a real gateway.

## 2026-03-22 07:41
- app: route the default test-record query dependency through `TestRecordQueryGatewayDefaults.Create()` instead of constructing `TestRecordQueryGatewayStub` inline inside `AppBootstrap`, so the App runtime now has a single swap point for future real query gateways.
- app: add `TestRecordQueryGatewayDefaults` as the current default gateway factory while keeping the stub as the fallback implementation, reducing the amount of query-bridge wiring embedded in the App bootstrap path.
- docs: sync worklog/migration notes so the next step stays focused on replacing the default factory target with a real gateway instead of adding more placeholder query shape.

# CHANGELOG 2026-03-19

## 2026-03-22 05:11
- app: remove legacy `ContractsBridge/TestRecordDetailContract.cs` and `ContractsBridge/TestRecordListItemContract.cs` alias-shell files so App now consumes shared record query contracts directly from `StandardTestNext.Contracts` instead of keeping local mirror wrappers.
- build: re-run `dotnet build StandardTestNext.sln --no-restore` after the cleanup and keep the mainline green at 0 warning / 0 error.
- docs: sync migration/worklog notes with the contract-shell cleanup so the next step stays focused on replacing the App stub with a real query path.

## 2026-03-22 04:41
- contracts/app/test: extend `TestRecordListItemContract` / `TestRecordSummary` / `TestRecordListView` with `ItemAttachmentBucketCount` so recent-list consumers can see both record-level attachments and item attachment buckets without forcing a second detail query.
- test: update `TestRecordQueryViewAssembler` to build recent-list item details from actual item attachments and carry `record attachments + item attachment buckets + primary/lightweight artifact` state into the shared list view.
- app/test: align `TestRecordQueryGatewayAdapter`, `TestRecordQueryGatewayStub`, `AppBootstrap`, and `TestBootstrap` with the richer recent-list contract so App-side preview output now exercises the same attachment/report summary shape as Test-side runtime output.
- docs: sync worklog and migration plan with the tightened App/Test query-contract boundary so the next step is replacing the stub with a real App query path rather than inventing more placeholder fields.

## 2026-03-22 02:11
- test: enrich realtime sample partition descriptors with `DisplayName` + `SortOrder` and make `TestRecordSamplePartitioner` return deterministic partition order, so later report/detail consumers stop inferring semantics from `ItemCode` or insertion order.
- test: propagate partition metadata into `TestRecordSamplePartitionSummary`, `TestRecordItemDetail`, and `TestRecordMappingSnapshotFactory`, so mapping snapshots and detail views now carry stable presentation metadata alongside counts.
- test: update `TestRecordQueryViewAssembler` to emit ordered item details with explicit display names for `RealtimeKeyPoints` / `RealtimeContinuous`, reducing downstream App/API guesswork before formal report templates arrive.
- test: add `TestReportManifest` / `TestReportManifestMapper` / `ManifestTestReportRenderer` and wire manifest export into `TestBootstrap` so the report boundary now also produces a lightweight summary artifact instead of forcing every downstream consumer to read full JSON/Markdown payloads.
- Test: let `TestBootstrap` export both JSON and Markdown report artifacts, and persist both formats into report history/summaries so report abstraction is exercised by the runtime path rather than only kept as idle interfaces.

## 05:08
- test: add `TestRecordQueryViewAssembler` + `TestRecordItemPayloadReader` so recent/detail query DTO assembly and `DataJson` payload parsing stop being duplicated inside `TestRecordQueryService`
- test: extend `TestRecordDetail` with `Mapping` and update bootstrap reload output to print reloaded `samples/kp/cont` snapshot directly
- docs: align test README/worklog with the tightened query-assembly boundary

## 19:05
- test: wire product-definition query service into TestBootstrap so the demo path now prints recent products plus by-kind readback instead of leaving product queries as repository-only plumbing
- docs: align test README / worklog / migration plan with the product-definition query path now being exercised in runtime output

## 18:40
- test: add IProductDefinitionQueryService + ProductDefinitionQueryService and extend product repositories with ListRecentAsync so product-definition reads stop leaking back into bootstrap paths
- docs: align test README/worklog with the new product-definition query boundary

## 18:22
- test: extend TestRecordSummary/TestRecordQueryService so recent record queries also carry product code/model and whether the product definition was already resolved on the record
- test: update bootstrap recent-record output to print product-definition reuse plus report/artifact state directly
- docs: align test README / worklog / migration plan with the tightened recent-list query view

## 18:05
- test: extend TestRecordSummary/TestRecordQueryService so recent record queries now carry report count, artifact availability, and latest report timestamp
- test: update bootstrap recent-record output to print report/artifact state directly instead of record code only
- docs: align test README / worklog / migration plan with the tightened recent-list query view

## 17:10
- test: add ITestProductDefinitionService + TestProductDefinitionService so product definitions are reused by productKind and refreshed only when rated-params snapshots change
- test: update TestBootstrap/TestRecordAggregateBuilder so record construction consumes a resolved product definition instead of always creating a new inline ProductDefinition
- docs: align test README / worklog / migration plan with the new product-definition resolution boundary

## 12:40
- app/test: fix Program.cs boundaries so each executable now loads only its own runtime config and bootstrap instead of cross-starting the other side
- app/test: carry messageBus config through startup options and add minimal runtime validation/reporting for provider/port/clientId/topicPrefix plus app/test-specific warnings
- docs: align runtime configuration, architecture, migration notes, vertical slice, and app/test READMEs with the corrected single-side entrypoint and validation baseline

## 07:18
- app/test: add IMessageBus + MessageBusFactory + MessageBusOptions and switch Program/Bootstrap entrypoints from concrete InMemoryMessageBus signatures to the bus abstraction
- app: expose `messageBus.*` runtime settings in appsettings.app.json as the minimal landing zone for future MQTT/other providers while keeping `inmemory` as the current default
- docs: align vertical slice / migration plan / app-test readmes and worklog with the new pluggable-bus baseline


## 06:10
- test: add HasRemark to TestRecordItemDetail and expose HasReports/HasReportArtifacts on TestRecordDetail so detail consumers can render state without rescanning raw remark/report collections
- test: update bootstrap reload output to print item remark flags and report artifact availability for the current record-detail demo path
- docs: align test README/worklog with the tightened record-detail query view

## 05:38
- test: add TestRuntimeConfiguration + loader and wire appsettings.test.json as the default runtime config source before env/cli overrides
- test: extend startup parser with --config so deployment-specific config files can be selected without changing code
- docs: align README / migration plan / vertical-slice notes with the new config-file-first runtime path
- ops: tencent-docs maintenance files are still blocked by MCP access limit (-32603), so online sync remains pending quota recovery

## 05:10
- test: carry report artifact filename/path into TestReportSnapshot so record-detail queries no longer need a second join step just to surface exported files
- test: update in-memory/sqlite report snapshot queries and bootstrap output so record reports now show artifact metadata directly
- docs: align README / migration plan / worklog with the new report-artifact-in-detail state

## 04:10
- docs: align repo docs with the actual startup-entry state so Test command-line persistence switches are documented as already wired into Program.cs
- docs: update migration plan / vertical slice next steps to move from dual command-line+env entrypoints toward a unified runtime configuration path
- ops: attempted to read Tencent Docs maintenance files for App/Test updates, but current tencent-docs MCP calls were blocked by access limit (-32603), so the online sync is pending quota recovery

## 02:43
- test: extend record attachment repository with record/item readback queries for in-memory and sqlite persistence
- test: add TestRecordDetail and upgrade record query service from aggregate-only reload to detail reload with attachment buckets
- test: update bootstrap demo output to print reloaded attachment counts so record persistence path proves write + readback instead of write-only

## 02:11
- test: add MarkdownTestReportRenderer so report export boundary now supports markdown draft output alongside json preview
- docs: update test README to reflect markdown report draft capability and next reporting step

## 01:40
- test: extract realtime sample partition summaries/statistics from aggregate builder into reusable mapping result models
- test: carry record statistics into report document and console summary so report preview no longer has to infer counts from raw DataJson
- docs: update migration/readme/worklog to reflect the new mapping/statistics boundary

## 01:10
- test: wire sqlite repositories into TestBootstrap with STNEXT_TEST_PERSISTENCE mode switching
- docs: update migration/worklog/readme to reflect real sqlite-enabled runtime path

## 00:40
- docs: align StandardTestNext.Test README with actual query/report demo coverage
- docs: record that bootstrap output now proves recent records, report summaries, and record reload paths

## 00:12
- test: add report query service boundary for recent export summaries
- test: extend in-memory/sqlite report repositories and bootstrap demo with summary readback

## 23:45
- test: add SQLite persistence bootstrap and repository samples for product/record/attachment/report storage
- test: wire sqlite package reference and document the new persistence landing zone in README/worklog

## 21:10
- test: extend in-memory record repository with find-by-record-code and recent-list query semantics
- test: add record query service and wire bootstrap readback demo for replay/list scenarios

## 20:40
- test: add report persistence summary model for lightweight export history metadata
- test: persist report summary alongside full report content in in-memory repository demo

## 20:10
- test: add report artifact descriptor and timestamped file naming strategy
- test: expose persisted artifact metadata to bootstrap output

## 19:40
- test: add filesystem report artifact writer and persist JSON report preview to disk
- test: extend report export service with export-and-write flow

## 19:10
- test: add report repository boundary and persist report export demo flow
- test: split report export into document-build and render steps

## 本次修改目标
基于当前从 git 拉下来的旧 Test 与旧 App，整理既有架构与数据模型经验，并在 `next-gen/` 中建立新的主干项目骨架。

---

## 一、新建的 next-gen 项目文件

### 解决方案
- `next-gen/StandardTestNext.sln`

### Contracts
- `next-gen/StandardTestNext.Contracts/StandardTestNext.Contracts.csproj`
- `next-gen/StandardTestNext.Contracts/ContractTopics.cs`
- `next-gen/StandardTestNext.Contracts/MotorRatedParamsContract.cs`
- `next-gen/StandardTestNext.Contracts/MotorRealtimeSampleContract.cs`
- `next-gen/StandardTestNext.Contracts/TestCommandContract.cs`
- `next-gen/StandardTestNext.Contracts/DeviceStatusContract.cs`

### App
- `next-gen/StandardTestNext.App/StandardTestNext.App.csproj`
- `next-gen/StandardTestNext.App/Program.cs`
- `next-gen/StandardTestNext.App/Application/AppBootstrap.cs`
- `next-gen/StandardTestNext.App/Application/MotorSamplingService.cs`
- `next-gen/StandardTestNext.App/Application/DeviceStatusReportingService.cs`
- `next-gen/StandardTestNext.App/Application/AppCommandConsumer.cs`
- `next-gen/StandardTestNext.App/ContractsBridge/IMessagePublisher.cs`
- `next-gen/StandardTestNext.App/ContractsBridge/IMessageSubscriber.cs`
- `next-gen/StandardTestNext.App/ContractsBridge/InMemoryMessageBus.cs`
- `next-gen/StandardTestNext.App/Devices/IMotorDeviceGateway.cs`
- `next-gen/StandardTestNext.App/Devices/MockMotorDeviceGateway.cs`

### Test
- `next-gen/StandardTestNext.Test/StandardTestNext.Test.csproj`
- `next-gen/StandardTestNext.Test/Program.cs`
- `next-gen/StandardTestNext.Test/Application/TestBootstrap.cs`
- `next-gen/StandardTestNext.Test/Application/Abstractions/ITestRecordRepository.cs`
- `next-gen/StandardTestNext.Test/Application/Abstractions/IProductDefinitionRepository.cs`
- `next-gen/StandardTestNext.Test/Application/Abstractions/IRecordAttachmentRepository.cs`
- `next-gen/StandardTestNext.Test/Application/Services/MotorTestSessionService.cs`
- `next-gen/StandardTestNext.Test/Application/Services/TestCommandBuilder.cs`
- `next-gen/StandardTestNext.Test/Application/Services/TestRuntimeOrchestrator.cs`
- `next-gen/StandardTestNext.Test/Application/Services/TestRecordAggregateBuilder.cs`
- `next-gen/StandardTestNext.Test/Application/Services/TestRecordItemMapper.cs`
- `next-gen/StandardTestNext.Test/Application/Services/TestRecordConsolePresenter.cs`
- `next-gen/StandardTestNext.Test/Application/Services/JsonTestReportRenderer.cs`
- `next-gen/StandardTestNext.Test/Application/Services/TestReportDocument.cs`
- `next-gen/StandardTestNext.Test/Application/Services/TestReportDocumentMapper.cs`
- `next-gen/StandardTestNext.Test/Application/Services/TestReportExportService.cs`
- `next-gen/StandardTestNext.Test/Application/Abstractions/ITestReportRenderer.cs`
- `next-gen/StandardTestNext.Test/Domain/Records/ProductDefinition.cs`
- `next-gen/StandardTestNext.Test/Domain/Records/TestRecordAggregate.cs`
- `next-gen/StandardTestNext.Test/Domain/Records/TestRecordItemAggregate.cs`
- `next-gen/StandardTestNext.Test/Domain/Records/RecordAttachment.cs`
- `next-gen/StandardTestNext.Test/Infrastructure/Persistence/InMemoryTestRecordRepository.cs`
- `next-gen/StandardTestNext.Test/Infrastructure/Persistence/InMemoryProductDefinitionRepository.cs`
- `next-gen/StandardTestNext.Test/Infrastructure/Persistence/InMemoryRecordAttachmentRepository.cs`

### 文档
- `next-gen/README.md`
- `next-gen/WORKLOG_2026-03-19.md`
- `next-gen/docs/ARCHITECTURE.md`
- `next-gen/docs/MIGRATION_PLAN.md`
- `next-gen/docs/VERTICAL_SLICE_01.md`

---

## 二、当前已落地内容

### 1. 新系统分层
已明确拆分为：
- `StandardTestNext.App`
- `StandardTestNext.Test`
- `StandardTestNext.Contracts`

### 2. 第一批共享契约
已定义：
- `MotorRatedParamsContract`
- `MotorRealtimeSampleContract`
- `TestCommandContract`
- `DeviceStatusContract`
- `ContractTopics`

### 3. 第一条垂直切片骨架
已建立：
- Test 侧生成额定参数示例
- Test 侧生成并发布启动命令
- App 侧读取模拟设备采样并发布样本
- App 侧订阅启动命令并回报设备状态
- 两侧统一通过 Contracts 交换边界模型
- Test 侧在命令链路之后生成最小试验记录聚合，覆盖产品定义、记录、记录分项、附件四类核心骨架

### 4. 记录主线阶段性决策
- 参考旧 `StandardTest.Model` 后，`ProductType.RatedParams` 与 `TestRecordItem.Data` 在 phase-1 继续保留为 JSON 载荷
- 新主干先围绕聚合边界稳定下来，不急着复制旧 ORM/框架基类
- 附件模型保留独立对象，并继续按“记录级 + 分项级”双层挂载方式设计

---

## 三、参考旧系统提炼出的事实
本轮主要参考来源：
- `ClassLibary/StandardTest.Model/`
- `ClassLibary/StandardTest.ViewModel/`
- `ClassLibary/StandardTest/`
- `app-repos/StandardTestApp/`

目前已确认：
1. 旧 Test 侧的核心实体主线为：
   - `ProductType`
   - `TestRecord`
   - `TestRecordItem`
   - `FileAttachment`
2. 旧系统中附件更多是挂在 `TestRecordItem` 维度
3. 旧 App 中包含大量设备侧控制界面与现场变体项目
4. 新系统应吸收其设备控制经验，但不继承其历史耦合结构

---

## 四、后续计划
1. 将 `InMemoryMessageBus` 替换为可落地的 MQTT/总线抽象
2. 在 `next-gen/Test` 中补试验记录主线模型
3. 在 `next-gen/App` 中继续补设备适配层抽象与真实设备样板
4. 继续从旧 Test / App 中提炼记录、附件、控制动作的稳定模型
5. 待主机具备 .NET SDK 后补做编译与运行验证

---

## 五、说明
旧仓库中的部分改动仍保留在工作区，当前主方向已经转为 `next-gen/` 新项目主干建设。后续检查时，可优先查看 `next-gen/` 下文件。 

- runtime bridge: attempted to collapse Test-side message-bus interfaces onto Contracts aliases, but reverted after `dotnet build StandardTestNext.sln --no-restore` exposed CS8914 and unresolved interface types; follow-up should migrate namespace usage properly instead of placing `global using` inside namespace-scoped files
