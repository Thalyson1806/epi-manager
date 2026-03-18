import { useState } from 'react'
import { useQuery, useMutation } from '@tanstack/react-query'
import {
  Box, Button, Card, CardContent, Typography, TextField, Grid,
  FormControlLabel, Checkbox, MenuItem, Alert, CircularProgress, Divider,
} from '@mui/material'
import { Save, Email, Send } from '@mui/icons-material'
import { useForm, Controller } from 'react-hook-form'
import { adminApi, type SmtpConfig } from '../api/admin'

export default function SettingsPage() {
  const [saveSuccess, setSaveSuccess] = useState(false)
  const [saveError, setSaveError] = useState<string | null>(null)
  const [testSuccess, setTestSuccess] = useState<string | null>(null)
  const [testError, setTestError] = useState<string | null>(null)
  const [alertSuccess, setAlertSuccess] = useState<string | null>(null)
  const [alertError, setAlertError] = useState<string | null>(null)

  const { data: config, isLoading } = useQuery({
    queryKey: ['admin-config'],
    queryFn: adminApi.getConfig,
  })

  const { control, handleSubmit, reset } = useForm<SmtpConfig>({
    defaultValues: {
      smtpHost: '',
      smtpPort: 587,
      smtpUser: '',
      smtpPassword: '',
      smtpFromEmail: '',
      smtpFromName: '',
      smtpUseSsl: false,
      alertEnabled: false,
      alertHour: 6,
    },
    values: config,
  })

  const saveMutation = useMutation({
    mutationFn: adminApi.saveConfig,
    onSuccess: () => {
      setSaveSuccess(true)
      setSaveError(null)
      setTimeout(() => setSaveSuccess(false), 3000)
    },
    onError: (err: any) => {
      setSaveError(err?.response?.data?.message ?? 'Erro ao salvar configurações.')
      setSaveSuccess(false)
    },
  })

  const testMutation = useMutation({
    mutationFn: adminApi.testEmail,
    onSuccess: (res) => {
      setTestSuccess((res.data as any)?.message ?? 'E-mail de teste enviado!')
      setTestError(null)
      setTimeout(() => setTestSuccess(null), 5000)
    },
    onError: (err: any) => {
      setTestError(err?.response?.data?.message ?? 'Erro ao enviar e-mail de teste.')
      setTestSuccess(null)
    },
  })

  const sendAlertsMutation = useMutation({
    mutationFn: adminApi.sendAlerts,
    onSuccess: (res) => {
      setAlertSuccess((res.data as any)?.message ?? 'Alertas enviados!')
      setAlertError(null)
      setTimeout(() => setAlertSuccess(null), 5000)
    },
    onError: (err: any) => {
      setAlertError(err?.response?.data?.message ?? 'Erro ao enviar alertas.')
      setAlertSuccess(null)
    },
  })

  if (isLoading) return <CircularProgress />

  return (
    <Box>
      <Typography variant="h5" mb={3}>Configurações do Sistema</Typography>

      <Card>
        <CardContent>
          <Typography variant="h6" mb={2}>Configurações de E-mail (SMTP)</Typography>
          <Divider sx={{ mb: 3 }} />

          <form onSubmit={handleSubmit((d) => saveMutation.mutate(d))}>
            <Grid container spacing={2}>
              <Grid item xs={12} md={8}>
                <Controller name="smtpHost" control={control}
                  render={({ field }) => (
                    <TextField {...field} label="Servidor SMTP" fullWidth placeholder="smtp.gmail.com" />
                  )} />
              </Grid>
              <Grid item xs={12} md={4}>
                <Controller name="smtpPort" control={control}
                  render={({ field }) => (
                    <TextField {...field} type="number" label="Porta" fullWidth />
                  )} />
              </Grid>
              <Grid item xs={12} md={6}>
                <Controller name="smtpUser" control={control}
                  render={({ field }) => (
                    <TextField {...field} label="Usuário" fullWidth />
                  )} />
              </Grid>
              <Grid item xs={12} md={6}>
                <Controller name="smtpPassword" control={control}
                  render={({ field }) => (
                    <TextField {...field} type="password" label="Senha" fullWidth />
                  )} />
              </Grid>
              <Grid item xs={12} md={6}>
                <Controller name="smtpFromEmail" control={control}
                  render={({ field }) => (
                    <TextField {...field} label="E-mail remetente" fullWidth />
                  )} />
              </Grid>
              <Grid item xs={12} md={6}>
                <Controller name="smtpFromName" control={control}
                  render={({ field }) => (
                    <TextField {...field} label="Nome remetente" fullWidth />
                  )} />
              </Grid>
              <Grid item xs={12} md={4}>
                <Controller name="smtpUseSsl" control={control}
                  render={({ field }) => (
                    <FormControlLabel
                      control={<Checkbox checked={field.value} onChange={e => field.onChange(e.target.checked)} />}
                      label="Usar SSL" />
                  )} />
              </Grid>
              <Grid item xs={12} md={4}>
                <Controller name="alertEnabled" control={control}
                  render={({ field }) => (
                    <FormControlLabel
                      control={<Checkbox checked={field.value} onChange={e => field.onChange(e.target.checked)} />}
                      label="Alertas ativos" />
                  )} />
              </Grid>
              <Grid item xs={12} md={4}>
                <Controller name="alertHour" control={control}
                  render={({ field }) => (
                    <TextField {...field} select label="Horário de envio" fullWidth>
                      {Array.from({ length: 24 }, (_, i) => (
                        <MenuItem key={i} value={i}>{String(i).padStart(2, '0')}:00</MenuItem>
                      ))}
                    </TextField>
                  )} />
              </Grid>
            </Grid>

            {saveSuccess && <Alert severity="success" sx={{ mt: 2 }}>Configurações salvas com sucesso!</Alert>}
            {saveError && <Alert severity="error" sx={{ mt: 2 }}>{saveError}</Alert>}
            {testSuccess && <Alert severity="success" sx={{ mt: 2 }}>{testSuccess}</Alert>}
            {testError && <Alert severity="error" sx={{ mt: 2 }}>{testError}</Alert>}
            {alertSuccess && <Alert severity="success" sx={{ mt: 2 }}>{alertSuccess}</Alert>}
            {alertError && <Alert severity="error" sx={{ mt: 2 }}>{alertError}</Alert>}

            <Box sx={{ display: 'flex', gap: 2, mt: 3 }}>
              <Button
                type="submit"
                variant="contained"
                startIcon={<Save />}
                disabled={saveMutation.isPending}
              >
                Salvar
              </Button>
              <Button
                variant="outlined"
                startIcon={<Email />}
                disabled={testMutation.isPending}
                onClick={() => testMutation.mutate()}
              >
                Enviar e-mail de teste
              </Button>
              <Button
                variant="outlined"
                color="warning"
                startIcon={<Send />}
                disabled={sendAlertsMutation.isPending}
                onClick={() => sendAlertsMutation.mutate()}
              >
                Enviar alertas agora
              </Button>
            </Box>
          </form>
        </CardContent>
      </Card>
    </Box>
  )
}
