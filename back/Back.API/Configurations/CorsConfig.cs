namespace Back.API.Configurations;

public static class CorsConfig
{
    public const string PolicyName = "Default";

    public static IServiceCollection AddCorsConfig(
        this IServiceCollection services,
        string? allowedOrigins,
        bool isDevelopment)
    {
        var origins = (allowedOrigins ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        services.AddCors(options =>
        {
            options.AddPolicy(PolicyName, policy =>
            {
                if (origins.Length > 0)
                {
                    // Origens explicitamente liberadas via CORS_ALLOWED_ORIGIN.
                    policy.WithOrigins(origins)
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                }
                else if (isDevelopment)
                {
                    // Sem origem configurada, libera tudo apenas em desenvolvimento.
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                }

                // Em producao, sem CORS_ALLOWED_ORIGIN definido, nenhuma origem
                // cross-origin e permitida (fail-safe: a politica fica vazia).
            });
        });

        return services;
    }
}
