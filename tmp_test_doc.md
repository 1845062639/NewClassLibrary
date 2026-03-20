# Test 维护文档（本地待同步草稿）

## 本次维护说明
- 时间：2026-03-21 04:42（Asia/Shanghai）
- 本轮先检查腾讯文档 Test 在线内容、next-gen 工作区未提交改动，以及上一轮待办里哪些项已经能直接落地。
- 根据“继续把映射策略独立出来”的待办，本小时没有停留在文档维护，直接继续推进了 StandardTestNext.Test 查询侧的记录映射摘要重构。
- 本轮已完成真实 `build + run` 验证，说明这次不是纸面调整，而是可运行改动。

## 代码修改记录
- 新增 `StandardTestNext.Test/Application/Services/TestRecordMappingSnapshotFactory.cs`
  - 将 query 侧由 `TestRecordItemDetail` 汇总 `samples/kp/cont` 的逻辑抽成独立工厂
  - 为后续列表页/API/报表页复用同一套统计口径做准备
- 更新 `StandardTestNext.Test/Application/Services/TestRecordQueryService.cs`
  - recent list 先构建 `itemDetails`
  - 再统一调用 `TestRecordMappingSnapshotFactory` 生成 `Mapping`
  - 不再把分区快照统计硬编码在查询服务内部
- 更新 `StandardTestNext.Test/README.md`
  - 补充 `TestRecordMappingSnapshotFactory` 的职责说明
- 本轮关联但非新增改动文件仍包括：
  - `StandardTestNext.Test/Application/Services/TestRecordMappingSnapshot.cs`
  - `StandardTestNext.Test/Application/Services/TestRecordSummary.cs`
  - `StandardTestNext.Test/Application/TestBootstrap.cs`
  - `docs/RUNTIME_CONFIGURATION.md`
- 本轮真实验证：
  - `dotnet build /root/.openclaw/workspace/next-gen/StandardTestNext.sln --no-restore` 通过，`0 Warning / 0 Error`
  - `dotnet run --project /root/.openclaw/workspace/next-gen/StandardTestNext.Test/StandardTestNext.Test.csproj --no-build` 通过
  - 运行输出已确认包含 `Recent records` 的 `samples/kp/cont` 摘要、`Record reports`、`Reloaded item details`

## Git 提交记录
- 维护本文档时，`next-gen` 本轮改动仍处于未提交状态。
- 本轮计划提交内容：query 侧映射摘要抽离 + README 更新。

## 待办事项
- 将 `TestRecordMappingSnapshotFactory` 继续上提成更明确的查询层/报告层共享服务，避免控制台输出与后续 API 端重复拼装。
- 继续把 recent list / detail list 的摘要模型收口成稳定 DTO，而不是让 Bootstrap/控制台承担展示拼装。
- 完成本轮 git 提交后，继续推进 MQTT 真实 smoke 验证或更接近协议层的连接/认证诊断。

## 下次更新时间
- 2026-03-21 05:40（Asia/Shanghai）或下次定时维护时
