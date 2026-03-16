import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import {
  Box, Button, Card, CardContent, Typography, Chip,
  TextField, Dialog, DialogTitle, DialogContent, DialogActions,
  Table, TableBody, TableCell, TableContainer, TableHead, TableRow,
  Grid, MenuItem, IconButton, Tooltip, CircularProgress,
} from '@mui/material'
import { Add, Edit } from '@mui/icons-material'
import { useForm, Controller } from 'react-hook-form'
import { episApi, type Epi } from '../api/epis'

const EPI_TYPES = ['Capacete', 'Luva', 'Óculos', 'Protetor Auricular', 'Botina', 'Cinto de Segurança', 'Máscara', 'Colete', 'Outro']

type EpiForm = Omit<Epi, 'id' | 'isActive'>

export default function EpisPage() {
  const qc = useQueryClient()
  const [open, setOpen] = useState(false)
  const [editing, setEditing] = useState<Epi | null>(null)

  const { data: epis = [], isLoading } = useQuery({ queryKey: ['epis'], queryFn: episApi.getAll })

  const { control, handleSubmit, reset, formState: { errors } } = useForm<EpiForm>({
    defaultValues: { name: '', code: '', description: '', validityDays: 30, type: '' },
  })

  const mutation = useMutation({
    mutationFn: (data: EpiForm) => editing ? episApi.update(editing.id, data) : episApi.create(data),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ['epis'] }); handleClose() },
  })

  const handleOpen = (epi?: Epi) => {
    setEditing(epi ?? null)
    reset(epi ? { name: epi.name, code: epi.code, description: epi.description ?? '', validityDays: epi.validityDays, type: epi.type } : { name: '', code: '', description: '', validityDays: 30, type: '' })
    setOpen(true)
  }

  const handleClose = () => { setOpen(false); setEditing(null) }

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h5">EPIs Cadastrados</Typography>
        <Button variant="contained" startIcon={<Add />} onClick={() => handleOpen()}>Novo EPI</Button>
      </Box>

      <Card>
        <CardContent>
          {isLoading ? <CircularProgress /> : (
            <TableContainer>
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell>Nome</TableCell>
                    <TableCell>Código</TableCell>
                    <TableCell>Tipo</TableCell>
                    <TableCell align="center">Validade (dias)</TableCell>
                    <TableCell align="center">Status</TableCell>
                    <TableCell align="center">Ações</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {epis.map((epi) => (
                    <TableRow key={epi.id} hover>
                      <TableCell>
                        <Typography variant="body2" fontWeight={500}>{epi.name}</Typography>
                        {epi.description && <Typography variant="caption" color="text.secondary">{epi.description}</Typography>}
                      </TableCell>
                      <TableCell><Chip label={epi.code} size="small" variant="outlined" /></TableCell>
                      <TableCell>{epi.type}</TableCell>
                      <TableCell align="center">{epi.validityDays}</TableCell>
                      <TableCell align="center">
                        <Chip label={epi.isActive ? 'Ativo' : 'Inativo'} size="small" color={epi.isActive ? 'success' : 'error'} />
                      </TableCell>
                      <TableCell align="center">
                        <Tooltip title="Editar">
                          <IconButton size="small" onClick={() => handleOpen(epi)}><Edit fontSize="small" /></IconButton>
                        </Tooltip>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          )}
        </CardContent>
      </Card>

      <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth>
        <DialogTitle>{editing ? 'Editar EPI' : 'Novo EPI'}</DialogTitle>
        <form onSubmit={handleSubmit((d) => mutation.mutate(d))}>
          <DialogContent>
            <Grid container spacing={2}>
              <Grid item xs={12} sm={8}>
                <Controller name="name" control={control} rules={{ required: true }}
                  render={({ field }) => <TextField {...field} label="Nome" fullWidth error={!!errors.name} />} />
              </Grid>
              <Grid item xs={12} sm={4}>
                <Controller name="code" control={control} rules={{ required: true }}
                  render={({ field }) => <TextField {...field} label="Código" fullWidth error={!!errors.code} />} />
              </Grid>
              <Grid item xs={12} sm={6}>
                <Controller name="type" control={control} rules={{ required: true }}
                  render={({ field }) => (
                    <TextField {...field} select label="Tipo" fullWidth error={!!errors.type}>
                      {EPI_TYPES.map((t) => <MenuItem key={t} value={t}>{t}</MenuItem>)}
                    </TextField>
                  )} />
              </Grid>
              <Grid item xs={12} sm={6}>
                <Controller name="validityDays" control={control} rules={{ required: true, min: 1 }}
                  render={({ field }) => <TextField {...field} type="number" label="Validade (dias)" fullWidth error={!!errors.validityDays} />} />
              </Grid>
              <Grid item xs={12}>
                <Controller name="description" control={control}
                  render={({ field }) => <TextField {...field} label="Descrição" fullWidth multiline rows={2} />} />
              </Grid>
            </Grid>
          </DialogContent>
          <DialogActions>
            <Button onClick={handleClose}>Cancelar</Button>
            <Button type="submit" variant="contained" disabled={mutation.isPending}>Salvar</Button>
          </DialogActions>
        </form>
      </Dialog>
    </Box>
  )
}
