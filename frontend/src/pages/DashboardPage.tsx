import { useQuery } from '@tanstack/react-query'
import {
  Box, Grid, Card, CardContent, Typography, Chip,
  Table, TableBody, TableCell, TableContainer, TableHead, TableRow,
  CircularProgress,
} from '@mui/material'
import {
  LocalShipping, People, Warning, CheckCircle,
} from '@mui/icons-material'
import { deliveriesApi } from '../api/deliveries'
import { format } from 'date-fns'
import { ptBR } from 'date-fns/locale'

function StatCard({
  title, value, icon, color,
}: {
  title: string
  value: number
  icon: React.ReactNode
  color: string
}) {
  return (
    <Card>
      <CardContent>
        <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
          <Box>
            <Typography variant="body2" color="text.secondary">{title}</Typography>
            <Typography variant="h4" fontWeight={700}>{value}</Typography>
          </Box>
          <Box sx={{ bgcolor: color, borderRadius: 2, p: 1.5, color: 'white' }}>{icon}</Box>
        </Box>
      </CardContent>
    </Card>
  )
}

export default function DashboardPage() {
  const { data, isLoading } = useQuery({
    queryKey: ['dashboard'],
    queryFn: deliveriesApi.getDashboard,
    refetchInterval: 30000,
  })

  if (isLoading) return <Box sx={{ display: 'flex', justifyContent: 'center', mt: 4 }}><CircularProgress /></Box>

  return (
    <Box>
      <Typography variant="h5" sx={{ mb: 3 }}>Dashboard</Typography>

      <Grid container spacing={3} sx={{ mb: 4 }}>
        <Grid item xs={12} sm={6} md={3}>
          <StatCard
            title="Entregas Hoje"
            value={data?.todayDeliveries ?? 0}
            icon={<LocalShipping />}
            color="#1565C0"
          />
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <StatCard
            title="Funcionários Atendidos"
            value={data?.todayEmployeesAttended ?? 0}
            icon={<People />}
            color="#2E7D32"
          />
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <StatCard
            title="EPIs Vencendo (30 dias)"
            value={data?.expiringEpisNext30Days ?? 0}
            icon={<Warning />}
            color="#E65100"
          />
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <StatCard
            title="Funcionários Ativos"
            value={data?.activeEmployees ?? 0}
            icon={<CheckCircle />}
            color="#6A1B9A"
          />
        </Grid>
      </Grid>

      <Card>
        <CardContent>
          <Typography variant="h6" sx={{ mb: 2 }}>Entregas Recentes</Typography>
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
                {data?.recentDeliveries.map((d) => (
                  <TableRow key={d.deliveryId} hover>
                    <TableCell>{d.employeeName}</TableCell>
                    <TableCell>{d.sectorName}</TableCell>
                    <TableCell>
                      {format(new Date(d.deliveryDate), 'dd/MM/yyyy HH:mm', { locale: ptBR })}
                    </TableCell>
                    <TableCell align="center">
                      <Chip label={d.itemsCount} size="small" color="primary" />
                    </TableCell>
                  </TableRow>
                ))}
                {!data?.recentDeliveries.length && (
                  <TableRow>
                    <TableCell colSpan={4} align="center" sx={{ py: 3, color: 'text.secondary' }}>
                      Nenhuma entrega recente
                    </TableCell>
                  </TableRow>
                )}
              </TableBody>
            </Table>
          </TableContainer>
        </CardContent>
      </Card>
    </Box>
  )
}
