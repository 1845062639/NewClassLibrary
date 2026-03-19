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
- RealtimeSampleContract（待建）
- TestCommandContract（待建）
- DeviceStatusContract（待建）

## 4. 协议原则
- 禁止以 CLR 类型名作为 MQTT/消息 topic
- topic 必须显式命名并版本化
- 建议形式：`stnext/{domain}/{message}/v1`

## 5. 迁移方式
- 从旧系统抽取规则，不直接复制其耦合结构
- 旧 App 项目只作为设备流程参考样本
- 新项目优先建立统一主干，再按产品线扩展
