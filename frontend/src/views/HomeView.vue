<script setup>
import { ref } from 'vue'
import { useAuthStore } from '../stores/auth'
import { useRouter } from 'vue-router'
import TodoListSidebar from '../components/TodoListSidebar.vue'

const auth = useAuthStore()
const router = useRouter()
const selectedList = ref(null)

function logout() {
  auth.logout()
  router.push('/login')
}
</script>

<template>
  <div class="min-h-screen flex flex-col">
    <header class="bg-white border-b border-slate-200 px-6 py-3 flex items-center justify-between">
      <span class="font-semibold text-slate-700">Todo App</span>
      <div class="flex items-center gap-4">
        <span class="text-sm text-slate-500">{{ auth.user?.email }}</span>
        <button @click="logout" class="text-sm text-slate-600 hover:text-slate-900 font-medium">
          Sign out
        </button>
      </div>
    </header>
    <main class="flex-1 flex overflow-hidden">
      <TodoListSidebar @select="selectedList = $event" />
      <section class="flex-1 p-6">
        <p v-if="!selectedList" class="text-slate-400">Select a list to get started.</p>
        <p v-else class="text-slate-700 font-semibold text-lg">{{ selectedList.name }}</p>
      </section>
    </main>
  </div>
</template>

