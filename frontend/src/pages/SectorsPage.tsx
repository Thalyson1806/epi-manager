import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import {
  Box, Button, Card, CardContent, Typography, Chip,
  TextField, Dialog, DialogTitle, DialogContent, DialogActions,
  Table, TableBody, TableCell, TableContainer, TableHead, TableRow,
  IconButton, Tooltip, CircularProgress, Grid, Divider,
  FormControlLabel, Checkbox, Collapse, Alert,
} from '@mui/material'
import {
  Add, Edit, Delete, ExpandMore, ExpandLess, Security, CheckCircle, RadioButtonUnchecked,
} from '@mui/icons-material'
import { useForm, Controller } from 'react-hook-form'
import { sectorsApi, type Sector, type SectorEpi } from '../api/sectors'
import { episApi } from '../api/epis'

type SectorForm = { name: string; description: string; supervisorName: string; supervisorEmail: string }
type EpiForm = { epiId: string; isRequired: boolean; replacementPeriodDays: number; maxQuantityAllowed: number }

export default function SectorsPage() {
  const qc = useQueryClient()
  const [sectorOpen, setSectorOpen] = useState(false)
  const [editingSector, setEditingSector] = useState<Sector | null>(null)
  const [epiOpen, setEpiOpen] = useState(false)
  const [selectedSectorId, setSelectedSectorId] = useState<string | null>(null)
  const [editingEpi, setEditingEpi] = useState<SectorEpi | null>(null)
  const [expanded, setExpanded] = useState<string | null>(null)
  const [deleteConfirm, setDeleteConfirm] = useState<{ type: 'sector' | 'epi'; id: string; name: string } | null>(null)

  const { data: sectors = [], isLoading } = useQuery({ queryKey: ['sectors'], queryFn: sectorsApi.getAll })
  const { data: allEpis = [] } = useQuery({ queryKey: ['epis'], queryFn: episApi.getAll })
  const { data: sectorEpis = [] } = useQuery({
    queryKey: ['sector-epis', expanded],
    queryFn: () => sectorsApi.getEpis(expanded!),
    enabled: !!expanded,
  })

  const sectorForm = useForm<SectorForm>({ defaultValues: { name: '', description: '', supervisorName: '', supervisorEmail: '' } })
  const saveSectorMutation = useMutation({
    mutationFn: (data: SectorForm) =>
      editingSector ? sectorsApi.update(editingSector.id, data) : sectorsApi.create(data),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ['sectors'] }); closeSectorDialog() },
  })
  const deleteSectorMutation = useMutation({
    mutationFn: sectorsApi.delete,
    onSuccess: () => { qc.invalidateQueries({ queryKey: ['sectors'] }); setDeleteConfirm(null) },
  })

  const epiForm = useForm<EpiForm>({
    defaultValues: { epiId: '', isRequired: true, replacementPeriodDays: 365, maxQuantityAllowed: 1 },
  })
  const saveEpiMutation = useMutation({
    mutationFn: (data: EpiForm) =>
      editingEpi
        ? sectorsApi.updateEpi(editingEpi.id, { isRequired: data.isRequired, replacementPeriodDays: data.replacementPeriodDays, maxQuantityAllowed: data.maxQuantityAllowed })
        : sectorsApi.addEpi(selectedSectorId!, data),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ['sector-epis', expanded] }); closeEpiDialog() },
  })
  const removeEpiMutation = useMutation({
    mutationFn: sectorsApi.removeEpi,
    onSuccess: () => { qc.invalidateQueries({ queryKey: ['sector-epis', expanded] }); setDeleteConfirm(null) },
  })

  const openSectorDialog = (sector?: Sector) => {
    setEditingSector(sector ?? null)
    sectorForm.reset({
      name: sector?.name ?? '',
      description: sector?.description ?? '',
      supervisorName: sector?.supervisorName ?? '',
      supervisorEmail: sector?.supervisorEmail ?? '',
    })
    setSectorOpen(true)
  }
  const closeSectorDialog = () => { setSectorOpen(false); setEditingSector(null) }

  const openEpiDialog = (sectorId: string, epi?: SectorEpi) => {
    setSelectedSectorId(sectorId)
    setEditingEpi(epi ?? null)
    epiForm.reset(epi
      ? { epiId: epi.epiId, isRequired: epi.isRequired, replacementPeriodDays: epi.replacementPeriodDays, maxQuantityAllowed: epi.maxQuantityAllowed }
      : { epiId: '', isRequired: true, replacementPeriodDays: 365, maxQuantityAllowed: 1 })
    setEpiOpen(true)
  }
  const closeEpiDialog = () => { setEpiOpen(false); setEditingEpi(null) }
  const toggleExpand = (id: string) => setExpanded(prev => prev === id ? null : id)

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h5">Setores</Typography>
        <Button variant="contained" startIcon={<Add />} onClick={() => openSectorDialog()}>Novo Setor</Button>
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
                    <>
                      <TableRow key={s.id} hover>
                        <TableCell sx={{ fontWeight: 600 }}>{s.name}</TableCell>
                        <TableCell>{s.description ?? '-'}</TableCell>
                        <TableCell align="center"><Chip label={s.employeeCount} size="small" color="primary" /></TableCell>
                        <TableCell align="center">
                          <Tooltip title={expanded === s.id ? 'Ocultar EPIs' : 'Gerenciar EPIs'}>
                            <IconButton size="small" color="info" onClick={() => toggleExpand(s.id)}>
                              <Security fontSize="small" />
                              {expanded === s.id ? <ExpandLess fontSize="small" /> : <ExpandMore fontSize="small" />}
                            </IconButton>
                          </Tooltip>
                          <Tooltip title="Editar"><IconButton size="small" onClick={() => openSectorDialog(s)}><Edit fontSize="small" /></IconButton></Tooltip>
                          <Tooltip title="Excluir">
                            <IconButton size="small" color="error" onClick={() => setDeleteConfirm({ type: 'sector', id: s.id, name: s.name })}>
                              <Delete fontSize="small" />
                            </IconButton>
                          </Tooltip>
                        </TableCell>
                      </TableRow>
                      <TableRow key={`${s.id}-epis`}>
                        <TableCell colSpan={4} sx={{ p: 0, border: 0 }}>
                          <Collapse in={expanded === s.id} unmountOnExit>
                            <Box sx={{ bgcolor: 'grey.50', px: 3, py: 2, borderBottom: '1px solid', borderColor: 'divider' }}>
                              <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
                                <Typography variant="subtitle2" color="primary">EPIs do setor — {s.name}</Typography>
                                <Button size="small" startIcon={<Add />} variant="outlined" onClick={() => openEpiDialog(s.id)}>Vincular EPI</Button>
                              </Box>
                              <Divider sx={{ mb: 1 }} />
                              {sectorEpis.length === 0
                                ? <Typography variant="body2" color="text.secondary">Nenhum EPI vinculado.</Typography>
                                : (
                                  <Table size="small">
                                    <TableHead>
                                      <TableRow>
                                        <TableCell>EPI</TableCell>
                                        <TableCell align="center">Obrigatório</TableCell>
                                        <TableCell align="center">Troca (dias)</TableCell>
                                        <TableCell align="center">Qtd. máx.</TableCell>
                                        <TableCell align="center">Ações</TableCell>
                                      </TableRow>
                                    </TableHead>
                                    <TableBody>
                                      {sectorEpis.map((se) => (
                                        <TableRow key={se.id}>
                                          <TableCell>{se.epiName}</TableCell>
                                          <TableCell align="center">
                                            {se.isRequired ? <CheckCircle fontSize="small" color="success" /> : <RadioButtonUnchecked fontSize="small" color="disabled" />}
                                          </TableCell>
                                          <TableCell align="center">{se.replacementPeriodDays}d</TableCell>
                                          <TableCell align="center">{se.maxQuantityAllowed}</TableCell>
                                          <TableCell align="center">
                                            <Tooltip title="Editar"><IconButton size="small" onClick={() => openEpiDialog(s.id, se)}><Edit fontSize="small" /></IconButton></Tooltip>
                                            <Tooltip title="Desvincular">
                                              <IconButton size="small" color="error" onClick={() => setDeleteConfirm({ type: 'epi', id: se.id, name: se.epiName })}>
                                                <Delete fontSize="small" />
                                              </IconButton>
                                            </Tooltip>
                                          </TableCell>
                                        </TableRow>
                                      ))}
                                    </TableBody>
                                  </Table>
                                )}
                            </Box>
                          </Collapse>
                        </TableCell>
                      </TableRow>
                    </>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          )}
        </CardContent>
      </Card>

      {/* Dialog setor */}
      <Dialog open={sectorOpen} onClose={closeSectorDialog} maxWidth="sm" fullWidth>
        <DialogTitle>{editingSector ? 'Editar Setor' : 'Novo Setor'}</DialogTitle>
        <form onSubmit={sectorForm.handleSubmit((d) => saveSectorMutation.mutate(d))}>
          <DialogContent>
            <Grid container spacing={2}>
              <Grid item xs={12}>
                <Controller name="name" control={sectorForm.control} rules={{ required: true }}
                  render={({ field }) => <TextField {...field} label="Nome" fullWidth error={!!sectorForm.formState.errors.name} />} />
              </Grid>
              <Grid item xs={12}>
                <Controller name="description" control={sectorForm.control}
                  render={({ field }) => <TextField {...field} label="Descrição" fullWidth multiline rows={2} />} />
              </Grid>
              <Grid item xs={12} sm={6}>
                <Controller name="supervisorName" control={sectorForm.control}
                  render={({ field }) => <TextField {...field} label="Nome do supervisor" fullWidth />} />
              </Grid>
              <Grid item xs={12} sm={6}>
                <Controller name="supervisorEmail" control={sectorForm.control}
                  render={({ field }) => <TextField {...field} label="E-mail do supervisor" fullWidth type="email" />} />
              </Grid>
            </Grid>
          </DialogContent>
          <DialogActions>
            <Button onClick={closeSectorDialog}>Cancelar</Button>
            <Button type="submit" variant="contained" disabled={saveSectorMutation.isPending}>Salvar</Button>
          </DialogActions>
        </form>
      </Dialog>

      {/* Dialog EPI */}
      <Dialog open={epiOpen} onClose={closeEpiDialog} maxWidth="sm" fullWidth>
        <DialogTitle>{editingEpi ? 'Editar vínculo EPI' : 'Vincular EPI ao setor'}</DialogTitle>
        <form onSubmit={epiForm.handleSubmit((d) => saveEpiMutation.mutate(d))}>
          <DialogContent>
            <Grid container spacing={2}>
              {!editingEpi && (
                <Grid item xs={12}>
                  <Controller name="epiId" control={epiForm.control} rules={{ required: true }}
                    render={({ field }) => (
                      <TextField {...field} select label="EPI" fullWidth SelectProps={{ native: true }} error={!!epiForm.formState.errors.epiId}>
                        <option value="">Selecione...</option>
                        {allEpis.filter(e => e.isActive).map(e => (
                          <option key={e.id} value={e.id}>{e.name} ({e.code})</option>
                        ))}
                      </TextField>
                    )} />
                </Grid>
              )}
              {editingEpi && <Grid item xs={12}><Typography variant="body2">EPI: <strong>{editingEpi.epiName}</strong></Typography></Grid>}
              <Grid item xs={6}>
                <Controller name="replacementPeriodDays" control={epiForm.control} rules={{ required: true, min: 1 }}
                  render={({ field }) => <TextField {...field} type="number" label="Período de troca (dias)" fullWidth />} />
              </Grid>
              <Grid item xs={6}>
                <Controller name="maxQuantityAllowed" control={epiForm.control} rules={{ required: true, min: 1 }}
                  render={({ field }) => <TextField {...field} type="number" label="Qtd. máxima" fullWidth />} />
              </Grid>
              <Grid item xs={12}>
                <Controller name="isRequired" control={epiForm.control}
                  render={({ field }) => (
                    <FormControlLabel
                      control={<Checkbox checked={field.value} onChange={e => field.onChange(e.target.checked)} />}
                      label="EPI obrigatório para este setor" />
                  )} />
              </Grid>
            </Grid>
            {saveEpiMutation.isError && (
              <Alert severity="error" sx={{ mt: 1 }}>
                {(saveEpiMutation.error as any)?.response?.data?.message ?? 'Erro ao salvar.'}
              </Alert>
            )}
          </DialogContent>
          <DialogActions>
            <Button onClick={closeEpiDialog}>Cancelar</Button>
            <Button type="submit" variant="contained" disabled={saveEpiMutation.isPending}>Salvar</Button>
          </DialogActions>
        </form>
      </Dialog>

      {/* Confirmação exclusão */}
      <Dialog open={!!deleteConfirm} onClose={() => setDeleteConfirm(null)}>
        <DialogTitle>Confirmar exclusão</DialogTitle>
        <DialogContent>
          <Typography>
            {deleteConfirm?.type === 'sector'
              ? <>Excluir o setor <strong>{deleteConfirm?.name}</strong>?</>
              : <>Desvincular o EPI <strong>{deleteConfirm?.name}</strong> deste setor?</>}
          </Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteConfirm(null)}>Cancelar</Button>
          <Button variant="contained" color="error"
            disabled={deleteSectorMutation.isPending || removeEpiMutation.isPending}
            onClick={() => {
              if (deleteConfirm?.type === 'sector') deleteSectorMutation.mutate(deleteConfirm.id)
              else if (deleteConfirm?.type === 'epi') removeEpiMutation.mutate(deleteConfirm.id)
            }}>
            Confirmar
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  )
}
