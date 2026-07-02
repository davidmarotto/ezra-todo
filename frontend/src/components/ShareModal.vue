<script setup>
import { ref, onMounted, onUnmounted } from 'vue'
import { permissionsApi } from '../services/api'

const props = defineProps({
  list: { type: Object, required: true }
})

const emit = defineEmits(['close'])

const permissions = ref([])
const email = ref('')
const error = ref(null)

function onKeydown(e) {
  if (e.key === 'Escape') emit('close')
}

onMounted(() => {
  fetchPermissions()
  window.addEventListener('keydown', onKeydown)
})

onUnmounted(() => {
  window.removeEventListener('keydown', onKeydown)
})

async function fetchPermissions() {
  try {
    permissions.value = await permissionsApi.getAll(props.list.id)
    error.value = null
  } catch (e) {
    error.value = e.message
  }
}

async function share() {
  if (!email.value.trim()) return
  try {
    const permission = await permissionsApi.share(props.list.id, email.value.trim())
    permissions.value.push(permission)
    email.value = ''
    error.value = null
  } catch (e) {
    error.value = e.message
  }
}

async function revoke(userId) {
  try {
    await permissionsApi.revoke(props.list.id, userId)
    permissions.value = permissions.value.filter(p => p.userId !== userId)
    error.value = null
  } catch (e) {
    error.value = e.message
  }
}
</script>

<template>
  <div class="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
    <div class="bg-white rounded-xl shadow-xl w-full max-w-md p-6">

      <div class="flex items-center justify-between mb-4">
        <h2 class="text-lg font-semibold text-slate-800">Share "{{ list.name }}"</h2>
        <button @click="emit('close')" class="text-slate-400 hover:text-slate-600 text-xl leading-none">×</button>
      </div>

      <p v-if="error" class="text-red-500 text-sm mb-3">{{ error }}</p>

      <div class="mb-6">
        <h3 class="text-xs font-medium text-slate-500 uppercase tracking-wide mb-2">People with access</h3>
        <p v-if="permissions.length === 0" class="text-sm text-slate-400">Not shared with anyone yet.</p>
        <div v-for="p in permissions" :key="p.userId"
          class="flex items-center justify-between py-2 border-b border-slate-100">
          <div>
            <span class="text-sm text-slate-800">{{ p.email }}</span>
            <span class="text-xs text-slate-400 ml-2">{{ p.role }}</span>
          </div>
          <button @click="revoke(p.userId)" class="text-xs text-red-400 hover:text-red-600">
            Revoke
          </button>
        </div>
      </div>

      <div class="flex gap-2">
        <input v-model="email" type="email" placeholder="Email address" @keyup.enter="share"
          class="flex-1 text-sm border border-slate-200 rounded px-3 py-2 focus:outline-none focus:ring-1 focus:ring-primary" />
<button @click="share"
          class="bg-primary text-white text-sm px-4 py-2 rounded hover:bg-primary-hover">
          Share
        </button>
      </div>

    </div>
  </div>
</template>
