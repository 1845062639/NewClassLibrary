# StandardTest Next 架构草案

## 1. 系统拆分

### StandardTestNext.App
职责：
- 设备连接与驱动适配
- 采集任务执行
- 控制流程编排
- 现场特化配置
- 向 Test 发送结构化采集结果与状态事件

### StandardTestNext.Test
职责：
- 产品/试验模型管理
- 试验任务编排与状态持久化
- 数据解析、校验、计算
- 报告生成
- 附件与记录管理
- 权限与组织能力
- 将记录聚合映射为报告文档模型，再交给可替换渲染器输出

### StandardTestNext.Contracts
职责：
- DTO
- Topic/Event 名称
- 命令/响应模型
- 边界枚举
- 版本策略

## 2. 核心边界

### App 不负责
- 报告结构
- 长期存储模型
- Test 内部算法类
- 试验记录数据库模型

### Test 不负责
- 具体设备驱动细节
- 现场硬件差异补丁
- 现场联机控制界面实现

## 3. 第一批共享契约
- MotorRatedParamsContract
- MotorRealtimeSampleContract
- TestCommandContract
- DeviceStatusContract

## 4. 协议原则
- 禁止以 CLR 类型名作为 MQTT/消息 topic
- topic 必须显式命名并版本化
- 建议形式：`stnext/{domain}/{message}/v1`
- App/Test 运行入口统一依赖 `IMessageBus`，发布/订阅不再在 Bootstrap 层拆成两套注入接口；后续补 MQTT provider 时只扩 `MessageBusFactory` 与 provider 配置
- App/Test 均使用各自的 `appsettings.app.json` / `appsettings.test.json` 维护运行参数，统一遵循“配置文件 < 环境变量 < 命令行参数”；共享键名优先保持同构（如 `messageBus.*`），避免双端部署时再互相借配置
- 双端 `Program.cs` 当前已收口为“各自只启动自身 Bootstrap”，不再互相串拉另一侧运行链路；后续如果需要端到端 demo，应另建集成宿主而不是继续污染单端入口
- 运行配置的正式约定已收敛到 `docs/RUNTIME_CONFIGURATION.md`，README/部署脚本/样例配置应以该文件为准，避免 README 各写各的后续漂移
- 当前已补最小配置校验与启动摘要输出，但仍属于轻量护栏；后续接入 MQTT 与正式部署脚本前，应把非法配置从“警告”提升为更明确的失败策略

## 5. 迁移方式
- 从旧系统抽取规则，不直接复制其耦合结构
- 旧 App 项目只作为设备流程参考样本
- 新项目优先建立统一主干，再按产品线扩展
