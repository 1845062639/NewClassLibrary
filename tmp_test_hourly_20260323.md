# Test维护文档

## 本次维护说明
- 时间：2026-03-23 06:41（Asia/Shanghai）
- 本小时先复核了 Test 维护文档、next-gen 当前工作区与最近 git 状态，然后继续按待办推进 Test 侧“legacy payload 摘要 -> query contract -> 运行输出 -> App 消费”这条链路。
- 这轮没有继续横向扩新的记录聚合或仓储模型，而是把已经进入 `TestRecordItemDetailContract.LegacyPayload` 的摘要，继续接到真实运行输出和 App 查询输出里，降低后续联调时重新翻 `DataJson` 的成本。
- 已执行 `dotnet build StandardTestNext.sln --no-restore -nologo`，结果为 **0 Warning / 0 Error**。

## 代码修改记录
### 本小时新增/更新的 Test 侧真实代码
- `next-gen/StandardTestNext.Test/Application/Services/TestRecordConsolePresenter.cs`
  - 新增 `[Test] Legacy payload summary` 控制台输出。
  - 对每个 `ItemCode` 直接展示：
    - `LegacySampleCount`
    - `PowerCurveImageCount`
    - `TempCurveImageCount`
    - `VibrationCurveImageCount`
  - 让 Test 运行链路不再只展示 items/counts，而是能直接验证 legacy 摘要解析是否进入 payload reader。

- `next-gen/StandardTestNext.Test/Application/TestBootstrap.cs`
  - 扩展 `Reloaded item details` 输出。
  - 在原有 `RecordMode / SampleCount / Remark` 之外，新增打印：
    - `legacy`
    - `power`
    - `temp`
    - `vibration`
  - 继续通过 `TestRecordQueryFacade -> TestRecordQueryGatewayAdapter` 验证 query 回读路径拿到的 legacy 摘要结果。

### 本小时继续复核并沿用的上一轮关键文件
- `next-gen/StandardTestNext.Contracts/LegacyMotorRealtimeEnvelopeContract.cs`
- `next-gen/StandardTestNext.Contracts/TestRecordContracts.cs`
- `next-gen/StandardTestNext.Test/Application/Services/TestRecordLegacyPayloadSummary.cs`
- `next-gen/StandardTestNext.Test/Application/Services/TestRecordItemPayloadReader.cs`
- `next-gen/StandardTestNext.Test/Application/Services/TestRecordItemDetail.cs`
- `next-gen/StandardTestNext.Test/Application/Services/TestRecordQueryViewAssembler.cs`
- `next-gen/StandardTestNext.Test/Application/AppSide/TestRecordQueryGatewayAdapter.cs`

### 本轮阶段性结果
- Test 侧当前已经形成更完整的链路：
  - payload 解析
  - item detail/query contract
  - bootstrap/reload 输出
  - console summary
  - App 查询消费
- legacy payload 摘要不再只是 contract 层字段，而是已经进入运行输出观察面，后续联调排查时可以直接看摘要结果是否正确。

## Git 提交记录
- 本小时 **没有新增 git 提交**。
- `next-gen/` 当前最近相关提交仍为：
  - `f3e83b7 feat: expose legacy payload summaries in query contracts`
  - `cd7973d refactor: add null fallback for app query gateway`
  - `21bf4fe refactor: align test runtime bridge with shared contracts`
- 本小时新增改动目前仍保留在工作区，主要涉及：
  - `StandardTestNext.Test/Application/Services/TestRecordConsolePresenter.cs`
  - `StandardTestNext.Test/Application/TestBootstrap.cs`
  - `StandardTestNext.App/Application/AppBootstrap.cs`

## 待办事项
1. 为 `TestRecordItemPayloadReader` 与 `TestRecordQueryGatewayAdapter` 补最小回归测试，覆盖 legacy 摘要映射。
2. 继续评估是否把 legacy payload 中的曲线图片与测量指标拆成更明确的 report artifact / measurement contract，而不是长期只保留摘要计数。
3. 继续把 Test 查询桥接从 demo/bootstrap 输出推进到更稳定的应用服务出口，减少调试打印承担验证职责。
4. 在 legacy 摘要链路稳定后，再继续推进真实 App 查询消费与报告边界细化，避免再次回到“只扩 contract、不接消费者”的节奏。

## 下次更新时间
- 预计下次更新时间：2026-03-23 07:41（Asia/Shanghai）
