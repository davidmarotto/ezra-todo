import { defineStore } from 'pinia'
import { authApi } from '../services/api'

export const useAuthStore = defineStore('auth', {
  state: () => ({
    token: localStorage.getItem('token') || null,
    user: JSON.parse(localStorage.getItem('user') || 'null')
  }),
  getters: {
    isAuthenticated: (state) => !!state.token
  },
  actions: {
    async login(email, password) {
      const response = await authApi.login(email, password)
      this._persist(response)
    },
    async register(email, password) {
      const response = await authApi.register(email, password)
      this._persist(response)
    },
    logout() {
      this.token = null
      this.user = null
      localStorage.removeItem('token')
      localStorage.removeItem('user')
    },
    _persist(response) {
      this.token = response.token
      this.user = { email: response.email }
      localStorage.setItem('token', response.token)
      localStorage.setItem('user', JSON.stringify(this.user))
    }
  }
})
