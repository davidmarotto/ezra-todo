import { useAuthStore } from '../stores/auth'

const BASE_URL = 'http://localhost:5049'

async function request(path, options = {}) {
  const auth = useAuthStore()

  const response = await fetch(`${BASE_URL}${path}`, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      ...(auth.token ? { Authorization: `Bearer ${auth.token}` } : {}),
      ...options.headers
    }
  })

  if (!response.ok) {
    const error = await response.json().catch(() => ({ title: 'An unexpected error occurred.' }))
    throw { status: response.status, message: error.title }
  }

  if (response.status === 204) return null
  return response.json()
}

export const authApi = {
  register: (email, password) =>
    request('/auth/register', { method: 'POST', body: JSON.stringify({ email, password }) }),
  login: (email, password) =>
    request('/auth/login', { method: 'POST', body: JSON.stringify({ email, password }) })
}

export const listsApi = {
  getAll: () => request('/lists'),
  getOne: (id) => request(`/lists/${id}`),
  create: (name) => request('/lists', { method: 'POST', body: JSON.stringify({ name }) }),
  update: (id, name) => request(`/lists/${id}`, { method: 'PUT', body: JSON.stringify({ name }) }),
  remove: (id) => request(`/lists/${id}`, { method: 'DELETE' })
}

export const todosApi = {
  getAll: (listId, status) => request(`/lists/${listId}/todos${status ? `?status=${status}` : ''}`),
  getOne: (listId, id) => request(`/lists/${listId}/todos/${id}`),
  create: (listId, data) => request(`/lists/${listId}/todos`, { method: 'POST', body: JSON.stringify(data) }),
  update: (listId, id, data) => request(`/lists/${listId}/todos/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
  remove: (listId, id) => request(`/lists/${listId}/todos/${id}`, { method: 'DELETE' })
}
