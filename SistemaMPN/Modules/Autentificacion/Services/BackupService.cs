using Npgsql;
using System.Diagnostics;

namespace SistemaMPN.Modules.Autentificacion.Services
{
    public class BackupService : IBackupService
    {
        private readonly IConfiguration _config;
        private readonly string _backupFolder;
        private readonly string _pgDumpPath; 
        private readonly string _psqlPath;   

        public BackupService(IConfiguration config)
        {
            _config = config;
            
            _backupFolder = Path.Combine(Directory.GetCurrentDirectory(), "BackupsDB");
            if (!Directory.Exists(_backupFolder)) Directory.CreateDirectory(_backupFolder);

            _pgDumpPath = config["Paths:PGDumpPath"] ?? string.Empty;
            _psqlPath = config["Paths:PSQLPath"] ?? string.Empty;
        }

        public async Task<string> CreateBackupAsync(CancellationToken cancellationToken = default)
        {
            if (!File.Exists(_pgDumpPath))
            {
                throw new FileNotFoundException("La ruta de PGDump es incorrecta (modificar en los settings) -> ", _pgDumpPath);
            }

            var connectionString = _config.GetConnectionString("DefaultConnection");
            var builder = new NpgsqlConnectionStringBuilder(connectionString);

            var fileName = $"backup_{DateTime.Now:yyyyMMdd_HHmmss}.sql";
            var filePath = Path.Combine(_backupFolder, fileName);

            var startInfo = new ProcessStartInfo
            {
                FileName = _pgDumpPath,
                Arguments = $"-h {builder.Host} -U {builder.Username} -d {builder.Database} " + $"--clean --if-exists -f \"{filePath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            startInfo.EnvironmentVariables["PGPASSWORD"] = builder.Password;

            using var process = new Process { StartInfo = startInfo };
            process.Start();

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var waitTask = process.WaitForExitAsync(linkedCts.Token);
            var timeoutTask = Task.Delay(TimeSpan.FromMinutes(30), cancellationToken);
            var completedTask = await Task.WhenAny(waitTask, timeoutTask);

            if (completedTask == timeoutTask && !waitTask.IsCompleted)
            {
                process.Kill(true);
                throw new OperationCanceledException("El proceso de backup excedió el tiempo límite.");
            }

            await waitTask;

            if (process.ExitCode != 0)
            {
                var error = await process.StandardError.ReadToEndAsync();
                throw new Exception($"Error en pg_dump: {error}");
            }

            return fileName;
        }

        public async Task RestoreBackupAsync(string fileName, CancellationToken cancellationToken = default)
        {
            if (!File.Exists(_psqlPath))
            {
                throw new FileNotFoundException("La ruta de PSQL es incorrecta (modificar en los settings) -> ", _psqlPath);
            }

            if (string.IsNullOrWhiteSpace(fileName) || fileName.Contains("..") ||
                Path.IsPathRooted(fileName) || fileName.Contains('/') || fileName.Contains('\\'))
            {
                throw new ArgumentException("Nombre de archivo inválido.", nameof(fileName));
            }

            var filePath = Path.Combine(_backupFolder, fileName);
            if (!File.Exists(filePath)) throw new FileNotFoundException("El archivo de backup no existe.", fileName);

            var connectionString = _config.GetConnectionString("DefaultConnection");
            var builder = new NpgsqlConnectionStringBuilder(connectionString);

            await TerminateConnections(builder.Database, builder, cancellationToken);
            await Task.Delay(1000, cancellationToken);

            var startInfo = new ProcessStartInfo
            {
                FileName = _psqlPath,
                Arguments = $"-h {builder.Host} -U {builder.Username} -d {builder.Database} -f \"{filePath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            startInfo.EnvironmentVariables["PGPASSWORD"] = builder.Password;

            using var process = new Process { StartInfo = startInfo };
            process.Start();

            var errorTask = process.StandardError.ReadToEndAsync();
            var outputTask = process.StandardOutput.ReadToEndAsync();

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var waitTask = process.WaitForExitAsync(linkedCts.Token);
            var timeoutTask = Task.Delay(TimeSpan.FromMinutes(30), cancellationToken);
            var completedTask = await Task.WhenAny(waitTask, timeoutTask);

            if (completedTask == timeoutTask && !waitTask.IsCompleted)
            {
                process.Kill(true);
                throw new OperationCanceledException("El proceso de restauración excedió el tiempo límite.");
            }

            await waitTask;

            var error = await errorTask;

            if (process.ExitCode != 0)
            {
                throw new Exception($"Error en psql restore: {error}");
            }
        }

        private async Task TerminateConnections(string dbName, NpgsqlConnectionStringBuilder builder, CancellationToken cancellationToken = default)
        {
            var adminBuilder = new NpgsqlConnectionStringBuilder(builder.ConnectionString)
            {
                Database = "postgres"
            };

            using var conn = new NpgsqlConnection(adminBuilder.ConnectionString);
            await conn.OpenAsync(cancellationToken);

            var sql = "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = @dbName AND pid <> pg_backend_pid();";
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@dbName", dbName);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }

        public IEnumerable<FileInfo> GetBackups()
        {
            var directory = new DirectoryInfo(_backupFolder);
            return directory.GetFiles("*.sql").OrderByDescending(f => f.LastWriteTime);
        }

        public string GetBackupPath(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName) || fileName.Contains("..") || 
                Path.IsPathRooted(fileName) || fileName.Contains('/') || fileName.Contains('\\'))
            {
                throw new ArgumentException("Nombre de archivo inválido.", nameof(fileName));
            }

            var filePath = Path.Combine(_backupFolder, fileName);
            var fullPath = Path.GetFullPath(filePath);

            if (!fullPath.StartsWith(Path.GetFullPath(_backupFolder)))
            {
                throw new ArgumentException("Ruta de archivo fuera del directorio de backups.", nameof(fileName));
            }

            return filePath;
        }
    }
}
