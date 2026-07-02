<script setup>
import { ref, watch, computed } from 'vue'
import { todosApi, listsApi } from '../services/api'
import TodoItem from './TodoItem.vue'
import AddTodoForm from './AddTodoForm.vue'

const props = defineProps({
  list: { type: Object, required: true }
})

const todos = ref([])
const status = ref(null)
const error = ref(null)
const canEdit = ['Owner', 'Editor'].includes(props.list.role)
const editingName = ref(false)
const nameInput = ref('')

async function saveListName() {
  if (!nameInput.value.trim() || nameInput.value.trim() === props.list.name) {
    editingName.value = false
    return
  }
  try {
    await listsApi.update(props.list.id, nameInput.value.trim())
    props.list.name = nameInput.value.trim()
    editingName.value = false
  } catch (e) {
    error.value = e.message
  }
}

watch(() => props.list.id, fetchTodos, { immediate: true })
watch(status, fetchTodos)

async function fetchTodos() {
  try {
    todos.value = await todosApi.getAll(props.list.id, status.value)
    error.value = null
  } catch (e) {
    error.value = e.message
  }
}

async function createTodo(data) {
  try {
    const todo = await todosApi.create(props.list.id, data)
    todos.value.push(todo)
  } catch (e) {
    error.value = e.message
  }
}

async function toggleTodo(todo) {
  try {
    const updated = await todosApi.update(props.list.id, todo.id, {
      title: todo.title,
      isCompleted: !todo.isCompleted,
      dueDate: todo.dueDate
    })
    const index = todos.value.findIndex(t => t.id === todo.id)
    todos.value[index] = updated
  } catch (e) {
    error.value = e.message
  }
}

async function renameTodo({ todo, title }) {
  try {
    const updated = await todosApi.update(props.list.id, todo.id, {
      title,
      isCompleted: todo.isCompleted,
      dueDate: todo.dueDate
    })
    const index = todos.value.findIndex(t => t.id === todo.id)
    todos.value[index] = updated
  } catch (e) {
    error.value = e.message
  }
}

async function deleteTodo(todo) {
  try {
    await todosApi.remove(props.list.id, todo.id)
    todos.value = todos.value.filter(t => t.id !== todo.id)
  } catch (e) {
    error.value = e.message
  }
}

const sortedTodos = computed(() => {
  const active = todos.value
    .filter(t => !t.isCompleted)
    .sort((a, b) => {
      if (a.dueDate && b.dueDate) return new Date(a.dueDate) - new Date(b.dueDate)
      if (a.dueDate) return -1
      if (b.dueDate) return 1
      return 0
    })

  const completed = todos.value
    .filter(t => t.isCompleted)
    .sort((a, b) => new Date(a.updatedAt) - new Date(b.updatedAt))

  return [...active, ...completed]
})

const filters = [
  { label: 'All', value: null },
  { label: 'Active', value: 'active' },
  { label: 'Completed', value: 'completed' }
]
</script>

<template>
  <div class="flex flex-col h-full">
    <div class="flex items-center justify-between mb-4">
      <input v-if="editingName && canEdit" v-model="nameInput"
        @keyup.enter="saveListName" @keyup.esc="editingName = false" @blur="saveListName"
        class="text-lg font-semibold text-slate-800 bg-transparent border-b border-primary focus:outline-none" />
      <h2 v-else class="text-lg font-semibold text-slate-800"
        :class="{ 'cursor-pointer hover:text-primary': canEdit }"
        @click="canEdit && (editingName = true, nameInput = props.list.name)">
        {{ list.name }}
      </h2>
      <div class="flex gap-1">
        <button v-for="f in filters" :key="f.label" @click="status = f.value"
          class="text-xs px-3 py-1 rounded-full border"
          :class="status === f.value
            ? 'bg-primary text-white border-primary'
            : 'text-slate-500 border-slate-200 hover:border-slate-400'">
          {{ f.label }}
        </button>
      </div>
    </div>

    <p v-if="error" class="text-red-500 text-sm mb-3">{{ error }}</p>

    <div v-if="canEdit" class="mb-4">
      <AddTodoForm @submit="createTodo" />
    </div>

    <div class="flex-1 overflow-y-auto">
      <p v-if="todos.length === 0" class="text-slate-400 text-sm py-4">No todos here.</p>
      <template v-for="(todo, index) in sortedTodos" :key="todo.id">
        <div v-if="todo.isCompleted && !sortedTodos[index - 1]?.isCompleted"
          class="flex items-center gap-2 my-2">
          <div class="flex-1 h-px bg-slate-100"></div>
          <span class="text-xs text-slate-400">Completed</span>
          <div class="flex-1 h-px bg-slate-100"></div>
        </div>
        <TodoItem :todo="todo" :readonly="!canEdit" @toggle="toggleTodo" @delete="deleteTodo" @rename="renameTodo" />
      </template>
    </div>
  </div>
</template>
