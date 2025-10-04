using Lab.CommandInjection.Fixed.Models;
using System.Text.RegularExpressions;

namespace Lab.CommandInjection.Fixed.Controllers
{
    [ApiController]
    [Route("webhook-safe")] 
    public class WebhookSafeController : ControllerBase
    {
        [HttpPost]
        [Consumes("application/json")]
        public async Task<IActionResult> ReceiveWebhookAsync([FromBody] RepoSafePayload payload)
        {
            #region EDGE CASES

            // Minimal null check
            if (string.IsNullOrWhiteSpace(payload?.Repo))
                return BadRequest("Repo is required.");

            // Validate URL format
            if (!Uri.TryCreate(payload.Repo, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
                return BadRequest("Repo must be a valid HTTPS URL.");

            // ### ALLOW LIST hosts
            // Adjust the allowlist to your needs (e.g. "github.com", "gitlab.com", etc.)
            var allowedHosts = new[] { "github.com", "raw.githubusercontent.com" };
            if (!allowedHosts.Any(h => string.Equals(uri.Host, h, StringComparison.OrdinalIgnoreCase)))
                return BadRequest("Repository host is not allowed.");

            #endregion

            #region DEFENSIVE APPROACH

            // Block obvious shell metacharacters from the input.
            // This is an extra layer main protection is using ArgumentList
            var forbidden = new Regex(@"[&|;<>`$\\\r\n]", RegexOptions.Compiled);
            if (forbidden.IsMatch(payload.Repo))
                return BadRequest("Repository URL contains forbidden characters.");

            #endregion

            try
            {
                #region SAFE WAY

                // invoke git as an executable with argument list (no cmd.exe or other)
                var processInfo = new ProcessStartInfo
                {
                    FileName = "git", // Implement full path if known (e.g. "C:\Program Files\Git\bin\git.exe")
                    UseShellExecute = false, // required to redirect output and avoid shell interpretation
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = Directory.GetCurrentDirectory()
                };

                // Add arguments as separate tokens (no string concatenation)
                processInfo.ArgumentList.Add("clone");
                processInfo.ArgumentList.Add(payload.Repo);

                using var process = Process.Start(processInfo)!;
                if (process == null) return StatusCode(500, "Failed to start process");

                #endregion

                #region GOOD PRACTICE

                // asynchronous reading using tasks to avoid potential deadlock
                Task<string> ReadStdOutAsync() => Task.Run(() => process.StandardOutput.ReadToEnd());
                Task<string> ReadStdErrAsync() => Task.Run(() => process.StandardError.ReadToEnd());

                var readOutTask = ReadStdOutAsync();
                var readErrTask = ReadStdErrAsync();

                // Wait for process exit with timeout
                var exited = process.WaitForExit(30_000);
                if (!exited)
                {
                    try { process.Kill(true); }
                    catch (Exception ex) { Console.WriteLine("[ERROR] Kill failed: " + ex); }
                    return StatusCode(504, "git clone timed out");
                }

                // Ensure output tasks are completed
                var output = await readOutTask;
                var error = await readErrTask;

                return Ok(new { exitCode = process.ExitCode, output, error });

                #endregion

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(500, "Internal error");
            }
        }
    }
}
