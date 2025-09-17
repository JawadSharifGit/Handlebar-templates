namespace HandlebarsEmailHelper.Dto
{
    public class EmailTemplateDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string? HtmlBody { get; set; }
    }
}
