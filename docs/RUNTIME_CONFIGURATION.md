# StandardTest Next 运行配置约定

## 目标
- 统一 App/Test 双端运行配置键名与部署目录约定
- 明确配置文件、环境变量、命令行参数之间的覆盖优先级
- 为后续接入 MQTT provider、真实设备连接参数、部署脚本提供稳定边界

## 配置来源优先级
统一遵循：

1. 配置文件
2. 环境变量
3. 命令行参数

即：**配置文件 < 环境变量 < 命令行参数**

## 默认配置文件
- App：`appsettings.app.json`
- Test：`appsettings.test.json`

默认查找顺序：
1. 程序输出目录（`AppContext.BaseDirectory`）
2. 当前工作目录
3. 若均不存在，则退回内置默认值

也支持通过命令行显式指定：
- `--config <path>`
- `--config=<path>`

## 共享配置节：messageBus
App/Test 双端统一使用同构配置结构：

```json
{
  "messageBus": {
    "provider": "inmemory",
    "host": "127.0.0.1",
    "port": 1883,
    "clientId": "stnext-app or stnext-test",
    "topicPrefix": "stnext",
    "username": null,
    "password": null,
    "publishTimeoutSeconds": 5,
    "subscribeTimeoutSeconds": 5
  }
}
```

### 字段约定
- `provider`：消息总线实现类型；当前已落地 `inmemory` 与 `mqtt`
- `host`：总线服务地址；当前 `inmemory` 模式可忽略，MQTT 模式将使用
- `port`：总线服务端口；当前 `inmemory` 模式可忽略，MQTT 模式将使用
- `clientId`：客户端标识；建议 App/Test 分别使用独立值
- `topicPrefix`：主题前缀；默认 `stnext`
- `username` / `password`：认证参数；当前为预留字段
- `publishTimeoutSeconds`：单次发布超时秒数；当前默认 5 秒
- `subscribeTimeoutSeconds`：单次订阅超时秒数；当前默认 5 秒

### 当前环境变量入口
- `STNEXT_MESSAGE_BUS` → 覆盖 `messageBus.provider`
- `STNEXT_MESSAGE_BUS_HOST` → 覆盖 `messageBus.host`
- `STNEXT_MESSAGE_BUS_PORT` → 覆盖 `messageBus.port`
- `STNEXT_MESSAGE_BUS_CLIENT_ID` → 覆盖 `messageBus.clientId`
- `STNEXT_MESSAGE_BUS_TOPIC_PREFIX` → 覆盖 `messageBus.topicPrefix`
- `STNEXT_MESSAGE_BUS_USERNAME` → 覆盖 `messageBus.username`
- `STNEXT_MESSAGE_BUS_PASSWORD` → 覆盖 `messageBus.password`
- `STNEXT_MESSAGEBUS_PUBLISH_TIMEOUT_SECONDS` → 覆盖 `messageBus.publishTimeoutSeconds`
- `STNEXT_MESSAGEBUS_SUBSCRIBE_TIMEOUT_SECONDS` → 覆盖 `messageBus.subscribeTimeoutSeconds`

说明：
- 当前细粒度环境变量已接入 App/Test 双端 Program 启动路径
- 若 `STNEXT_MESSAGE_BUS_PORT` 非法，当前会回退到配置文件中的 `messageBus.port`

## App 侧配置约定
### 配置文件：`appsettings.app.json`

```json
{
  "deviceId": "mock-motor-device",
  "productKind": "Motor_Y",
  "samplingMode": "single",
  "queryGateway": "auto",
  "queryGatewaySqliteDbPath": "../StandardTestNext.Test/artifacts/test-persistence/standardtest-next.db",
  "messageBus": {
    "provider": "inmemory",
    "host": "127.0.0.1",
    "port": 1883,
    "clientId": "stnext-app",
    "topicPrefix": "stnext"
  }
}
```

### 字段说明
- `deviceId`：设备实例标识
- `productKind`：产品型号/产品线标识
- `samplingMode`：采样模式；当前支持 `single` / `burst`
- `queryGateway`：App 查询网关模式；当前支持 `auto` / `seeded-inproc` / `sqlite-inproc` / `null-fallback`
- `queryGatewaySqliteDbPath`：当 `queryGateway=sqlite-inproc` 时使用的 SQLite 数据库路径；`auto` 模式下若该路径存在，则优先读取真实 SQLite，否则回退 seeded in-proc

### 环境变量
- `STNEXT_APP_DEVICE_ID`
- `STNEXT_APP_PRODUCT_KIND`
- `STNEXT_APP_SAMPLING_MODE`
- `STNEXT_APP_QUERY_GATEWAY`
- `STNEXT_APP_QUERY_GATEWAY_SQLITE_DB`
- `STNEXT_MESSAGE_BUS`

