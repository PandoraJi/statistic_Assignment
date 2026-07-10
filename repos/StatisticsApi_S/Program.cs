using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Microsoft.OpenApi;
using StatisticsApi_S.Service;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));
var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();
builder.Services.AddScoped<IStatisticService, StatisticService>();

builder.Services.AddEndpointsApiExplorer();

//builder.Services.AddSwaggerGen(options =>
//{
//options.SwaggerDoc("v1", new OpenApiInfo
//{
//    Title = "Parquet Statistics API",
//    Version = "v1",
//    Description = "API that accepts a parquet file and returns a parquet file containing Min, Max, P10, P50 and P90."
//});
//    options.OperationFilter<FileUploadOperationFilter>();
//});

builder.Services.AddSwaggerGen(c =>
{
    c.IncludeXmlComments(xmlPath);
});

// Display file upload control
var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Statistics API v1");
    options.RoutePrefix = string.Empty;   // Opens Swagger at http://localhost:5000
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


