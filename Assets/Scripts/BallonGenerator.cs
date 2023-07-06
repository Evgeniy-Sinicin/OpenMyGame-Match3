using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;

/// <summary>
/// Генератор шариков
/// </summary>
public class BallonGenerator : MonoBehaviour
{
    /// <summary>
    /// Количество генерируемых объектов на уровне
    /// </summary>
    public int count;
    public float minSpeed;
    public float maxSpeed;
    /// <summary>
    /// Каркасы объектов
    /// </summary>
    public GameObject[] prefabs;

    /// <summary>
    /// Отступ от края экрана
    /// </summary>
    private float _borderOffset;
    /// <summary>
    /// Нижняя левая точка камеры
    /// </summary>
    private Vector2 _leftBottomCameraPoint;
    /// <summary>
    /// Праввая верхняя точка экрана
    /// </summary>
    private Vector2 _rightTopCameraPoint;
    private List<Ballon> _ballons;

    void Start()
    {
        _borderOffset = 1.0f;
        _leftBottomCameraPoint = GameObject.Find("LeftBottomCameraPoint").transform.position;
        _rightTopCameraPoint = GameObject.Find("RightTopCameraPoint").transform.position;
        _ballons = new List<Ballon>(count);

        for (int i = 0; i < count; i++)
        {
            var ballon = new Ballon();

            InitializeBallon(ballon, prefabs[Random.Range(0, prefabs.Length)]);

            // Запускаем анимацию полёта со случайного момента
            ballon.Instance.GetComponent<Animator>().Play("idle", -1, Random.Range(0, 100) / 100f);

            _ballons.Add(ballon);
        }
    }

    void Update()
    {
        Fly();
    }

    /// <summary>
    /// Инициализируем шар случайными значениями
    /// </summary>
    private void InitializeBallon(Ballon ballon, GameObject prefab)
    {
        //Задаём случайное направление движения
        var direction = Random.Range(-1, 1) >= 0 ? Vector2.right : Vector2.left;

        // Задаём случайную позицию на сцене
        var horizontPoint = (direction.Equals(Vector2.left)) ? _rightTopCameraPoint.x + _borderOffset : _leftBottomCameraPoint.x - _borderOffset;
        var verticalPoint = Random.Range(_leftBottomCameraPoint.y + _borderOffset * 3, _rightTopCameraPoint.y - _borderOffset);
        var position = new Vector2(horizontPoint, verticalPoint);

        if (ballon.Instance == null)
        {
            ballon.Instance = Instantiate(prefab, position, Quaternion.identity);
            ballon.Instance.transform.parent = GetComponent<Level>().transform;
        }
        else
        {
            ballon.Instance.transform.position = position;
        }

        ballon.Speed = Random.Range(minSpeed, maxSpeed);
        ballon.Direction = direction;
    }

    /// <summary>
    /// Перемещаем шары по сцене
    /// </summary>
    private void Fly()
    {
        for (int i = 0; i < _ballons.Count; i++)
        {
            var ballon = _ballons[i];
            var position = ballon.Instance.transform.position;

            // Если шар вылетел за стену, перенаправляем его
            if (position.x > _rightTopCameraPoint.x + _borderOffset ||
                position.x < _leftBottomCameraPoint.x - _borderOffset)
            {
                InitializeBallon(ballon, ballon.Instance);
                position = ballon.Instance.transform.position;
            }

            var speed = ballon.Speed * Time.deltaTime;
            ballon.Instance.transform.position = Vector2.Lerp(position, (Vector2)position + ballon.Direction, speed);
        }
    }
}
