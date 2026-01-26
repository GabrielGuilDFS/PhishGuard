namespace PhishGuard.Backend.Models
{
    public class Alvo
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Setor { get; set; } = string.Empty;
        public int AdminId { get; set; }
    }
}