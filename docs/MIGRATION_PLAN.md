# StandardTest Next 迁移计划

## 阶段 0：识别与抽取
- 从旧 Test 提取产品模型、试验模型、记录模型经验
- 从旧 App 提取设备接入模式、实时采集字段、控制动作集合
- 抽取共享契约，不继承旧命名耦合

## 阶段 1：新项目骨架
- 建立 `StandardTestNext.Contracts`
- 建立 `StandardTestNext.App`
- 建立 `StandardTestNext.Test`
- 定义第一批 topic 与 DTO

## 阶段 2：首个垂直切片
建议先做一个最典型切片：
- Motor 额定参数下发
- 实时采集样本上传
- Test 侧接收并形成试验记录中间态

## 阶段 3：记录与报告主链路
- 记录实体（已补最小聚合骨架）
- 附件模型（已补独立对象骨架）
- 报表生成接口
- 导出链路

### 阶段 3 当前落点
- 已在 `StandardTestNext.Test/Domain/Records` 建立 `ProductDefinition / TestRecordAggregate / TestRecordItemAggregate / RecordAttachment`
- 本小时继续把产品定义边界往前推：新增 `ITestProductDefinitionService` + `TestProductDefinitionService`，让 Test 启动链路先按 `productKind` 复用已有 `ProductDefinition`，当额定参数快照变化时再更新，而不是每次构建记录都重新 new 一份产品定义
- Phase-1 决策：`RatedParamsJson`、`DataJson` 继续保留 JSON 负载，先稳定新边界，再逐步结构化
- 报告输出抽象已补最小接口：`ITestReportRenderer` + `JsonTestReportRenderer` + `MarkdownTestReportRenderer` + `ManifestTestReportRenderer`，并新增 `TestReportDocumentMapper` 先把记录聚合收敛为独立报告文档模型；当前 `TestBootstrap` 已能从该文档模型同时导出 JSON 预览、Markdown 草稿与 manifest 概览，并将三种格式都写入报告仓储/摘要
- `TestReportDocument` 已补 `Metadata` 与 `AccompanyProduct`，报告层开始摆脱对领域聚合字段命名的直接耦合
- 当前已补最小报告仓储边界：`ITestReportRepository` + `InMemoryTestReportRepository`，先把“报告文档 + 渲染结果”与记录聚合分开持久化，后续再替换为 SQLite/EF/Dapper 落地实现
- 当前已补最小报告制品落盘边界：`ITestReportArtifactWriter` + `FileSystemTestReportArtifactWriter`，phase-1 先将 JSON 报告预览直接写入本地 artifacts，后续再替换为 Word/PDF/模板导出
- 当前已补轻量报告历史摘要：`TestReportPersistenceSummary`，把导出时间、文件名、保存路径、内容长度从完整报告正文中再单独抽出一层，便于后续做导出历史列表、审计与查询
- 已补最小记录查询边界：`ITestRecordQueryService` + `TestRecordQueryService`，当前先支持按 `RecordCode` 回放与 recent list 摘要，后续可平滑替换为 SQLite/EF/Dapper 查询实现
- 本小时继续补最小报告查询边界：`ITestReportQueryService` + `TestReportQueryService`，并让 in-memory / SQLite 两套报告仓储都支持 recent summaries 回读，避免“报告历史”继续只是写入而不能查询
- 已将 `SQLite*Repository` 真正接入 `TestBootstrap` 运行路径，可通过 `STNEXT_TEST_PERSISTENCE=sqlite` 切换到真实 SQLite 闭环，当前默认仍为 memory，降低 demo 启动门槛
- 本小时继续把“样本映射策略”往前推：`TestRecordItemMapper` 不再只返回匿名分项列表，而是返回 `TestRecordItemMappingResult`（记录分项 + 分区摘要），并补 `TestRecordStatistics` 让报告文档与控制台摘要都能消费同一份统计结果
- `TestReportDocument` 已补 `Statistics` 区块，当前 JSON 报告预览已能显式带出 item/sample 汇总，后续正式报告模板可直接复用这层摘要而不是重新反推 `DataJson`
- 已补一版启动参数入口：`--persistence memory|sqlite` 与 `--sqlite-db <path>`，并已真正接入 `StandardTestNext.Test/Program.cs`，当前命令行参数、环境变量两条入口都可驱动 `TestBootstrap`
- 本小时继续把运行入口往统一配置收口：新增 `TestRuntimeConfiguration` / `TestRuntimeConfigurationLoader` 与 `appsettings.test.json`，当前默认配置文件、环境变量、命令行三层都可驱动 `TestBootstrap`，优先级已明确为“配置文件 < 环境变量 < 命令行参数”
- 本小时继续把这套 runtime 配置真正平移到 App 侧：新增 `AppRuntimeConfiguration` / `AppRuntimeConfigurationLoader` / `AppStartupOptionsParser` 与 `appsettings.app.json`，并让 `AppBootstrap` / `MockMotorDeviceGateway` / `AppCommandConsumer` 消费 `deviceId`、`productKind`、`samplingMode`，先把双端入口收敛到同一种“配置文件 + 环境变量 + CLI 覆盖”模式
- 本小时继续把消息桥从具体实现里抽出来：新增 `IMessageBus` / `MessageBusFactory` / `MessageBusOptions`，并让 App/Test 两个 Program 与 Bootstrap 都改为依赖总线抽象；当前默认 provider 为 `inmemory`，双端配置文件均已补齐同构的 `messageBus.*` 连接参数，作为后续接 MQTT 的最小落点
- 本小时继续把消息桥边界再收一层：`AppCommandConsumer` 已从单独依赖 `IMessageSubscriber` 收口为直接依赖 `IMessageBus`，当前 App/Test 运行主干都只认同一套总线入口，避免后续接 MQTT 时还要维护额外注入分叉
- 本小时继续把消息总线配置从单一 provider 推到连接参数级：`AppRuntimeConfiguration.MessageBus` / `TestRuntimeConfiguration.MessageBus` / `MessageBusOptions` 已补 `host`、`port`、`clientId`、`topicPrefix`、`username`、`password` 占位，App/Test 两个 Program 已统一透传这组配置；其中 Test 入口已不再借道 App 配置读取总线参数，后续可分别按部署目录维护双端配置文件
- 本小时继续把“口头约定”落成正式文档：新增 `docs/RUNTIME_CONFIGURATION.md`，统一 App/Test 配置文件名、共享 `messageBus.*` 键名、环境变量/命令行覆盖链路与建议部署目录，后续接 MQTT 与部署脚本时可直接复用
- 本小时继续把消息总线环境变量覆盖补到连接参数级：新增 `MessageBusOptionsFactory`，App/Test 双端 Program 已统一通过 `STNEXT_MESSAGE_BUS_HOST|PORT|CLIENT_ID|TOPIC_PREFIX|USERNAME|PASSWORD` 覆盖 `messageBus.*` 配置；当前 `PORT` 非法时回退配置文件值，先保证启动路径稳定
- 本小时继续按待办把“配置校验/启动前自检”推进到真实代码：新增 `RuntimeConfigurationValidator` + `RuntimeConfigurationConsoleReporter`，App/Test 双端入口会在启动时打印当前 runtime/messageBus 摘要，并对 provider、port、clientId、topicPrefix、samplingMode、persistenceMode、sqliteDbPath 给出最小告警
- 本小时继续把这层护栏从“控制台提示”推进到“启动前失败”：`RuntimeConfigurationValidationResult` 已补 `Errors`，App/Test 双端 `Program.cs` 会在报告配置摘要后调用 `ThrowIfInvalid`；当前不支持的 provider、非法端口、空 `clientId` / `topicPrefix`、非法 `samplingMode` / `persistenceMode` 会直接阻断启动，先把最容易误配的假绿路径封住
- 同步修正双端 `Program.cs` 启动边界：App/Test 入口各自只加载自身配置并启动自身 Bootstrap，不再互相串拉对方 Bootstrap/配置，避免后续接 MQTT/部署脚本时继续把 demo 串线状态带进主干
- 本小时继续把消息总线配置入口从“配置文件 + 环境变量”推进到“配置文件 + 环境变量 + CLI”：`AppStartupOptionsParser` / `TestStartupOptionsParser` 已支持 `--message-bus`、`--message-bus-host`、`--message-bus-port`、`--message-bus-client-id`、`--message-bus-topic-prefix`、`--message-bus-username`、`--message-bus-password`，后续切 MQTT/provider 调试时不用再依赖改部署目录配置文件
- 本小时继续把此前的假绿缺口补成真实实现：`MessageBusFactory` 已真正接入 `mqtt` provider，当前 validator 与 runtime 行为已对齐，不再出现“配置校验通过、运行直接 `NotSupportedException`”的状态
- 本小时继续顺手清理共享基础设施技术债：`IMessageBus` 已不再继承 obsolete 的 `IMessageSubscriber` 兼容接口，`Subscribe<T>` 直接并入主总线接口；复验 `dotnet build StandardTestNext.sln --no-restore`、`dotnet run` App/Test 两端均通过，当前主干恢复为 0 warning / 0 error
- 本小时继续把“连接/目录级自检”落到真实代码：`provider=mqtt` 时 App/Test 启动前会检查 `messageBus.host` 非空，并对 `host:port` 做轻量 TCP 探测；Test 在 `persistenceMode=sqlite` 且显式传入 `sqliteDbPath` 时会预探测目录可写性，优先暴露部署期配置坑
- 本小时继续把“真实跨进程联调入口”补成可复用脚手架：新增 `scripts/run-mqtt-smoke.sh`，统一启动 App/Test 双进程、透传 `mqtt` provider 与 broker 参数，并默认让 Test 落 SQLite；当前机器未发现本地 broker 可执行文件，因此本轮先把脚本、日志约定与文档落地，待 broker 就位后即可直接验证真实 MQTT 链路
- 本小时继续把“启动前可达性 warning”推进到结构化诊断：新增 `ConnectivityProbeResult`，App/Test 双端在 `provider=mqtt` 时会把端点自检细分为 `reachable / timeout / connection-refused / dns-failed / auth-failed / probe-failed`，减少联调阶段人工翻异常堆栈的成本
- 下一步优先补：在已补连接参数配置骨架、细粒度环境变量入口、公共配置说明、CLI 覆盖入口、已落地的 MQTT 最小实现、已清理的总线兼容层、已落地的 host/目录级启动前自检、已补好的结构化连通性诊断以及已补好的双进程 smoke 脚手架前提下，继续推进 MQTT 稳定联调、把当前 TCP 级探测升级为更严格的 MQTT 认证/权限级自检、正式报告模板渲染、在已落地的 SQLite 样板基础上细化表结构/查询模型并评估是否继续引入 EF 或 Dapper、样本映射策略与试验方法编码的对应表
- 本小时继续把记录查询边界从“回读聚合”推进到“回读聚合 + 附件明细”：`IRecordAttachmentRepository` 已补 record/item 两级附件查询接口，`ITestRecordQueryService` 已返回 `TestRecordDetail` 组合结果，为后续记录详情页、报告附件清单、审计查询预留稳定边界
- 本小时继续把“报告摘要/导出制品引用并入查询对象”往前推：`ITestReportRepository` / `ITestReportQueryService` 已补按 `RecordCode` 回读 `TestReportSnapshot`，`TestRecordDetail` 已并入 report snapshots / report summaries，当前记录详情查询不再只能看到聚合与附件，也能看到同记录下的报告正文快照与摘要元信息
- 本小时继续把 item 级统计显式化：新增 `TestRecordItemDetail`，并将 `ItemCode / MethodCode / RecordMode / SampleCount / AttachmentCount` 收敛到 `TestRecordDetail.ItemDetails`，减少后续详情页/API 对 `DataJson` 的直接理解成本
- 本小时继续把导出制品引用真正并入记录详情查询：`TestReportSnapshot` 已直接携带 `ArtifactFileName / ArtifactSavedPath`，`TestRecordDetail.Reports` 不再只有正文快照，后续详情页可直接落报告文件链接/路径展示
- 本小时继续把 recent list 也收敛成更稳定的查询视图：`TestRecordSummary` 已新增 `ReportCount / HasReportArtifacts / LatestReportSavedAt / ProductCode / ProductModel / ReusedProductDefinition`，`TestRecordQueryService.ListRecentAsync` 会同步拼出最近记录的报告存在性、制品状态以及产品定义复用状态，减少后续列表页/API 再逐条回查报告仓储或产品主数据
- 本小时继续把产品主数据也从“只写不查”往前推：新增 `IProductDefinitionQueryService` + `ProductDefinitionQueryService`，并为产品仓储补 `ListRecentAsync`，后续无论是主数据列表、记录详情产品信息回查还是主数据对账，都可以先走稳定查询边界而不是继续把读取逻辑散回 Bootstrap
- 本轮继续把这条边界从“接口存在”推进到“启动链路真实使用”：`TestBootstrap` 已直接补 recent products 与 `GetByKind(productKind)` 回读输出，后续做 API/UI 时可先复用这层查询服务，而不是重新在 demo/控制台里拼仓储读取
- 本小时继续把记录查询组装逻辑从服务内部往外收口：新增 `TestRecordQueryViewAssembler` 与 `TestRecordItemPayloadReader`，把 recent/detail 两条查询路径共用的 `ItemDetails / Mapping / ReportSummaries` 组装以及 `DataJson -> SampleCount/RecordMode` 解析统一下来；`TestRecordDetail` 也已直接携带 `Mapping`，后续详情页/API 无需再自行扫描 item 列表回算 `samples/kp/cont`
- 本小时继续把“样本映射策略”从纯统计结果推进到带展示语义的稳定元数据：`TestRecordSamplePartitionDescriptor` 已补 `DisplayName / SortOrder`，`TestRecordSamplePartitioner`、`TestRecordSamplePartitionSummary`、`TestRecordItemDetail`、`TestRecordMappingSnapshotFactory` 现会统一传递和保序这层信息，后续 App 详情页、轻量报告、正式模板都可直接复用，不必再把 `ItemCode -> 展示名/顺序` 的映射散落在多个消费端
- 本小时继续把这条查询边界往 App 侧推一小步：当前共享 `TestRecordContracts.cs` 已统一承载 `detail/list/item/report-summary` 契约，Test 侧 `TestRecordQueryGatewayAdapter` 已能完整投影 `ItemDetails / ReportSummaries`，App 侧 `TestRecordQueryGatewayStub` 也已补齐同结构占位返回；本轮又把 `AppBootstrap` 从“只看 recent list”推进到“继续读取 detail 并打印 item/sample/report artifact 摘要”，让 App 主干先真正消费一遍 detail 合同；下一步不再是继续扩 Contract 字段，而是让真实 App 查询消费路径替换掉 stub


