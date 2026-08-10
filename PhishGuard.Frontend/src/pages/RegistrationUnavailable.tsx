import { Alert, Box, Button, Container, Paper, Typography } from '@mui/material';
import { Link as RouterLink } from 'react-router-dom';

export default function RegistrationUnavailable() {
  return (
    <Box sx={{ minHeight: '100vh', display: 'grid', placeItems: 'center', px: 2, bgcolor: 'background.default' }}>
      <Container maxWidth="sm">
        <Paper elevation={4} sx={{ p: { xs: 3, sm: 5 }, textAlign: 'center' }}>
          <Typography component="h1" variant="h4" fontWeight={800} gutterBottom>
            Cadastro temporariamente indisponível
          </Typography>
          <Alert severity="info" sx={{ mt: 2, mb: 3, textAlign: 'left' }}>
            Novas contas estão desabilitadas durante o ambiente de demonstração do PhishGuard.
            Contas já provisionadas continuam acessando normalmente.
          </Alert>
          <Button component={RouterLink} to="/login" variant="contained" size="large">
            Ir para o login
          </Button>
        </Paper>
      </Container>
    </Box>
  );
}
