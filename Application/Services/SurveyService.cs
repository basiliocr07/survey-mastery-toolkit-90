
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SurveyApp.Application.DTOs;
using SurveyApp.Application.Ports;
using SurveyApp.Domain.Entities;

namespace SurveyApp.Application.Services
{
    public class SurveyService : ISurveyService
    {
        private readonly ISurveyRepository _surveyRepository;
        private readonly ISurveyResponseRepository _surveyResponseRepository;
        private readonly IEmailService _emailService;

        public SurveyService(
            ISurveyRepository surveyRepository, 
            ISurveyResponseRepository surveyResponseRepository, 
            IEmailService emailService)
        {
            _surveyRepository = surveyRepository;
            _surveyResponseRepository = surveyResponseRepository;
            _emailService = emailService;
        }

        // Create methods
        public async Task<SurveyDto> CreateSurveyAsync(CreateSurveyDto surveyDto)
        {
            // Create a survey entity with the data from DTO
            var survey = new Survey
            {
                Id = Guid.NewGuid(),
                Title = surveyDto.Title,
                Description = surveyDto.Description,
                CreatedAt = DateTime.UtcNow,
                Status = "Active",
                Responses = 0
            };

            // Add questions to the survey if provided
            if (surveyDto.Questions != null)
            {
                foreach (var questionDto in surveyDto.Questions)
                {
                    var question = new Question
                    {
                        Id = Guid.NewGuid(),
                        Title = questionDto.Title,
                        Description = questionDto.Description,
                        Type = questionDto.Type,
                        Required = questionDto.Required,
                        Options = questionDto.Options
                    };

                    survey.AddQuestion(question);
                }
            }

            // Set delivery configuration if provided
            if (surveyDto.DeliveryConfig != null)
            {
                var deliveryConfig = new DeliveryConfig
                {
                    Type = surveyDto.DeliveryConfig.Type,
                    EmailAddresses = surveyDto.DeliveryConfig.EmailAddresses
                };

                if (surveyDto.DeliveryConfig.Schedule != null)
                {
                    deliveryConfig.Schedule = new Schedule
                    {
                        Frequency = surveyDto.DeliveryConfig.Schedule.Frequency,
                        DayOfMonth = surveyDto.DeliveryConfig.Schedule.DayOfMonth ?? 1,
                        DayOfWeek = surveyDto.DeliveryConfig.Schedule.DayOfWeek,
                        Time = surveyDto.DeliveryConfig.Schedule.Time,
                        StartDate = surveyDto.DeliveryConfig.Schedule.StartDate
                    };
                }

                if (surveyDto.DeliveryConfig.Trigger != null)
                {
                    deliveryConfig.Trigger = new Trigger
                    {
                        Type = surveyDto.DeliveryConfig.Trigger.Type,
                        DelayHours = surveyDto.DeliveryConfig.Trigger.DelayHours,
                        SendAutomatically = surveyDto.DeliveryConfig.Trigger.SendAutomatically
                    };
                }

                survey.SetDeliveryConfig(deliveryConfig);
            }

            // Save the survey to repository
            var createdSurvey = await _surveyRepository.CreateAsync(survey);

            // Map entity back to DTO and return
            return MapToSurveyDto(createdSurvey);
        }

        public async Task UpdateSurveyAsync(Guid id, UpdateSurveyDto surveyDto)
        {
            var survey = await _surveyRepository.GetByIdAsync(id);
            if (survey == null)
                throw new KeyNotFoundException($"Survey with ID {id} not found");

            // Update basic properties
            survey.Title = surveyDto.Title;
            survey.Description = surveyDto.Description;
            
            // Update delivery configuration if provided
            if (surveyDto.DeliveryConfig != null)
            {
                var deliveryConfig = new DeliveryConfig
                {
                    Type = surveyDto.DeliveryConfig.Type,
                    EmailAddresses = surveyDto.DeliveryConfig.EmailAddresses
                };

                if (surveyDto.DeliveryConfig.Schedule != null)
                {
                    deliveryConfig.Schedule = new Schedule
                    {
                        Frequency = surveyDto.DeliveryConfig.Schedule.Frequency,
                        DayOfMonth = surveyDto.DeliveryConfig.Schedule.DayOfMonth ?? 1,
                        DayOfWeek = surveyDto.DeliveryConfig.Schedule.DayOfWeek,
                        Time = surveyDto.DeliveryConfig.Schedule.Time,
                        StartDate = surveyDto.DeliveryConfig.Schedule.StartDate
                    };
                }

                if (surveyDto.DeliveryConfig.Trigger != null)
                {
                    deliveryConfig.Trigger = new Trigger
                    {
                        Type = surveyDto.DeliveryConfig.Trigger.Type,
                        DelayHours = surveyDto.DeliveryConfig.Trigger.DelayHours,
                        SendAutomatically = surveyDto.DeliveryConfig.Trigger.SendAutomatically
                    };
                }

                survey.SetDeliveryConfig(deliveryConfig);
            }

            await _surveyRepository.UpdateAsync(survey);
        }

