import { beforeEach, afterEach, describe, expect, it, vi } from 'vitest';
import { fireEvent, render, screen } from '@testing-library/react';
import Targets from './Targets';
import { NotificationProvider } from '../context/NotificationContext';

const authFetch = vi.hoisted(() => vi.fn());
vi.mock('../auth/session', () => ({ authFetch, getToken: () => 'test-token' }));

beforeEach(() => {
  authFetch.mockReset();
  authFetch.mockImplementation(async (url: string, init?: RequestInit) => {
    if (init?.method === 'POST' || init?.method === 'DELETE')
      return new Response(JSON.stringify({ message: init.method === 'POST'
        ? 'Este e-mail já está cadastrado neste tenant.'
        : 'Este alvo possui histórico e não pode ser excluído.' }), { status: 409 });
    return new Response(JSON.stringify(url.includes('/quota')
      ? { limiteAlvos: 50, plano: 'Bronze' }
      : [{ id: 't1', nome: 'Pessoa', email: 'pessoa@example.test', departamento: 'TI' }]), { status: 200 });
  });
});
afterEach(() => vi.restoreAllMocks());

describe('Targets — conflitos de integridade', () => {
  it('permite corrigir o cadastro e explica que campanhas ativadas preservam os destinatários', async () => {
    authFetch.mockImplementation(async (url: string, init?: RequestInit) => {
      if (init?.method === 'PUT') return new Response(null, { status: 204 });
      return new Response(JSON.stringify(url.includes('/quota')
        ? { limiteAlvos: 50, plano: 'Bronze' }
        : [{ id: 't1', nome: 'Pessoa', email: 'pessoa@example.test', departamento: 'TI' }]), { status: 200 });
    });
    render(<NotificationProvider><Targets /></NotificationProvider>);
    await screen.findByText('pessoa@example.test');
    fireEvent.click(screen.getByRole('button', { name: 'Editar alvo' }));
    expect(screen.getByText(/Campanhas já ativadas preservam o nome/)).toBeInTheDocument();
    fireEvent.change(screen.getByLabelText('E-mail Corporativo'), { target: { value: 'novo@example.test' } });
    fireEvent.change(screen.getByLabelText('Departamento'), { target: { value: 'Financeiro' } });
    fireEvent.click(screen.getByRole('button', { name: 'Salvar' }));
    expect(await screen.findByText('Alvo atualizado!')).toBeInTheDocument();
    expect(authFetch.mock.calls.some(([url, init]) => url.endsWith('/Targets/t1') && init?.method === 'PUT'
      && JSON.parse(init.body).email === 'novo@example.test' && JSON.parse(init.body).departamento === 'Financeiro')).toBe(true);
  });

  it('mantém o formulário aberto e explica a duplicidade', async () => {
    render(<NotificationProvider><Targets /></NotificationProvider>);
    await screen.findByText('pessoa@example.test');
    fireEvent.click(screen.getByRole('button', { name: 'Novo Alvo' }));
    fireEvent.change(screen.getByLabelText('Nome Completo'), { target: { value: 'Pessoa' } });
    fireEvent.change(screen.getByLabelText('E-mail Corporativo'), { target: { value: 'pessoa@example.test' } });
    fireEvent.change(screen.getByLabelText('Departamento'), { target: { value: 'TI' } });
    fireEvent.click(screen.getByRole('button', { name: 'Salvar' }));
    expect(await screen.findByText('Este e-mail já está cadastrado neste tenant.')).toBeInTheDocument();
    expect(screen.getByRole('dialog')).toBeInTheDocument();
  });

  it('explica o bloqueio de exclusão e mantém o alvo na tabela', async () => {
    vi.spyOn(window, 'confirm').mockReturnValue(true);
    render(<NotificationProvider><Targets /></NotificationProvider>);
    await screen.findByText('pessoa@example.test');
    fireEvent.click(screen.getByRole('button', { name: 'Excluir alvo' }));
    expect(await screen.findByText('Este alvo possui histórico e não pode ser excluído.')).toBeInTheDocument();
    expect(screen.getByText('pessoa@example.test')).toBeInTheDocument();
  });
});
