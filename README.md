# StandardTest Next

这是基于旧 StandardTest 体系经验重新规划的一代新项目，而不是在旧 App 集合上做持续修补。

## 目标
- 明确分离 App / Test / Contracts
- 用稳定协议替代 `typeof(...).Name` 式耦合
- 允许 App 独立演进设备接入与现场控制
- 允许 Test 独立演进业务、记录、报表、存储

## 目录
- `StandardTestNext.App`：新一代现场采集与控制端
- `StandardTestNext.Test`：新一代试验业务与数据处理端
- `StandardTestNext.Contracts`：两端唯一共享契约
- `docs/`：设计文档、迁移计划、领域分析

## 原则
1. 旧系统是参考，不是包袱
2. 旧 App 不做全面修复性重构
3. 新功能尽量优先落到新项目主干
4. 旧系统中的协议、设备经验、业务规则按需抽取进入新系统
