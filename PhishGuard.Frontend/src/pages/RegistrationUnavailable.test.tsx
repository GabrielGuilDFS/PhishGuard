import { render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import RegistrationUnavailable from './RegistrationUnavailable';

describe('RegistrationUnavailable', () => {
  it('informa o bloqueio temporário e oferece acesso ao login', () => {
    render(
      <MemoryRouter>
        <RegistrationUnavailable />
      </MemoryRouter>,
    );

    expect(screen.getByRole('heading', { name: /cadastro temporariamente indisponível/i }))
      .toBeInTheDocument();
    expect(screen.getByRole('link', { name: /ir para o login/i }))
      .toHaveAttribute('href', '/login');
  });
});
