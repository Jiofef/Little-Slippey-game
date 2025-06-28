public interface ITypeHolder { }
public interface ITypeHolder<TEnum>: ITypeHolder where TEnum : System.Enum
{
	TEnum Type { get; set; }
}