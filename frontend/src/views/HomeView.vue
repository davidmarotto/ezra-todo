<script setup>
import { ref } from 'vue'
import { useAuthStore } from '../stores/auth'
import { useRouter } from 'vue-router'
import TodoListSidebar from '../components/TodoListSidebar.vue'
import TodoList from '../components/TodoList.vue'
import ShareModal from '../components/ShareModal.vue'

const auth = useAuthStore()
const router = useRouter()
const selectedList = ref(null)
const shareList = ref(null)

function logout() {
  auth.logout()
  router.push('/login')
}
</script>

<template>
  <div class="min-h-screen flex flex-col">
    <header class="bg-slate-700 px-6 py-3 flex items-center justify-between">
      <span class="font-bold text-white tracking-tight text-lg">David Marotto: Todo</span>
      <div class="flex items-center gap-4">
        <span class="text-sm text-slate-400">{{ auth.user?.email }}</span>
        <button @click="logout" class="text-sm text-slate-300 hover:text-white font-medium">
          Sign out
        </button>
      </div>
    </header>
    <main class="flex-1 flex overflow-hidden">
      <TodoListSidebar @select="selectedList = $event" @share="shareList = $event" />
      <section class="flex-1 p-6 overflow-hidden flex flex-col">
        <p v-if="!selectedList" class="text-slate-400">Select a list to get started.</p>
        <TodoList v-else :list="selectedList" :key="selectedList.id" />
      </section>
    </main>
    <ShareModal v-if="shareList" :list="shareList" @close="shareList = null" />
  </div>
</template>

