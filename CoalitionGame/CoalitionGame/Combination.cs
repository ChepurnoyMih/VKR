using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CoalitionGame
{
    internal class Combination
    {
        private List<string> combinations; // Список комбинаций
        private Dictionary<string, int> winnings; // Выигрыши для комбинаций
        private int numberOfPlayers; // Число игроков
        private bool foundInCore; // Флаг, найдено ли распределение в ядре
        private List<ProfitDistribution> profitDistributions; // Список распределений для таблицы

        public Combination(int numberOfPlayers)
        {
            this.numberOfPlayers = numberOfPlayers;
            combinations = new List<string>();
            winnings = new Dictionary<string, int>();
            profitDistributions = new List<ProfitDistribution>();
            foundInCore = false;
            GenerateCombinations(numberOfPlayers);
        }

        // Структура для хранения распределения прибыли
        public class ProfitDistribution
        {
            public string Permutation { get; set; }
            public int[] Profits { get; set; }
            public bool IsInCore { get; set; }
        }

        // Получение списка распределений
        public List<ProfitDistribution> GetProfitDistributions()
        {
            return profitDistributions;
        }

        // Генерация всех возможных комбинаций игроков
        private void GenerateCombinations(int numberOfPlayers)
        {
            for (int length = 1; length <= numberOfPlayers; length++)
            {
                GenerateCombinationsRecursive("", length, 1, numberOfPlayers);
            }
        }

        // Рекурсивная генерация комбинаций с сортировкой
        private void GenerateCombinationsRecursive(string currentCombination, int length, int start, int numberOfPlayers)
        {
            if (currentCombination.Length == length)
            {
                var sortedCombo = String.Concat(currentCombination.OrderBy(c => c));
                combinations.Add(sortedCombo);
                return;
            }

            for (int i = start; i <= numberOfPlayers; i++)
            {
                GenerateCombinationsRecursive(currentCombination + i, length, i + 1, numberOfPlayers);
            }
        }

        // Вывод всех комбинаций и их выигрышей
        public void DisplayCombinations()
        {
            Console.WriteLine("\nСгенерированные комбинации и их выигрыши:");
            foreach (var combination in combinations)
            {
                if (winnings.ContainsKey(combination))
                {
                    Console.WriteLine($"Комбинация: {combination}, Выигрыш: {winnings[combination]}");
                }
                else
                {
                    Console.WriteLine($"Комбинация: {combination}, Выигрыш: отсутствует");
                }
            }
        }

        // Ввод выигрышей для комбинаций (из файла или консоли)
        public void InputWinningsForCombinations(string filePath = null)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                try
                {
                    filePath = Path.GetFullPath(filePath);
                    if (File.Exists(filePath))
                    {
                        Console.WriteLine($"\nЧтение выигрышей из файла: {filePath}");
                        ReadWinningsFromFile(filePath);
                    }
                    else
                    {
                        Console.WriteLine($"Файл '{filePath}' не найден. Переход к вводу через консоль.");
                        ReadWinningsFromConsole();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при обработке файла '{filePath}': {ex.Message}");
                    Console.WriteLine("Переход к вводу выигрышей через консоль.");
                    ReadWinningsFromConsole();
                }
            }
            else
            {
                Console.WriteLine("\nФайл не указан. Введите выигрыши через консоль:");
                ReadWinningsFromConsole();
            }
        }

        // Чтение выигрышей из файла
        private void ReadWinningsFromFile(string filePath)
        {
            var lines = File.ReadAllLines(filePath, System.Text.Encoding.UTF8);
            foreach (var line in lines)
            {
                var parts = line.Split(':');
                if (parts.Length != 2)
                {
                    Console.WriteLine($"Ошибка в строке '{line}': неверный формат. Пропуск.");
                    continue;
                }

                string combination = parts[0].Trim();
                string winningsInput = parts[1].Trim();

                var sortedCombo = String.Concat(combination.OrderBy(c => c));
                if (!combinations.Contains(sortedCombo))
                {
                    Console.WriteLine($"Ошибка: комбинация '{combination}' не существует. Пропуск.");
                    continue;
                }

                if (int.TryParse(winningsInput, out int win))
                {
                    winnings[sortedCombo] = win;
                }
                else
                {
                    Console.WriteLine($"Ошибка в строке '{line}': выигрыш должен быть целым числом. Установлено значение 0.");
                    winnings[sortedCombo] = 0;
                }
            }

            foreach (var combination in combinations)
            {
                if (!winnings.ContainsKey(combination))
                {
                    Console.WriteLine($"Комбинация {combination} отсутствует в файле. Установлено значение 0.");
                    winnings[combination] = 0;
                }
            }
        }

        // Ввод выигрышей через консоль
        private void ReadWinningsFromConsole()
        {
            foreach (var combination in combinations)
            {
                Console.Write($"Комбинация {combination}: ");
                string winningsInput = Console.ReadLine();
                if (int.TryParse(winningsInput, out int win))
                {
                    winnings[combination] = win;
                }
                else
                {
                    Console.WriteLine("Ошибка: введите целое число. Установлено значение 0.");
                    winnings[combination] = 0;
                }
            }
        }

        // Обмен элементов для генерации перестановок
        private void Swap(char[] array, int left, int right)
        {
            char temp = array[left];
            array[left] = array[right];
            array[right] = temp;
        }

        // Рекурсивная генерация перестановок
        private void GetPermutations(char[] array, int left, int right, List<string> results)
        {
            if (left == right)
            {
                results.Add(new string(array));
            }
            else
            {
                for (int i = left; i <= right; i++)
                {
                    Swap(array, left, i);
                    GetPermutations(array, left + 1, right, results);
                    Swap(array, left, i); // Откат изменений
                }
            }
        }

        // Генерация перестановок для списка комбинаций
        private List<string> GeneratePermutations(List<string> inputCombinations)
        {
            var permutations = new List<string>();
            foreach (var combination in inputCombinations)
            {
                GetPermutations(combination.ToCharArray(), 0, combination.Length - 1, permutations);
            }
            return permutations;
        }

        // Распределение прибыли для игроков
        public void CalculateProfitDistribution()
        {
            Console.WriteLine("\nРаспределение прибыли для игроков:");
            foundInCore = false; // Сбрасываем флаг
            profitDistributions.Clear(); // Очищаем список распределений
            var filteredCombinations = combinations.Where(c => c.Length == numberOfPlayers).ToList();

            foreach (var combination in filteredCombinations)
            {
                Console.WriteLine($"\nРасчет для комбинации {combination}:");
                int totalWin = winnings.ContainsKey(combination) ? winnings[combination] : 0;
                var permutations = GeneratePermutations(new List<string> { combination });

                foreach (var perm in permutations)
                {
                    CalculateForPermutation(perm, totalWin);
                }
            }

            // Если ни одно распределение не в ядре, ищем распределение, исключая игроков
            if (!foundInCore)
            {
                Console.WriteLine("\n[ВНИМАНИЕ] Ни одно распределение не находится в ядре. Поиск распределений, исключая одного игрока:\n");
                FindDistributionExcludingPlayer();
            }
        }

        // Расчет прибыли для одной перестановки
        private void CalculateForPermutation(string permutation, int totalWin)
        {
            Console.WriteLine($"  Перестановка: {permutation}");

            int[] playerProfits = new int[permutation.Length];
            int previousWin = 0; // Выигрыш предыдущего префикса

            for (int i = 0; i < permutation.Length; i++)
            {
                string currentPrefix = permutation.Substring(0, i + 1);
                int currentWin = GetWin(currentPrefix); // Получение выигрыша для префикса

                int deltaWin = currentWin - previousWin;
                previousWin = currentWin;

                playerProfits[i] = deltaWin;

                Console.WriteLine($"    Игрок {permutation[i]} получает: {deltaWin}");
            }

            // Проверка распределения на ядро и нуклеолус
            CheckCoreAndNucleolus(permutation, playerProfits, totalWin);

            // Сохранение распределения
            var distribution = new ProfitDistribution
            {
                Permutation = permutation,
                Profits = playerProfits,
                IsInCore = foundInCore
            };
            profitDistributions.Add(distribution);
        }

        // Получение выигрыша для комбинации
        private int GetWin(string combo)
        {
            var sortedCombo = String.Concat(combo.OrderBy(c => c));
            return winnings.ContainsKey(sortedCombo) ? winnings[sortedCombo] : 0;
        }

        // Проверка распределения на ядро и нуклеолус
        private void CheckCoreAndNucleolus(string permutation, int[] playerProfits, int totalWin)
        {
            Console.WriteLine($"    Проверка распределения для перестановки {permutation}:");

            // Маппинг игроков на их прибыли
            var distribution = new Dictionary<char, int>();
            for (int i = 0; i < permutation.Length; i++)
            {
                distribution[permutation[i]] = playerProfits[i];
            }

            // Проверка ядра
            bool isInCore = CheckCore(distribution, totalWin);
            Console.WriteLine($"      В ядре: {(isInCore ? "Да" : "Нет")}");
            if (isInCore)
            {
                foundInCore = true; // Устанавливаем флаг, если найдено распределение в ядре
            }

            // Проверка нуклеолуса (упрощенная)
            bool isNucleolus = CheckNucleolus(distribution, totalWin);
            Console.WriteLine($"      Является нуклеолусом: {(isNucleolus ? "Да" : "Нет (упрощенная проверка)")}");
        }

        // Проверка, находится ли распределение в ядре
        private bool CheckCore(Dictionary<char, int> distribution, int totalWin)
        {
            // Проверка эффективности
            int sum = distribution.Values.Sum();
            string grandCoalition = String.Concat(Enumerable.Range(1, numberOfPlayers).Select(i => i.ToString()).OrderBy(c => c));
            if (sum != totalWin)
            {
                return false;
            }

            // Проверка стабильности
            foreach (var coalition in combinations)
            {
                int coalitionSum = coalition.Sum(player => distribution.ContainsKey(player) ? distribution[player] : 0);
                int coalitionValue = GetWin(coalition);
                if (coalitionSum < coalitionValue)
                {
                    return false;
                }
            }

            return true;
        }

        // Упрощенная проверка нуклеолуса
        private bool CheckNucleolus(Dictionary<char, int> distribution, int totalWin)
        {
            var excesses = new List<double>();
            foreach (var coalition in combinations)
            {
                int coalitionSum = coalition.Sum(player => distribution.ContainsKey(player) ? distribution[player] : 0);
                int coalitionValue = GetWin(coalition);
                double excess = coalitionValue - coalitionSum;
                excesses.Add(excess);
            }

            excesses.Sort((a, b) => b.CompareTo(a));
            return excesses[0] <= 0; // Упрощенная проверка
        }

        // Поиск распределения, исключая одного игрока
        private void FindDistributionExcludingPlayer()
        {
            for (int excludedPlayer = 1; excludedPlayer <= numberOfPlayers; excludedPlayer++)
            {
                Console.WriteLine($"\nИсключение игрока {excludedPlayer}:");

                // Фильтруем комбинации, исключая коалиции с игроком excludedPlayer
                string excludedPlayerStr = excludedPlayer.ToString();
                var subgameCombinations = combinations
                .Where(c => !c.Contains(excludedPlayerStr))
                .ToList();

                if (subgameCombinations.Count == 0)
                {
                    Console.WriteLine("  Нет коалиций без этого игрока.");
                    continue;
                }

                // Находим полную коалицию без исключенного игрока
                var subgameGrandCoalition = subgameCombinations
 .Where(c => c.Length == numberOfPlayers - 1)
 .FirstOrDefault();

                if (subgameGrandCoalition == null)
                {
                    Console.WriteLine("  Полная коалиция без игрока отсутствует.");
                    continue;
                }

                int subgameTotalWin = GetWin(subgameGrandCoalition);
                Console.WriteLine($"  Полная коалиция: {subgameGrandCoalition}, Выигрыш: {subgameTotalWin}");

                // Генерируем перестановки для полной коалиции
                var permutations = GeneratePermutations(new List<string> { subgameGrandCoalition });

                bool subgameFoundInCore = false;
                foreach (var perm in permutations)
                {
                    Console.WriteLine($"    Перестановка: {perm}");

                    // Распределяем прибыль для подигры
                    int[] playerProfits = new int[perm.Length];
                    int previousWin = 0;

                    for (int i = 0; i < perm.Length; i++)
                    {
                        string currentPrefix = perm.Substring(0, i + 1);
                        int currentWin = GetWin(currentPrefix);
                        int deltaWin = currentWin - previousWin;
                        previousWin = currentWin;
                        playerProfits[i] = deltaWin;
                        Console.WriteLine($"      Игрок {perm[i]} получает: {deltaWin}");
                    }

                    // Проверка ядра для подигры
                    var distribution = new Dictionary<char, int>();
                    for (int i = 0; i < perm.Length; i++)
                    {
                        distribution[perm[i]] = playerProfits[i];
                    }

                    bool isInCore = CheckSubgameCore(distribution, subgameCombinations, subgameTotalWin);
                    Console.WriteLine($"      В ядре подигры: {(isInCore ? "Да" : "Нет")}");
                    if (isInCore)
                    {
                        subgameFoundInCore = true;
                    }

                    // Сохранение распределения для подигры
                    // Создаем полное распределение, где исключенный игрок получает 0
                    int[] fullProfits = new int[numberOfPlayers];
                    for (int i = 0; i < perm.Length; i++)
                    {
                        int playerId = int.Parse(perm[i].ToString()) - 1;
                        fullProfits[playerId] = playerProfits[i];
                    }
                    profitDistributions.Add(new ProfitDistribution
                    {
                        Permutation = perm + $" (-{excludedPlayer})",
                        Profits = fullProfits,
                        IsInCore = isInCore
                    });
                }

                if (!subgameFoundInCore)
                {
                    Console.WriteLine("  Подходящее распределение не найдено.");
                }
            }
        }

        // Проверка ядра для подигры
        private bool CheckSubgameCore(Dictionary<char, int> distribution, List<string> subgameCombinations, int totalWin)
        {
            // Проверка эффективности
            int sum = distribution.Values.Sum();
            if (sum != totalWin)
            {
                return false;
            }

            // Проверка стабильности
            foreach (var coalition in subgameCombinations)
            {
                int coalitionSum = coalition.Sum(player => distribution.ContainsKey(player) ? distribution[player] : 0);
                int coalitionValue = GetWin(coalition);
                if (coalitionSum < coalitionValue)
                {
                    return false;
                }
            }

            return true;
        }
    }
}