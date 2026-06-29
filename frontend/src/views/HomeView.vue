<script setup>
import { useAuthStore } from '../stores/auth'
import { useRouter } from 'vue-router'

const auth = useAuthStore()
const router = useRouter()

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
        <button @click="logout"
          class="text-sm text-slate-600 hover:text-slate-900 font-medium">
          Sign out
        </button>
      </div>
    </header>
    <main class="flex-1 flex">
      <p class="p-6 text-slate-400">Todo lists go here</p>
    </main>
  </div>
</template>

