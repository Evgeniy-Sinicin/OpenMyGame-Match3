using Assets.Scripts;

/// <summary>
/// Родительская ячейка элемента
/// </summary>
public class Node
{
    /// <summary>
    /// Неудачная пародия на константу для перемещения элемента в центр ячейки
    /// </summary>
    public static float CENTER_OFFSET { get; } = 0.5f;

    // Координаты позиции на уровне
    private int _x;
    private int _y;

    public int X { get => _x; }
    public int Y { get => _y; }

    public Element NodeElement { get; set; }

    public Node(StartElement startElement) 
    {
        _x = startElement.x;
        _y = startElement.y;
        NodeElement = new Element(startElement);
    }

    public Node(int x, int y)
    {
        _x = x;
        _y = y;
        NodeElement = new Element(new StartElement() { x = x, y = y, type = -1 });
    }

    public override string ToString()
    {
        return NodeElement.ToString(X, Y);
    }
}