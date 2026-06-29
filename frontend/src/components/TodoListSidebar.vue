<script setup>
import { ref, onMounted } from 'vue'
import { listsApi } from '../services/api'

const emit = defineEmits(['select'])

const lists = ref([])
const selectedId = ref(null)
const newListName = ref('')
const creating = ref(false)
const error = ref(null)

onMounted(fetchLists)

async function fetchLists() {
  try {
    lists.value = await listsApi.getAll()
  } catch (e) {
    error.value = e.message
  }
}

function select(list) {
  selectedId.value = list.id
  emit('select', list)
}

async function createList() {
  if (!newListName.value.trim()) return
  try {
    const list = await listsApi.create(newListName.value.trim())
    lists.value.push(list)
    newListName.value = ''
    creating.value = false
    select(list)
  } catch (e) {
    error.value = e.message
  }
}
</script>

<template>
  <aside class="w-64 bg-white border-r border-slate-200 flex flex-col">
    <div class="p-4 border-b border-slate-200 flex items-center justify-between">
      <h2 class="text-sm font-semibold text-slate-700 uppercase tracking-wide">My Lists</h2>
      <button @click="creating = !creating"
        class="text-slate-400 hover:text-primary text-xl leading-none">+</button>
    </div>

    <div v-if="creating" class="p-3 border-b border-slate-100">
      <input v-model="newListName" @keyup.enter="createList" @keyup.esc="creating = false"
        placeholder="List name..."
        class="w-full border border-slate-300 rounded px-2 py-1 text-sm focus:outline-none focus:ring-2 focus:ring-primary"
        autofocus />
      <div class="flex gap-2 mt-2">
        <button @click="createList"
          class="flex-1 bg-primary text-white text-xs py-1 rounded hover:bg-primary-hover">
          Create
        </button>
        <button @click="creating = false"
          class="flex-1 border border-slate-300 text-xs py-1 rounded text-slate-600 hover:bg-slate-50">
          Cancel
        </button>
      </div>
    </div>

    <nav class="flex-1 overflow-y-auto p-2">
      <p v-if="error" class="text-red-500 text-xs p-2">{{ error }}</p>
      <p v-else-if="lists.length === 0" class="text-slate-400 text-sm p-2">No lists yet.</p>
      <button v-for="list in lists" :key="list.id" @click="select(list)"
        class="w-full text-left px-3 py-2 rounded-lg text-sm flex items-center justify-between group"
        :class="selectedId === list.id
          ? 'bg-primary text-white'
          : 'text-slate-700 hover:bg-slate-100'">
        <span class="truncate">{{ list.name }}</span>
        <span class="text-xs opacity-60 ml-2 shrink-0">{{ list.role }}</span>
      </button>
    </nav>
  </aside>
</template>
