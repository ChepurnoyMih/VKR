using System;
using System.Collections.Generic;
using System.Linq;

namespace CooperativeGameSolver
{
    public class CooperativeGame
    {
        private int numPlayers;
        private Dictionary<string, double> characteristicFunction;

        public void SetGame(int numPlayers, Dictionary<string, double> v)
        {
            if (numPlayers < 1)
                throw new ArgumentException("Число игроков должно быть положительным.");
            if (v == null)
                throw new ArgumentNullException(nameof(v));
            if (!v.ContainsKey(string.Join(",", Enumerable.Range(1, numPlayers))))
                throw new InvalidOperationException("Характеристическая функция не содержит полной коалиции.");
            this.numPlayers = numPlayers;
            this.characteristicFunction = v;
        }

        public List<List<int>> GenerateCoalitions(int numPlayers)
        {
            var coalitions = new List<List<int>>();
            for (int i = 1; i < (1 << numPlayers); i++) // 2^n subsets, excluding empty set
            {
                var coalition = new List<int>();
                for (int j = 0; j < numPlayers; j++)
                {
                    if ((i & (1 << j)) != 0)
                    {
                        coalition.Add(j + 1); // Players are 1-based
                    }
                }
                coalitions.Add(coalition);
            }
            return coalitions;
        }

        public double[] CalculateShapleyVector()
        {
            if (numPlayers == 0 || characteristicFunction == null)
                throw new InvalidOperationException("Игра не инициализирована.");

            double[] shapley = new double[numPlayers];
            var permutations = GetPermutations(Enumerable.Range(1, numPlayers).ToList());

            foreach (var permutation in permutations)
            {
                var coalition = new List<int>();
                for (int i = 0; i < permutation.Count; i++)
                {
                    int player = permutation[i];
                    string coalitionKey = string.Join(",", coalition.OrderBy(x => x));
                    string newCoalitionKey = string.Join(",", (coalition.Concat(new[] { player })).OrderBy(x => x));

                    double marginalContribution = characteristicFunction.ContainsKey(newCoalitionKey)
                    ? characteristicFunction[newCoalitionKey] - (characteristicFunction.ContainsKey(coalitionKey) 
                    ? characteristicFunction[coalitionKey] : 0)
                    : 0;

                    shapley[player - 1] += marginalContribution / permutations.Count;
                    coalition.Add(player);
                }
            }

            return shapley;
        }

        public bool CheckCore(double[] allocation)
        {
            if (allocation == null || allocation.Length != numPlayers)
                throw new ArgumentException("Недопустимый вектор распределения.");

            // Efficiency: Sum of allocation equals v(N)
            string grandCoalitionKey = string.Join(",", Enumerable.Range(1, numPlayers));
            if (Math.Abs(allocation.Sum() - characteristicFunction[grandCoalitionKey]) > 1e-6)
                return false;

            // Stability: Sum of allocation for each coalition >= v(S)
            var coalitions = GenerateCoalitions(numPlayers);
            foreach (var coalition in coalitions)
            {
                string coalitionKey = string.Join(",", coalition.OrderBy(x => x));
                double coalitionValue = characteristicFunction.ContainsKey(coalitionKey) ? characteristicFunction[coalitionKey] : 0;
                double allocationSum = coalition.Sum(player => allocation[player - 1]);
                if (allocationSum < coalitionValue - 1e-6)
                    return false;
            }

            return true;
        }

        public bool CheckProfitability()
        {
            string grandCoalitionKey = string.Join(",", Enumerable.Range(1, numPlayers));
            double grandValue = characteristicFunction[grandCoalitionKey];

            var coalitions = GenerateCoalitions(numPlayers);
            foreach (var coalition in coalitions)
            {
                if (coalition.Count == numPlayers) continue; // Skip grand coalition
                string coalitionKey = string.Join(",", coalition.OrderBy(x => x));
                double coalitionValue = characteristicFunction.ContainsKey(coalitionKey) ? characteristicFunction[coalitionKey] : 0;
                if (coalitionValue > grandValue + 1e-6)
                    return false; // A smaller coalition is more profitable
            }

            return true;
        }

        public double[] CalculateNucleolus()
        {
            if (numPlayers == 0 || characteristicFunction == null)
                throw new InvalidOperationException("Игра не инициализирована.");

            double[] x = new double[numPlayers];
            double grandValue = characteristicFunction[string.Join(",", Enumerable.Range(1, numPlayers))];
            for (int i = 0; i < numPlayers; i++)
                x[i] = grandValue / numPlayers; // Начальное равное распределение

            var coalitions = GenerateCoalitions(numPlayers);
            const int maxIterations = 1000; // Увеличено для лучшей сходимости
            double epsilon = 0.001; // Меньший шаг для большей точности
            const double tolerance = 1e-6;

            for (int iter = 0; iter < maxIterations; iter++)
            {
                // Вычисление избытков
                var excesses = new List<(string Coalition, double Excess, List<int> Players)>();
                foreach (var coalition in coalitions)
                {
                    string key = string.Join(",", coalition.OrderBy(c => c));
                    double coalitionValue = characteristicFunction.ContainsKey(key) ? characteristicFunction[key] : 0;
                    double allocationSum = coalition.Sum(player => x[player - 1]);
                    excesses.Add((key, coalitionValue - allocationSum, coalition));
                }

                // Сортировка избытков по убыванию
                var sortedExcesses = excesses.OrderByDescending(ex => ex.Excess).ToList();
                double maxExcess = sortedExcesses[0].Excess;
                if (Math.Abs(maxExcess) < tolerance)
                    break;

                // Корректировка для коалиций с максимальным избытком
                double totalAdjustment = 0;
                var criticalCoalitions = sortedExcesses.Where(ex => ex.Excess > maxExcess - tolerance).ToList();
                foreach (var coalition in criticalCoalitions)
                {
                    double adjustment = maxExcess / coalition.Players.Count;
                    foreach (int player in coalition.Players)
                    {
                        x[player - 1] += adjustment;
                        totalAdjustment += adjustment;
                    }
                }

                // Обеспечение эффективности
                double sum = x.Sum();
                if (Math.Abs(sum - grandValue) > tolerance)
                {
                    double adjustment = (grandValue - sum) / numPlayers;
                    for (int i = 0; i < numPlayers; i++)
                        x[i] += adjustment;
                }

                // Проверка сходимости
                if (criticalCoalitions.Count == coalitions.Count) // Все коалиции имеют одинаковый избыток
                    break;
            }

            return x;
        }

        public List<(string Coalition, double Excess)> CalculateExcesses(double[] x)
        {
            var excesses = new List<(string, double)>();
            foreach (var coalition in GenerateCoalitions(numPlayers))
            {
                string key = string.Join(",", coalition.OrderBy(c => c));
                double value = characteristicFunction.ContainsKey(key) ? characteristicFunction[key] : 0;
                double allocationSum = coalition.Sum(player => x[player - 1]);
                excesses.Add((key, value - allocationSum));
            }
            return excesses;
        }

        private List<List<int>> GetPermutations(List<int> players)
        {
            var result = new List<List<int>>();
            if (players.Count == 1)
            {
                result.Add(new List<int>(players));
                return result;
            }

            foreach (int player in players)
            {
                var remaining = players.Where(p => p != player).ToList();
                var subPermutations = GetPermutations(remaining);
                foreach (var subPerm in subPermutations)
                {
                    var perm = new List<int> { player };
                    perm.AddRange(subPerm);
                    result.Add(perm);
                }
            }

            return result;
        }
    }
}