## 2026-03-22 10:11 App 默认查询主路径改走真实 in-proc adapter
- 本小时继续按待办推进“让 App 默认查询入口替换 stub”，这次不再只是加工厂切换点，而是把默认主路径真正切到了可工作的 Test 查询 adapter。
- 新增 `StandardTestNext.Contracts/TestRecordQueryGatewayFactory.cs`，将默认工厂与 null fallback 下沉到共享 contracts；stub 不再作为 App 项目内主路径实现存在，而是退为 resolver 缺席时的共享兜底。
- `StandardTestNext.App/Program.cs` 已开始实际组装 `InMemoryTestRecordRepository + InMemoryRecordAttachmentRepository + InMemoryTestReportRepository + TestRecordQueryService + TestRecordQueryFacade + TestRecordQueryGatewayAdapter`，并把该 gateway 注入 `AppBootstrap`。
- 同步删除 App 侧重复壳文件：
  - `StandardTestNext.App/Application/TestRecordQueryGatewayFactory.cs`
  - `StandardTestNext.App/Application/TestRecordQueryGatewayStub.cs`
- `StandardTestNext.App/StandardTestNext.App.csproj` 已新增对 `StandardTestNext.Test` 的项目引用；当前这是阶段性接受的 in-proc 耦合，目的是优先结束“App 默认查询只能消费假数据”的状态。
- 已执行 `dotnet build StandardTestNext.sln --no-restore` 复验通过，结果仍为 `0 warning / 0 error`。
- 下一步应继续把这条默认 in-proc adapter 提升为可配置策略（in-proc / remote / null fallback），并开始让 App 可选择接真实 Test 持久化数据源，而不是继续停留在“空 in-memory 仓储 + 真 adapter”这层半步状态。

