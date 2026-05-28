<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRouter } from 'vue-router'
import { promptApi, categoryApi } from '@/services/api'
import { ElMessage, ElMessageBox } from 'element-plus'

const router = useRouter()
const prompts = ref<any[]>([])
const categories = ref<any[]>([])
const loading = ref(false)
const searchQuery = ref('')
const selectedCategory = ref<number | null>(null)
const showFavoritesOnly = ref(false)
const importDialogVisible = ref(false)
const importJson = ref('')

const filteredPrompts = computed(() => {
  let result = prompts.value
  if (selectedCategory.value) {
    result = result.filter(p => p.categoryId === selectedCategory.value)
  }
  if (showFavoritesOnly.value) {
    result = result.filter(p => p.isFavorite)
  }
  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    result = result.filter(p =>
      p.title.toLowerCase().includes(query) ||
      p.content.toLowerCase().includes(query)
    )
  }
  return result
})

const loadPrompts = async () => {
  loading.value = true
  try {
    const { data } = await promptApi.getAll()
    prompts.value = data
  } catch (error) {
    ElMessage.error('加载失败')
  } finally {
    loading.value = false
  }
}

const loadCategories = async () => {
  try {
    const { data } = await categoryApi.getAll()
    categories.value = data
  } catch (error) {
    console.error('Failed to load categories:', error)
  }
}

const toggleFavorite = async (prompt: any) => {
  try {
    await promptApi.toggleFavorite(prompt.id)
    prompt.isFavorite = !prompt.isFavorite
    ElMessage.success(prompt.isFavorite ? '已收藏' : '已取消收藏')
  } catch (error) {
    ElMessage.error('操作失败')
  }
}

const deletePrompt = async (prompt: any) => {
  try {
    await ElMessageBox.confirm('确定要删除这个 Prompt 吗？', '确认删除', {
      type: 'warning'
    })
    await promptApi.delete(prompt.id)
    prompts.value = prompts.value.filter(p => p.id !== prompt.id)
    ElMessage.success('删除成功')
  } catch (error) {
    if (error !== 'cancel') {
      ElMessage.error('删除失败')
    }
  }
}

const exportPrompts = async () => {
  try {
    const response = await promptApi.export(selectedCategory.value || undefined)
    const url = window.URL.createObjectURL(new Blob([response.data]))
    const link = document.createElement('a')
    link.href = url
    link.setAttribute('download', 'prompts.json')
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
    ElMessage.success('导出成功')
  } catch (error) {
    ElMessage.error('导出失败')
  }
}

const openImportDialog = () => {
  importJson.value = ''
  importDialogVisible.value = true
}

const handleImport = async () => {
  if (!importJson.value) {
    ElMessage.warning('请输入 JSON 数据')
    return
  }

  try {
    JSON.parse(importJson.value)
  } catch {
    ElMessage.error('JSON 格式不正确')
    return
  }

  try {
    const { data } = await promptApi.import(importJson.value)
    ElMessage.success(`成功导入 ${data.imported} 个 Prompt`)
    importDialogVisible.value = false
    loadPrompts()
  } catch (error) {
    ElMessage.error('导入失败')
  }
}

const handleFileUpload = (event: Event) => {
  const input = event.target as HTMLInputElement
  if (!input.files?.length) return

  const file = input.files[0]
  const reader = new FileReader()
  reader.onload = (e) => {
    importJson.value = e.target?.result as string
  }
  reader.readAsText(file)
  input.value = ''
}

onMounted(() => {
  loadPrompts()
  loadCategories()
})
</script>

