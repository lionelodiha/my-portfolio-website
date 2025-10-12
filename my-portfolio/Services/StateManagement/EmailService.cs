using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using MyPortfolio.Contracts.ContactForm;
using MyPortfolio.Models.ContactForm;
using MyPortfolio.Models.DTOs;
using MyPortfolio.Services.Config;

namespace MyPortfolio.Services.StateManagement;

public class EmailService : IEmailService
{
	private readonly HttpClient _http;
	private readonly EmailJSConfig _emailJSConfig;
	private readonly JsonSerializerOptions _options = new() { PropertyNameCaseInsensitive = true };
	private const string EmailJSApiUrl = "https://api.emailjs.com/api/v1.0/email/send";

	public EmailService(HttpClient http, IOptions<EmailJSConfig> emailJSConfig)
	{
		_http = http;
		_emailJSConfig = emailJSConfig.Value;
	}

	public async Task<ApiResponse<object>> SendEmailAsync(ContactFormModel model)
	{
		try
		{
			// Check if EmailJS is configured
			if (string.IsNullOrEmpty(_emailJSConfig.ServiceId) ||
			    string.IsNullOrEmpty(_emailJSConfig.TemplateId) ||
			    string.IsNullOrEmpty(_emailJSConfig.PublicKey))
			{
				return ApiResponse<object>.Fail(null, "Email service is not configured. Please contact the site administrator.");
			}

			// Prepare EmailJS payload
			var emailJSPayload = new
			{
				service_id = _emailJSConfig.ServiceId,
				template_id = _emailJSConfig.TemplateId,
				user_id = _emailJSConfig.PublicKey,
				template_params = new
				{
					from_name = $"{model.FirstName} {model.LastName}",
					from_email = model.Email,
					service = model.Service,
					reason = model.Reason,
					message = model.Message,
					to_email = "odhialionel@gmail.com"
				}
			};

			HttpResponseMessage response = await _http.PostAsJsonAsync(EmailJSApiUrl, emailJSPayload);

			if (response.IsSuccessStatusCode)
			{
				return ApiResponse<object>.Ok(null, "Message sent successfully! I'll get back to you soon.");
			}

			string errorContent = await response.Content.ReadAsStringAsync();
			return ApiResponse<object>.Fail(null, $"Failed to send message. Please try again or contact me directly at odhialionel@gmail.com");
		}
		catch (Exception)
		{
			return ApiResponse<object>.Fail(null, $"An error occurred. Please contact me directly at odhialionel@gmail.com");
		}
	}
}
