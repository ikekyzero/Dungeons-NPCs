using UnityEngine;

public class DirectionalLightController : MonoBehaviour
{
    // Длительность одного полного дня (24 часа) в секундах
    public float dayDuration = 120f;
    // Фиксированный угол поворота по оси Y
    public float yRotation = 0f;

    // Время дня в часах, от 0 до 24
    public float timeOfDay = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Инициализируем время дня как 0 (ночь)
        timeOfDay = 9f;
    }

    // Update is called once per frame
    void Update()
    {
        // Увеличиваем время дня. Каждая секунда добавляет (24/dayDuration) часов
        timeOfDay += (24f / dayDuration) * Time.deltaTime;
        
        // Если время превышает 24, сбрасываем до 0
        if(timeOfDay >= 24f)
        {
            timeOfDay = 0f;
        }

        // Вычисляем угол по оси X: при timeOfDay = 0 -> -90, при 12 часов -> 90, при 24 -> 270 (что эквивалентно -90 после цикла)
        float sunAngleX = (timeOfDay / 24f) * 360f - 90f;
        
        // Обновляем поворот Directional Light
        transform.rotation = Quaternion.Euler(sunAngleX, yRotation, 0f);
    }
}
