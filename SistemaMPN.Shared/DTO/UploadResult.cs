using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaMPN.Shared.DTO
{
    public class UploadResult
    {
        public string NodeId { get; set; } = "";
        public string FileName { get; set; } = "";
        public long Size { get; set; }
        public string? PublicLink { get; set; }
    }
}