## 阶段 4：旧系统并行期
- 新旧系统并行验证
- 逐步把新增能力放到新系统
- 旧系统仅维护关键稳定性

## 当前决策
- 重点转向新项目，不再以大规模修正旧 App 为主要路径
- 旧项目继续作为领域参考和迁移素材

## 2026-03-22 04:41 App/Test recent list 契约继续收口
- 本小时继续顺着 App/Test 查询契约待办往前推：`StandardTestNext.Contracts/TestRecordContracts.cs` 的 recent list 契约新增 `RecordAttachmentCount / ItemAttachmentBucketCount`，不再只暴露样本数和报告数。
- `TestRecordSummary`、`TestRecordListView`、`TestRecordQueryViewAssembler`、`TestRecordViewMapper` 已同步把记录级附件数、分项附件桶数、primary artifact、lightweight artifact 收敛到 recent list 视图里；其中 summary 组装不再用空附件集合占位，而是直接消费 `item.Attachments`。
- `TestRecordQueryGatewayAdapter`、`TestRecordQueryGatewayStub`、`AppBootstrap`、`TestBootstrap` 已一并改为消费这组 richer list 契约，当前 App 侧 recent list 预览与 Test 侧运行输出都能直接看到 `attachments + primary/lightweight report artifact` 摘要。
- 这一步的真实目标不是继续堆 contract 字段，而是把 App 侧列表查询先收敛到与 Test 侧 detail/query 相同的语义平面上；下一步应开始用真实 App 查询入口替换 stub，而不是继续扩一次性占位结构。

