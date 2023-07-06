using Assets.Scripts;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    [Header("Level Variables")]
    /// <summary>
    /// Ширина уровня
    /// </summary>
    public int width;
    /// <summary>
    /// Высота уровня
    /// </summary>
    public int height;

    /// <summary>
    /// Каркас для элементов
    /// </summary>
    public GameObject[] elementPrefabs;
    /// <summary>
    /// Содержимое узла, которое инициализируется на уровне в момент проектирования уровня
    /// </summary>
    public StartElement[] customElements;

    [HideInInspector]
    /// <summary>
    /// Массив узлов игрового поля
    /// </summary>
    public Node[,] gameLevelNodes;

    [HideInInspector]
    /// <summary>
    /// Нормализован ли уровень?
    /// </summary>
    public bool IsNormalized { get; set; }
    public bool IsGameOver { get; set; }
    public bool AreDropedElements { get; set; }

    private List<Element> _matches;

    private void Awake()
    {
        gameLevelNodes = new Node[height, width];
        InitializeLevel();
        InitializeNodeElements();
        IsNormalized = false;
        IsGameOver = false;
    }

    private void Update()
    {
        Normalize();    // нормализуем уровень
        DropElements(); // роняем элементы
    }

    private void DropElements()
    {
        if (!AreDropedElements)
        {
            return;
        }

        var wasSwap = false;

        for (int x = 0; x < width; x++)
        {
            var emptyCount = 0;

            for (int y = 0; y < height; y++)
            {
                var currnetNode = gameLevelNodes[y, x];

                if (currnetNode.NodeElement.ElementType == Element.Type.Empty)
                {
                    emptyCount++;
                }
                else if (emptyCount > 0)
                {
                    var emptyNode = gameLevelNodes[y - emptyCount, x];

                    SwipeHandler.SwapElements(currnetNode, emptyNode);
                    wasSwap = true;
                }
            }
        }

        if (!wasSwap)
        {
            AreDropedElements = false;
        }
    }

    /// <summary>
    /// Вычисляем количество живых элементов каждый раз после нормализации уровня
    /// </summary>
    /// <returns></returns>
    private int CalculateLiveElementsCount()
    {
        var sum = 0;

        foreach (var node in gameLevelNodes)
        {
            if (node.NodeElement.Instance != null &&
                node.NodeElement.Instance.activeSelf)
            {
                sum++;
            }
        }

        return sum;
    }

    private void Normalize()
    {
        if (IsNormalized ||
            AreDropedElements)
        {
            return;
        }

        // Если список совпадений ещё не был инициализирован
        // Ищем совпадения и воспроизводим анимацию уничтожения
        if (_matches == null)
        {
            _matches = FindMatches();

            // Воспроизводим анимацию уничтожения
            for (int i = 0; i < _matches.Count; i++)
            {
                var animator = _matches[i].Instance.GetComponent<ElementAnimation>();

                if (!animator.WasDestructionAnimationPlayed)
                {
                    animator.PlayDestruction();
                }
            }
        }

        // Если совпадения уже найдены, значит и анимация была воспроизведена
        // Уничтожаем совпадения, чья онимация уже закончилась
        if (_matches != null &&
            _matches.Count > 0)
        {
            var destroyed = new List<Element>();

            for (int i = 0; i < _matches.Count; i++)
            {
                var animator = _matches[i].Instance.GetComponent<ElementAnimation>();

                // Ищем совпадения, в которых закончилась анимация уничтожения
                if (animator.WasDestructionAnimationPlayed &&
                    !destroyed.Contains(_matches[i]))
                {
                    destroyed.Add(_matches[i]);
                }
            }

            if (destroyed.Count == _matches.Count)
            {
                for (int i = 0; i < destroyed.Count; i++)
                {
                    _matches.Remove(destroyed[i]);
                    ClearElement(destroyed[i]);
                }
            }
        }

        // Если список совпадений был инициализирован и его длина стала 0
        // Значит уровень нормализовался
        if (_matches != null &&
            _matches.Count == 0)
        {
            _matches = null;
            IsNormalized = true;
            AreDropedElements = true;

            if (CalculateLiveElementsCount() <= 0) // Проверяем, не закончилась ли игра
            {
                IsGameOver = true;
            }
        }
    }

    private List<Element> FindMatches()
    {
        var matches = new List<Element>();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var element = gameLevelNodes[y, x].NodeElement;

                if (element.ElementType == Element.Type.Empty)
                {
                    continue;
                }

                // получаем соседей
                var leftNighbour = (x - 1 >= 0) ? gameLevelNodes[y, x - 1].NodeElement : null;
                var rightNighbour = (x + 1 < width) ? gameLevelNodes[y, x + 1].NodeElement : null;
                var upNighbour = (y + 1 < height) ? gameLevelNodes[y + 1, x].NodeElement : null;
                var downNighbour = (y - 1 >= 0) ? gameLevelNodes[y - 1, x].NodeElement : null;

                // проверка по горизонтали
                if ((leftNighbour != null && rightNighbour != null) &&
                    element.ElementType == leftNighbour.ElementType &&
                    element.ElementType == rightNighbour.ElementType)
                {
                    element.IsMatched = leftNighbour.IsMatched = rightNighbour.IsMatched = true;
                }

                // проверка по вертикали
                if ((upNighbour != null && downNighbour != null) &&
                    element.ElementType == upNighbour.ElementType &&
                    element.ElementType == downNighbour.ElementType)
                {
                    element.IsMatched = upNighbour.IsMatched = downNighbour.IsMatched = true;
                }
            }
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var element = gameLevelNodes[y, x].NodeElement;

                if (element.IsMatched)
                {
                    matches.Add(element);
                }
            }
        }

        return matches;
    }

    private void ClearElement(Element element)
    {
        element.ElementType = Element.Type.Empty;

        if (element.Instance != null)
        {
            element.Instance.SetActive(false);
        }

        element.IsMatched = false;
        element.IsMoving = false;
        element.IsReachedPosition = true;
    }

    public void InitializeLevel()
    {
        // Инициализируем все ячейки
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var wasInit = false;

                for (int i = 0; i < customElements.Length; i++)
                {
                    if (customElements[i].x == x &&
                        customElements[i].y == y)
                    {
                        gameLevelNodes[y, x] = new Node(customElements[i]);
                        wasInit = true;
                        break;
                    }
                }

                if (!wasInit)
                {
                    gameLevelNodes[y, x] = new Node(x, y);
                }
            }
        }
    }

    public bool AreMovingElements()
    {
        foreach (var node in gameLevelNodes)
        {
            if (node.NodeElement.IsMoving ||
                !node.NodeElement.IsReachedPosition)
            {
                return true;
            }
        }

        return false;
    }

    public void ResetAnimations()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (gameLevelNodes[y, x].NodeElement.Instance != null)
                {
                    gameLevelNodes[y, x].NodeElement.Instance.GetComponent<ElementAnimation>().ResetAnimation();
                }
            }
        }
    }

    public void ReturnElementsToStartingPosition()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var startNode = gameLevelNodes[y, x];

                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        var node = gameLevelNodes[i, j];

                        if (node.NodeElement.StartInfo.x == x &&
                            node.NodeElement.StartInfo.y == y)
                        {
                            SwipeHandler.SwapElements(node, startNode, true);
                        }
                    }
                }
            }
        }

        IsNormalized = false;
        IsGameOver = false;
    }

    public void InitializeNodeElements()
    {
        // Заполняем пустые ячейки
        for (int i = 0; i < customElements.Length; i++)
        {
            var x = customElements[i].x;
            var y = customElements[i].y;
            var node = gameLevelNodes[y, x];
            GameObject prefab;

            if (customElements[i].type >= elementPrefabs.Length)
            {
                continue;
            }

            prefab = elementPrefabs[customElements[i].type];
            node.NodeElement.Instance = Instantiate(prefab, new Vector2(x + Node.CENTER_OFFSET + transform.position.x,
                                                                        y + Node.CENTER_OFFSET + transform.position.y), Quaternion.identity);

            node.NodeElement.Instance.name = node.ToString();
            node.NodeElement.Instance.transform.SetParent(transform);
        }
    }
}