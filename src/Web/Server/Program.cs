using Microsoft.SemanticKernel;
using SemanticKernel.Community.WebHosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.AddSemanticKernel(kernelBuilder =>
{
    kernelBuilder.AddOpenAIChatCompletion("gpt-3", "your-openai-api-key");
    kernelBuilder.Plugins
        .AddFromFunctions("rewriter", [
            KernelFunctionFactory.CreateFromPrompt("Rewrite the input to say: 'Hello, World!'"),
            KernelFunctionFactory.CreateFromMethod((string input) => $"Hello, {input}!", "NativeFunction"),
        ]);
});
var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};
var kernel = app.Services.GetRequiredService<Kernel>();
kernel.Data["Summaries"] = summaries;
app.MapKernel(kernel);
app.Run();

