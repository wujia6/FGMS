using System.Reflection;
using System.Runtime.Loader;

namespace FGMS.Utils
{
    public class ApplicationFactory
    {
        /// <summary>
        /// 创建对象
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="parms">可变参数</param>
        /// <returns></returns>
        public static T Create<T>(params object[] parms) where T : class
        {
            Type tp = typeof(T);
            var obj = Activator.CreateInstance(typeof(T));
            if (parms.Length == 0) return (T)obj;
            int index = 0;
            foreach (var prop in tp.GetProperties())
            {
                //index = Array.FindIndex(props, p => p.Name == prop.Name);
                //判断属性类型
                if (!prop.PropertyType.IsGenericType && prop.PropertyType.IsValueType || prop.PropertyType.Equals(typeof(string)))
                {
                    prop.SetValue(obj, parms[index]);
                    index++;
                }
            }
            return (T)obj;
        }

        /// <summary>
        /// 创建对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="func">委托(回调方法)</param>
        /// <returns></returns>
        public static T Create<T>(Func<T, T> func) where T : class
        {
            T TClass = (T)Activator.CreateInstance(typeof(T));
            return func(TClass);
        }

        /// <summary>
        /// 获取指定程序集对象
        /// </summary>
        /// <param name="assemblyName">程序集名称</param>
        /// <returns></returns>
        public static Assembly GetAssembly(string assemblyName)
        {
            var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(AppContext.BaseDirectory + $"{assemblyName}.dll");
            return assembly;
        }
    }
}
