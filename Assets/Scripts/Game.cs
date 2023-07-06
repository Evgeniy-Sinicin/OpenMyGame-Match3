using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    public int currentLevelIndex;
    public List<Level> levelPrefabs;

    private Level _currentLevel;
    private List<Level> _levels;

    void Start()
    {
        _levels = new List<Level>(levelPrefabs.Count);

        for (int i = 0; i < levelPrefabs.Count; i++)
        {
            var level = Instantiate(levelPrefabs[i],
                                    levelPrefabs[i].transform.position,
                                    Quaternion.identity);
            level.transform.parent = transform;

            if (i == currentLevelIndex)
            {
                _currentLevel = level;
            }
            else
            {
                level.gameObject.SetActive(false);
            }

            _levels.Add(level);
        }
    }

    private void Update()
    {
        if (_currentLevel.IsGameOver)
        {
            ChangeLevel();
        }
    }

    public void ChangeLevel()
    {
        if (!_currentLevel.IsNormalized ||
            _currentLevel.AreMovingElements())
        {
            return;
        }

        _currentLevel.ReturnElementsToStartingPosition();
        _currentLevel.gameObject.SetActive(false);
        currentLevelIndex = (currentLevelIndex == levelPrefabs.Count - 1) ? 0 : currentLevelIndex + 1;
        _currentLevel = _levels[currentLevelIndex];
        _currentLevel.gameObject.SetActive(true);
        _currentLevel.ResetAnimations();
    }
}
