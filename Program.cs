var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
// Register an HttpClient for IBGE API
builder.Services.AddHttpClient("ibge", client =>
{
    client.BaseAddress = new Uri("https://servicodados.ibge.gov.br/");
    client.Timeout = TimeSpan.FromSeconds(30);
});
// Register IBGE ranking service
builder.Services.AddScoped<UCS_Projeto_Integrador_IV_B.Services.IIBGERankingService, UCS_Projeto_Integrador_IV_B.Services.IBGERankingService>();

var app = builder.Build();

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
