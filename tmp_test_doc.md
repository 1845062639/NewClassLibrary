Test维护文档

本次维护说明
维护时间：2026-03-23 08:41 Asia/Shanghai
本小时先复核了 Test 维护文档、next-gen 当前工作区与最近 build/run 状态，然后继续按待办直接推进 legacy payload 查询链路，而不是停留在文档同步。本轮真实推进重点不是再扩新的记录 DTO，而是把 legacy payload 摘要从“已经能查到”继续收口为“共享格式化口径 + 最小 smoke tests + 运行链路可见 + query adapter 可回归验证”。

代码修改记录
本小时 Test 侧与 legacy payload 主线直接相关的真实代码变更：
- next-gen/StandardTestNext.Contracts/TestRecordLegacyPayloadFormatter.cs
  - 新增共享 formatter，统一 recent list / detail 摘要字符串口径，供 App/Test 两侧复用。
- next-gen/StandardTestNext.Test/Application/Services/TestRecordLegacyPayloadReaderSmokeTests.cs
  - 新增最小 smoke tests。
  - 覆盖 legacy payload JSON 解析：LegacySampleCount、PowerCurveImageCount、TempCurveImageCount、VibrationCurveImageCount、HasIncomingPowerMetrics、HasWindingTemperatureMetrics。
  - 覆盖 legacy payload 摘要格式化输出，避免后续口径漂移时只能靠人工对控制台。
- next-gen/StandardTestNext.Test/Application/Services/TestRecordQueryGatewayAdapterSmokeTests.cs
  - 新增 query gateway adapter smoke tests。
  - 覆盖 detail/list 路径对 LegacyPayload 摘要字段的映射与格式化结果。
- next-gen/StandardTestNext.Test/Program.cs
  - 在进入 TestBootstrap 前先执行 smoke tests。
- next-gen/StandardTestNext.Test/Application/Services/TestRecordConsolePresenter.cs
  - legacy payload summary 保持可见，并继续输出 incoming / winding 标志位，便于与共享 formatter / App 输出对照。
- next-gen/StandardTestNext.Test/Application/TestBootstrap.cs
  - 保持 Reloaded item details 的 legacy / power / temp / vibration 输出，继续验证 query 回读链路。

本轮 Test 侧阶段性结果：
- legacy payload 解析链路仍保持：payload reader -> item detail/query contract -> bootstrap/reload 输出 -> console summary。
- 共享 formatter 已下沉到 Contracts，减少 App/Test 两侧继续各自拼接 legacy 摘要字符串。
- smoke tests 已开始覆盖 legacy payload 基本解析与 query adapter 摘要映射，后续继续补更靠近真实消费路径的回归测试时有了最小支点。

本小时真实验证：
- dotnet build next-gen/StandardTestNext.sln --no-restore -nologo ✅
- dotnet run --project next-gen/StandardTestNext.Test/StandardTestNext.Test.csproj --no-build -nologo ✅
当前结果：0 Warning / 0 Error。

Git 提交记录
本小时没有新增 git 提交。
当前 next-gen/ 最近相关提交仍为：
- f3e83b7 feat: expose legacy payload summaries in query contracts
- cd7973d refactor: add null fallback for app query gateway
- 21bf4fe refactor: align test runtime bridge with shared contracts
- a954cac refactor: unify test record contracts and partition pipeline
- a9b8448 refactor: decouple app seed gateway wiring

本小时新增改动仍处于工作区未提交状态。

待办事项
- 继续为 TestRecordQueryGatewayAdapter 增加更贴近 query contract 的最小回归测试，不只停留在 payload reader smoke tests。
- 继续把 legacy 曲线图片、进线电参、绕组温度指标评估为更明确的 measurement / report artifact contract，而不是长期只保留摘要计数。
- 继续收口 bootstrap / console 层的 legacy 摘要演示，把稳定输出迁到更明确的 presenter / application service 边界。
- 在 legacy 摘要链路稳定后，再继续推进真实 App 查询消费与报告边界细化。

下次更新时间
2026-03-23 09:41（Asia/Shanghai）