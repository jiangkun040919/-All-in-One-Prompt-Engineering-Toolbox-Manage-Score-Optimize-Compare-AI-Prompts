<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { promptApi } from '@/services/api'

const router = useRouter()
const stats = ref({
  totalPrompts: 0,
  categories: 0,
  favorites: 0,
  avgScore: 0
})

const recentPrompts = ref<any[]>([])

onMounted(async () => {
  try {
    const { data } = await promptApi.getAll()
    stats.value.totalPrompts = data.length
    stats.value.favorites = data.filter((p: any) => p.isFavorite).length
    recentPrompts.value = data.slice(0, 6)
  } catch (error) {
    console.error('Failed to load prompts:', error)
  }
})

const features = [
  {
    icon: 'Edit',
    title: 'Prompt 编辑器',
    desc: '语法高亮、变量插值、版本管理',
    color: '#409EFF',
    path: '/editor'
  },
  {
    icon: 'DataAnalysis',
    title: '批量测试',
    desc: '同一 Prompt 对比多个模型输出',
    color: '#67C23A',
    path: '/test'
  },
  {
    icon: 'MagicStick',
    title: 'AI 优化',
    desc: 'AI 分析并优化你的 Prompt',
    color: '#E6A23C',
    path: '/optimize'
  },
  {
    icon: 'Document',
    title: '模板库',
    desc: '分类管理常用 Prompt 模板',
    color: '#F56C6C',
    path: '/prompts'
  }
]
</script>

<template>
  <div class="home">
    <!-- Hero Section -->
    <div class="hero">
      <h1>PromptCraft</h1>
      <p>一站式 Prompt 工程工具箱</p>
      <div class="hero-stats">
        <div class="stat-item">
          <span class="stat-value">{{ stats.totalPrompts }}</span>
          <span class="stat-label">Prompts</span>
        </div>
        <div class="stat-item">
          <span class="stat-value">{{ stats.favorites }}</span>
          <span class="stat-label">收藏</span>
        </div>
        <div class="stat-item">
          <span class="stat-value">{{ stats.avgScore || '--' }}</span>
          <span class="stat-label">平均分</span>
        </div>
      </div>
    </div>

    <!-- Features Grid -->
    <div class="features-grid">
      <div
        v-for="feature in features"
        :key="feature.title"
        class="feature-card"
        @click="router.push(feature.path)"
      >
        <div class="feature-icon" :style="{ background: feature.color + '20', color: feature.color }">
          <el-icon :size="32"><component :is="feature.icon" /></el-icon>
        </div>
        <h3>{{ feature.title }}</h3>
        <p>{{ feature.desc }}</p>
      </div>
    </div>

    <!-- Recent Prompts -->
    <div class="section" v-if="recentPrompts.length">
      <h2>最近使用</h2>
      <div class="prompts-grid">
        <el-card
          v-for="prompt in recentPrompts"
          :key="prompt.id"
          class="prompt-card"
          shadow="hover"
          @click="router.push(`/editor/${prompt.id}`)"
        >
          <template #header>
            <div class="prompt-header">
              <span>{{ prompt.title }}</span>
              <el-icon v-if="prompt.isFavorite" color="#E6A23C"><StarFilled /></el-icon>
            </div>
          </template>
          <p class="prompt-preview">{{ prompt.content.slice(0, 100) }}...</p>
          <div class="prompt-meta">
            <el-tag size="small">{{ prompt.category?.name || '未分类' }}</el-tag>
            <span class="usage-count">使用 {{ prompt.usageCount }} 次</span>
          </div>
        </el-card>
      </div>
    </div>

    <!-- Quick Start -->
    <div class="section">
      <h2>快速开始</h2>
      <el-steps :active="0" align-center>
        <el-step title="创建 Prompt" description="在编辑器中编写你的 Prompt" />
        <el-step title="测试优化" description="使用 AI 评分和优化功能" />
        <el-step title="对比模型" description="在不同模型间测试效果" />
        <el-step title="管理收藏" description="整理你的 Prompt 模板库" />
      </el-steps>
    </div>
  </div>
</template>

<style scoped>
.home {
  width: 100%;
  max-width: 1400px;
  margin: 0 auto;
}

.hero {
  text-align: center;
  padding: 40px 20px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border-radius: 12px;
  color: white;
  margin-bottom: 30px;
}

.hero h1 {
  font-size: 36px;
  margin: 0 0 10px;
}

.hero p {
  font-size: 18px;
  opacity: 0.9;
  margin: 0 0 30px;
}

.hero-stats {
  display: flex;
  justify-content: center;
  gap: 40px;
}

.stat-item {
  display: flex;
  flex-direction: column;
}

.stat-value {
  font-size: 32px;
  font-weight: bold;
}

.stat-label {
  font-size: 14px;
  opacity: 0.8;
}

.features-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 20px;
  margin-bottom: 30px;
}

.feature-card {
  background: white;
  border-radius: 12px;
  padding: 24px;
  text-align: center;
  cursor: pointer;
  transition: all 0.3s;
}

.feature-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.1);
}

.feature-icon {
  width: 64px;
  height: 64px;
  border-radius: 16px;
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 auto 16px;
}

.feature-card h3 {
  margin: 0 0 8px;
  font-size: 16px;
}

.feature-card p {
  margin: 0;
  color: #666;
  font-size: 14px;
}

.section {
  margin-bottom: 30px;
}

.section h2 {
  font-size: 20px;
  margin: 0 0 20px;
  padding-bottom: 10px;
  border-bottom: 2px solid #409EFF;
  display: inline-block;
}

.prompts-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 16px;
}

.prompt-card {
  cursor: pointer;
}

.prompt-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.prompt-preview {
  color: #666;
  font-size: 14px;
  line-height: 1.5;
  margin: 0 0 12px;
}

.prompt-meta {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.usage-count {
  color: #999;
  font-size: 12px;
}
</style>
