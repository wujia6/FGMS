namespace FGMS.Utils
{
    public class GenerateRandomNumber
    {
        /// <summary>
        /// 产生工单号（日期-随机数）
        /// </summary>
        /// <returns></returns>
        public string CreateOrderNum()
        {
            string datePart = DateTime.Now.ToString("yyyyMMdd");
            Guid guid = Guid.NewGuid();
            string randomPart = guid.ToString()[..4].ToUpper();
            return $"{datePart}{randomPart}";
        }

        /// <summary>
        /// 产生4位随机数（字母+数字）
        /// </summary>
        /// <returns></returns>
        public string Create()
        {
            var random = new Random();
            var digits = new HashSet<int>();
            while (digits.Count < 2)
            {
                digits.Add(random.Next(0, 10)); // 产生 0 到 9 之间的随机数字  
            }

            var letters = new HashSet<char>();
            while (letters.Count < 2)
            {
                letters.Add((char)random.Next('A', 'Z' + 1)); // 产生 A 到 Z 之间的随机字母  
            }

            var combined = new List<string>();
            combined.AddRange(digits.Select(d => d.ToString())); // 将数字转化为字符串  
            combined.AddRange(letters.Select(l => l.ToString())); // 将字母转化为字符串  

            RandomizeList(combined, random);
            return string.Join("", combined);
        }

        private static void RandomizeList<T>(List<T> list, Random random)
        {
            int n = list.Count;
            while (n > 1)
            {
                int k = random.Next(n--);
                T temp = list[n];
                list[n] = list[k];
                list[k] = temp;
            }
        }
    }
}
