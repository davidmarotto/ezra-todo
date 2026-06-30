<script setup>
import { ref, watch } from 'vue'
import { todosApi } from '../services/api'
import TodoItem from './TodoItem.vue'
import AddTodoForm from './AddTodoForm.vue'

const props = defineProps({
  list: { type: Object, required: true }
})

const todos = ref([])
const status = ref(null)
const error = ref(null)
const canEdit = ['Owner', 'Editor'].includes(props.list.role)

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

async function deleteTodo(todo) {
  try {
    await todosApi.remove(props.list.id, todo.id)
    todos.value = todos.value.filter(t => t.id !== todo.id)
  } catch (e) {
    error.value = e.message
  }
}

const filters = [
  { label: 'All', value: null },
  { label: 'Active', value: 'active' },
  { label: 'Completed', value: 'completed' }
]
</script>

<template>
  <div class="flex flex-col h-full">
    <div class="flex items-center justify-between mb-4">
      <h2 class="text-lg font-semibold text-slate-800">{{ list.name }}</h2>
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

    <div class="flex-1 overflow-y-auto">
      <p v-if="todos.length === 0" class="text-slate-400 text-sm py-4">No todos here.</p>
      <TodoItem v-for="todo in todos" :key="todo.id" :todo="todo" :readonly="!canEdit"
        @toggle="toggleTodo" @delete="deleteTodo" />
    </div>

    <div v-if="canEdit" class="mt-4">
      <AddTodoForm @submit="createTodo" />
    </div>
  </div>
</template>