## 2026-03-22 05:11 App 合同镜像壳文件清理
- 本小时先复核 `next-gen/` 当前真实状态：`dotnet build StandardTestNext.sln --no-restore` 通过，主干仍保持 `0 warning / 0 error`。
- 继续按上轮待办里“清理 Contracts 目录/消费端残留镜像壳文件”的方向推进：已删除 App 侧遗留的合同镜像文件：
  - `StandardTestNext.App/ContractsBridge/TestRecordDetailContract.cs`
  - `StandardTestNext.App/ContractsBridge/TestRecordListItemContract.cs`
- 这两个文件本质上只是指向 `StandardTestNext.Contracts` 的单行 alias 壳；在共享 contracts 已集中到 `StandardTestNext.Contracts/TestRecordContracts.cs` 后，继续把它们留在 App 项目里，只会让“共享合同是否已经真正集中”这件事看起来比实际更模糊。
- 删除后重新执行 `dotnet build StandardTestNext.sln --no-restore` 复验通过，说明当前 App 侧已可直接消费共享 contracts，不再依赖这层历史镜像文件。
- 下一步不再优先制造新的占位合同文件，而是继续把 App 的真实查询消费入口从 stub 切到 Test 查询网关/真实进程边界。

## 2026-03-22 00:11 RuntimeBridge alias 收口尝试与回退
- 本小时尝试把 `StandardTestNext.Test/Application/RuntimeBridge` 下 4 个接口文件收口为对 `StandardTestNext.Contracts` 的 alias，以减少 Test 侧重复定义：
  - `IMessageBus.cs`
  - `IMessageBusConfiguration.cs`
  - `IMessagePublisher.cs`
  - `IMessageSubscriber.cs`
