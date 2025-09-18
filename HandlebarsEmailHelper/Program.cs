using HandlebarsEmailHelper.Services;
using HandlebarsEmailHelper.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// EF Core SQLite
builder.Services.AddDbContext<HandlebarsEmailHelper.Models.AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=emails.db"));

// Configure Email options
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection(EmailOptions.SectionName));

// Services
builder.Services.AddScoped<IEmailTemplateService, EmailTemplateRepository>();
builder.Services.AddSingleton<ITemplateService, TemplateService>();
builder.Services.AddScoped<ITemplateApplicationService, TemplateApplicationService>();
builder.Services.AddScoped<IEmailService, ConsoleEmailService>();

// Add API services only
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Handlebars Email Template API",
        Version = "v1",
        Description = "API for managing and rendering Handlebars email templates with culture support"
    });
});

// Ensure database created and seed on startup

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Enable static file serving
app.UseRouting();

app.UseAuthorization();

// Map API routes only
app.MapControllers();

// Add default route to serve dashboard
app.MapGet("/", context =>
{
    context.Response.Redirect("/index.html");
    return Task.CompletedTask;
});

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Handlebars Email Template API v1");
        c.RoutePrefix = "swagger";
        c.DocumentTitle = "Handlebars Email Template API";
        c.HeadContent = @"
            <style>
                .topbar { display: none !important; }
                .swagger-ui .topbar { display: none !important; }
            </style>
            <script>
                window.addEventListener('DOMContentLoaded', function() {
                    setTimeout(function() {
                        var topbar = document.querySelector('.topbar');
                        if (topbar) {
                            var backLink = document.createElement('div');
                            backLink.innerHTML = '<a href=""/"" style=""position: fixed; top: 10px; left: 10px; z-index: 9999; background: #667eea; color: white; padding: 8px 16px; text-decoration: none; border-radius: 4px; font-size: 14px;"">← Back to Dashboard</a>';
                            document.body.appendChild(backLink);
                        }
                    }, 100);
                });
            </script>";
    });
}


// Seed database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<HandlebarsEmailHelper.Models.AppDbContext>();
    await HandlebarsEmailHelper.Models.SeedData.InitializeAsync(db);
}


app.Run();