<template>
  <div class="prompt-list">
    <!-- Header -->
    <div class="list-header">
      <div class="header-left">
        <el-input
          v-model="searchQuery"
          placeholder="搜索 Prompt..."
          :prefix-icon="'Search'"
          clearable
          style="width: 300px"
        />
        <el-select v-model="selectedCategory" placeholder="选择分类" clearable style="width: 150px">
          <el-option
            v-for="cat in categories"
            :key="cat.id"
            :label="cat.name"
            :value="cat.id"
          />
        </el-select>
        <el-checkbox v-model="showFavoritesOnly">仅收藏</el-checkbox>
      </div>
      <div class="header-right">
        <el-button @click="openImportDialog">
          <el-icon><Upload /></el-icon>
          导入
        </el-button>
        <el-button @click="exportPrompts">
          <el-icon><Download /></el-icon>
          导出
        </el-button>
        <el-button type="primary" @click="router.push('/editor')">
          <el-icon><Plus /></el-icon>
          新建 Prompt
        </el-button>
      </div>
    </div>

    <!-- Prompts Grid -->
    <div class="prompts-grid" v-loading="loading">
      <el-card
        v-for="prompt in filteredPrompts"
        :key="prompt.id"
        class="prompt-card"
        shadow="hover"
      >
        <template #header>
          <div class="card-header">
            <span class="title" @click="router.push(`/editor/${prompt.id}`)">{{ prompt.title }}</span>
            <div class="actions">
              <el-icon
                :class="{ 'is-favorite': prompt.isFavorite }"
                @click.stop="toggleFavorite(prompt)"
              >
                <StarFilled v-if="prompt.isFavorite" />
                <Star v-else />
              </el-icon>
              <el-icon @click.stop="deletePrompt(prompt)"><Delete /></el-icon>
            </div>
          </div>
        </template>

        <p class="content-preview">{{ prompt.content.slice(0, 150) }}...</p>

        <div class="card-footer">
          <el-tag size="small" :type="prompt.category?.color ? '' : 'info'">
            {{ prompt.category?.name || '未分类' }}
          </el-tag>
          <div class="meta">
            <span><el-icon><View /></el-icon> {{ prompt.usageCount }}</span>
            <span v-if="prompt.avgScore">
              <el-icon><Star /></el-icon> {{ prompt.avgScore.toFixed(1) }}
            </span>
          </div>
        </div>

        <div class="tags" v-if="prompt.tags">
          <el-tag
            v-for="tag in JSON.parse(prompt.tags)"
            :key="tag"
            size="small"
            type="info"
            effect="plain"
          >
            {{ tag }}
          </el-tag>
        </div>
      </el-card>

      <!-- Empty State -->
      <el-empty v-if="!loading && filteredPrompts.length === 0" description="暂无 Prompt">
        <el-button type="primary" @click="router.push('/editor')">创建第一个 Prompt</el-button>
      </el-empty>
    </div>

    <!-- Import Dialog -->
    <el-dialog v-model="importDialogVisible" title="导入 Prompt" width="600px">
      <div class="import-content">
        <p>支持 JSON 格式导入，格式示例：</p>
        <pre class="format-example">[
  {
    "title": "Prompt 标题",
    "content": "Prompt 内容",
    "description": "描述",
    "category": "分类名称",
    "tags": ["标签1", "标签2"],
    "variables": ["变量1"]
  }
]</pre>

        <div class="import-actions">
          <el-button @click="() => { const input = document.createElement('input'); input.type = 'file'; input.accept = '.json'; input.onchange = handleFileUpload; input.click() }">
            <el-icon><Upload /></el-icon>
            从文件导入
          </el-button>
        </div>

        <el-input
          v-model="importJson"
          type="textarea"
          :rows="10"
          placeholder="粘贴 JSON 数据..."
        />
      </div>

      <template #footer>
        <el-button @click="importDialogVisible = false">取消</el-button>
        <el-button type="primary" @click="handleImport">导入</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped>
.prompt-list {
  width: 100%;
  height: 100%;
}

.list-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
  padding: 16px;
  background: white;
  border-radius: 8px;
}

.header-left {
  display: flex;
  gap: 12px;
  align-items: center;
}

.header-right {
  display: flex;
  gap: 8px;
}

.prompts-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
  gap: 16px;
}

.prompt-card {
  cursor: pointer;
  transition: all 0.3s;
}

.prompt-card:hover {
  transform: translateY(-3px);
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.card-header .title {
  font-weight: 600;
  font-size: 16px;
  color: #303133;
}

.card-header .title:hover {
  color: #409EFF;
}

.actions {
  display: flex;
  gap: 8px;
}

.actions .el-icon {
  cursor: pointer;
  color: #909399;
  font-size: 16px;
}

.actions .el-icon:hover {
  color: #F56C6C;
}

.is-favorite {
  color: #E6A23C !important;
}

.content-preview {
  color: #606266;
  font-size: 14px;
  line-height: 1.6;
  margin: 0 0 12px;
  min-height: 60px;
}

.card-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 8px;
}

.meta {
  display: flex;
  gap: 12px;
  color: #909399;
  font-size: 12px;
}

.meta .el-icon {
  margin-right: 4px;
}

.tags {
  display: flex;
  flex-wrap: wrap;
  gap: 4px;
  margin-top: 8px;
}

.import-content {
  margin-bottom: 16px;
}

.import-content p {
  margin: 0 0 12px;
  color: #606266;
}

.format-example {
  background: #f5f7fa;
  padding: 12px;
  border-radius: 4px;
  font-size: 12px;
  color: #606266;
  margin: 0 0 16px;
  overflow-x: auto;
}

.import-actions {
  margin-bottom: 16px;
}
</style>
