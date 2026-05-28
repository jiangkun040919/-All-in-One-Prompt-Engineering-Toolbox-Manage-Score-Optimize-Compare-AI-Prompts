<div align="center">

# PromptCraft

**一站式 Prompt 工程工具箱**

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-9.0-purple)](https://dotnet.microsoft.com/)
[![Vue](https://img.shields.io/badge/Vue-3.4-green)](https://vuejs.org/)
[![Docker](https://img.shields.io/badge/Docker-Ready-blue)](docker-compose.yml)

[English](./README_EN.md) | 中文

</div>

## 功能特性

- **Prompt 编辑器** - 变量插值 `{{变量名}}`、版本管理、实时预览
- **模板库** - 分类管理常用 Prompt，支持收藏、标签、搜索
- **AI 评分** - 自动评估 Prompt 质量（清晰度、具体性、结构性、完整性）
- **AI 优化** - AI 分析并优化你的 Prompt
- **模型对比** - 同一 Prompt 对比多个模型的输出效果
- **模型管理** - 自定义添加模型、配置 API Key、测试连接
- **导入导出** - JSON 格式批量导入导出

## 技术栈

| 层 | 技术 |
|---|------|
| 前端 | Vue 3 + TypeScript + Element Plus + Pinia |
| 后端 | .NET 9 Minimal API + EF Core + SQLite |
| AI | DeepSeek / 通义千问 / OpenAI 兼容接口 |
| 部署 | Docker Compose |

## 快速开始

### 方式一：Docker Compose（推荐）

```bash
git clone https://github.com/yourusername/promptcraft.git
cd promptcraft

# 配置环境变量
cp .env.example .env
# 编辑 .env 填入 API Key

# 启动
docker-compose up -d
```

访问 http://localhost 即可使用。

### 方式二：本地开发

**1. 克隆项目**

```bash
git clone https://github.com/yourusername/promptcraft.git
cd promptcraft
```

**2. 启动后端**

```bash
cd backend/PromptCraft.Api
dotnet run
```

后端将在 `http://localhost:5076` 启动。

**3. 启动前端**

```bash
cd frontend/frontend
npm install
npm run dev
```

前端将在 `http://localhost:5173` 启动。

**4. 配置模型**

访问"模型管理"页面，添加模型并配置 API Key。

## 项目结构

```
promptcraft/
├── backend/
│   └── PromptCraft.Api/
│       ├── Data/          # 数据库上下文
│       ├── Endpoints/     # API 端点
│       ├── Models/        # 数据模型
│       └── Services/      # 业务服务
├── frontend/
│   └── frontend/
│       ├── src/
│       │   ├── services/  # API 服务
│       │   └── views/     # 页面组件
│       └── ...
├── docker-compose.yml     # Docker 部署配置
└── README.md
```

## API 文档

启动后端后访问 `http://localhost:5076/openapi` 查看 OpenAPI 文档。

### 主要接口

| 方法 | 路径 | 说明 |
|------|------|------|
| GET | `/api/prompts` | 获取所有 Prompt |
| POST | `/api/prompts` | 创建 Prompt |
| PUT | `/api/prompts/{id}` | 更新 Prompt |
| DELETE | `/api/prompts/{id}` | 删除 Prompt |
| GET | `/api/models` | 获取所有模型 |
| POST | `/api/models` | 添加模型 |
| POST | `/api/models/{id}/test` | 测试模型连接 |
| POST | `/api/ai/score` | AI 评分 |
| POST | `/api/ai/optimize` | AI 优化 |
| POST | `/api/ai/compare` | 模型对比 |

## 支持的模型

| 模型 | 提供商 | 说明 |
|------|--------|------|
| deepseek-chat | DeepSeek | 通用对话 |
| deepseek-reasoner | DeepSeek | 推理增强 |
| qwen-plus | 阿里巴巴 | 通义千问 Plus |
| qwen-turbo | 阿里巴巴 | 通义千问 Turbo |
| gpt-4o-mini | OpenAI | GPT-4o Mini |
| gpt-4o | OpenAI | GPT-4o |
| 自定义 | 任意 | 支持添加任意 OpenAI 兼容模型 |

## 贡献

欢迎提交 Issue 和 Pull Request！

## 许可证

[MIT License](LICENSE)
