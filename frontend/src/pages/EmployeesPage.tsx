import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import {
  Box, Button, Card, CardContent, Typography, Chip,
  IconButton, TextField, InputAdornment, Tooltip,
  Table, TableBody, TableCell, TableContainer, TableHead, TableRow,
  CircularProgress, Dialog, DialogTitle, DialogContent, DialogActions,
} from '@mui/material'
import {
  Add, Search, Edit, PersonOff, PersonAdd, Fingerprint, Visibility,
} from '@mui/icons-material'
import { employeesApi } from '../api/employees'
import { format } from 'date-fns'
import { ptBR } from 'date-fns/locale'

const statusLabel: Record<number, { label: string; color: 'success' | 'error' }> = {
  1: { label: 'Ativo', color: 'success' },
  2: { label: 'Inativo', color: 'error' },
}

export default function EmployeesPage() {
  const navigate = useNavigate()
  const qc = useQueryClient()
  const [search, setSearch] = useState('')
  const [confirmDialog, setConfirmDialog] = useState<{ id: string; action: 'activate' | 'deactivate'; name: string } | null>(null)

  const { data: employees = [], isLoading } = useQuery({
    queryKey: ['employees'],
    queryFn: employeesApi.getAll,
  })

  const toggleMutation = useMutation({
    mutationFn: ({ id, action }: { id: string; action: 'activate' | 'deactivate' }) =>
      action === 'activate' ? employeesApi.activate(id) : employeesApi.deactivate(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['employees'] })
      setConfirmDialog(null)
    },
  })

  const filtered = employees.filter(
    (e) =>
      e.name.toLowerCase().includes(search.toLowerCase()) ||
      e.registration.toLowerCase().includes(search.toLowerCase()) ||
      e.cpf.includes(search),
  )

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h5">Funcionários</Typography>
        <Button variant="contained" startIcon={<Add />} onClick={() => navigate('/employees/new')}>
          Novo Funcionário
        </Button>
      </Box>

      <Card>
        <CardContent>
          <TextField
            placeholder="Buscar por nome, matrícula ou CPF..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            sx={{ mb: 2, width: { xs: '100%', sm: 360 } }}
            InputProps={{ startAdornment: <InputAdornment position="start"><Search /></InputAdornment> }}
            size="small"
          />

          {isLoading ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}><CircularProgress /></Box>
          ) : (
            <TableContainer>
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell>Nome</TableCell>
                    <TableCell>Matrícula</TableCell>
                    <TableCell>CPF</TableCell>
                    <TableCell>Setor</TableCell>
                    <TableCell>Cargo</TableCell>
                    <TableCell>Admissão</TableCell>
                    <TableCell align="center">Biometria</TableCell>
                    <TableCell align="center">Status</TableCell>
                    <TableCell align="center">Ações</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {filtered.map((emp) => (
                    <TableRow key={emp.id} hover>
                      <TableCell>{emp.name}</TableCell>
                      <TableCell>{emp.registration}</TableCell>
                      <TableCell>{emp.cpf}</TableCell>
                      <TableCell>{emp.sectorName}</TableCell>
                      <TableCell>{emp.position}</TableCell>
                      <TableCell>
                        {format(new Date(emp.admissionDate), 'dd/MM/yyyy', { locale: ptBR })}
                      </TableCell>
                      <TableCell align="center">
                        <Chip
                          icon={<Fingerprint />}
                          label={emp.hasBiometric ? 'Cadastrado' : 'Pendente'}
                          size="small"
                          color={emp.hasBiometric ? 'success' : 'warning'}
                          variant="outlined"
                        />
                      </TableCell>
                      <TableCell align="center">
                        <Chip
                          label={statusLabel[emp.status]?.label}
                          size="small"
                          color={statusLabel[emp.status]?.color}
                        />
                      </TableCell>
                      <TableCell align="center">
                        <Tooltip title="Ver ficha">
                          <IconButton size="small" onClick={() => navigate(`/employees/${emp.id}`)}>
                            <Visibility fontSize="small" />
                          </IconButton>
                        </Tooltip>
                        <Tooltip title="Editar">
                          <IconButton size="small" onClick={() => navigate(`/employees/${emp.id}/edit`)}>
                            <Edit fontSize="small" />
                          </IconButton>
                        </Tooltip>
                        <Tooltip title={emp.status === 1 ? 'Desativar' : 'Ativar'}>
                          <IconButton
                            size="small"
                            color={emp.status === 1 ? 'error' : 'success'}
                            onClick={() => setConfirmDialog({ id: emp.id, action: emp.status === 1 ? 'deactivate' : 'activate', name: emp.name })}
                          >
                            {emp.status === 1 ? <PersonOff fontSize="small" /> : <PersonAdd fontSize="small" />}
                          </IconButton>
                        </Tooltip>
                      </TableCell>
                    </TableRow>
                  ))}
                  {filtered.length === 0 && (
                    <TableRow>
                      <TableCell colSpan={9} align="center" sx={{ py: 3, color: 'text.secondary' }}>
                        Nenhum funcionário encontrado
                      </TableCell>
                    </TableRow>
                  )}
                </TableBody>
              </Table>
            </TableContainer>
          )}
        </CardContent>
      </Card>

      <Dialog open={!!confirmDialog} onClose={() => setConfirmDialog(null)}>
        <DialogTitle>
          {confirmDialog?.action === 'deactivate' ? 'Desativar Funcionário' : 'Ativar Funcionário'}
        </DialogTitle>
        <DialogContent>
          <Typography>
            Deseja {confirmDialog?.action === 'deactivate' ? 'desativar' : 'ativar'} o funcionário{' '}
            <strong>{confirmDialog?.name}</strong>?
          </Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setConfirmDialog(null)}>Cancelar</Button>
          <Button
            variant="contained"
            color={confirmDialog?.action === 'deactivate' ? 'error' : 'success'}
            onClick={() => confirmDialog && toggleMutation.mutate({ id: confirmDialog.id, action: confirmDialog.action })}
            disabled={toggleMutation.isPending}
          >
            Confirmar
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  )
}
