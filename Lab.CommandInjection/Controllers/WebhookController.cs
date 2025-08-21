using Lab.CommandInjection.Models;

namespace Lab.CommandInjection.Controllers
{
    [ApiController]
    [Route("webhook")]
    public class WebhookController : ControllerBase
    {
        [HttpPost]
        [Consumes("application/json")]
        public IActionResult ReceiveWebhook([FromBody] RepoPayload payload)
        {
            if (payload.Repo is null)
                return BadRequest("Repo is required");

            string command = payload.Repo;

            try
            {
                var processInfo = new ProcessStartInfo("cmd.exe", "/c " + command)
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WorkingDirectory = Directory.GetCurrentDirectory()
                };

                var process = Process.Start(processInfo);
                string output = process!.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                return Ok(new { status = "executed", output, error });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}