using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    /// Содержимое узла для игрового поля уровня
    /// </summary>
    public class Element
    {
        public enum Type
        {
            Empty = -1,
            Water = 0,
            Fire = 1
        }

        /// <summary>
        /// Перемещается ли элемент в данный момоент?
        /// </summary>
        public bool IsMoving { get; set; }
        /// <summary>
        /// Достиг ли элемент позиции своего узла?
        /// </summary>
        public bool IsReachedPosition { get; set; } = true;
        /// <summary>
        /// Является ли элемент совпадением?
        /// </summary>
        public bool IsMatched { get; set; } = false;
        public Type ElementType { get; set; }
        /// <summary>
        /// Структура со стартовой информацией, требуется для перемещения элемента на стартовую позицию при смене уровня
        /// </summary>
        public StartElement StartInfo { get; }
        public GameObject Instance { get; set; }

        public Element(StartElement startElement)
        {
            StartInfo = startElement;
            ElementType = (Type)startElement.type;
        }

        public string ToString(int x, int y)
        {
            var name = string.Empty;

            switch (ElementType)
            {
                case Type.Water:
                    name = "Water";
                    break;
                case Type.Fire:
                    name = "Fire";
                    break;
                default:
                    name = "Empty";
                    break;
            }

            if (Instance == null)
            {
                return $"{name}: ({x}, {y})";
            }

            Instance.name = $"{name}: ({x}, {y})";

            return Instance.name;
        }
    }

    [System.Serializable]
    public struct StartElement
    {
        /// <summary>
        /// Координата ширины
        /// </summary>
        public int x;
        /// <summary>
        /// Координата высоты
        /// </summary>
        public int y;
        /// <summary>
        /// Тип узла: -1 = пусто; 0 = огонь; 1 = вода
        /// </summary>
        public int type;
    }
}
