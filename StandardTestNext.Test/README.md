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
- `ITestReportRenderer` + `JsonTestReportRenderer` + `MarkdownTestReportRenderer`：报告渲染抽象，phase-1 已同时支持 JSON 预览与 Markdown 报告草稿导出
- `ITestReportRepository`：报告文档/渲染结果持久化边界
- `ITestReportArtifactWriter`：报告制品写出边界
- `FileSystemTestReportArtifactWriter`：phase-1 将报告预览落到 `artifacts/reports/`
- `TestReportArtifactDescriptor`：统一描述输出文件名、格式、路径、写出时间，便于后续切到 Word/PDF
- `ITestRecordQueryService` + `TestRecordQueryService`：为记录回放/查询摘要提供独立边界，不把后续列表页、详情页逻辑直接塞回仓储或 Bootstrap
- `ITestReportQueryService` + `TestReportQueryService`：为报告导出历史摘要与按记录码回读报告快照提供独立查询边界，后续列表页/审计页/详情页可直接复用
- `TestReportSnapshot`：把报告正文回读收敛成轻量查询对象，并直接带出 artifact 文件名/路径，避免详情查询再去二次拼报告摘要
- `TestRecordItemDetail`：把 item 级详情摘要（`ItemCode / MethodCode / RecordMode / SampleCount / AttachmentCount / IsValid / Remark / HasRemark`）从原始 `DataJson` 中提炼出来，减少后续详情页/API 对 JSON 负载的直接理解成本
- `SQLiteTestPersistence` + `SQLite*Repository`：补了 SQLite 持久化样板，把产品定义、记录聚合、附件、报告正文、报告摘要落到统一 db 文件，作为后续替换内存仓储的第一步

## 当前 demo 启动链路实际覆盖
- App 先通过 `IMessageBus` 发布设备 Ready/Running 状态与实时样本；当前默认实现仍为 `InMemoryMessageBus`
- Test 再发布 `TestCommandContract`，并驱动最小试验会话启动
- Test 侧基于额定参数与实时样本生成 `TestRecordAggregate`
- 实时样本已先按 `KeyPointOnly / Continuous` 两类分区映射，并生成独立统计摘要
- 记录、产品、附件、报告正文、报告摘要都已串到最小持久化接口
- `TestBootstrap` 现已支持通过 `appsettings.test.json` 读取默认运行配置，并允许环境变量与命令行参数覆盖
- 当前优先级为：配置文件 < 环境变量 < 命令行参数
- 仍支持通过 `--persistence memory|sqlite` 或环境变量 `STNEXT_TEST_PERSISTENCE=memory|sqlite` 切换持久化模式
- `sqlite` 模式额外支持 `--sqlite-db <path>` / `STNEXT_TEST_SQLITE_DB=<path>`，以及配置文件中的 `sqliteDbPath`
- 额外支持 `--config <path>` / `--config=<path>` 指定非默认配置文件，便于后续接部署目录与多环境配置
- 当使用 `sqlite` 模式时，会自动初始化 `artifacts/test-persistence/standardtest-next.db`（或自定义路径）并走 `SQLite*Repository` 闭环
- 启动输出已覆盖 recent records / record reports / recent report summaries / record reload / reloaded item details，说明 phase-1 不再只是“能写不能查"

## 下一步优先项
- 在已接通 `appsettings.test.json` 的基础上，继续把 App/Test 两侧运行参数收敛到统一 runtime 配置模型，避免后续配置键名继续漂移
- 为报告历史与记录回放补更稳定的查询模型，而不只是控制台摘要
- 在现有 Markdown 草稿导出之上，继续抽正式报告模板渲染出口，逐步替换当前 JSON 预览
