<script setup lang="ts">
import { ref, onMounted, watch, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { promptApi, categoryApi, aiApi, modelConfigApi } from '@/services/api'
import { ElMessage } from 'element-plus'

const route = useRoute()
const router = useRouter()
const isEditing = ref(false)
const loading = ref(false)
const categories = ref<any[]>([])
const allModels = ref<any[]>([])
const showVersions = ref(false)
const versions = ref<any[]>([])

// 只显示已配置 API Key 的模型
const availableModels = computed(() => allModels.value.filter(m => m.hasApiKey && m.isActive))

const form = ref({
  title: '',
  content: '',
  description: '',
  categoryId: null as number | null,
  tags: [] as string[],
  variables: [] as string[],
  modelId: ''
})

const tagInput = ref('')
const variableInput = ref('')
const aiScore = ref<any>(null)
const scoring = ref(false)

const loadCategories = async () => {
  try {
    const { data } = await categoryApi.getAll()
    categories.value = data
  } catch (error) {
    console.error('Failed to load categories:', error)
  }
}

const loadModels = async () => {
  try {
    const { data } = await modelConfigApi.getAll()
    allModels.value = data
  } catch (error) {
    console.error('Failed to load models:', error)
  }
}

const loadPrompt = async (id: number) => {
  loading.value = true
  try {
    const { data } = await promptApi.getById(id)
    form.value = {
      title: data.title,
      content: data.content,
      description: data.description || '',
      categoryId: data.categoryId,
      tags: data.tags ? JSON.parse(data.tags) : [],
      variables: data.variables ? JSON.parse(data.variables) : [],
      modelId: data.modelId || ''
    }
    isEditing.value = true
  } catch (error) {
    ElMessage.error('加载 Prompt 失败')
  } finally {
    loading.value = false
  }
}

const loadVersions = async () => {
  if (!route.params.id) return
  try {
    const { data } = await promptApi.getVersions(Number(route.params.id))
    versions.value = data
    showVersions.value = true
  } catch (error) {
    ElMessage.error('加载版本历史失败')
  }
}

const savePrompt = async () => {
  if (!form.value.title || !form.value.content) {
    ElMessage.warning('标题和内容不能为空')
    return
  }

  const payload = {
    ...form.value,
    tags: JSON.stringify(form.value.tags),
    variables: JSON.stringify(form.value.variables)
  }

  try {
    if (isEditing.value && route.params.id) {
      await promptApi.update(Number(route.params.id), payload)
      ElMessage.success('更新成功')
    } else {
      await promptApi.create(payload)
      ElMessage.success('创建成功')
      router.push('/prompts')
    }
  } catch (error) {
    ElMessage.error('保存失败')
  }
}

const scorePrompt = async () => {
  if (!form.value.content) {
    ElMessage.warning('请先输入 Prompt 内容')
    return
  }

  scoring.value = true
  try {
    const { data } = await aiApi.score({
      prompt: form.value.content,
      task: form.value.description
    })
    aiScore.value = data
  } catch (error) {
    ElMessage.error('评分失败，请检查 API Key 配置')
  } finally {
    scoring.value = false
  }
}

const addTag = () => {
  if (tagInput.value && !form.value.tags.includes(tagInput.value)) {
    form.value.tags.push(tagInput.value)
    tagInput.value = ''
  }
}

const removeTag = (tag: string) => {
  form.value.tags = form.value.tags.filter(t => t !== tag)
}

const addVariable = () => {
  if (variableInput.value && !form.value.variables.includes(variableInput.value)) {
    form.value.variables.push(variableInput.value)
    variableInput.value = ''
  }
}

const removeVariable = (variable: string) => {
  form.value.variables = form.value.variables.filter(v => v !== variable)
}

const restoreVersion = (version: any) => {
  form.value.content = version.content
  showVersions.value = false
  ElMessage.success(`已恢复到版本 ${version.versionNumber}`)
}

// Extract variables from content
watch(() => form.value.content, (content) => {
  const matches = content.match(/\{\{(\w+)\}\}/g)
  if (matches) {
    const vars = matches.map(m => m.replace(/\{\{|\}\}/g, ''))
    form.value.variables = [...new Set(vars)]
  }
})

onMounted(() => {
  loadCategories()
  loadModels()
  if (route.params.id) {
    loadPrompt(Number(route.params.id))
  }
})
</script>

<template>
  <div class="editor-view" v-loading="loading">
    <div class="editor-layout">
      <!-- Left: Editor -->
      <div class="editor-main">
        <el-card>
          <template #header>
            <div class="editor-header">
              <h3>{{ isEditing ? '编辑 Prompt' : '新建 Prompt' }}</h3>
              <div class="header-actions">
                <el-button @click="loadVersions" v-if="isEditing">
                  <el-icon><Clock /></el-icon>
                  版本历史
                </el-button>
                <el-button @click="scorePrompt" :loading="scoring">
                  <el-icon><DataAnalysis /></el-icon>
                  AI 评分
                </el-button>
                <el-button type="primary" @click="savePrompt">
                  <el-icon><Check /></el-icon>
                  保存
                </el-button>
              </div>
            </div>
          </template>

          <el-form label-position="top">
            <el-form-item label="标题">
              <el-input v-model="form.title" placeholder="输入 Prompt 标题" />
            </el-form-item>

            <el-form-item label="描述">
              <el-input v-model="form.description" placeholder="描述这个 Prompt 的用途" />
            </el-form-item>

            <el-form-item label="分类">
              <el-select v-model="form.categoryId" placeholder="选择分类">
                <el-option
                  v-for="cat in categories"
                  :key="cat.id"
                  :label="cat.name"
                  :value="cat.id"
                />
              </el-select>
            </el-form-item>

            <el-form-item label="首选模型">
              <div class="model-select-container">
                <el-select v-model="form.modelId" placeholder="选择首选模型" clearable style="width: 100%">
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

            <el-form-item label="Prompt 内容">
              <el-input
                v-model="form.content"
                type="textarea"
                :rows="12"
                placeholder="输入你的 Prompt 内容...&#10;&#10;使用 {{变量名}} 来定义变量"
              />
            </el-form-item>

            <el-form-item label="变量">
              <div class="variable-list">
                <el-tag
                  v-for="variable in form.variables"
                  :key="variable"
                  closable
                  @close="removeVariable(variable)"
                >
                  {{ variable }}
                </el-tag>
              </div>
              <el-input
                v-model="variableInput"
                placeholder="添加变量名"
                @keyup.enter="addVariable"
                style="width: 200px"
              >
                <template #append>
                  <el-button @click="addVariable">添加</el-button>
                </template>
              </el-input>
            </el-form-item>

            <el-form-item label="标签">
              <div class="tag-list">
                <el-tag
                  v-for="tag in form.tags"
                  :key="tag"
                  closable
                  @close="removeTag(tag)"
                >
                  {{ tag }}
                </el-tag>
              </div>
              <el-input
                v-model="tagInput"
                placeholder="添加标签"
                @keyup.enter="addTag"
                style="width: 200px"
              >
                <template #append>
                  <el-button @click="addTag">添加</el-button>
                </template>
              </el-input>
            </el-form-item>
          </el-form>
        </el-card>
      </div>

      <!-- Right: Preview & Score -->
      <div class="editor-sidebar">
        <!-- AI Score Card -->
        <el-card v-if="aiScore" class="score-card">
          <template #header>
            <h4>AI 评分结果</h4>
          </template>

          <div class="score-total">
            <el-progress
              type="dashboard"
              :percentage="aiScore.totalScore"
              :color="aiScore.totalScore >= 80 ? '#67C23A' : aiScore.totalScore >= 60 ? '#E6A23C' : '#F56C6C'"
            />
          </div>

          <div class="score-details">
            <div class="score-item">
              <span>清晰度</span>
              <el-progress :percentage="aiScore.clarity * 4" :show-text="false" />
              <span>{{ aiScore.clarity }}/25</span>
            </div>
            <div class="score-item">
              <span>具体性</span>
              <el-progress :percentage="aiScore.specificity * 4" :show-text="false" />
              <span>{{ aiScore.specificity }}/25</span>
            </div>
            <div class="score-item">
              <span>结构性</span>
              <el-progress :percentage="aiScore.structure * 4" :show-text="false" />
              <span>{{ aiScore.structure }}/25</span>
            </div>
            <div class="score-item">
              <span>完整性</span>
              <el-progress :percentage="aiScore.completeness * 4" :show-text="false" />
              <span>{{ aiScore.completeness }}/25</span>
            </div>
          </div>

          <div class="score-feedback" v-if="aiScore.strengths?.length">
            <h5>优点</h5>
            <ul>
              <li v-for="s in aiScore.strengths" :key="s">{{ s }}</li>
            </ul>
          </div>

          <div class="score-feedback" v-if="aiScore.improvements?.length">
            <h5>改进建议</h5>
            <ul>
              <li v-for="i in aiScore.improvements" :key="i">{{ i }}</li>
            </ul>
          </div>
        </el-card>

        <!-- Versions Card -->
        <el-card v-if="showVersions" class="versions-card">
          <template #header>
            <div class="versions-header">
              <h4>版本历史</h4>
              <el-button text @click="showVersions = false">关闭</el-button>
            </div>
          </template>

          <el-timeline>
            <el-timeline-item
              v-for="version in versions"
              :key="version.id"
              :timestamp="new Date(version.createdAt).toLocaleString()"
              placement="top"
            >
              <el-card shadow="never">
                <h5>版本 {{ version.versionNumber }}</h5>
                <p>{{ version.changeNote }}</p>
                <el-button size="small" @click="restoreVersion(version)">恢复此版本</el-button>
              </el-card>
            </el-timeline-item>
          </el-timeline>
        </el-card>

        <!-- Preview Card -->
        <el-card class="preview-card">
          <template #header>
            <h4>预览</h4>
          </template>
          <div class="preview-content">
            <pre>{{ form.content || '暂无内容' }}</pre>
          </div>
        </el-card>
      </div>
    </div>
  </div>
</template>

<style scoped>
.editor-view {
  width: 100%;
  height: 100%;
}

.editor-layout {
  display: grid;
  grid-template-columns: 1fr 360px;
  gap: 20px;
}

.editor-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.editor-header h3 {
  margin: 0;
}

.header-actions {
  display: flex;
  gap: 8px;
}

.model-select-container {
  width: 100%;
}

.manage-link {
  margin-top: 8px;
}

.variable-list,
.tag-list {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  margin-bottom: 8px;
}

.score-card {
  margin-bottom: 16px;
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

.score-feedback h5 {
  margin: 0 0 8px;
  font-size: 14px;
  color: #303133;
}

.score-feedback ul {
  margin: 0;
  padding-left: 20px;
  font-size: 13px;
  color: #606266;
}

.versions-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.versions-header h4 {
  margin: 0;
}

.preview-card h4 {
  margin: 0;
}

.preview-content pre {
  white-space: pre-wrap;
  word-wrap: break-word;
  font-family: inherit;
  font-size: 14px;
  line-height: 1.6;
  color: #303133;
  margin: 0;
}
</style>
