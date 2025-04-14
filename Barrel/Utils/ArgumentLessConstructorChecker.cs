namespace Barrel.Utils;

public static class ArgumentLessConstructorChecker
{
    public static bool HasArgumentLessConstructor(Type type)
    {
        var constructor = type.GetConstructors().FirstOrDefault(constructor =>
        {
            if (!constructor.IsPublic) return false;

            var parameters = constructor.GetParameters();
            if (!parameters.All(p => p.HasDefaultValue)) return false;

            return true;
        });

        return constructor is not null;
    }
}