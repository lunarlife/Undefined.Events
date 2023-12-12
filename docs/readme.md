EXAMPLE:
```csharp
private readonly Event _onSomeHappens = new();

public IEventAccess OnSomeHappens => _onSomeHappens.Access;
 
private void ExampleRaise()
{
    _onSomeHappens.Raise();
}
```