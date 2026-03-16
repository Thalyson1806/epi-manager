import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import {
  Box, Stepper, Step, StepLabel, Card, CardContent, Typography,
  Button, CircularProgress, Alert, Chip, Grid,
  Table, TableBody, TableCell, TableRow, IconButton, Divider,
  Paper,
} from '@mui/material'
import { Fingerprint, Add, Remove, CheckCircle, Send } from '@mui/icons-material'
import { deliveriesApi, type BiometricIdentifyResult } from '../api/deliveries'
import { episApi, type Epi } from '../api/epis'

const STEPS = ['Identificação Biométrica', 'Seleção de EPIs', 'Assinatura Biométrica', 'Confirmação']

interface SelectedEpi {
  epi: Epi
  quantity: number
}

export default function DeliveryPage() {
  const qc = useQueryClient()
  const [activeStep, setActiveStep] = useState(0)
  const [identifiedEmployee, setIdentifiedEmployee] = useState<BiometricIdentifyResult | null>(null)
  const [selectedEpis, setSelectedEpis] = useState<SelectedEpi[]>([])
  const [biometricSignature, setBiometricSignature] = useState<string | null>(null)
  const [scanning, setScanning] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState(false)

  const { data: epis = [] } = useQuery({ queryKey: ['epis'], queryFn: episApi.getAll })

  const identifyMutation = useMutation({
    mutationFn: deliveriesApi.identify,
    onSuccess: (result) => {
      if (result.identified) {
        setIdentifiedEmployee(result)
        setActiveStep(1)
        setError('')
      } else {
        setError('Funcionário não identificado. Tente novamente.')
      }
      setScanning(false)
    },
    onError: () => { setError('Erro ao processar biometria.'); setScanning(false) },
  })

  const deliveryMutation = useMutation({
    mutationFn: deliveriesApi.create,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['dashboard'] })
      setSuccess(true)
      setActiveStep(4)
    },
    onError: () => setError('Erro ao registrar entrega.'),
  })

  // Simulate biometric capture (replace with actual DigitalPersona SDK WebSocket/bridge)
  const handleBiometricScan = async (purpose: 'identify' | 'sign') => {
    setScanning(true)
    setError('')

    if (purpose === 'identify') {
      // In production: call local bridge/service that interfaces with DigitalPersona SDK
      // For now: prompt for manual employee search or use demo mode
      setTimeout(() => {
        // Demo: send empty base64 to backend which will handle gracefully
        identifyMutation.mutate(btoa('demo-biometric-sample'))
      }, 2000)
    } else {
      // Signature capture
      setTimeout(() => {
        setBiometricSignature(btoa('signature-sample-' + Date.now()))
        setScanning(false)
        setActiveStep(3)
      }, 2000)
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
      prev.map((s) => s.epi.id === epiId ? { ...s, quantity: Math.max(1, s.quantity + delta) } : s)
        .filter((s) => s.quantity > 0),
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

      {error && <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError('')}>{error}</Alert>}

      {/* Step 0: Biometric Identification */}
      {activeStep === 0 && (
        <Card>
          <CardContent sx={{ textAlign: 'center', py: 6 }}>
            <Fingerprint sx={{ fontSize: 80, color: scanning ? 'primary.main' : 'grey.400', mb: 2, transition: 'color 0.3s' }} />
            <Typography variant="h6" sx={{ mb: 1 }}>Identificação Biométrica</Typography>
            <Typography color="text.secondary" sx={{ mb: 3 }}>
              Solicite ao funcionário que coloque o dedo no leitor DigitalPersona
            </Typography>
            <Button
              variant="contained"
              size="large"
              onClick={() => handleBiometricScan('identify')}
              disabled={scanning}
              startIcon={scanning ? <CircularProgress size={20} color="inherit" /> : <Fingerprint />}
            >
              {scanning ? 'Aguardando leitura...' : 'Iniciar Leitura Biométrica'}
            </Button>
          </CardContent>
        </Card>
      )}

      {/* Step 1: EPI Selection */}
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
                <Button
                  variant="contained"
                  fullWidth
                  disabled={selectedEpis.length === 0}
                  onClick={() => setActiveStep(2)}
                >
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

      {/* Step 2: Biometric Signature */}
      {activeStep === 2 && (
        <Card>
          <CardContent sx={{ textAlign: 'center', py: 6 }}>
            <Fingerprint sx={{ fontSize: 80, color: scanning ? 'secondary.main' : 'grey.400', mb: 2 }} />
            <Typography variant="h6" sx={{ mb: 1 }}>Assinatura Biométrica</Typography>
            <Typography color="text.secondary" sx={{ mb: 3 }}>
              Solicite ao funcionário <strong>{identifiedEmployee?.employeeName}</strong> que coloque o dedo novamente para assinar a ficha
            </Typography>
            <Box sx={{ display: 'flex', gap: 2, justifyContent: 'center' }}>
              <Button variant="outlined" onClick={() => setActiveStep(1)}>Voltar</Button>
              <Button
                variant="contained"
                color="secondary"
                size="large"
                onClick={() => handleBiometricScan('sign')}
                disabled={scanning}
                startIcon={scanning ? <CircularProgress size={20} color="inherit" /> : <Fingerprint />}
              >
                {scanning ? 'Capturando assinatura...' : 'Capturar Assinatura Digital'}
              </Button>
            </Box>
          </CardContent>
        </Card>
      )}

      {/* Step 3: Confirmation */}
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
