using Assets.Scripts;
using UnityEngine;

/// <summary>
/// Обработчик перемещения элементов между узлами игрового поля уровня
/// </summary>
public class SwipeHandler : MonoBehaviour
{
    private const float MIN_DISTANCE_RANGE = 0.25f;

    /// <summary>
    /// Колонка первого узла
    /// </summary>
    private int _startColumn;
    /// <summary>
    /// Строка первого узла
    /// </summary>
    private int _startRow;
    /// <summary>
    /// Колонка второго узла
    /// </summary>
    private int _finishColumn;
    /// <summary>
    /// Строка второго узла
    /// </summary>
    private int _finishRow;
    /// <summary>
    /// Угол от первого до второго клика
    /// </summary>
    private float _swipeAngle;
    /// <summary>
    /// Дистанция от первого до второго клика
    /// </summary>
    private float _distance;
    /// <summary>
    /// Игровой уровень, на котором размещены узлы, в которых находятся элементы
    /// </summary>
    private Level _gameLevel;
    /// <summary>
    /// Выбранный узел
    /// </summary>
    private Node _firstNode;
    /// <summary>
    /// Узел с которым будем меняться содержимым
    /// </summary>
    private Node _secondNode;
    /// <summary>
    /// Позиция первого клика
    /// </summary>
    private Vector2 _startSwipePosition;
    /// <summary>
    /// Позиция второго клика
    /// </summary>
    private Vector2 _finishSwipePosition;

    /// <summary>
    /// Сбрасываем значения элемента узла, до исходных
    /// </summary>
    /// <param name="node">Родительский узел</param>
    private static void ResetNodeElement(Node node)
    {
        node.NodeElement.ElementType = (Element.Type)node.NodeElement.StartInfo.type;
        node.ToString(); // Переименовываем элемент
        node.NodeElement.IsMoving = false;
        node.NodeElement.IsReachedPosition = true;
        if (node.NodeElement.Instance != null)
        {
            node.NodeElement.Instance.SetActive(true);
            node.NodeElement.Instance.transform.position = new Vector2(node.X + Node.CENTER_OFFSET, node.Y + Node.CENTER_OFFSET);
        }
    }

    /// <summary>
    /// Обмениваемся элементами ячеек
    /// </summary>
    /// <param name="toStartingNode">Если происходит перемещение к стартовому узлу</param>
    public static void SwapElements(Node firstNode, Node secondNode, bool toStartingNode = false)
    {
        var temp = secondNode.NodeElement;
        secondNode.NodeElement = firstNode.NodeElement;
        firstNode.NodeElement = temp;

        if (toStartingNode) // Если происходит перемещение к стартовому узлу
        {
            // Сбрасываем значения элементов текущих узлов
            ResetNodeElement(firstNode);
            ResetNodeElement(secondNode);

            return;
        }

        // Переименовываем
        firstNode.ToString();
        secondNode.ToString();

        // Если элементы не пусты, требуется анимированно переместить их к своим родительским узлам
        if (firstNode.NodeElement.ElementType != Element.Type.Empty)
        {
            firstNode.NodeElement.IsMoving = true;
            firstNode.NodeElement.IsReachedPosition = false;
        }

        secondNode.NodeElement.IsMoving = true;
        secondNode.NodeElement.IsReachedPosition = false;
    }

    /// <summary>
    /// Анимированно перемещаем визуальное содержимое элемента к родительской ячейке, если оно находится за её пределами
    /// </summary>
    /// <returns>True, если то перемещение до конца закончено успешно</returns>
    public static bool MoveElement(Node node)
    {
        if (node == null ||
            node.NodeElement == null ||
            !node.NodeElement.IsMoving ||
            node.NodeElement.IsReachedPosition)
        {
            return false;
        }

        var speed = Time.deltaTime * 6.5f;
        var element = node.NodeElement;
        var targetPos = new Vector2(node.X + Node.CENTER_OFFSET, node.Y + Node.CENTER_OFFSET);

        if (element.Instance != null)
        {
            if (Vector2.Distance(element.Instance.transform.position, targetPos) < speed) // Если мы близко к цели
            {
                // Задаём элементу точную позицию
                element.Instance.transform.position = targetPos;
                element.IsMoving = false;
                element.IsReachedPosition = true;

                return true;
            }
            else
            {
                // Анимированно подталкиваем элемент к цели
                element.Instance.transform.position = Vector2.Lerp(element.Instance.transform.position, targetPos, speed);
            }
        }

        return false;
    }