- 但实际执行 `dotnet build StandardTestNext.sln --no-restore` 后确认该做法会直接触发 `CS8914`（`global using` 不能置于命名空间内），并导致 Test 侧 `IMessageBus` / `IMessageBusConfiguration` 无法解析。
- 因此本轮已立即回退到原始可编译接口定义，避免把未完成的 alias 收口错误留在主干。
- 真实结论：后续若要把 RuntimeBridge 接口统一到 Contracts，不能用“文件内 namespace + global using alias”这种写法；需要改为直接替换引用命名空间、或改用普通 `using`/类型转发方案后再统一迁移。

## 2026-03-22 07:41 App 查询默认网关切换点收口
- 本小时继续按上一轮待办推进“用真实 App 查询入口替换 stub”的前置收口：`AppBootstrap` 已不再直接依赖 `new TestRecordQueryGatewayStub()`，而是统一经由 `TestRecordQueryGatewayDefaults.Create()` 解析默认网关。
- 当前默认返回值仍是 stub，这一步并没有假装“真实查询已经接通”；它的真实意义是先把切换点收成单一入口，避免后续接进程内 adapter / 跨进程网关时还要回头修改 App 主流程。
- 下一步应在这个默认工厂背后引入可配置/可替换实现（例如进程内 adapter 或跨边界查询客户端），而不是继续向 stub 本体堆更多占位细节。

## 2026-03-22 09:11 报告主次选择规则收口
- 本小时继续把 Test 查询/报告边界往前推了一小步：新增 `StandardTestNext.Test/Application/Services/TestReportSelection.cs`，统一封装 `primary report` 与 `lightweight report` 的挑选规则。
- `TestRecordQueryViewAssembler` 不再直接依赖 `reports.FirstOrDefault(x => x.IsPrimaryEntry)` 这类散落判断，而是改成显式规则：主报告优先 `IsPrimaryEntry`，再回退 `json`，再回退最新一条；轻量报告优先 `IsLightweightEntry`，再回退 `manifest`，再兜底其他报告。
- 这一步的目标不是新增功能，而是降低后续 App recent list、报告历史页、查询网关适配器在“主报告/轻量报告到底选哪条”上的重复实现成本。
- 下一步仍应优先把 App 侧默认查询网关从 stub 切成真实实现，而不是继续扩占位查询结构。
