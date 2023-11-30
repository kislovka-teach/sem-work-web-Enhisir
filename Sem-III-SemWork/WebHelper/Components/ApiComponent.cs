namespace WebHelper.Components;

public abstract class ApiComponent : Component
{
    protected ApiComponent(string domain) : base($"api/v1/{domain.Trim('/')}") {}
}