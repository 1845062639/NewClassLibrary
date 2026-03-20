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
- Phase-1 决策：`RatedParamsJson`、`DataJson` 继续保留 JSON 负载，先稳定新边界，再逐步结构化
- 报告输出抽象已补最小接口：`ITestReportRenderer` + `JsonTestReportRenderer`，并新增 `TestReportDocumentMapper` 先把记录聚合收敛为独立报告文档模型，当前可从该文档模型导出 JSON 预览
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
- 本小时继续把这层护栏从“控制台提示”推进到“启动前失败”：`RuntimeConfigurationValidationResult` 已补 `Errors`，App/Test 双端 `Program.cs` 会在报告配置摘要后调用 `ThrowIfInvalid`；当前未实现的 `mqtt` provider、非法端口、空 `clientId` / `topicPrefix`、非法 `samplingMode` / `persistenceMode` 会直接阻断启动，先把最容易误配的假绿路径封住
- 同步修正双端 `Program.cs` 启动边界：App/Test 入口各自只加载自身配置并启动自身 Bootstrap，不再互相串拉对方 Bootstrap/配置，避免后续接 MQTT/部署脚本时继续把 demo 串线状态带进主干
- 本小时继续把消息总线配置入口从“配置文件 + 环境变量”推进到“配置文件 + 环境变量 + CLI”：`AppStartupOptionsParser` / `TestStartupOptionsParser` 已支持 `--message-bus`、`--message-bus-host`、`--message-bus-port`、`--message-bus-client-id`、`--message-bus-topic-prefix`、`--message-bus-username`、`--message-bus-password`，后续切 MQTT/provider 调试时不用再依赖改部署目录配置文件
- 本小时顺手实跑校验了这条链路，并暴露出一个真实边界：validator 把 `mqtt` 当作合法 provider，但 `MessageBusFactory` 仍只实现 `inmemory`，此前会出现“配置校验无警告、运行直接抛 `NotSupportedException`”的假绿状态；现已把 App/Test 双端启动告警改为明确提示“mqtt 尚未实现，当前切过去会失败”
- 本小时继续顺手清理共享基础设施技术债：`IMessageBus` 已不再继承 obsolete 的 `IMessageSubscriber` 兼容接口，`Subscribe<T>` 直接并入主总线接口；复验 `dotnet build StandardTestNext.sln --no-restore`、`dotnet run` App/Test 两端均通过，当前主干恢复为 0 warning / 0 error
- 下一步优先补：在已补连接参数配置骨架、细粒度环境变量入口、公共配置说明、CLI 覆盖入口、已纠正的最小配置告警以及已清理的总线兼容层前提下接入 MQTT 实现、把当前“控制台告警”升级为更严格的非法配置失败策略、正式报告模板渲染、在已落地的 SQLite 样板基础上细化表结构/查询模型并评估是否继续引入 EF 或 Dapper、样本映射策略与试验方法编码的对应表
- 本小时继续把记录查询边界从“回读聚合”推进到“回读聚合 + 附件明细”：`IRecordAttachmentRepository` 已补 record/item 两级附件查询接口，`ITestRecordQueryService` 已返回 `TestRecordDetail` 组合结果，为后续记录详情页、报告附件清单、审计查询预留稳定边界
- 本小时继续把“报告摘要/导出制品引用并入查询对象”往前推：`ITestReportRepository` / `ITestReportQueryService` 已补按 `RecordCode` 回读 `TestReportSnapshot`，`TestRecordDetail` 已并入 report snapshots / report summaries，当前记录详情查询不再只能看到聚合与附件，也能看到同记录下的报告正文快照与摘要元信息
- 本小时继续把 item 级统计显式化：新增 `TestRecordItemDetail`，并将 `ItemCode / MethodCode / RecordMode / SampleCount / AttachmentCount` 收敛到 `TestRecordDetail.ItemDetails`，减少后续详情页/API 对 `DataJson` 的直接理解成本
- 本小时继续把导出制品引用真正并入记录详情查询：`TestReportSnapshot` 已直接携带 `ArtifactFileName / ArtifactSavedPath`，`TestRecordDetail.Reports` 不再只有正文快照，后续详情页可直接落报告文件链接/路径展示

## 阶段 4：旧系统并行期
- 新旧系统并行验证
- 逐步把新增能力放到新系统
- 旧系统仅维护关键稳定性

## 当前决策
- 重点转向新项目，不再以大规模修正旧 App 为主要路径
- 旧项目继续作为领域参考和迁移素材
