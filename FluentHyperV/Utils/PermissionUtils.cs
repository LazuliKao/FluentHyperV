namespace FluentHyperV.Utils;

using System.Diagnostics;
using System.Security.Principal;

public class PermissionUtils
{
    public static void InsureAdministrator()
    {
        if (!IsAdministrator())
        {
            // 重新启动自身并请求管理员权限
            var processInfo = new ProcessStartInfo
            {
                FileName = Environment.ProcessPath,
                UseShellExecute = true,
                Verb = "runas",
            };
            try
            {
                Process.Start(processInfo);
            }
            catch (Exception ex)
            {
                throw new Exception("Administrator privileges are required to run this application.", ex);
            }
        }
    }

    public static bool IsAdministrator()
    {
        using WindowsIdentity identity = WindowsIdentity.GetCurrent();
        WindowsPrincipal principal = new(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }
}
