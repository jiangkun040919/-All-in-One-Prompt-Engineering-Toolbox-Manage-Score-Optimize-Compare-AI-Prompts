import axios from 'axios'

const api = axios.create({
  baseURL: 'http://localhost:5076/api',
  timeout: 30000,
})

// Prompt API
export const promptApi = {
  getAll: (params?: { categoryId?: number; search?: string; favorite?: boolean }) =>
    api.get('/prompts', { params }),

  getById: (id: number) => api.get(`/prompts/${id}`),

  create: (data: any) => api.post('/prompts', data),

  update: (id: number, data: any) => api.put(`/prompts/${id}`, data),

  delete: (id: number) => api.delete(`/prompts/${id}`),

  toggleFavorite: (id: number) => api.post(`/prompts/${id}/favorite`),

  getVersions: (id: number) => api.get(`/prompts/${id}/versions`),

  export: (categoryId?: number) =>
    api.get('/prompts/export', { params: { categoryId }, responseType: 'blob' }),

  import: (data: any) => api.post('/prompts/import', data),
}

// Category API
export const categoryApi = {
  getAll: () => api.get('/categories'),
  create: (data: any) => api.post('/categories', data),
}

// Model Config API
export const modelConfigApi = {
  getAll: () => api.get('/models'),

  getById: (id: number) => api.get(`/models/${id}`),

  create: (data: {
    modelId: string
    name: string
    provider: string
    apiEndpoint: string
    apiKey: string
  }) => api.post('/models', data),

  update: (id: number, data: {
    name?: string
    apiEndpoint?: string
    apiKey?: string
    isActive?: boolean
  }) => api.put(`/models/${id}`, data),

  delete: (id: number) => api.delete(`/models/${id}`),

  toggleActive: (id: number) => api.post(`/models/${id}/toggle`),

  testConnection: (id: number) => api.post(`/models/${id}/test`),

  getProviders: () => api.get('/models/providers'),
}

// AI API
export const aiApi = {
  chat: (data: { model: string; prompt: string; systemMessage?: string }) =>
    api.post('/ai/chat', data),

  score: (data: { prompt: string; task?: string }) => api.post('/ai/score', data),

  optimize: (data: { prompt: string; goal?: string; targetModel?: string }) =>
    api.post('/ai/optimize', data),

  compare: (data: { prompt: string; input: string; models: string[] }) =>
    api.post('/ai/compare', data),

  getModels: () => api.get('/ai/models'),
}

export default api
