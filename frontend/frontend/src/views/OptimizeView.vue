<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRouter } from 'vue-router'
import { aiApi, modelConfigApi } from '@/services/api'
import { ElMessage } from 'element-plus'

const router = useRouter()
const originalPrompt = ref('')
const goal = ref('')
const targetModel = ref('')
const optimizedPrompt = ref('')
const loading = ref(false)
const scoreResult = ref<any>(null)
const scoring = ref(false)
const allModels = ref<any[]>([])

// 只显示已配置 API Key 的模型
const availableModels = computed(() => allModels.value.filter(m => m.hasApiKey && m.isActive))

const loadModels = async () => {
  try {
    const { data } = await modelConfigApi.getAll()
    allModels.value = data
  } catch (error) {
    console.error('Failed to load models:', error)
  }
}

const optimizePrompt = async () => {
  if (!originalPrompt.value) {
    ElMessage.warning('请输入要优化的 Prompt')
    return
  }

  loading.value = true
  optimizedPrompt.value = ''

  try {
    const { data } = await aiApi.optimize({
      prompt: originalPrompt.value,
      goal: goal.value,
      targetModel: targetModel.value
    })
    optimizedPrompt.value = data.content
  } catch (error) {
    ElMessage.error('优化失败，请检查 API Key 配置')
  } finally {
    loading.value = false
  }
}

const scorePrompt = async (content: string) => {
  scoring.value = true
  try {
    const { data } = await aiApi.score({
      prompt: content,
      task: goal.value
    })
    scoreResult.value = data
  } catch (error) {
    ElMessage.error('评分失败')
  } finally {
    scoring.value = false
  }
}

const copyToClipboard = () => {
  navigator.clipboard.writeText(optimizedPrompt.value)
  ElMessage.success('已复制到剪贴板')
}

const useOptimized = () => {
  originalPrompt.value = optimizedPrompt.value
  optimizedPrompt.value = ''
  scoreResult.value = null
}

onMounted(() => {
  loadModels()
})
</script>

<template>
  <div class="optimize-view">
    <el-row :gutter="20">
      <!-- Left: Input -->
      <el-col :span="12">
        <el-card>
          <template #header>
            <h3>原始 Prompt</h3>
          </template>

          <el-form label-position="top">
            <el-form-item label="Prompt 内容">
              <el-input
                v-model="originalPrompt"
                type="textarea"
                :rows="10"
                placeholder="输入要优化的 Prompt..."
              />
            </el-form-item>

            <el-form-item label="优化目标（可选）">
              <el-input
                v-model="goal"
                placeholder="例如：更清晰的指令、更好的结构、更高的准确性"
              />
            </el-form-item>

            <el-form-item label="目标模型（可选）">
              <div class="model-select-container">
                <el-select v-model="targetModel" placeholder="选择目标模型" clearable style="width: 100%">
                  <el-option
                    v-for="model in availableModels"
                    :key="model.modelId"
                    :label="model.name"
                    :value="model.modelId"
                  >
                    <span>{{ model.name }}</span>
                    <el-tag size="small" type="info" style="margin-left: 8px">
                      {{ model.provider }}
                    </el-tag>
                  </el-option>
                </el-select>

                <div class="manage-link">
                  <el-button text type="primary" size="small" @click="router.push('/models')">
                    <el-icon><Setting /></el-icon>
                    管理模型
                  </el-button>
                </div>
              </div>
            </el-form-item>

            <el-form-item>
              <el-button type="primary" @click="optimizePrompt" :loading="loading" style="width: 100%">
                <el-icon><MagicStick /></el-icon>
                AI 优化
              </el-button>
            </el-form-item>
          </el-form>
        </el-card>
      </el-col>

      <!-- Right: Output -->
      <el-col :span="12">
        <el-card>
          <template #header>
            <div class="output-header">
              <h3>优化结果</h3>
              <div class="output-actions" v-if="optimizedPrompt">
                <el-button text @click="scorePrompt(optimizedPrompt)" :loading="scoring">
                  <el-icon><DataAnalysis /></el-icon>
                  评分
                </el-button>
                <el-button text @click="copyToClipboard">
                  <el-icon><CopyDocument /></el-icon>
                  复制
                </el-button>
                <el-button text @click="useOptimized">
                  <el-icon><RefreshRight /></el-icon>
                  使用
                </el-button>
              </div>
            </div>
          </template>

          <div class="output-content" v-loading="loading">
            <el-empty v-if="!optimizedPrompt && !loading" description='点击"AI 优化"开始' />
            <pre v-else>{{ optimizedPrompt }}</pre>
          </div>
        </el-card>

        <!-- Score Card -->
        <el-card v-if="scoreResult" class="score-card">
          <template #header>
            <h4>评分结果</h4>
          </template>

          <div class="score-total">
            <el-progress
              type="dashboard"
              :percentage="scoreResult.totalScore"
              :color="scoreResult.totalScore >= 80 ? '#67C23A' : scoreResult.totalScore >= 60 ? '#E6A23C' : '#F56C6C'"
            />
          </div>

          <div class="score-details">
            <div class="score-item">
              <span>清晰度</span>
              <el-progress :percentage="scoreResult.clarity * 4" :show-text="false" />
              <span>{{ scoreResult.clarity }}/25</span>
            </div>
            <div class="score-item">
              <span>具体性</span>
              <el-progress :percentage="scoreResult.specificity * 4" :show-text="false" />
              <span>{{ scoreResult.specificity }}/25</span>
            </div>
            <div class="score-item">
              <span>结构性</span>
              <el-progress :percentage="scoreResult.structure * 4" :show-text="false" />
              <span>{{ scoreResult.structure }}/25</span>
            </div>
            <div class="score-item">
              <span>完整性</span>
              <el-progress :percentage="scoreResult.completeness * 4" :show-text="false" />
              <span>{{ scoreResult.completeness }}/25</span>
            </div>
          </div>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<style scoped>
.optimize-view {
  max-width: 1400px;
  margin: 0 auto;
}

.model-select-container {
  width: 100%;
}

.manage-link {
  margin-top: 8px;
}

.output-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.output-header h3 {
  margin: 0;
}

.output-actions {
  display: flex;
  gap: 4px;
}

.output-content {
  min-height: 300px;
}

.output-content pre {
  white-space: pre-wrap;
  word-wrap: break-word;
  font-family: inherit;
  font-size: 14px;
  line-height: 1.6;
  margin: 0;
}

.score-card {
  margin-top: 16px;
}

.score-card h4 {
  margin: 0;
}

.score-total {
  text-align: center;
  margin-bottom: 20px;
}

.score-details {
  margin-bottom: 16px;
}

.score-item {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 8px;
}

.score-item span:first-child {
  width: 60px;
  font-size: 14px;
}

.score-item .el-progress {
  flex: 1;
}

.score-item span:last-child {
  width: 50px;
  text-align: right;
  font-size: 14px;
  color: #606266;
}
</style>
