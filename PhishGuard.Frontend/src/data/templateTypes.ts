// Tipo compartilhado dos moldes estáticos (landing pages e páginas educacionais).
// Extraído do antigo HtmlEditorView para desacoplar os catálogos de dados do
// componente de edição (removido na refatoração de Cenários de Simulação).
export interface TemplateModel {
  id: string;
  nome: string;
  html: string;
  categoria?: string;
}
