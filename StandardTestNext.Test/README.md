# StandardTestNext.Test

新一代试验业务与数据处理端。

## 第一版目标
- 管理试验任务与记录中间态
- 接收 App 上传的实时样本
- 完成解析、校验、计算与报告输出接口抽象

## 后续建议结构
- `Domain/`：核心领域模型
- `Application/`：用例服务
- `Infrastructure/`：存储、消息、文件
- `Reports/`：报表接口与实现

## 当前已落地的最小报告导出闭环
- `TestRecordItemMapper` + `TestRecordItemMappingResult`：将实时样本映射结果收敛成"记录分项 + 分区摘要"，不再把分桶规则散落在 Builder/Bootstrap
- `TestRecordStatistics` + `TestRecordBuildResult`：把记录构建结果中的统计摘要单独建模，供控制台输出、报告文档、后续查询模型复用
- `TestReportDocumentMapper`：记录聚合 -> 独立报告文档模型
- `ITestReportRenderer` + `JsonTestReportRenderer` + `MarkdownTestReportRenderer` + `ManifestTestReportRenderer`：报告渲染抽象，phase-1 已同时支持 JSON 预览、Markdown 报告草稿以及更轻量的 manifest 概览导出；当前 `TestBootstrap` 已把三种格式都真正接入导出与摘要持久化链路，不再只是接口存在
- `ITestReportRepository`：报告文档/渲染结果持久化边界
- `ITestReportArtifactWriter`：报告制品写出边界
- `FileSystemTestReportArtifactWriter`：phase-1 将报告预览落到 `artifacts/reports/`
- `TestReportArtifactDescriptor`：统一描述输出文件名、格式、路径、写出时间，便于后续切到 Word/PDF
- `ITestRecordQueryService` + `TestRecordQueryService`：为记录回放/查询摘要提供独立边界，不把后续列表页、详情页逻辑直接塞回仓储或 Bootstrap；本小时 recent list 视图已补 `ReportCount / HasReportArtifacts / LatestReportSavedAt / ProductCode / ProductModel / ReusedProductDefinition`，减少列表页再二次拼报告状态或产品主数据状态
- `TestRecordQueryFacade`：把 `TestRecordListView / TestRecordDetailView` 进一步收口成面向 App/API 的轻量查询出口，先提供 `ListRecentForApp / GetDetailForApp`，避免后续消费层直接混用 detail domain/query summary 两套模型
- `TestRecordMappingSnapshotFactory`：把 query 侧由 `TestRecordItemDetail` 汇总 `samples/kp/cont` 的快照逻辑单独抽出，避免分区摘要统计继续散落在查询服务内部，方便后续列表页/API/报表页复用同一套口径
- `TestRecordQueryViewAssembler`：继续把 recent/detail 两条查询路径共用的 DTO 组装逻辑收口到一处，统一回填 `ItemDetails / Mapping / ReportSummaries`，避免查询服务再次沦为拼装脚本
- `TestRecordItemPayloadReader`：把 `DataJson -> SampleCount / RecordMode` 的解析逻辑收口成独立读取器，后续 API/报告层如果也需要读取 item payload，不必再各自重写 JSON 解析
- `ITestReportQueryService` + `TestReportQueryService`：为报告导出历史摘要与按记录码回读报告快照提供独立查询边界，后续列表页/审计页/详情页可直接复用
- `TestReportSnapshot`：把报告正文回读收敛成轻量查询对象，并直接带出 artifact 文件名/路径，避免详情查询再去二次拼报告摘要
- `TestReportManifest` + `TestReportManifestMapper`：把“记录聚合/报告文档 -> 轻量报告概览模型”独立出来，先稳定后续正式报告索引页、导出目录页、审计摘要页可复用的边界，而不是让这些场景直接依赖完整报告正文
- `TestRecordItemDetail`：把 item 级详情摘要（`ItemCode / MethodCode / RecordMode / SampleCount / AttachmentCount / IsValid / Remark / HasRemark`）从原始 `DataJson` 中提炼出来，减少后续详情页/API 对 JSON 负载的直接理解成本
- `SQLiteTestPersistence` + `SQLite*Repository`：补了 SQLite 持久化样板，把产品定义、记录聚合、附件、报告正文、报告摘要落到统一 db 文件，作为后续替换内存仓储的第一步
- `ITestProductDefinitionService` + `TestProductDefinitionService`：把“按 productKind 复用已有产品定义；额定参数变化时更新快照”的逻辑从 Bootstrap 中抽出，减少后续记录构建对初始化样板代码的依赖
- `IProductDefinitionQueryService` + `ProductDefinitionQueryService`：补产品定义查询边界，先提供 `GetByKind` / `ListRecent`，避免后续列表页、主数据对账、记录详情又把产品定义读取逻辑塞回 Bootstrap 或直接扫仓储；当前 demo 启动链路也已实际接入 recent products / by-kind 回读，不再只是接口静态存在

