// O catálogo usa os IDs originais anteriores a 460dc77. Registros persistidos
// com os IDs adotados depois desse commit continuam resolvendo para o mesmo cenário.
export function normalizarIdIsca(id: string | undefined): string | undefined {
  return id === 'mercadoliv-novo-acesso' ? 'mercado-liv-novo-acesso' : id;
}

export function normalizarIdLanding(id: string | undefined): string | undefined {
  return id === 'mercadoliv-login' ? 'mercado-liv-login' : id;
}