        // Delete methods
        public async Task DeleteSurveyAsync(Guid id)
        {
            var survey = await _surveyRepository.GetByIdAsync(id);
            if (survey == null)
                throw new KeyNotFoundException($"Survey with ID {id} not found");
                
            await _surveyRepository.DeleteAsync(id);
        }

        // Update status methods
        public async Task UpdateSurveyStatusAsync(Guid id, string status)
        {
            var survey = await _surveyRepository.GetByIdAsync(id);
            if (survey == null)
                throw new KeyNotFoundException($"Survey with ID {id} not found");

            survey.Status = status;
            await _surveyRepository.UpdateAsync(survey);
        }

        // Query methods
        public async Task<SurveyDto> GetSurveyByIdAsync(Guid id)
        {
            var survey = await _surveyRepository.GetByIdAsync(id);
            if (survey == null)
                return null;

            return MapToSurveyDto(survey);
        }

        public async Task<List<SurveyDto>> GetAllSurveysAsync()
        {
            var surveys = await _surveyRepository.GetAllAsync();
            return surveys.Select(MapToSurveyDto).ToList();
        }

        public async Task<bool> SurveyExistsAsync(Guid id)
        {
            return await _surveyRepository.ExistsAsync(id);
        }

        public async Task<List<string>> GetAllCategoriesAsync()
        {
            return await _surveyRepository.GetAllCategoriesAsync();
        }

        public async Task<(List<SurveyDto> Surveys, int TotalCount)> GetPagedSurveysAsync(
            int pageNumber, 
            int pageSize, 
            string searchTerm = null, 
            string statusFilter = null, 
            string categoryFilter = null)
        {
            var (surveys, totalCount) = await _surveyRepository.GetPagedSurveysAsync(
                pageNumber, 
                pageSize, 
                searchTerm, 
                statusFilter, 
                categoryFilter);
                
            var surveyDtos = surveys.Select(MapToSurveyDto).ToList();
            
            return (surveyDtos, totalCount);
        }

        // Email methods
        public async Task SendSurveyEmailAsync(string email, string surveyTitle, string surveyLink)
        {
            Console.WriteLine($"Enviando encuesta '{surveyTitle}' a {email} con link {surveyLink}");
            await _emailService.SendSurveyInvitationAsync(email, surveyTitle, surveyLink);
        }

        public async Task SendSurveyEmailsAsync(Guid id)
        {
            var survey = await _surveyRepository.GetByIdAsync(id);
            if (survey == null)
                throw new KeyNotFoundException($"Encuesta con ID {id} no encontrada.");
                
            if (survey.DeliveryConfig == null || survey.DeliveryConfig.EmailAddresses == null || !survey.DeliveryConfig.EmailAddresses.Any())
                throw new InvalidOperationException("La encuesta no tiene configuraciones de email válidas.");
                
            string surveyBaseLink = "https://localhost:7200/survey/";
            
            foreach (var email in survey.DeliveryConfig.EmailAddresses)
            {
                Console.WriteLine($"Enviando encuesta '{survey.Title}' a {email}...");
                await _emailService.SendSurveyInvitationAsync(
                    email,
                    survey.Title,
                    $"{surveyBaseLink}{survey.Id}"
                );
            }
        }

