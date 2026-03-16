import { createTheme } from '@mui/material/styles'
import { ptBR } from '@mui/material/locale'

export const theme = createTheme(
  {
    palette: {
      primary: {
        main: '#1565C0',
        light: '#1976D2',
        dark: '#0D47A1',
      },
      secondary: {
        main: '#FF6F00',
        light: '#FFA000',
        dark: '#E65100',
      },
      background: {
        default: '#F5F5F5',
      },
    },
    typography: {
      fontFamily: '"Roboto", "Helvetica", "Arial", sans-serif',
      h4: { fontWeight: 700 },
      h5: { fontWeight: 600 },
      h6: { fontWeight: 600 },
    },
    shape: {
      borderRadius: 8,
    },
    components: {
      MuiButton: {
        styleOverrides: {
          root: { textTransform: 'none', fontWeight: 600 },
        },
      },
      MuiCard: {
        styleOverrides: {
          root: { boxShadow: '0 2px 8px rgba(0,0,0,0.1)' },
        },
      },
    },
  },
  ptBR,
)