## 当前 demo 启动链路实际覆盖
- App 先通过 `IMessageBus` 发布设备 Ready/Running 状态与实时样本；当前默认实现仍为 `InMemoryMessageBus`
- Test 再发布 `TestCommandContract`，并驱动最小试验会话启动
- Test 侧先通过 `TestProductDefinitionService` 按 `productKind` 解析/复用产品定义，再基于额定参数与实时样本生成 `TestRecordAggregate`
- 实时样本已先按 `KeyPointOnly / Continuous` 两类分区映射，并生成独立统计摘要
- 记录、产品、附件、报告正文、报告摘要都已串到最小持久化接口
- `TestBootstrap` 现已支持通过 `appsettings.test.json` 读取默认运行配置，并允许环境变量与命令行参数覆盖
- 当前优先级为：配置文件 < 环境变量 < 命令行参数
- 当前已支持配置项：`persistenceMode`、`sqliteDbPath`、`messageBus.provider|host|port|clientId|topicPrefix|username|password`
- 当前已支持环境变量：`STNEXT_TEST_PERSISTENCE`、`STNEXT_TEST_SQLITE_DB`、`STNEXT_MESSAGE_BUS`、`STNEXT_MESSAGE_BUS_HOST`、`STNEXT_MESSAGE_BUS_PORT`、`STNEXT_MESSAGE_BUS_CLIENT_ID`、`STNEXT_MESSAGE_BUS_TOPIC_PREFIX`、`STNEXT_MESSAGE_BUS_USERNAME`、`STNEXT_MESSAGE_BUS_PASSWORD`、`STNEXT_MESSAGEBUS_PUBLISH_TIMEOUT_SECONDS`、`STNEXT_MESSAGEBUS_SUBSCRIBE_TIMEOUT_SECONDS`
- 当前已支持命令行参数：`--config`、`--persistence`、`--sqlite-db`、`--message-bus`、`--message-bus-host`、`--message-bus-port`、`--message-bus-client-id`、`--message-bus-topic-prefix`、`--message-bus-username`、`--message-bus-password`
- 仍支持通过 `--persistence memory|sqlite` 或环境变量 `STNEXT_TEST_PERSISTENCE=memory|sqlite` 切换持久化模式
- `sqlite` 模式额外支持 `--sqlite-db <path>` / `STNEXT_TEST_SQLITE_DB=<path>`，以及配置文件中的 `sqliteDbPath`
- `messageBus` 配置节已与 App 侧对齐，Test 入口不再借道 `appsettings.app.json` 读取总线配置，后续切 MQTT/其他 provider 时可分别在双端部署目录落各自配置文件
- 默认配置文件仍放在程序输出目录，也支持通过 `--config <path>` / `--config=<path>` 指定部署目录中的替代配置
- Test 入口 `Program.cs` 已修正为只启动 Test 自身，不再误拉 App 侧 Bootstrap/配置；消息总线配置改为直接从 `TestStartupOptions.MessageBus` 透传到 `MessageBusFactory`
- 已补 `RuntimeConfigurationValidator` + `RuntimeConfigurationConsoleReporter`，启动时会打印 persistence/messageBus 配置摘要，并对 provider、port、clientId、topicPrefix、persistenceMode、sqliteDbPath 做校验；当前已将明显非法配置（如不支持的 provider、空 `clientId` / `topicPrefix`、非法端口、非法 `persistenceMode`）从“仅告警”推进为启动前直接失败，避免配置看起来合法却在运行期再炸
- 当 `messageBus.provider=mqtt` 时，启动前会额外检查 `messageBus.host` 非空，并对 `host:port` 做一次轻量 TCP reachability probe；当显式指定 `sqliteDbPath` 且 `persistenceMode=sqlite` 时，还会预先探测目录可创建/可写，尽量把部署期权限问题前移到启动前
- 当使用 `sqlite` 模式时，会自动初始化 `artifacts/test-persistence/standardtest-next.db`（或自定义路径）并走 `SQLite*Repository` 闭环
- 启动输出已覆盖 recent records / record reports / recent report summaries / record reload / reloaded item details，说明 phase-1 不再只是“能写不能查"

