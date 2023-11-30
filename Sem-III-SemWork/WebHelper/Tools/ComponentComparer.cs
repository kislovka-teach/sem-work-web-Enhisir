using WebHelper.Components;

namespace WebHelper.Tools;

class ComponentComparer : IEqualityComparer<Component>
{
    public bool Equals(Component? x, Component? y)
        => x?.GetType() == y?.GetType();

    public int GetHashCode(Component obj)
        => obj.GetHashCode();
}