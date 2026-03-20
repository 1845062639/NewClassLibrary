# App 维护文档（本地待同步草稿）

## 本次维护说明
- 时间：2026-03-21 04:42（Asia/Shanghai）
- 本轮先复核 App 在线维护文档、旧 App/新主干的分工边界，以及当前 next-gen 工作区的真实改动。
- App 侧本小时没有继续扩大旧 App 历史项目改动范围，仍坚持把旧 App 当参考样本，把主推进重心放在 next-gen 新主干。
- 虽然本轮直接编码主要发生在 Test 侧，但它会直接影响 App/Test 后续共用的记录摘要口径，因此已纳入 App 侧维护说明。

## 代码修改记录
- App 侧本小时未新增旧 App 仓库源码修改。
- 关联 App/Test 协作边界的真实变更主要在 next-gen：
  - `StandardTestNext.Test/Application/Services/TestRecordMappingSnapshotFactory.cs`
  - `StandardTestNext.Test/Application/Services/TestRecordQueryService.cs`
  - `StandardTestNext.Test/README.md`
- 本轮把记录样本分区快照统计从查询服务内部抽成独立工厂，后续 App 侧如果接列表页/详情页/API，可直接消费统一的 `samples/kp/cont` 统计口径，减少 UI 层自行拼装。
- 本轮真实复验：
  - `dotnet build /root/.openclaw/workspace/next-gen/StandardTestNext.sln --no-restore` 通过，`0 Warning / 0 Error`
  - `dotnet run --project /root/.openclaw/workspace/next-gen/StandardTestNext.Test/StandardTestNext.Test.csproj --no-build` 通过，说明本轮重构没有把双端共享主干带回归

## Git 提交记录
- App 仓库本小时没有新增 git 提交。
- next-gen 在本文档更新时，本轮改动仍处于待提交状态。

## 待办事项
- 继续把旧 App 中设备网关、实时采样、控制命令、状态上报四类职责提炼成新 App 可复用边界。
- 根据 Test 侧已经稳定下来的 `samples/kp/cont` 摘要口径，评估 App 侧列表/详情/宿主界面应消费哪些查询 DTO。
- 等本轮 next-gen 提交完成后，再补 App/Test 共享查询 DTO 或 API 契约草案。

## 下次更新时间
- 2026-03-21 05:40（Asia/Shanghai）或下次定时维护时
