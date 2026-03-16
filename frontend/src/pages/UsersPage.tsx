import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import {
  Box, Button, Card, CardContent, Typography, Chip,
  Dialog, DialogTitle, DialogContent, DialogActions,
  Table, TableBody, TableCell, TableContainer, TableHead, TableRow,
  Grid, MenuItem, TextField, CircularProgress,
} from '@mui/material'
import { Add } from '@mui/icons-material'
import { useForm, Controller } from 'react-hook-form'
import { authApi } from '../api/auth'

const ROLES = [
  { value: 1, label: 'Administrador' },
  { value: 2, label: 'RH' },
  { value: 3, label: 'Almoxarifado' },
]

const roleLabel: Record<number, string> = { 1: 'Administrador', 2: 'RH', 3: 'Almoxarifado' }

type UserForm = { name: string; email: string; password: string; role: number }

export default function UsersPage() {
  const qc = useQueryClient()
  const [open, setOpen] = useState(false)

  const { data: users = [], isLoading } = useQuery({ queryKey: ['users'], queryFn: authApi.getUsers })

  const { control, handleSubmit, reset, formState: { errors } } = useForm<UserForm>({
    defaultValues: { name: '', email: '', password: '', role: 2 },
  })

  const mutation = useMutation({
    mutationFn: authApi.createUser,
    onSuccess: () => { qc.invalidateQueries({ queryKey: ['users'] }); setOpen(false); reset() },
  })

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h5">Usuários do Sistema</Typography>
        <Button variant="contained" startIcon={<Add />} onClick={() => setOpen(true)}>Novo Usuário</Button>
      </Box>

      <Card>
        <CardContent>
          {isLoading ? <CircularProgress /> : (
            <TableContainer>
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell>Nome</TableCell>
                    <TableCell>E-mail</TableCell>
                    <TableCell align="center">Perfil</TableCell>
                    <TableCell align="center">Status</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {users.map((u) => (
                    <TableRow key={u.id} hover>
                      <TableCell>{u.name}</TableCell>
                      <TableCell>{u.email}</TableCell>
                      <TableCell align="center">
                        <Chip label={roleLabel[u.role] ?? u.role} size="small" color="primary" variant="outlined" />
                      </TableCell>
                      <TableCell align="center">
                        <Chip label={u.isActive ? 'Ativo' : 'Inativo'} size="small" color={u.isActive ? 'success' : 'error'} />
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          )}
        </CardContent>
      </Card>

      <Dialog open={open} onClose={() => setOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Novo Usuário</DialogTitle>
        <form onSubmit={handleSubmit((d) => mutation.mutate(d))}>
          <DialogContent>
            <Grid container spacing={2}>
              <Grid item xs={12}>
                <Controller name="name" control={control} rules={{ required: true }}
                  render={({ field }) => <TextField {...field} label="Nome" fullWidth error={!!errors.name} />} />
              </Grid>
              <Grid item xs={12}>
                <Controller name="email" control={control} rules={{ required: true }}
                  render={({ field }) => <TextField {...field} label="E-mail" type="email" fullWidth error={!!errors.email} />} />
              </Grid>
              <Grid item xs={12}>
                <Controller name="password" control={control} rules={{ required: true, minLength: 6 }}
                  render={({ field }) => <TextField {...field} label="Senha" type="password" fullWidth error={!!errors.password} helperText={errors.password ? 'Mínimo 6 caracteres' : ''} />} />
              </Grid>
              <Grid item xs={12}>
                <Controller name="role" control={control} rules={{ required: true }}
                  render={({ field }) => (
                    <TextField {...field} select label="Perfil" fullWidth>
                      {ROLES.map((r) => <MenuItem key={r.value} value={r.value}>{r.label}</MenuItem>)}
                    </TextField>
                  )} />
              </Grid>
            </Grid>
          </DialogContent>
          <DialogActions>
            <Button onClick={() => setOpen(false)}>Cancelar</Button>
            <Button type="submit" variant="contained" disabled={mutation.isPending}>Criar</Button>
          </DialogActions>
        </form>
      </Dialog>
    </Box>
  )
}
