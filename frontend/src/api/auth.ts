import api from './axios'

export interface LoginResult {
  token: string
  name: string
  email: string
  role: string
  userId: string
}

export interface User {
  id: string
  name: string
  email: string
  role: number
  isActive: boolean
}

export const authApi = {
  login: (email: string, password: string) =>
    api.post<LoginResult>('/auth/login', { email, password }).then((r) => r.data),
  getUsers: () => api.get<User[]>('/auth/users').then((r) => r.data),
  createUser: (data: { name: string; email: string; password: string; role: number }) =>
    api.post<User>('/auth/users', data).then((r) => r.data),
}
