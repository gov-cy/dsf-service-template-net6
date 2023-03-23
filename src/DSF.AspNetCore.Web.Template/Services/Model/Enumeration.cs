
/// FROM => Docs  .NET  .NET application architecture guide .NET microservices - Architecture e-book 
/// Use enumeration classes instead of enum types
/// https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/enumeration-classes-over-enum-types#implement-an-enumeration-base-class

namespace DSF.AspNetCore.Web.Template.Services.Model;

using System.Reflection;

public abstract class Enumeration : IComparable
{
    public string Name { get; private set; } = string.Empty;

    public int Id { get; private set; }

    protected Enumeration(int id, string name) => (Id, Name) = (id, name);

    public override string ToString() => Name;

    public static IEnumerable<T> GetAll<T>() where T : Enumeration =>
        typeof(T).GetFields(BindingFlags.Public |
                            BindingFlags.Static |
                            BindingFlags.DeclaredOnly)
                 .Select(f => f.GetValue(null))
                 .Cast<T>();
    public static T FromValue<T>(int value) where T : Enumeration
    {
        var matchingItem = Parse<T, int>(value, "value", item => item.Id == value);
        return matchingItem;
    }

    public static T FromName<T>(string name) where T : Enumeration
    {
        var matchingItem = Parse<T, string>(name, nameof(Name), item => item.Name == name);
        return matchingItem;
    }
    private static T Parse<T, K>(K value, string description, Func<T, bool> predicate) where T : Enumeration
    {
        var matchingItem = GetAll<T>().FirstOrDefault(predicate);

        if (matchingItem == null)
            throw new InvalidOperationException($"'{value}' is not a valid {description} in {typeof(T)}");

        return matchingItem;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Enumeration otherValue)
        {
            return false;
        }

        var typeMatches = GetType().Equals(obj.GetType());
        var valueMatches = Id.Equals(otherValue.Id);

        return typeMatches && valueMatches;
    }

    public int CompareTo(object? other)
    {
        if (other != null)
            return Id.CompareTo(((Enumeration)other).Id);
        else if (this != null)
            return 1; //this is greater than the other null object
        else
            return 0; //both are null, thus equal
    }

    public static implicit operator string(Enumeration s) => s.Name;

    public static implicit operator int(Enumeration s) => s.Id;

    public override int GetHashCode()
    {
        throw new NotImplementedException();
    }
}
