using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

namespace Task3_EventWaitHandle_DataAnalysis
{
    class Program
    {
        // Общий массив для хранения сгенерированных чисел
        static int[] numbers;
        
        // Событие для сигнализации о завершении генерации чисел
        static EventWaitHandle generationCompletedEvent = new EventWaitHandle(false, EventResetMode.ManualReset);
        
        // События для сигнализации о завершении анализа
        static EventWaitHandle maxCompletedEvent = new EventWaitHandle(false, EventResetMode.ManualReset);
        static EventWaitHandle minCompletedEvent = new EventWaitHandle(false, EventResetMode.ManualReset);
        static EventWaitHandle avgCompletedEvent = new EventWaitHandle(false, EventResetMode.ManualReset);
        
        // Результаты анализа
        static int maxValue = 0;
        static int minValue = 0;
        static double averageValue = 0;
        
        // Объект для синхронизации доступа к консоли
        static object consoleLock = new object();
        
        // Генератор случайных чисел (thread-safe)
        static Random random = new Random();

        static void Main(string[] args)
        {
            Console.WriteLine("=== Задание 3: Генерация и анализ данных с использованием событий ===\n");
            Console.WriteLine("Программа генерирует 1000 чисел в диапазоне от 0 до 5000.");
            Console.WriteLine("После генерации запускаются 3 потока для анализа данных:");
            Console.WriteLine("  - Поток 1: поиск максимального значения");
            Console.WriteLine("  - Поток 2: поиск минимального значения");
            Console.WriteLine("  - Поток 3: вычисление среднего арифметического\n");
            Console.WriteLine(new string('=', 80));

            // Создаем и запускаем потоки
            Thread generatorThread = new Thread(GenerateNumbers);
            Thread maxThread = new Thread(FindMax);
            Thread minThread = new Thread(FindMin);
            Thread avgThread = new Thread(CalculateAverage);

            generatorThread.Start();
            maxThread.Start();
            minThread.Start();
            avgThread.Start();

            // Ожидаем завершения всех потоков
            generatorThread.Join();
            maxThread.Join();
            minThread.Join();
            avgThread.Join();

            // Выводим результаты
            PrintResults();

            Console.WriteLine("\n" + new string('=', 80));
            Console.WriteLine("\nПрограмма завершена. Нажмите любую клавишу для выхода...");
            Console.ReadKey();
        }

        /// <summary>
        /// Поток 1: Генерация 1000 случайных чисел
        /// </summary>
        static void GenerateNumbers()
        {
            lock (consoleLock)
            {
                Console.WriteLine("[Генератор] Запущен. Начинает генерацию 1000 чисел...");
            }

            numbers = new int[1000];
            
            for (int i = 0; i < numbers.Length; i++)
            {
                numbers[i] = random.Next(0, 5001); // от 0 до 5000 включительно
                
                // Выводим прогресс каждые 100 чисел
                if ((i + 1) % 100 == 0)
                {
                    lock (consoleLock)
                    {
                        Console.WriteLine($"[Генератор] Сгенерировано {i + 1} чисел...");
                    }
                }
                
                // Небольшая задержка для наглядности (в реальном приложении можно убрать)
                Thread.Sleep(1);
            }

            lock (consoleLock)
            {
                Console.WriteLine($"[Генератор] Генерация завершена. Сгенерировано {numbers.Length} чисел.");
            }

            // Сигнализируем о завершении генерации
            generationCompletedEvent.Set();
        }

