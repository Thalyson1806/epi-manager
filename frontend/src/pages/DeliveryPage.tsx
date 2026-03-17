import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import {
  Box, Stepper, Step, StepLabel, Card, CardContent, Typography,
  Button, CircularProgress, Alert, Chip, Grid,
  Table, TableBody, TableCell, TableRow, IconButton, Divider,
  Paper, LinearProgress, Switch, FormControlLabel, TextField, Tooltip,
} from '@mui/material'
import { Fingerprint, Add, Remove, CheckCircle, Send, WifiOff, Warning } from '@mui/icons-material'
import { deliveriesApi, type BiometricIdentifyResult } from '../api/deliveries'
import { episApi, type Epi } from '../api/epis'
import { employeesApi } from '../api/employees'
import { sectorsApi } from '../api/sectors'
import { useBiometric } from '../hooks/useBiometric'

const STEPS = ['Identificação Biométrica', 'Seleção de EPIs', 'Assinatura Biométrica', 'Confirmação']

interface SelectedEpi {
  epi: Epi
  quantity: number
  isEarlyReplacement: boolean
  earlyReplacementReason: string
}

export default function DeliveryPage() {
  const qc = useQueryClient()
  const bio = useBiometric()

  const [activeStep, setActiveStep] = useState(0)
  const [identifiedEmployee, setIdentifiedEmployee] = useState<BiometricIdentifyResult | null>(null)
  const [selectedEpis, setSelectedEpis] = useState<SelectedEpi[]>([])
  const [biometricSignature, setBiometricSignature] = useState<string | null>(null)
  const [notes, setNotes] = useState('')
  const [scanning, setScanning] = useState(false)
  const [progress, setProgress] = useState('')
  const [error, setError] = useState('')
  const [success, setSuccess] = useState(false)

  const { data: epis = [] } = useQuery({ queryKey: ['epis'], queryFn: episApi.getAll })

  const { data: suggestedEpis = [] } = useQuery({
    queryKey: ['suggestedEpis', identifiedEmployee?.employeeId],
    queryFn: () => sectorsApi.getSuggestedEpis(identifiedEmployee!.employeeId!),
    enabled: !!identifiedEmployee?.employeeId,
  })

  const deliveryMutation = useMutation({
    mutationFn: deliveriesApi.create,
    onSuccess: () => { qc.invalidateQueries({ queryKey: ['dashboard'] }); setSuccess(true) },
    onError: () => setError('Erro ao registrar entrega.'),
  })

  const handleIdentify = async () => {
    setScanning(true); setError('')
    setProgress('Conectando ao leitor biométrico...')
    try {
      const status = await bio.checkStatus()
      if (!status.online) {
        setError('Biometric Bridge não encontrado. Verifique se o serviço está rodando na máquina com o leitor DigitalPersona conectado (ws://localhost:7001).')
        return
      }
      if (!status.readerConnected) { setError('Leitor DigitalPersona não detectado. Verifique a conexão USB.'); return }
      const employeeId = await bio.captureIdentification((msg) => setProgress(msg))
      setProgress('Carregando dados do funcionário...')
      const emp = await employeesApi.getById(employeeId)
      setIdentifiedEmployee({ identified: true, employeeId: emp.id, employeeName: emp.name, registration: emp.registration, sectorName: emp.sectorName, position: emp.position, photoUrl: emp.photoUrl })
      setActiveStep(1)
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Erro desconhecido')
    } finally { setScanning(false); setProgress('') }
  }

  const handleSign = async () => {
    setScanning(true); setError('')
    setProgress('Solicitando assinatura biométrica...')
    try {
      const status = await bio.checkStatus()
      if (!status.online) { setError('Biometric Bridge offline.'); return }
      const signatureBase64 = await bio.captureVerification((msg) => setProgress(msg))
      setBiometricSignature(signatureBase64)
      setActiveStep(3)
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Erro ao capturar assinatura')
    } finally { setScanning(false); setProgress('') }
  }

  const addEpi = (epi: Epi) =>
    setSelectedEpis((prev) =>
      prev.find((s) => s.epi.id === epi.id)
        ? prev
        : [...prev, { epi, quantity: 1, isEarlyReplacement: false, earlyReplacementReason: '' }])

  const changeQty = (id: string, delta: number) =>
    setSelectedEpis((prev) => prev.map((s) => s.epi.id === id ? { ...s, quantity: Math.max(1, s.quantity + delta) } : s))

  const removeEpi = (id: string) => setSelectedEpis((prev) => prev.filter((s) => s.epi.id !== id))

  const toggleEarlyReplacement = (id: string, value: boolean) =>
    setSelectedEpis((prev) => prev.map((s) => s.epi.id === id ? { ...s, isEarlyReplacement: value, earlyReplacementReason: value ? s.earlyReplacementReason : '' } : s))

  const setReason = (id: string, reason: string) =>
    setSelectedEpis((prev) => prev.map((s) => s.epi.id === id ? { ...s, earlyReplacementReason: reason } : s))

  const handleConfirmDelivery = () => {
    if (!identifiedEmployee?.employeeId) return
    deliveryMutation.mutate({
      employeeId: identifiedEmployee.employeeId,
      biometricSignatureBase64: biometricSignature ?? undefined,
      notes: notes || undefined,
      items: selectedEpis.map((s) => ({ epiId: s.epi.id, quantity: s.quantity, isEarlyReplacement: s.isEarlyReplacement, earlyReplacementReason: s.earlyReplacementReason || undefined })),
    })
  }

  const handleReset = () => {
    setActiveStep(0); setIdentifiedEmployee(null); setSelectedEpis([])
    setBiometricSignature(null); setNotes(''); setScanning(false); setProgress(''); setError(''); setSuccess(false)
  }

  if (success) return (
    <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', mt: 8 }}>
      <CheckCircle sx={{ fontSize: 80, color: 'success.main', mb: 2 }} />
      <Typography variant="h5" fontWeight={700} sx={{ mb: 1 }}>Entrega Registrada com Sucesso!</Typography>
      <Typography color="text.secondary" sx={{ mb: 3 }}>{selectedEpis.length} EPI(s) entregue(s) para {identifiedEmployee?.employeeName}</Typography>
      <Button variant="contained" onClick={handleReset} size="large">Nova Entrega</Button>
    </Box>
  )

  return (
    <Box>
      <Typography variant="h5" sx={{ mb: 3 }}>Entrega de EPI</Typography>
      <Stepper activeStep={activeStep} sx={{ mb: 4 }}>
        {STEPS.map((label) => <Step key={label}><StepLabel>{label}</StepLabel></Step>)}
      </Stepper>

      {error && <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError('')} icon={error.includes('Bridge') || error.includes('leitor') ? <WifiOff /> : undefined}>{error}</Alert>}

      {activeStep === 0 && (
        <Card>
          <CardContent sx={{ textAlign: 'center', py: 6 }}>
            <Fingerprint sx={{ fontSize: 80, color: scanning ? 'primary.main' : 'grey.400', mb: 2 }} />
            <Typography variant="h6" sx={{ mb: 1 }}>Identificação Biométrica</Typography>
            <Typography color="text.secondary" sx={{ mb: 1 }}>Solicite ao funcionário que coloque o dedo no leitor DigitalPersona</Typography>
            {progress && <Box sx={{ mb: 2 }}><Typography variant="body2" color="primary" sx={{ mb: 1 }}>{progress}</Typography><LinearProgress /></Box>}
            <Box sx={{ display: 'flex', gap: 2, justifyContent: 'center' }}>
              <Button variant="contained" size="large" onClick={handleIdentify} disabled={scanning}
                startIcon={scanning ? <CircularProgress size={20} color="inherit" /> : <Fingerprint />}>
                {scanning ? 'Lendo digital...' : 'Iniciar Leitura Biométrica'}
              </Button>
              {scanning && <Button variant="outlined" onClick={bio.cancel}>Cancelar</Button>}
            </Box>
          </CardContent>
        </Card>
      )}

      {activeStep === 1 && identifiedEmployee && (
        <Grid container spacing={3}>
          <Grid item xs={12} md={4}>
            <Card sx={{ mb: 2 }}>
              <CardContent>
                <Typography variant="h6" sx={{ mb: 1 }}>Funcionário Identificado</Typography>
                <Typography fontWeight={600}>{identifiedEmployee.employeeName}</Typography>
                <Chip label={identifiedEmployee.registration} size="small" variant="outlined" sx={{ my: 0.5 }} />
                <Typography variant="body2" color="text.secondary">{identifiedEmployee.sectorName}</Typography>
                <Typography variant="body2">{identifiedEmployee.position}</Typography>
              </CardContent>
            </Card>
            <Card>
              <CardContent>
                <Typography variant="h6" sx={{ mb: 2 }}>EPIs Selecionados</Typography>
                {selectedEpis.length === 0
                  ? <Typography variant="body2" color="text.secondary">Nenhum EPI selecionado</Typography>
                  : selectedEpis.map((s) => (
                    <Box key={s.epi.id} sx={{ mb: 1.5, p: 1, border: '1px solid', borderColor: s.isEarlyReplacement ? 'warning.main' : 'divider', borderRadius: 1 }}>
                      <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                        <Typography variant="body2" fontWeight={500}>{s.epi.name}</Typography>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                          <IconButton size="small" onClick={() => changeQty(s.epi.id, -1)}><Remove fontSize="small" /></IconButton>
                          <Typography>{s.quantity}</Typography>
                          <IconButton size="small" onClick={() => changeQty(s.epi.id, 1)}><Add fontSize="small" /></IconButton>
                          <IconButton size="small" color="error" onClick={() => removeEpi(s.epi.id)}><Remove fontSize="small" /></IconButton>
                        </Box>
                      </Box>
                      <FormControlLabel
                        control={<Switch size="small" checked={s.isEarlyReplacement} onChange={(e) => toggleEarlyReplacement(s.epi.id, e.target.checked)} />}
                        label={<Typography variant="caption" color={s.isEarlyReplacement ? 'warning.main' : 'text.secondary'}><Warning fontSize="inherit" sx={{ mr: 0.5, verticalAlign: 'middle' }} />Troca antecipada</Typography>}
                      />
                      {s.isEarlyReplacement && (
                        <TextField size="small" fullWidth placeholder="Justificativa (ex: EPI rasgado, perdido)"
                          value={s.earlyReplacementReason} onChange={(e) => setReason(s.epi.id, e.target.value)}
                          error={!s.earlyReplacementReason} helperText={!s.earlyReplacementReason ? 'Justificativa obrigatória' : ''}
                          sx={{ mt: 0.5 }} />
                      )}
                    </Box>
                  ))
                }
                <TextField size="small" fullWidth multiline rows={2} label="Observações gerais (opcional)"
                  value={notes} onChange={(e) => setNotes(e.target.value)} sx={{ mt: 2, mb: 2 }} />
                <Divider sx={{ mb: 2 }} />
                <Button variant="contained" fullWidth
                  disabled={selectedEpis.length === 0 || selectedEpis.some((s) => s.isEarlyReplacement && !s.earlyReplacementReason)}
                  onClick={() => setActiveStep(2)}>
                  Prosseguir para Assinatura
                </Button>
              </CardContent>
            </Card>
          </Grid>

          <Grid item xs={12} md={8}>
            {suggestedEpis.length > 0 && (
              <Card sx={{ mb: 2 }}>
                <CardContent>
                  <Typography variant="h6" sx={{ mb: 0.5 }}>EPIs do Setor — {identifiedEmployee.sectorName}</Typography>
                  <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>EPIs obrigatórios e recomendados para este setor</Typography>
                  <Grid container spacing={1}>
                    {suggestedEpis.map((se) => {
                      const epi = epis.find((e) => e.id === se.epiId)
                      if (!epi) return null
                      const added = selectedEpis.some((s) => s.epi.id === epi.id)
                      return (
                        <Grid item xs={12} sm={6} key={se.id}>
                          <Tooltip title={se.isRequired ? 'Obrigatório para este setor' : 'Recomendado'}>
                            <Paper variant="outlined"
                              sx={{ p: 1.5, cursor: added ? 'default' : 'pointer', borderColor: se.isRequired ? 'error.main' : 'divider', bgcolor: added ? 'success.50' : 'inherit', '&:hover': added ? {} : { bgcolor: 'primary.50', borderColor: 'primary.main' } }}
                              onClick={() => !added && addEpi(epi)}>
                              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'start' }}>
                                <Typography variant="body2" fontWeight={500}>{epi.name}</Typography>
                                {se.isRequired && <Chip label="Obrigatório" size="small" color="error" sx={{ ml: 1 }} />}
                              </Box>
                              <Box sx={{ display: 'flex', gap: 0.5, mt: 0.5, flexWrap: 'wrap' }}>
                                <Chip label={epi.code} size="small" variant="outlined" />
                                <Chip label={`Troca: ${se.replacementPeriodDays}d`} size="small" color="info" variant="outlined" />
                              </Box>
                              {added && <Typography variant="caption" color="success.main">✓ Adicionado</Typography>}
                            </Paper>
                          </Tooltip>
                        </Grid>
                      )
                    })}
                  </Grid>
                </CardContent>
              </Card>
            )}
            <Card>
              <CardContent>
                <Typography variant="h6" sx={{ mb: 2 }}>Todos os EPIs Disponíveis</Typography>
                <Grid container spacing={1.5}>
                  {epis.filter((e) => e.isActive).map((epi) => (
                    <Grid item xs={12} sm={6} key={epi.id}>
                      <Paper variant="outlined"
                        sx={{ p: 1.5, cursor: 'pointer', '&:hover': { bgcolor: 'primary.50', borderColor: 'primary.main' } }}
                        onClick={() => addEpi(epi)}>
                        <Typography variant="body2" fontWeight={500}>{epi.name}</Typography>
                        <Box sx={{ display: 'flex', gap: 0.5, mt: 0.5 }}>
                          <Chip label={epi.code} size="small" variant="outlined" />
                          <Chip label={`${epi.validityDays}d`} size="small" color="info" variant="outlined" />
                        </Box>
                      </Paper>
                    </Grid>
                  ))}
                </Grid>
              </CardContent>
            </Card>
          </Grid>
        </Grid>
      )}

      {activeStep === 2 && (
        <Card>
          <CardContent sx={{ textAlign: 'center', py: 6 }}>
            <Fingerprint sx={{ fontSize: 80, color: scanning ? 'secondary.main' : 'grey.400', mb: 2 }} />
            <Typography variant="h6" sx={{ mb: 1 }}>Assinatura Biométrica</Typography>
            <Typography color="text.secondary" sx={{ mb: 3 }}>Solicite ao funcionário <strong>{identifiedEmployee?.employeeName}</strong> que coloque o dedo novamente para assinar digitalmente a ficha de EPI</Typography>
            {progress && <Box sx={{ mb: 2 }}><Typography variant="body2" color="secondary" sx={{ mb: 1 }}>{progress}</Typography><LinearProgress color="secondary" /></Box>}
            <Box sx={{ display: 'flex', gap: 2, justifyContent: 'center' }}>
              <Button variant="outlined" onClick={() => setActiveStep(1)} disabled={scanning}>Voltar</Button>
              <Button variant="contained" color="secondary" size="large" onClick={handleSign} disabled={scanning}
                startIcon={scanning ? <CircularProgress size={20} color="inherit" /> : <Fingerprint />}>
                {scanning ? 'Capturando assinatura...' : 'Capturar Assinatura Digital'}
              </Button>
              {scanning && <Button variant="outlined" onClick={bio.cancel}>Cancelar</Button>}
            </Box>
          </CardContent>
        </Card>
      )}

      {activeStep === 3 && (
        <Card>
          <CardContent>
            <Typography variant="h6" sx={{ mb: 3 }}>Confirmação da Entrega</Typography>
            <Grid container spacing={2} sx={{ mb: 3 }}>
              <Grid item xs={12} sm={6}>
                <Paper variant="outlined" sx={{ p: 2 }}>
                  <Typography variant="subtitle2" color="text.secondary">Funcionário</Typography>
                  <Typography fontWeight={600}>{identifiedEmployee?.employeeName}</Typography>
                  <Typography variant="body2">{identifiedEmployee?.sectorName} • {identifiedEmployee?.position}</Typography>
                </Paper>
              </Grid>
              <Grid item xs={12} sm={6}>
                <Paper variant="outlined" sx={{ p: 2 }}>
                  <Typography variant="subtitle2" color="text.secondary">Assinatura</Typography>
                  <Chip icon={<Fingerprint />} label="Biometria capturada" color="success" />
                </Paper>
              </Grid>
            </Grid>
            <Typography variant="subtitle2" sx={{ mb: 1 }}>EPIs a Entregar:</Typography>
            <Table size="small" sx={{ mb: 3 }}>
              <TableBody>
                {selectedEpis.map((s) => (
                  <TableRow key={s.epi.id}>
                    <TableCell>{s.epi.name}</TableCell>
                    <TableCell><Chip label={s.epi.code} size="small" variant="outlined" /></TableCell>
                    <TableCell>Qtd: <strong>{s.quantity}</strong></TableCell>
                    {s.isEarlyReplacement && <TableCell><Chip icon={<Warning />} label={`Troca antecipada: ${s.earlyReplacementReason}`} size="small" color="warning" /></TableCell>}
                  </TableRow>
                ))}
              </TableBody>
            </Table>
            {notes && <Alert severity="info" sx={{ mb: 2 }}>Obs: {notes}</Alert>}
            <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end' }}>
              <Button onClick={() => setActiveStep(2)}>Voltar</Button>
              <Button variant="contained" color="success" size="large"
                startIcon={deliveryMutation.isPending ? <CircularProgress size={20} color="inherit" /> : <Send />}
                onClick={handleConfirmDelivery} disabled={deliveryMutation.isPending}>
                Confirmar Entrega
              </Button>
            </Box>
          </CardContent>
        </Card>
      )}
    </Box>
  )
}
