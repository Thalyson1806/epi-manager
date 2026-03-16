import api from './axios'

export interface Epi {
  id: string
  name: string
  code: string
  description?: string
  validityDays: number
  type: string
  isActive: boolean
}

export const episApi = {
  getAll: () => api.get<Epi[]>('/epis').then((r) => r.data),
  getById: (id: string) => api.get<Epi>(`/epis/${id}`).then((r) => r.data),
  create: (data: Omit<Epi, 'id' | 'isActive'>) =>
    api.post<Epi>('/epis', data).then((r) => r.data),
  update: (id: string, data: Omit<Epi, 'id' | 'isActive'>) =>
    api.put<Epi>(`/epis/${id}`, data).then((r) => r.data),
}
