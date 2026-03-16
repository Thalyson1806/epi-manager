import { useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import {
  Box, Button, Card, CardContent, Typography, TextField,
  MenuItem, Grid, CircularProgress, Alert,
} from '@mui/material'
import { ArrowBack, Save } from '@mui/icons-material'
import { useForm, Controller } from 'react-hook-form'
import { employeesApi, type CreateEmployee } from '../api/employees'
import { sectorsApi } from '../api/sectors'

export default function EmployeeFormPage() {
  const navigate = useNavigate()
  const { id } = useParams()
  const qc = useQueryClient()
  const isEdit = !!id

  const { control, handleSubmit, reset, formState: { errors } } = useForm<CreateEmployee>({
    defaultValues: {
      name: '', cpf: '', registration: '',
      sectorId: '', position: '', admissionDate: '',
    },
  })

  const { data: sectors = [] } = useQuery({ queryKey: ['sectors'], queryFn: sectorsApi.getAll })

  const { data: employee, isLoading: loadingEmployee } = useQuery({
    queryKey: ['employee', id],
    queryFn: () => employeesApi.getById(id!),
    enabled: isEdit,
  })

  useEffect(() => {
    if (employee) {
      reset({
        name: employee.name,
        cpf: employee.cpf,
        registration: employee.registration,
        sectorId: employee.sectorId,
        position: employee.position,
        admissionDate: employee.admissionDate.split('T')[0],
      })
    }
  }, [employee, reset])

  const mutation = useMutation({
    mutationFn: (data: CreateEmployee) =>
      isEdit ? employeesApi.update(id!, data) : employeesApi.create(data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['employees'] })
      navigate('/employees')
    },
  })

  if (isEdit && loadingEmployee) return <CircularProgress />

  return (
    <Box>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 3 }}>
        <Button startIcon={<ArrowBack />} onClick={() => navigate('/employees')}>Voltar</Button>
        <Typography variant="h5">{isEdit ? 'Editar Funcionário' : 'Novo Funcionário'}</Typography>
      </Box>

      <Card>
        <CardContent>
          {mutation.isError && (
            <Alert severity="error" sx={{ mb: 2 }}>
              Erro ao salvar. Verifique os dados e tente novamente.
            </Alert>
          )}

          <form onSubmit={handleSubmit((data) => mutation.mutate(data))}>
            <Grid container spacing={2}>
              <Grid item xs={12} md={6}>
                <Controller
                  name="name"
                  control={control}
                  rules={{ required: 'Nome é obrigatório' }}
                  render={({ field }) => (
                    <TextField {...field} label="Nome Completo" fullWidth error={!!errors.name} helperText={errors.name?.message} />
                  )}
                />
              </Grid>
              <Grid item xs={12} md={3}>
                <Controller
                  name="cpf"
                  control={control}
                  rules={{ required: 'CPF é obrigatório' }}
                  render={({ field }) => (
                    <TextField {...field} label="CPF" fullWidth error={!!errors.cpf} helperText={errors.cpf?.message} />
                  )}
                />
              </Grid>
              <Grid item xs={12} md={3}>
                <Controller
                  name="registration"
                  control={control}
                  rules={{ required: 'Matrícula é obrigatória' }}
                  render={({ field }) => (
                    <TextField {...field} label="Matrícula" fullWidth error={!!errors.registration} helperText={errors.registration?.message} />
                  )}
                />
              </Grid>
              <Grid item xs={12} md={4}>
                <Controller
                  name="sectorId"
                  control={control}
                  rules={{ required: 'Setor é obrigatório' }}
                  render={({ field }) => (
                    <TextField {...field} select label="Setor" fullWidth error={!!errors.sectorId} helperText={errors.sectorId?.message}>
                      {sectors.map((s) => <MenuItem key={s.id} value={s.id}>{s.name}</MenuItem>)}
                    </TextField>
                  )}
                />
              </Grid>
              <Grid item xs={12} md={4}>
                <Controller
                  name="position"
                  control={control}
                  rules={{ required: 'Cargo é obrigatório' }}
                  render={({ field }) => (
                    <TextField {...field} label="Cargo" fullWidth error={!!errors.position} helperText={errors.position?.message} />
                  )}
                />
              </Grid>
              <Grid item xs={12} md={4}>
                <Controller
                  name="admissionDate"
                  control={control}
                  rules={{ required: 'Data de admissão é obrigatória' }}
                  render={({ field }) => (
                    <TextField {...field} label="Data de Admissão" type="date" fullWidth InputLabelProps={{ shrink: true }} error={!!errors.admissionDate} helperText={errors.admissionDate?.message} />
                  )}
                />
              </Grid>
            </Grid>

            <Box sx={{ mt: 3, display: 'flex', gap: 2, justifyContent: 'flex-end' }}>
              <Button onClick={() => navigate('/employees')}>Cancelar</Button>
              <Button type="submit" variant="contained" startIcon={<Save />} disabled={mutation.isPending}>
                {mutation.isPending ? <CircularProgress size={20} /> : 'Salvar'}
              </Button>
            </Box>
          </form>
        </CardContent>
      </Card>
    </Box>
  )
}
