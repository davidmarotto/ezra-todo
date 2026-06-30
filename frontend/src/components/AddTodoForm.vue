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
  <div class="border border-slate-200 rounded-lg p-3 bg-white">
    <input v-model="title" @focus="expanded = true" @keyup.enter="submit"
      placeholder="Add a todo..."
      class="w-full text-sm text-slate-800 placeholder-slate-400 focus:outline-none" />
    <div v-if="expanded" class="mt-3 flex items-center gap-3">
      <input v-model="dueDate" type="date"
        class="text-xs border border-slate-200 rounded px-2 py-1 text-slate-600 focus:outline-none focus:ring-1 focus:ring-primary" />
      <button @click="submit"
        class="ml-auto bg-primary text-white text-xs px-3 py-1 rounded hover:bg-primary-hover">
        Add
      </button>
      <button @click="expanded = false; title = ''; dueDate = ''"
        class="text-xs text-slate-400 hover:text-slate-600">
        Cancel
      </button>
    </div>
  </div>
</template>