## 本小时进展补充
- 本轮继续推进“用真实 App 查询入口替换 stub”：App 默认启动链路当前通过 `InProcAppQueryGatewayFactory -> InProcAppQueryGatewaySeedFactory` 反射装配 seeded in-proc query gateway，默认 recent/detail/report summaries 预览已走 `TestRecordQueryService -> TestRecordQueryFacade -> TestRecordQueryGatewayAdapter` 的真实查询链，而不是空仓储 adapter 或纯 stub。
- 新增 `StandardTestNext.Contracts/TestRecordQuerySeedContracts.cs`，把默认 seeded rated params + realtime samples 收成共享种子契约，避免 App 默认入口、Test demo、后续 smoke host 再散落多份演示样本。
- 当前 App 已去掉对 `StandardTestNext.Test` 的编译期项目引用；这次先接受“运行期反射装配 Test 查询实现”的阶段性 in-proc 边界，目的是先让默认查询主路径吃到真实数据，再逐步演进到可配置 remote/sqlite-backed gateway。
- `TestReportSelection` 已把 primary/lightweight 报告选择收成显式规则，`TestRecordQueryViewAssembler` 不再直接依赖 `FirstOrDefault` 或仓储偶然顺序挑选 recent/detail 中的主报告与轻量报告。
- 已复验 `dotnet build StandardTestNext.sln --no-restore` 通过，主干仍保持 `0 warning / 0 error`。

## 下一步优先项
- App/Test 双端统一配置约定已整理到 `docs/RUNTIME_CONFIGURATION.md`，且消息总线连接参数已补齐 CLI 覆盖入口；本轮继续把 `provider=mqtt` 的启动前自检从单一可达性 warning 推进到结构化状态诊断，能区分 `reachable / timeout / connection-refused / dns-failed / auth-failed / probe-failed`；`dotnet build StandardTestNext.sln --no-restore` 当前仍是 0 warning / 0 error，下一步重点转为真实 MQTT smoke 验证与更接近协议层的认证/权限级诊断，而不是继续口头维护键名约定
- 本小时继续把共享总线诊断补前置：Test 启动摘要已输出 `publishTimeoutSeconds` / `subscribeTimeoutSeconds`，配置非法时会在启动前直接失败，后续做真实 MQTT 联调时更容易定位“配置问题”还是“broker 问题”
- 新增 `scripts/run-mqtt-smoke.sh`：在本机已有 MQTT broker 的前提下，可一键拉起 App/Test 双进程 smoke run，默认把 Test 侧落到 SQLite 持久化并输出双端日志，方便验证跨进程消息链路而不必手工敲两条长命令。
- 已通过 `StandardTestNext.Contracts/TestRecordContracts.cs` 将 `TestRecordDetailContract / TestRecordListItemContract / TestRecordItemDetailContract / TestReportSummaryContract` 收口到共享契约文件，当前 `TestRecordQueryGatewayAdapter` 已能把 Test 侧 `ItemDetails / ReportSummaries` 完整投影给 App 侧消费者
- 下一步继续把这条链路从“共享契约 + adapter 完整投影”推进到“真实 App 查询消费”，逐步替换当前 stub/pending bridge 占位实现；本轮 App 侧已先接上 `GetDetailAsync(recordCode)` 的最小摘要消费，后续优先把这条调用切到真实 Test 查询网关而非继续停留在 stub
- 在现有 Markdown 草稿导出之上，继续抽正式报告模板渲染出口，逐步替换当前 JSON 预览
