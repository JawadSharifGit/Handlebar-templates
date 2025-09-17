namespace HandlebarsEmailHelper.Interfaces
{
    public interface ITemplateService
    {
        string TransformTokens(string input);
        bool ContainsHandlebarsTokens(string input);
        string CompileTemplate(string templateText, object model);
        string CompileTemplate(string templateText, object model, IDictionary<string, string>? partials);
    }
}


