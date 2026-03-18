import api from './axios'

export interface SmtpConfig {
  smtpHost: string
  smtpPort: number
  smtpUser: string
  smtpPassword: string
  smtpFromEmail: string
  smtpFromName: string
  smtpUseSsl: boolean
  alertEnabled: boolean
  alertHour: number
}

export interface OverdueItem {
  employeeId: string
  employeeName: string
  employeeRegistration: string
  employeeWorkShift?: string
  position: string
  sectorName: string
  sectorSupervisorEmail?: string
  epiId: string
  epiName: string
  epiCode: string
  nextReplacementDate: string
  daysOverdue: number
}

export const adminApi = {
  getConfig: () => api.get<SmtpConfig>('/admin/config').then(r => r.data),
  saveConfig: (data: SmtpConfig) => api.post('/admin/config', data),
  testEmail: () => api.post<{ message: string }>('/admin/config/test-email'),
  getPending: () => api.get<OverdueItem[]>('/admin/pending').then(r => r.data),
  sendAlerts: () => api.post<{ message: string }>('/admin/send-alerts'),
}
