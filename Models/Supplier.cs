using FluentValidation;
using System.ComponentModel.DataAnnotations;

namespace DemoMinimalAPI.Models
{
    public class Supplier
    {
        public Guid Id { get; set; }
        [Required]
        public string? Name { get; set; }
        [Required]
        public string? Document { get; set; }
        [Required]
        public bool Active { get; set; }

        public static SupplierValidator GetValidator() => new();
    }

    public class SupplierValidator : AbstractValidator<Supplier>
    {
        const string msg = "property has to have a value";

        public SupplierValidator()
        {
            RuleFor(x => x.Name)
                .NotNull()
                .NotEmpty()
                .WithMessage($"Name {msg}");

            RuleFor(x => x.Document)
                .NotNull()
                .NotEmpty()
                .WithMessage($"Document {msg}");

            RuleFor(x => x.Active)
                .NotNull()
                .WithMessage($"Active {msg}");
        }
    }
}
