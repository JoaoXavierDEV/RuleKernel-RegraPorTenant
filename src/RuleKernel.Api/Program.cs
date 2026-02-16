using Microsoft.EntityFrameworkCore;
using RuleKernel.Core.Models;
using RuleKernel.Api.Services;
using RuleKernel.Core.Data;
using RuleKernel.Core.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<RuleKernelDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ConsoleScriptRuleExecutor>();
builder.Services.AddScoped<IRuleRunner, RuleRunner>();
builder.Services.AddScoped<CalcularService>();
builder.Services.AddScoped<FaturaService>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();