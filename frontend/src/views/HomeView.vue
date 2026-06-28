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
  <div>
    <header>
      <span>{{ auth.user?.email }}</span>
      <button @click="logout">Sign out</button>
    </header>
    <main>
      <p>Home view — todo lists go here</p>
    </main>
  </div>
</template>

