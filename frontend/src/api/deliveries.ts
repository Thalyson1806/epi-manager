import api from './axios'

export interface DeliveryItem {
  id: string
  epiId: string
  epiName: string
  epiCode: string
  quantity: number
  nextReplacementDate: string
}

export interface Delivery {
  id: string
  employeeId: string
  employeeName: string
  employeeRegistration: string
  sectorName: string
  operatorId: string
  operatorName: string
  deliveryDate: string
  hasBiometricSignature: boolean
  notes?: string
  items: DeliveryItem[]
}

export interface BiometricIdentifyResult {
  identified: boolean
  employeeId?: string
  employeeName?: string
  registration?: string
  sectorName?: string
  position?: string
  photoUrl?: string
}

export interface Dashboard {
  todayDeliveries: number
  todayEmployeesAttended: number
  expiringEpisNext30Days: number
  activeEmployees: number
  recentDeliveries: {
    deliveryId: string
    employeeName: string
    sectorName: string
    deliveryDate: string
    itemsCount: number
  }[]
}

export const deliveriesApi = {
  getDashboard: () => api.get<Dashboard>('/deliveries/dashboard').then((r) => r.data),
  getByEmployee: (employeeId: string) =>
    api.get<Delivery[]>(`/deliveries/employee/${employeeId}`).then((r) => r.data),
  getById: (id: string) => api.get<Delivery>(`/deliveries/${id}`).then((r) => r.data),
  identify: (biometricSampleBase64: string) =>
    api.post<BiometricIdentifyResult>('/deliveries/identify', { biometricSampleBase64 }).then((r) => r.data),
  create: (data: {
    employeeId: string
    biometricSignatureBase64?: string
    notes?: string
    items: { epiId: string; quantity: number }[]
  }) => api.post<Delivery>('/deliveries', data).then((r) => r.data),
  exportPdf: (employeeId: string, startDate?: string, endDate?: string) => {
    const params = new URLSearchParams()
    if (startDate) params.set('startDate', startDate)
    if (endDate) params.set('endDate', endDate)
    return api.get(`/deliveries/employee/${employeeId}/pdf?${params}`, { responseType: 'blob' })
  },
}
