using UnityEngine;
using System.Collections.Generic;

public class StoneSpawn : MonoBehaviour
{
    public GameObject stonePrefab;
    public Transform player;
    public int maxStones = 20;
    public float minDistance = 8f;      // Минимальное расстояние до игрока (камни не спавнятся ближе)
    public float spawnDistance = 15f;   // Среднее расстояние для спавна
    public float maxDistance = 22f;     // Если игрок ушел дальше, камень удаляется
    public float stoneDropSpeed = 8f;   // Скорость опускания камня вниз
    public float minStoneToStoneDistance = 4f; // Минимальное расстояние между камнями

    private List<GameObject> stones = new List<GameObject>();

    // Для хранения исходной высоты каждого камня
    private Dictionary<GameObject, float> stoneOriginalY = new Dictionary<GameObject, float>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null || stonePrefab == null) return;

        // Спавн новых камней, если их меньше maxStones
        int maxAttempts = 30;
        while (stones.Count < maxStones)
        {
            bool found = false;
            Vector3 spawnPos = Vector3.zero;
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                spawnPos = GetRandomPositionAroundPlayer();
                bool tooClose = false;
                foreach (var s in stones) // <-- изменено имя переменной
                {
                    if (s == null) continue;
                    if (Vector3.Distance(s.transform.position, spawnPos) < minStoneToStoneDistance)
                    {
                        tooClose = true;
                        break;
                    }
                }
                if (!tooClose)
                {
                    found = true;
                    break;
                }
            }
            if (!found)
                break; // Не удалось найти подходящее место

            GameObject stone = Instantiate(stonePrefab, spawnPos, Quaternion.identity);
            stones.Add(stone);
            stoneOriginalY[stone] = spawnPos.y; // Сохраняем исходную высоту
        }

        // Обработка существующих камней
        for (int i = stones.Count - 1; i >= 0; i--)
        {
            GameObject stone = stones[i];
            if (stone == null)
            {
                stones.RemoveAt(i);
                continue;
            }

            float dist = Vector3.Distance(player.position, stone.transform.position);

            // Если игрок слишком далеко — удалить камень
            if (dist > maxDistance)
            {
                stoneOriginalY.Remove(stone);
                Destroy(stone);
                stones.RemoveAt(i);
                continue;
            }

            // Если игрок слишком близко — опускать камень вниз
            // Если игрок отходит — поднимать камень обратно на исходную высоту
            if (stoneOriginalY.ContainsKey(stone))
            {
                float originalY = stoneOriginalY[stone];
                Vector3 pos = stone.transform.position;
                if (dist < minDistance)
                {
                    pos.y = Mathf.MoveTowards(pos.y, originalY - 10f, stoneDropSpeed * Time.deltaTime);
                }
                else
                {
                    pos.y = Mathf.MoveTowards(pos.y, originalY, stoneDropSpeed * Time.deltaTime);
                }
                stone.transform.position = pos;
            }
        }
    }

    Vector3 GetRandomPositionAroundPlayer()
    {
        // Случайный угол и расстояние в диапазоне [spawnDistance, maxDistance]
        float angle = Random.Range(0f, Mathf.PI * 2f);
        float distance = Random.Range(spawnDistance, maxDistance - 1f);
        Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * distance;
        Vector3 pos = player.position + offset;
        pos.y = player.position.y; // На уровне игрока
        return pos;
    }
}

// Нет изменений в этом файле, но вот как реализовать невидимость персонажа от первого лица:

// Чтобы от первого лица не было видно персонажа:
// 1. Откройте вашу модель персонажа в иерархии.
// 2. Найдите все MeshRenderer/SkinnedMeshRenderer, которые отвечают за визуализацию тела/головы/рук.
// 3. Для этих рендереров:
//    - Либо отключите их (renderer.enabled = false) в Start, если камера — дочерний объект персонажа.
//    - Либо используйте отдельный слой (например, "PlayerBody") и настройте камеру так, чтобы она не рендерила этот слой:
//      Camera > Culling Mask > уберите галочку с вашего слоя тела.

// Пример кода для отключения рендереров в Start (добавьте в ваш скрипт персонажа):

/*
void Start()
{
    foreach (var renderer in GetComponentsInChildren<Renderer>())
    {
        renderer.enabled = false;
    }
}
*/

// Или используйте слои и Culling Mask для более гибкой настройки.
