var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();


// Serve static files from wwwroot
app.UseStaticFiles();

// Route: redirect email confirmation request to the frontend page
app.MapGet("/email-confirmation", context =>
{
    context.Response.Redirect("/EmailConfirmationFrontend/EmailConfirmationFrontend.html" + context.Request.QueryString);
    return Task.CompletedTask;
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
