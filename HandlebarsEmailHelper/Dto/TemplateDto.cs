namespace HandlebarsEmailHelper.Dto
{
    public class TemplateDto
    {
        public string HtmlContent { get; set; } = string.Empty;
        public object Data { get; set; } = new();
    }
}
