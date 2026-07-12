using System.Reflection;
using FluentValidation;

namespace Renty.Server.Application.Common.Validators;

public static class PasswordValidationExtensions
{
    private static readonly Lazy<HashSet<string>> CommonPasswords = new(LoadCommonPasswords);

    public static IRuleBuilderOptions<T, string> MustBeStrongPassword<T>(
        this IRuleBuilder<T, string> ruleBuilder) =>
        ruleBuilder
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .MaximumLength(128).WithMessage("Password must not exceed 128 characters.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.")
            .Must(password => !CommonPasswords.Value.Contains(password))
            .WithMessage("This password is too common. Please choose a less predictable password.");

    private static HashSet<string> LoadCommonPasswords()
    {
        var assembly = Assembly.GetExecutingAssembly();
        const string resourceName = "Renty.Server.Application.Common.Validators.Resources.CommonPasswords.txt";

        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded resource '{resourceName}' was not found.");
        using var reader = new StreamReader(stream);

        var passwords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        while (reader.ReadLine() is { } line)
        {
            if (!string.IsNullOrWhiteSpace(line))
                passwords.Add(line.Trim());
        }

        return passwords;
    }
}
