# Dashboard e validação de sessão

## Carregamento e início de campanhas

O módulo inicial do dashboard não importa Recharts: os painéis são chunks lazy,
com skeleton enquanto carregam. KPIs e tabela ficam disponíveis independentemente
dos gráficos; o glossário é baixado quando aberto. Isso reduz o JavaScript necessário
para apresentar a tela inicial, não o total baixado ao visualizar todos os gráficos.

Ativação e retry notificam CampaignDispatchSignal somente após salvar o estado.
O sinal é singleton e agrupa avisos, sem guardar campanhas; o banco permanece como
fila durável. O worker continua usando os claims e a idempotência existentes e
consulta a fila no startup e após até 10s de espera sem aviso. Se estiver enviando
outro lote, atende o aviso ao terminar. O sinal é local; outras instâncias dependem
da varredura. Não garante início imediato com worker ocupado ou host suspenso.

A tela aplica o status confirmado pela ativação/retry e consulta a cada 2s durante
Processando (8s para Agendada), aguardando a resposta antes do próximo poll.
Consultas periódicas são pausadas com a aba oculta e param nos estados terminais.

O dashboard consulta departamentos distintos, filtra alvos por subconsulta e conta
os envios do ciclo anterior no banco. Apenas a primeira ocorrência de cada ação por
campanha/alvo do ciclo atual é materializada. O gráfico ordena esses eventos uma
vez e os percorre incrementalmente, preservando a coorte de envios do ciclo inteiro,
o fuso civil e a inferência de abertura. Ainda há consumo proporcional aos pares
campanha/alvo do período atual; esta mudança não é uma tabela de agregações diárias.

## Cache de sessões

`AuthSessionCache__SingleInstanceEnabled=true` habilita o cache de validações
positivas por até 10 segundos, limitado a 10.000 entradas. A expiração da sessão
limita esse prazo; acertos não renovam o TTL. A chave contém administrador, tenant
e sessão. JWT, assinatura, claims e expiração do access token continuam validados
em toda requisição. Falhas de consulta não são convertidas em autorização.

Habilitado em Development; desabilitado por padrão em Production. Para ativar em
produção, confirme que há **uma única instância** e que todas as alterações de
AuthSession, Administrador e Tenant passam pelo SaveChanges deste processo.
Não habilite para múltiplas réplicas, scripts SQL externos, ExecuteUpdate/Delete
nessas entidades ou outro serviço escrevendo autenticação: exigem invalidação
distribuída. Sem isso, alterações externas podem levar até 10 segundos para aparecer.

SaveChanges invalida antes e depois da persistência, incluindo falhas. Um contador
de geração impede que consultas iniciadas antes da invalidação repovoem o cache.
Escritas de autenticação em transações explícitas/ambientes desligam o cache até
o reinício do processo, porque SaveChanges não representa o commit nesse caso.
O cache é conservador: qualquer escrita nessas entidades invalida todas as entradas.

## Verificação operacional

Compare latência e alocações de `/api/Dashboard/overview` com o mesmo dataset e
períodos 7d/30d/90d. Use logs de comandos EF para verificar COUNT/DISTINCT/GROUP BY
no banco e planos PostgreSQL para avaliar o índice existente.
Com cache habilitado, requests autenticados sucessivos da mesma sessão não devem
executar novamente a consulta de validação dentro de 10 segundos. Verifique logout,
troca de senha, desativação do tenant e expiração antes de liberar tráfego.
