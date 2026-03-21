## 本次维护说明
- 时间：2026-03-21 14:48（Asia/Shanghai）
- 本轮先尝试检查并更新腾讯文档 App/Test 在线维护文档，但腾讯文档 MCP 继续返回 `400007 access limit`，本小时仍无法在线写回，因此先把维护内容落到本地草稿，待额度恢复后直接同步。
- 这轮继续按待办推进，把“查询 view 已适合 App/API 消费”的口头状态，进一步收口成真实代码边界：新增 `TestRecordQueryFacade`，显式提供 `ListRecentForApp / GetDetailForApp`。
- 目的不是再造一层空壳，而是把 App/API 未来真正要消费的轻量查询出口从 `ITestRecordQueryService` 的通用查询职责中剥出来，减少后续 UI/宿主层直接耦合 summary/detail/domain 细节。

## 代码修改记录
- 本小时真实新增/修改文件：
  - 新增 `next-gen/StandardTestNext.Test/Application/Services/TestRecordQueryFacade.cs`
  - 更新 `next-gen/StandardTestNext.Test/Application/TestBootstrap.cs`
  - 更新 `next-gen/StandardTestNext.Test/README.md`
- 具体变更：
  - `TestRecordQueryFacade` 封装 `ITestRecordQueryService`，统一暴露 `ListRecentForAppAsync` 与 `GetDetailForAppAsync`。
  - `TestBootstrap` 改为通过 facade 获取 `recentRecordViews` 与 `reloadedRecordView`，让 demo 启动链路开始真实消费这层面向 App/API 的轻量查询出口。
  - `README.md` 已同步记录 facade 的定位，保证仓库文档与当前代码边界一致。
- 本轮真实验证：
  - `dotnet build /root/.openclaw/workspace/next-gen/StandardTestNext.sln --no-restore` → 通过
  - `dotnet run --project /root/.openclaw/workspace/next-gen/StandardTestNext.Test/StandardTestNext.Test.csproj --no-build` → 通过
- 当前运行链路仍持续输出：
  - `Recent record views`
  - `Reloaded record view`
  - `Primary record report`
  - `Lightweight report artifact`
  说明 facade 接入后未破坏既有查询/导出闭环。

## Git 提交记录
- 本轮准备整理为 git 提交：`feat: add app-facing test record query facade`
- 若提交成功，应同步把提交号回填到下轮在线文档。

## 待办事项
- 等腾讯文档额度恢复后，把本地草稿同步回在线 App/Test 维护文档。
- 继续评估是否把 facade 再扩成更明确的宿主 API/query contract（例如独立 endpoint DTO），让 App 侧完全不接触底层 query service。
- 在 App 侧真正落一层消费代码，验证 `PrimaryReport / LightweightReport` 显式字段与 facade 能否覆盖列表页/详情页最小需求。

## 下次更新时间
- 2026-03-21 15:45（Asia/Shanghai）或下次定时维护时
