import api from './axios'

export interface Employee {
  id: string
  name: string
  cpf: string
  registration: string
  sectorId: string
  sectorName: string
  position: string
  workShift?: string
  admissionDate: string
  status: 'Active' | 'Inactive'
  hasBiometric: boolean
  photoUrl?: string
  createdAt: string
}

export interface CreateEmployee {
  name: string
  cpf: string
  registration: string
  sectorId: string
  position: string
  workShift?: string
  admissionDate: string
}

export const employeesApi = {
  getAll: () => api.get<Employee[]>('/employees').then((r) => r.data),
  getById: (id: string) => api.get<Employee>(`/employees/${id}`).then((r) => r.data),
  create: (data: CreateEmployee) => api.post<Employee>('/employees', data).then((r) => r.data),
  update: (id: string, data: CreateEmployee) => api.put<Employee>(`/employees/${id}`, data).then((r) => r.data),
  activate: (id: string) => api.post(`/employees/${id}/activate`),
  deactivate: (id: string) => api.post(`/employees/${id}/deactivate`),
  setBiometric: (id: string, templateBase64: string) =>
    api.post(`/employees/${id}/biometric`, { employeeId: id, templateBase64 }),
  delete: (id: string) => api.delete(`/employees/${id}`),
  clearBiometric: (id: string) => api.delete(`/employees/${id}/biometric`),
}