### 命令行参数
- `--config`
- `--device-id`
- `--product-kind`
- `--sampling-mode`
- `--query-gateway`
- `--query-gateway-sqlite-db`

## Test 侧配置约定
### 配置文件：`appsettings.test.json`

```json
{
  "persistenceMode": "memory",
  "sqliteDbPath": "artifacts/test-persistence/standardtest-next.db",
  "messageBus": {
    "provider": "inmemory",
    "host": "127.0.0.1",
    "port": 1883,
    "clientId": "stnext-test",
    "topicPrefix": "stnext"
  }
}
```

### 字段说明
- `persistenceMode`：持久化模式；当前支持 `memory` / `sqlite`
- `sqliteDbPath`：SQLite 数据库路径；仅在 `sqlite` 模式下生效

### 环境变量
- `STNEXT_TEST_PERSISTENCE`
- `STNEXT_TEST_SQLITE_DB`
- `STNEXT_MESSAGE_BUS`

### 命令行参数
- `--config`
- `--persistence`
- `--sqlite-db`

## 部署目录约定（当前建议）
建议 App/Test 分别部署到独立目录，各自携带自己的配置文件：

```text
/deploy/standardtest-next/app/
  StandardTestNext.App(.exe)
  appsettings.app.json

/deploy/standardtest-next/test/
  StandardTestNext.Test(.exe)
  appsettings.test.json
  artifacts/
```

原则：
- **双端各自维护自己的配置文件**，不要交叉读取
- **共享键名保持同构**，但配置文件保持独立
- **provider 切换不改变 Bootstrap 主干签名**，只在配置与工厂层扩展

## 当前限制
- `MessageBusFactory` 当前已实现 `inmemory` 与 `mqtt`；MQTT provider 本小时已补上连接后自动重订阅、断线后订阅状态清理、重复订阅控制，并关闭 clean session 以便后续做更稳的联调
- `host/port/clientId/topicPrefix/username/password` 在 `inmemory` 模式下主要是结构占位；在 `mqtt` 模式下已进入真实连接参数
- 当前已补 `RuntimeConfigurationValidator` + `RuntimeConfigurationConsoleReporter`，并已对一批明显非法值启用启动前失败策略：不支持的 provider、非法端口、空 `clientId` / `topicPrefix`、非法 `samplingMode` / `persistenceMode`
- 本小时继续把部署坑前移到启动前自检：当 `provider=mqtt` 时会校验 `messageBus.host` 非空，并尝试对 `host:port` 做一次轻量 TCP reachability probe；当前探测结果已结构化区分 `reachable / timeout / connection-refused / dns-failed / auth-failed / probe-failed`，成功时记 `Info`，失败时记带状态的 `Warning`，减少联调时人工翻异常栈的成本；当 `persistenceMode=sqlite` 且显式给出 `sqliteDbPath` 时，会在启动前探测目录可创建/可写
- 当前 App 查询入口新增了 `sqlite-inproc` 模式：允许 App 在保持当前 in-proc query adapter 结构不变的前提下，直接读取 Test 持久化到 SQLite 的真实记录/附件/报告摘要；这一步先解决“App 默认只看 seed 假数据”的问题，但仍不等同于最终的跨进程/远程 query 边界
- 当前仍未覆盖的主要缺口：真正基于 MQTT 协议握手的认证有效性与 topic ACL 判定、主动重连/backoff 的更细粒度观测、持久化文件锁竞争与更细粒度的 SQLite schema/version 自检

## 下一步
- 继续把 `mqtt` provider 从“最小能连”推进到“可稳定联调”：补主动重连/backoff、连接失败诊断、发布/订阅超时治理
- 新增 `scripts/run-mqtt-smoke.sh` 作为真实联调脚手架：约定 `BROKER_HOST/BROKER_PORT/TOPIC_PREFIX/APP_CLIENT_ID/TEST_CLIENT_ID/RUN_SECONDS` 环境变量，统一启动 App/Test 双进程并落日志到 `artifacts/logs/`；当前机器未发现本地 broker 可执行文件，因此本轮先把双进程 smoke 入口脚本与文档补齐，待 broker 就位后即可直接做真实 MQTT 验证。
- 将当前 TCP 级结构化探测继续推进到更完整的 MQTT 连接/认证/权限级自检，而不只停在 socket 可达性
- 增加运行配置自检与错误提示
- 将本文件中的配置约定同步进部署脚本/样例配置模板