        /// <summary>
        /// Поток 2: Поиск максимального значения
        /// </summary>
        static void FindMax()
        {
            lock (consoleLock)
            {
                Console.WriteLine("[Анализатор 1] Запущен. Ожидает завершения генерации...");
            }

            // Ожидаем завершения генерации
            generationCompletedEvent.WaitOne();

            lock (consoleLock)
            {
                Console.WriteLine("[Анализатор 1] Получил сигнал. Начинает поиск максимума...");
            }

            // Выполняем поиск максимума
            maxValue = numbers[0];
            for (int i = 1; i < numbers.Length; i++)
            {
                if (numbers[i] > maxValue)
                {
                    maxValue = numbers[i];
                }
                
                // Показываем прогресс каждые 200 элементов
                if ((i + 1) % 200 == 0)
                {
                    lock (consoleLock)
                    {
                        Console.WriteLine($"[Анализатор 1] Проверено {i + 1} элементов...");
                    }
                }
            }

            lock (consoleLock)
            {
                Console.WriteLine($"[Анализатор 1] Поиск завершен. Максимальное значение: {maxValue}");
            }

            // Сигнализируем о завершении поиска максимума
            maxCompletedEvent.Set();
        }

        /// <summary>
        /// Поток 3: Поиск минимального значения
        /// </summary>
        static void FindMin()
        {
            lock (consoleLock)
            {
                Console.WriteLine("[Анализатор 2] Запущен. Ожидает завершения генерации...");
            }

            // Ожидаем завершения генерации
            generationCompletedEvent.WaitOne();

            lock (consoleLock)
            {
                Console.WriteLine("[Анализатор 2] Получил сигнал. Начинает поиск минимума...");
            }

            // Выполняем поиск минимума
            minValue = numbers[0];
            for (int i = 1; i < numbers.Length; i++)
            {
                if (numbers[i] < minValue)
                {
                    minValue = numbers[i];
                }
                
                // Показываем прогресс каждые 200 элементов
                if ((i + 1) % 200 == 0)
                {
                    lock (consoleLock)
                    {
                        Console.WriteLine($"[Анализатор 2] Проверено {i + 1} элементов...");
                    }
                }
            }

            lock (consoleLock)
            {
                Console.WriteLine($"[Анализатор 2] Поиск завершен. Минимальное значение: {minValue}");
            }

            // Сигнализируем о завершении поиска минимума
            minCompletedEvent.Set();
        }

        /// <summary>
        /// Поток 4: Вычисление среднего арифметического
        /// </summary>
        static void CalculateAverage()
        {
            lock (consoleLock)
            {
                Console.WriteLine("[Анализатор 3] Запущен. Ожидает завершения генерации...");
            }

            // Ожидаем завершения генерации
            generationCompletedEvent.WaitOne();

            lock (consoleLock)
            {
                Console.WriteLine("[Анализатор 3] Получил сигнал. Начинает вычисление среднего...");
            }

            // Вычисляем сумму
            long sum = 0;
            for (int i = 0; i < numbers.Length; i++)
            {
                sum += numbers[i];
                
                // Показываем прогресс каждые 200 элементов
                if ((i + 1) % 200 == 0)
                {
                    lock (consoleLock)
                    {
                        Console.WriteLine($"[Анализатор 3] Обработано {i + 1} элементов...");
                    }
                }
            }

            averageValue = (double)sum / numbers.Length;

            lock (consoleLock)
            {
                Console.WriteLine($"[Анализатор 3] Вычисление завершено. Среднее арифметическое: {averageValue:F2}");
            }

            // Сигнализируем о завершении вычисления среднего
            avgCompletedEvent.Set();
        }

        /// <summary>
        /// Вывод результатов анализа
        /// </summary>
        static void PrintResults()
        {
            // Ожидаем завершения всех аналитических потоков
            WaitHandle.WaitAll(new WaitHandle[] { maxCompletedEvent, minCompletedEvent, avgCompletedEvent });

            Console.WriteLine("\n" + new string('=', 80));
            Console.WriteLine("=== РЕЗУЛЬТАТЫ АНАЛИЗА ===");
            Console.WriteLine($"Всего сгенерировано чисел: {numbers.Length}");
            Console.WriteLine($"Диапазон генерации: от 0 до 5000");
            Console.WriteLine($"Максимальное значение: {maxValue}");
            Console.WriteLine($"Минимальное значение: {minValue}");
            Console.WriteLine($"Среднее арифметическое: {averageValue:F2}");
            Console.WriteLine($"Размах (макс - мин): {maxValue - minValue}");
            Console.WriteLine("=" + new string('=', 79));
        }
    }
}
