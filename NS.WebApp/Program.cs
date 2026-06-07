var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSoloDBDataServices(
    builder.Configuration.GetValue<string>("SoloDB:DatabasePath") ?? "nimble-sheet.db");

builder.Services.AddFastEndpoints(o => o.Assemblies = [typeof(AssemblyMarker).Assembly]);

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new()
        {
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SigningKey"]!)),
            NameClaimType = JwtRegisteredClaimNames.Name,
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
        };
    });

builder.Services.AddAuthorization(options =>
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build());

var app = builder.Build();

app.UseHttpsRedirection();

// Serve the SvelteKit SPA (copied into wwwroot at publish) as same-origin static
// content. Static files are served before auth, so assets load without a token.
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();
app.UseFastEndpoints();

// SPA fallback: any non-API, non-file request returns the app shell so client-side
// routing (deep links such as the login page) works. AllowAnonymous so the shell
// loads without a token; the API endpoints keep their own auth requirements.
app.MapFallbackToFile("index.html").AllowAnonymous();

app.Run();
