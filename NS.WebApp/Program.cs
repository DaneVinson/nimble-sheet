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

await app.Services.GetRequiredService<IReferenceDataSeeder>().SeedAsync();

app.UseHttpsRedirection();

// Serve the SvelteKit SPA (copied into wwwroot at publish) as same-origin static
// content. Static files are served before auth, so assets load without a token.
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

// All API endpoints are served under the "/api" prefix so they never collide with
// SPA client routes (e.g. /heroes, /heroes/{id}). This lets the fallback below return
// the app shell for those deep links instead of the API answering them.
// Serialize enums by name (e.g. "Oathsworn", "D10") and accept names or integers on input.
app.UseFastEndpoints(c =>
{
    c.Endpoints.RoutePrefix = "api";
    c.Serializer.Options.Converters.Add(new JsonStringEnumConverter());
});

// SPA fallback: any non-API, non-file request returns the app shell so client-side
// routing and deep links (e.g. refreshing on /heroes/{id}) work. AllowAnonymous so the
// shell loads without a token; the /api endpoints keep their own auth requirements.
app.MapFallbackToFile("index.html").AllowAnonymous();

app.Run();