    void Start()
    {
        _gameLevel = GetComponent<Level>();
    }

    void Update()
    {
        for (int y = 0; y < _gameLevel.height; y++)
        {
            for (int x = 0; x < _gameLevel.width; x++)
            {
                var isReplacedNode = MoveElement(_gameLevel.gameLevelNodes[y, x]);

                if (isReplacedNode) // Если хотя бы один элемент достиг своего родительского узла и завершил перемещение
                {
                    _gameLevel.IsNormalized = false;
                    _gameLevel.AreDropedElements = true;
                }
            }
        }
    }

    /// <summary>
    /// Узнаём позицию первой ячейки для свайпа
    /// </summary>
    private void OnMouseDown()
    {
        if (_gameLevel.AreDropedElements ||
            !_gameLevel.IsNormalized)
        {
            return;
        }

        _startSwipePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _startColumn = (int)_startSwipePosition.x;
        _startRow = (int)_startSwipePosition.y;
    }

    /// <summary>
    /// Узнаём позицию второй ячейки для свайпа
    /// </summary>
    private void OnMouseUp()
    {
        if (_gameLevel.AreDropedElements ||
            !_gameLevel.IsNormalized)
        {
            return;
        }

        _finishSwipePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _finishColumn = (int)_finishSwipePosition.x;
        _finishRow = (int)_finishSwipePosition.y;

        // Вычисляем угол и расстояние между ячейками
        CalculateSwipeAngle();
        _distance = Vector2.Distance(_startSwipePosition, _finishSwipePosition);

        if (_distance < MIN_DISTANCE_RANGE) // Если мы недостаточно далеко свайпнули, то действий не производим
        {
            return;
        }

        CalculateNewPositionByAngle();

        if (IsOnLevelFieldPosition(_finishColumn, _finishRow) &&
            IsValidSwipeRange() &&
            !IsMovingElelements())
        {
            _firstNode = _gameLevel.gameLevelNodes[_startRow, _startColumn];
            _secondNode = _gameLevel.gameLevelNodes[_finishRow, _finishColumn];

            // Запрещаем свайпаться от пустого элемента или подбрасывать их
            if (_firstNode.NodeElement.ElementType == Element.Type.Empty ||
                (_secondNode.NodeElement.ElementType == Element.Type.Empty && _firstNode.Y == _secondNode.Y - 1))
            {
                return;
            }

            SwapElements(_firstNode, _secondNode);
        }
    }

    private void CalculateSwipeAngle()
    {
        _swipeAngle = Mathf.Atan2(_finishSwipePosition.y - _startSwipePosition.y, _finishSwipePosition.x - _startSwipePosition.x) * 180 / Mathf.PI;
    }

    private void CalculateNewPositionByAngle()
    {
        if (_swipeAngle > 45 && _swipeAngle <= 135) // вверх
        {
            _finishColumn = _startColumn;
            _finishRow = _startRow + 1;
        }
        else if (_swipeAngle > -135 && _swipeAngle <= -45) // вниз
        {
            _finishColumn = _startColumn;
            _finishRow = _startRow - 1;
        }
        else if (_swipeAngle >= -45 && _swipeAngle < 45) // вправо
        {
            _finishColumn = _startColumn + 1;
            _finishRow = _startRow;
        }
        else if ((_swipeAngle >= 135 && _swipeAngle < 180) ||
                 _swipeAngle > -180 && _swipeAngle < -135) // влево
        {
            _finishColumn = _startColumn - 1;
            _finishRow = _startRow;
        }
    }

    private bool IsValidSwipeRange()
    {
        return (Vector2.Distance(new Vector2(_startColumn, _startRow), new Vector2(_finishColumn, _finishRow)) <= 1) ? true : false;
    }

    private bool IsOnLevelFieldPosition(int x, int y)
    {
        if (x >= _gameLevel.width ||
            x < 0 ||
            y >= _gameLevel.height ||
            y < 0) // если мы свайпнули за предел поля, не перемещаем содержимое ячейки
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Проверяем, есть ли движущиеся элементы на уровне
    /// </summary>
    /// <returns></returns>
    private bool IsMovingElelements()
    {
        for (int y = 0; y < _gameLevel.height; y++)
        {
            for (int x = 0; x < _gameLevel.width; x++)
            {
                if (_gameLevel.gameLevelNodes[y, x].NodeElement.IsMoving)
                {
                    return true;
                }
            }
        }

        return false;
    }
}