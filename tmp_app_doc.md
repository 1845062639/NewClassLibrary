App维护文档

本次维护说明
维护时间：2026-03-23 08:41 Asia/Shanghai
本小时先复核了 App/Test 两侧维护文档、next-gen 当前工作区与最近构建状态，然后继续按待办直接推进真实可落地的开发工作，而不是只停留在文档同步。当前 App 侧主推进仍放在 next-gen/ 主干，没有继续扩散旧 app-repos/StandardTestApp 历史项目改动。

本小时 App 侧的真实进展主要有两点：
1. 继续把 legacy payload 摘要输出从散落字符串拼接收口为共享 formatter，避免 App/Test 两侧展示口径继续漂移。
2. 为 legacy payload 解析与查询桥接补最小 smoke tests，并完成 build/run 复验，确保这条链路不再只靠人工盯控制台输出。

代码修改记录
本小时 App 侧与共享查询/展示边界直接相关的真实代码变更：
- next-gen/StandardTestNext.Contracts/TestRecordLegacyPayloadFormatter.cs
  - 新增共享 formatter，统一 recent list / detail 的 legacy payload 摘要输出口径。
- next-gen/StandardTestNext.App/Application/AppBootstrap.cs
  - list legacy payload summary 改为走 TestRecordLegacyPayloadFormatter.FormatListSummary。
  - detail legacy payload summary 改为走 TestRecordLegacyPayloadFormatter.FormatDetailSummary。
  - App 继续直接消费共享 contract 中的 LegacyPayload 字段，但不再自己维护重复的摘要拼接逻辑。
- next-gen/StandardTestNext.Test/Application/Services/TestRecordLegacyPayloadReaderSmokeTests.cs
  - 新增最小 smoke tests，覆盖 legacy payload 解析与摘要格式化的基本场景。
- next-gen/StandardTestNext.Test/Application/Services/TestRecordQueryGatewayAdapterSmokeTests.cs
  - 新增最小 smoke tests，覆盖 query gateway adapter 对 legacy payload 摘要的 detail/list 映射。
- next-gen/StandardTestNext.Test/Program.cs
  - 启动时先跑 smoke tests，再进入原有 TestBootstrap。

本小时继续确认有效、但未新增旧 App 历史仓库修改的边界参考文件包括：
- app-repos/StandardTestApp/ALS_20220330/AppMotorRatedParamsDto.cs
- app-repos/StandardTestApp/ALS_20220330/AppTestContractTopics.cs
- app-repos/StandardTestApp/QDHX_20220310/AppMotorRatedParamsDto.cs
- app-repos/StandardTestApp/QDHX_20220310/AppTestContractTopics.cs
- app-repos/StandardTestApp/JSFC_20220616/AppMotorRatedParamsDto.cs
- app-repos/StandardTestApp/SINEN_TEST_11KW/AppMotorRatedParamsDto.cs
这些旧 App 改动本小时没有继续扩散，仍主要作为“轻量 DTO + topic 常量”迁移样板保留。

Git 提交记录
本小时没有新增 git 提交。
当前 next-gen/ 最近相关提交仍为：
- f3e83b7 feat: expose legacy payload summaries in query contracts
- cd7973d refactor: add null fallback for app query gateway
- 21bf4fe refactor: align test runtime bridge with shared contracts
- a954cac refactor: unify test record contracts and partition pipeline
- a9b8448 refactor: decouple app seed gateway wiring

本小时新增改动仍处于 next-gen/ 工作区未提交状态。

待办事项
- 继续把当前 legacy payload 摘要从 bootstrap/控制台打印收口为更稳定的 App 查询展示装配层，而不是长期停留在启动输出。
- 继续把 App 默认查询入口从 seeded in-proc 方案推进到更稳定的可配置策略（in-proc / sqlite-backed / remote / null fallback）。
- 评估是否把 legacy 曲线图、进线电参、绕组温度这些摘要进一步拆成更明确的 measurement / artifact contract，而不是长期只保留摘要计数。
- 在 smoke tests 基础上继续补更贴近 query adapter 与 App 消费侧的最小回归验证，减少未来 contract 漂移时只能靠人工发现。

下次更新时间
2026-03-23 09:41（Asia/Shanghai）