import { create } from 'zustand'

interface AuthUser {
  id: string
  name: string
  email: string
  role: string
}

interface AuthState {
  token: string | null
  user: AuthUser | null
  setAuth: (token: string, user: AuthUser) => void
  logout: () => void
}

const stored = localStorage.getItem('epi-auth')
const initial = stored ? JSON.parse(stored) : { token: null, user: null }

export const useAuthStore = create<AuthState>()((set) => ({
  token: initial.token,
  user: initial.user,
  setAuth: (token, user) => {
    localStorage.setItem('epi-auth', JSON.stringify({ token, user }))
    set({ token, user })
  },
  logout: () => {
    localStorage.removeItem('epi-auth')
    set({ token: null, user: null })
  },
}))
