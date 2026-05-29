<div align="center">

# PromptCraft

### 一站式 Prompt 工程工具箱

*创建 · 管理 · 测试 · 优化 — 让每一条 Prompt 都发挥最大价值*

<br/>

![Vue 3](https://img.shields.io/badge/Vue%203-35495E?style=flat-square&logo=vuedotjs&logoColor=4FC08D)
![.NET 9](https://img.shields.io/badge/.NET%209-512BD4?style=flat-square&logo=dotnet&logoColor=white)
![TypeScript](https://img.shields.io/badge/TypeScript-3178C6?style=flat-square&logo=typescript&logoColor=white)
![SQLite](https://img.shields.io/badge/SQLite-003B57?style=flat-square&logo=sqlite&logoColor=white)
![Element Plus](https://img.shields.io/badge/Element%20Plus-409EFF?style=flat-square&logo=element&logoColor=white)
![License](https://img.shields.io/badge/License-MIT-green?style=flat-square)

<br/>

**[English](#features)** · **[中文](#功能特性)** · **[快速开始](#快速开始)** · **[API](#api-概览)**

</div>

---

## 为什么需要 PromptCraft？

> 写了一条好 Prompt，下次却找不到了。
> 改了好几版，想回到之前的版本。
> 不确定哪个模型对你的 Prompt 表现更好。
> 想优化 Prompt，但不知道从何下手。

**如果你遇到过以上任何一条，PromptCraft 就是为你准备的。**

---

## 功能特性

<table>
<tr>
<td width="50%">

### Prompt 管理
- 分类、标签、收藏，像管理代码一样管理 Prompt
- `{{变量}}` 语法自动提取，一条 Prompt 适配多种场景
- 版本历史，每一次修改都有迹可循
- JSON 导入 / 导出，轻松迁移和分享

</td>
<td width="50%">

### AI 评分
- 四维度量化评估：**清晰度 · 具体性 · 结构性 · 完整性**
- 每项 1-25 分，总分 100，告别"感觉还行"
- 支持 DeepSeek / 通义千问 / OpenAI 多模型打分

</td>
</tr>
<tr>
<td>

### 多模型对比
- 同一条 Prompt 同时发给多个模型
- 并排查看输出结果、Token 用量、响应延迟
- 用数据说话，找到最适合你场景的模型

</td>
<td>

### AI 优化
- AI 自动重写 Prompt，提升清晰度和结构
- 可指定优化目标和目标模型
- 优化结果一键评分、复制，或继续迭代

</td>
</tr>
</table>

---

## 技术栈

| 层级 | 技术 |
|------|------|
| **前端** | Vue 3 + TypeScript + Vite + Element Plus + Pinia |
| **后端** | .NET 9 + ASP.NET Core Minimal APIs |
| **数据库** | SQLite + Entity Framework Core |
| **AI 接口** | OpenAI 兼容协议（DeepSeek / 通义千问 / OpenAI） |

---

## 快速开始

### 环境要求

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js](https://nodejs.org/) >= 18

### 方式一：Docker Compose（推荐）

```bash
git clone https://github.com/jiangkun040919/-All-in-One-Prompt-Engineering-Toolbox-Manage-Score-Optimize-Compare-AI-Prompts.git
cd -All-in-One-Prompt-Engineering-Toolbox-Manage-Score-Optimize-Compare-AI-Prompts

# 配置环境变量
cp .env.example .env
# 编辑 .env 填入 API Key

# 启动
docker-compose up -d
```

访问 `http://localhost` 即可使用。

### 方式二：本地开发

**启动后端**

```bash
cd backend/PromptCraft.Api
dotnet run
```

后端运行在 `http://localhost:5076`，Swagger 文档：`http://localhost:5076/openapi`

**启动前端**

```bash
cd frontend/frontend
npm install
npm run dev
```

前端运行在 `http://localhost:5173`

**配置 AI 模型**

进入 **模型管理** 页面，添加模型连接：

| 提供商 | 模型 | API 地址 |
|--------|------|----------|
| DeepSeek | deepseek-chat / deepseek-reasoner | `https://api.deepseek.com/v1` |
| 通义千问 | qwen-plus / qwen-turbo | `https://dashscope.aliyuncs.com/compatible-mode/v1` |
| OpenAI | gpt-4o-mini / gpt-4o | `https://api.openai.com/v1` |

填入 API Key，点击"测试连接"即可。

---

## API 概览

| 方法 | 路径 | 说明 |
|------|------|------|
| `GET/POST` | `/api/prompts` | Prompt 增删改查 |
| `POST` | `/api/prompts/{id}/favorite` | 收藏 / 取消收藏 |
| `GET` | `/api/prompts/{id}/versions` | 版本历史 |
| `GET/POST` | `/api/prompts/export` `/import` | 导入 / 导出 |
| `GET/POST` | `/api/categories` | 分类管理 |
| `GET/POST/PUT/DELETE` | `/api/models` | 模型配置 |
| `POST` | `/api/models/{id}/test` | 测试模型连接 |
| `POST` | `/api/ai/score` | AI 评分 |
| `POST` | `/api/ai/optimize` | AI 优化 |
| `POST` | `/api/ai/compare` | 多模型对比 |

启动后端后访问 `/openapi` 查看完整文档。

---

## 项目结构

```
promptcraft/
├── backend/
│   └── PromptCraft.Api/
│       ├── Program.cs              # 入口 & DI 配置
│       ├── Models/                 # 数据模型
│       ├── Services/               # 业务逻辑
│       ├── Endpoints/              # API 端点定义
│       └── Data/                   # EF Core DbContext
├── frontend/
│   └── frontend/
│       └── src/
│           ├── views/              # 页面组件
│           ├── services/           # API 调用
│           ├── router/             # 路由配置
│           └── stores/             # Pinia 状态管理
├── docker-compose.yml              # Docker 部署配置
└── README.md
```

---

<div align="center">

**如果这个项目对你有帮助，给个 Star 吧**

[![Star](https://img.shields.io/github/stars/jiangkun040919/-All-in-One-Prompt-Engineering-Toolbox-Manage-Score-Optimize-Compare-AI-Prompts?style=social)](https://github.com/jiangkun040919/-All-in-One-Prompt-Engineering-Toolbox-Manage-Score-Optimize-Compare-AI-Prompts)

</div>
