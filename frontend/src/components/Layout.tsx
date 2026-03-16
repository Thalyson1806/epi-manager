import { useState } from 'react'
import { Outlet, useNavigate, useLocation } from 'react-router-dom'
import {
  Box, Drawer, AppBar, Toolbar, List, ListItem, ListItemButton,
  ListItemIcon, ListItemText, Typography, IconButton, Avatar,
  Divider, Chip,
} from '@mui/material'
import {
  Dashboard as DashboardIcon,
  People as PeopleIcon,
  Security as SecurityIcon,
  Business as BusinessIcon,
  LocalShipping as DeliveryIcon,
  BarChart as ReportsIcon,
  ManageAccounts as UsersIcon,
  Logout as LogoutIcon,
  Menu as MenuIcon,
} from '@mui/icons-material'
import { useAuthStore } from '../store/authStore'

const DRAWER_WIDTH = 240

const navItems = [
  { label: 'Dashboard', icon: <DashboardIcon />, path: '/dashboard', roles: ['Administrator', 'HR', 'Warehouse'] },
  { label: 'Funcionários', icon: <PeopleIcon />, path: '/employees', roles: ['Administrator', 'HR'] },
  { label: 'EPIs', icon: <SecurityIcon />, path: '/epis', roles: ['Administrator', 'HR'] },
  { label: 'Setores', icon: <BusinessIcon />, path: '/sectors', roles: ['Administrator', 'HR'] },
  { label: 'Entrega de EPI', icon: <DeliveryIcon />, path: '/delivery', roles: ['Administrator', 'Warehouse'] },
  { label: 'Relatórios', icon: <ReportsIcon />, path: '/reports', roles: ['Administrator', 'HR'] },
  { label: 'Usuários', icon: <UsersIcon />, path: '/users', roles: ['Administrator'] },
]

const roleLabel: Record<string, string> = {
  Administrator: 'Administrador',
  HR: 'RH',
  Warehouse: 'Almoxarifado',
}

export default function Layout() {
  const navigate = useNavigate()
  const location = useLocation()
  const { user, logout } = useAuthStore()
  const [mobileOpen, setMobileOpen] = useState(false)

  const handleLogout = () => {
    logout()
    navigate('/login')
  }

  const visibleItems = navItems.filter((item) =>
    item.roles.includes(user?.role ?? ''),
  )

  const drawer = (
    <Box sx={{ display: 'flex', flexDirection: 'column', height: '100%' }}>
      <Box sx={{ p: 2, bgcolor: 'primary.main', color: 'white' }}>
        <Typography variant="h6" fontWeight={700}>
          Gestão de EPI
        </Typography>
        <Typography variant="caption" sx={{ opacity: 0.85 }}>
          Sistema Corporativo
        </Typography>
      </Box>
      <List sx={{ flex: 1, pt: 1 }}>
        {visibleItems.map((item) => (
          <ListItem key={item.path} disablePadding>
            <ListItemButton
              selected={location.pathname.startsWith(item.path)}
              onClick={() => { navigate(item.path); setMobileOpen(false) }}
              sx={{
                mx: 1, borderRadius: 1, mb: 0.5,
                '&.Mui-selected': {
                  bgcolor: 'primary.main',
                  color: 'white',
                  '& .MuiListItemIcon-root': { color: 'white' },
                  '&:hover': { bgcolor: 'primary.dark' },
                },
              }}
            >
              <ListItemIcon sx={{ minWidth: 40 }}>{item.icon}</ListItemIcon>
              <ListItemText primary={item.label} />
            </ListItemButton>
          </ListItem>
        ))}
      </List>
      <Divider />
      <Box sx={{ p: 2 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
          <Avatar sx={{ width: 32, height: 32, bgcolor: 'secondary.main', fontSize: 14 }}>
            {user?.name?.charAt(0)}
          </Avatar>
          <Box sx={{ flex: 1, minWidth: 0 }}>
            <Typography variant="body2" fontWeight={600} noWrap>{user?.name}</Typography>
            <Chip label={roleLabel[user?.role ?? ''] ?? user?.role} size="small" color="primary" variant="outlined" />
          </Box>
        </Box>
        <ListItemButton onClick={handleLogout} sx={{ borderRadius: 1, color: 'error.main' }}>
          <ListItemIcon sx={{ minWidth: 36, color: 'error.main' }}><LogoutIcon fontSize="small" /></ListItemIcon>
          <ListItemText primary="Sair" primaryTypographyProps={{ variant: 'body2' }} />
        </ListItemButton>
      </Box>
    </Box>
  )

  return (
    <Box sx={{ display: 'flex' }}>
      <AppBar position="fixed" sx={{ zIndex: (theme) => theme.zIndex.drawer + 1, display: { sm: 'none' } }}>
        <Toolbar>
          <IconButton color="inherit" edge="start" onClick={() => setMobileOpen(!mobileOpen)}>
            <MenuIcon />
          </IconButton>
          <Typography variant="h6" sx={{ ml: 1 }}>Gestão de EPI</Typography>
        </Toolbar>
      </AppBar>

      <Box component="nav" sx={{ width: { sm: DRAWER_WIDTH }, flexShrink: { sm: 0 } }}>
        <Drawer
          variant="temporary"
          open={mobileOpen}
          onClose={() => setMobileOpen(false)}
          ModalProps={{ keepMounted: true }}
          sx={{ display: { xs: 'block', sm: 'none' }, '& .MuiDrawer-paper': { width: DRAWER_WIDTH } }}
        >
          {drawer}
        </Drawer>
        <Drawer
          variant="permanent"
          sx={{ display: { xs: 'none', sm: 'block' }, '& .MuiDrawer-paper': { width: DRAWER_WIDTH, boxSizing: 'border-box' } }}
          open
        >
          {drawer}
        </Drawer>
      </Box>

      <Box component="main" sx={{ flexGrow: 1, p: 3, width: { sm: `calc(100% - ${DRAWER_WIDTH}px)` }, mt: { xs: 7, sm: 0 } }}>
        <Outlet />
      </Box>
    </Box>
  )
}
