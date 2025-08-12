namespace HydroLink.Services
{
    public interface IInventarioService
    {
        Task<bool> ReducirInventarioAsync(int componenteId, decimal cantidad);
        Task<bool> AumentarInventarioAsync(int componenteId, decimal cantidad);
        Task<decimal> ObtenerExistenciaAsync(int componenteId);
        Task<bool> ValidarExistenciaSuficienteAsync(int componenteId, decimal cantidadRequerida);
    }
}
