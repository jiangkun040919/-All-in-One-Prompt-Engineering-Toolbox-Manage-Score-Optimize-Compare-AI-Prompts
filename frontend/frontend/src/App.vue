<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'

const router = useRouter()
const isCollapse = ref(false)

const menuItems = [
  { path: '/', icon: 'HomeFilled', title: '首页' },
  { path: '/prompts', icon: 'Document', title: 'Prompt 库' },
  { path: '/editor', icon: 'Edit', title: '编辑器' },
  { path: '/test', icon: 'DataAnalysis', title: '测试对比' },
  { path: '/optimize', icon: 'MagicStick', title: 'AI 优化' },
  { path: '/models', icon: 'Connection', title: '模型管理' },
]
</script>

<template>
  <el-container class="app-container">
    <!-- Sidebar -->
    <el-aside :width="isCollapse ? '64px' : '220px'" class="app-aside">
      <div class="logo-container">
        <img src="@/assets/logo.svg" alt="Logo" class="logo" />
        <span v-show="!isCollapse" class="logo-text">PromptCraft</span>
      </div>

      <el-menu
        :default-active="router.currentRoute.value.path"
        :collapse="isCollapse"
        router
        class="sidebar-menu"
      >
        <el-menu-item v-for="item in menuItems" :key="item.path" :index="item.path">
          <el-icon><component :is="item.icon" /></el-icon>
          <template #title>{{ item.title }}</template>
        </el-menu-item>
      </el-menu>

      <div class="collapse-btn" @click="isCollapse = !isCollapse">
        <el-icon>
          <Fold v-if="!isCollapse" />
          <Expand v-else />
        </el-icon>
      </div>
    </el-aside>

    <!-- Main Content -->
    <el-container>
      <el-header class="app-header">
        <div class="header-left">
          <h2>{{ router.currentRoute.value.meta.title || 'PromptCraft' }}</h2>
        </div>
        <div class="header-right">
          <el-button :icon="'Setting'" circle @click="router.push('/models')" />
        </div>
      </el-header>

      <el-main class="app-main">
        <div class="main-content">
          <RouterView />
        </div>
      </el-main>
    </el-container>
  </el-container>
</template>

<style scoped>
.app-container {
  width: 100%;
  height: 100vh;
}

.app-aside {
  background: #304156;
  transition: width 0.3s;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.logo-container {
  height: 60px;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 0 16px;
  border-bottom: 1px solid #3d4f65;
  flex-shrink: 0;
}

.logo {
  width: 32px;
  height: 32px;
}

.logo-text {
  color: #fff;
  font-size: 18px;
  font-weight: bold;
  margin-left: 10px;
  white-space: nowrap;
}

.sidebar-menu {
  flex: 1;
  border-right: none;
  background: transparent;
  overflow-y: auto;
}

.sidebar-menu:not(.el-menu--collapse) {
  width: 220px;
}

.collapse-btn {
  height: 48px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #bfcbd9;
  cursor: pointer;
  border-top: 1px solid #3d4f65;
  flex-shrink: 0;
}

.collapse-btn:hover {
  background: #263445;
}

.app-header {
  background: #fff;
  border-bottom: 1px solid #e6e6e6;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 20px;
  height: 60px;
  flex-shrink: 0;
}

.app-header h2 {
  margin: 0;
  font-size: 18px;
  color: #333;
}

.app-main {
  background: #f5f7fa;
  padding: 20px;
  overflow-y: auto;
  height: calc(100vh - 60px);
}

.main-content {
  width: 100%;
  min-height: 100%;
}
</style>
