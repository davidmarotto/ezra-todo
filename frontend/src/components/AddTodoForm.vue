<script setup>
import { ref } from 'vue'

const emit = defineEmits(['submit'])

const title = ref('')
const dueDate = ref('')
const expanded = ref(false)

function submit() {
  if (!title.value.trim()) return
  emit('submit', {
    title: title.value.trim(),
    dueDate: dueDate.value || null
  })
  title.value = ''
  dueDate.value = ''
  expanded.value = false
}
</script>

<template>
  <div class="border border-slate-200 rounded-lg bg-white shadow-sm">
    <div class="p-3">
      <input v-model="title" @focus="expanded = true" @keyup.enter="submit"
        placeholder="Add a todo..."
        class="w-full text-sm text-slate-800 placeholder-slate-400 focus:outline-none" />
    </div>
    <div v-if="expanded" class="border-t border-slate-100 bg-slate-50 px-3 py-2 rounded-b-lg">
      <div class="flex items-center gap-4">
        <div class="flex flex-col gap-1">
          <label class="text-xs font-medium text-slate-500">Due date</label>
          <input v-model="dueDate" type="date"
            class="text-xs border border-slate-200 rounded px-2 py-1 text-slate-600 bg-white focus:outline-none focus:ring-1 focus:ring-primary" />
        </div>
        <div class="ml-auto flex items-center gap-2">
          <button @click="expanded = false; title = ''; dueDate = ''"
            class="text-xs text-slate-400 hover:text-slate-600">
            Cancel
          </button>
          <button @click="submit"
            class="bg-primary text-white text-xs px-3 py-1.5 rounded hover:bg-primary-hover">
            Add
          </button>
        </div>
      </div>
    </div>
  </div>
</template>
