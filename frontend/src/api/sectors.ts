import api from './axios'

export interface Sector {
  id: string
  name: string
  description?: string
  employeeCount: number
}

export interface SectorEpi {
  id: string
  sectorId: string
  sectorName: string
  epiId: string
  epiName: string
  isRequired: boolean
  replacementPeriodDays: number
  maxQuantityAllowed: number
}

export const sectorsApi = {
  getAll: () => api.get<Sector[]>('/sectors').then((r) => r.data),
  getById: (id: string) => api.get<Sector>(`/sectors/${id}`).then((r) => r.data),
  create: (data: { name: string; description?: string }) =>
    api.post<Sector>('/sectors', data).then((r) => r.data),
  update: (id: string, data: { name: string; description?: string }) =>
    api.put<Sector>(`/sectors/${id}`, data).then((r) => r.data),
  delete: (id: string) => api.delete(`/sectors/${id}`),

  getEpis: (sectorId: string) =>
    api.get<SectorEpi[]>(`/sectors/${sectorId}/epis`).then((r) => r.data),
  addEpi: (sectorId: string, data: {
    epiId: string; isRequired: boolean; replacementPeriodDays: number; maxQuantityAllowed: number
  }) => api.post<SectorEpi>(`/sectors/${sectorId}/epis`, { sectorId, ...data }).then((r) => r.data),
  updateEpi: (sectorEpiId: string, data: {
    isRequired: boolean; replacementPeriodDays: number; maxQuantityAllowed: number
  }) => api.put<SectorEpi>(`/sectors/epis/${sectorEpiId}`, data).then((r) => r.data),
  removeEpi: (sectorEpiId: string) => api.delete(`/sectors/epis/${sectorEpiId}`),

  getSuggestedEpis: (employeeId: string) =>
    api.get<SectorEpi[]>(`/sectors/suggested-epis/${employeeId}`).then((r) => r.data),
}
