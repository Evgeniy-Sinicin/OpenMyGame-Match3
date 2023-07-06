using UnityEngine;

/// <summary>
/// Обработчик анимации элемента
/// </summary>
public class ElementAnimation : MonoBehaviour
{
    /// <summary>
    /// Была ли програна анимация уничтожения элемента?
    /// </summary>
    public bool WasDestructionAnimationPlayed { get; set; }

    private int _layerNumber;
    private int _currentAnimationNumber;
    private string _key;
    private Animator _animator;

    private void Awake()
    {
        _layerNumber = -1;
        _animator = GetComponent<Animator>();
        _key = "AnimationNumber";
        ResetAnimation();
    }

    /// <summary>
    /// Запускаем анимацию бездействия с рандомного кадра
    /// </summary>
    public void ResetAnimation()
    {
        _currentAnimationNumber = 0;
        _animator.SetInteger(_key, _currentAnimationNumber);
        _animator.Play("idle", _layerNumber, Random.Range(0, 100) / 100f);

        WasDestructionAnimationPlayed = false;
    }

    /// <summary>
    /// Проигрываем анимацию уничтожения
    /// </summary>
    public void PlayDestruction()
    {
        _currentAnimationNumber = 1;
        _animator.SetInteger(_key, _currentAnimationNumber);
        _animator.Play("destroy", _layerNumber, 0);
    }

    /// <summary>
    /// Тригер выполняет этот метод, когда анимация уничтожения проиграет до конца
    /// </summary>
    public void OnAnimationDestroyed()
    {
        _currentAnimationNumber = 2;
        _animator.SetInteger(_key, _currentAnimationNumber);

        WasDestructionAnimationPlayed = true;
    }
}
