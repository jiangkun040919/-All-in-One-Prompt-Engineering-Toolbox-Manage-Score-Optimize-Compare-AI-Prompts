<div align="center">

# PromptCraft

**All-in-One Prompt Engineering Toolbox**

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-9.0-purple)](https://dotnet.microsoft.com/)
[![Vue](https://img.shields.io/badge/Vue-3.4-green)](https://vuejs.org/)

English | [中文](./README.md)

</div>

## Features

- **Prompt Editor** - Syntax highlighting, variable interpolation `{{variable}}`, version management
- **Template Library** - Categorized prompt management with favorites and tags
- **AI Scoring** - Automatic quality assessment (clarity, specificity, structure, completeness)
- **AI Optimization** - AI analyzes and optimizes your prompts
- **Model Comparison** - Compare outputs from multiple models with the same prompt
- **Import/Export** - Batch import/export in JSON format

## Tech Stack

| Layer | Technology |
|-------|------------|
| Frontend | Vue 3 + TypeScript + Element Plus + Pinia |
| Backend | .NET 9 Minimal API + EF Core + SQLite |
| AI | DeepSeek / Qwen / OpenAI compatible API |

## Quick Start

### 1. Clone the project

```bash
git clone https://github.com/yourusername/promptcraft.git
cd promptcraft
```

### 2. Configure API Key

Edit `backend/PromptCraft.Api/appsettings.json`:

```json
{
  "AI": {
    "DefaultModel": "deepseek-chat",
    "DeepSeekApiKey": "your_api_key_here",
    "QwenApiKey": "",
    "OpenAIApiKey": ""
  }
}
```

### 3. Start Backend

```bash
cd backend/PromptCraft.Api
dotnet run
```

Backend will start at `http://localhost:5000`.

### 4. Start Frontend

```bash
cd frontend/frontend
npm install
npm run dev
```

Frontend will start at `http://localhost:5173`.

## Project Structure

```
promptcraft/
├── backend/
│   └── PromptCraft.Api/
│       ├── Data/          # Database context
│       ├── Endpoints/     # API endpoints
│       ├── Models/        # Data models
│       └── Services/      # Business services
├── frontend/
│   └── frontend/
│       ├── src/
│       │   ├── services/  # API services
│       │   └── views/     # Page components
│       └── ...
└── README.md
```

## API Documentation

Access `http://localhost:5000/openapi` after starting the backend to view OpenAPI documentation.

### Main Endpoints

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/prompts` | Get all prompts |
| POST | `/api/prompts` | Create prompt |
| PUT | `/api/prompts/{id}` | Update prompt |
| DELETE | `/api/prompts/{id}` | Delete prompt |
| POST | `/api/ai/score` | AI scoring |
| POST | `/api/ai/optimize` | AI optimization |
| POST | `/api/ai/compare` | Model comparison |

## Supported Models

| Model | Provider | Description |
|-------|----------|-------------|
| deepseek-chat | DeepSeek | General chat |
| deepseek-reasoner | DeepSeek | Reasoning enhanced |
| qwen-plus | Alibaba | Qwen Plus |
| qwen-turbo | Alibaba | Qwen Turbo |
| gpt-4o-mini | OpenAI | GPT-4o Mini |
| gpt-4o | OpenAI | GPT-4o |

## Contributing

Issues and Pull Requests are welcome!

## License

[MIT License](LICENSE)
