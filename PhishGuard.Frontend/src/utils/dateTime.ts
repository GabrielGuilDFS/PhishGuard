// Converte um instante ISO (UTC, vindo da API) para o valor de um <input type="datetime-local">,
// que espera a HORA LOCAL (wall-clock) no formato 'YYYY-MM-DDTHH:mm'. Antes fazia só
// `iso.slice(0,16)`, o que jogava a hora UTC crua dentro do input local — a data exibida (e
// re-enviada ao editar) escorregava pelo offset do fuso a cada round-trip. Aqui compensamos o
// offset; usa só getTime() (epoch) + getTimezoneOffset() + toISOString() (UTC) → determinístico.
export const toDateTimeLocal = (iso?: string | null): string => {
    if (!iso) return '';
    const d = new Date(iso);
    if (Number.isNaN(d.getTime())) return '';
    const localMs = d.getTime() - d.getTimezoneOffset() * 60000;
    return new Date(localMs).toISOString().slice(0, 16);
};
