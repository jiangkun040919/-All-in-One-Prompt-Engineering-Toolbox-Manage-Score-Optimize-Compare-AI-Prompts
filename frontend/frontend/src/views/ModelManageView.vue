<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { modelConfigApi } from '@/services/api'
import { ElMessage, ElMessageBox } from 'element-plus'

const models = ref<any[]>([])
const providers = ref<any[]>([])
const loading = ref(false)
const showDialog = ref(false)
const isEditing = ref(false)
const editingId = ref<number | null>(null)

const form = ref({
  modelId: '',
  name: '',
  provider: '',
  apiEndpoint: '',
  apiKey: ''
})

const loadModels = async () => {
  loading.value = true
  try {
    const { data } = await modelConfigApi.getAll()
    models.value = data
  } catch (error) {
    ElMessage.error('加载模型列表失败')
  } finally {
    loading.value = false
  }
}

const loadProviders = async () => {
  try {
    const { data } = await modelConfigApi.getProviders()
    providers.value = data
  } catch (error) {
    console.error('Failed to load providers:', error)
  }
}

const openAddDialog = () => {
  isEditing.value = false
  editingId.value = null
  form.value = {
    modelId: '',
    name: '',
    provider: '',
    apiEndpoint: '',
    apiKey: ''
  }
  showDialog.value = true
}

const openEditDialog = (model: any) => {
  isEditing.value = true
  editingId.value = model.id
  form.value = {
    modelId: model.modelId,
    name: model.name,
    provider: model.provider,
    apiEndpoint: model.apiEndpoint,
    apiKey: '' // Don't show existing key
  }
  showDialog.value = true
}

const onProviderChange = (providerId: string) => {
  const provider = providers.value.find(p => p.id === providerId)
  if (provider) {
    form.value.apiEndpoint = provider.defaultEndpoint
  }
}

const saveModel = async () => {
  if (!form.value.modelId || !form.value.name || !form.value.provider) {
    ElMessage.warning('请填写必填字段')
    return
  }

  try {
    if (isEditing.value && editingId.value) {
      await modelConfigApi.update(editingId.value, {
        name: form.value.name,
        apiEndpoint: form.value.apiEndpoint,
        apiKey: form.value.apiKey || undefined
      })
      ElMessage.success('更新成功')
    } else {
      await modelConfigApi.create(form.value)
      ElMessage.success('添加成功')
    }
    showDialog.value = false
    loadModels()
  } catch (error: any) {
    ElMessage.error(error.response?.data?.error || '操作失败')
  }
}

const deleteModel = async (model: any) => {
  try {
    await ElMessageBox.confirm(`确定要删除模型 ${model.name} 吗？`, '确认删除', {
      type: 'warning'
    })
    await modelConfigApi.delete(model.id)
    ElMessage.success('删除成功')
    loadModels()
  } catch (error) {
    if (error !== 'cancel') {
      ElMessage.error('删除失败')
    }
  }
}

const toggleActive = async (model: any) => {
  try {
    await modelConfigApi.toggleActive(model.id)
    model.isActive = !model.isActive
    ElMessage.success(model.isActive ? '已启用' : '已禁用')
  } catch (error) {
    ElMessage.error('操作失败')
  }
}

const testConnection = async (model: any) => {
  if (!model.hasApiKey) {
    ElMessage.warning('请先配置 API Key')
    return
  }

  try {
    const { data } = await modelConfigApi.testConnection(model.id)
    if (data.success) {
      ElMessage.success(`${data.message} (${data.latencyMs}ms)`)
    } else {
      ElMessage.error(data.message)
    }
  } catch (error: any) {
    ElMessage.error(error.response?.data?.error || '测试失败')
  }
}

onMounted(() => {
  loadModels()
  loadProviders()
})
</script>

<template>
  <div class="model-manage">
    <el-card>
      <template #header>
        <div class="card-header">
          <h3>模型管理</h3>
          <el-button type="primary" @click="openAddDialog">
            <el-icon><Plus /></el-icon>
            添加模型
          </el-button>
        </div>
      </template>

      <el-table :data="models" v-loading="loading" style="width: 100%">
        <el-table-column prop="name" label="模型名称" width="180">
          <template #default="{ row }">
            <div class="model-name">
              <span>{{ row.name }}</span>
              <el-tag size="small" :type="row.isActive ? 'success' : 'info'">
                {{ row.isActive ? '启用' : '禁用' }}
              </el-tag>
            </div>
          </template>
        </el-table-column>

        <el-table-column prop="modelId" label="模型 ID" width="180" />

        <el-table-column prop="provider" label="提供商" width="120">
          <template #default="{ row }">
            <el-tag size="small">{{ row.provider }}</el-tag>
          </template>
        </el-table-column>

        <el-table-column prop="apiEndpoint" label="API 端点" min-width="200">
          <template #default="{ row }">
            <span class="endpoint">{{ row.apiEndpoint }}</span>
          </template>
        </el-table-column>

        <el-table-column label="API Key" width="120">
          <template #default="{ row }">
            <el-tag :type="row.hasApiKey ? 'success' : 'danger'" size="small">
              {{ row.hasApiKey ? '已配置' : '未配置' }}
            </el-tag>
          </template>
        </el-table-column>

        <el-table-column label="操作" width="300" fixed="right">
          <template #default="{ row }">
            <el-button text type="primary" size="small" @click="openEditDialog(row)">
              编辑
            </el-button>
            <el-button text type="success" size="small" @click="testConnection(row)">
              测试
            </el-button>
            <el-button text type="primary" size="small" @click="toggleActive(row)">
              {{ row.isActive ? '禁用' : '启用' }}
            </el-button>
            <el-button text type="danger" size="small" @click="deleteModel(row)">
              删除
            </el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <!-- Add/Edit Dialog -->
    <el-dialog
      v-model="showDialog"
      :title="isEditing ? '编辑模型' : '添加模型'"
      width="500px"
    >
      <el-form label-width="100px">
        <el-form-item label="提供商" required>
          <el-select
            v-model="form.provider"
            placeholder="选择提供商"
            @change="onProviderChange"
            :disabled="isEditing"
          >
            <el-option
              v-for="p in providers"
              :key="p.id"
              :label="p.name"
              :value="p.id"
            />
          </el-select>
        </el-form-item>

        <el-form-item label="模型 ID" required>
          <el-input
            v-model="form.modelId"
            placeholder="例如：deepseek-chat、gpt-4o"
            :disabled="isEditing"
          />
        </el-form-item>

        <el-form-item label="显示名称" required>
          <el-input v-model="form.name" placeholder="例如：DeepSeek Chat" />
        </el-form-item>

        <el-form-item label="API 端点" required>
          <el-input v-model="form.apiEndpoint" placeholder="https://api.example.com/v1/chat/completions" />
        </el-form-item>

        <el-form-item label="API Key">
          <el-input
            v-model="form.apiKey"
            type="password"
            show-password
            :placeholder="isEditing ? '留空则不修改' : '输入 API Key'"
          />
        </el-form-item>
      </el-form>

      <template #footer>
        <el-button @click="showDialog = false">取消</el-button>
        <el-button type="primary" @click="saveModel">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped>
.model-manage {
  width: 100%;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.card-header h3 {
  margin: 0;
}

.model-name {
  display: flex;
  align-items: center;
  gap: 8px;
}

.endpoint {
  font-size: 12px;
  color: #606266;
  word-break: break-all;
}
</style>
