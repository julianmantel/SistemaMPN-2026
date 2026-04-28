using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace SistemaMPN.Shared.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class NotPastDateAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if(value is DateTime dateValue)
            {
                if (dateValue < DateTime.Today)
                {
                    return new ValidationResult("La fecha no puede ser una fecha pasada.");
                }
            }

            return ValidationResult.Success;
        }
    }
}
