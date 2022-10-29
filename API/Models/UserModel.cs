namespace API.Models
{
    public class UserModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "empty";
        public string Email { get; set; } = "empty";
        public DateTimeOffset BirthDate { get; set; }

        public UserModel(Guid id,string name, string email, DateTimeOffset birthDate)
        {
            Id = id;
            Name = name;
            Email = email;
            BirthDate = birthDate;
        }
    }
}
