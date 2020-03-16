using Microsoft.AspNetCore.Http;

namespace Sitko.Core.Email
{
    public abstract class EmailModuleConfig
    {
        public HostString Host { get; } = new HostString("localhost");
        public string Scheme { get; set; } = "localhost";
    }
}
