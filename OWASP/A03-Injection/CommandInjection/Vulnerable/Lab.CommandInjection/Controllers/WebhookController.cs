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
            // Minimal null check
            if (payload.Repo is null)
                return BadRequest("Repo is required");

            // VULNERABILITY:
            // The raw user input is directly passed to the command string.
            // This allows command injection via shell metacharacters (&, |, >, etc.)
            string command = payload.Repo;

            try
            {
                // Configure process execution
                var processInfo = new ProcessStartInfo("cmd.exe", "/c " + command)
                {
                    UseShellExecute = false, // Prevent shell execution, we control IO
                    CreateNoWindow = true, // Do not show a console window
                    RedirectStandardOutput = true, // Capture STDOUT
                    RedirectStandardError = true, // Capture STDERR
                    WorkingDirectory = Directory.GetCurrentDirectory() // Set working dir (deterministic file location)
                };

                // Start the process
                var process = Process.Start(processInfo);

                // Read STDOUT and STDERR (can be empty if command only writes to a file)
                string output = process!.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                // Wait until the process exits
                process.WaitForExit();

                // Return the status and what the process wrote
                return Ok(new { status = "executed", output, error });
            }
            catch (Exception e)
            {
                // In real apps, never expose stack traces to the client
                // Here we log to console for debugging in the lab
                Console.WriteLine(e);
                throw; // Generate a 500 error
            }
        }
    }
}
