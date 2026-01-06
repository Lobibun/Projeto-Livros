namespace LivroCDF.ViewModels
{
    public class HomeViewModel
    {
        public int TotalLivrosCadastrados { get; set; }
        public int TotalClientes { get; set; }
        public int QtdEmEstoque { get; set; }
        public int QtdVendidos { get; set; }
        public int QtdAPagar { get; set; }
        public float ValorTotalAPagar { get; set; } 
    }
}