        public async Task ProcessAutomaticSurveyDeliveryAsync()
        {
            var surveys = await _surveyRepository.GetAllAsync();
            
            foreach (var survey in surveys)
            {
                if (survey.DeliveryConfig?.Trigger?.SendAutomatically == true)
                {
                    try
                    {
                        string surveyBaseLink = "https://yourdomain.com/survey/";
                        
                        if (survey.DeliveryConfig.Type == "scheduled")
                        {
                            if (ShouldSendBasedOnSchedule(survey.DeliveryConfig.Schedule))
                            {
                                foreach (var email in survey.DeliveryConfig.EmailAddresses)
                                {
                                    await _emailService.SendSurveyInvitationAsync(
                                        email,
                                        survey.Title,
                                        $"{surveyBaseLink}{survey.Id}"
                                    );
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error procesando encuesta {survey.Id}: {ex.Message}");
                    }
                }
            }
        }

        private bool ShouldSendBasedOnSchedule(Schedule schedule)
        {
            if (schedule == null)
                return false;

            var now = DateTime.UtcNow;
            
            if (schedule.StartDate.HasValue && now < schedule.StartDate.Value)
                return false;
                
            if (schedule.EndDate.HasValue && now > schedule.EndDate.Value)
                return false;
                
            if (schedule.Frequency == "monthly" && now.Day == schedule.DayOfMonth)
            {
                return IsTimeMatch(now, schedule.Time);
            }
            
            if (schedule.Frequency == "weekly" && schedule.DayOfWeek.HasValue && 
                (int)now.DayOfWeek == schedule.DayOfWeek.Value)
            {
                return IsTimeMatch(now, schedule.Time);
            }
            
            if (schedule.Frequency == "daily")
            {
                return IsTimeMatch(now, schedule.Time);
            }
            
            return false;
        }
        
        private bool IsTimeMatch(DateTime now, string scheduledTime)
        {
            if (string.IsNullOrEmpty(scheduledTime))
                return false;
                
            if (TimeSpan.TryParse(scheduledTime, out TimeSpan scheduledTimeSpan))
            {
                var currentTime = new TimeSpan(now.Hour, now.Minute, 0);
                var diff = (currentTime - scheduledTimeSpan).Duration();
                return diff.TotalMinutes <= 5;
            }
            
            return false;
        }

        // Client access methods
        public async Task<SurveyDto> GetSurveyForClientAsync(Guid id)
        {
            var survey = await _surveyRepository.GetByIdAsync(id);
            if (survey == null || survey.Status != "Active")
                throw new KeyNotFoundException($"Encuesta con ID {id} no encontrada o no está disponible.");
                
            return MapToSurveyDto(survey);
        }

        // Response methods
        public async Task<SurveyResponseDto> SubmitSurveyResponseAsync(CreateSurveyResponseDto createResponseDto)
        {
            var survey = await _surveyRepository.GetByIdAsync(createResponseDto.SurveyId);
            if (survey == null)
                throw new KeyNotFoundException($"Encuesta con ID {createResponseDto.SurveyId} no encontrada.");

            var surveyResponse = new SurveyResponse(
                createResponseDto.SurveyId, 
                createResponseDto.RespondentName, 
                createResponseDto.RespondentEmail,
                null,  // RespondentPhone no incluido en CreateSurveyResponseDto
                null   // RespondentCompany no incluido en CreateSurveyResponseDto
            );
            
            // No se incluye información del cliente porque no está en el DTO

            foreach (var answerEntry in createResponseDto.Answers)
            {
                var questionId = Guid.Parse(answerEntry.Key);
                var question = survey.Questions.FirstOrDefault(q => q.Id == questionId);
                
                if (question == null)
                    continue;

                if (question.Type == "multiple-choice" && answerEntry.Value is List<string> multipleAnswers)
                {
                    var questionResponse = new QuestionResponse(questionId, question.Title, question.Type, multipleAnswers);
                    surveyResponse.AddAnswer(questionResponse);
                }
                else
                {
                    var answerValue = answerEntry.Value?.ToString() ?? string.Empty;
                    var questionResponse = new QuestionResponse(questionId, question.Title, question.Type, answerValue);
                    surveyResponse.AddAnswer(questionResponse);
                }
            }

            var createdResponse = await _surveyResponseRepository.CreateAsync(surveyResponse);

            survey.IncrementResponses();
            await _surveyRepository.UpdateAsync(survey);

            return MapToSurveyResponseDto(createdResponse);
        }

        public async Task<List<SurveyResponseDto>> GetSurveyResponsesAsync(Guid surveyId)
        {
            var survey = await _surveyRepository.GetByIdAsync(surveyId);
            if (survey == null)
                throw new KeyNotFoundException($"Encuesta con ID {surveyId} no encontrada.");

            var responses = await _surveyResponseRepository.GetBySurveyIdAsync(surveyId);

            return responses.Select(MapToSurveyResponseDto).ToList();
        }

        public async Task<List<RecentResponseDto>> GetRecentResponsesAsync(int count)
        {
            var recentResponses = await _surveyResponseRepository.GetRecentResponsesAsync(count);
            var result = new List<RecentResponseDto>();
            
            foreach (var response in recentResponses)
            {
                var survey = await _surveyRepository.GetByIdAsync(response.SurveyId);
                result.Add(new RecentResponseDto
                {
                    Id = response.Id,
                    SurveyId = response.SurveyId,
                    SurveyTitle = survey?.Title ?? "Encuesta sin título",
                    RespondentName = response.RespondentName,
                    SubmittedAt = response.SubmittedAt
                });
            }
            
            return result;
        }

        public async Task<SurveyResponseAnalyticsDto> GetResponseByIdAsync(Guid id)
        {
            // Este método está declarado en la interfaz pero no implementado
            // Implementamos una versión básica
            var response = await _surveyResponseRepository.GetByIdAsync(id);
            if (response == null)
                throw new KeyNotFoundException($"Respuesta con ID {id} no encontrada.");
                
            var survey = await _surveyRepository.GetByIdAsync(response.SurveyId);
            if (survey == null)
                throw new KeyNotFoundException($"Encuesta asociada no encontrada.");
                
            // Este método debería implementarse según los requisitos específicos
            // Por ahora devolvemos null ya que no conocemos la estructura completa
            return null;
        }

        // Mapping methods
        private SurveyResponseDto MapToSurveyResponseDto(SurveyResponse response)
        {
            return new SurveyResponseDto
            {
                Id = response.Id,
                SurveyId = response.SurveyId,
                SurveyTitle = null, // No disponible en el modelo de respuesta
                RespondentName = response.RespondentName,
                RespondentEmail = response.RespondentEmail,
                SubmittedAt = response.SubmittedAt,
                Answers = response.Answers.Select(a => new QuestionResponseDto
                {
                    QuestionId = a.QuestionId,
                    QuestionTitle = a.QuestionTitle,
                    QuestionType = a.QuestionType,
                    Answer = a.Answer,
                    MultipleAnswers = a.MultipleAnswers
                }).ToList()
            };
        }

        private SurveyDto MapToSurveyDto(Survey survey)
        {
            return new SurveyDto
            {
                Id = survey.Id,
                Title = survey.Title,
                Description = survey.Description,
                CreatedAt = survey.CreatedAt,
                Responses = survey.Responses,
                CompletionRate = survey.CompletionRate,
                Questions = survey.Questions?.Select(q => new QuestionDto
                {
                    Id = q.Id,
                    Title = q.Title,
                    Description = q.Description,
                    Type = q.Type,
                    Required = q.Required,
                    Options = q.Options,
                    Settings = q.Settings != null ? new QuestionSettingsDto
                    {
                        MinValue = q.Settings.MinValue,
                        MaxValue = q.Settings.MaxValue,
                        LowLabel = q.Settings.LowLabel,
                        MiddleLabel = q.Settings.MiddleLabel,
                        HighLabel = q.Settings.HighLabel
                    } : null
                }).ToList(),
                DeliveryConfig = survey.DeliveryConfig != null ? new DeliveryConfigDto
                {
                    Type = survey.DeliveryConfig.Type,
                    EmailAddresses = survey.DeliveryConfig.EmailAddresses,
                    Schedule = survey.DeliveryConfig.Schedule != null ? new ScheduleDto
                    {
                        Frequency = survey.DeliveryConfig.Schedule.Frequency,
                        DayOfMonth = survey.DeliveryConfig.Schedule.DayOfMonth,
                        DayOfWeek = survey.DeliveryConfig.Schedule.DayOfWeek,
                        Time = survey.DeliveryConfig.Schedule.Time,
                        StartDate = survey.DeliveryConfig.Schedule.StartDate
                    } : null,
                    Trigger = survey.DeliveryConfig.Trigger != null ? new TriggerDto
                    {
                        Type = survey.DeliveryConfig.Trigger.Type,
                        DelayHours = survey.DeliveryConfig.Trigger.DelayHours,
                        SendAutomatically = survey.DeliveryConfig.Trigger.SendAutomatically
                    } : null
                } : null
            };
        }
    }
}
