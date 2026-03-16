import api from './axios'

export interface Sector {
  id: string
  name: string
  description?: string
  employeeCount: number
}

export const sectorsApi = {
  getAll: () => api.get<Sector[]>('/sectors').then((r) => r.data),
  getById: (id: string) => api.get<Sector>(`/sectors/${id}`).then((r) => r.data),
  create: (data: { name: string; description?: string }) =>
    api.post<Sector>('/sectors', data).then((r) => r.data),
  update: (id: string, data: { name: string; description?: string }) =>
    api.put<Sector>(`/sectors/${id}`, data).then((r) => r.data),
  delete: (id: string) => api.delete(`/sectors/${id}`),
}
