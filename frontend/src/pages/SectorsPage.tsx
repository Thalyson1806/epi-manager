import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import {
  Box, Button, Card, CardContent, Typography, Chip,
  TextField, Dialog, DialogTitle, DialogContent, DialogActions,
  Table, TableBody, TableCell, TableContainer, TableHead, TableRow,
  IconButton, Tooltip, CircularProgress, Grid,
} from '@mui/material'
import { Add, Edit, Delete } from '@mui/icons-material'
import { useForm, Controller } from 'react-hook-form'
import { sectorsApi, type Sector } from '../api/sectors'

type SectorForm = { name: string; description: string }

export default function SectorsPage() {
  const qc = useQueryClient()
  const [open, setOpen] = useState(false)
  const [editing, setEditing] = useState<Sector | null>(null)

  const { data: sectors = [], isLoading } = useQuery({ queryKey: ['sectors'], queryFn: sectorsApi.getAll })

  const { control, handleSubmit, reset, formState: { errors } } = useForm<SectorForm>({
    defaultValues: { name: '', description: '' },
  })

  const saveMutation = useMutation({
    mutationFn: (data: SectorForm) =>
      editing ? sectorsApi.update(editing.id, data) : sectorsApi.create(data),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ['sectors'] }); handleClose() },
  })

  const deleteMutation = useMutation({
    mutationFn: sectorsApi.delete,
    onSuccess: () => qc.invalidateQueries({ queryKey: ['sectors'] }),
  })

  const handleOpen = (sector?: Sector) => {
    setEditing(sector ?? null)
    reset({ name: sector?.name ?? '', description: sector?.description ?? '' })
    setOpen(true)
  }

  const handleClose = () => { setOpen(false); setEditing(null) }

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h5">Setores</Typography>
        <Button variant="contained" startIcon={<Add />} onClick={() => handleOpen()}>Novo Setor</Button>
      </Box>

      <Card>
        <CardContent>
          {isLoading ? <CircularProgress /> : (
            <TableContainer>
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell>Nome</TableCell>
                    <TableCell>Descrição</TableCell>
                    <TableCell align="center">Funcionários</TableCell>
                    <TableCell align="center">Ações</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {sectors.map((s) => (
                    <TableRow key={s.id} hover>
                      <TableCell>{s.name}</TableCell>
                      <TableCell>{s.description ?? '-'}</TableCell>
                      <TableCell align="center"><Chip label={s.employeeCount} size="small" color="primary" /></TableCell>
                      <TableCell align="center">
                        <Tooltip title="Editar">
                          <IconButton size="small" onClick={() => handleOpen(s)}><Edit fontSize="small" /></IconButton>
                        </Tooltip>
                        <Tooltip title="Excluir">
                          <IconButton size="small" color="error" onClick={() => deleteMutation.mutate(s.id)}>
                            <Delete fontSize="small" />
                          </IconButton>
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
        <DialogTitle>{editing ? 'Editar Setor' : 'Novo Setor'}</DialogTitle>
        <form onSubmit={handleSubmit((d) => saveMutation.mutate(d))}>
          <DialogContent>
            <Grid container spacing={2}>
              <Grid item xs={12}>
                <Controller name="name" control={control} rules={{ required: true }}
                  render={({ field }) => <TextField {...field} label="Nome" fullWidth error={!!errors.name} />} />
              </Grid>
              <Grid item xs={12}>
                <Controller name="description" control={control}
                  render={({ field }) => <TextField {...field} label="Descrição" fullWidth multiline rows={2} />} />
              </Grid>
            </Grid>
          </DialogContent>
          <DialogActions>
            <Button onClick={handleClose}>Cancelar</Button>
            <Button type="submit" variant="contained" disabled={saveMutation.isPending}>Salvar</Button>
          </DialogActions>
        </form>
      </Dialog>
    </Box>
  )
}
