<script setup>
import { ref, onMounted } from 'vue'
import { listsApi } from '../services/api'

const emit = defineEmits(['select', 'share'])

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
  <aside class="w-64 bg-slate-100 border-r border-slate-300 flex flex-col">
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

      <template v-else>
        <div v-for="list in lists.filter(l => l.role === 'Owner')" :key="list.id"
          @click="select(list)"
          class="w-full text-left px-3 py-2 rounded-lg text-sm flex items-center justify-between group cursor-pointer"
          :class="selectedId === list.id ? 'bg-primary text-white' : 'text-slate-700 hover:bg-slate-100'">
          <span class="truncate">{{ list.name }}</span>
          <button @click.stop="emit('share', list)"
            class="opacity-0 group-hover:opacity-100 shrink-0 ml-2 p-0.5 rounded hover:bg-black/10 transition-opacity">
            <svg xmlns="http://www.w3.org/2000/svg" class="w-3.5 h-3.5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <circle cx="18" cy="5" r="3"/><circle cx="6" cy="12" r="3"/><circle cx="18" cy="19" r="3"/>
              <line x1="8.59" y1="13.51" x2="15.42" y2="17.49"/><line x1="15.41" y1="6.51" x2="8.59" y2="10.49"/>
            </svg>
          </button>
        </div>

        <template v-if="lists.some(l => l.role !== 'Owner')">
          <div class="flex items-center gap-2 my-2 px-1">
            <div class="flex-1 h-px bg-slate-100"></div>
            <span class="text-xs text-slate-400">Shared with me</span>
            <div class="flex-1 h-px bg-slate-100"></div>
          </div>
          <button v-for="list in lists.filter(l => l.role !== 'Owner')" :key="list.id"
            @click="select(list)"
            class="w-full text-left px-3 py-2 rounded-lg text-sm flex items-center justify-between"
            :class="selectedId === list.id ? 'bg-primary text-white' : 'text-slate-700 hover:bg-slate-100'">
            <span class="truncate">{{ list.name }}</span>
          </button>
        </template>
      </template>
    </nav>
  </aside>
</template>
