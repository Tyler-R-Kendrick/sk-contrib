using Microsoft.Extensions.Configuration;
using Microsoft.ML.Tokenizers;
using Microsoft.Extensions.DependencyInjection;

namespace Constraints;

using static SKHelpers.Constraints.ConstrainedDecoding;

[TestFixture]
public class Tests
{
    const string model = "gpt-4";
    private Kernel kernel = default!;

    private record TestData(string[] Values)
    {
        private readonly Tokenizer tokenizer = Tokenizer.CreateTiktokenForModel(model);
        public Tokenizer Tokenizer => tokenizer;
        public string Prompt => $"Select a value from [{string.Join(", ", Values.Select(x => $"\"{x}\""))}]: ";
        public IDictionary<string, IReadOnlyList<int>> TokenIds => Values.ToDictionary(x => x, x => tokenizer.EncodeToIds(x));
        public TestData(string valueString) : this(SplitValues(valueString)) { }
        public override string ToString() => Prompt;
        
        private static string[] SplitValues(string valueString)
            => valueString.Split(", ").Select(x => x.Trim('\'').Trim()).ToArray();
    }

    [SetUp]
    public void Setup()
    {
        var config = new ConfigurationBuilder()
            .AddUserSecrets<Tests>()
            .Build();
        var apiKey = config["OPENAI_API_KEY"]
            ?? throw new Exception("OPENAI_API_KEY environment variable not set.");
        var builder = Kernel.CreateBuilder();
        builder.Services.ConfigureHttpClientDefaults(x => x.AddStandardResilienceHandler());
        kernel = builder
            .AddOpenAIChatCompletion(model, apiKey)
            .Build();
    }

    [TestCase("'One', 'Two', 'Three'")]
    [TestCase("'1', '2', '3'")]
    public async Task SingleTokenGeneration(string valueString)
    {
        var prompt = new TestData(valueString);
        var biases = prompt.TokenIds.ToDictionary(x => x.Value[0], x => 100);
        var result = await GenTokens(prompt.ToString(), kernel, biases);
        Assert.That(result.ToString(), Is.AnyOf(prompt.Values));
    }
    
    [TestCase("'One', 'Two', 'Three'", 1)]
    [TestCase("'1', '2', '3'", 1)]
    [TestCase("'One Potato', 'Two', '3 Potato', '4'", 3)]
    public async Task MultiTokenGeneration(string valueString, int maxTokens)
    {
        TestData prompt = new(valueString);
        async Task<string> GenToken(
            Func<(int index, int count, int tokenId), int> biasFactory,
            Func<string, bool> stopFunc)
        {
            var result = await GenTokens(
                prompt.ToString(), kernel, prompt.Tokenizer,
                biasFactory, stopFunc,
                [], maxTokens, prompt.Values);
            return result.ToString();
        }
        
        var inclusiveResult = await GenToken(_ => 100, term => prompt.Values.Contains(term));
        Assert.That(inclusiveResult, Is.AnyOf(prompt.Values));

        var exclusiveResult = await GenToken(_ => -100, term => prompt.Values.Contains(term));
        Assert.That(exclusiveResult, Is.Not.AnyOf(prompt.Values));
    }
}