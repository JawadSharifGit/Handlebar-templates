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
app.UseRouting();

app.UseAuthorization();

// Map API routes only
app.MapControllers();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


// Seed database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<HandlebarsEmailHelper.Models.AppDbContext>();
    await HandlebarsEmailHelper.Models.SeedData.InitializeAsync(db);
}


app.Run();
