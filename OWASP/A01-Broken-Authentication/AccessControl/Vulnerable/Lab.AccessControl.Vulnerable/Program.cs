using Lab.AccessControl.Vulnerable.Data;
using Lab.AccessControl.Vulnerable.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

#region BUILDER

var builder = WebApplication.CreateBuilder(args);
var jwtSecretKey = builder.Configuration.GetValue<string>("incredibly_long_and_secure_key") ?? "incredibly_long_and_secure_key";

#endregion

#region AUTHENTICATION-AUTHORIZATION

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
 {
     options.RequireHttpsMetadata = false;
     options.SaveToken = true;
     options.TokenValidationParameters = new TokenValidationParameters
     {
         ValidateIssuer = false,
         ValidateAudience = false,
         ValidateLifetime = true,
         ValidateIssuerSigningKey = true,
         IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey))
     };
 });

#endregion

#region SERVICES & DI

builder.Services.AddAuthorization();
builder.Services.AddControllers();

builder.Services.AddSingleton<InMemoryStore>();
builder.Services.AddSingleton<TokenService>(token => new TokenService(jwtSecretKey));

#endregion

#region PIPELINE

var app = builder.Build();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();

#endregion
