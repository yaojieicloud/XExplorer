using System.Text.RegularExpressions;

namespace XExplorer.Core.Utils;

public static class SortUtils
{
    public static List<string> Sort(List<string> input, bool isDesc)
    {
        input.Sort((a, b) =>
        {
            var numsA = ExtractNumbers(a);
            var numsB = ExtractNumbers(b);
            int result = CompareNumberLists(numsA, numsB);
            if (result == 0)
            {
                // 如果数字序列完全相同，再按字符串比较
                result = string.Compare(a, b, StringComparison.OrdinalIgnoreCase);
            }

            return isDesc ? -result : result;
        });

        return input;
    }

    private static List<int> ExtractNumbers(string s)
    {
        var matches = Regex.Matches(s, @"\d+");
        var numbers = new List<int>();
        foreach (Match m in matches)
        {
            numbers.Add(int.Parse(m.Value));
        }
        return numbers;
    }

    private static int CompareNumberLists(List<int> numsA, List<int> numsB)
    {
        int len = Math.Min(numsA.Count, numsB.Count);
        for (int i = 0; i < len; i++)
        {
            int cmp = numsA[i].CompareTo(numsB[i]);
            if (cmp != 0) return cmp;
        }
        // 如果前面都相同，长度短的排前
        return numsA.Count.CompareTo(numsB.Count);
    }
}