using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlTypes;
using System.Text.RegularExpressions;

namespace MaximTask7.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StringOperationController : ControllerBase
    {
        [HttpGet]
        public ActionResult<string> ProcessStringAction(string inputString, SortingOption sortingOption)
        {
            var regex = new Regex("^[a-z]+$");
            if (!regex.IsMatch(inputString))
            {
                string errorMessage = "";
                foreach (var item in inputString)
                {
                    if (!regex.IsMatch(item.ToString()))
                    {
                        errorMessage += item;
                    }
                }
                return BadRequest("Некоректные символы - " + errorMessage);

            }
            else
            {
                int length = inputString.Length;
                if (length % 2 == 0)
                {
                    string firstHalf = inputString.Substring(0, length / 2);
                    string secondHalf = inputString.Substring(length / 2);
                    firstHalf = Reverse(firstHalf);
                    secondHalf = Reverse(secondHalf);

                    inputString = firstHalf + secondHalf;
                }
                else
                {
                    inputString = Reverse(inputString) + inputString;
                }

                var lettersCount = new List<string>();
                foreach (var item in inputString.Distinct().ToArray())
                {
                    var count = inputString.Count(symbol => symbol == item);
                    lettersCount.Add("Кол-во символов " + item.ToString() + " - " + count.ToString());
                }

                char[] chars = inputString.ToCharArray();
                if (sortingOption.ToString() == "1")
                {
                    SortClass.QuickSort(chars, 0, chars.Length - 1);
                }
                else if(sortingOption.ToString() == "2")
                {
                    SortClass.TreeSort(chars);
                }
                var responseObject = new
                {
                    ProcessedString = inputString,
                    LettersCount = lettersCount,
                    LongestSubstring = FindLargestSubstring(inputString),
                    SortString = new string(chars),
                    RemoveString = (DeleteSymbol(inputString, int.Parse(GetRandomNumber(inputString.Length - 1).Result)))
                };

                return Ok(responseObject);
            }
        }
        public enum SortingOption
        {
            Option1 = 1,
            Option2 = 2
        }
        static string Reverse(string s)
        {
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        static string FindLargestSubstring(string inputString)
        {
            string vowelLetters = "aeiouy";
            string largestSubstring = "";
            for (int i = 0; i < inputString.Length; i++)
            {
                for (int j = i; j < inputString.Length; j++)
                {
                    if (vowelLetters.Contains(inputString[i]) && vowelLetters.Contains(inputString[j]))
                    {
                        string substring = inputString.Substring(i, j - i + 1);
                        if (substring.Length > largestSubstring.Length)
                        {
                            largestSubstring = substring;
                        }
                    }
                }
            }
            return largestSubstring;
        }

        static async Task<string> GetRandomNumber(int inputStringLenght)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string apiUrl = "https://www.random.org/integers/?num=1&min=0&max=" + inputStringLenght.ToString() + "&col=1&base=10&format=plain&rnd=new";
                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        return await response.Content.ReadAsStringAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            Random random = new Random();
            return random.Next(0, inputStringLenght).ToString();
        }

        static string DeleteSymbol(string inputString, int index)
        {
            var duplicate = inputString;
            return duplicate.Remove(index, 1);
        }
    }

    class SymbolException : Exception
    {
        public SymbolException(string message) : base(message) { }
    }

    class TreeNode
    {
        public char Data;
        public List<int> Indices;
        public TreeNode Left, Right;

        public TreeNode(char item, int index)
        {
            Data = item;
            Indices = new List<int> { index };
            Left = Right = null;
        }
    }

    static class SortClass
    {
        private static int currentIndex = 0;
        public static void QuickSort(char[] array, int low, int high)
        {
            if (low < high)
            {
                int partitionIndex = Partition(array, low, high);

                QuickSort(array, low, partitionIndex - 1);
                QuickSort(array, partitionIndex + 1, high);
            }
        }

        private static int Partition(char[] array, int low, int high)
        {
            char pivotValue = array[high];
            int i = low - 1;

            for (int j = low; j < high; j++)
            {
                if (array[j] < pivotValue)
                {
                    i++;

                    char swap = array[i];
                    array[i] = array[j];
                    array[j] = swap;
                }
            }

            char swap1 = array[i + 1];
            array[i + 1] = array[high];
            array[high] = swap1;

            return i + 1;
        }
        public static void TreeSort(char[] array)
        {
            TreeNode root = null;

            for (int i = 0; i < array.Length; i++)
            {
                root = Insert(root, array[i], i);
            }

            InOrderTraversal(root, array, ref currentIndex);
        }



        private static TreeNode Insert(TreeNode root, char key, int index)
        {
            if (root == null)
            {
                root = new TreeNode(key, index);
                return root;
            }

            if (key == root.Data)
            {
                root.Indices.Add(index);
            }
            else if (key < root.Data)
            {
                root.Left = Insert(root.Left, key, index);
            }
            else
            {
                root.Right = Insert(root.Right, key, index);
            }

            return root;
        }

        private static void InOrderTraversal(TreeNode root, char[] array, ref int index)
        {
            if (root != null)
            {
                InOrderTraversal(root.Left, array, ref index);

                foreach (var idx in root.Indices)
                {
                    array[index++] = root.Data;
                }

                InOrderTraversal(root.Right, array, ref index);
            }
        }
    }
}
