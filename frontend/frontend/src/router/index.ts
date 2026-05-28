import { createRouter, createWebHistory } from 'vue-router'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'home',
      component: () => import('../views/HomeView.vue'),
      meta: { title: '首页' }
    },
    {
      path: '/prompts',
      name: 'prompts',
      component: () => import('../views/PromptListView.vue'),
      meta: { title: 'Prompt 库' }
    },
    {
      path: '/editor',
      name: 'editor',
      component: () => import('../views/EditorView.vue'),
      meta: { title: 'Prompt 编辑器' }
    },
    {
      path: '/editor/:id',
      name: 'editor-edit',
      component: () => import('../views/EditorView.vue'),
      meta: { title: '编辑 Prompt' }
    },
    {
      path: '/test',
      name: 'test',
      component: () => import('../views/TestView.vue'),
      meta: { title: '测试对比' }
    },
    {
      path: '/optimize',
      name: 'optimize',
      component: () => import('../views/OptimizeView.vue'),
      meta: { title: 'AI 优化' }
    },
    {
      path: '/models',
      name: 'models',
      component: () => import('../views/ModelManageView.vue'),
      meta: { title: '模型管理' }
    },
  ],
})

export default router
