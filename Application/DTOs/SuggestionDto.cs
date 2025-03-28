
using System;

namespace SurveyApp.Application.DTOs
{
    public class SuggestionDto
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; }
        public string Category { get; set; }
        public string Priority { get; set; }
        public bool IsAnonymous { get; set; }
        public string Response { get; set; }
        public DateTime? ResponseDate { get; set; }
        public string[] SimilarSuggestions { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Source { get; set; }
        public int CompletionPercentage { get; set; }
        public DateTime? TargetDate { get; set; }
        public string AcceptanceCriteria { get; set; }
    }

    public class CreateSuggestionDto
    {
        public string Content { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string Category { get; set; }
        public bool IsAnonymous { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Source { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerCompany { get; set; }
        public Guid? CustomerId { get; set; }
    }

    public class UpdateSuggestionDto
    {
        public string Content { get; set; }
        public string CustomerName { get; set; }
        public string Status { get; set; }
        public string Category { get; set; }
        public string Priority { get; set; }
        public string Response { get; set; }
    }

    public class MonthlyReportDto
    {
        public int TotalSuggestions { get; set; }
        public int ImplementedSuggestions { get; set; }
        public List<CategoryCountDto> TopCategories { get; set; }
        public List<MonthlyDataDto> MonthlyData { get; set; }
        public List<SuggestionDto> Suggestions { get; set; }
    }

    public class CategoryCountDto
    {
        public string Category { get; set; }
        public int Count { get; set; }
    }

    public class MonthlyDataDto
    {
        public string Month { get; set; }
        public int Year { get; set; }
        public int TotalSuggestions { get; set; }
        public int ImplementedSuggestions { get; set; }
    }
}
