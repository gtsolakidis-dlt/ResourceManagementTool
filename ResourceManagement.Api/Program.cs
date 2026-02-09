

using ResourceManagement.Application;
using ResourceManagement.Infrastructure;
using ResourceManagement.Api.Middleware;
using Serilog;
using Dapper;

SqlMapper.AddTypeMap(typeof(DateTime), System.Data.DbType.DateTime2);

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog with console and file logging
Serilog.Debugging.SelfLog.Enable(msg => 
{
    try { File.AppendAllText("serilog-selflog.txt", DateTime.Now.ToString("O") + " " + msg + Environment.NewLine); }
    catch { } // Best effort
});

var seqUrl = builder.Configuration["Serilog:SeqServerUrl"] ?? "http://127.0.0.1:5341";

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .Enrich.WithProcessId()
    .Enrich.WithThreadId()
    .WriteTo.Console()
    .WriteTo.Seq(seqUrl)
    .WriteTo.File(
        path: "logs/api-.log",
        rollingInterval: Serilog.RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
        retainedFileCountLimit: 30)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    c.AddSecurityDefinition("basic", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "basic",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Basic Authorization header using the Bearer scheme."
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "basic"
                }
            },
            new string[] {}
        }
    });
});

// Register Custom Layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddAuthentication("Basic")
    .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, ResourceManagement.Api.Security.BasicAuthenticationHandler>("Basic", null);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", policy =>
    {
        policy.SetIsOriginAllowed(origin => true) // allow any origin
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .WithExposedHeaders("Content-Disposition", "X-Correlation-ID");
    });
});

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

// Configure the HTTP request pipeline

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowReact");

app.UseSerilogRequestLogging();

// app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Add audit logging middleware after authentication (so we have user context)
app.UseMiddleware<AuditLoggingMiddleware>();

app.MapControllers();


app.Run();

