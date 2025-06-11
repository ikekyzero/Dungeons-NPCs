using UnityEngine;

public class DirectionalLightController : MonoBehaviour
{
    // Длительность одного полного дня (24 часа) в секундах
    public float dayDuration = 120f;
    // Фиксированный угол поворота по оси Y
    public float yRotation = 0f;
    // Время дня в часах, от 0 до 24
    public float timeOfDay = 0f;
    // Пороговые значения для ночи (ночь с 18:00 до 6:00)
    public float nightStartHour = 18f;
    public float nightEndHour = 6f;
    // Значения интенсивности окружающей среды для дня и ночи
    public float dayAmbientIntensity = 1f; // Интенсивность днём
    public float nightAmbientIntensity = 0.1f; // Интенсивность ночью

    private bool isNight = false;
    private Light directionalLight; // Ссылка на компонент Light

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Инициализируем время дня как 9:00 (утро)
        timeOfDay = 9f;
        // Получаем компонент Light
        directionalLight = GetComponent<Light>();
        // Устанавливаем начальное состояние света и интенсивности окружения
        UpdateLightAndEnvironment();
    }

    // Update is called once per frame
    void Update()
    {
        // Увеличиваем время дня. Каждая секунда добавляет (24/dayDuration) часов
        timeOfDay += (24f / dayDuration) * Time.deltaTime;
        
        // Если время превышает 24, сбрасываем до 0
        if (timeOfDay >= 24f)
        {
            timeOfDay = 0f;
        }

        // Вычисляем угол по оси X: при timeOfDay = 0 -> -90, при 12 часов -> 90, при 24 -> 270 (что эквивалентно -90 после цикла)
        float sunAngleX = (timeOfDay / 24f) * 360f - 90f;
        
        // Обновляем поворот Directional Light
        transform.rotation = Quaternion.Euler(sunAngleX, yRotation, 0f);

        // Обновляем состояние света и интенсивности окружения
        UpdateLightAndEnvironment();
    }

    // Метод для управления светом и интенсивностью окружения
    private void UpdateLightAndEnvironment()
    {
        bool newIsNight = timeOfDay >= nightStartHour || timeOfDay < nightEndHour;

        // Проверяем, изменилось ли состояние дня/ночи
        if (newIsNight != isNight)
        {
            isNight = newIsNight;

            if (isNight)
            {
                // Отключаем компонент Light ночью
                if (directionalLight != null)
                {
                    directionalLight.enabled = false;
                }
                // Уменьшаем интенсивность окружающей среды ночью
                RenderSettings.ambientIntensity = nightAmbientIntensity;
            }
            else
            {
                // Включаем компонент Light днём
                if (directionalLight != null)
                {
                    directionalLight.enabled = true;
                }
                // Устанавливаем интенсивность окружающей среды днём
                RenderSettings.ambientIntensity = dayAmbientIntensity;
            }

            // Обновляем освещение сцены
            DynamicGI.UpdateEnvironment();
        }
    }
}