using SemanticKernel.Community.Web.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddSemanticKernel(kernelBuilder =>
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

var kernel = app.Services.GetRequiredService<Kernel>();
kernel.Data["Summaries"] = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};
app.MapKernel(kernel);
await app.RunAsync();
