<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRouter } from 'vue-router'
import { aiApi, modelConfigApi } from '@/services/api'
import { ElMessage } from 'element-plus'

const router = useRouter()
const prompt = ref('')
const input = ref('')
const selectedModels = ref<string[]>([])
const allModels = ref<any[]>([])
const results = ref<any[]>([])
const loading = ref(false)

// 只显示已配置 API Key 的模型
const availableModels = computed(() => allModels.value.filter(m => m.hasApiKey && m.isActive))

const loadModels = async () => {
  try {
    const { data } = await modelConfigApi.getAll()
    allModels.value = data
    // 默认选中前两个可用模型
    if (availableModels.value.length >= 2) {
      selectedModels.value = [availableModels.value[0].modelId, availableModels.value[1].modelId]
    } else if (availableModels.value.length === 1) {
      selectedModels.value = [availableModels.value[0].modelId]
    }
  } catch (error) {
    console.error('Failed to load models:', error)
  }
}

const runComparison = async () => {
  if (!prompt.value || !input.value) {
    ElMessage.warning('请输入 Prompt 和测试输入')
    return
  }

  if (selectedModels.value.length === 0) {
    ElMessage.warning('请选择至少一个模型')
    return
  }

  loading.value = true
  results.value = []

  try {
    const { data } = await aiApi.compare({
      prompt: prompt.value,
      input: input.value,
      models: selectedModels.value
    })
    results.value = data.results
  } catch (error) {
    ElMessage.error('测试失败，请检查 API Key 配置')
  } finally {
    loading.value = false
  }
}

const clearResults = () => {
  results.value = []
}

onMounted(() => {
  loadModels()
})
</script>

<template>
  <div class="test-view">
    <el-card class="test-form">
      <template #header>
        <h3>模型对比测试</h3>
      </template>

      <el-form label-position="top">
        <el-form-item label="System Prompt（可选）">
          <el-input
            v-model="prompt"
            type="textarea"
            :rows="4"
            placeholder="输入系统提示词..."
          />
        </el-form-item>

        <el-form-item label="用户输入">
          <el-input
            v-model="input"
            type="textarea"
            :rows="4"
            placeholder="输入测试内容..."
          />
        </el-form-item>

        <el-form-item label="选择模型">
          <div class="models-container">
            <div v-if="availableModels.length === 0" class="no-models">
              <el-empty description="暂无可用模型">
                <el-button type="primary" @click="router.push('/models')">
                  前往配置模型
                </el-button>
              </el-empty>
            </div>

            <el-checkbox-group v-model="selectedModels" v-else>
              <div class="models-grid">
                <div
                  v-for="model in availableModels"
                  :key="model.modelId"
                  class="model-item"
                >
                  <el-checkbox :label="model.modelId">
                    <span class="model-name">{{ model.name }}</span>
                    <el-tag size="small" type="info">{{ model.provider }}</el-tag>
                  </el-checkbox>
                </div>
              </div>
            </el-checkbox-group>

            <div class="manage-link">
              <el-button text type="primary" @click="router.push('/models')">
                <el-icon><Setting /></el-icon>
                管理模型
              </el-button>
            </div>
          </div>
        </el-form-item>

        <el-form-item>
          <el-button type="primary" @click="runComparison" :loading="loading" :disabled="availableModels.length === 0">
            <el-icon><CaretRight /></el-icon>
            开始测试
          </el-button>
          <el-button @click="clearResults">清空结果</el-button>
        </el-form-item>
      </el-form>
    </el-card>

    <!-- Results -->
    <div class="results-section" v-if="results.length">
      <h3>测试结果</h3>
      <div class="results-grid">
        <el-card
          v-for="result in results"
          :key="result.model"
          class="result-card"
        >
          <template #header>
            <div class="result-header">
              <span class="model-name">{{ result.model }}</span>
              <div class="result-meta">
                <el-tag size="small">{{ result.tokensUsed }} tokens</el-tag>
                <el-tag size="small" type="info">{{ result.latencyMs }}ms</el-tag>
              </div>
            </div>
          </template>

          <div class="result-content">
            <pre>{{ result.output }}</pre>
          </div>

          <div class="result-footer" v-if="result.score">
            <el-rate
              :model-value="result.score / 2"
              disabled
              show-score
            />
          </div>
        </el-card>
      </div>
    </div>
  </div>
</template>

<style scoped>
.test-view {
  width: 100%;
  max-width: 1400px;
  margin: 0 auto;
}

.test-form {
  margin-bottom: 20px;
}

.test-form h3 {
  margin: 0;
}

.models-container {
  width: 100%;
}

.no-models {
  padding: 20px 0;
}

.models-grid {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
  margin-bottom: 12px;
}

.model-item {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 12px;
  background: #f5f7fa;
  border-radius: 8px;
  border: 1px solid #e4e7ed;
}

.model-item:hover {
  border-color: #409EFF;
}

.model-item .model-name {
  font-weight: 500;
}

.manage-link {
  margin-top: 8px;
}

.results-section h3 {
  margin: 0 0 16px;
  font-size: 18px;
}

.results-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(350px, 1fr));
  gap: 16px;
}

.result-card {
  height: 100%;
}

.result-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.result-header .model-name {
  font-weight: 600;
  font-size: 16px;
}

.result-meta {
  display: flex;
  gap: 8px;
}

.result-content {
  max-height: 400px;
  overflow-y: auto;
}

.result-content pre {
  white-space: pre-wrap;
  word-wrap: break-word;
  font-family: inherit;
  font-size: 14px;
  line-height: 1.6;
  margin: 0;
}

.result-footer {
  margin-top: 16px;
  padding-top: 16px;
  border-top: 1px solid #ebeef5;
}
</style>
