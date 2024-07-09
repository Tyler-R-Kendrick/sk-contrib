using Microsoft.ML.Tokenizers;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace SKHelpers.Constraints;

public static class ConstrainedDecoding
{
    public static IReadOnlyList<int> GetTokenIds(Tokenizer tokenizer, string term)
        => tokenizer.EncodeToIds(term);
    public static IReadOnlyList<Token> GetTokens(Tokenizer tokenizer, string term, out string? normalizedString)
        => tokenizer.Encode(term, out normalizedString);
    public static IEnumerable<IReadOnlyList<int>> GetTokenIdTree(Tokenizer tokenizer, params string[] terms)
        => terms.Select(x => GetTokenIds(tokenizer, x));

    private static Dictionary<int, int> GetBias(
        IReadOnlyList<int> tokenIds,
        Func<(int index, int count, int tokenId), int> biasFunc)
    {
        Dictionary<int, int> biases = [];
        for (var i = 0; i < tokenIds.Count; i++)
        {
            var id = tokenIds[i];
            var bias = biasFunc((i, tokenIds.Count, id));
            if(!biases.TryAdd(id, bias))
            {
                biases[id] = bias;
            }
        }
        return biases;
    }

    public static async Task<FunctionResult> GenTokens(
        string prompt,
        Kernel kernel,
        IDictionary<int, int> biases,
        int maxTokens = 1,
        params string[] stopSequences)
    {
        // Clone the kernel to avoid side effects from changing prompt.
        kernel = kernel.Clone();
        kernel.FunctionInvocationFilters.Clear();
        kernel.AutoFunctionInvocationFilters.Clear();
        kernel.PromptRenderFilters.Clear();
        OpenAIPromptExecutionSettings executionSettings = new()
        {
            MaxTokens = maxTokens,
            TokenSelectionBiases = biases,
            StopSequences = stopSequences,
        };
        return await kernel.InvokePromptAsync(prompt, new(executionSettings));
    }

    private static async Task<string> GenToken(
        int[] indexedIds,
        Tokenizer tokenizer,
        string prompt,
        Kernel kernel,
        Func<(int index, int count, int tokenId), int> biasFunc,
        string[] stopSequences)
    {
        var biases = GetBias(indexedIds, biasFunc);
        var selectableTokenIds = biases.Where(x => x.Value == 100);
        if(selectableTokenIds.Count() == 1)
        {
            var tokenId = selectableTokenIds.First().Key;
            return tokenizer.MapIdToToken(tokenId)
                ?? throw new Exception($"Token id '{tokenId}' not mapped to a token.");
        }
        else
        {
            var result = await GenTokens(prompt, kernel, biases, 1, stopSequences);
            return result.ToString();
        }
    }

    public static async Task<string> GenTokens(
        string prompt,
        Kernel kernel,
        Tokenizer tokenizer,
        Func<(int index, int count, int tokenId), int> biasFunc,
        Func<string, bool> stopFunc,
        string[] stopSequences,
        int maxTokens = 1000,
        params string[] terms)
    {
        var tokenIds = terms
            .Select(x => (value: x, ids: tokenizer.EncodeToIds(x)));
        var tokenIndex = 0;
        var aggregate = string.Empty;
        do
        {
            var indexedIds = tokenIds
                .Where(x => x.ids.Count > tokenIndex)
                .Select(x => x.ids[tokenIndex])
                .ToArray();
            var localPrompt = prompt + aggregate;
            aggregate += await GenToken(indexedIds, tokenizer, localPrompt, kernel, biasFunc, stopSequences);
            tokenIndex++;
        } while (tokenIndex < maxTokens && !stopFunc(aggregate));
        return aggregate;
    }
}
