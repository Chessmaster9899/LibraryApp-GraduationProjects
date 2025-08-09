using System.ComponentModel.DataAnnotations;

namespace LibraryApp.Attributes
{
    /// <summary>
    /// Custom validation attribute for elegant error messages
    /// </summary>
    public class ElegantRequiredAttribute : RequiredAttribute
    {
        public ElegantRequiredAttribute(string fieldName = "This field")
        {
            ErrorMessage = $"{fieldName} is required and cannot be empty.";
        }
    }

    /// <summary>
    /// Custom validation attribute for string length with elegant error messages
    /// </summary>
    public class ElegantStringLengthAttribute : StringLengthAttribute
    {
        public ElegantStringLengthAttribute(int maximumLength, string fieldName = "This field") 
            : base(maximumLength)
        {
            ErrorMessage = $"{fieldName} must be between {{2}} and {{1}} characters long.";
        }

        public ElegantStringLengthAttribute(int maximumLength, int minimumLength, string fieldName = "This field") 
            : base(maximumLength)
        {
            MinimumLength = minimumLength;
            ErrorMessage = minimumLength > 0 
                ? $"{fieldName} must be between {minimumLength} and {maximumLength} characters long."
                : $"{fieldName} cannot exceed {maximumLength} characters.";
        }
    }

    /// <summary>
    /// Custom validation attribute for email with elegant error messages
    /// </summary>
    public class ElegantEmailAttribute : ValidationAttribute
    {
        public ElegantEmailAttribute()
        {
            ErrorMessage = "Please enter a valid email address (e.g., example@domain.com).";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return ValidationResult.Success; // Let RequiredAttribute handle null/empty
            }

            var emailAttribute = new EmailAddressAttribute();
            if (!emailAttribute.IsValid(value))
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }

    /// <summary>
    /// Custom validation attribute for phone numbers
    /// </summary>
    public class ElegantPhoneAttribute : RegularExpressionAttribute
    {
        public ElegantPhoneAttribute() : base(@"^[\+]?[1-9][\d]{0,15}$")
        {
            ErrorMessage = "Please enter a valid phone number.";
        }
    }

    /// <summary>
    /// Custom validation attribute for university ID format
    /// </summary>
    public class UniversityIdAttribute : RegularExpressionAttribute
    {
        public UniversityIdAttribute() : base(@"^[A-Z]{2}\d{7}$")
        {
            ErrorMessage = "University ID must be in format: 2 letters followed by 7 digits (e.g., CS2025001).";
        }
    }

    /// <summary>
    /// Custom validation attribute for file upload validation
    /// </summary>
    public class AllowedFileExtensionsAttribute : ValidationAttribute
    {
        private readonly string[] _extensions;

        public AllowedFileExtensionsAttribute(params string[] extensions)
        {
            _extensions = extensions;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!_extensions.Contains(extension))
                {
                    return new ValidationResult($"Only files with the following extensions are allowed: {string.Join(", ", _extensions)}");
                }
            }

            return ValidationResult.Success;
        }
    }

    /// <summary>
    /// Custom validation attribute for file size validation
    /// </summary>
    public class MaxFileSizeAttribute : ValidationAttribute
    {
        private readonly int _maxFileSize;

        public MaxFileSizeAttribute(int maxFileSize)
        {
            _maxFileSize = maxFileSize;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                if (file.Length > _maxFileSize)
                {
                    var maxSizeMB = _maxFileSize / (1024 * 1024);
                    return new ValidationResult($"File size cannot exceed {maxSizeMB} MB.");
                }
            }

            return ValidationResult.Success;
        }
    }

    /// <summary>
    /// Custom validation attribute for date ranges
    /// </summary>
    public class DateRangeAttribute : ValidationAttribute
    {
        private readonly DateTime _minDate;
        private readonly DateTime _maxDate;

        public DateRangeAttribute(string minDate, string maxDate)
        {
            _minDate = DateTime.Parse(minDate);
            _maxDate = DateTime.Parse(maxDate);
        }

        public DateRangeAttribute(int yearsBack, int yearsForward)
        {
            _minDate = DateTime.Now.AddYears(-yearsBack);
            _maxDate = DateTime.Now.AddYears(yearsForward);
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is DateTime date)
            {
                if (date < _minDate || date > _maxDate)
                {
                    return new ValidationResult($"Date must be between {_minDate:MMM dd, yyyy} and {_maxDate:MMM dd, yyyy}.");
                }
            }

            return ValidationResult.Success;
        }
    }
}