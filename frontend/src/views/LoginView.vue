<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const router = useRouter()
const auth = useAuthStore()

const email = ref('')
const password = ref('')
const error = ref(null)
const loading = ref(false)

async function submit() {
  error.value = null
  loading.value = true
  try {
    await auth.login(email.value, password.value)
    router.push('/')
  } catch (e) {
    error.value = e.message
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="min-h-screen flex items-center justify-center">
    <div class="w-full max-w-sm bg-white rounded-xl shadow p-8">
      <h1 class="text-2xl font-semibold text-slate-900 mb-6">Sign in</h1>
      <form @submit.prevent="submit" class="space-y-4">
        <div class="flex flex-col gap-1">
          <label class="text-sm font-medium text-slate-700">Email</label>
          <input v-model="email" type="email" required autocomplete="email"
            class="border border-slate-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary" />
        </div>
        <div class="flex flex-col gap-1">
          <label class="text-sm font-medium text-slate-700">Password</label>
          <input v-model="password" type="password" required autocomplete="current-password"
            class="border border-slate-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary" />
        </div>
        <p v-if="error" class="text-red-500 text-sm">{{ error }}</p>
        <button type="submit" :disabled="loading"
          class="w-full bg-primary hover:bg-primary-hover text-white font-medium py-2 rounded-lg text-sm disabled:opacity-50">
          {{ loading ? 'Signing in...' : 'Sign in' }}
        </button>
      </form>
      <p class="text-center text-sm text-slate-500 mt-6">
        Don't have an account?
        <RouterLink to="/register" class="text-primary font-medium hover:underline">Register</RouterLink>
      </p>
    </div>
  </div>
</template>

