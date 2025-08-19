using AvaloniaApplication3.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace AvaloniaApplication3.Utility
{
    public static class QuizJsonImporter
    {
        private static readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

        public static async Task<int> ImportFromFileAsync(AppDbContext ctx, string path, bool replaceIfTitleExists = false)
        {
            var json = await File.ReadAllTextAsync(path);
            return await ImportFromJsonAsync(ctx, json, replaceIfTitleExists);
        }

        public static async Task<int> ImportFromJsonAsync(AppDbContext ctx, string json, bool replaceIfTitleExists = false)
        {
            var dto = JsonSerializer.Deserialize<QuizDto>(json, _json)
                      ?? throw new InvalidOperationException("Invalid quiz JSON.");

            if (string.IsNullOrWhiteSpace(dto.Title))
                throw new InvalidOperationException("Quiz 'title' is required.");

            if (replaceIfTitleExists)
            {
                var existing = await ctx.Quizzes
                    .Include(q => q.Questions)
                    .ThenInclude(q => q.Answers)
                    .FirstOrDefaultAsync(q => q.Title == dto.Title);

                if (existing != null)
                {
                    ctx.Answers.RemoveRange(existing.Questions.SelectMany(x => x.Answers));
                    ctx.Questions.RemoveRange(existing.Questions);
                    ctx.Quizzes.Remove(existing);
                    await ctx.SaveChangesAsync();
                }
            }

            var quiz = new Quiz
            {
                Title = dto.Title.Trim(),
                Description = dto.Description?.Trim(),
                Questions = MapQuestions(dto.Questions)
            };

            ctx.Quizzes.Add(quiz);
            await ctx.SaveChangesAsync();
            return quiz.Id;
        }

        private static List<Question> MapQuestions(List<QuestionDto>? dtos)
        {
            var list = new List<Question>();
            if (dtos == null) return list;

            foreach (var q in dtos)
            {
                var type = Enum.TryParse<QuestionType>(q.Type, true, out var t) ? t : QuestionType.SingleChoice;

                list.Add(new Question
                {
                    Text = q.Text?.Trim() ?? string.Empty,
                    Explanation = q.Explanation?.Trim(),
                    Type = type,
                    Answers = MapAnswers(q.Answers)
                });
            }
            return list;
        }

        private static List<Answer> MapAnswers(List<AnswerDto>? dtos)
        {
            var list = new List<Answer>();
            if (dtos == null) return list;

            foreach (var a in dtos)
            {
                list.Add(new Answer
                {
                    Text = a.Text?.Trim() ?? string.Empty,
                    IsCorrect = a.IsCorrect
                });
            }
            return list;
        }

        // DTOs expected in JSON
        private sealed class QuizDto
        {
            public string Title { get; set; } = string.Empty;
            public string? Description { get; set; }
            public List<QuestionDto>? Questions { get; set; }
        }

        private sealed class QuestionDto
        {
            public string Text { get; set; } = string.Empty;
            public string Type { get; set; } = "SingleChoice"; // SingleChoice | MultipleChoice | TrueFalse
            public string? Explanation { get; set; }
            public List<AnswerDto>? Answers { get; set; }
        }

        private sealed class AnswerDto
        {
            public string Text { get; set; } = string.Empty;
            public bool IsCorrect { get; set; }
        }
    }
}
