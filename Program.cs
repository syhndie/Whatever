using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using Whatever.Components;
using Whatever.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentityCore<IdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.User.RequireUniqueEmail = true;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;
    })
    .AddSignInManager()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddIdentityCookies();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "Whatever.Auth";
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
    options.AccessDeniedPath = "/login";
    options.SlidingExpiration = true;
});

builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapPost("/account/login", async (
    SignInManager<IdentityUser> signInManager,
    [FromForm] string email,
    [FromForm] string password,
    [FromForm] bool? rememberMe,
    [FromForm] string? returnUrl) =>
{
    var redirectUrl = NormalizeLocalReturnUrl(returnUrl);
    var result = await signInManager.PasswordSignInAsync(email, password, rememberMe ?? false, lockoutOnFailure: false);

    if (result.Succeeded)
    {
        return Results.LocalRedirect(redirectUrl);
    }

    var error = Uri.EscapeDataString("Invalid login attempt.");
    return Results.LocalRedirect($"/login?error={error}&returnUrl={Uri.EscapeDataString(redirectUrl)}");
}).DisableAntiforgery();

app.MapPost("/account/register", async (
    UserManager<IdentityUser> userManager,
    SignInManager<IdentityUser> signInManager,
    [FromForm] string email,
    [FromForm] string password,
    [FromForm] string confirmPassword,
    [FromForm] string? returnUrl) =>
{
    var redirectUrl = NormalizeLocalReturnUrl(returnUrl);

    if (!string.Equals(password, confirmPassword, StringComparison.Ordinal))
    {
        var mismatch = Uri.EscapeDataString("Passwords do not match.");
        return Results.LocalRedirect($"/register?error={mismatch}&returnUrl={Uri.EscapeDataString(redirectUrl)}");
    }

    var user = new IdentityUser
    {
        UserName = email,
        Email = email
    };

    var createResult = await userManager.CreateAsync(user, password);
    if (!createResult.Succeeded)
    {
        var firstError = createResult.Errors.FirstOrDefault()?.Description ?? "Registration failed.";
        var encodedError = Uri.EscapeDataString(firstError);
        return Results.LocalRedirect($"/register?error={encodedError}&returnUrl={Uri.EscapeDataString(redirectUrl)}");
    }

    await signInManager.SignInAsync(user, isPersistent: false);
    return Results.LocalRedirect(redirectUrl);
}).DisableAntiforgery();

app.MapPost("/account/logout", async (
    SignInManager<IdentityUser> signInManager,
    [FromForm] string? returnUrl) =>
{
    await signInManager.SignOutAsync();
    return Results.LocalRedirect(NormalizeLocalReturnUrl(returnUrl));
}).DisableAntiforgery();

app.Run();

static string NormalizeLocalReturnUrl(string? returnUrl)
{
    if (string.IsNullOrWhiteSpace(returnUrl) || !Uri.TryCreate(returnUrl, UriKind.Relative, out _)
        || returnUrl.StartsWith("//", StringComparison.Ordinal))
    {
        return "/";
    }

    return returnUrl;
}
