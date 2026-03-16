import { useQuery } from '@tanstack/react-query'
import {
  Box, Card, CardContent, Typography, Grid,
  Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Chip,
} from '@mui/material'
import { employeesApi } from '../api/employees'
import { episApi } from '../api/epis'
import { deliveriesApi } from '../api/deliveries'
import { format } from 'date-fns'
import { ptBR } from 'date-fns/locale'

export default function ReportsPage() {
  const { data: employees = [] } = useQuery({ queryKey: ['employees'], queryFn: employeesApi.getAll })
  const { data: epis = [] } = useQuery({ queryKey: ['epis'], queryFn: episApi.getAll })
  const { data: dashboard } = useQuery({ queryKey: ['dashboard'], queryFn: deliveriesApi.getDashboard })

  const activeEmployees = employees.filter((e) => e.status === 'Active')
  const inactiveEmployees = employees.filter((e) => e.status === 'Inactive')

  const sectorCounts = employees.reduce<Record<string, number>>((acc, e) => {
    acc[e.sectorName] = (acc[e.sectorName] ?? 0) + 1
    return acc
  }, {})

  return (
    <Box>
      <Typography variant="h5" sx={{ mb: 3 }}>Relatórios</Typography>

      <Grid container spacing={3}>
        {/* Employees Summary */}
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="h6" sx={{ mb: 2 }}>Funcionários por Setor</Typography>
              <TableContainer>
                <Table size="small">
                  <TableHead>
                    <TableRow>
                      <TableCell>Setor</TableCell>
                      <TableCell align="center">Total</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {Object.entries(sectorCounts).map(([sector, count]) => (
                      <TableRow key={sector}>
                        <TableCell>{sector}</TableCell>
                        <TableCell align="center"><Chip label={count} size="small" color="primary" /></TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </TableContainer>
            </CardContent>
          </Card>
        </Grid>

        {/* Status Summary */}
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="h6" sx={{ mb: 2 }}>Resumo de Funcionários</Typography>
              <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <Typography>Total de Funcionários</Typography>
                  <Chip label={employees.length} color="default" />
                </Box>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <Typography>Funcionários Ativos</Typography>
                  <Chip label={activeEmployees.length} color="success" />
                </Box>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <Typography>Funcionários Inativos</Typography>
                  <Chip label={inactiveEmployees.length} color="error" />
                </Box>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <Typography>Com Biometria Cadastrada</Typography>
                  <Chip label={employees.filter((e) => e.hasBiometric).length} color="info" />
                </Box>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <Typography>EPIs Vencendo (30 dias)</Typography>
                  <Chip label={dashboard?.expiringEpisNext30Days ?? 0} color="warning" />
                </Box>
              </Box>
            </CardContent>
          </Card>
        </Grid>

        {/* EPIs List */}
        <Grid item xs={12}>
          <Card>
            <CardContent>
              <Typography variant="h6" sx={{ mb: 2 }}>Catálogo de EPIs</Typography>
              <TableContainer>
                <Table size="small">
                  <TableHead>
                    <TableRow>
                      <TableCell>EPI</TableCell>
                      <TableCell>Código</TableCell>
                      <TableCell>Tipo</TableCell>
                      <TableCell align="center">Validade</TableCell>
                      <TableCell align="center">Status</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {epis.map((epi) => (
                      <TableRow key={epi.id} hover>
                        <TableCell>{epi.name}</TableCell>
                        <TableCell><Chip label={epi.code} size="small" variant="outlined" /></TableCell>
                        <TableCell>{epi.type}</TableCell>
                        <TableCell align="center">{epi.validityDays} dias</TableCell>
                        <TableCell align="center">
                          <Chip label={epi.isActive ? 'Ativo' : 'Inativo'} size="small" color={epi.isActive ? 'success' : 'error'} />
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </TableContainer>
            </CardContent>
          </Card>
        </Grid>

        {/* Recent Deliveries */}
        <Grid item xs={12}>
          <Card>
            <CardContent>
              <Typography variant="h6" sx={{ mb: 2 }}>Entregas Recentes (últimos 7 dias)</Typography>
              <TableContainer>
                <Table size="small">
                  <TableHead>
                    <TableRow>
                      <TableCell>Funcionário</TableCell>
                      <TableCell>Setor</TableCell>
                      <TableCell>Data</TableCell>
                      <TableCell align="center">Itens</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {dashboard?.recentDeliveries.map((d) => (
                      <TableRow key={d.deliveryId} hover>
                        <TableCell>{d.employeeName}</TableCell>
                        <TableCell>{d.sectorName}</TableCell>
                        <TableCell>{format(new Date(d.deliveryDate), 'dd/MM/yyyy HH:mm', { locale: ptBR })}</TableCell>
                        <TableCell align="center"><Chip label={d.itemsCount} size="small" color="primary" /></TableCell>
                      </TableRow>
                    ))}
                    {!dashboard?.recentDeliveries.length && (
                      <TableRow>
                        <TableCell colSpan={4} align="center" sx={{ py: 3, color: 'text.secondary' }}>
                          Nenhuma entrega nos últimos 7 dias
                        </TableCell>
                      </TableRow>
                    )}
                  </TableBody>
                </Table>
              </TableContainer>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Box>
  )
}
