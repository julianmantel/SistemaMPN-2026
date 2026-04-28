namespace SistemaMPN.Modules.Autentificacion.Services
{
    public interface IBackupService
    {
        Task<string> CreateBackupAsync(CancellationToken cancellationToken = default);
        Task RestoreBackupAsync(string fileName, CancellationToken cancellationToken = default); 
        IEnumerable<FileInfo> GetBackups();
        string GetBackupPath(string fileName);
    }
}
