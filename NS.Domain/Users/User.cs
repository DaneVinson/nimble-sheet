namespace NS.Domain;

/// <summary>Represents an application user account.</summary>
public sealed class User
{
    /// <summary>Private parameterless constructor reserved for deserializers.</summary>
    private User()
    {
        Email = null!;
        Name = null!;
    }

    /// <summary>Initializes a new user.</summary>
    /// <param name="created">The UTC offset date and time when the user was created.</param>
    /// <param name="email">The user's email address.</param>
    /// <param name="id">The unique identifier.</param>
    /// <param name="name">The user's display name.</param>
    public User(DateTimeOffset created, string email, Guid id, string name)
    {
        Created = created;
        Email = email;
        Id = id;
        Name = name;
    }

    /// <summary>Gets the UTC offset date and time when the user was created.</summary>
    public DateTimeOffset Created { get; private set; }

    /// <summary>Gets the user's email address.</summary>
    public string Email { get; private set; }

    /// <summary>Gets the unique identifier.</summary>
    public Guid Id { get; private set; }

    /// <summary>Gets the user's display name.</summary>
    public string Name { get; private set; }

    /// <summary>Updates the user's email address.</summary>
    /// <param name="email">The new email address.</param>
    public void UpdateEmail(string email) => Email = email;
}
