# App维护文档

## 本次维护说明
- 时间：2026-03-23 06:41（Asia/Shanghai）
- 本小时先复核了 App/Test 两侧维护文档、next-gen 当前工作区与最近 git 状态，然后继续按文档待办推进真实代码。
- App 侧这小时没有再回头扩旧 `app-repos/StandardTestApp` 的历史现场项目改造，主推进仍放在 `next-gen/` 新主干。
- 本小时直接落地的 App 侧开发，是把 legacy payload 摘要真正接到 App 查询输出里，让 App 不只看到 recent list / detail 的 items、samples、reports 汇总，还能直接看到 legacy sample 数、功率/温升/振动曲线摘要以及 incoming/winding 指标存在性。
- 已执行 `dotnet build StandardTestNext.sln --no-restore -nologo`，结果为 **0 Warning / 0 Error**，说明本轮 App/Test/Contracts 三项目主干仍保持可构建。

## 代码修改记录
### 本小时新增/更新的 App 侧真实代码
- `next-gen/StandardTestNext.App/Application/AppBootstrap.cs`
  - recent list 预览新增 `[App] List legacy payload summary` 输出。
  - detail 预览新增 `[App] Detail legacy payload summary` 输出。
  - 直接消费 `TestRecordItemDetailContract.LegacyPayload` 中的：
    - `LegacySampleCount`
    - `PowerCurveImageCount`
    - `TempCurveImageCount`
    - `VibrationCurveImageCount`
    - `HasIncomingPowerMetrics`
    - `HasWindingTemperatureMetrics`
  - 当前 App 默认查询路径已经不只消费 `RecordCode / ProductDisplayName / ReportCount` 这类薄摘要，而是开始真实消费 legacy payload 摘要字段。

### 本小时联动复核到、但不是本轮新起手编写的相关边界文件
- `next-gen/StandardTestNext.Contracts/TestRecordContracts.cs`
- `next-gen/StandardTestNext.Contracts/LegacyMotorRealtimeEnvelopeContract.cs`
- `next-gen/StandardTestNext.Test/Application/AppSide/TestRecordQueryGatewayAdapter.cs`
- `next-gen/StandardTestNext.Test/Application/AppSide/InProcAppQueryGatewaySeedFactory.cs`
- `next-gen/StandardTestNext.App/Program.cs`

### 旧 App 迁移样板现状（本小时复核，无新增扩散）
以下旧 App 现场项目中的“轻量 DTO + topic 常量”收口仍保留为迁移样板，但本小时未继续扩大修改面：
- `app-repos/StandardTestApp/ALS_20220330/AppMotorRatedParamsDto.cs`
- `app-repos/StandardTestApp/ALS_20220330/AppTestContractTopics.cs`
- `app-repos/StandardTestApp/QDHX_20220310/AppMotorRatedParamsDto.cs`
- `app-repos/StandardTestApp/QDHX_20220310/AppTestContractTopics.cs`
- `app-repos/StandardTestApp/JSFC_20220616/AppMotorRatedParamsDto.cs`
- `app-repos/StandardTestApp/SINEN_TEST_11KW/AppMotorRatedParamsDto.cs`

## Git 提交记录
- 本小时 **没有新增 git 提交**。
- `next-gen/` 当前最近相关提交仍为：
  - `f3e83b7 feat: expose legacy payload summaries in query contracts`
  - `cd7973d refactor: add null fallback for app query gateway`
  - `21bf4fe refactor: align test runtime bridge with shared contracts`
- 本小时新增改动目前仍保留在工作区，主要涉及：
  - `StandardTestNext.App/Application/AppBootstrap.cs`
  - `StandardTestNext.Test/Application/Services/TestRecordConsolePresenter.cs`
  - `StandardTestNext.Test/Application/TestBootstrap.cs`

## 待办事项
1. 继续把 App 查询输出从“调试打印”推进到更稳定的应用服务/展示装配层，减少 `AppBootstrap` 承担展示拼接职责。
2. 继续把默认查询入口从 seeded in-proc 方案推进到更稳定的可配置策略（例如 in-proc / sqlite-backed / remote / null fallback）。
3. 对照旧 App 现场工程（ALS / QDHX / JSFC / SINEN），确认哪些 legacy 指标需要在 App 查询侧长期保留，哪些只保留在 Test/报告侧。
4. 在查询展示边界再稳定一轮后，整理本轮 legacy payload 消费推进与前序 query bridge 收口，准备形成干净 git 提交。

## 下次更新时间
- 预计下次更新时间：2026-03-23 07:41（Asia/Shanghai）
