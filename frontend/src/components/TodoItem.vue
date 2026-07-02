<script setup>
import { ref } from 'vue'

const props = defineProps({
  todo: { type: Object, required: true },
  readonly: { type: Boolean, default: false }
})

const emit = defineEmits(['toggle', 'delete', 'rename'])

const editing = ref(false)
const titleInput = ref('')

function startEditing() {
  if (props.readonly || props.todo.isCompleted) return
  editing.value = true
  titleInput.value = props.todo.title
}

function save() {
  if (!titleInput.value.trim() || titleInput.value.trim() === props.todo.title) {
    editing.value = false
    return
  }
  emit('rename', { todo: props.todo, title: titleInput.value.trim() })
  editing.value = false
}
</script>

<template>
  <div class="flex items-center gap-3 py-3 border-b border-slate-100 group">
    <input type="checkbox" :checked="todo.isCompleted" :disabled="readonly"
      @change="emit('toggle', todo)"
      class="w-4 h-4 accent-primary cursor-pointer" />
    <div class="flex-1 min-w-0">
      <input v-if="editing" v-model="titleInput"
        @keyup.enter="save" @keyup.esc="editing = false" @blur="save"
        class="text-sm text-slate-800 w-full bg-transparent border-b border-primary focus:outline-none" />
      <span v-else class="text-sm text-slate-800 block truncate"
        :class="{ 'line-through text-slate-400': todo.isCompleted, 'cursor-pointer hover:text-primary': !readonly && !todo.isCompleted }"
        @click="startEditing">
        {{ todo.title }}
      </span>
      <span v-if="todo.dueDate" class="text-xs text-slate-400">
        Due {{ new Date(todo.dueDate).toLocaleDateString() }}
      </span>
    </div>
    <button v-if="!readonly" @click="emit('delete', todo)"
      class="text-slate-300 hover:text-red-400 opacity-0 group-hover:opacity-100 text-lg leading-none">
      ×
    </button>
  </div>
</template>
