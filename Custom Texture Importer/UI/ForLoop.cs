namespace Custom_Texture_Importer.UI;

public sealed class ForLoop<T>
{
    public ForLoop(int count, int startIndex, Action<ForLoop<T>> blockContext)
        : this(new T[count], startIndex, blockContext)
    {
    }

    public ForLoop(T[] array, int startIndex, Action<ForLoop<T>> blockContext)
    {
        Array = array;
        StartIndex = startIndex;
        BlockContext = blockContext;
        Count = array.Length;
    }

    public T[] Array { get; }
    public int StartIndex { get; }
    public Action<ForLoop<T>> BlockContext { get; }
    public int Count { get; }
    public int Index { get; private set; }
    public bool Break { get; set; } = false;

    public void Run(Action action)
    {
        for (Index = StartIndex; Index < Count; Index++)
        {
            BlockContext.Invoke(this);
            action.Invoke();
            if (Break)
            {
                break;
            }
        }
    }
}
