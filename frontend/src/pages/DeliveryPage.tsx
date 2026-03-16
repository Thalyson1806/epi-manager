import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import {
  Box, Stepper, Step, StepLabel, Card, CardContent, Typography,
  Button, CircularProgress, Alert, Chip, Grid,
  Table, TableBody, TableCell, TableRow, IconButton, Divider,
  Paper, LinearProgress,
} from '@mui/material'
import { Fingerprint, Add, Remove, CheckCircle, Send, WifiOff } from '@mui/icons-material'
import { deliveriesApi, type BiometricIdentifyResult } from '../api/deliveries'
import { episApi, type Epi } from '../api/epis'
import { useBiometric } from '../hooks/useBiometric'

const STEPS = ['Identificação Biométrica', 'Seleção de EPIs', 'Assinatura Biométrica', 'Confirmação']

interface SelectedEpi {
  epi: Epi
  quantity: number
}

export default function DeliveryPage() {
  const qc = useQueryClient()

  // Hook de biometria — gerencia WebSocket com o bridge local
  const bio = useBiometric()

  const [activeStep, setActiveStep] = useState(0)
  const [identifiedEmployee, setIdentifiedEmployee] = useState<BiometricIdentifyResult | null>(null)
  const [selectedEpis, setSelectedEpis] = useState<SelectedEpi[]>([])
  const [biometricSignature, setBiometricSignature] = useState<string | null>(null)
  const [scanning, setScanning] = useState(false)
  const [progress, setProgress] = useState('')
  const [error, setError] = useState('')
  const [success, setSuccess] = useState(false)

  const { data: epis = [] } = useQuery({ queryKey: ['epis'], queryFn: episApi.getAll })

  const deliveryMutation = useMutation({
    mutationFn: deliveriesApi.create,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['dashboard'] })
      setSuccess(true)
    },
    onError: () => setError('Erro ao registrar entrega.'),
  })

  // ----------------------------------------------------------------------------
  // PASSO 1: Identificação biométrica
  // Chama o bridge → captura 1 amostra → envia ao backend → identifica funcionário
  // ----------------------------------------------------------------------------
  const handleIdentify = async () => {
    setScanning(true)
    setError('')
    setProgress('Conectando ao leitor biométrico...')

    try {
      // Verifica se o bridge está online antes de tentar
      const status = await bio.checkStatus()
      if (!status.online) {
        setError(
          'Biometric Bridge não encontrado. Verifique se o serviço está rodando na máquina com o leitor DigitalPersona conectado (ws://localhost:7001).',
        )
        setScanning(false)
        setProgress('')
        return
      }

      if (!status.readerConnected) {
        setError('Leitor DigitalPersona não detectado. Verifique a conexão USB.')
        setScanning(false)
        setProgress('')
        return
      }

      // Captura amostra via bridge WebSocket
      // O usuário deve colocar o dedo no leitor
      const sampleBase64 = await bio.captureVerification((msg) => setProgress(msg))

      // Envia amostra ao backend para identificação 1:N
      setProgress('Identificando funcionário...')
      const result = await deliveriesApi.identify(sampleBase64)

      if (result.identified) {
        setIdentifiedEmployee(result)
        setActiveStep(1)
        setProgress('')
      } else {
        setError('Funcionário não identificado. Verifique o cadastro biométrico ou tente novamente.')
      }
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Erro desconhecido'
      setError(message)
    } finally {
      setScanning(false)
      setProgress('')
    }
  }

  // ----------------------------------------------------------------------------
  // PASSO 3: Assinatura biométrica
  // Segunda leitura do dedo — funciona como assinatura digital da ficha
  // O template capturado fica registrado no registro de entrega (biometric_signature)
  // ----------------------------------------------------------------------------
  const handleSign = async () => {
    setScanning(true)
    setError('')
    setProgress('Solicitando assinatura biométrica...')

    try {
      const status = await bio.checkStatus()
      if (!status.online) {
        setError('Biometric Bridge offline. Verifique o serviço.')
        return
      }

      // Captura amostra para usar como assinatura
      const signatureBase64 = await bio.captureVerification((msg) => setProgress(msg))

      setBiometricSignature(signatureBase64)
      setActiveStep(3)
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Erro ao capturar assinatura'
      setError(message)
    } finally {
      setScanning(false)
      setProgress('')
    }
  }

  const handleAddEpi = (epi: Epi) => {
    setSelectedEpis((prev) => {
      const existing = prev.find((s) => s.epi.id === epi.id)
      if (existing) return prev.map((s) => s.epi.id === epi.id ? { ...s, quantity: s.quantity + 1 } : s)
      return [...prev, { epi, quantity: 1 }]
    })
  }

  const handleChangeQuantity = (epiId: string, delta: number) => {
    setSelectedEpis((prev) =>
      prev.map((s) => s.epi.id === epiId ? { ...s, quantity: Math.max(1, s.quantity + delta) } : s),
    )
  }

  const handleRemoveEpi = (epiId: string) => {
    setSelectedEpis((prev) => prev.filter((s) => s.epi.id !== epiId))
  }

  const handleConfirmDelivery = () => {
    if (!identifiedEmployee?.employeeId) return
    deliveryMutation.mutate({
      employeeId: identifiedEmployee.employeeId,
      biometricSignatureBase64: biometricSignature ?? undefined,
      items: selectedEpis.map((s) => ({ epiId: s.epi.id, quantity: s.quantity })),
    })
  }

  const handleReset = () => {
    setActiveStep(0)
    setIdentifiedEmployee(null)
    setSelectedEpis([])
    setBiometricSignature(null)
    setScanning(false)
    setProgress('')
    setError('')
    setSuccess(false)
  }

  if (success) {
    return (
      <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', mt: 8 }}>
        <CheckCircle sx={{ fontSize: 80, color: 'success.main', mb: 2 }} />
        <Typography variant="h5" fontWeight={700} sx={{ mb: 1 }}>Entrega Registrada com Sucesso!</Typography>
        <Typography color="text.secondary" sx={{ mb: 3 }}>
          {selectedEpis.length} EPI(s) entregue(s) para {identifiedEmployee?.employeeName}
        </Typography>
        <Button variant="contained" onClick={handleReset} size="large">Nova Entrega</Button>
      </Box>
    )
  }

  return (
    <Box>
      <Typography variant="h5" sx={{ mb: 3 }}>Entrega de EPI</Typography>

      <Stepper activeStep={activeStep} sx={{ mb: 4 }}>
        {STEPS.map((label) => (
          <Step key={label}><StepLabel>{label}</StepLabel></Step>
        ))}
      </Stepper>

      {error && (
        <Alert
          severity="error"
          sx={{ mb: 2 }}
          onClose={() => setError('')}
          icon={error.includes('Bridge') || error.includes('leitor') ? <WifiOff /> : undefined}
        >
          {error}
        </Alert>
      )}

      {/* PASSO 0: Identificação Biométrica */}
      {activeStep === 0 && (
        <Card>
          <CardContent sx={{ textAlign: 'center', py: 6 }}>
            <Fingerprint
              sx={{
                fontSize: 80,
                color: scanning ? 'primary.main' : 'grey.400',
                mb: 2,
                transition: 'color 0.3s',
                animation: scanning ? 'pulse 1.5s infinite' : 'none',
              }}
            />
            <Typography variant="h6" sx={{ mb: 1 }}>Identificação Biométrica</Typography>
            <Typography color="text.secondary" sx={{ mb: 1 }}>
              Solicite ao funcionário que coloque o dedo no leitor DigitalPersona
            </Typography>
            <Typography variant="caption" color="text.secondary" sx={{ mb: 3, display: 'block' }}>
              O serviço bridge deve estar rodando na máquina com o leitor conectado
            </Typography>

            {progress && (
              <Box sx={{ mb: 2 }}>
                <Typography variant="body2" color="primary" sx={{ mb: 1 }}>{progress}</Typography>
                <LinearProgress />
              </Box>
            )}

            <Box sx={{ display: 'flex', gap: 2, justifyContent: 'center' }}>
              <Button
                variant="contained"
                size="large"
                onClick={handleIdentify}
                disabled={scanning}
                startIcon={scanning ? <CircularProgress size={20} color="inherit" /> : <Fingerprint />}
              >
                {scanning ? 'Lendo digital...' : 'Iniciar Leitura Biométrica'}
              </Button>
              {scanning && (
                <Button variant="outlined" onClick={bio.cancel}>Cancelar</Button>
              )}
            </Box>
          </CardContent>
        </Card>
      )}

      {/* PASSO 1: Seleção de EPIs */}
      {activeStep === 1 && identifiedEmployee && (
        <Grid container spacing={3}>
          <Grid item xs={12} md={4}>
            <Card sx={{ mb: 2 }}>
              <CardContent>
                <Typography variant="h6" sx={{ mb: 2 }}>Funcionário Identificado</Typography>
                <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
                  <Typography fontWeight={600}>{identifiedEmployee.employeeName}</Typography>
                  <Chip label={identifiedEmployee.registration} size="small" variant="outlined" />
                  <Typography variant="body2" color="text.secondary">{identifiedEmployee.sectorName}</Typography>
                  <Typography variant="body2">{identifiedEmployee.position}</Typography>
                </Box>
              </CardContent>
            </Card>

            <Card>
              <CardContent>
                <Typography variant="h6" sx={{ mb: 2 }}>EPIs Selecionados</Typography>
                {selectedEpis.length === 0 ? (
                  <Typography variant="body2" color="text.secondary">Nenhum EPI selecionado</Typography>
                ) : (
                  <Table size="small">
                    <TableBody>
                      {selectedEpis.map((s) => (
                        <TableRow key={s.epi.id}>
                          <TableCell sx={{ pl: 0 }}>
                            <Typography variant="body2">{s.epi.name}</Typography>
                          </TableCell>
                          <TableCell>
                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                              <IconButton size="small" onClick={() => handleChangeQuantity(s.epi.id, -1)}><Remove fontSize="small" /></IconButton>
                              <Typography>{s.quantity}</Typography>
                              <IconButton size="small" onClick={() => handleChangeQuantity(s.epi.id, 1)}><Add fontSize="small" /></IconButton>
                            </Box>
                          </TableCell>
                          <TableCell>
                            <IconButton size="small" color="error" onClick={() => handleRemoveEpi(s.epi.id)}>
                              <Remove fontSize="small" />
                            </IconButton>
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                )}
                <Divider sx={{ my: 2 }} />
                <Button variant="contained" fullWidth disabled={selectedEpis.length === 0} onClick={() => setActiveStep(2)}>
                  Prosseguir para Assinatura
                </Button>
              </CardContent>
            </Card>
          </Grid>

          <Grid item xs={12} md={8}>
            <Card>
              <CardContent>
                <Typography variant="h6" sx={{ mb: 2 }}>Selecionar EPIs</Typography>
                <Grid container spacing={1.5}>
                  {epis.filter((e) => e.isActive).map((epi) => (
                    <Grid item xs={12} sm={6} key={epi.id}>
                      <Paper
                        variant="outlined"
                        sx={{ p: 1.5, cursor: 'pointer', '&:hover': { bgcolor: 'primary.50', borderColor: 'primary.main' } }}
                        onClick={() => handleAddEpi(epi)}
                      >
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

      {/* PASSO 2: Assinatura Biométrica */}
      {activeStep === 2 && (
        <Card>
          <CardContent sx={{ textAlign: 'center', py: 6 }}>
            <Fingerprint sx={{ fontSize: 80, color: scanning ? 'secondary.main' : 'grey.400', mb: 2 }} />
            <Typography variant="h6" sx={{ mb: 1 }}>Assinatura Biométrica</Typography>
            <Typography color="text.secondary" sx={{ mb: 3 }}>
              Solicite ao funcionário <strong>{identifiedEmployee?.employeeName}</strong> que coloque
              o dedo novamente para assinar digitalmente a ficha de EPI
            </Typography>

            {progress && (
              <Box sx={{ mb: 2 }}>
                <Typography variant="body2" color="secondary" sx={{ mb: 1 }}>{progress}</Typography>
                <LinearProgress color="secondary" />
              </Box>
            )}

            <Box sx={{ display: 'flex', gap: 2, justifyContent: 'center' }}>
              <Button variant="outlined" onClick={() => setActiveStep(1)} disabled={scanning}>Voltar</Button>
              <Button
                variant="contained"
                color="secondary"
                size="large"
                onClick={handleSign}
                disabled={scanning}
                startIcon={scanning ? <CircularProgress size={20} color="inherit" /> : <Fingerprint />}
              >
                {scanning ? 'Capturando assinatura...' : 'Capturar Assinatura Digital'}
              </Button>
              {scanning && (
                <Button variant="outlined" onClick={bio.cancel}>Cancelar</Button>
              )}
            </Box>
          </CardContent>
        </Card>
      )}

      {/* PASSO 3: Confirmação */}
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
                    <TableCell>Validade: {s.epi.validityDays} dias</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>

            <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end' }}>
              <Button onClick={() => setActiveStep(2)}>Voltar</Button>
              <Button
                variant="contained"
                color="success"
                size="large"
                startIcon={deliveryMutation.isPending ? <CircularProgress size={20} color="inherit" /> : <Send />}
                onClick={handleConfirmDelivery}
                disabled={deliveryMutation.isPending}
              >
                Confirmar Entrega
              </Button>
            </Box>
          </CardContent>
        </Card>
      )}
    </Box>
  )
}
