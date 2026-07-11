namespace PTMK_Test.Core.Implementation.VO
{
    public sealed record Name(string FirstName, string Surname, string MiddleName = "") 
    {
        public string FirstName { get; init; } =
            string.IsNullOrWhiteSpace(FirstName)
                ? throw new ArgumentException($"{nameof(Name)} FirstName cannot be empty", nameof(FirstName))
                : FirstName.Trim();

        public string Surname { get; init; } =
            string.IsNullOrWhiteSpace(Surname)
                ? throw new ArgumentException($"{nameof(Name)} Surname cannot be empty", nameof(Surname))
                : Surname.Trim();

        public string MiddleName { get; init; } = MiddleName?.Trim() ?? "";

        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(MiddleName))
            {
                return $"{Surname} {FirstName}";
            }
            else
            {
                return $"{Surname} {FirstName} {MiddleName}";
            }
        }

        public string ToShortString()
        {
            string initials = NameUtils.GetInitials(FirstName) + NameUtils.GetInitials(MiddleName);
            return $"{Surname} {initials}";
        }
    }

    public static class NameUtils
    {
        public static string GetInitials(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "";

            return name.Trim()[0].ToString().ToUpper() + ".";
        }
    }
}
