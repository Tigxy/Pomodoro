using System.Linq;

namespace Pomodoro
{
    /// <summary>
    /// A collection of utility functions
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Copies the properties from one class to the other class
        /// </summary>
        /// <typeparam name="T">The type of class to copy the properties of</typeparam>
        /// <param name="source">The source class instance</param>
        /// <param name="target">The target class instance</param>
        public static void CopyProperties<T>(T source, T target)
        {
            // Credits to https://stackoverflow.com/a/33814017
            foreach (var property in typeof(T).GetProperties().Where(p => p.CanWrite))
                property.SetValue(target, property.GetValue(source));
        }
    }
}
