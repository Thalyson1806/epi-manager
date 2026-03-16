import { useState } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import {
  Box, Button, Card, CardContent, Typography, Chip, Grid,
  Table, TableBody, TableCell, TableContainer, TableHead, TableRow,
  CircularProgress, Divider, TextField, Dialog, DialogTitle,
  DialogContent, DialogActions, LinearProgress, Alert,
} from '@mui/material'
import { ArrowBack, PictureAsPdf, Fingerprint } from '@mui/icons-material'
import { employeesApi } from '../api/employees'
import { deliveriesApi } from '../api/deliveries'
import { useBiometric } from '../hooks/useBiometric'
import { format } from 'date-fns'
import { ptBR } from 'date-fns/locale'
import api from '../api/axios'

export default function EmployeeDetailPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const qc = useQueryClient()
  const bio = useBiometric()

  const [startDate, setStartDate] = useState('')
  const [endDate, setEndDate] = useState('')
  const [exportLoading, setExportLoading] = useState(false)

  // Estado do modal de cadastro biométrico
  const [bioDialogOpen, setBioDialogOpen] = useState(false)
  const [bioProgress, setBioProgress] = useState('')
  const [bioScanning, setBioScanning] = useState(false)
  const [bioError, setBioError] = useState('')

  const { data: emp, isLoading } = useQuery({
    queryKey: ['employee', id],
    queryFn: () => employeesApi.getById(id!),
    enabled: !!id,
  })

  const { data: deliveries = [] } = useQuery({
    queryKey: ['deliveries', 'employee', id],
    queryFn: () => deliveriesApi.getByEmployee(id!),
    enabled: !!id,
  })

  const handleExportPdf = async () => {
    setExportLoading(true)
    try {
      const response = await deliveriesApi.exportPdf(id!, startDate || undefined, endDate || undefined)
      const blob = new Blob([response.data], { type: 'application/pdf' })
      const url = URL.createObjectURL(blob)
      const a = document.createElement('a')
      a.href = url
      a.download = `ficha-epi-${emp?.name ?? id}.pdf`
      a.click()
      URL.revokeObjectURL(url)
    } finally {
      setExportLoading(false)
    }
  }

  // --------------------------------------------------------------------------
  // Cadastro biométrico: abre modal, chama bridge para enrollment (4 amostras)
  // Ao concluir, salva o template via POST /api/employees/{id}/biometric
  // --------------------------------------------------------------------------
  const handleBioEnroll = async () => {
    setBioScanning(true)
    setBioError('')
    setBioProgress('Verificando leitor biométrico...')

    try {
      // Verifica status do bridge antes de iniciar
      const status = await bio.checkStatus()
      if (!status.online) {
        setBioError('Biometric Bridge offline. Inicie o serviço biometric-bridge na máquina com o leitor.')
        setBioScanning(false)
        return
      }
      if (!status.readerConnected) {
        setBioError('Leitor DigitalPersona não detectado. Verifique a conexão USB.')
        setBioScanning(false)
        return
      }

      // Inicia enrollment: captura 4 amostras e gera template
      setBioProgress('Coloque o dedo no leitor...')
      const templateBase64 = await bio.captureEnrollment((msg) => setBioProgress(msg))

      // Salva template no banco via API
      setBioProgress('Salvando template biométrico...')
      await api.post(`/employees/${id}/biometric`, {
        employeeId: id,
        templateBase64,
      })

      qc.invalidateQueries({ queryKey: ['employee', id] })
      setBioDialogOpen(false)
      setBioProgress('')
    } catch (err: unknown) {
      setBioError(err instanceof Error ? err.message : 'Erro ao cadastrar biometria')
    } finally {
      setBioScanning(false)
    }
  }

  if (isLoading) return <CircularProgress />

  return (
    <Box>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 3 }}>
        <Button startIcon={<ArrowBack />} onClick={() => navigate('/employees')}>Voltar</Button>
        <Typography variant="h5">Ficha de EPI — {emp?.name}</Typography>
      </Box>

      <Grid container spacing={3}>
        <Grid item xs={12} md={4}>
          <Card>
            <CardContent>
              <Typography variant="h6" sx={{ mb: 2 }}>Dados do Funcionário</Typography>
              <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1.5 }}>
                {[
                  ['Nome', emp?.name],
                  ['Matrícula', emp?.registration],
                  ['CPF', emp?.cpf],
                  ['Setor', emp?.sectorName],
                  ['Cargo', emp?.position],
                  ['Admissão', emp?.admissionDate ? format(new Date(emp.admissionDate), 'dd/MM/yyyy', { locale: ptBR }) : '-'],
                ].map(([label, value]) => (
                  <Box key={label}>
                    <Typography variant="caption" color="text.secondary">{label}</Typography>
                    <Typography variant="body2" fontWeight={500}>{value}</Typography>
                  </Box>
                ))}
                <Divider />
                <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}>
                  <Chip
                    label={emp?.status === 1 ? 'Ativo' : 'Inativo'}
                    color={emp?.status === 1 ? 'success' : 'error'}
                    size="small"
                  />
                  <Chip
                    icon={<Fingerprint />}
                    label={emp?.hasBiometric ? 'Biometria OK' : 'Sem Biometria'}
                    color={emp?.hasBiometric ? 'success' : 'warning'}
                    size="small"
                    variant="outlined"
                  />
                </Box>
                {/* Botão de cadastro biométrico */}
                <Button
                  variant={emp?.hasBiometric ? 'outlined' : 'contained'}
                  color={emp?.hasBiometric ? 'inherit' : 'primary'}
                  startIcon={<Fingerprint />}
                  onClick={() => { setBioError(''); setBioProgress(''); setBioDialogOpen(true) }}
                  size="small"
                >
                  {emp?.hasBiometric ? 'Recadastrar Biometria' : 'Cadastrar Biometria'}
                </Button>
              </Box>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} md={8}>
          <Card>
            <CardContent>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2, flexWrap: 'wrap', gap: 1 }}>
                <Typography variant="h6">Histórico de Entregas</Typography>
                <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap', alignItems: 'center' }}>
                  <TextField type="date" label="De" size="small" value={startDate} onChange={(e) => setStartDate(e.target.value)} InputLabelProps={{ shrink: true }} sx={{ width: 150 }} />
                  <TextField type="date" label="Até" size="small" value={endDate} onChange={(e) => setEndDate(e.target.value)} InputLabelProps={{ shrink: true }} sx={{ width: 150 }} />
                  <Button variant="outlined" startIcon={<PictureAsPdf />} onClick={handleExportPdf} disabled={exportLoading}>
                    Exportar PDF
                  </Button>
                </Box>
              </Box>

              <TableContainer>
                <Table size="small">
                  <TableHead>
                    <TableRow>
                      <TableCell>Data</TableCell>
                      <TableCell>EPI</TableCell>
                      <TableCell align="center">Qtd</TableCell>
                      <TableCell>Próx. Troca</TableCell>
                      <TableCell align="center">Assinatura</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {deliveries.flatMap((d) =>
                      d.items.map((item) => (
                        <TableRow key={item.id} hover>
                          <TableCell>{format(new Date(d.deliveryDate), 'dd/MM/yyyy HH:mm', { locale: ptBR })}</TableCell>
                          <TableCell>
                            <Typography variant="body2">{item.epiName}</Typography>
                            <Typography variant="caption" color="text.secondary">{item.epiCode}</Typography>
                          </TableCell>
                          <TableCell align="center">{item.quantity}</TableCell>
                          <TableCell>{format(new Date(item.nextReplacementDate), 'dd/MM/yyyy', { locale: ptBR })}</TableCell>
                          <TableCell align="center">
                            <Chip
                              icon={<Fingerprint />}
                              label={d.hasBiometricSignature ? 'Biometria' : 'Manual'}
                              size="small"
                              color={d.hasBiometricSignature ? 'success' : 'default'}
                              variant="outlined"
                            />
                          </TableCell>
                        </TableRow>
                      )),
                    )}
                    {deliveries.length === 0 && (
                      <TableRow>
                        <TableCell colSpan={5} align="center" sx={{ py: 3, color: 'text.secondary' }}>
                          Nenhuma entrega registrada
                        </TableCell>
                      </TableRow>
                    )}
                  </TableBody>
                </Table>
              </TableContainer>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Modal de Cadastro Biométrico */}
      <Dialog open={bioDialogOpen} onClose={() => !bioScanning && setBioDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <Fingerprint color="primary" />
          Cadastro Biométrico — {emp?.name}
        </DialogTitle>
        <DialogContent>
          <Box sx={{ textAlign: 'center', py: 3 }}>
            <Fingerprint
              sx={{
                fontSize: 72,
                color: bioScanning ? 'primary.main' : 'grey.400',
                mb: 2,
                transition: 'color 0.5s',
              }}
            />
            <Typography variant="body1" sx={{ mb: 1 }}>
              O funcionário deve colocar o dedo no leitor <strong>4 vezes</strong>
            </Typography>
            <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mb: 2 }}>
              O Biometric Bridge deve estar rodando na máquina com o leitor conectado
            </Typography>

            {bioError && <Alert severity="error" sx={{ mb: 2, textAlign: 'left' }}>{bioError}</Alert>}

            {bioProgress && (
              <Box sx={{ mb: 2 }}>
                <Typography variant="body2" color="primary" sx={{ mb: 1 }}>{bioProgress}</Typography>
                <LinearProgress />
              </Box>
            )}
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => { bio.cancel(); setBioDialogOpen(false) }} disabled={false}>
            Cancelar
          </Button>
          <Button
            variant="contained"
            onClick={handleBioEnroll}
            disabled={bioScanning}
            startIcon={bioScanning ? <CircularProgress size={18} color="inherit" /> : <Fingerprint />}
          >
            {bioScanning ? 'Cadastrando...' : 'Iniciar Cadastro'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  )
}
