using System.Security.Claims;
using AuthenticationDemo.Auth;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<UserContext>();

builder.Services.AddAuthentication(o =>
    {
        o.DefaultScheme = IdentityConstants.ApplicationScheme;
        o.DefaultChallengeScheme = "oidc";
    })
    .AddOpenIdConnect("oidc", "Duende IdentityServer", options =>
    {
        options.ForwardDefaultSelector = ctx =>
        {
            var hasLocal = ctx.Request.Query["src"] == "local";
            return hasLocal ? IdentityConstants.ApplicationScheme : null;
        };
        options.Authority = "https://demo.duendesoftware.com";
        options.ClientId = "interactive.confidential";
        options.ClientSecret = "secret";
        options.ResponseType = "code";
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");
        options.Scope.Add("api");
        options.Scope.Add("offline_access");
        options.GetClaimsFromUserInfoEndpoint = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = "email"
        };
        options.Events.OnTokenValidated = DoClaimsTransformation;
        options.SaveTokens = true;
    }).AddIdentityCookies();


builder.Services.AddIdentityCore<AppUser>(o =>
    {
        o.Stores.MaxLengthForKeys = 128;
        o.SignIn.RequireConfirmedAccount = true;

    })
    .AddDefaultUI()
    .AddDefaultTokenProviders()
    .AddEntityFrameworkStores<UserContext>();


builder.Services.AddRazorPages();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    
    var userCtx = services.GetRequiredService<UserContext>();
    userCtx.Database.Migrate();

    if (app.Environment.IsDevelopment())
    {
        var userMgr = services.GetRequiredService<UserManager<AppUser>>();
        await DevUserHelper.CreateDevUsers(userMgr);
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseRouting();
app.UseAuthorization();

app.MapRazorPages();

app.Run();

async Task DoClaimsTransformation(TokenValidatedContext context)
{
    var signInMgr = context.HttpContext.RequestServices.GetRequiredService<SignInManager<AppUser>>();
    var userMgr = context.HttpContext.RequestServices.GetRequiredService<UserManager<AppUser>>();

    var claims = context.Principal?.Claims.ToList() ?? new List<Claim>();
    var subjectId = claims.FirstOrDefault(c => c.Type.Contains("nameidentifier"))?.Value;

    var user = await userMgr.Users.FirstOrDefaultAsync(u => u.ExternalSubjectId == subjectId);
    claims.AddRange(await userMgr.GetClaimsAsync(user));
    var newIdentity = new ClaimsIdentity(claims, context.Principal.Identity.AuthenticationType, "name", "role");
    context.Principal = new ClaimsPrincipal(newIdentity);
    await signInMgr.SignInWithClaimsAsync(user, false, claims);
}