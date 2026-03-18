import { useState } from 'react'
import { useQuery, useMutation } from '@tanstack/react-query'
import {
  Box, Button, Card, CardContent, Typography, Chip,
  Table, TableBody, TableCell, TableContainer, TableHead, TableRow,
  CircularProgress, Alert,
} from '@mui/material'
import { Send, WarningAmber } from '@mui/icons-material'
import { adminApi, type OverdueItem } from '../api/admin'

export default function PendingPage() {
  const [sendSuccess, setSendSuccess] = useState<string | null>(null)
  const [sendError, setSendError] = useState<string | null>(null)

  const { data: items = [], isLoading } = useQuery({
    queryKey: ['pending-epis'],
    queryFn: adminApi.getPending,
  })

  const sendAlertsMutation = useMutation({
    mutationFn: adminApi.sendAlerts,
    onSuccess: (res) => {
      setSendSuccess((res.data as any)?.message ?? 'Alertas enviados!')
      setSendError(null)
      setTimeout(() => setSendSuccess(null), 5000)
    },
    onError: (err: any) => {
      setSendError(err?.response?.data?.message ?? 'Erro ao enviar alertas.')
      setSendSuccess(null)
    },
  })

  // Group by sector
  const bySector = items.reduce<Record<string, OverdueItem[]>>((acc, item) => {
    const key = item.sectorName || 'Sem setor'
    if (!acc[key]) acc[key] = []
    acc[key].push(item)
    return acc
  }, {})

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <WarningAmber color="warning" />
          <Typography variant="h5">EPIs Pendentes de Troca</Typography>
          {!isLoading && (
            <Chip
              label={`${items.length} item${items.length !== 1 ? 's' : ''}`}
              color={items.length > 0 ? 'error' : 'default'}
              size="small"
            />
          )}
        </Box>
        <Button
          variant="contained"
          color="warning"
          startIcon={<Send />}
          disabled={sendAlertsMutation.isPending || items.length === 0}
          onClick={() => sendAlertsMutation.mutate()}
        >
          Enviar alertas por e-mail
        </Button>
      </Box>

      {sendSuccess && <Alert severity="success" sx={{ mb: 2 }}>{sendSuccess}</Alert>}
      {sendError && <Alert severity="error" sx={{ mb: 2 }}>{sendError}</Alert>}

      {isLoading ? (
        <CircularProgress />
      ) : items.length === 0 ? (
        <Card>
          <CardContent>
            <Typography color="text.secondary" textAlign="center" py={4}>
              Nenhum EPI com prazo vencido encontrado.
            </Typography>
          </CardContent>
        </Card>
      ) : (
        Object.entries(bySector).map(([sectorName, sectorItems]) => (
          <Card key={sectorName} sx={{ mb: 3 }}>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
                <Typography variant="h6" color="error.main">{sectorName}</Typography>
                <Chip label={`${sectorItems.length} EPI${sectorItems.length !== 1 ? 's' : ''}`} color="error" size="small" />
              </Box>
              <TableContainer>
                <Table size="small">
                  <TableHead>
                    <TableRow>
                      <TableCell>Funcionário</TableCell>
                      <TableCell>Matrícula</TableCell>
                      <TableCell>Turno</TableCell>
                      <TableCell>Função</TableCell>
                      <TableCell>EPI</TableCell>
                      <TableCell>Venceu em</TableCell>
                      <TableCell align="center">Dias em atraso</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {sectorItems.map((item, idx) => (
                      <TableRow key={`${item.employeeId}-${item.epiId}-${idx}`} hover>
                        <TableCell sx={{ fontWeight: 600 }}>{item.employeeName}</TableCell>
                        <TableCell>{item.employeeRegistration}</TableCell>
                        <TableCell>{item.employeeWorkShift ?? '-'}</TableCell>
                        <TableCell>{item.position}</TableCell>
                        <TableCell>{item.epiName} <Typography variant="caption" color="text.secondary">({item.epiCode})</Typography></TableCell>
                        <TableCell>
                          {new Date(item.nextReplacementDate).toLocaleDateString('pt-BR')}
                        </TableCell>
                        <TableCell align="center">
                          <Chip
                            label={`${item.daysOverdue}d`}
                            color="error"
                            size="small"
                            sx={{ fontWeight: 700 }}
                          />
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </TableContainer>
            </CardContent>
          </Card>
        ))
      )}
    </Box>
  )
}
