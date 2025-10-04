# dotnet-app-sec-labs

A collection of intentionally vulnerable labs for learning **Application Security in .NET / ASP.NET Core**.  
Each lab is a minimal Web API that demonstrates a specific vulnerability class, useful for security testing, research, and education.

---

Labs list:

| Lab | Vulnerability | Description |
|-----|---------------|-------------|
| [Lab.CommandInjection](./OWASP/A03-Injection/CommandInjection) | Command Injection | Example of shell command injection via unsanitized input |
| [Lab.AccessControl](./OWASP/A01-Broken-Authentication/AccessControl) | Access Control | Example of broken access control (IDOR): missing owner check on resource |
