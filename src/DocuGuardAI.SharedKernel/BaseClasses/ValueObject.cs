namespace DocuGuardAI.SharedKernel.BaseClasses;

public abstract record ValueObject
{
    protected abstract IEnumerable<object> GetEqualityComponents();
}